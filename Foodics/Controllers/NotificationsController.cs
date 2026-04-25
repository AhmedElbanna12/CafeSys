using Foodics.Dtos.Admin.notification;
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
    public async Task<IActionResult> SendToAll([FromBody] NotificationDto dto)
    {
        // 1. خزن في DB (اختياري لكن مهم)
        var users = await _context.Users.ToListAsync();

        foreach (var user in users)
        {
            _context.Notifications.Add(new Notification
            {
                UserId = user.Id,
                Title = dto.Title,
                Body = dto.Body
            });
        }

        await _context.SaveChangesAsync();

        // 2. هات كل التوكنز
        var tokens = await _context.UserDevices
            .Select(x => x.DeviceToken)
            .ToListAsync();

        // 3. ابعت لكل الأجهزة
        foreach (var token in tokens)
        {
            await _fcmService.SendAsync(
                token,
                dto.Title,
                dto.Body,
                new Dictionary<string, string>
                {
                { "type", "admin_message" }
                });
        }

        return Ok("Notification sent to all users");
    }

    // =====================
    // ارسال لمستخدم معين
    // =====================
    [Authorize(Roles = "Admin")]
    [HttpPost("send-to-user")]
    public async Task<IActionResult> SendToUser([FromBody] SendToUserDto dto)
    {
        // حفظ في DB
        _context.Notifications.Add(new Notification
        {
            UserId = dto.UserId,
            Title = dto.Title,
            Body = dto.Body
        });

        await _context.SaveChangesAsync();

        // SignalR (لو فاتح)
        await _hub.Clients.User(dto.UserId)
            .SendAsync("ReceiveNotification", dto.Title, dto.Body);

        // FCM (الأساس)
        var tokens = await _context.UserDevices
            .Where(x => x.UserId == dto.UserId)
            .Select(x => x.DeviceToken)
            .ToListAsync();

        foreach (var token in tokens)
        {
            await _fcmService.SendAsync(token, dto.Title, dto.Body,
                new Dictionary<string, string>
                {
                    { "type", "user" },
                    { "userId", dto.UserId }
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
        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new
            {
                n.Id,
                n.Title,
                n.Body,
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
        var userId = User.FindFirst("userId")?.Value;
        var exists = await _context.UserDevices
            .AnyAsync(x => x.DeviceToken == dto.Token);

        if (!exists)
        {
            _context.UserDevices.Add(new UserDevice
            {
                UserId = userId,
                DeviceToken = dto.Token
            });

            await _context.SaveChangesAsync();
        }

        return Ok();
    }

    [HttpPost("test-notification")]
    public async Task<IActionResult> TestNotification([FromBody] string userId)
    {
        var tokens = await _context.UserDevices
            .Where(x => x.UserId == userId)
            .Select(x => x.DeviceToken)
            .ToListAsync();

        foreach (var token in tokens)
        {
            await _fcmService.SendAsync(
                token,
                " Test Notification",
                "لو وصلتك الرسالة يبقى كله شغال"
            );
        }

        return Ok("Sent");
    }
}