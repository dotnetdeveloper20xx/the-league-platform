# 18 — Analytics & Churn Prediction

## 📖 Feature Overview

The Analytics module captures weekly snapshots of club health metrics, calculates a composite health score, predicts member churn risk, forecasts revenue, and provides platform-wide benchmarking. It helps club administrators make data-driven decisions about engagement, retention, and growth.

### Key Capabilities
- Weekly club analytics snapshots (24-month retention)
- Health score calculation (weighted average of 4 dimensions)
- Churn prediction (flags at-risk members based on behaviour changes)
- Member engagement metrics tracking
- Revenue forecasting (next 3 months)
- Platform benchmarking (anonymised averages across all clubs)
- Insufficient data handling (minimum 30 days before predictions)

---

## 🏗️ Architecture & Design Decisions

| Decision | Rationale |
|----------|-----------|
| Weekly snapshots (not real-time) | Reduces compute cost; trends are weekly anyway |
| 24-month retention | Enough for year-over-year comparison; manageable storage |
| Weighted health score (25% each) | Equal importance to growth, payments, attendance, engagement |
| 90-day window for churn detection | Balances sensitivity with noise reduction |
| 50% drop threshold for churn flags | Significant enough to indicate real disengagement |
| 30-day minimum data requirement | Prevents false predictions from insufficient history |
| Anonymised benchmarking | Privacy-preserving; clubs see averages, not competitors |

---

## 📊 Data Model

### ClubAnalyticsSnapshot
```csharp
public class ClubAnalyticsSnapshot : TenantEntity
{
    public DateTime SnapshotDate { get; private set; }    // Weekly capture date
    public int TotalMembers { get; private set; }
    public int ActiveMembers { get; private set; }
    public int NewMembersThisPeriod { get; private set; }
    public int ChurnedMembersThisPeriod { get; private set; }
    public decimal MemberGrowthRate { get; private set; } // % change from previous period
    public decimal TotalRevenue { get; private set; }     // Revenue this period
    public decimal OutstandingPayments { get; private set; }
    public decimal PaymentCollectionRate { get; private set; } // % of invoices paid on time
    public decimal AverageAttendanceRate { get; private set; } // % of booked sessions attended
    public int TotalSessionsHeld { get; private set; }
    public int TotalEventsHeld { get; private set; }
    public decimal EngagementScore { get; private set; }  // 0-100 composite
    public decimal HealthScore { get; private set; }      // 0-100 overall health
    public DateTime RetentionExpiryDate { get; private set; } // SnapshotDate + 24 months
}
```

### Snapshot Capture (Weekly Background Job)
```csharp
public class WeeklySnapshotJob : IScheduledJob
{
    public async Task Execute()
    {
        var clubs = await _db.Clubs.Where(c => c.IsActive).ToListAsync();

        foreach (var club in clubs)
        {
            var snapshot = await BuildSnapshot(club.Id);
            snapshot.RetentionExpiryDate = snapshot.SnapshotDate.AddMonths(24);
            await _db.ClubAnalyticsSnapshots.AddAsync(snapshot);
        }

        // Purge expired snapshots (older than 24 months)
        var expired = await _db.ClubAnalyticsSnapshots
            .Where(s => s.RetentionExpiryDate < DateTime.UtcNow)
            .ToListAsync();
        _db.ClubAnalyticsSnapshots.RemoveRange(expired);

        await _db.SaveChangesAsync();
    }
}
```

---

## 🏥 Health Score Calculator

### HealthScoreCalculator
```csharp
public class HealthScoreCalculator
{
    // Weights: 25% each dimension
    private const decimal GrowthWeight = 0.25m;
    private const decimal PaymentWeight = 0.25m;
    private const decimal AttendanceWeight = 0.25m;
    private const decimal EngagementWeight = 0.25m;

    public decimal Calculate(ClubHealthMetrics metrics)
    {
        // Each dimension scored 0-100, then weighted
        var growthScore = CalculateGrowthScore(metrics);
        var paymentScore = CalculatePaymentScore(metrics);
        var attendanceScore = CalculateAttendanceScore(metrics);
        var engagementScore = CalculateEngagementScore(metrics);

        var healthScore = (growthScore * GrowthWeight)
                        + (paymentScore * PaymentWeight)
                        + (attendanceScore * AttendanceWeight)
                        + (engagementScore * EngagementWeight);

        return Math.Round(Math.Clamp(healthScore, 0, 100), 1);
    }

    private decimal CalculateGrowthScore(ClubHealthMetrics m)
    {
        // Positive growth = higher score
        // 0% growth = 50, +10% = 100, -10% = 0
        return Math.Clamp(50 + (m.MemberGrowthRate * 5), 0, 100);
    }

    private decimal CalculatePaymentScore(ClubHealthMetrics m)
    {
        // Payment collection rate directly maps to score
        // 100% collection = 100, 0% = 0
        return Math.Clamp(m.PaymentCollectionRate, 0, 100);
    }

    private decimal CalculateAttendanceScore(ClubHealthMetrics m)
    {
        // Average attendance rate maps to score
        // 100% attendance = 100, 0% = 0
        return Math.Clamp(m.AverageAttendanceRate, 0, 100);
    }

    private decimal CalculateEngagementScore(ClubHealthMetrics m)
    {
        // Composite of login frequency, feature usage, event participation
        return Math.Clamp(m.EngagementScore, 0, 100);
    }
}
```

**Health score interpretation:**
| Score Range | Status | Meaning |
|-------------|--------|---------|
| 80-100 | Excellent | Club is thriving |
| 60-79 | Good | Healthy with minor areas to improve |
| 40-59 | Fair | Needs attention in some areas |
| 20-39 | Poor | Significant issues, intervention needed |
| 0-19 | Critical | At risk of churn/closure |

---

## 🔮 Churn Prediction

### ChurnPredictor
```csharp
public class ChurnPredictor
{
    private const int WindowDays = 90;
    private const decimal DropThreshold = 0.50m; // 50% drop triggers flag

    public async Task<List<ChurnPrediction>> PredictAtRiskMembers(Guid clubId)
    {
        var predictions = new List<ChurnPrediction>();
        var members = await GetActiveMembers(clubId);

        foreach (var member in members)
        {
            var dataAge = (DateTime.UtcNow - member.JoinedDate).TotalDays;

            // Insufficient data check: need at least 30 days of history
            if (dataAge < 30)
                continue;

            var riskFactors = new List<string>();
            var riskScore = 0m;

            // Factor 1: Attendance drop ≥50% in 90-day window
            var attendanceDrop = await CalculateAttendanceDrop(member.Id, WindowDays);
            if (attendanceDrop >= DropThreshold)
            {
                riskFactors.Add($"Attendance dropped {attendanceDrop:P0} in last {WindowDays} days");
                riskScore += 35m;
            }

            // Factor 2: ≥2 missed payments in 90-day window
            var missedPayments = await CountMissedPayments(member.Id, WindowDays);
            if (missedPayments >= 2)
            {
                riskFactors.Add($"{missedPayments} missed payments in last {WindowDays} days");
                riskScore += 35m;
            }

            // Factor 3: Login frequency drop ≥50% in 90-day window
            var loginDrop = await CalculateLoginDrop(member.Id, WindowDays);
            if (loginDrop >= DropThreshold)
            {
                riskFactors.Add($"Login frequency dropped {loginDrop:P0} in last {WindowDays} days");
                riskScore += 30m;
            }

            if (riskFactors.Any())
            {
                predictions.Add(new ChurnPrediction
                {
                    MemberId = member.Id,
                    RiskScore = Math.Min(riskScore, 100),
                    RiskFactors = riskFactors,
                    PredictedAt = DateTime.UtcNow
                });
            }
        }

        return predictions.OrderByDescending(p => p.RiskScore).ToList();
    }
}
```

### ChurnPrediction Entity
```csharp
public class ChurnPrediction : TenantEntity
{
    public Guid MemberId { get; private set; }
    public decimal RiskScore { get; private set; }        // 0-100
    public List<string> RiskFactors { get; private set; }
    public DateTime PredictedAt { get; private set; }
    public ChurnRiskLevel RiskLevel { get; private set; } // Low, Medium, High, Critical
    public bool IsResolved { get; private set; }          // Admin marked as addressed
}

public enum ChurnRiskLevel
{
    Low,        // Score 1-25: Minor concern
    Medium,     // Score 26-50: Worth monitoring
    High,       // Score 51-75: Needs intervention
    Critical    // Score 76-100: Likely to leave soon
}
```

### Attendance Drop Calculation
```csharp
private async Task<decimal> CalculateAttendanceDrop(Guid memberId, int windowDays)
{
    var now = DateTime.UtcNow;
    var windowStart = now.AddDays(-windowDays);
    var previousWindowStart = windowStart.AddDays(-windowDays);

    // Current window attendance
    var currentAttendance = await _db.SessionAttendances
        .CountAsync(a => a.MemberId == memberId
            && a.SessionDate >= windowStart && a.SessionDate <= now
            && a.Status == AttendanceStatus.Present);

    // Previous window attendance (baseline)
    var previousAttendance = await _db.SessionAttendances
        .CountAsync(a => a.MemberId == memberId
            && a.SessionDate >= previousWindowStart && a.SessionDate < windowStart
            && a.Status == AttendanceStatus.Present);

    if (previousAttendance == 0)
        return 0; // No baseline to compare

    var drop = 1m - ((decimal)currentAttendance / previousAttendance);
    return Math.Max(0, drop); // Only positive drops (not improvements)
}
```

---

## 📈 Member Engagement Metrics

### MemberEngagement Entity
```csharp
public class MemberEngagement : TenantEntity
{
    public Guid MemberId { get; private set; }
    public DateTime PeriodStart { get; private set; }
    public DateTime PeriodEnd { get; private set; }
    public int LoginCount { get; private set; }
    public int SessionsAttended { get; private set; }
    public int EventsAttended { get; private set; }
    public int PaymentsMade { get; private set; }
    public int BookingsMade { get; private set; }
    public int MessagesRead { get; private set; }
    public DateTime? LastActiveDate { get; private set; }
    public decimal EngagementScore { get; private set; }  // 0-100
}
```

**Engagement score formula:**
```
EngagementScore = (LoginFrequency × 0.2)
                + (SessionAttendance × 0.3)
                + (EventParticipation × 0.2)
                + (PaymentTimeliness × 0.15)
                + (FeatureUsage × 0.15)
```

---

## 💰 Revenue Forecaster

### RevenueForecaster
```csharp
public class RevenueForecaster
{
    public async Task<RevenueForecast> Forecast(Guid clubId, int monthsAhead = 3)
    {
        // Get last 6 months of revenue data
        var historicalRevenue = await GetMonthlyRevenue(clubId, monthsBack: 6);

        if (historicalRevenue.Count < 3)
            return RevenueForecast.InsufficientData();

        // Simple linear regression for trend
        var trend = CalculateLinearTrend(historicalRevenue);

        var forecasts = new List<MonthlyForecast>();
        for (int i = 1; i <= monthsAhead; i++)
        {
            var forecastMonth = DateTime.UtcNow.AddMonths(i);
            var predictedRevenue = trend.Intercept + (trend.Slope * (historicalRevenue.Count + i));

            // Factor in known recurring revenue (active memberships)
            var recurringRevenue = await CalculateRecurringRevenue(clubId, forecastMonth);

            forecasts.Add(new MonthlyForecast
            {
                Month = forecastMonth,
                PredictedRevenue = Math.Max(0, predictedRevenue),
                RecurringRevenue = recurringRevenue,
                Confidence = CalculateConfidence(historicalRevenue.Count, i)
            });
        }

        return new RevenueForecast(forecasts);
    }

    private decimal CalculateConfidence(int dataPoints, int monthsAhead)
    {
        // Confidence decreases with fewer data points and further predictions
        var baseConfidence = Math.Min(dataPoints * 10, 80); // Max 80% from data
        var decayPerMonth = 10; // Lose 10% confidence per month ahead
        return Math.Max(20, baseConfidence - (monthsAhead * decayPerMonth));
    }
}
```

---

## 🏆 Platform Benchmarking

```csharp
public class PlatformBenchmarkService
{
    public async Task<BenchmarkResult> GetBenchmarks(Guid clubId)
    {
        // Get anonymised averages across all clubs on the same tier
        var clubTier = await GetClubTier(clubId);
        var peerClubs = await _db.ClubAnalyticsSnapshots
            .Where(s => s.SnapshotDate == GetLatestSnapshotDate())
            .ToListAsync();

        return new BenchmarkResult
        {
            ClubHealthScore = await GetClubHealthScore(clubId),
            PlatformAverageHealthScore = peerClubs.Average(c => c.HealthScore),
            ClubAttendanceRate = await GetClubAttendanceRate(clubId),
            PlatformAverageAttendanceRate = peerClubs.Average(c => c.AverageAttendanceRate),
            ClubPaymentRate = await GetClubPaymentRate(clubId),
            PlatformAveragePaymentRate = peerClubs.Average(c => c.PaymentCollectionRate),
            ClubGrowthRate = await GetClubGrowthRate(clubId),
            PlatformAverageGrowthRate = peerClubs.Average(c => c.MemberGrowthRate),
            TotalClubsInBenchmark = peerClubs.Count
        };
    }
}
```

**Benchmarking rules:**
- All data is anonymised (no club names or identifiers exposed)
- Minimum 5 clubs required for benchmarking (prevents identification)
- Clubs only see their own data vs. platform averages
- Benchmarks update weekly (aligned with snapshot schedule)

---

## ⚠️ Insufficient Data Handling

```csharp
public class DataSufficiencyChecker
{
    private const int MinimumDaysForPrediction = 30;
    private const int MinimumSnapshotsForTrend = 4;  // 4 weeks

    public DataSufficiency Check(Guid clubId, DateTime clubCreatedDate)
    {
        var daysSinceCreation = (DateTime.UtcNow - clubCreatedDate).TotalDays;

        if (daysSinceCreation < MinimumDaysForPrediction)
        {
            return new DataSufficiency
            {
                IsSufficient = false,
                Reason = $"Need {MinimumDaysForPrediction} days of data. " +
                         $"Currently have {(int)daysSinceCreation} days.",
                AvailableDate = clubCreatedDate.AddDays(MinimumDaysForPrediction)
            };
        }

        return new DataSufficiency { IsSufficient = true };
    }
}
```

**When data is insufficient:**
- Health score: displays "Collecting data..." instead of a number
- Churn prediction: skips member (no false positives)
- Revenue forecast: returns `InsufficientData` result
- Benchmarking: excluded from platform averages

---

## 🌐 API Endpoints

| Method | Route | Permission | Purpose |
|--------|-------|-----------|---------|
| GET | /api/v1/analytics/health-score | ManageMembers | Get club health score |
| GET | /api/v1/analytics/snapshots | ManageMembers | List historical snapshots |
| GET | /api/v1/analytics/churn-predictions | ManageMembers | Get at-risk members |
| PUT | /api/v1/analytics/churn/{id}/resolve | ManageMembers | Mark churn as resolved |
| GET | /api/v1/analytics/engagement | ManageMembers | Member engagement metrics |
| GET | /api/v1/analytics/revenue-forecast | ManageMembers | Revenue forecast |
| GET | /api/v1/analytics/benchmarks | ManageMembers | Platform benchmarks |
| GET | /api/v1/analytics/dashboard | ViewMembers | Dashboard summary |

---

## 🧪 Testing Approach

### Property Tests
```
Property 33: Health Score Bounds
  For ANY club analytics snapshot, the health score
  SHALL be >= 0 AND <= 100.

Property 34: Churn Prediction Requires Minimum Data
  For ANY member with less than 30 days of history,
  the churn predictor SHALL NOT generate a prediction.

Property 35: Revenue Forecast Non-Negative
  For ANY revenue forecast, all predicted monthly values
  SHALL be >= 0.
```

### Unit Tests
- Health score with all metrics at 100% → score = 100
- Health score with all metrics at 0% → score = 0
- Health score with mixed metrics → weighted average
- Churn: attendance drops 60% → flagged as at-risk
- Churn: attendance drops 40% → NOT flagged (below 50% threshold)
- Churn: member with 20 days history → skipped
- Revenue forecast with 2 months data → InsufficientData
- Revenue forecast with 6 months data → 3-month prediction
- Benchmark with < 5 clubs → not available

---

## 🚀 How to Extend

### Adding cohort analysis:
1. Group members by join month (cohort)
2. Track retention rate per cohort over time
3. Visualise as a retention matrix (cohort × month)

### Adding predictive lifetime value (LTV):
1. Calculate average revenue per member per month
2. Multiply by predicted retention duration
3. Factor in churn probability for at-risk members
