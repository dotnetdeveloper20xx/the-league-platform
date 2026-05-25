# 10 — Competition & Tournament Management

## 📖 Feature Overview

The Competitions module manages seasons, leagues, tournaments, and cup competitions. It handles team registration, fixture generation (round-robin and knockout), match result recording, automatic standings calculation, and match events (goals, cards, substitutions).

### Key Capabilities
- Season management (time-bounded competition periods)
- 11 competition types (League, Tournament, Cup, Knockout, RoundRobin, etc.)
- Team registration with squad validation (11-30 players, exactly 1 captain)
- Fixture generation algorithms (round-robin and knockout with byes)
- Match result recording with automatic standings recalculation
- Match events (goals, cards, substitutions — max 5 subs per team)
- Match status state machine (Scheduled → Confirmed → InProgress → Completed)

---

## 🏗️ Architecture & Design Decisions

| Decision | Rationale |
|----------|-----------|
| Season as top-level container | Groups competitions by time period (e.g., "2024 Summer Season") |
| Configurable points system | Clubs can set points for win/draw/loss per competition |
| Squad size 11-30 | Minimum for a cricket/football XI, maximum for a large squad |
| Exactly 1 captain per team | Clear leadership; enforced at domain level |
| Denormalized `CompetitionStanding` | Pre-calculated for fast leaderboard queries |
| Match events as separate entity | Flexible — supports any sport's event types |
| Walkover as match status | Common in club sports when teams can't field a side |
| `GoalDifference` calculated on update | Derived field, always consistent with GoalsFor - GoalsAgainst |

---

## 📊 Data Model

### Season
```csharp
public class Season : TenantEntity
{
    public string Name { get; private set; }        // "2024 Summer", max 100 chars
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }   // Must be after StartDate
    public bool IsActive { get; private set; }
}
```

### Competition
```csharp
public class Competition : TenantEntity
{
    public Guid SeasonId { get; private set; }
    public string Name { get; private set; }                    // Max 200 chars
    public CompetitionType CompetitionType { get; private set; }
    public string Status { get; private set; }                  // Draft, Active, Completed
    public int PointsForWin { get; private set; }               // Default: 3
    public int PointsForDraw { get; private set; }              // Default: 1
    public int PointsForLoss { get; private set; }              // Default: 0
    public string? DefaultWalkoverScore { get; private set; }   // e.g., "3-0"

    // Navigation
    public ICollection<CompetitionTeam> Teams { get; private set; }
    public ICollection<Match> Matches { get; private set; }
    public ICollection<CompetitionStanding> Standings { get; private set; }
}
```

### CompetitionType Enum (11 types)
```csharp
public enum CompetitionType
{
    League,         // Round-robin, all teams play each other
    Tournament,     // Multi-format tournament
    Cup,            // Knockout cup competition
    Friendly,       // Non-competitive matches
    Championship,   // Top-tier league
    Qualifier,      // Qualification rounds
    Playoff,        // Post-season playoffs
    RoundRobin,     // Explicit round-robin format
    Knockout,       // Single-elimination
    Ladder,         // Challenge ladder
    TimeTrial       // Individual time-based competition
}
```

---

## 👥 Team Registration

### CompetitionTeam Entity
```csharp
public class CompetitionTeam : TenantEntity
{
    public Guid CompetitionId { get; private set; }
    public string TeamName { get; private set; }          // Max 100 chars
    public Guid? CaptainMemberId { get; private set; }    // Exactly 1 captain required
    public Guid? HomeVenueId { get; private set; }
    public string? HomeVenueName { get; private set; }
    public string? TeamColor { get; private set; }
    public int SquadSize { get; private set; }            // 11-30

    public ICollection<CompetitionParticipant> Participants { get; private set; }
}
```

### Squad Validation Rules
```csharp
public static CompetitionTeam Create(Guid clubId, Guid competitionId, string teamName,
    Guid? captainMemberId, Guid? homeVenueId, string? homeVenueName,
    string? teamColor, int squadSize)
{
    if (teamName.Length > 100)
        throw new ArgumentException("Team name must be at most 100 characters.");
    if (squadSize < 11 || squadSize > 30)
        throw new ArgumentException("Squad size must be between 11 and 30.");

    return new CompetitionTeam { /* ... */ };
}
```

**Registration rules:**
- Squad size: 11-30 players
- Exactly 1 captain per team (validated on registration)
- Captain must be a member of the squad
- Players can only be in one team per competition
- Team name must be unique within a competition

---

## 🗓️ Fixture Generation Algorithms

### Round-Robin (League format)
Every team plays every other team once. Total fixtures = N × (N-1) / 2

```csharp
public List<Match> GenerateRoundRobinFixtures(Competition competition, List<CompetitionTeam> teams)
{
    var fixtures = new List<Match>();
    var n = teams.Count;
    // Total matches: N × (N-1) / 2
    // Example: 8 teams = 8 × 7 / 2 = 28 matches

    for (int i = 0; i < n; i++)
    {
        for (int j = i + 1; j < n; j++)
        {
            var match = Match.Create(
                competition.ClubId,
                competition.Id,
                teams[i].Id,  // Home
                teams[j].Id,  // Away
                CalculateMatchDate(round: j - i),
                roundNumber: j - i);
            fixtures.Add(match);
        }
    }
    return fixtures;
}
```

**Round assignment:** Uses the circle method — teams are arranged in a circle, one team is fixed, others rotate. Each rotation produces one round.

### Knockout (Single Elimination)
Teams are eliminated after one loss. Byes are assigned when team count isn't a power of 2.

```csharp
public List<Match> GenerateKnockoutFixtures(Competition competition, List<CompetitionTeam> teams)
{
    var fixtures = new List<Match>();
    var n = teams.Count;

    // Calculate byes needed: next power of 2 minus team count
    var nextPowerOf2 = (int)Math.Pow(2, Math.Ceiling(Math.Log2(n)));
    var byeCount = nextPowerOf2 - n;

    // First round: teams without byes play, others get a bye to round 2
    var round1Teams = teams.Skip(byeCount).ToList(); // Teams that play in round 1
    var byeTeams = teams.Take(byeCount).ToList();    // Teams with byes

    // Generate round 1 matches
    for (int i = 0; i < round1Teams.Count; i += 2)
    {
        fixtures.Add(Match.Create(competition.ClubId, competition.Id,
            round1Teams[i].Id, round1Teams[i + 1].Id,
            CalculateMatchDate(round: 1), roundNumber: 1));
    }

    // Subsequent rounds are generated as results come in
    return fixtures;
}
```

**Bye calculation example:**
- 8 teams → 0 byes (8 is 2³), 4 matches in round 1
- 6 teams → 2 byes (next power is 8), 2 matches in round 1, 2 teams skip to round 2
- 5 teams → 3 byes, 1 match in round 1, 3 teams skip to round 2

---

## ⚽ Match Management

### Match Entity
```csharp
public class Match : TenantEntity
{
    public Guid CompetitionId { get; private set; }
    public int? RoundNumber { get; private set; }
    public Guid HomeTeamId { get; private set; }
    public Guid AwayTeamId { get; private set; }
    public Guid? VenueId { get; private set; }
    public string? VenueName { get; private set; }
    public DateTime ScheduledDateTime { get; private set; }
    public MatchStatus Status { get; private set; }       // Scheduled → ... → Completed
    public string? HomeScore { get; private set; }        // String for flexibility (e.g., "245/8")
    public string? AwayScore { get; private set; }
    public MatchResult Result { get; private set; }       // HomeWin, AwayWin, Draw, NotPlayed, etc.

    public ICollection<MatchEvent> Events { get; private set; }
    public ICollection<MatchLineup> Lineups { get; private set; }
}
```

### Match Status State Machine
```
┌───────────┐    Confirm()    ┌───────────┐    Start()    ┌────────────┐
│ Scheduled │────────────────▶│ Confirmed │──────────────▶│ InProgress │
└─────┬─────┘                 └─────┬─────┘               └──────┬─────┘
      │                             │                            │
      │ Cancel()                    │ Postpone()                 │ Complete()
      ▼                             │ Cancel()                   │ Abandon()
┌───────────┐                       ▼                            ▼
│ Cancelled │                ┌───────────┐               ┌───────────┐
└───────────┘                │ Postponed │               │ Completed │
                             └─────┬─────┘               └───────────┘
                                   │                            │
                                   │ Confirm()                  │ (also: Abandoned)
                                   │ Cancel()
                                   ▼
                             ┌───────────┐
                             │ Confirmed │ (rescheduled)
                             └───────────┘
```

**Allowed transitions:**
| From | To |
|------|-----|
| Scheduled | Confirmed, Cancelled |
| Confirmed | InProgress, Postponed, Cancelled |
| InProgress | Completed, Abandoned |
| Postponed | Confirmed (reschedule), Cancelled |
| Completed | — (terminal) |
| Cancelled | — (terminal) |
| Abandoned | — (terminal) |

### Recording a Result
```csharp
public void Complete(string homeScore, string awayScore, MatchResult result)
{
    ValidateTransition(MatchStatus.Completed);
    Status = MatchStatus.Completed;
    HomeScore = homeScore;
    AwayScore = awayScore;
    Result = result;  // HomeWin, AwayWin, Draw
}
```

---

## 📊 Match Events

```csharp
public class MatchEvent : TenantEntity
{
    public Guid MatchId { get; private set; }
    public Guid TeamId { get; private set; }
    public Guid? PlayerId { get; private set; }
    public string EventType { get; private set; }   // Goal, YellowCard, RedCard, Substitution, Wicket, etc.
    public int? Minute { get; private set; }
    public string? Description { get; private set; }
    public DateTime Timestamp { get; private set; }
}
```

**Event types (sport-agnostic):**
- `Goal` / `Wicket` / `Try` — Scoring events
- `YellowCard` / `RedCard` — Disciplinary
- `Substitution` — Player replacement (max 5 per team per match)
- `Injury` — Injury record
- `PenaltyScored` / `PenaltyMissed` — Penalty events

**Substitution limit enforcement:**
```csharp
// In command handler:
var subsForTeam = await _db.MatchEvents
    .Where(e => e.MatchId == matchId && e.TeamId == teamId && e.EventType == "Substitution")
    .CountAsync();

if (subsForTeam >= 5)
    return Result.Failure("Maximum 5 substitutions per team reached.");
```

---

## 📈 Automatic Standings Calculation

When a match result is recorded, standings are recalculated:

```csharp
public class CompetitionStanding : TenantEntity
{
    public Guid CompetitionId { get; private set; }
    public Guid TeamId { get; private set; }
    public int Played { get; private set; }
    public int Won { get; private set; }
    public int Drawn { get; private set; }
    public int Lost { get; private set; }
    public int GoalsFor { get; private set; }
    public int GoalsAgainst { get; private set; }
    public int GoalDifference { get; private set; }  // Calculated: GoalsFor - GoalsAgainst
    public int Points { get; private set; }
    public string? Form { get; private set; }         // Last 5 results: "WWDLW"
}
```

### Recalculation Logic
```csharp
public void Update(int played, int won, int drawn, int lost,
    int goalsFor, int goalsAgainst, int points, string? form)
{
    Played = played;
    Won = won;
    Drawn = drawn;
    Lost = lost;
    GoalsFor = goalsFor;
    GoalsAgainst = goalsAgainst;
    GoalDifference = goalsFor - goalsAgainst;
    Points = points;
    Form = form;  // Last 5: "WWDLW"
}
```

**Points calculation:**
```
HomeWin  → Home team: +PointsForWin,  Away team: +PointsForLoss
AwayWin  → Away team: +PointsForWin,  Home team: +PointsForLoss
Draw     → Both teams: +PointsForDraw
Walkover → Winning team: +PointsForWin, Losing team: +PointsForLoss
```

**Standings sort order:** Points (desc) → GoalDifference (desc) → GoalsFor (desc)

---

## 🌐 API Endpoints

| Method | Route | Permission | Purpose |
|--------|-------|-----------|---------|
| GET | /api/v1/competitions/seasons | ViewMembers | List seasons |
| POST | /api/v1/competitions/seasons | ManageMembers | Create season |
| GET | /api/v1/competitions | ViewMembers | List competitions |
| POST | /api/v1/competitions | ManageMembers | Create competition |
| PUT | /api/v1/competitions/{id}/activate | ManageMembers | Activate competition |
| POST | /api/v1/competitions/{id}/teams | ManageMembers | Register team |
| PUT | /api/v1/competitions/{id}/teams/{teamId}/captain | ManageMembers | Set captain |
| POST | /api/v1/competitions/{id}/fixtures/generate | ManageMembers | Generate fixtures |
| GET | /api/v1/competitions/{id}/matches | ViewMembers | List matches |
| PUT | /api/v1/competitions/matches/{id}/result | ManageMembers | Record result |
| POST | /api/v1/competitions/matches/{id}/events | ManageMembers | Add match event |
| GET | /api/v1/competitions/{id}/standings | ViewMembers | Get standings |

---

## 🧪 Testing Approach

### Property Tests
```
Property 16: Round-Robin Fixture Count
  For ANY competition with N teams in round-robin format,
  the generated fixture count SHALL equal N × (N-1) / 2.

Property 17: Standings Consistency
  For ANY competition, the sum of (Won + Drawn + Lost) for each team
  SHALL equal the team's Played count, and Points SHALL equal
  (Won × PointsForWin) + (Drawn × PointsForDraw) + (Lost × PointsForLoss).
```

### Unit Tests
- Create team with squad size 10 → throws (below minimum 11)
- Create team with squad size 31 → throws (above maximum 30)
- Generate round-robin for 8 teams → 28 fixtures
- Generate knockout for 6 teams → 2 byes assigned
- Record result → standings updated correctly
- 6th substitution → rejected
- Complete a scheduled match → throws (must be Confirmed first)
- Postpone completed match → throws (terminal state)

---

## 🚀 How to Extend

### Adding double round-robin (home and away):
1. Modify `GenerateRoundRobinFixtures` to generate N × (N-1) fixtures (not /2)
2. Each pair plays twice — once as home, once as away
3. Add `IsDoubleRoundRobin` flag to Competition entity

### Adding playoff bracket generation:
1. After league phase completes, take top N teams
2. Seed them into a knockout bracket (1 vs N, 2 vs N-1, etc.)
3. Generate knockout fixtures for the seeded teams

### Adding cricket-specific scoring:
1. Use the string-based `HomeScore`/`AwayScore` fields (e.g., "245/8 (50)")
2. Add cricket-specific match events: Wicket, Over, Boundary, Six
3. Add `Overs`, `RunRate` to a cricket-specific match extension
