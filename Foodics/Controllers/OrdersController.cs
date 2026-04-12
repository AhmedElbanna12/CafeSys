//using Foodics.Dtos.Cart.Order;
//using Foodics.ExtensionMethod;
//using Foodics.Models;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using POSSystem.Data;

//namespace Foodics.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    public class OrdersController : ControllerBase
//    {
//        private readonly ApplicationDbContext _context;

//        public OrdersController(ApplicationDbContext context)
//        {
//            _context = context;
//        }

//        // 🛒 Get all orders for current user
//        [HttpGet("my-orders")]
//        public async Task<IActionResult> GetMyOrders()
//        {
//            var userId = User.FindFirst("uid")?.Value;

//            var orders = await _context.Orders
//                .Include(o => o.OrderItems)
//                    .ThenInclude(oi => oi.Modifiers)
//                .Where(o => o.UserId == userId)
//                .OrderByDescending(o => o.CreatedAt)
//                .ToListAsync();

//            return Ok(orders);
//        }

//        // 📄 Get order by Id
//        [HttpGet("{id}")]
//        public async Task<IActionResult> GetOrderById(int id)
//        {
//            var userId = User.FindFirst("uid")?.Value;

//            var order = await _context.Orders
//                .Include(o => o.OrderItems)
//                    .ThenInclude(oi => oi.Modifiers)
//                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

//            if (order == null) return NotFound("Order not found");

//            return Ok(order);
//        }

//        // ✅ Admin: update order status (e.g., Completed / Cancelled)
//        [Authorize(Roles = "Admin")]
//        [HttpPost("update-status")]
//        public async Task<IActionResult> UpdateOrderStatus(UpdateOrderStatusDto dto)
//        {
//            var order = await _context.Orders
//                .Include(o => o.User)
//                .FirstOrDefaultAsync(o => o.Id == dto.OrderId);

//            if (order == null) return NotFound("Order not found");

//            order.OrderStatus = dto.OrderStatus;

//            // ✅ فقط لو الدفع عند الاستلام وتم اكتمال الطلب
//            if (order.PaymentMethod == PaymentMethod.CashOnDelivery && order.OrderStatus == OrderStatus.Completed)
//            {
//                order.PaymentStatus = PaymentStatus.Paid;

//                // جلب نقاط المستخدم أو انشائها لو مش موجودة
//                var userPoints = await _context.UserPoints.FirstOrDefaultAsync(up => up.UserId == order.UserId);
//                if (userPoints == null)
//                {
//                    userPoints = new UserPoints
//                    {
//                        UserId = order.UserId,
//                        TotalPoints = 0,
//                        UsedPoints = 0
//                    };
//                    _context.UserPoints.Add(userPoints);
//                }

//                // زيادة النقاط
//                userPoints.TotalPoints += order.PointsEarned;

//                // اضافة سجل بالمعاملة
//                var transaction = new PointsTransaction
//                {
//                    UserId = order.UserId,
//                    OrderId = order.Id,
//                    Points = order.PointsEarned,
//                    Type = "Earn",
//                    WeekStartDate = DateTime.UtcNow.StartOfWeek(DayOfWeek.Sunday)
//                };
//                _context.PointsTransactions.Add(transaction);
//            }

//            await _context.SaveChangesAsync();
//            return Ok(order);

//        }
//    }
//}