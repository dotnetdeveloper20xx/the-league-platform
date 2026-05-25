# 09 — Event Management

## 📖 Feature Overview

The Events module handles club events — from social gatherings and AGMs to tournaments and fundraisers. Events support both ticketed (paid) and RSVP (free) registration models, QR code tickets, a full lifecycle state machine, cancellation with automatic refund initiation, and recurring event series.

### Key Capabilities
- 9 event types (Social, Tournament, AGM, Training, Fundraiser, Competition, Meeting, Presentation, Other)
- Ticketed events with standard/member pricing
- RSVP events with guest count tracking
- QR code generation for ticket check-in
- Full lifecycle state machine (Draft → Published → ... → Completed)
- Cancellation with automatic refund initiation for ticket holders
- Event series (recurring events, max 52 occurrences)

---

## 🏗️ Architecture & Design Decisions

| Decision | Rationale |
|----------|-----------|
| State machine with `EnsureValidTransition` | Prevents invalid lifecycle changes at domain level |
| Separate `EventTicket` and `EventRSVP` entities | Different data needs — tickets have price/QR, RSVPs have guest count |
| `EventRegistration` as unified tracking | Single entity tracks all registrations regardless of type |
| QR code data stored as string | Generated once, stored for fast retrieval (no re-generation) |
| `CurrentRegistrationCount` denormalized | Performance — avoids COUNT on every registration check |
| Event series with max 52 | Practical limit — one year of weekly events |
| Cancellation triggers refund event | Loose coupling — Events module doesn't handle payments directly |

---

## 📊 Data Model

### Event Entity
```csharp
public class Event : TenantEntity
{
    public string Title { get; private set; }                // 1-200 characters
    public string? Description { get; private set; }
    public EventType EventType { get; private set; }         // Social, Tournament, AGM, etc.
    public EventStatus Status { get; private set; }          // Draft → ... → Completed
    public DateTime StartDateTime { get; private set; }
    public DateTime EndDateTime { get; private set; }        // Must be after StartDateTime
    public Guid? VenueId { get; private set; }
    public string? VenueName { get; private set; }
    public int? Capacity { get; private set; }               // null = unlimited
    public int CurrentRegistrationCount { get; private set; }
    public bool IsTicketed { get; private set; }             // true = paid tickets
    public decimal? StandardPrice { get; private set; }      // Non-member price
    public decimal? MemberPrice { get; private set; }        // Member price (usually lower)
    public bool AllowRsvp { get; private set; }              // true = free RSVP
    public int CancellationDeadlineHours { get; private set; } // Default: 48
}
```

### EventType Enum
```csharp
public enum EventType
{
    Social,        // Club dinners, BBQs, parties
    Tournament,    // Competitive events
    AGM,           // Annual General Meeting
    Training,      // Training camps, workshops
    Fundraiser,    // Charity events, auctions
    Competition,   // Matches, leagues
    Meeting,       // Committee meetings
    Presentation,  // Awards nights, talks
    Other          // Catch-all
}
```

---

## 🔧 Event Lifecycle State Machine

```
┌───────┐     Publish()     ┌───────────┐    OpenRegistration()   ┌──────────────────┐
│ Draft │──────────────────▶│ Published │─────────────────────────▶│ RegistrationOpen │
└───┬───┘                   └─────┬─────┘                         └────────┬─────────┘
    │                             │                                        │
    │ Cancel()                    │ Cancel()                               │ CloseRegistration()
    │                             │ Postpone()                             │ Cancel()
    ▼                             ▼                                        │ Postpone()
┌───────────┐              ┌───────────┐                                   ▼
│ Cancelled │              │ Postponed │                          ┌────────────────────┐
└───────────┘              └───────────┘                          │ RegistrationClosed │
                                                                  └────────┬───────────┘
                                                                           │ Start()
                                                                           │ Cancel()
                                                                           │ Postpone()
                                                                           ▼
                                                                  ┌──────────────┐
                                                                  │  InProgress  │
                                                                  └──────┬───────┘
                                                                         │ Complete()
                                                                         │ Cancel()
                                                                         │ Postpone()
                                                                         ▼
                                                                  ┌───────────┐
                                                                  │ Completed │
                                                                  └───────────┘
```

### Allowed Transitions
| From | To |
|------|-----|
| Draft | Published, Cancelled |
| Published | RegistrationOpen, Cancelled, Postponed |
| RegistrationOpen | RegistrationClosed, Cancelled, Postponed |
| RegistrationClosed | InProgress, Cancelled, Postponed |
| InProgress | Completed, Cancelled, Postponed |
| Completed | — (terminal) |
| Cancelled | — (terminal) |
| Postponed | — (terminal) |

**Domain enforcement:**
```csharp
private void EnsureValidTransition(EventStatus newStatus)
{
    var allowed = GetAllowedTransitions(Status);
    if (!allowed.Contains(newStatus))
        throw new InvalidOperationException(
            $"Cannot transition from {Status} to {newStatus}.");
}
```

---

## 🎫 Ticketed Events

For paid events (`IsTicketed = true`):

```csharp
public class EventTicket : TenantEntity
{
    public Guid EventId { get; private set; }
    public Guid MemberId { get; private set; }
    public string TicketNumber { get; private set; }    // Unique identifier (e.g., "EVT-2024-001-T042")
    public string QRCodeData { get; private set; }      // Encoded ticket data for scanning
    public decimal PricePaid { get; private set; }      // MemberPrice or StandardPrice
    public DateTime PurchasedAt { get; private set; }
    public bool IsCheckedIn { get; private set; }
    public DateTime? CheckedInAt { get; private set; }
}
```

### QR Code Generation
```csharp
// QR code contains a signed payload:
var qrPayload = $"{eventId}|{ticketId}|{memberId}|{ticketNumber}";
var qrCodeData = QRCodeGenerator.Generate(qrPayload); // Base64 PNG or SVG
```

### Check-In Flow
```csharp
public void CheckIn()
{
    if (IsCheckedIn)
        throw new InvalidOperationException("Ticket has already been checked in.");
    IsCheckedIn = true;
    CheckedInAt = DateTime.UtcNow;
}
```

- Staff scans QR code at venue entrance
- System validates ticket: exists, not already checked in, correct event
- Marks ticket as checked in with timestamp

---

## 📝 RSVP Events

For free events (`AllowRsvp = true`):

```csharp
public class EventRSVP : TenantEntity
{
    public Guid EventId { get; private set; }
    public Guid MemberId { get; private set; }
    public RSVPResponse Response { get; private set; }  // Yes, No, Maybe
    public int GuestCount { get; private set; }         // Additional guests
    public DateTime RespondedAt { get; private set; }
}
```

**RSVP rules:**
- Members can change their response at any time before the event
- Guest count is tracked for capacity planning
- Capacity check includes member + guests

---

## 📋 Event Registration (Unified Tracking)

```csharp
public class EventRegistration : TenantEntity
{
    public Guid EventId { get; private set; }
    public Guid MemberId { get; private set; }
    public string RegistrationType { get; private set; }  // "Ticket" or "RSVP"
    public DateTime RegisteredAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public bool RefundInitiated { get; private set; }
}
```

### Registration Flow
```
1. Validate event status is RegistrationOpen
2. Check capacity: event.IsAtCapacity()
3. If ticketed:
   a. Calculate price (MemberPrice or StandardPrice)
   b. Create EventTicket with QR code
   c. Trigger payment
4. If RSVP:
   a. Create EventRSVP record
5. Create EventRegistration record
6. Increment event.CurrentRegistrationCount
7. Return confirmation
```

---

## ❌ Cancellation & Refunds

### Event Cancellation (by admin)
When an event is cancelled, all registrations are automatically processed:

```csharp
// In CancelEventCommandHandler:
event.Cancel(); // State transition

// For each registration:
foreach (var registration in registrations)
{
    registration.Cancel();
    if (registration.RegistrationType == "Ticket")
    {
        registration.InitiateRefund();
        await _eventBus.Publish(new RefundRequestedEvent(
            registration.MemberId, ticket.PricePaid, "Event cancelled"));
    }
}
```

### Individual Registration Cancellation
- Checked against `CancellationDeadlineHours` (default 48h)
- Before deadline: full refund initiated
- After deadline: no refund (or admin override)

---

## 🔄 Event Series

Recurring events are managed through `EventSeries`:

```csharp
public class EventSeries : TenantEntity
{
    public string Title { get; private set; }
    public EventType EventType { get; private set; }
    public int MaxOccurrences { get; private set; }       // 1-52
    public string? RecurrencePattern { get; private set; } // JSON pattern definition
    public bool IsActive { get; private set; }
}
```

**RecurrencePattern JSON:**
```json
{
  "frequency": "weekly",        // weekly, fortnightly, monthly
  "dayOfWeek": "Saturday",
  "startTime": "14:00",
  "endTime": "17:00",
  "startDate": "2024-04-01",
  "venueId": "...",
  "capacity": 100
}
```

**Rules:**
- Maximum 52 occurrences (1 year of weekly events)
- Each occurrence is a separate `Event` entity (independently manageable)
- Series can be deactivated to stop generating new occurrences
- Individual events in a series can be cancelled without affecting others

---

## 🌐 API Endpoints

| Method | Route | Permission | Purpose |
|--------|-------|-----------|---------|
| GET | /api/v1/events | ViewMembers | List events (filterable) |
| GET | /api/v1/events/{id} | ViewMembers | Get event details |
| POST | /api/v1/events | ManageEvents | Create event |
| PUT | /api/v1/events/{id} | ManageEvents | Update event (Draft only) |
| PUT | /api/v1/events/{id}/status | ManageEvents | Change event status |
| POST | /api/v1/events/{id}/register | BookSessions | Register for event |
| DELETE | /api/v1/events/{id}/register | BookSessions | Cancel registration |
| POST | /api/v1/events/{id}/rsvp | BookSessions | Submit RSVP |
| POST | /api/v1/events/{id}/checkin | ManageEvents | Check in ticket |
| GET | /api/v1/events/{id}/registrations | ManageEvents | List registrations |
| POST | /api/v1/events/series | ManageEvents | Create event series |
| GET | /api/v1/events/series | ManageEvents | List event series |

---

## 🧪 Testing Approach

### Property Tests
```
Property 14: Event Status Transition Validity
  For ANY event in ANY status,
  and FOR ANY attempted transition,
  the transition SHALL succeed ONLY if it matches the allowed transitions map.

Property 15: Registration Count Consistency
  For ANY event, CurrentRegistrationCount SHALL equal the count of
  EventRegistration records where CancelledAt IS NULL for that event.
```

### Unit Tests
- Create event with end before start → throws
- Publish draft event → status = Published
- Complete a draft event → throws (invalid transition)
- Register when at capacity → rejected
- Cancel event → all registrations cancelled, refunds initiated
- Check in ticket twice → throws
- Event series with 53 occurrences → throws (max 52)
- Update published event → throws (only Draft editable)

---

## 🚀 How to Extend

### Adding a new event type:
1. Add value to `EventType` enum
2. No other changes needed — types are just categorization labels

### Adding waitlist for events:
1. Create `EventWaitlist` entity (similar to session waitlist)
2. Add waitlist check in registration handler when at capacity
3. Add promotion logic when a registration is cancelled

### Adding multi-day events:
1. Use `EventSession` entity for individual time slots within an event
2. Each session has its own start/end time and optional capacity
3. Registration can be per-event or per-session
