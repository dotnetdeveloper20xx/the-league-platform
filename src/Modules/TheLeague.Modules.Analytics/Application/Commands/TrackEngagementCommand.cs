using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Analytics.Application.Dtos;
using TheLeague.Modules.Analytics.Domain;
using TheLeague.Modules.Analytics.Infrastructure.Persistence;
using TheLeague.Shared.Contracts.Services;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Analytics.Application.Commands;

public record TrackEngagementCommand(
    Guid MemberId,
    DateOnly Month,
    int SessionsAttended,
    int EventsAttended,
    decimal PaymentTimelinessDays,
    int PortalLogins
) : IRequest<Result<MemberEngagementDto>>;

public class TrackEngagementCommandHandler : IRequestHandler<TrackEngagementCommand, Result<MemberEngagementDto>>
{
    private readonly AnalyticsDbContext _db;
    private readonly ITenantService _tenantService;

    public TrackEngagementCommandHandler(AnalyticsDbContext db, ITenantService tenantService)
    {
        _db = db;
        _tenantService = tenantService;
    }

    public async Task<Result<MemberEngagementDto>> Handle(TrackEngagementCommand request, CancellationToken cancellationToken)
    {
        if (_tenantService.CurrentTenantId is null)
            return Result.Failure<MemberEngagementDto>("Tenant context is required.");

        var clubId = _tenantService.CurrentTenantId.Value;

        // Check if engagement record already exists for this member/month
        var existing = await _db.MemberEngagements
            .FirstOrDefaultAsync(e => e.MemberId == request.MemberId && e.Month == request.Month, cancellationToken);

        if (existing != null)
        {
            // Remove old record and create new one (update pattern)
            _db.MemberEngagements.Remove(existing);
        }

        var engagement = MemberEngagement.Create(
            clubId,
            request.MemberId,
            request.Month,
            request.SessionsAttended,
            request.EventsAttended,
            request.PaymentTimelinessDays,
            request.PortalLogins);

        _db.MemberEngagements.Add(engagement);
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success(new MemberEngagementDto(
            engagement.MemberId,
            engagement.Month,
            engagement.SessionsAttended,
            engagement.EventsAttended,
            engagement.PaymentTimelinessDays,
            engagement.PortalLogins));
    }
}
