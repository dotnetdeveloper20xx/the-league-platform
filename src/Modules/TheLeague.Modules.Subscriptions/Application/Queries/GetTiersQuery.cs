using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Subscriptions.Application.Dtos;
using TheLeague.Modules.Subscriptions.Infrastructure.Persistence;

namespace TheLeague.Modules.Subscriptions.Application.Queries;

public record GetTiersQuery : IRequest<List<TierDto>>;

public class GetTiersQueryHandler : IRequestHandler<GetTiersQuery, List<TierDto>>
{
    private readonly SubscriptionsDbContext _db;

    public GetTiersQueryHandler(SubscriptionsDbContext db)
    {
        _db = db;
    }

    public async Task<List<TierDto>> Handle(GetTiersQuery request, CancellationToken cancellationToken)
    {
        var tiers = await _db.TierConfigs
            .OrderBy(t => t.MonthlyPrice)
            .ToListAsync(cancellationToken);

        return tiers.Select(t => new TierDto(
            t.Tier,
            t.Name,
            t.MonthlyPrice,
            t.MaxMembers,
            t.MaxStorageBytes,
            t.MonthlySmsCredits,
            t.Features
        )).ToList();
    }
}
