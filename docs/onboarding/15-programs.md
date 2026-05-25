# 15 — Programs & Certificates

## 📖 Feature Overview

The Programs module manages structured coaching programs (courses, camps, academies) with session scheduling, enrollment with capacity limits, attendance tracking, and certificate issuance upon completion. It bridges the gap between ad-hoc training sessions and formal development pathways.

### Key Capabilities
- Program creation with type and skill level classification
- Session scheduling with instructor assignment
- Enrollment with capacity check and automatic waitlist (max 50)
- Attendance tracking per session per participant
- Completion rate calculation (attended sessions / total sessions)
- Certificate issuance (≥80% attendance AND program ended)
- Certificate number generation with verification support

---

## 🏗️ Architecture & Design Decisions

| Decision | Rationale |
|----------|-----------|
| Program as container for sessions | Logical grouping; one program = many sessions over time |
| Capacity + waitlist (max 50) | Prevents overcrowding; waitlist caps prevent infinite queues |
| 80% attendance threshold | Industry standard for course completion; allows some absence |
| Certificate as separate entity | Independently verifiable; can be revoked if needed |
| Instructor per session (not program) | Flexibility: different coaches for different sessions |
| Completion rate as calculated value | Always accurate; derived from attendance records |
| Skill level on program | Helps members find appropriate programs |

---

## 📊 Data Model

### Program Entity
```csharp
public class Program : TenantEntity
{
    public string Name { get; private set; }              // "Junior Cricket Academy"
    public string? Description { get; private set; }
    public ProgramType ProgramType { get; private set; }
    public SkillLevel SkillLevel { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public int MaxCapacity { get; private set; }          // Max enrollments
    public int MaxWaitlist { get; private set; }          // Max 50
    public decimal? Fee { get; private set; }             // Program fee (null = free)
    public ProgramStatus Status { get; private set; }
    public int MinAttendancePercent { get; private set; } // Default: 80

    public ICollection<ProgramSession> Sessions { get; private set; }
    public ICollection<ProgramEnrollment> Enrollments { get; private set; }
}

public enum ProgramType
{
    Course,         // Multi-week structured course
    Camp,           // Intensive short-duration (e.g., holiday camp)
    Academy,        // Long-term development program
    Workshop,       // Single or few sessions, focused topic
    Masterclass,    // Expert-led advanced session
    Assessment      // Skill assessment/grading program
}

public enum SkillLevel
{
    Beginner,       // New to the sport
    Intermediate,   // Basic skills acquired
    Advanced,       // Competent player
    Elite,          // High-performance/representative level
    AllLevels       // Open to everyone
}

public enum ProgramStatus
{
    Draft,          // Being set up
    Published,      // Open for enrollment
    InProgress,     // Started, sessions underway
    Completed,      // All sessions done
    Cancelled       // Program cancelled
}
```

### ProgramSession Entity
```csharp
public class ProgramSession : TenantEntity
{
    public Guid ProgramId { get; private set; }
    public int SessionNumber { get; private set; }        // 1, 2, 3... (sequential)
    public string Title { get; private set; }             // "Week 3: Bowling Technique"
    public DateTime ScheduledDateTime { get; private set; }
    public int DurationMinutes { get; private set; }
    public Guid? InstructorMemberId { get; private set; } // Coach assigned to this session
    public string? InstructorName { get; private set; }
    public Guid? FacilityId { get; private set; }         // Where the session takes place
    public string? Notes { get; private set; }            // Session plan/objectives
    public bool IsCancelled { get; private set; }

    public ICollection<ProgramAttendance> Attendance { get; private set; }
}
```

### ProgramEnrollment Entity
```csharp
public class ProgramEnrollment : TenantEntity
{
    public Guid ProgramId { get; private set; }
    public Guid MemberId { get; private set; }
    public DateTime EnrolledAt { get; private set; }
    public EnrollmentStatus Status { get; private set; }
    public int? WaitlistPosition { get; private set; }    // null if enrolled (not waitlisted)
    public decimal? FeePaid { get; private set; }
    public Guid? PaymentId { get; private set; }
}

public enum EnrollmentStatus
{
    Enrolled,       // Actively participating
    Waitlisted,     // On waitlist, awaiting spot
    Completed,      // Finished program (certificate eligible)
    Withdrawn,      // Voluntarily left
    Removed         // Removed by admin
}
```

### Enrollment with Capacity Check
```csharp
public static ProgramEnrollment Create(Guid clubId, Program program, Guid memberId)
{
    var currentEnrolled = program.Enrollments
        .Count(e => e.Status == EnrollmentStatus.Enrolled);
    var currentWaitlisted = program.Enrollments
        .Count(e => e.Status == EnrollmentStatus.Waitlisted);

    if (currentEnrolled >= program.MaxCapacity)
    {
        // Check waitlist capacity
        if (currentWaitlisted >= program.MaxWaitlist)
            throw new InvalidOperationException(
                $"Program is full and waitlist is at maximum capacity ({program.MaxWaitlist}).");

        // Add to waitlist
        return new ProgramEnrollment
        {
            ProgramId = program.Id,
            MemberId = memberId,
            EnrolledAt = DateTime.UtcNow,
            Status = EnrollmentStatus.Waitlisted,
            WaitlistPosition = currentWaitlisted + 1
        };
    }

    // Direct enrollment
    return new ProgramEnrollment
    {
        ProgramId = program.Id,
        MemberId = memberId,
        EnrolledAt = DateTime.UtcNow,
        Status = EnrollmentStatus.Enrolled,
        WaitlistPosition = null
    };
}
```

---

## 📋 Attendance Tracking

### ProgramAttendance Entity
```csharp
public class ProgramAttendance : TenantEntity
{
    public Guid ProgramSessionId { get; private set; }
    public Guid MemberId { get; private set; }
    public AttendanceStatus Status { get; private set; }
    public DateTime? MarkedAt { get; private set; }       // When attendance was recorded
    public string? MarkedBy { get; private set; }         // Instructor who marked it
}

public enum AttendanceStatus
{
    Present,        // Attended the session
    Absent,         // Did not attend
    Late,           // Arrived late (still counts as attended)
    Excused         // Absent with valid reason (counts toward total but not attended)
}
```

### Completion Rate Calculation
```csharp
public class CompletionRateCalculator
{
    public decimal Calculate(Guid programId, Guid memberId,
        List<ProgramSession> sessions, List<ProgramAttendance> attendance)
    {
        // Only count non-cancelled sessions
        var totalSessions = sessions.Count(s => !s.IsCancelled);

        if (totalSessions == 0)
            return 0m;

        // Present and Late count as attended
        var attendedSessions = attendance.Count(a =>
            a.MemberId == memberId &&
            (a.Status == AttendanceStatus.Present || a.Status == AttendanceStatus.Late));

        return Math.Round((decimal)attendedSessions / totalSessions * 100, 1);
    }
}
```

**Completion rate rules:**
- `Present` → counts as attended ✓
- `Late` → counts as attended ✓
- `Absent` → does NOT count as attended ✗
- `Excused` → does NOT count as attended ✗ (but session still counts in total)
- Cancelled sessions are excluded from the total

---

## 🎓 Certificate Issuance

### MemberCertificate Entity
```csharp
public class MemberCertificate : TenantEntity
{
    public Guid MemberId { get; private set; }
    public Guid ProgramId { get; private set; }
    public string CertificateNumber { get; private set; } // "CERT-2024-ABC123"
    public string ProgramName { get; private set; }       // Denormalized for display
    public DateTime IssuedDate { get; private set; }
    public decimal CompletionRate { get; private set; }   // Actual % achieved
    public SkillLevel SkillLevel { get; private set; }    // From program
    public string? InstructorName { get; private set; }   // Primary instructor
    public bool IsRevoked { get; private set; }
    public string? RevokedReason { get; private set; }
}
```

### Certificate Eligibility Check
```csharp
public class CertificateEligibilityService
{
    public CertificateEligibility Check(Program program, Guid memberId,
        decimal completionRate)
    {
        // Rule 1: Program must have ended
        if (program.Status != ProgramStatus.Completed)
            return CertificateEligibility.NotEligible("Program has not ended yet.");

        // Rule 2: Member must be enrolled (not waitlisted/withdrawn)
        var enrollment = program.Enrollments
            .FirstOrDefault(e => e.MemberId == memberId);
        if (enrollment == null || enrollment.Status != EnrollmentStatus.Completed)
            return CertificateEligibility.NotEligible("Member is not enrolled.");

        // Rule 3: Attendance must be >= minimum (default 80%)
        if (completionRate < program.MinAttendancePercent)
            return CertificateEligibility.NotEligible(
                $"Attendance {completionRate}% is below required {program.MinAttendancePercent}%.");

        return CertificateEligibility.Eligible();
    }
}
```

### Certificate Number Generation
```csharp
public class CertificateNumberGenerator
{
    public string Generate(Guid clubId)
    {
        // Format: CERT-{YEAR}-{RANDOM6}
        // Example: CERT-2024-A7K9M2
        var year = DateTime.UtcNow.Year;
        var random = GenerateAlphanumeric(6); // A-Z, 0-9
        return $"CERT-{year}-{random}";
    }

    private string GenerateAlphanumeric(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Range(0, length)
            .Select(_ => chars[random.Next(chars.Length)])
            .ToArray());
    }
}
```

---

## 🔄 Program Lifecycle Flow

```
┌───────┐   Publish()   ┌───────────┐   Start()   ┌────────────┐   End()   ┌───────────┐
│ Draft │──────────────▶│ Published │────────────▶│ InProgress │─────────▶│ Completed │
└───────┘               └─────┬─────┘             └────────────┘          └─────┬─────┘
                              │                                                  │
                              │ Cancel()                                          │ Issue
                              ▼                                                  │ Certificates
                        ┌───────────┐                                            ▼
                        │ Cancelled │                                    ┌──────────────┐
                        └───────────┘                                    │ Certificates │
                                                                         │   Issued     │
                                                                         └──────────────┘
```

**Post-completion flow:**
1. Program status set to `Completed`
2. All enrolled members' enrollment status → `Completed`
3. Calculate completion rate for each member
4. For members with ≥80% attendance → issue certificate
5. Publish `ProgramCompletedEvent` for analytics

---

## 🌐 API Endpoints

| Method | Route | Permission | Purpose |
|--------|-------|-----------|---------|
| GET | /api/v1/programs | ViewMembers | List programs |
| POST | /api/v1/programs | ManageMembers | Create program |
| PUT | /api/v1/programs/{id} | ManageMembers | Update program |
| PUT | /api/v1/programs/{id}/publish | ManageMembers | Publish program |
| POST | /api/v1/programs/{id}/sessions | ManageMembers | Add session |
| POST | /api/v1/programs/{id}/enroll | ViewMembers | Enroll member |
| PUT | /api/v1/programs/{id}/enrollments/{memberId}/withdraw | ViewMembers | Withdraw |
| POST | /api/v1/programs/sessions/{id}/attendance | ManageMembers | Mark attendance |
| GET | /api/v1/programs/{id}/completion-rates | ManageMembers | Get completion rates |
| POST | /api/v1/programs/{id}/certificates/issue | ManageMembers | Issue certificates |
| GET | /api/v1/certificates/{number}/verify | Public | Verify certificate |

---

## 🧪 Testing Approach

### Property Tests
```
Property 24: Capacity Enforcement
  For ANY program, the count of Enrolled members
  SHALL never exceed MaxCapacity.

Property 25: Waitlist Cap
  For ANY program, the count of Waitlisted members
  SHALL never exceed MaxWaitlist (50).

Property 26: Certificate Eligibility
  For ANY member with completion rate < MinAttendancePercent,
  certificate issuance SHALL be rejected.
```

### Unit Tests
- Enroll when capacity available → status = Enrolled
- Enroll when at capacity → status = Waitlisted
- Enroll when waitlist full (50) → throws
- Calculate completion: 8/10 sessions attended → 80%
- Calculate completion: 7/10 sessions attended → 70%
- Issue certificate at 80% attendance → success
- Issue certificate at 79% attendance → rejected
- Issue certificate before program ends → rejected
- Cancel session → excluded from completion calculation
- Late attendance → counts as attended

---

## 🚀 How to Extend

### Adding prerequisite programs:
1. Add `PrerequisiteProgramId` to Program entity
2. Validate member has completed prerequisite before enrollment
3. Check certificate exists for prerequisite program

### Adding skill assessments within programs:
1. Create `ProgramAssessment` entity (linked to session)
2. Record scores/grades per member per assessment
3. Include assessment results on certificate
