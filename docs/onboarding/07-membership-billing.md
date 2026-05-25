# 07 — Membership & Billing

## 📖 Feature Overview

The Memberships module handles membership type configuration, enrollment, billing cycles, renewals, freezes, discounts, and capacity-based waitlists. It's the commercial engine of the platform — connecting members to their club's subscription offerings.

### Key Capabilities
- Configurable membership types with pricing, age limits, and capacity
- 9 billing cycles (Weekly through Lifetime)
- Enrollment with automatic age validation
- Auto-renewal with payment trigger
- Membership freeze (1-365 days) with optional fee
- 6 discount types with promo code support
- Capacity-based waitlist with position ordering

---

## 🏗️ Architecture & Design Decisions

| Decision | Rationale |
|----------|-----------|
| `MembershipType` as configuration entity | Clubs define their own plans; reusable across members |
| `Membership` as enrollment record | One per member-per-type; tracks dates, status, price paid |
| Billing cycle as enum (not custom intervals) | Covers 99% of use cases; simpler than arbitrary day counts |
| Age validation at enrollment time | Prevents invalid signups; checked against DateOfBirth |
| Freeze as separate entity | Tracks history; membership status stays Active during freeze |
| Discount calculation in domain | Business logic in entity, not in service layer |
| Waitlist with position ordering | Fair queue; position assigned on join, promoted on vacancy |

---

## 📊 Data Model

### MembershipType (Configuration)
```csharp
public class MembershipType : TenantEntity
{
    public string Name { get; private set; }              // "Adult Annual", "Junior Monthly"
    public string? Description { get; private set; }
    public decimal Price { get; private set; }            // Base price per billing cycle
    public BillingCycle BillingCycle { get; private set; } // Weekly → Lifetime
    public int? MinAge { get; private set; }              // null = no minimum
    public int? MaxAge { get; private set; }              // null = no maximum
    public int? Capacity { get; private set; }            // null = unlimited
    public decimal? JoiningFee { get; private set; }      // One-time fee on first enrollment
    public bool IsActive { get; private set; }            // Can new members enroll?
    public bool AllowAutoRenewal { get; private set; }    // Can members opt into auto-renew?
    public decimal? FreezeFee { get; private set; }       // Fee charged per freeze period
}
```

### BillingCycle Enum (9 options)
```csharp
public enum BillingCycle
{
    Weekly,        // 7 days
    Fortnightly,   // 14 days
    Monthly,       // Calendar month
    Quarterly,     // 3 months
    Biannual,      // 6 months
    Annual,        // 12 months
    Lifetime,      // Never expires
    OneTime,       // Single payment, fixed end date
    PayAsYouGo     // No recurring charge, pay per session
}
```

### Membership (Enrollment Record)
```csharp
public class Membership : TenantEntity
{
    public Guid MemberId { get; private set; }
    public Guid MembershipTypeId { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public MembershipStatus Status { get; private set; }  // Active, Expired, Cancelled, PendingPayment
    public bool AutoRenew { get; private set; }
    public decimal PricePaid { get; private set; }
    public decimal? DiscountApplied { get; private set; }
    public DiscountType? DiscountType { get; private set; }
}
```

---

## 🔧 Enrollment Flow

### Age Validation
When a member enrolls in a membership type with age limits:

```csharp
public Result ValidateAge(DateTime? dateOfBirth, MembershipType type)
{
    if (!dateOfBirth.HasValue && (type.MinAge.HasValue || type.MaxAge.HasValue))
        return Result.Failure("Date of birth required for age-restricted membership.");

    var age = CalculateAge(dateOfBirth.Value);

    if (type.MinAge.HasValue && age < type.MinAge.Value)
        return Result.Failure($"Minimum age is {type.MinAge}. Member is {age}.");

    if (type.MaxAge.HasValue && age > type.MaxAge.Value)
        return Result.Failure($"Maximum age is {type.MaxAge}. Member is {age}.");

    return Result.Success();
}
```

### Enrollment Command Flow
```
1. Validate member exists and is Active
2. Validate membership type is active
3. Check age eligibility
4. Check capacity (if limited)
   → If at capacity: add to waitlist instead
5. Calculate price (apply discount if provided)
6. Create Membership record
7. Trigger payment (integration event to Payments module)
8. Return enrollment confirmation
```

---

## ⚙️ Auto-Renewal

When `AutoRenew = true` and the membership approaches its `EndDate`:

```csharp
public void Renew(DateTime newEndDate)
{
    if (Status != MembershipStatus.Active && Status != MembershipStatus.Expired)
        throw new InvalidOperationException($"Cannot renew membership in {Status} status.");

    StartDate = EndDate;        // New period starts where old one ended
    EndDate = newEndDate;       // Calculated based on BillingCycle
    Status = MembershipStatus.Active;
    UpdatedAt = DateTime.UtcNow;
}
```

**Renewal job (background):**
- Runs daily
- Finds memberships where `EndDate <= today + 3 days` and `AutoRenew = true`
- Publishes `MembershipRenewalDueEvent` → Payments module charges the member
- On successful payment → calls `Renew(newEndDate)`
- On failed payment → sets `Status = PendingPayment`

---

## ❄️ Membership Freeze

Members can freeze their membership for 1-365 days:

```csharp
public class MembershipFreeze : TenantEntity
{
    public Guid MembershipId { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public decimal FeeCharged { get; private set; }   // From MembershipType.FreezeFee
    public string? Reason { get; private set; }
}
```

**Freeze rules:**
- Only Active memberships can be frozen
- Duration: 1-365 days
- Optional freeze fee (configured per MembershipType)
- Membership end date is extended by the freeze duration
- Multiple freezes allowed (but not overlapping)
- Member cannot book sessions during freeze period

**Freeze flow:**
```
1. Validate membership is Active
2. Validate freeze duration (1-365 days)
3. Validate no overlapping freeze exists
4. Create MembershipFreeze record
5. Extend membership EndDate by freeze duration
6. Charge freeze fee (if configured)
```

---

## 💰 Discounts (6 Types)

```csharp
public enum DiscountType
{
    EarlyBird,    // Time-limited: enroll before X date
    Loyalty,      // Based on membership tenure (years as member)
    Family,       // Multiple family members enrolled
    Corporate,    // Company/organization partnership
    PromoCode,    // Manual code entry at enrollment
    Referral      // Referred by existing member
}
```

### MembershipDiscount Entity
```csharp
public class MembershipDiscount : TenantEntity
{
    public Guid? MembershipTypeId { get; private set; }  // null = applies to all types
    public DiscountType DiscountType { get; private set; }
    public bool IsPercentage { get; private set; }       // true = %, false = fixed amount
    public decimal Value { get; private set; }           // 10 = 10% or £10
    public DateTime ValidFrom { get; private set; }
    public DateTime ValidTo { get; private set; }
    public string? PromoCode { get; private set; }       // For PromoCode type
    public int? MaxUses { get; private set; }            // null = unlimited
    public int CurrentUses { get; private set; }
}
```

### Discount Calculation
```csharp
public decimal CalculateDiscount(decimal basePrice)
{
    if (IsPercentage)
    {
        var discount = Math.Round(basePrice * Value / 100m, 2);
        return Math.Min(discount, basePrice); // Never exceed base price
    }
    return Math.Min(Value, basePrice); // Fixed amount, capped at base price
}

public bool IsValid(DateTime asOf)
{
    if (asOf < ValidFrom || asOf > ValidTo) return false;
    if (MaxUses.HasValue && CurrentUses >= MaxUses.Value) return false;
    return true;
}
```

---

## 📋 Capacity-Based Waitlist

When a membership type has a capacity limit and is full:

```csharp
public class MembershipWaitlist : TenantEntity
{
    public Guid MembershipTypeId { get; private set; }
    public Guid MemberId { get; private set; }
    public int Position { get; private set; }          // 1-based, ordered by RequestedAt
    public DateTime RequestedAt { get; private set; }
    public DateTime? NotifiedAt { get; private set; }
    public string Status { get; private set; }         // Waiting, Offered, Accepted, Expired
}
```

**Waitlist flow:**
```
1. Member tries to enroll → capacity full
2. Add to waitlist with next position number
3. When a spot opens (cancellation/expiry):
   a. Find position 1 on waitlist
   b. Send notification (email + in-app)
   c. Set Status = Offered, NotifiedAt = now
   d. Member has 48 hours to accept
4. If accepted → enroll member, remove from waitlist
5. If expired (48h) → move to next in queue, set Status = Expired
```

---

## 🌐 API Endpoints

| Method | Route | Permission | Purpose |
|--------|-------|-----------|---------|
| GET | /api/v1/memberships/types | ViewMembers | List membership types |
| POST | /api/v1/memberships/types | ManageMembers | Create membership type |
| PUT | /api/v1/memberships/types/{id} | ManageMembers | Update membership type |
| POST | /api/v1/memberships/enroll | ManageMembers | Enroll member |
| PUT | /api/v1/memberships/{id}/renew | ManageMembers | Manual renewal |
| POST | /api/v1/memberships/{id}/freeze | ManageMembers | Freeze membership |
| PUT | /api/v1/memberships/{id}/cancel | ManageMembers | Cancel membership |
| GET | /api/v1/memberships/discounts | ManageMembers | List discounts |
| POST | /api/v1/memberships/discounts | ManageMembers | Create discount |
| POST | /api/v1/memberships/discounts/validate | ViewMembers | Validate promo code |
| GET | /api/v1/memberships/waitlist/{typeId} | ViewMembers | View waitlist |

---

## 🧪 Testing Approach

### Property Tests
```
Property 10: Discount Never Exceeds Base Price
  For ANY discount (percentage or fixed) applied to ANY base price,
  the calculated discount SHALL never exceed the base price.

Property 11: Waitlist Position Ordering
  For ANY membership type waitlist,
  positions SHALL be sequential (1, 2, 3, ...) with no gaps,
  ordered by RequestedAt timestamp.
```

### Unit Tests
- Enroll with valid age → success
- Enroll under minimum age → failure with message
- Enroll at capacity → added to waitlist
- Freeze active membership → freeze record created, end date extended
- Freeze cancelled membership → throws InvalidOperationException
- Apply 50% discount to £100 → £50 discount
- Apply £200 fixed discount to £100 → capped at £100
- Expired promo code → IsValid returns false

---

## 🚀 How to Extend

### Adding a new billing cycle:
1. Add value to `BillingCycle` enum
2. Update end-date calculation logic in enrollment handler
3. Update renewal job to handle the new cycle duration

### Adding a new discount type:
1. Add value to `DiscountType` enum
2. Add eligibility logic in the discount validation service
3. Update the enrollment handler to check the new type
