# 06 — Member Management

## 📖 Feature Overview

The Members module is the core of The League Platform. Every person who interacts with a sports club — players, parents, volunteers, social members — is represented as a `Member` entity. The module handles registration, profile management, status lifecycle, family accounts, custom fields, and bulk import.

### Key Capabilities
- Rich member profiles with value objects (Address, EmergencyContact, MedicalInfo)
- Status state machine with enforced transitions
- Auto-generated member numbers (MBR-001 format)
- Family accounts linking up to 10 dependents to a primary member
- Club-defined custom fields with type validation
- CSV/Excel bulk import with row-level error reporting
- QR code generation for member identification

---

## 🏗️ Architecture & Design Decisions

| Decision | Rationale |
|----------|-----------|
| Value objects for Address, EmergencyContact, MedicalInfo | Encapsulate validation; stored as owned entities in EF Core |
| State machine with `EnsureValidTransition` | Prevents invalid status changes at the domain level |
| `MemberNumber` generated server-side | Sequential within a club, formatted as MBR-001 |
| Family accounts via `FamilyMember` join entity | Flexible relationships (Spouse, Child, Parent, Sibling, Other) |
| Custom fields as JSON string | Schema-free per-club configuration without schema migrations |
| `CustomFieldDefinition` per club | Clubs define their own fields (type, required, options) |
| Soft-delete via `IsActive` flag | Members are never physically deleted (audit trail) |

---

## 📊 Data Model

### Member Entity
```csharp
public class Member : AuditableEntity  // inherits Id, ClubId, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy
{
    public Guid? UserId { get; private set; }           // Link to Identity (nullable for non-login members)
    public string MemberNumber { get; private set; }     // "MBR-001" format
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string Email { get; private set; }
    public string? Phone { get; private set; }
    public DateTime? DateOfBirth { get; private set; }
    public Gender? Gender { get; private set; }          // Male, Female, Other, PreferNotToSay
    public Address? Address { get; private set; }        // Value object (owned entity)
    public EmergencyContact? PrimaryEmergencyContact { get; private set; }
    public EmergencyContact? SecondaryEmergencyContact { get; private set; }
    public MedicalInfo? MedicalInfo { get; private set; }
    public string? CustomFieldValues { get; private set; } // JSON
    public MemberStatus Status { get; private set; }     // Pending, Active, Expired, Suspended, Cancelled
    public bool IsFamilyAccount { get; private set; }
    public Guid? PrimaryMemberId { get; private set; }   // If this is a dependent
    public DateTime JoinedDate { get; private set; }
    public bool IsActive { get; private set; }
}
```

### Value Objects (Owned Entities)

```csharp
// Address — stored as columns in the Members table (owned entity)
public class Address
{
    public string? Line1 { get; set; }
    public string? Line2 { get; set; }
    public string? City { get; set; }
    public string? County { get; set; }
    public string? PostCode { get; set; }
    public string? Country { get; set; }
}

// EmergencyContact — two per member (primary + secondary)
public class EmergencyContact
{
    public string Name { get; set; }
    public string Phone { get; set; }
    public string? Relationship { get; set; }
}

// MedicalInfo — sensitive data, access-controlled
public class MedicalInfo
{
    public string? Conditions { get; set; }
    public string? Allergies { get; set; }
    public string? Medications { get; set; }
    public string? DoctorName { get; set; }
    public string? DoctorPhone { get; set; }
}
```

---

## 🔧 Status State Machine

The member lifecycle follows strict transition rules enforced at the domain level:

```
                    ┌──────────────┐
                    │   Pending    │
                    └──────┬───────┘
                           │ Activate()
                           ▼
                    ┌──────────────┐
              ┌─────│    Active    │─────┐
              │     └──────┬───────┘     │
              │            │             │
    Suspend() │  Expire()  │  Cancel()   │
              │            │             │
              ▼            ▼             ▼
       ┌───────────┐ ┌──────────┐ ┌───────────┐
       │ Suspended │ │ Expired  │ │ Cancelled │ (terminal)
       └─────┬─────┘ └────┬─────┘ └───────────┘
             │             │
             └──────┬──────┘
                    │ Reactivate()
                    ▼
             ┌──────────────┐
             │    Active    │
             └──────────────┘
```

**Allowed transitions:**
| From | To |
|------|-----|
| Pending | Active |
| Active | Expired, Suspended, Cancelled |
| Suspended | Active (reactivate) |
| Expired | Active (reactivate) |
| Cancelled | — (terminal state, no transitions) |

**Domain enforcement:**
```csharp
private void EnsureValidTransition(MemberStatus newStatus)
{
    var allowed = GetAllowedTransitions(Status);
    if (!allowed.Contains(newStatus))
        throw new InvalidOperationException(
            $"Cannot transition from {Status} to {newStatus}.");
}
```

Each transition is also recorded in `MemberStatusTransition` for audit:
```csharp
public class MemberStatusTransition : BaseEntity
{
    public Guid MemberId { get; private set; }
    public Guid ClubId { get; private set; }
    public MemberStatus PreviousStatus { get; private set; }
    public MemberStatus NewStatus { get; private set; }
    public DateTime ChangedAt { get; private set; }
    public string? ChangedByUserId { get; private set; }
}
```

---

## ⚙️ Member Number Generation

Member numbers are sequential per club, formatted as `MBR-{number:D3}`:

```csharp
public class MemberNumberGenerator : IMemberNumberGenerator
{
    public async Task<string> GenerateAsync(Guid clubId, MembersDbContext db)
    {
        var maxNumber = await db.Members
            .Where(m => m.ClubId == clubId)
            .MaxAsync(m => (int?)ExtractNumber(m.MemberNumber)) ?? 0;

        return $"MBR-{(maxNumber + 1):D3}"; // MBR-001, MBR-002, ...
    }
}
```

- Format: `MBR-001`, `MBR-002`, ..., `MBR-999`, `MBR-1000`
- Scoped per club (each club starts from MBR-001)
- Generated on creation, immutable after assignment

---

## 👨‍👩‍👧‍👦 Family Accounts

A primary member can link up to 10 dependents:

```csharp
public class FamilyMember : TenantEntity
{
    public Guid PrimaryMemberId { get; private set; }
    public Guid DependentMemberId { get; private set; }
    public FamilyMemberRelation Relationship { get; private set; } // Spouse, Child, Parent, Sibling, Other
}
```

**Rules:**
- Maximum 10 dependents per primary member
- A dependent cannot be a primary member of another family
- Dependents inherit the primary member's billing (optional)
- Each dependent is still a full `Member` entity with their own profile

---

## 📝 Custom Fields

Clubs can define their own fields without schema changes:

```csharp
public class CustomFieldDefinition : TenantEntity
{
    public string Name { get; private set; }        // "Playing Position"
    public string FieldType { get; private set; }   // Text, Number, Date, Boolean, Select, MultiSelect, TextArea
    public bool IsRequired { get; private set; }
    public string? Options { get; private set; }    // JSON: ["Batsman","Bowler","All-rounder"] for Select types
    public int DisplayOrder { get; private set; }
}
```

**Storage:** Custom field values are stored as JSON on the Member entity:
```json
{
  "Playing Position": "All-rounder",
  "Shirt Size": "L",
  "Years Playing": 12,
  "Available Saturdays": true
}
```

**Validation:** On save, the handler validates each value against its `CustomFieldDefinition`:
- `Text` → string, max 500 chars
- `Number` → numeric value
- `Date` → valid ISO date
- `Boolean` → true/false
- `Select` → value must be in Options array
- `MultiSelect` → all values must be in Options array

---

## 📥 CSV/Excel Import

Bulk import with row-level validation and error reporting:

```csharp
// POST /api/v1/members/import
// Content-Type: multipart/form-data
// Body: file (CSV or XLSX)

// Response:
{
  "totalRows": 150,
  "successCount": 142,
  "errorCount": 8,
  "errors": [
    { "row": 12, "field": "Email", "message": "Invalid email format" },
    { "row": 45, "field": "DateOfBirth", "message": "Date cannot be in the future" },
    { "row": 89, "field": "Email", "message": "Duplicate email: john@club.com" }
  ]
}
```

**Import rules:**
- Required columns: FirstName, LastName, Email
- Optional columns: Phone, DateOfBirth, Gender, Address fields
- Duplicate emails (within file or existing members) are rejected per-row
- Valid rows are imported even if other rows fail
- Member numbers are auto-generated for all imported members

---

## 🌐 API Endpoints

| Method | Route | Permission | Purpose |
|--------|-------|-----------|---------|
| GET | /api/v1/members | ViewMembers | List members (paginated, filterable) |
| GET | /api/v1/members/{id} | ViewMembers | Get member by ID |
| POST | /api/v1/members | ManageMembers | Create member |
| PUT | /api/v1/members/{id} | ManageMembers | Update member |
| PUT | /api/v1/members/{id}/status | ManageMembers | Change status |
| POST | /api/v1/members/import | ManageMembers | Bulk import |
| GET | /api/v1/members/{id}/family | ViewMembers | Get family members |
| POST | /api/v1/members/{id}/family | ManageMembers | Add family member |
| GET | /api/v1/members/custom-fields | ViewMembers | Get custom field definitions |
| POST | /api/v1/members/custom-fields | ManageMembers | Create custom field |

---

## 🧪 Testing Approach

### Property Tests
```
Property 8: Status Transition Validity
  For ANY member in ANY status,
  and FOR ANY attempted transition,
  the transition SHALL succeed ONLY if it matches the allowed transitions map.

Property 9: Member Number Uniqueness
  For ANY club with N members,
  ALL member numbers SHALL be unique within that club.
```

### Unit Tests
- Create member → status is Pending, MemberNumber assigned
- Activate pending member → status is Active
- Cancel active member → status is Cancelled
- Cancel cancelled member → throws InvalidOperationException
- Family account with 11 dependents → rejected
- Custom field validation for each type

---

## 🚀 How to Extend

### Adding a new member status:
1. Add value to `MemberStatus` enum
2. Update `GetAllowedTransitions()` in `Member.cs`
3. Add transition method (e.g., `Freeze()`)
4. Update status change command handler
5. Add integration test for the new transition

### Adding a new value object:
1. Create the value object class in `Shared.Domain/ValueObjects/`
2. Add property to `Member` entity
3. Configure as owned entity in `MembersDbContext.OnModelCreating`
4. Update the `Update()` method to accept the new value object
