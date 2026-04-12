using Microsoft.AspNet.SignalR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POSSystem.Data;
using System.Security.Claims;

namespace Foodics.Controllers.User
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserOrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UserOrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        private string? GetUserId() => User.FindFirstValue("userId");

        // 📦 Get My Orders
        [HttpGet]
        public async Task<IActionResult> GetMyOrders()
        {
            var userId = GetUserId();

            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.Id)
                .ToListAsync();

            return Ok(orders);
        }

        // 📄 Order Details
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderDetails(int id)
        {
            var userId = GetUserId();

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(i => i.Modifiers)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

            if (order == null) return NotFound();

            return Ok(order);
        }
    }
}
