using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Memberships.Application.Dtos;
using TheLeague.Modules.Memberships.Infrastructure.Persistence;
using TheLeague.Modules.Memberships.Infrastructure.Services;
using TheLeague.Shared.Contracts.Events;
using TheLeague.Shared.Contracts.Messaging;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Memberships.Application.Commands;

public record RenewMembershipCommand(Guid MembershipId) : IRequest<Result<MembershipDto>>;

public class RenewMembershipCommandHandler : IRequestHandler<RenewMembershipCommand, Result<MembershipDto>>
{
    private readonly MembershipsDbContext _db;
    private readonly IIntegrationEventBus _eventBus;

    public RenewMembershipCommandHandler(MembershipsDbContext db, IIntegrationEventBus eventBus)
    {
        _db = db;
        _eventBus = eventBus;
    }

    public async Task<Result<MembershipDto>> Handle(RenewMembershipCommand request, CancellationToken cancellationToken)
    {
        var membership = await _db.Memberships
            .FirstOrDefaultAsync(x => x.Id == request.MembershipId, cancellationToken);

        if (membership is null)
            return Result.Failure<MembershipDto>("Membership not found.");

        var membershipType = await _db.MembershipTypes
            .FirstOrDefaultAsync(x => x.Id == membership.MembershipTypeId, cancellationToken);

        if (membershipType is null)
            return Result.Failure<MembershipDto>("Membership type not found.");

        var newEndDate = BillingCycleCalculator.CalculateEndDate(membership.EndDate, membershipType.BillingCycle);

        membership.Renew(newEndDate);
        await _db.SaveChangesAsync(cancellationToken);

        await _eventBus.PublishAsync(
            new MembershipRenewedEvent(membership.Id, membership.MemberId, membership.ClubId),
            cancellationToken);

        var dto = new MembershipDto(
            membership.Id, membership.ClubId, membership.MemberId, membership.MembershipTypeId,
            membership.StartDate, membership.EndDate, membership.Status, membership.AutoRenew,
            membership.PricePaid, membership.DiscountApplied, membership.DiscountType,
            membership.CreatedAt, membership.UpdatedAt);

        return Result.Success(dto);
    }
}
