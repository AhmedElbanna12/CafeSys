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

using Foodics.Dtos.Admin.Orders;
using Foodics.ExtensionMethod;
using Foodics.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POSSystem.Data;
using System.Security.Claims;

namespace Foodics.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================
        // 🌍 Language Helper
        // =========================
        private string GetLang()
        {
            var langHeader = Request.Headers["Accept-Language"].ToString();

            return langHeader.StartsWith("ar") ? "ar" : "en";
        }

        // =========================
        // 👤 User Helper
        // =========================
        private string? GetUserId() =>
            User.FindFirstValue("userId");

        // =========================
        // 🛒 Get My Orders
        // =========================
        [HttpGet("my-orders")]
        public async Task<IActionResult> GetMyOrders()
        {
            var userId = GetUserId();
            var lang = GetLang();

            var orders = await _context.Orders
                .AsNoTracking()
                .Include(o => o.OrderItems)
                    .ThenInclude(i => i.Modifiers)
                        .ThenInclude(m => m.ModifierOption)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new OrderResponseDto
                {
                    OrderId = o.Id,

                    CustomerName = o.User.UserName,
                    CustomerPhone = o.User.PhoneNumber,

                    TotalAmount = o.TotalAmount,
                    PointsEarned = o.PointsEarned,
                    CreatedAt = o.CreatedAt,

                    Items = o.OrderItems.Select(i => new OrderItemResponseDto
                    {
                        ProductName = LocalizationExtensions.Localize(
                            i.ProductNameAr,
                            i.ProductNameEn,
                            lang),

                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice,
                        TotalPrice = i.TotalPrice,

                        Modifiers = i.Modifiers.Select(m => new OrderItemModifierDto
                        {
                            Name = LocalizationExtensions.Localize(
                                m.ModifierOption.NameAr,
                                m.ModifierOption.NameEn,
                                lang),

                            ExtraPrice = m.Price
                        }).ToList()
                    }).ToList()
                })
                .ToListAsync();

            return Ok(orders);
        }

        // =========================
        // 📄 Get Order By Id
        // =========================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            var userId = GetUserId();
            var lang = GetLang();

            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(i => i.Modifiers)
                        .ThenInclude(m => m.ModifierOption)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

            if (order == null)
                return NotFound("Order not found");

            var result = new OrderResponseDto
            {
                OrderId = order.Id,

                CustomerName = order.User?.UserName,
                CustomerPhone = order.User?.PhoneNumber,

                TotalAmount = order.TotalAmount,
                PointsEarned = order.PointsEarned,
                CreatedAt = order.CreatedAt,

                Items = order.OrderItems.Select(i => new OrderItemResponseDto
                {
                    ProductName = LocalizationExtensions.Localize(
                        i.ProductNameAr,
                        i.ProductNameEn,
                        lang),

                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    TotalPrice = i.TotalPrice,

                    Modifiers = i.Modifiers.Select(m => new OrderItemModifierDto
                    {
                        Name = LocalizationExtensions.Localize(
                            m.ModifierOption.NameAr,
                            m.ModifierOption.NameEn,
                            lang),

                        ExtraPrice = m.Price
                    }).ToList()
                }).ToList()
            };

            return Ok(result);
        }
    }
}