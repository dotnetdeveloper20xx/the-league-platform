using Microsoft.AspNetCore.SignalR;

namespace TheLeague.Shared.Infrastructure.SignalR;

public class MatchCentreHub : Hub
{
    public async Task JoinMatch(Guid matchId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"match-{matchId}");
    }

    public async Task LeaveMatch(Guid matchId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"match-{matchId}");
    }
}
