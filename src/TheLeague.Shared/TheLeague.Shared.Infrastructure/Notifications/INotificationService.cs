namespace TheLeague.Shared.Infrastructure.Notifications;

public interface INotificationService
{
    Task SendToUserAsync(string userId, string notificationType, object payload, CancellationToken ct = default);
    Task SendToClubAsync(string clubId, string notificationType, object payload, CancellationToken ct = default);
    Task SendToMatchAsync(Guid matchId, string eventType, object payload, CancellationToken ct = default);
}
