using Microsoft.Extensions.Logging;

namespace TheLeague.Shared.Infrastructure.Integrations;

public class IntegrationService : IIntegrationService
{
    private readonly ILogger<IntegrationService> _logger;

    public IntegrationService(ILogger<IntegrationService> logger)
    {
        _logger = logger;
    }

    public Task SyncToAccountingAsync(Guid clubId, CancellationToken ct = default)
    {
        _logger.LogInformation("Syncing club {ClubId} data to accounting provider", clubId);
        return Task.CompletedTask;
    }

    public Task SyncToCalendarAsync(Guid clubId, CancellationToken ct = default)
    {
        _logger.LogInformation("Syncing club {ClubId} sessions/events to calendar provider", clubId);
        return Task.CompletedTask;
    }

    public Task PostToSocialMediaAsync(Guid clubId, string content, CancellationToken ct = default)
    {
        _logger.LogInformation("Posting to social media for club {ClubId}: {Content}", clubId, content);
        return Task.CompletedTask;
    }
}
