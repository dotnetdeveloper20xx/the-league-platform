using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Analytics.Application.Dtos;
using TheLeague.Modules.Analytics.Infrastructure.Persistence;
using TheLeague.Shared.Contracts.Services;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Analytics.Application.Queries;

public record GetChurnPredictionsQuery : IRequest<Result<List<ChurnPredictionDto>>>;

public class GetChurnPredictionsQueryHandler : IRequestHandler<GetChurnPredictionsQuery, Result<List<ChurnPredictionDto>>>
{
    private readonly AnalyticsDbContext _db;
    private readonly ITenantService _tenantService;

    public GetChurnPredictionsQueryHandler(AnalyticsDbContext db, ITenantService tenantService)
    {
        _db = db;
        _tenantService = tenantService;
    }

    public async Task<Result<List<ChurnPredictionDto>>> Handle(GetChurnPredictionsQuery request, CancellationToken cancellationToken)
    {
        if (_tenantService.CurrentTenantId is null)
            return Result.Failure<List<ChurnPredictionDto>>("Tenant context is required.");

        // Get the latest prediction for each member that is at risk
        var predictions = await _db.ChurnPredictions
            .Where(p => p.IsAtRisk)
            .GroupBy(p => p.MemberId)
            .Select(g => g.OrderByDescending(p => p.PredictionDate).First())
            .ToListAsync(cancellationToken);

        var dtos = predictions.Select(p => new ChurnPredictionDto(
            p.MemberId,
            p.PredictionDate,
            p.IsAtRisk,
            p.RiskFactors,
            p.AttendanceDropPercent,
            p.MissedPaymentCount,
            p.LoginDropPercent)).ToList();

        return Result.Success(dtos);
    }
}
