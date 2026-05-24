namespace TheLeague.Modules.Identity.Application.Dtos;

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    UserDto User);

public record UserDto(
    string Id,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    Guid? ClubId,
    Guid? MemberId);

public record SessionDto(
    Guid Id,
    string DeviceIdentifier,
    string? IpAddress,
    DateTime LastActiveAt,
    DateTime CreatedAt);
