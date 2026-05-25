using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using TheLeague.Shared.Infrastructure.SignalR;

namespace TheLeague.Shared.Infrastructure.Notifications;

public class NotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub> _notificationHub;
    private readonly IHubContext<MatchCentreHub> _matchHub;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IHubContext<NotificationHub> notificationHub,
        IHubContext<MatchCentreHub> matchHub,
        ILogger<NotificationService> logger)
    {
        _notificationHub = notificationHub;
        _matchHub = matchHub;
        _logger = logger;
    }

    public async Task SendToUserAsync(string userId, string notificationType, object payload, CancellationToken ct = default)
    {
        _logger.LogInformation("Sending notification {Type} to user {UserId}", notificationType, userId);
        await _notificationHub.Clients.Group($"user-{userId}")
            .SendAsync("ReceiveNotification", notificationType, payload, ct);
    }

    public async Task SendToClubAsync(string clubId, string notificationType, object payload, CancellationToken ct = default)
    {
        _logger.LogInformation("Sending notification {Type} to club {ClubId}", notificationType, clubId);
        await _notificationHub.Clients.Group($"club-{clubId}")
            .SendAsync("ReceiveNotification", notificationType, payload, ct);
    }

    public async Task SendToMatchAsync(Guid matchId, string eventType, object payload, CancellationToken ct = default)
    {
        _logger.LogInformation("Sending match event {Type} to match {MatchId}", eventType, matchId);
        await _matchHub.Clients.Group($"match-{matchId}")
            .SendAsync("MatchEvent", eventType, payload, ct);
    }
}
