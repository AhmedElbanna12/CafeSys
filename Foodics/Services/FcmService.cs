using FirebaseAdmin.Messaging;

namespace Foodics.Services
{
    public class FcmService
    {
        public async Task SendAsync(string token, string title, string body, Dictionary<string, string>? data = null)
        {
            var message = new FirebaseAdmin.Messaging.Message()
            {
                Token = token,
                Notification = new FirebaseAdmin.Messaging.Notification
                {
                    Title = title,
                    Body = body
                },
                Data = data
            };

            await FirebaseAdmin.Messaging.FirebaseMessaging.DefaultInstance.SendAsync(message);
        
    }
    }
}
