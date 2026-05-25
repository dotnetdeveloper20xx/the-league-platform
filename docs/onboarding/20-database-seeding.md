# 20 — Database Seeding & Demo Data

## 📖 Feature Overview

The Database Seeding module provides a deterministic, idempotent seeding system that populates the database with realistic demo data for development, testing, and demonstrations. It follows a strict dependency graph to ensure referential integrity and uses deterministic GUIDs for reproducibility.

### Key Capabilities
- Data dependency graph (Identity → Clubs → Members → ... → Payments)
- Deterministic GUIDs (predictable, reproducible across environments)
- Idempotent seeding (check before insert, safe to run multiple times)
- SeedData static class with generator methods
- DatabaseSeeder orchestrator (runs seeders in dependency order)
- Realistic data volumes (4 clubs, 71 members, 300+ sessions, 400+ payments)
- Status distribution matching real-world patterns
- Seasonal payment patterns and demo credentials

---

## 🏗️ Architecture & Design Decisions

| Decision | Rationale |
|----------|-----------|
| Deterministic GUIDs | Same data every run; tests can reference known IDs |
| Idempotent (check before insert) | Safe to run on existing database; no duplicates |
| Dependency graph ordering | Ensures FK constraints are satisfied |
| Realistic volumes | Meaningful for performance testing and UI demos |
| Status distribution (70/10/10/10) | Mirrors real club data patterns |
| Static SeedData class | Easy to reference in tests; no DI needed |
| Seasonal patterns | Payments cluster around term starts; realistic cash flow |
| Demo credentials documented | Developers can log in immediately |

---

## 📊 Data Dependency Graph

```
┌──────────────┐
│   Identity   │  (Users, Roles)
└──────┬───────┘
       │
       ▼
┌──────────────┐
│    Clubs     │  (4 clubs with settings)
└──────┬───────┘
       │
       ▼
┌──────────────┐
│   Members    │  (71 members across 4 clubs)
└──────┬───────┘
       │
       ├──────────────────┐
       ▼                  ▼
┌──────────────┐   ┌──────────────┐
│ Memberships  │   │  Facilities  │
└──────┬───────┘   └──────┬───────┘
       │                  │
       ▼                  ▼
┌──────────────┐   ┌──────────────┐
│   Sessions   │   │   Bookings   │  (300+ sessions)
└──────┬───────┘   └──────────────┘
       │
       ▼
┌──────────────┐
│    Events    │  (Matches, Tournaments)
└──────┬───────┘
       │
       ▼
┌──────────────┐
│ Competitions │  (Seasons, Fixtures, Standings)
└──────┬───────┘
       │
       ▼
┌──────────────┐
│   Payments   │  (400+ payment records)
└──────────────┘
```

**Seeding order (must be sequential):**
1. Identity (users + roles)
2. Clubs (club entities + settings)
3. Members (linked to users and clubs)
4. Membership Types + Memberships
5. Facilities
6. Sessions + Attendance
7. Events
8. Competitions (seasons, teams, fixtures, standings)
9. Payments + Invoices

---

## 🔑 Deterministic GUIDs

### Pattern: `{prefix}-{index}-{suffix}`
```csharp
public static class SeedGuids
{
    // Pattern: 00000000-0000-0000-0000-{prefix}{index:D4}{suffix}
    // This ensures GUIDs are valid, unique, and predictable

    // Clubs (prefix: "c1ub")
    public static Guid Club1 = Guid.Parse("00000000-0000-0000-0000-c1ub00010000");
    public static Guid Club2 = Guid.Parse("00000000-0000-0000-0000-c1ub00020000");
    public static Guid Club3 = Guid.Parse("00000000-0000-0000-0000-c1ub00030000");
    public static Guid Club4 = Guid.Parse("00000000-0000-0000-0000-c1ub00040000");

    // Members (prefix: "memb")
    public static Guid Member1 = Guid.Parse("00000000-0000-0000-0000-memb00010000");
    public static Guid Member2 = Guid.Parse("00000000-0000-0000-0000-memb00020000");
    // ... up to Member71

    // Users (prefix: "user")
    public static Guid User1 = Guid.Parse("00000000-0000-0000-0000-user00010000");
    public static Guid AdminUser = Guid.Parse("00000000-0000-0000-0000-user00000001");

    // Seasons (prefix: "seas")
    public static Guid Season2024 = Guid.Parse("00000000-0000-0000-0000-seas00010000");

    // Facilities (prefix: "fac1")
    public static Guid Facility1 = Guid.Parse("00000000-0000-0000-0000-fac100010000");

    // Helper method for generating sequential GUIDs
    public static Guid Generate(string prefix, int index)
    {
        // Pad prefix to 4 chars, index to 4 digits
        var paddedPrefix = prefix.PadRight(4, '0')[..4];
        var guidString = $"00000000-0000-0000-0000-{paddedPrefix}{index:D4}0000";
        return Guid.Parse(guidString);
    }
}
```

**Why deterministic GUIDs?**
- Tests can assert against known IDs without querying
- Seed data is identical across all developer machines
- Easy to reference in Postman collections and API tests
- Debugging: recognise seed data by GUID pattern

---

## 🔒 Idempotent Seeding

### Check Before Insert Pattern
```csharp
public abstract class BaseSeeder
{
    protected async Task<bool> SeedIfNotExists<T>(
        DbSet<T> dbSet, Guid id, Func<T> entityFactory) where T : class
    {
        var exists = await dbSet.AnyAsync(e => EF.Property<Guid>(e, "Id") == id);
        if (exists)
        {
            _logger.LogDebug("Entity {Type} with ID {Id} already exists, skipping", typeof(T).Name, id);
            return false;
        }

        var entity = entityFactory();
        await dbSet.AddAsync(entity);
        return true;
    }
}

// Usage in a seeder:
public class ClubSeeder : BaseSeeder
{
    public async Task Seed()
    {
        await SeedIfNotExists(_db.Clubs, SeedGuids.Club1, () => new Club
        {
            Id = SeedGuids.Club1,
            Name = "Riverside Cricket Club",
            Subdomain = "riverside",
            // ...
        });

        await SeedIfNotExists(_db.Clubs, SeedGuids.Club2, () => new Club
        {
            Id = SeedGuids.Club2,
            Name = "Hillside Sports Club",
            Subdomain = "hillside",
            // ...
        });

        await _db.SaveChangesAsync();
    }
}
```

**Idempotency guarantees:**
- Running seeder twice produces the same database state
- No duplicate records created
- No exceptions on re-run
- Safe to include in application startup (development mode)

---

## 🏭 SeedData Static Class

```csharp
public static class SeedData
{
    // ─── Clubs ───────────────────────────────────────────────
    public static List<Club> GetClubs() => new()
    {
        new Club { Id = SeedGuids.Club1, Name = "Riverside Cricket Club", Subdomain = "riverside", IsActive = true },
        new Club { Id = SeedGuids.Club2, Name = "Hillside Sports Club", Subdomain = "hillside", IsActive = true },
        new Club { Id = SeedGuids.Club3, Name = "Valley United CC", Subdomain = "valleyunited", IsActive = true },
        new Club { Id = SeedGuids.Club4, Name = "Oakwood Athletic", Subdomain = "oakwood", IsActive = true },
    };

    // ─── Members (71 total, distributed across clubs) ────────
    public static List<Member> GetMembers() => new()
    {
        // Club 1: Riverside — 25 members
        new Member { Id = SeedGuids.Generate("memb", 1), ClubId = SeedGuids.Club1,
            FirstName = "James", LastName = "Wilson", Email = "james.wilson@example.com",
            Status = MemberStatus.Active, DateOfBirth = new DateTime(1990, 3, 15) },
        new Member { Id = SeedGuids.Generate("memb", 2), ClubId = SeedGuids.Club1,
            FirstName = "Sarah", LastName = "Johnson", Email = "sarah.j@example.com",
            Status = MemberStatus.Active, DateOfBirth = new DateTime(1985, 7, 22) },
        // ... (25 members for Club 1)

        // Club 2: Hillside — 20 members
        // Club 3: Valley United — 16 members
        // Club 4: Oakwood — 10 members
    };

    // ─── Membership Types ────────────────────────────────────
    public static List<MembershipType> GetMembershipTypes() => new()
    {
        new MembershipType { Name = "Adult Annual", Price = 150m, BillingCycle = BillingCycle.Annual },
        new MembershipType { Name = "Junior Annual", Price = 75m, BillingCycle = BillingCycle.Annual, MaxAge = 17 },
        new MembershipType { Name = "Senior Monthly", Price = 15m, BillingCycle = BillingCycle.Monthly, MinAge = 60 },
        new MembershipType { Name = "Family Annual", Price = 350m, BillingCycle = BillingCycle.Annual },
        new MembershipType { Name = "Social Member", Price = 30m, BillingCycle = BillingCycle.Annual },
    };

    // ─── Sessions (300+) ─────────────────────────────────────
    public static List<Session> GenerateSessions(int count = 300)
    {
        var sessions = new List<Session>();
        var random = new Random(42); // Deterministic seed

        for (int i = 0; i < count; i++)
        {
            var clubIndex = random.Next(4);
            var daysAgo = random.Next(365);
            sessions.Add(new Session
            {
                Id = SeedGuids.Generate("sess", i + 1),
                ClubId = SeedGuids.Generate("c1ub", clubIndex + 1),
                Title = GetRandomSessionTitle(random),
                ScheduledDate = DateTime.UtcNow.AddDays(-daysAgo),
                Status = daysAgo > 0 ? SessionStatus.Completed : SessionStatus.Scheduled
            });
        }
        return sessions;
    }

    // ─── Payments (400+) ─────────────────────────────────────
    public static List<Payment> GeneratePayments(int count = 400)
    {
        var payments = new List<Payment>();
        var random = new Random(42);

        for (int i = 0; i < count; i++)
        {
            var monthsAgo = random.Next(12);
            payments.Add(new Payment
            {
                Id = SeedGuids.Generate("paym", i + 1),
                Amount = GetSeasonalAmount(monthsAgo, random),
                Status = GetWeightedStatus(random),
                PaidAt = DateTime.UtcNow.AddMonths(-monthsAgo).AddDays(-random.Next(28))
            });
        }
        return payments;
    }
}
```

---

## 🎯 Status Distribution

### Member Status Distribution (70/10/10/10)
```csharp
private static MemberStatus GetWeightedMemberStatus(Random random)
{
    var roll = random.Next(100);
    return roll switch
    {
        < 70 => MemberStatus.Active,      // 70% Active
        < 80 => MemberStatus.Pending,     // 10% Pending
        < 90 => MemberStatus.Expired,     // 10% Expired
        _    => MemberStatus.Suspended    // 10% Suspended
    };
}
```

**Distribution across 71 members:**
| Status | Percentage | Count |
|--------|-----------|-------|
| Active | 70% | ~50 |
| Pending | 10% | ~7 |
| Expired | 10% | ~7 |
| Suspended | 10% | ~7 |

### Payment Status Distribution
```csharp
private static PaymentStatus GetWeightedPaymentStatus(Random random)
{
    var roll = random.Next(100);
    return roll switch
    {
        < 75 => PaymentStatus.Completed,   // 75% Completed
        < 85 => PaymentStatus.Pending,     // 10% Pending
        < 92 => PaymentStatus.Failed,      //  7% Failed
        < 97 => PaymentStatus.Refunded,    //  5% Refunded
        _    => PaymentStatus.Cancelled    //  3% Cancelled
    };
}
```

---

## 📅 Seasonal Payment Patterns

```csharp
private static decimal GetSeasonalAmount(int monthsAgo, Random random)
{
    var month = DateTime.UtcNow.AddMonths(-monthsAgo).Month;

    // Higher payments in season-start months (April, September)
    var seasonalMultiplier = month switch
    {
        4 => 2.5m,   // April: season start, annual renewals
        9 => 2.0m,   // September: autumn term, new joiners
        1 => 1.5m,   // January: New Year resolutions
        _ => 1.0m    // Other months: baseline
    };

    var baseAmount = random.Next(15, 200);
    return Math.Round(baseAmount * seasonalMultiplier, 2);
}
```

**Payment volume by month (approximate):**
```
Jan ████████████░░░░ (150% — New Year)
Feb ████████░░░░░░░░ (100% — baseline)
Mar ████████░░░░░░░░ (100% — baseline)
Apr ████████████████████ (250% — season start)
May ████████░░░░░░░░ (100% — baseline)
Jun ████████░░░░░░░░ (100% — baseline)
Jul ████████░░░░░░░░ (100% — baseline)
Aug ████████░░░░░░░░ (100% — baseline)
Sep ████████████████ (200% — autumn term)
Oct ████████░░░░░░░░ (100% — baseline)
Nov ████████░░░░░░░░ (100% — baseline)
Dec ████████░░░░░░░░ (100% — baseline)
```

---

## 🎭 DatabaseSeeder Orchestrator

```csharp
public class DatabaseSeeder
{
    private readonly IServiceProvider _services;
    private readonly ILogger<DatabaseSeeder> _logger;

    public async Task SeedAsync()
    {
        _logger.LogInformation("Starting database seeding...");

        // Execute seeders in dependency order
        var seeders = new List<ISeeder>
        {
            _services.GetRequiredService<IdentitySeeder>(),      // 1. Users & Roles
            _services.GetRequiredService<ClubSeeder>(),          // 2. Clubs
            _services.GetRequiredService<MemberSeeder>(),        // 3. Members
            _services.GetRequiredService<MembershipSeeder>(),    // 4. Memberships
            _services.GetRequiredService<FacilitySeeder>(),      // 5. Facilities
            _services.GetRequiredService<SessionSeeder>(),       // 6. Sessions
            _services.GetRequiredService<EventSeeder>(),         // 7. Events
            _services.GetRequiredService<CompetitionSeeder>(),   // 8. Competitions
            _services.GetRequiredService<PaymentSeeder>(),       // 9. Payments
        };

        foreach (var seeder in seeders)
        {
            var name = seeder.GetType().Name;
            _logger.LogInformation("Running {Seeder}...", name);

            try
            {
                await seeder.SeedAsync();
                _logger.LogInformation("{Seeder} completed successfully", name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Seeder} failed", name);
                throw; // Fail fast — downstream seeders depend on this
            }
        }

        _logger.LogInformation("Database seeding completed. Total time: {Elapsed}");
    }
}
```

### Registration in Program.cs
```csharp
// Only seed in Development environment
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    await seeder.SeedAsync();
}
```

---

## 📊 Realistic Data Volumes

| Entity | Count | Distribution |
|--------|-------|-------------|
| Clubs | 4 | Riverside, Hillside, Valley United, Oakwood |
| Users | 75 | 71 members + 4 club admins |
| Members | 71 | 25 + 20 + 16 + 10 across clubs |
| Membership Types | 5 | Per club (20 total) |
| Memberships | 71 | One per member |
| Facilities | 12 | 3 per club |
| Sessions | 300+ | Spread over 12 months |
| Session Attendance | 1500+ | ~5 attendees per session |
| Events | 40 | 10 per club |
| Competitions | 8 | 2 per club |
| Matches | 100+ | From fixture generation |
| Payments | 400+ | Seasonal distribution |
| Invoices | 200+ | Linked to payments |

---

## 🔐 Demo Credentials

```
┌─────────────────────────────────────────────────────────────────┐
│ Role          │ Email                    │ Password    │ Club     │
├───────────────┼──────────────────────────┼─────────────┼──────────┤
│ Platform Admin│ admin@theleague.dev      │ Admin123!   │ (all)    │
│ Club Admin    │ admin@riverside.dev      │ Club123!    │ Riverside│
│ Club Admin    │ admin@hillside.dev       │ Club123!    │ Hillside │
│ Club Admin    │ admin@valleyunited.dev   │ Club123!    │ Valley   │
│ Club Admin    │ admin@oakwood.dev        │ Club123!    │ Oakwood  │
│ Coach         │ coach@riverside.dev      │ Coach123!   │ Riverside│
│ Member        │ james.wilson@example.com │ Member123!  │ Riverside│
│ Member        │ sarah.j@example.com      │ Member123!  │ Riverside│
└───────────────┴──────────────────────────┴─────────────┴──────────┘
```

**Note:** These credentials are for development only. The seeder only runs in `Development` environment.

---

## 🌐 API Endpoints

| Method | Route | Permission | Purpose |
|--------|-------|-----------|---------|
| POST | /api/v1/admin/seed | PlatformAdmin | Trigger manual re-seed |
| POST | /api/v1/admin/seed/reset | PlatformAdmin | Drop and re-seed all data |
| GET | /api/v1/admin/seed/status | PlatformAdmin | Check seeding status |

---

## 🧪 Testing Approach

### Property Tests
```
Property 39: Deterministic GUID Uniqueness
  For ANY two calls to SeedGuids.Generate with different (prefix, index) pairs,
  the resulting GUIDs SHALL be different.

Property 40: Idempotent Seeding
  For ANY seeder run N times (N >= 1),
  the resulting database state SHALL be identical to running it once.

Property 41: Dependency Order Integrity
  For ANY seeded entity with a foreign key,
  the referenced entity SHALL have been seeded in a prior step.
```

### Unit Tests
- Run seeder once → all entities created
- Run seeder twice → no duplicates, same count
- Generate GUID for ("memb", 1) → always same GUID
- Generate GUID for ("memb", 1) vs ("memb", 2) → different GUIDs
- Member status distribution over 100 members → ~70% Active
- Payment seasonal pattern in April → higher amounts
- Seeder with missing dependency → fails fast with clear error
- Demo credentials → can authenticate successfully

---

## 🚀 How to Extend

### Adding a new seeder:
1. Create class implementing `ISeeder`
2. Add to `DatabaseSeeder` list in correct dependency position
3. Use `SeedGuids.Generate()` for deterministic IDs
4. Use `SeedIfNotExists()` for idempotency
5. Follow status distribution patterns for realistic data

### Adding performance test data (large volumes):
1. Create `PerformanceSeeder` with configurable multiplier
2. Generate 10,000+ members, 50,000+ sessions
3. Use bulk insert (`AddRangeAsync`) for performance
4. Only run when `SEED_PERFORMANCE=true` environment variable is set
