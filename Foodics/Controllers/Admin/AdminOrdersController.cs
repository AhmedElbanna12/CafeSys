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


        [HttpGet("search")]
        public async Task<IActionResult> SearchOrders(
            string? search = null,
            int page = 1,
            int pageSize = 10)
        {
            if (page < 1)
                page = 1;

            if (pageSize < 1)
                pageSize = 10;

            if (pageSize > 100)
                pageSize = 100;

            var query = _context.Orders
                .AsNoTracking()
                .Where(o => true);

            // Server-side search
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                if (int.TryParse(search, out int orderId))
                {
                    query = query.Where(o =>
                        o.Id == orderId ||
                        o.PhoneNumber.Contains(search) ||
                        o.User.UserName.Contains(search));
                }
                else
                {
                    query = query.Where(o =>
                        o.PhoneNumber.Contains(search) ||
                        o.User.UserName.Contains(search));
                }
            }

            // Total count after search
            var totalCount = await query.CountAsync();

            // Pagination
            var orders = await query
                .OrderByDescending(o => o.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
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

                    o.City,
                    o.Street,
                    o.BuildingNumber,
                    o.FloorNumber,
                    o.ApartmentNumber,
                    o.Landmark,
                    o.PhoneNumber,

                    o.PromoCode,
                    o.PromoDiscountPercentage,

                    o.Latitude,
                    o.Longitude,
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

                        oi.ProductSizeId,

                        SizeNameAr = oi.ProductSize != null
                            ? oi.ProductSize.NameAr
                            : null,

                        SizeNameEn = oi.ProductSize != null
                            ? oi.ProductSize.NameEn
                            : null,

                        oi.Comment,
                        oi.Quantity,
                        oi.UnitPrice,
                        oi.DiscountAmount,
                        oi.TotalPrice,

                        Modifiers = oi.Modifiers.Select(m => new
                        {
                            ModifierOptionNameAr = m.ModifierOption.NameAr,
                            ModifierOptionNameEn = m.ModifierOption.NameEn,
                            m.Quantity,
                            UnitPrice = m.Price,
                            TotalPrice = m.Price * m.Quantity
                        })
                    })
                })
                .ToListAsync();

            var totalPages = (int)Math.Ceiling(
                (double)totalCount / pageSize
            );

            return Ok(new
            {
                data = orders,

                pagination = new
                {
                    currentPage = page,
                    pageSize,
                    totalCount,
                    totalPages,
                    hasNextPage = page < totalPages,
                    hasPreviousPage = page > 1
                }
            });
        }

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
                    //o.ShippingAddress,
                    o.City,
                    o.Street,
                    o.BuildingNumber,
                    o.FloorNumber,
                    o.ApartmentNumber,
                    o.Landmark,
                    o.PhoneNumber,


                    o.PromoCode,
                    o.PromoDiscountPercentage,

                    o.Latitude,
                    o.Longitude,
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


                        oi.Comment,  

                        oi.Quantity,
                        oi.UnitPrice,
                        oi.DiscountAmount,
                        oi.TotalPrice,

                        // Modifiers
                        Modifiers = oi.Modifiers.Select(m => new
                        {
                            ModifierOptionNameAr = m.ModifierOption.NameAr,
                            ModifierOptionNameEn = m.ModifierOption.NameEn,
                            m.Quantity,
                            UnitPrice = m.Price,
                            TotalPrice = m.Price * m.Quantity
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
