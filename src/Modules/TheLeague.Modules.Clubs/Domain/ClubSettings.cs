using TheLeague.Shared.Domain.Entities;

namespace TheLeague.Modules.Clubs.Domain;

public class ClubSettings : BaseEntity
{
    public Guid ClubId { get; private set; }
    public string? Timezone { get; private set; } = "Europe/London";
    public string? Currency { get; private set; } = "GBP";
    public string? Locale { get; private set; } = "en-GB";
    public int BookingCancellationHours { get; private set; } = 24;
    public bool RequireEmailVerification { get; private set; } = true;
    public string? CustomTerminology { get; private set; } // JSON: {"match": "fixture", "pitch": "ground"}

    public static ClubSettings CreateDefault(Guid clubId) => new() { ClubId = clubId };
}
