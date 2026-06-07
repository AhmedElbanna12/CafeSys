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

        //[HttpGet]
        //public async Task<IActionResult> GetAllOrders()
        //{
        //    var orders = await _context.Orders
        //        .Include(o => o.OrderItems)
        //            .ThenInclude(oi => oi.Modifiers)
        //        .Include(o => o.User)
        //        .OrderByDescending(o => o.Id)
        //        .ToListAsync();

        //    return Ok(orders);
        //}

        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Modifiers)
                        .ThenInclude(m => m.ModifierOption)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductSize)
                .Include(o => o.User)
                .OrderByDescending(o => o.Id)
                .Select(o => new
                {
                    o.Id,
                    o.UserId,
                    o.SubTotal,
                    o.DiscountAmount,
                    o.TotalAmount,
                    o.PointsEarned,
                    o.PointsRedeemed,
                    o.CreatedAt,
                    o.OrderStatus,
                    o.PaymentStatus,
                    o.PaymentMethod,
                    o.ShippingAddress,
                    o.OrderType,
                    o.DeliveryFee,
                    o.IsRewardOrder,

                    User = o.User,

                    OrderItems = o.OrderItems.Select(oi => new
                    {
                        oi.Id,
                        oi.OrderId,
                        oi.ProductId,
                        oi.ProductNameAr,
                        oi.ProductNameEn,
                        // Size Name
                        oi.ProductSizeId,
                        // SizeName = oi.ProductSize.Name,
                        SizeNameAr = oi.ProductSize != null ? oi.ProductSize.NameAr : null,
                        SizeNameEn = oi.ProductSize != null ? oi.ProductSize.NameEn : null,

                        oi.Quantity,
                        oi.UnitPrice,
                        oi.DiscountAmount,
                        oi.TotalPrice,

                        // Modifiers
                        Modifiers = oi.Modifiers.Select(m => new
                        {
                            // بدل الـ IDs
                            ModifierOptionNameAr = m.ModifierOption.NameAr,
                            ModifierOptionNameEn = m.ModifierOption.NameEn,
                            m.Price
                        })
                    })
                })
                .ToListAsync();

            return Ok(orders);
        }



        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int id, OrderStatus status)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            // ✅ مهم: نتأكد إن دي أول مرة يتحول لـ Completed
            if (order.OrderStatus != OrderStatus.Completed && status == OrderStatus.Completed)
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
                            TotalPoints = 0,
                            UsedPoints = 0
                        };

                        _context.UserPoints.Add(userPoints);
                    }

                    // ✅ إضافة النقاط
                    userPoints.TotalPoints += order.PointsEarned;

                    // ✅ تسجيل Transaction (دي كانت ناقصة)
                    var transactionExists = await _context.PointsTransactions
                        .AnyAsync(t => t.OrderId == order.Id && t.Type == "Earn");

                    if (!transactionExists)
                    {
                        var transaction = new PointsTransaction
                        {
                            UserId = order.UserId,
                            OrderId = order.Id,
                            Points = order.PointsEarned,
                            Type = "Earn",
                            //  WeekStartDate = DateTime.UtcNow.Date.AddDays(-(int)DateTime.UtcNow.DayOfWeek)
                        };

                        _context.PointsTransactions.Add(transaction);
                    }
                }

                // الدفع عند الاستلام
                if (order.PaymentMethod == PaymentMethod.CashOnDelivery)
                {
                    order.PaymentStatus = PaymentStatus.Paid;
                }
            }

            // تحديث الحالة في الآخر
            order.OrderStatus = status;

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
