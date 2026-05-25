using System.Text.Json;
using MediatR;
using TheLeague.Modules.Analytics.Application.Dtos;
using TheLeague.Modules.Analytics.Domain;
using TheLeague.Modules.Analytics.Infrastructure.Persistence;
using TheLeague.Modules.Analytics.Infrastructure.Services;
using TheLeague.Shared.Contracts.Services;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Analytics.Application.Commands;

public record CalculateChurnPredictionsCommand(
    List<MemberActivityInput> MemberActivities
) : IRequest<Result<List<ChurnPredictionDto>>>;

public record MemberActivityInput(
    Guid MemberId,
    decimal AttendanceCurrentPeriod,
    decimal AttendancePreviousPeriod,
    int ConsecutiveMissedPayments,
    decimal LoginsCurrentPeriod,
    decimal LoginsPreviousPeriod,
    bool HasBookingsOrRegistrations
);

public class CalculateChurnPredictionsCommandHandler : IRequestHandler<CalculateChurnPredictionsCommand, Result<List<ChurnPredictionDto>>>
{
    private readonly AnalyticsDbContext _db;
    private readonly ITenantService _tenantService;
    private readonly IChurnPredictor _churnPredictor;

    public CalculateChurnPredictionsCommandHandler(
        AnalyticsDbContext db,
        ITenantService tenantService,
        IChurnPredictor churnPredictor)
    {
        _db = db;
        _tenantService = tenantService;
        _churnPredictor = churnPredictor;
    }

    public async Task<Result<List<ChurnPredictionDto>>> Handle(CalculateChurnPredictionsCommand request, CancellationToken cancellationToken)
    {
        if (_tenantService.CurrentTenantId is null)
            return Result.Failure<List<ChurnPredictionDto>>("Tenant context is required.");

        var clubId = _tenantService.CurrentTenantId.Value;
        var predictions = new List<ChurnPredictionDto>();

        foreach (var memberActivity in request.MemberActivities)
        {
            var activityData = new MemberActivityData(
                memberActivity.MemberId,
                clubId,
                memberActivity.AttendanceCurrentPeriod,
                memberActivity.AttendancePreviousPeriod,
                memberActivity.ConsecutiveMissedPayments,
                memberActivity.LoginsCurrentPeriod,
                memberActivity.LoginsPreviousPeriod,
                memberActivity.HasBookingsOrRegistrations);

            var result = _churnPredictor.Predict(activityData);

            var riskFactorsJson = result.RiskFactors.Count > 0
                ? JsonSerializer.Serialize(result.RiskFactors)
                : null;

            var prediction = ChurnPrediction.Create(
                clubId,
                memberActivity.MemberId,
                DateTime.UtcNow,
                result.IsAtRisk,
                riskFactorsJson,
                result.AttendanceDropPercent,
                result.MissedPaymentCount,
                result.LoginDropPercent);

            _db.ChurnPredictions.Add(prediction);

            predictions.Add(new ChurnPredictionDto(
                prediction.MemberId,
                prediction.PredictionDate,
                prediction.IsAtRisk,
                prediction.RiskFactors,
                prediction.AttendanceDropPercent,
                prediction.MissedPaymentCount,
                prediction.LoginDropPercent));
        }

        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success(predictions);
    }
}
