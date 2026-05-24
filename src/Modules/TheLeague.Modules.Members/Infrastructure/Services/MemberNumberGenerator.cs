using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Members.Infrastructure.Persistence;

namespace TheLeague.Modules.Members.Infrastructure.Services;

public class MemberNumberGenerator
{
    private readonly MembersDbContext _db;

    public MemberNumberGenerator(MembersDbContext db)
    {
        _db = db;
    }

    public async Task<string> GenerateNextAsync(Guid clubId, CancellationToken ct = default)
    {
        var maxNumber = await _db.Members
            .Where(m => m.ClubId == clubId && m.MemberNumber != null && m.MemberNumber != "")
            .Select(m => m.MemberNumber)
            .ToListAsync(ct);

        var nextSequence = 1;

        if (maxNumber.Count > 0)
        {
            nextSequence = maxNumber
                .Select(n => n.Replace("MBR-", ""))
                .Where(n => int.TryParse(n, out _))
                .Select(n => int.Parse(n))
                .DefaultIfEmpty(0)
                .Max() + 1;
        }

        var padded = nextSequence.ToString().PadLeft(3, '0');
        return $"MBR-{padded}";
    }
}
