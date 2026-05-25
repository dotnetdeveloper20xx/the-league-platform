namespace TheLeague.Shared.Infrastructure.Integrations;

public interface IIntegrationService
{
    Task SyncToAccountingAsync(Guid clubId, CancellationToken ct = default);
    Task SyncToCalendarAsync(Guid clubId, CancellationToken ct = default);
    Task PostToSocialMediaAsync(Guid clubId, string content, CancellationToken ct = default);
}
