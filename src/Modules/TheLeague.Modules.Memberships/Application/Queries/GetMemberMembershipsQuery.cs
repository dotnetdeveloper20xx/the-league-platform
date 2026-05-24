using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Memberships.Application.Dtos;
using TheLeague.Modules.Memberships.Infrastructure.Persistence;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Memberships.Application.Queries;

public record GetMemberMembershipsQuery(Guid MemberId) : IRequest<Result<List<MembershipDto>>>;

public class GetMemberMembershipsQueryHandler : IRequestHandler<GetMemberMembershipsQuery, Result<List<MembershipDto>>>
{
    private readonly MembershipsDbContext _db;

    public GetMemberMembershipsQueryHandler(MembershipsDbContext db)
    {
        _db = db;
    }

    public async Task<Result<List<MembershipDto>>> Handle(GetMemberMembershipsQuery request, CancellationToken cancellationToken)
    {
        var memberships = await _db.Memberships
            .AsNoTracking()
            .Where(x => x.MemberId == request.MemberId)
            .OrderByDescending(x => x.StartDate)
            .Select(x => new MembershipDto(
                x.Id, x.ClubId, x.MemberId, x.MembershipTypeId,
                x.StartDate, x.EndDate, x.Status, x.AutoRenew,
                x.PricePaid, x.DiscountApplied, x.DiscountType,
                x.CreatedAt, x.UpdatedAt))
            .ToListAsync(cancellationToken);

        return Result.Success(memberships);
    }
}
