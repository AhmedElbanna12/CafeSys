using FirebaseAdmin.Messaging;
using Microsoft.EntityFrameworkCore;
using POSSystem.Data;

namespace Foodics.Services
{
    public class FcmService
    {

        private readonly ApplicationDbContext _context;

        public FcmService(ApplicationDbContext context)
        {
            _context = context;
        }

        private async Task RemoveTokenFromDb(string token)
        {
            var device = await _context.UserDevices
                .FirstOrDefaultAsync(x => x.DeviceToken == token);

            if (device != null)
            {
                device.DeviceToken = null; // أو احذفه تمامًا
                                        // _context.UserDevices.Remove(device); // اختار الأسلوب المناسب

                await _context.SaveChangesAsync();
            }
        }
        public async Task SendAsync(string token, string title, string body, Dictionary<string, string> data)
        {
            var message = new Message
            {
                Token = token,
                Notification = new Notification
                {
                    Title = title,
                    Body = body
                },
                Data = data
            };

            try
            {
                await FirebaseMessaging.DefaultInstance.SendAsync(message);
            }
            catch (FirebaseMessagingException ex)
            {
                if (ex.MessagingErrorCode == MessagingErrorCode.Unregistered ||
                    ex.MessagingErrorCode == MessagingErrorCode.InvalidArgument)
                {
                    // ❌ token بايظ → احذفه من DB
                    await RemoveTokenFromDb(token);
                    return; // مهم: متكملش throw
                }

                // غير كده اعمل throw عادي
                throw;
            }
        }
    }
}
