using Microsoft.AspNetCore.SignalR;

namespace Foodics.Hub
{
    // استخدم الاسم الكامل لتجنب التعارض
    public class NotificationHub : Microsoft.AspNetCore.SignalR.Hub
    {
        public async Task SendToUser(string userId, string title, string body)
        {
            await Clients.User(userId).SendAsync("ReceiveNotification", title, body);
        }

        public async Task SendToAll(string title, string body)
        {
            await Clients.All.SendAsync("ReceiveNotification", title, body);
        }
    }
}