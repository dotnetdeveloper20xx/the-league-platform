namespace TheLeague.Shared.Infrastructure.Revenue;

public interface ISponsorService
{
    Task<List<SponsorDto>> GetSponsorsAsync(Guid clubId, CancellationToken ct = default);
    Task<SponsorDto> AddSponsorAsync(Guid clubId, AddSponsorRequest request, CancellationToken ct = default);
}

public record SponsorDto(
    Guid Id,
    string Name,
    string? LogoUrl,
    string? WebsiteUrl,
    string? Tier,
    DateTime? StartDate,
    DateTime? EndDate,
    decimal? AgreedAmount);

public record AddSponsorRequest(
    string Name,
    string? LogoUrl,
    string? WebsiteUrl,
    string? Tier,
    DateTime? StartDate,
    DateTime? EndDate,
    decimal? AgreedAmount);
