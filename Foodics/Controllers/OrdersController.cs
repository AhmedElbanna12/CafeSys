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

                    DiscountAmount = o.DiscountAmount,

                    PromoCode = o.PromoCode,

                    PromoDiscountPercentage = o.PromoDiscountPercentage,

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