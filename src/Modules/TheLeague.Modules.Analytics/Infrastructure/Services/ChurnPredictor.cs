using TheLeague.Modules.Analytics.Domain;

namespace TheLeague.Modules.Analytics.Infrastructure.Services;

public interface IChurnPredictor
{
    ChurnPredictionResult Predict(MemberActivityData activityData);
}

public record MemberActivityData(
    Guid MemberId,
    Guid ClubId,
    decimal AttendanceCurrentPeriod,
    decimal AttendancePreviousPeriod,
    int ConsecutiveMissedPayments,
    decimal LoginsCurrentPeriod,
    decimal LoginsPreviousPeriod,
    bool HasBookingsOrRegistrations
);

public record ChurnPredictionResult(
    bool IsAtRisk,
    List<string> RiskFactors,
    decimal? AttendanceDropPercent,
    int? MissedPaymentCount,
    decimal? LoginDropPercent
);

public class ChurnPredictor : IChurnPredictor
{
    private const decimal AttendanceDropThreshold = 50m;
    private const int MissedPaymentThreshold = 2;
    private const decimal LoginDropThreshold = 50m;

    /// <summary>
    /// Predicts churn risk for a member within a 90-day window.
    /// Flags at-risk if: attendance drops ≥50%, ≥2 consecutive missed payments, or login drops ≥50%.
    /// Excludes members with no bookings/registrations in the window.
    /// </summary>
    public ChurnPredictionResult Predict(MemberActivityData activityData)
    {
        // Exclude members with no bookings/registrations in the window
        if (!activityData.HasBookingsOrRegistrations)
        {
            return new ChurnPredictionResult(false, new List<string>(), null, null, null);
        }

        var riskFactors = new List<string>();
        decimal? attendanceDropPercent = null;
        int? missedPaymentCount = null;
        decimal? loginDropPercent = null;

        // Check attendance drop
        if (activityData.AttendancePreviousPeriod > 0)
        {
            var attendanceDrop = ((activityData.AttendancePreviousPeriod - activityData.AttendanceCurrentPeriod) / activityData.AttendancePreviousPeriod) * 100m;
            if (attendanceDrop >= AttendanceDropThreshold)
            {
                riskFactors.Add("attendance_drop");
                attendanceDropPercent = Math.Round(attendanceDrop, 2);
            }
        }

        // Check missed payments
        if (activityData.ConsecutiveMissedPayments >= MissedPaymentThreshold)
        {
            riskFactors.Add("missed_payments");
            missedPaymentCount = activityData.ConsecutiveMissedPayments;
        }

        // Check login drop
        if (activityData.LoginsPreviousPeriod > 0)
        {
            var loginDrop = ((activityData.LoginsPreviousPeriod - activityData.LoginsCurrentPeriod) / activityData.LoginsPreviousPeriod) * 100m;
            if (loginDrop >= LoginDropThreshold)
            {
                riskFactors.Add("login_drop");
                loginDropPercent = Math.Round(loginDrop, 2);
            }
        }

        var isAtRisk = riskFactors.Count > 0;
        return new ChurnPredictionResult(isAtRisk, riskFactors, attendanceDropPercent, missedPaymentCount, loginDropPercent);
    }
}
