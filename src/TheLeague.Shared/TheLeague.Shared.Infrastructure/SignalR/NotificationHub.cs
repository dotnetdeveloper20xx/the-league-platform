using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace TheLeague.Shared.Infrastructure.SignalR;

[Authorize]
public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var clubId = Context.User?.FindFirst("clubId")?.Value;
        var userId = Context.UserIdentifier;

        if (!string.IsNullOrEmpty(clubId))
            await Groups.AddToGroupAsync(Context.ConnectionId, $"club-{clubId}");

        if (!string.IsNullOrEmpty(userId))
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var clubId = Context.User?.FindFirst("clubId")?.Value;
        var userId = Context.UserIdentifier;

        if (!string.IsNullOrEmpty(clubId))
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"club-{clubId}");

        if (!string.IsNullOrEmpty(userId))
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user-{userId}");

        await base.OnDisconnectedAsync(exception);
    }
}
