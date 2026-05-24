using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Memberships.Application.Dtos;
using TheLeague.Modules.Memberships.Infrastructure.Persistence;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Memberships.Application.Commands;

public record CancelMembershipCommand(Guid MembershipId) : IRequest<Result<MembershipDto>>;

public class CancelMembershipCommandHandler : IRequestHandler<CancelMembershipCommand, Result<MembershipDto>>
{
    private readonly MembershipsDbContext _db;

    public CancelMembershipCommandHandler(MembershipsDbContext db)
    {
        _db = db;
    }

    public async Task<Result<MembershipDto>> Handle(CancelMembershipCommand request, CancellationToken cancellationToken)
    {
        var membership = await _db.Memberships
            .FirstOrDefaultAsync(x => x.Id == request.MembershipId, cancellationToken);

        if (membership is null)
            return Result.Failure<MembershipDto>("Membership not found.");

        membership.Cancel();
        await _db.SaveChangesAsync(cancellationToken);

        var dto = new MembershipDto(
            membership.Id, membership.ClubId, membership.MemberId, membership.MembershipTypeId,
            membership.StartDate, membership.EndDate, membership.Status, membership.AutoRenew,
            membership.PricePaid, membership.DiscountApplied, membership.DiscountType,
            membership.CreatedAt, membership.UpdatedAt);

        return Result.Success(dto);
    }
}
