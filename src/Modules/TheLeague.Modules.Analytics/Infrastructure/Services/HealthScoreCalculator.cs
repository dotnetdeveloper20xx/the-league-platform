namespace TheLeague.Modules.Analytics.Infrastructure.Services;

public interface IHealthScoreCalculator
{
    int Calculate(decimal memberGrowthRate, decimal paymentCollectionRate, decimal sessionAttendanceRate, decimal eventParticipationRate);
}

public class HealthScoreCalculator : IHealthScoreCalculator
{
    private const decimal MemberGrowthWeight = 0.25m;
    private const decimal PaymentCollectionWeight = 0.25m;
    private const decimal SessionAttendanceWeight = 0.25m;
    private const decimal EventParticipationWeight = 0.25m;

    /// <summary>
    /// Calculates the club health score as a weighted average of four metrics.
    /// Each metric is scored 0-100, and the final score is clamped to 0-100.
    /// </summary>
    public int Calculate(
        decimal memberGrowthRate,
        decimal paymentCollectionRate,
        decimal sessionAttendanceRate,
        decimal eventParticipationRate)
    {
        var memberGrowthScore = ClampMetric(memberGrowthRate);
        var paymentCollectionScore = ClampMetric(paymentCollectionRate);
        var sessionAttendanceScore = ClampMetric(sessionAttendanceRate);
        var eventParticipationScore = ClampMetric(eventParticipationRate);

        var weightedAverage =
            (memberGrowthScore * MemberGrowthWeight) +
            (paymentCollectionScore * PaymentCollectionWeight) +
            (sessionAttendanceScore * SessionAttendanceWeight) +
            (eventParticipationScore * EventParticipationWeight);

        var score = (int)Math.Round(weightedAverage);
        return Math.Clamp(score, 0, 100);
    }

    private static decimal ClampMetric(decimal value)
    {
        if (value < 0) return 0;
        if (value > 100) return 100;
        return value;
    }
}
