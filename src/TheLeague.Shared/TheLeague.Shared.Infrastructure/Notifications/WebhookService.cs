using Microsoft.Extensions.Logging;

namespace TheLeague.Shared.Infrastructure.Notifications;

public class WebhookService : IWebhookService
{
    private readonly ILogger<WebhookService> _logger;

    public WebhookService(ILogger<WebhookService> logger)
    {
        _logger = logger;
    }

    public Task DeliverAsync(Guid clubId, string eventType, object payload, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "Webhook delivery for club {ClubId}: event={EventType}, payload={@Payload}",
            clubId, eventType, payload);

        // TODO: Implement actual webhook delivery with HTTP client,
        // signature verification, and retry logic (3x exponential backoff: 5s, 25s, 125s)
        return Task.CompletedTask;
    }
}
