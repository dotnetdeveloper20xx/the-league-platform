using Microsoft.Extensions.Logging;

namespace TheLeague.Shared.Infrastructure.Onboarding;

public class OnboardingService : IOnboardingService
{
    private readonly ILogger<OnboardingService> _logger;

    public OnboardingService(ILogger<OnboardingService> logger)
    {
        _logger = logger;
    }

    public Task<OnboardingResult> CreateClubWithSubscriptionAsync(CreateClubRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("Creating club '{Name}' with tier '{Tier}' for {Email}",
            request.Name, request.SelectedTier, request.Email);

        // Placeholder: In production, this would create the club, user account, and subscription
        var clubId = Guid.NewGuid();
        var result = new OnboardingResult(Success: true, ClubId: clubId, Error: null);

        return Task.FromResult(result);
    }

    public Task<OnboardingStatus> GetOnboardingStatusAsync(Guid clubId, CancellationToken ct = default)
    {
        _logger.LogInformation("Getting onboarding status for club {ClubId}", clubId);

        // Placeholder: In production, this would check actual completion state
        var status = new OnboardingStatus(
            ProfileComplete: false,
            MembershipTypesConfigured: false,
            MembersImported: false,
            PaymentProviderConnected: false);

        return Task.FromResult(status);
    }
}
