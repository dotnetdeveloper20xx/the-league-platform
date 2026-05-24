namespace TheLeague.Modules.Subscriptions.Infrastructure.Services;

public interface IFeatureGateService
{
    Task<bool> IsFeatureEnabledAsync(Guid clubId, string featureKey, CancellationToken ct = default);
    Task<bool> CanAddMemberAsync(Guid clubId, CancellationToken ct = default);
    Task<bool> CanUploadAsync(Guid clubId, long fileSizeBytes, CancellationToken ct = default);
    Task<bool> CanSendSmsAsync(Guid clubId, CancellationToken ct = default);
}
