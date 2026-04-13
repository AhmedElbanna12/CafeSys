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
            var notification = new Notification
            {
                UserId = user.Id,
                Title = dto.Title,
                Body = dto.Body
            };
            _context.Notifications.Add(notification);
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
        var notification = new Notification
        {
            UserId = dto.UserId,
            Title = dto.Title,
            Body = dto.Body
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        await _hub.Clients.User(dto.UserId).SendAsync("ReceiveNotification", dto.Title, dto.Body);

        return Ok("Notification sent to user");
    }

    // =====================
    // جلب Notifications لمستخدم معين
   
   // =====================
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserNotifications(string userId)
    {
        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

        return Ok(notifications);
    }
}

