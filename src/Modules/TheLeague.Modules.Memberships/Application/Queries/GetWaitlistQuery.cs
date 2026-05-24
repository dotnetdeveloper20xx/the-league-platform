using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Memberships.Application.Dtos;
using TheLeague.Modules.Memberships.Infrastructure.Persistence;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Memberships.Application.Queries;

public record GetWaitlistQuery(Guid MembershipTypeId) : IRequest<Result<List<MembershipWaitlistDto>>>;

public class GetWaitlistQueryHandler : IRequestHandler<GetWaitlistQuery, Result<List<MembershipWaitlistDto>>>
{
    private readonly MembershipsDbContext _db;

    public GetWaitlistQueryHandler(MembershipsDbContext db)
    {
        _db = db;
    }

    public async Task<Result<List<MembershipWaitlistDto>>> Handle(GetWaitlistQuery request, CancellationToken cancellationToken)
    {
        var waitlist = await _db.MembershipWaitlists
            .AsNoTracking()
            .Where(x => x.MembershipTypeId == request.MembershipTypeId)
            .OrderBy(x => x.Position)
            .Select(x => new MembershipWaitlistDto(
                x.Id, x.MembershipTypeId, x.MemberId,
                x.Position, x.RequestedAt, x.NotifiedAt, x.Status))
            .ToListAsync(cancellationToken);

        return Result.Success(waitlist);
    }
}
