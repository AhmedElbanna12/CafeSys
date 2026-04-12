using Foodics.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POSSystem.Data;
using AppUser = Foodics.Models.User;


namespace Foodics.Controllers.Admin
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]

    public class AdminOrdersController : ControllerBase
    {

        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public AdminOrdersController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // 📋 كل الأوردرات
        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.User)
                .OrderByDescending(o => o.Id)
                .ToListAsync();

            return Ok(orders);
        }

        // 🔄 تحديث حالة الأوردر
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int id, OrderStatus status)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            order.OrderStatus = status;

            // ✅ لو الأوردر اتكمل → نضيف النقاط
            if (status == OrderStatus.Completed)
            {
                var user = await _userManager.FindByIdAsync(order.UserId);

                if (user != null)
                {
                    var userPoints = await _context.UserPoints
                        .FirstOrDefaultAsync(up => up.UserId == order.UserId);

                    if (userPoints == null)
                    {
                        userPoints = new UserPoints
                        {
                            UserId = order.UserId,
                            TotalPoints = order.PointsEarned,
                            UsedPoints = 0
                        };

                        _context.UserPoints.Add(userPoints);
                    }
                    else
                    {
                        userPoints.TotalPoints += order.PointsEarned;
                    }
                }

                // لو الدفع عند الاستلام → يبقى Paid
                if (order.PaymentMethod == PaymentMethod.CashOnDelivery)
                {
                    order.PaymentStatus = PaymentStatus.Paid;
                }
            }

            await _context.SaveChangesAsync();

            return Ok(order);
        }

        // 💰 تحديث حالة الدفع
        [HttpPut("{id}/payment")]
        public async Task<IActionResult> UpdatePaymentStatus(int id, PaymentStatus paymentStatus)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            order.PaymentStatus = paymentStatus;

            await _context.SaveChangesAsync();

            return Ok(order);
        }
    }
}
