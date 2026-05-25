namespace TheLeague.Shared.Infrastructure.Notifications;

public interface IWebhookService
{
    Task DeliverAsync(Guid clubId, string eventType, object payload, CancellationToken ct = default);
}
