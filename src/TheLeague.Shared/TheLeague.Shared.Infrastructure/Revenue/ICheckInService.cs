namespace TheLeague.Shared.Infrastructure.Revenue;

public interface ICheckInService
{
    Task<CheckInResult> ValidateAndCheckInAsync(Guid clubId, string qrCodeOrNfcData, CancellationToken ct = default);
}

public record CheckInResult(
    bool Success,
    string? MemberName,
    string? FailureReason);
