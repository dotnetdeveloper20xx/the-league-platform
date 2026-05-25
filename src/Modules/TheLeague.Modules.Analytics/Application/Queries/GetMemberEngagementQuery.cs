using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Analytics.Application.Dtos;
using TheLeague.Modules.Analytics.Infrastructure.Persistence;
using TheLeague.Shared.Contracts.Services;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Analytics.Application.Queries;

public record GetMemberEngagementQuery(Guid MemberId) : IRequest<Result<List<MemberEngagementDto>>>;

public class GetMemberEngagementQueryHandler : IRequestHandler<GetMemberEngagementQuery, Result<List<MemberEngagementDto>>>
{
    private readonly AnalyticsDbContext _db;
    private readonly ITenantService _tenantService;

    public GetMemberEngagementQueryHandler(AnalyticsDbContext db, ITenantService tenantService)
    {
        _db = db;
        _tenantService = tenantService;
    }

    public async Task<Result<List<MemberEngagementDto>>> Handle(GetMemberEngagementQuery request, CancellationToken cancellationToken)
    {
        if (_tenantService.CurrentTenantId is null)
            return Result.Failure<List<MemberEngagementDto>>("Tenant context is required.");

        var engagements = await _db.MemberEngagements
            .Where(e => e.MemberId == request.MemberId)
            .OrderByDescending(e => e.Month)
            .ToListAsync(cancellationToken);

        var dtos = engagements.Select(e => new MemberEngagementDto(
            e.MemberId,
            e.Month,
            e.SessionsAttended,
            e.EventsAttended,
            e.PaymentTimelinessDays,
            e.PortalLogins)).ToList();

        return Result.Success(dtos);
    }
}
