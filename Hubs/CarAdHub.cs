using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace CarAds.Hubs
{
    public class CarAdHub : Hub
    {
        // ✅ شمارنده کاربران آنلاین (thread-safe)
        private static int _connectedClients = 0;

        public override async Task OnConnectedAsync()
        {
            // ✅ افزایش شمارنده آنلاین
            var count = System.Threading.Interlocked.Increment(ref _connectedClients);
            await Clients.All.SendAsync("OnlineCount", count);

            var userId = Context.User?.FindFirst("userId")?.Value;
            if (!string.IsNullOrWhiteSpace(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"User:{userId}");
            }

            var role = Context.User?.FindFirst(ClaimTypes.Role)?.Value;
            if (role == "Admin" || role == "SuperAdmin")
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // ✅ کاهش شمارنده آنلاین
            var count = System.Threading.Interlocked.Decrement(ref _connectedClients);
            if (count < 0)
            {
                System.Threading.Interlocked.Increment(ref _connectedClients);
                count = 0;
            }
            await Clients.All.SendAsync("OnlineCount", count);

            await base.OnDisconnectedAsync(exception);
        }

        // ✅ هر کسی (حتی مهمان) وقتی صفحه نمایش کاربر رو باز کرد، join می‌کنه
        public Task JoinProfile(string userId) =>
            Groups.AddToGroupAsync(Context.ConnectionId, $"Profile:{userId}");

        public Task LeaveProfile(string userId) =>
            Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Profile:{userId}");

        // ✅ برگرداندن تعداد فعلی آنلاین به کاربر جدید
        public Task GetOnlineCount() =>
            Clients.Caller.SendAsync("OnlineCount", _connectedClients);
    }
}