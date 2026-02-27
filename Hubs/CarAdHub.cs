using Microsoft.AspNetCore.SignalR;

namespace CarAds.Hubs;

public class CarAdHub : Hub
{
    private static int _onlineCount = 0;

    public override async Task OnConnectedAsync()
    {
        _onlineCount++;
        await Clients.All.SendAsync("OnlineCount", _onlineCount);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _onlineCount = Math.Max(0, _onlineCount - 1);
        await Clients.All.SendAsync("OnlineCount", _onlineCount);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task GetOnlineCount()
    {
        await Clients.Caller.SendAsync("OnlineCount", _onlineCount);
    }

    public async Task JoinProfile(string userId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"profile_{userId}");
    }

    public async Task LeaveProfile(string userId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"profile_{userId}");
    }
}