using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Analytics.Application.Dtos;
using TheLeague.Modules.Analytics.Infrastructure.Persistence;
using TheLeague.Modules.Analytics.Infrastructure.Services;
using TheLeague.Shared.Contracts.Services;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Analytics.Application.Queries;

public record GetRevenueForecastQuery(
    int ActiveMembershipCount,
    decimal AverageMonthlyFee,
    decimal HistoricalRenewalRate
) : IRequest<Result<RevenueForecastDto>>;

public class GetRevenueForecastQueryHandler : IRequestHandler<GetRevenueForecastQuery, Result<RevenueForecastDto>>
{
    private readonly AnalyticsDbContext _db;
    private readonly ITenantService _tenantService;
    private readonly IRevenueForecaster _revenueForecaster;

    public GetRevenueForecastQueryHandler(AnalyticsDbContext db, ITenantService tenantService, IRevenueForecaster revenueForecaster)
    {
        _db = db;
        _tenantService = tenantService;
        _revenueForecaster = revenueForecaster;
    }

    public async Task<Result<RevenueForecastDto>> Handle(GetRevenueForecastQuery request, CancellationToken cancellationToken)
    {
        if (_tenantService.CurrentTenantId is null)
            return Result.Failure<RevenueForecastDto>("Tenant context is required.");

        // Get last 12 months of revenue from snapshots
        var twelveMonthsAgo = DateTime.UtcNow.AddMonths(-12);
        var snapshots = await _db.Snapshots
            .Where(s => s.SnapshotDate >= twelveMonthsAgo)
            .OrderBy(s => s.SnapshotDate)
            .ToListAsync(cancellationToken);

        var last12MonthsRevenue = snapshots.Select(s => s.TotalRevenue).ToList();

        var input = new RevenueForecastInput(
            request.ActiveMembershipCount,
            request.AverageMonthlyFee,
            request.HistoricalRenewalRate,
            last12MonthsRevenue);

        var forecast = _revenueForecaster.Forecast(input);

        var dto = new RevenueForecastDto(
            forecast.MonthlyForecasts.Select(f => new MonthForecastDto(
                f.MonthOffset,
                f.ProjectedRevenue,
                f.ProjectedMembershipRevenue,
                f.ConfidenceLevel)).ToList(),
            forecast.TotalProjectedRevenue);

        return Result.Success(dto);
    }
}
