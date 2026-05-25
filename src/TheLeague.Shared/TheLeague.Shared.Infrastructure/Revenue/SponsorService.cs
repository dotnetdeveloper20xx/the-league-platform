using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace TheLeague.Shared.Infrastructure.Revenue;

public class SponsorService : ISponsorService
{
    private readonly ILogger<SponsorService> _logger;
    private static readonly ConcurrentDictionary<Guid, List<SponsorDto>> _sponsors = new();

    public SponsorService(ILogger<SponsorService> logger)
    {
        _logger = logger;
    }

    public Task<List<SponsorDto>> GetSponsorsAsync(Guid clubId, CancellationToken ct = default)
    {
        _logger.LogInformation("Getting sponsors for club {ClubId}", clubId);

        var sponsors = _sponsors.GetValueOrDefault(clubId, new List<SponsorDto>());
        return Task.FromResult(sponsors);
    }

    public Task<SponsorDto> AddSponsorAsync(Guid clubId, AddSponsorRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("Adding sponsor '{Name}' to club {ClubId}", request.Name, clubId);

        var sponsor = new SponsorDto(
            Id: Guid.NewGuid(),
            Name: request.Name,
            LogoUrl: request.LogoUrl,
            WebsiteUrl: request.WebsiteUrl,
            Tier: request.Tier,
            StartDate: request.StartDate,
            EndDate: request.EndDate,
            AgreedAmount: request.AgreedAmount);

        _sponsors.AddOrUpdate(
            clubId,
            _ => new List<SponsorDto> { sponsor },
            (_, existing) => { existing.Add(sponsor); return existing; });

        return Task.FromResult(sponsor);
    }
}
