namespace TheLeague.Shared.Infrastructure.Onboarding;

public interface IOnboardingService
{
    Task<OnboardingResult> CreateClubWithSubscriptionAsync(CreateClubRequest request, CancellationToken ct = default);
    Task<OnboardingStatus> GetOnboardingStatusAsync(Guid clubId, CancellationToken ct = default);
}

public record CreateClubRequest(
    string Name,
    string Slug,
    string ClubType,
    string Email,
    string Password,
    string SelectedTier);

public record OnboardingResult(
    bool Success,
    Guid? ClubId,
    string? Error);

public record OnboardingStatus(
    bool ProfileComplete,
    bool MembershipTypesConfigured,
    bool MembersImported,
    bool PaymentProviderConnected);
