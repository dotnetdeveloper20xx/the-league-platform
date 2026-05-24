using TheLeague.Shared.Domain.Enums;

namespace TheLeague.Modules.Clubs.Application.Dtos;

public record ClubDto(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    string? LogoUrl,
    string PrimaryColor,
    string SecondaryColor,
    string? AccentColor,
    string? ContactEmail,
    string? ContactPhone,
    string? Address,
    string? Website,
    ClubType ClubType,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record ClubSettingsDto(
    Guid Id,
    Guid ClubId,
    string? Timezone,
    string? Currency,
    string? Locale,
    int BookingCancellationHours,
    bool RequireEmailVerification,
    string? CustomTerminology
);
