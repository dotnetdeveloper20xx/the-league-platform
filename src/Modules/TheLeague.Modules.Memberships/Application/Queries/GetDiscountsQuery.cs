using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Memberships.Application.Dtos;
using TheLeague.Modules.Memberships.Infrastructure.Persistence;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Memberships.Application.Queries;

public record GetDiscountsQuery(Guid ClubId, Guid? MembershipTypeId = null) : IRequest<Result<List<MembershipDiscountDto>>>;

public class GetDiscountsQueryHandler : IRequestHandler<GetDiscountsQuery, Result<List<MembershipDiscountDto>>>
{
    private readonly MembershipsDbContext _db;

    public GetDiscountsQueryHandler(MembershipsDbContext db)
    {
        _db = db;
    }

    public async Task<Result<List<MembershipDiscountDto>>> Handle(GetDiscountsQuery request, CancellationToken cancellationToken)
    {
        var query = _db.MembershipDiscounts
            .AsNoTracking()
            .Where(x => x.ClubId == request.ClubId);

        if (request.MembershipTypeId.HasValue)
            query = query.Where(x => x.MembershipTypeId == request.MembershipTypeId.Value);

        var discounts = await query
            .OrderByDescending(x => x.ValidFrom)
            .Select(x => new MembershipDiscountDto(
                x.Id, x.ClubId, x.MembershipTypeId, x.DiscountType,
                x.IsPercentage, x.Value, x.ValidFrom, x.ValidTo,
                x.PromoCode, x.MaxUses, x.CurrentUses))
            .ToListAsync(cancellationToken);

        return Result.Success(discounts);
    }
}
