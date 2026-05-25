# 14 — Equipment & Loan Management

## 📖 Feature Overview

The Equipment module tracks club-owned equipment (bats, balls, pads, stumps, training aids), manages loans to members, handles reservations, and monitors maintenance and depreciation. It ensures equipment is available when needed and in safe condition for use.

### Key Capabilities
- Equipment inventory with category and condition tracking
- Loanability validation (condition-based)
- Loan lifecycle management with overdue detection
- Reservation system with conflict detection (365-day max)
- Maintenance scheduling with condition updates
- Depreciation calculation for asset management
- Equipment history and audit trail

---

## 🏗️ Architecture & Design Decisions

| Decision | Rationale |
|----------|-----------|
| Condition-based loanability | Safety: damaged equipment cannot be loaned |
| Separate Loan and Reservation entities | Loans are active (item out); reservations are future (item reserved) |
| 365-day max reservation | Prevents indefinite hoarding; forces periodic review |
| Maintenance triggers condition update | Repair can improve condition; inspection can downgrade |
| Depreciation as calculated value | Not stored; derived from purchase price, age, and method |
| Overdue detection via background job | Daily scan; triggers notifications and potential fees |
| Equipment per club (tenant-scoped) | Each club manages its own inventory independently |

---

## 📊 Data Model

### EquipmentItem Entity
```csharp
public class EquipmentItem : TenantEntity
{
    public string Name { get; private set; }              // "Gray-Nicolls Bat #3"
    public string? Description { get; private set; }
    public string? SerialNumber { get; private set; }     // Manufacturer serial
    public string AssetTag { get; private set; }          // Club's internal tag: "EQ-001"
    public EquipmentCategory Category { get; private set; }
    public EquipmentCondition Condition { get; private set; }
    public decimal PurchasePrice { get; private set; }
    public DateTime PurchaseDate { get; private set; }
    public int? ExpectedLifespanYears { get; private set; }
    public string? StorageLocation { get; private set; }  // "Equipment shed, Shelf B"
    public bool IsActive { get; private set; }            // False = decommissioned
    public int Quantity { get; private set; }             // For bulk items (e.g., 12 balls)
}
```

### EquipmentCategory Enum
```csharp
public enum EquipmentCategory
{
    Bat,
    Ball,
    Pads,
    Gloves,
    Helmet,
    Stumps,
    TrainingAid,
    ProtectiveGear,
    ClothingKit,
    Maintenance,    // Rollers, covers, etc.
    Electronics,    // Scoring tablets, cameras
    Furniture,      // Benches, scoreboards
    Other
}
```

### EquipmentCondition Enum
```csharp
public enum EquipmentCondition
{
    New,              // Just purchased, unused
    Good,             // Normal wear, fully functional
    Fair,             // Visible wear, still functional
    NeedsRepair,     // Requires maintenance before use
    Damaged,          // Significant damage, unsafe to use
    Decommissioned    // End of life, to be disposed
}
```

### Loanability Validation
```csharp
public bool IsLoanable()
{
    // Equipment can only be loaned if in acceptable condition
    return Condition != EquipmentCondition.NeedsRepair
        && Condition != EquipmentCondition.Damaged
        && Condition != EquipmentCondition.Decommissioned
        && IsActive;
}
```

**Loanable conditions:** New, Good, Fair
**Non-loanable conditions:** NeedsRepair, Damaged, Decommissioned

---

## 📦 Equipment Loans

### EquipmentLoan Entity
```csharp
public class EquipmentLoan : TenantEntity
{
    public Guid EquipmentItemId { get; private set; }
    public Guid MemberId { get; private set; }
    public DateTime LoanDate { get; private set; }
    public DateTime DueDate { get; private set; }
    public DateTime? ReturnDate { get; private set; }
    public LoanStatus Status { get; private set; }
    public EquipmentCondition ConditionOnLoan { get; private set; }    // Condition when loaned
    public EquipmentCondition? ConditionOnReturn { get; private set; } // Condition when returned
    public string? Notes { get; private set; }
    public decimal? LateFee { get; private set; }         // Charged if overdue
}

public enum LoanStatus
{
    Active,       // Item is currently on loan
    Returned,     // Item returned in acceptable condition
    Overdue,      // Past due date, not yet returned
    Lost,         // Member reports item lost
    Damaged       // Returned in damaged condition
}
```

### Loan Lifecycle State Machine
```
┌────────┐   DueDate passes   ┌─────────┐
│ Active │───────────────────▶│ Overdue │
└───┬────┘                    └────┬────┘
    │                              │
    │ Return()                     │ Return()
    ▼                              ▼
┌──────────┐                 ┌──────────┐
│ Returned │                 │ Returned │ (with late fee)
└──────────┘                 └──────────┘

┌────────┐   ReportLost()    ┌────────┐
│ Active │──────────────────▶│  Lost  │
└────────┘                   └────────┘

┌────────┐   Return(damaged) ┌─────────┐
│ Active │──────────────────▶│ Damaged │
└────────┘                   └─────────┘
```

### Loan Creation with Validation
```csharp
public static EquipmentLoan Create(Guid clubId, Guid equipmentItemId, Guid memberId,
    DateTime dueDate, EquipmentItem item)
{
    if (!item.IsLoanable())
        throw new InvalidOperationException(
            $"Equipment '{item.Name}' cannot be loaned. Condition: {item.Condition}");

    if (dueDate <= DateTime.UtcNow)
        throw new ArgumentException("Due date must be in the future.");

    if (dueDate > DateTime.UtcNow.AddDays(365))
        throw new ArgumentException("Loan period cannot exceed 365 days.");

    return new EquipmentLoan
    {
        EquipmentItemId = equipmentItemId,
        MemberId = memberId,
        LoanDate = DateTime.UtcNow,
        DueDate = dueDate,
        Status = LoanStatus.Active,
        ConditionOnLoan = item.Condition
    };
}
```

### Overdue Detection (Background Job)
```csharp
public class OverdueEquipmentJob : IScheduledJob
{
    public async Task Execute()
    {
        var overdueLoans = await _db.EquipmentLoans
            .Where(l => l.Status == LoanStatus.Active && l.DueDate < DateTime.UtcNow)
            .ToListAsync();

        foreach (var loan in overdueLoans)
        {
            loan.MarkOverdue();
            await _mediator.Publish(new EquipmentOverdueEvent(
                loan.Id, loan.MemberId, loan.EquipmentItemId, loan.DueDate));
        }
    }
}
```

---

## 📋 Equipment Reservations

### EquipmentReservation Entity
```csharp
public class EquipmentReservation : TenantEntity
{
    public Guid EquipmentItemId { get; private set; }
    public Guid MemberId { get; private set; }
    public DateTime ReservedFrom { get; private set; }
    public DateTime ReservedUntil { get; private set; }
    public ReservationStatus Status { get; private set; }
    public string? Purpose { get; private set; }          // "Match day", "Practice"
}

public enum ReservationStatus
{
    Pending,      // Awaiting confirmation
    Confirmed,    // Reservation active
    Collected,    // Item picked up (converts to Loan)
    Cancelled,    // Cancelled by member or admin
    Expired       // Not collected by ReservedFrom date
}
```

### Reservation Conflict Detection
```csharp
public async Task<bool> HasConflict(Guid equipmentItemId, DateTime from, DateTime until,
    Guid? excludeReservationId = null)
{
    // Check against existing reservations
    var reservationConflict = await _db.EquipmentReservations
        .AnyAsync(r => r.EquipmentItemId == equipmentItemId
            && r.Status == ReservationStatus.Confirmed
            && r.ReservedFrom < until
            && r.ReservedUntil > from
            && r.Id != excludeReservationId);

    // Check against active loans
    var loanConflict = await _db.EquipmentLoans
        .AnyAsync(l => l.EquipmentItemId == equipmentItemId
            && (l.Status == LoanStatus.Active || l.Status == LoanStatus.Overdue)
            && l.LoanDate < until
            && l.DueDate > from);

    return reservationConflict || loanConflict;
}
```

**Reservation rules:**
- Maximum reservation period: 365 days
- Cannot reserve equipment that is NeedsRepair/Damaged/Decommissioned
- Reservation expires if not collected within 24 hours of `ReservedFrom`
- Collecting a reservation creates an `EquipmentLoan` record

---

## 🔧 Equipment Maintenance

### EquipmentMaintenance Entity
```csharp
public class EquipmentMaintenance : TenantEntity
{
    public Guid EquipmentItemId { get; private set; }
    public string Description { get; private set; }       // "Re-grip bat handle"
    public MaintenanceType Type { get; private set; }
    public DateTime ScheduledDate { get; private set; }
    public DateTime? CompletedDate { get; private set; }
    public EquipmentCondition ConditionBefore { get; private set; }
    public EquipmentCondition? ConditionAfter { get; private set; }
    public decimal? Cost { get; private set; }
    public string? PerformedBy { get; private set; }      // Staff name or external vendor
}

public enum MaintenanceType
{
    Inspection,       // Routine check
    Repair,           // Fix specific issue
    Cleaning,         // Deep clean
    Replacement,      // Replace component (e.g., grip, string)
    Certification     // Safety certification (e.g., helmets)
}
```

### Condition Tracking After Maintenance
```csharp
public void CompleteMaintenance(EquipmentCondition conditionAfter, decimal? cost)
{
    CompletedDate = DateTime.UtcNow;
    ConditionAfter = conditionAfter;
    Cost = cost;

    // Update the equipment item's condition
    _equipmentItem.UpdateCondition(conditionAfter);
}
```

---

## 📉 Depreciation Calculation

```csharp
public class DepreciationCalculator
{
    // Straight-line depreciation
    public decimal CalculateCurrentValue(EquipmentItem item)
    {
        if (!item.ExpectedLifespanYears.HasValue || item.ExpectedLifespanYears == 0)
            return item.PurchasePrice; // No depreciation configured

        var ageYears = (DateTime.UtcNow - item.PurchaseDate).TotalDays / 365.25;
        var annualDepreciation = item.PurchasePrice / item.ExpectedLifespanYears.Value;
        var totalDepreciation = (decimal)ageYears * annualDepreciation;

        // Cannot depreciate below zero
        return Math.Max(0, item.PurchasePrice - totalDepreciation);
    }

    // Book value at a specific date
    public decimal CalculateValueAtDate(EquipmentItem item, DateTime asOfDate)
    {
        if (!item.ExpectedLifespanYears.HasValue) return item.PurchasePrice;

        var ageYears = (asOfDate - item.PurchaseDate).TotalDays / 365.25;
        var annualDepreciation = item.PurchasePrice / item.ExpectedLifespanYears.Value;
        var totalDepreciation = (decimal)ageYears * annualDepreciation;

        return Math.Max(0, item.PurchasePrice - totalDepreciation);
    }
}
```

**Example:**
- Bat purchased for £300, expected lifespan 5 years
- After 2 years: £300 - (£60/year × 2) = £180 current value
- After 5 years: £300 - (£60/year × 5) = £0 (fully depreciated)

---

## 🌐 API Endpoints

| Method | Route | Permission | Purpose |
|--------|-------|-----------|---------|
| GET | /api/v1/equipment | ViewMembers | List equipment |
| POST | /api/v1/equipment | ManageMembers | Add equipment item |
| PUT | /api/v1/equipment/{id} | ManageMembers | Update equipment |
| PUT | /api/v1/equipment/{id}/condition | ManageMembers | Update condition |
| POST | /api/v1/equipment/{id}/loans | ManageMembers | Create loan |
| PUT | /api/v1/equipment/loans/{id}/return | ManageMembers | Return item |
| GET | /api/v1/equipment/loans/overdue | ManageMembers | List overdue loans |
| POST | /api/v1/equipment/{id}/reservations | ViewMembers | Reserve equipment |
| PUT | /api/v1/equipment/reservations/{id}/cancel | ViewMembers | Cancel reservation |
| POST | /api/v1/equipment/{id}/maintenance | ManageMembers | Schedule maintenance |
| GET | /api/v1/equipment/depreciation | ManageMembers | Get depreciation report |

---

## 🧪 Testing Approach

### Property Tests
```
Property 21: Loanability by Condition
  For ANY equipment item, IsLoanable() SHALL return false
  if and only if Condition is NeedsRepair, Damaged, or Decommissioned.

Property 22: Depreciation Bounds
  For ANY equipment item, the calculated current value
  SHALL be >= 0 AND <= PurchasePrice.

Property 23: Reservation No Overlap
  For ANY equipment item at ANY point in time,
  there SHALL be at most one confirmed reservation or active loan.
```

### Unit Tests
- Loan equipment in Good condition → success
- Loan equipment in NeedsRepair condition → throws
- Loan with due date > 365 days → throws
- Return loan after due date → marked Overdue, late fee applied
- Reserve already-loaned item for overlapping period → conflict detected
- Depreciate £300 bat after 5 years (5-year lifespan) → £0
- Depreciate £300 bat after 2 years (5-year lifespan) → £180
- Complete maintenance → equipment condition updated

---

## 🚀 How to Extend

### Adding equipment check-in/check-out kiosk:
1. Create `CheckInOut` entity with barcode/QR scanning
2. Link to `EquipmentLoan` for automatic loan creation on check-out
3. Auto-return on check-in with condition prompt

### Adding equipment insurance tracking:
1. Add `InsurancePolicy` entity linked to high-value items
2. Track policy number, provider, expiry date, coverage amount
3. Alert when policies are approaching renewal
