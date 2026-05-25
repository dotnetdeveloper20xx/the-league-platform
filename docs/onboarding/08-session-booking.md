# 08 — Session & Booking Management

## 📖 Feature Overview

The Sessions module manages training sessions, coaching slots, and facility bookings. It handles session creation with validation, recurring schedule templates, capacity-controlled bookings, waitlists, cancellation deadlines, and attendance tracking.

### Key Capabilities
- Session creation with strict validation (duration 15-480 min, capacity 1-500)
- Recurring schedule templates (day-of-week patterns, up to 12-week horizon)
- Booking with real-time capacity check
- Waitlist (max 50 per session, position-based, 24-hour acceptance window)
- Cancellation deadline enforcement
- Attendance tracking (Confirmed → Attended/NoShow/Cancelled)

---

## 🏗️ Architecture & Design Decisions

| Decision | Rationale |
|----------|-----------|
| Validation in domain entity factory | Fail fast at creation — invalid sessions can't exist |
| `CurrentBookingCount` on Session | Denormalized for performance — avoids COUNT query on every booking |
| Waitlist max 50 | Prevents unbounded queues; realistic for sports sessions |
| 24-hour acceptance window | Fair to other waitlisted members; prevents indefinite holds |
| `CancellationDeadlineHours` per session | Flexible — a casual session might be 2h, a coached session 24h |
| RecurringSchedule generates Sessions | Template pattern — schedule defines the pattern, sessions are concrete instances |
| Attendance as booking status transitions | Simple state machine on the booking entity itself |

---

## 📊 Data Model

### Session Entity
```csharp
public class Session : TenantEntity
{
    public string Title { get; private set; }              // 1-100 characters
    public SessionCategory Category { get; private set; }  // Training, Match, Social, Coaching, etc.
    public Guid? VenueId { get; private set; }
    public string? VenueName { get; private set; }
    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }          // Calculated: StartTime + Duration
    public int Duration { get; private set; }              // 15-480 minutes
    public int Capacity { get; private set; }              // 1-500
    public decimal Fee { get; private set; }               // 0-9999.99
    public int CurrentBookingCount { get; private set; }   // Denormalized counter
    public bool IsCancelled { get; private set; }
    public string? CancellationReason { get; private set; }
    public int CancellationDeadlineHours { get; private set; } // Default: 24
}
```

### Session Creation Validation
```csharp
public static Session Create(Guid clubId, string title, SessionCategory category,
    Guid? venueId, string? venueName, DateTime startTime, int duration,
    int capacity, decimal fee, int cancellationDeadlineHours = 24)
{
    if (string.IsNullOrWhiteSpace(title) || title.Length > 100)
        throw new ArgumentException("Title must be between 1 and 100 characters.");
    if (duration < 15 || duration > 480)
        throw new ArgumentException("Duration must be between 15 and 480 minutes.");
    if (capacity < 1 || capacity > 500)
        throw new ArgumentException("Capacity must be between 1 and 500.");
    if (fee < 0 || fee > 9999.99m)
        throw new ArgumentException("Fee must be between 0 and 9999.99.");

    return new Session { /* ... */ EndTime = startTime.AddMinutes(duration) };
}
```

---

## 🔁 Recurring Schedule Templates

Recurring schedules define a pattern that auto-generates session instances:

```csharp
public class RecurringSchedule : TenantEntity
{
    public string Title { get; private set; }
    public SessionCategory Category { get; private set; }
    public Guid? VenueId { get; private set; }
    public string? VenueName { get; private set; }
    public DayOfWeek DayOfWeek { get; private set; }     // Monday, Tuesday, etc.
    public TimeOnly StartTime { get; private set; }       // e.g., 18:30
    public int Duration { get; private set; }             // minutes
    public int Capacity { get; private set; }
    public decimal Fee { get; private set; }
    public int HorizonWeeks { get; private set; }         // 1-12 weeks ahead
    public bool IsActive { get; private set; }
}
```

### Session Generation
```csharp
public List<Session> GenerateSessions(DateTime fromDate)
{
    var sessions = new List<Session>();
    var endDate = fromDate.AddDays(HorizonWeeks * 7);
    var current = fromDate;

    while (current <= endDate)
    {
        if (current.DayOfWeek == DayOfWeek)
        {
            var sessionStart = current.Date.Add(StartTime.ToTimeSpan());
            if (sessionStart > fromDate)
            {
                sessions.Add(Session.Create(ClubId, Title, Category,
                    VenueId, VenueName, sessionStart, Duration, Capacity, Fee));
            }
        }
        current = current.AddDays(1);
    }
    return sessions;
}
```

**Example:** A "Tuesday Evening Nets" schedule with `HorizonWeeks = 12` generates 12 sessions, one per Tuesday for the next 12 weeks.

**Generation trigger:** A background job runs weekly (or on-demand) to generate sessions for active schedules.

---

## 🎫 Booking Flow

### SessionBooking Entity
```csharp
public class SessionBooking : TenantEntity
{
    public Guid SessionId { get; private set; }
    public Guid MemberId { get; private set; }
    public BookingStatus Status { get; private set; }  // Confirmed, Attended, NoShow, Cancelled
    public DateTime BookedAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }
}
```

### Booking Command Flow
```
1. Validate session exists and is not cancelled
2. Check: session.CanBook() → CurrentBookingCount < Capacity
   → If full: check waitlist availability (< 50)
     → If waitlist available: add to waitlist
     → If waitlist full: return failure
3. Check: member doesn't already have a booking for this session
4. Create SessionBooking with Status = Confirmed
5. Call session.IncrementBookings()
6. If session has a fee: trigger payment
7. Return booking confirmation
```

### Capacity Check
```csharp
public bool CanBook() => !IsCancelled && CurrentBookingCount < Capacity;

public void IncrementBookings()
{
    if (CurrentBookingCount >= Capacity)
        throw new InvalidOperationException("Session is at full capacity.");
    CurrentBookingCount++;
}
```

---

## 📋 Waitlist

When a session is full, members can join the waitlist:

```csharp
public class Waitlist : TenantEntity
{
    public Guid SessionId { get; private set; }
    public Guid MemberId { get; private set; }
    public int Position { get; private set; }           // 1-based, sequential
    public DateTime RequestedAt { get; private set; }
    public DateTime? OfferedAt { get; private set; }
    public DateTime? ExpiresAt { get; private set; }    // OfferedAt + 24 hours
    public string Status { get; private set; }          // Waiting, Offered, Accepted, Expired
}
```

### Waitlist Rules
- **Maximum 50** entries per session (`IsWaitlistAvailable` checks this)
- **Position-based**: assigned sequentially on join
- **24-hour acceptance window**: when a spot opens, position 1 gets an offer
- **Expiry**: if not accepted within 24h, offer moves to position 2

### Waitlist Promotion Flow
```
1. Booking cancelled → session.DecrementBookings()
2. Find waitlist entry at Position 1 with Status = Waiting
3. Call entry.Offer() → sets OfferedAt, ExpiresAt = now + 24h, Status = Offered
4. Send notification to member (email + push)
5. Member accepts within 24h:
   → entry.Accept(), create SessionBooking, session.IncrementBookings()
6. Member doesn't respond in 24h:
   → entry.Expire(), promote next in queue
```

---

## ⏰ Cancellation Deadline

Each session has a `CancellationDeadlineHours` (default 24):

```csharp
public bool CanCancelBooking(DateTime now)
{
    var deadline = StartTime.AddHours(-CancellationDeadlineHours);
    return now <= deadline;
}
```

**Example:** Session at 18:00 with 24h deadline → cancellation allowed until 18:00 the day before.

**After deadline:**
- Cancellation is rejected (or requires admin override)
- No refund issued
- Member is still marked as booked (can be marked NoShow later)

---

## 📊 Attendance Tracking

After a session occurs, attendance is recorded via booking status transitions:

```
Confirmed → Attended    (member showed up)
Confirmed → NoShow     (member didn't show up)
Confirmed → Cancelled  (member cancelled before deadline)
```

```csharp
public void MarkAttended() { Status = BookingStatus.Attended; }
public void MarkNoShow() { Status = BookingStatus.NoShow; }
public void Cancel() { Status = BookingStatus.Cancelled; CancelledAt = DateTime.UtcNow; }
```

**Attendance can be marked:**
- Individually (coach marks each member)
- Bulk (mark all remaining as NoShow after session ends)
- Via QR code scan (member scans on arrival)

---

## 🌐 API Endpoints

| Method | Route | Permission | Purpose |
|--------|-------|-----------|---------|
| GET | /api/v1/sessions | ViewMembers | List sessions (filterable by date, category) |
| GET | /api/v1/sessions/{id} | ViewMembers | Get session details |
| POST | /api/v1/sessions | ManageSessions | Create session |
| PUT | /api/v1/sessions/{id}/cancel | ManageSessions | Cancel session |
| POST | /api/v1/sessions/{id}/book | BookSessions | Book a session |
| DELETE | /api/v1/sessions/{id}/book | BookSessions | Cancel booking |
| POST | /api/v1/sessions/{id}/waitlist | BookSessions | Join waitlist |
| PUT | /api/v1/sessions/{id}/attendance | ManageSessions | Record attendance |
| GET | /api/v1/sessions/schedules | ManageSessions | List recurring schedules |
| POST | /api/v1/sessions/schedules | ManageSessions | Create recurring schedule |
| POST | /api/v1/sessions/schedules/{id}/generate | ManageSessions | Generate sessions from schedule |

---

## 🧪 Testing Approach

### Property Tests
```
Property 12: Booking Count Consistency
  For ANY session, CurrentBookingCount SHALL equal the count of
  SessionBooking records with Status IN (Confirmed, Attended, NoShow)
  for that session.

Property 13: Waitlist Position Integrity
  For ANY session waitlist, positions SHALL be sequential (1, 2, 3, ...)
  with no gaps among entries with Status = Waiting.
```

### Unit Tests
- Create session with duration 10 → throws (below minimum 15)
- Create session with capacity 501 → throws (above maximum 500)
- Book when at capacity → returns failure / adds to waitlist
- Cancel before deadline → success
- Cancel after deadline → rejected
- Waitlist offer expires after 24h → next member promoted
- Generate sessions from recurring schedule → correct count and dates

---

## 🚀 How to Extend

### Adding a new session category:
1. Add value to `SessionCategory` enum
2. No other changes needed — categories are just labels

### Adding session prerequisites:
1. Add `RequiredMembershipTypeId` to Session entity
2. Check membership status in booking command handler
3. Return clear error if member doesn't have required membership
