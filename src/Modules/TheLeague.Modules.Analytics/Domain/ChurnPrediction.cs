using TheLeague.Shared.Domain.Entities;

namespace TheLeague.Modules.Analytics.Domain;

public class ChurnPrediction : TenantEntity
{
    public Guid MemberId { get; private set; }
    public DateTime PredictionDate { get; private set; }
    public bool IsAtRisk { get; private set; }
    public string? RiskFactors { get; private set; } // JSON: attendance_drop, missed_payments, login_drop
    public decimal? AttendanceDropPercent { get; private set; }
    public int? MissedPaymentCount { get; private set; }
    public decimal? LoginDropPercent { get; private set; }

    private ChurnPrediction() { }

    public static ChurnPrediction Create(
        Guid clubId,
        Guid memberId,
        DateTime predictionDate,
        bool isAtRisk,
        string? riskFactors,
        decimal? attendanceDropPercent,
        int? missedPaymentCount,
        decimal? loginDropPercent)
    {
        return new ChurnPrediction
        {
            ClubId = clubId,
            MemberId = memberId,
            PredictionDate = predictionDate,
            IsAtRisk = isAtRisk,
            RiskFactors = riskFactors,
            AttendanceDropPercent = attendanceDropPercent,
            MissedPaymentCount = missedPaymentCount,
            LoginDropPercent = loginDropPercent
        };
    }
}
