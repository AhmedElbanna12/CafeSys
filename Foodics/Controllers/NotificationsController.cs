using FirebaseAdmin;
using Foodics.Dtos.Admin.notification;
using Foodics.ExtensionMethod;
using Foodics.Hub;
using Foodics.Models;
using Foodics.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using POSSystem.Data;
using System.Security.Claims;

[Route("api/[controller]")]
[ApiController]
public class NotificationsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IHubContext<NotificationHub> _hub;
    private readonly FcmService _fcmService;

    public NotificationsController(
        ApplicationDbContext context,
        IHubContext<NotificationHub> hub,
        FcmService fcmService)
    {
        _context = context;
        _hub = hub;
        _fcmService = fcmService;
    }


    private string GetLang()
    {
        var langHeader = Request.Headers["Accept-Language"].ToString();

        return langHeader
            .Split(',')[0]
            .Trim()
            .ToLower()
            .StartsWith("ar")
            ? "ar"
            : "en";
    }

    // =====================
    // ارسال لكل المستخدمين
    // =====================
    //[Authorize(Roles = "Admin")]
    //[HttpPost("send-to-all")]
    //public async Task<IActionResult> SendToAll([FromBody] NotificationDto dto)
    //{
    //    var users = await _context.Users.ToListAsync();

    //    foreach (var user in users)
    //    {
    //        _context.Notifications.Add(new Notification
    //        {
    //            UserId = user.Id,
    //            Title = dto.Title,
    //            Body = dto.Body
    //        });
    //    }

    //    await _context.SaveChangesAsync();

    //    // SignalR
    //    await _hub.Clients.All.SendAsync("ReceiveNotification", dto.Title, dto.Body);

    //    // FCM
    //    var tokens = await _context.UserDevices
    //        .Select(x => x.DeviceToken)
    //        .ToListAsync();

    //    foreach (var token in tokens)
    //    {
    //        await _fcmService.SendAsync(token, dto.Title, dto.Body,
    //            new Dictionary<string, string>
    //            {
    //                { "type", "general" }
    //            });
    //    }

    //    return Ok("Notification sent to all users");
    //}
    [Authorize(Roles = "Admin")]
    [HttpPost("send-to-all")]
    public async Task<IActionResult> SendToAll([FromBody] CreateNotificationDto dto)
    {
        var users = await _context.Users.ToListAsync();

        foreach (var user in users)
        {
            _context.Notifications.Add(new Notification
            {
                UserId = user.Id,

                TitleAr = dto.TitleAr,
                TitleEn = dto.TitleEn,

                BodyAr = dto.BodyAr,
                BodyEn = dto.BodyEn
            });
        }

        await _context.SaveChangesAsync();

        var tokens = await _context.UserDevices
            .Where(x => !string.IsNullOrEmpty(x.DeviceToken))
            .Select(x => x.DeviceToken)
            .ToListAsync();

        if (!tokens.Any())
            return Ok("Notifications saved. No device tokens found.");

        int sent = 0, failed = 0;

        foreach (var token in tokens)
        {
            try
            {
                await _fcmService.SendAsync(
                    token,
                    dto.TitleEn,
                    dto.BodyEn,
                    new Dictionary<string, string>
                    {
                    { "type", "admin_message" },
                    { "titleAr", dto.TitleAr },
                    { "titleEn", dto.TitleEn },
                    { "bodyAr", dto.BodyAr },
                    { "bodyEn", dto.BodyEn }
                    });

                sent++;
            }
            catch
            {
                failed++;
            }
        }

        return Ok($"Done. Sent: {sent}, Failed: {failed}");
    }
    // =====================
    // ارسال لمستخدم معين
    // =====================
    [Authorize(Roles = "Admin")]
    [HttpPost("send-to-user")]
    public async Task<IActionResult> SendToUser([FromBody] SendToUserDto dto)
    {
        _context.Notifications.Add(new Notification
        {
            UserId = dto.UserId,

            TitleAr = dto.TitleAr,
            TitleEn = dto.TitleEn,

            BodyAr = dto.BodyAr,
            BodyEn = dto.BodyEn
        });

        await _context.SaveChangesAsync();

        await _hub.Clients.User(dto.UserId)
            .SendAsync(
                "ReceiveNotification",
                dto.TitleAr,
                dto.TitleEn,
                dto.BodyAr,
                dto.BodyEn);

        var tokens = await _context.UserDevices
            .Where(x => x.UserId == dto.UserId)
            .Select(x => x.DeviceToken)
            .ToListAsync();

        foreach (var token in tokens)
        {
            await _fcmService.SendAsync(
                token,
                dto.TitleEn,
                dto.BodyEn,
                new Dictionary<string, string>
                {
                { "type", "user" },
                { "userId", dto.UserId },

                { "titleAr", dto.TitleAr },
                { "titleEn", dto.TitleEn },

                { "bodyAr", dto.BodyAr },
                { "bodyEn", dto.BodyEn }
                });
        }

        return Ok("Notification sent to user");
    }
    // =====================
    // جلب Notifications
    // =====================
    [Authorize]
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserNotifications(string userId)
    {
        var lang = GetLang();

        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new
            {
                n.Id,

                Title = LocalizationExtensions.Localize(
                    n.TitleAr,
                    n.TitleEn,
                    lang),

                Body = LocalizationExtensions.Localize(
                    n.BodyAr,
                    n.BodyEn,
                    lang),

                n.IsRead,
                n.CreatedAt
            })
            .ToListAsync();

        return Ok(notifications);
    }
    // =====================
    // عدد الغير مقروءة
    // =====================
    [Authorize]
    [HttpGet("unread-count/{userId}")]
    public async Task<IActionResult> GetUnreadCount(string userId)
    {
        var count = await _context.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead);

        return Ok(new { unreadCount = count });
    }

    // =====================
    // تحديد كمقروءة
    // =====================
    [Authorize]
    [HttpPut("mark-as-read/{id}")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var notification = await _context.Notifications.FindAsync(id);
        if (notification == null) return NotFound();

        notification.IsRead = true;
        await _context.SaveChangesAsync();

        return Ok("Marked as read");
    }


    // =====================
    // تحديد الكل كمقروءة
    // =====================
    [Authorize]
    [HttpPut("mark-all-as-read/{userId}")]
    public async Task<IActionResult> MarkAllAsRead(string userId)
    {
        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        notifications.ForEach(n => n.IsRead = true);

        await _context.SaveChangesAsync();

        return Ok("All marked as read");
    }

    // =====================
    // حذف Notification
    // =====================
    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteNotification(int id)
    {
        var notification = await _context.Notifications.FindAsync(id);
        if (notification == null) return NotFound();

        _context.Notifications.Remove(notification);
        await _context.SaveChangesAsync();

        return Ok("Deleted");
    }

    // =====================
    // تسجيل Device Token
    // =====================
    [Authorize]
    [HttpPost("register-device")]
    public async Task<IActionResult> RegisterDevice([FromBody] RegisterDeviceDto dto)
    {
        // ✅ لو الـ token فاضي مترجعش error بس متعملش حاجة
        if (string.IsNullOrEmpty(dto.Token))
            return BadRequest("Device token is required");

        var userId = User.FindFirst("userId")?.Value;

        // ✅ لو الـ token موجود لحد تاني، شيله منه الأول
        var existing = await _context.UserDevices
            .FirstOrDefaultAsync(x => x.DeviceToken == dto.Token);

        if (existing != null)
        {
            existing.UserId = userId; // حوّله للـ user الحالي
            await _context.SaveChangesAsync();
            return Ok();
        }

        // ✅ لو الـ user عنده device قبل كده، حدّثه بدل ما تضيف جديد
        var userDevice = await _context.UserDevices
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (userDevice != null)
        {
            userDevice.DeviceToken = dto.Token;
        }
        else
        {
            _context.UserDevices.Add(new UserDevice
            {
                UserId = userId,
                DeviceToken = dto.Token
            });
        }

        await _context.SaveChangesAsync();
        return Ok();
    }
}

