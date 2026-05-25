# 13 — Facility Booking & Conflict Detection

## 📖 Feature Overview

The Facilities module manages bookable spaces (pitches, courts, nets, meeting rooms) with availability schedules, dynamic pricing, conflict detection, and maintenance windows. It ensures no double-bookings occur and suggests alternatives when conflicts arise.

### Key Capabilities
- Facility management with type classification and capacity
- Operating schedule configuration (per day-of-week availability)
- Dynamic pricing (peak/off-peak, member/non-member rates)
- Booking with duration validation (30-240 min in 30-min increments)
- Conflict detection across bookings, maintenance, and blockouts
- Alternative slot suggestion (up to 3 options)
- Maintenance scheduling with auto-cancel of conflicting bookings
- Advance booking limit (30 days maximum)

---

## 🏗️ Architecture & Design Decisions

| Decision | Rationale |
|----------|-----------|
| Facility as top-level entity | Reusable across sessions, events, and ad-hoc bookings |
| Availability as separate entity | Different hours per day; seasonal changes without modifying facility |
| Pricing tiers (peak/off-peak + member/non-member) | Covers 95% of club pricing models |
| 30-min increment enforcement | Standard for sports facilities; prevents fragmentation |
| Conflict detection as a service | Centralised logic; used by bookings, maintenance, and events |
| 3 alternative suggestions | Helpful without overwhelming; ordered by proximity to requested time |
| 30-day advance limit | Prevents hoarding; keeps availability fair |
| Maintenance auto-cancels bookings | Safety/quality takes priority; affected members notified |

---

## 📊 Data Model

### Facility Entity
```csharp
public class Facility : TenantEntity
{
    public string Name { get; private set; }              // "Main Pitch", "Indoor Net 1"
    public string? Description { get; private set; }
    public FacilityType FacilityType { get; private set; }
    public int Capacity { get; private set; }             // Max people at once
    public string? Location { get; private set; }         // "Building A, Ground Floor"
    public bool IsActive { get; private set; }
    public bool RequiresApproval { get; private set; }    // Admin must approve bookings

    public ICollection<FacilityAvailability> Availability { get; private set; }
    public ICollection<FacilityPricing> Pricing { get; private set; }
}

public enum FacilityType
{
    Pitch,          // Outdoor playing field
    IndoorNet,      // Indoor cricket/batting net
    OutdoorNet,     // Outdoor practice net
    Court,          // Tennis/basketball court
    SwimmingPool,   // Pool lane or full pool
    Gym,            // Fitness area
    MeetingRoom,    // Clubhouse meeting room
    FunctionHall,   // Large event space
    ChangingRoom,   // Changing/locker room
    Pavilion        // Clubhouse/pavilion area
}
```

### FacilityAvailability (Operating Schedule)
```csharp
public class FacilityAvailability : TenantEntity
{
    public Guid FacilityId { get; private set; }
    public DayOfWeek DayOfWeek { get; private set; }      // Monday-Sunday
    public TimeSpan OpenTime { get; private set; }        // e.g., 07:00
    public TimeSpan CloseTime { get; private set; }       // e.g., 22:00
    public bool IsClosed { get; private set; }            // Override: facility closed this day
}
```

**Example schedule:**
| Day | Open | Close | Notes |
|-----|------|-------|-------|
| Monday | 07:00 | 22:00 | Normal hours |
| Tuesday | 07:00 | 22:00 | Normal hours |
| Wednesday | 07:00 | 22:00 | Normal hours |
| Thursday | 07:00 | 22:00 | Normal hours |
| Friday | 07:00 | 21:00 | Early close |
| Saturday | 08:00 | 18:00 | Weekend hours |
| Sunday | — | — | Closed |

### FacilityPricing (Dynamic Rates)
```csharp
public class FacilityPricing : TenantEntity
{
    public Guid FacilityId { get; private set; }
    public PricingTier Tier { get; private set; }
    public decimal HourlyRate { get; private set; }       // Price per hour
    public TimeSpan? PeakStartTime { get; private set; }  // e.g., 17:00
    public TimeSpan? PeakEndTime { get; private set; }    // e.g., 21:00
    public bool AppliesToWeekends { get; private set; }   // Weekend = peak?
}

public enum PricingTier
{
    MemberPeak,         // Member during peak hours
    MemberOffPeak,      // Member during off-peak hours
    NonMemberPeak,      // Non-member during peak hours
    NonMemberOffPeak    // Non-member during off-peak hours
}
```

**Pricing example (Indoor Net):**
| Tier | Hourly Rate |
|------|-------------|
| Member Off-Peak | £15.00 |
| Member Peak | £25.00 |
| Non-Member Off-Peak | £25.00 |
| Non-Member Peak | £40.00 |

Peak hours: 17:00-21:00 weekdays + all weekend hours.

---

## 📅 Facility Booking

### FacilityBooking Entity
```csharp
public class FacilityBooking : TenantEntity
{
    public Guid FacilityId { get; private set; }
    public Guid BookedByMemberId { get; private set; }
    public DateTime StartDateTime { get; private set; }
    public DateTime EndDateTime { get; private set; }
    public int DurationMinutes { get; private set; }      // 30-240 in 30-min increments
    public decimal TotalPrice { get; private set; }
    public BookingStatus Status { get; private set; }
    public string? Purpose { get; private set; }          // "Team practice", "Private hire"
    public Guid? PaymentId { get; private set; }
    public Guid? SessionId { get; private set; }          // If booked for a training session
    public Guid? EventId { get; private set; }            // If booked for an event
}

public enum BookingStatus
{
    Pending,        // Awaiting approval (if RequiresApproval)
    Confirmed,      // Approved and paid
    Cancelled,      // Cancelled by member or admin
    Completed,      // Time has passed, booking fulfilled
    NoShow          // Member didn't turn up
}
```

### Duration Validation
```csharp
public static FacilityBooking Create(Guid clubId, Guid facilityId, Guid memberId,
    DateTime startDateTime, int durationMinutes, decimal totalPrice, string? purpose)
{
    // Must be 30-240 minutes
    if (durationMinutes < 30 || durationMinutes > 240)
        throw new ArgumentException("Duration must be between 30 and 240 minutes.");

    // Must be in 30-minute increments
    if (durationMinutes % 30 != 0)
        throw new ArgumentException("Duration must be in 30-minute increments.");

    // Cannot book more than 30 days in advance
    if (startDateTime > DateTime.UtcNow.AddDays(30))
        throw new ArgumentException("Cannot book more than 30 days in advance.");

    // Cannot book in the past
    if (startDateTime < DateTime.UtcNow)
        throw new ArgumentException("Cannot book in the past.");

    var endDateTime = startDateTime.AddMinutes(durationMinutes);

    return new FacilityBooking { /* ... */ };
}
```

---

## 🔍 Conflict Detection Service

### ConflictDetectionService
```csharp
public class ConflictDetectionService : IConflictDetectionService
{
    public async Task<ConflictResult> CheckForConflicts(
        Guid facilityId, DateTime start, DateTime end, Guid? excludeBookingId = null)
    {
        var conflicts = new List<ConflictInfo>();

        // 1. Check existing bookings
        var bookingConflicts = await _db.FacilityBookings
            .Where(b => b.FacilityId == facilityId
                && b.Status != BookingStatus.Cancelled
                && b.StartDateTime < end
                && b.EndDateTime > start
                && b.Id != excludeBookingId)
            .ToListAsync();

        // 2. Check maintenance windows
        var maintenanceConflicts = await _db.FacilityMaintenance
            .Where(m => m.FacilityId == facilityId
                && m.StartDateTime < end
                && m.EndDateTime > start
                && m.Status != MaintenanceStatus.Cancelled)
            .ToListAsync();

        // 3. Check blockout periods
        var blockoutConflicts = await _db.FacilityBlockouts
            .Where(b => b.FacilityId == facilityId
                && b.StartDateTime < end
                && b.EndDateTime > start)
            .ToListAsync();

        foreach (var b in bookingConflicts)
            conflicts.Add(new ConflictInfo(ConflictType.Booking, b.StartDateTime, b.EndDateTime));
        foreach (var m in maintenanceConflicts)
            conflicts.Add(new ConflictInfo(ConflictType.Maintenance, m.StartDateTime, m.EndDateTime));
        foreach (var b in blockoutConflicts)
            conflicts.Add(new ConflictInfo(ConflictType.Blockout, b.StartDateTime, b.EndDateTime));

        return new ConflictResult(conflicts.Any(), conflicts);
    }
}
```

### Overlap Detection Logic
```
Two time ranges overlap if: Start1 < End2 AND End1 > Start2

┌─────────────┐
│  Existing   │
└─────────────┘
      ┌─────────────┐
      │  Requested  │  ← CONFLICT (overlaps)
      └─────────────┘

┌─────────────┐
│  Existing   │
└─────────────┘
                  ┌─────────────┐
                  │  Requested  │  ← NO CONFLICT (adjacent is OK)
                  └─────────────┘
```

---

## 💡 Alternative Slot Suggestion

When a conflict is detected, suggest up to 3 alternative slots:

```csharp
public async Task<List<AlternativeSlot>> SuggestAlternatives(
    Guid facilityId, DateTime requestedStart, int durationMinutes, int maxSuggestions = 3)
{
    var alternatives = new List<AlternativeSlot>();
    var duration = TimeSpan.FromMinutes(durationMinutes);

    // Strategy 1: Try later the same day (30-min increments)
    var candidate = requestedStart.AddMinutes(30);
    while (alternatives.Count < maxSuggestions && candidate.Date == requestedStart.Date)
    {
        var end = candidate.Add(duration);
        var conflict = await CheckForConflicts(facilityId, candidate, end);
        if (!conflict.HasConflict && IsWithinOperatingHours(facilityId, candidate, end))
        {
            alternatives.Add(new AlternativeSlot(candidate, end));
        }
        candidate = candidate.AddMinutes(30);
    }

    // Strategy 2: Try same time on next available days
    if (alternatives.Count < maxSuggestions)
    {
        for (int dayOffset = 1; dayOffset <= 7 && alternatives.Count < maxSuggestions; dayOffset++)
        {
            candidate = requestedStart.AddDays(dayOffset);
            var end = candidate.Add(duration);
            var conflict = await CheckForConflicts(facilityId, candidate, end);
            if (!conflict.HasConflict && IsWithinOperatingHours(facilityId, candidate, end))
            {
                alternatives.Add(new AlternativeSlot(candidate, end));
            }
        }
    }

    return alternatives;
}
```

---

## 🔧 Facility Maintenance

### FacilityMaintenance Entity
```csharp
public class FacilityMaintenance : TenantEntity
{
    public Guid FacilityId { get; private set; }
    public string Title { get; private set; }             // "Pitch resurfacing"
    public string? Description { get; private set; }
    public DateTime StartDateTime { get; private set; }
    public DateTime EndDateTime { get; private set; }
    public MaintenanceStatus Status { get; private set; } // Scheduled, InProgress, Completed, Cancelled
    public MaintenancePriority Priority { get; private set; } // Low, Medium, High, Emergency
    public Guid? AssignedToMemberId { get; private set; } // Groundskeeper/staff
}

public enum MaintenanceStatus { Scheduled, InProgress, Completed, Cancelled }
public enum MaintenancePriority { Low, Medium, High, Emergency }
```

### Auto-Cancel Conflicting Bookings
```csharp
public async Task ScheduleMaintenance(FacilityMaintenance maintenance)
{
    // Find all confirmed bookings that overlap with maintenance window
    var conflictingBookings = await _db.FacilityBookings
        .Where(b => b.FacilityId == maintenance.FacilityId
            && b.Status == BookingStatus.Confirmed
            && b.StartDateTime < maintenance.EndDateTime
            && b.EndDateTime > maintenance.StartDateTime)
        .ToListAsync();

    foreach (var booking in conflictingBookings)
    {
        booking.Cancel("Facility maintenance scheduled");

        // Trigger refund
        await _mediator.Publish(new BookingCancelledEvent(
            booking.Id, booking.BookedByMemberId, booking.TotalPrice,
            CancellationReason.Maintenance));
    }

    await _db.FacilityMaintenance.AddAsync(maintenance);
    await _db.SaveChangesAsync();
}
```

---

## 🌐 API Endpoints

| Method | Route | Permission | Purpose |
|--------|-------|-----------|---------|
| GET | /api/v1/facilities | ViewMembers | List facilities |
| POST | /api/v1/facilities | ManageMembers | Create facility |
| PUT | /api/v1/facilities/{id} | ManageMembers | Update facility |
| GET | /api/v1/facilities/{id}/availability | ViewMembers | Get operating hours |
| PUT | /api/v1/facilities/{id}/availability | ManageMembers | Set operating hours |
| GET | /api/v1/facilities/{id}/pricing | ViewMembers | Get pricing tiers |
| POST | /api/v1/facilities/bookings | ViewMembers | Create booking |
| GET | /api/v1/facilities/bookings | ViewMembers | List bookings |
| PUT | /api/v1/facilities/bookings/{id}/cancel | ViewMembers | Cancel booking |
| POST | /api/v1/facilities/bookings/check-availability | ViewMembers | Check conflicts |
| POST | /api/v1/facilities/maintenance | ManageMembers | Schedule maintenance |
| GET | /api/v1/facilities/{id}/slots | ViewMembers | Get available slots |

---

## 🧪 Testing Approach

### Property Tests
```
Property 18: No Double Bookings
  For ANY facility at ANY point in time,
  there SHALL be at most one confirmed booking occupying that time slot.

Property 19: Duration Increment Validity
  For ANY facility booking, the duration in minutes
  SHALL be divisible by 30 AND between 30 and 240 inclusive.

Property 20: Alternative Slots Are Conflict-Free
  For ANY suggested alternative slot,
  a conflict check against that slot SHALL return no conflicts.
```

### Unit Tests
- Book 60-min slot with no conflicts → confirmed
- Book overlapping slot → conflict detected
- Book 45-min slot → throws (not 30-min increment)
- Book 300-min slot → throws (exceeds 240 max)
- Book 31 days ahead → throws (exceeds advance limit)
- Schedule maintenance → conflicting bookings auto-cancelled
- Request alternatives → returns up to 3 conflict-free slots
- Book on closed day → throws (outside operating hours)

---

## 🚀 How to Extend

### Adding recurring bookings:
1. Create `RecurringBookingPattern` entity (weekly, fortnightly)
2. Generate individual `FacilityBooking` records for each occurrence
3. Conflict-check each occurrence; skip conflicting dates
4. Link all bookings via `RecurringPatternId`

### Adding facility images/floor plans:
1. Add `FacilityImage` entity with blob storage URL
2. Support multiple images per facility with ordering
3. Add thumbnail generation for list views
