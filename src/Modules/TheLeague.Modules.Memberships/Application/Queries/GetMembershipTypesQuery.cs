using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Memberships.Application.Dtos;
using TheLeague.Modules.Memberships.Infrastructure.Persistence;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Memberships.Application.Queries;

public record GetMembershipTypesQuery(Guid ClubId) : IRequest<Result<List<MembershipTypeDto>>>;

public class GetMembershipTypesQueryHandler : IRequestHandler<GetMembershipTypesQuery, Result<List<MembershipTypeDto>>>
{
    private readonly MembershipsDbContext _db;

    public GetMembershipTypesQueryHandler(MembershipsDbContext db)
    {
        _db = db;
    }

    public async Task<Result<List<MembershipTypeDto>>> Handle(GetMembershipTypesQuery request, CancellationToken cancellationToken)
    {
        var types = await _db.MembershipTypes
            .AsNoTracking()
            .Where(x => x.ClubId == request.ClubId)
            .OrderBy(x => x.Name)
            .Select(x => new MembershipTypeDto(
                x.Id, x.ClubId, x.Name, x.Description, x.Price, x.BillingCycle,
                x.MinAge, x.MaxAge, x.Capacity, x.JoiningFee, x.IsActive,
                x.AllowAutoRenewal, x.FreezeFee, x.CreatedAt, x.UpdatedAt))
            .ToListAsync(cancellationToken);

        return Result.Success(types);
    }
}
