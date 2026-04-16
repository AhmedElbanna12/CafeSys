using Foodics.Dtos.Admin.notification;
using Foodics.Hub;
using Foodics.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using POSSystem.Data;

[Route("api/[controller]")]
[ApiController]
public class NotificationsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IHubContext<NotificationHub> _hub;

    public NotificationsController(ApplicationDbContext context, IHubContext<NotificationHub> hub)
    {
        _context = context;
        _hub = hub;
    }

    // =====================
    // ارسال لكل المستخدمين
    // =====================
    [Authorize(Roles = "Admin")]
    [HttpPost("send-to-all")]
    public async Task<IActionResult> SendToAll([FromBody] NotificationDto dto)
    {
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
        await _hub.Clients.All.SendAsync("ReceiveNotification", dto.Title, dto.Body);
        return Ok("Notification sent to all users");
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
            Title = dto.Title,
            Body = dto.Body
        });
        await _context.SaveChangesAsync();
        await _hub.Clients.User(dto.UserId).SendAsync("ReceiveNotification", dto.Title, dto.Body);
        return Ok("Notification sent to user");
    }

    // =====================
    // جلب Notifications لمستخدم
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
                n.IsRead,       // ✅ مهم للـ Flutter
                n.CreatedAt
            })
            .ToListAsync();

        return Ok(notifications);
    }

    // =====================
    // ✅ عدد الغير مقروءة (للـ Badge)
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
    // ✅ تحديد كـ مقروءة
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
    // ✅ تحديد الكل كـ مقروءة
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
    // ✅ حذف Notification
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
}