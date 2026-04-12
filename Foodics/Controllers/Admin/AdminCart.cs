using Foodics.Dtos.Cart.Cart;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POSSystem.Data;

namespace Foodics.Controllers.Admin
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminCart : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AdminCart(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("all")]
        public async Task<IActionResult> GetAllCarts()
        {
            var carts = await _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(i => i.Modifiers)
                        .ThenInclude(m => m.ModifierOption)
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                .ToListAsync();

            var cartDtos = carts.Select(c => new CartDto
            {
                Id = c.Id,
                UserId = c.UserId,
                SubTotal = c.SubTotal,
                Discount = c.Discount,
                Total = c.Total,
                PromoCode = c.PromoCode,
                Items = c.Items.Select(i => new CartItemDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = i.Product.Name,
                    Price = i.Price,
                    Quantity = i.Quantity,
                    ProductSizeId = i.ProductSizeId,
                    ProductSizeName = i.ProductSize != null ? i.ProductSize.Name : null,
                    Modifiers = i.Modifiers.Select(m => new CartItemModifierDto
                    {
                        Id = m.Id,
                        ModifierOptionId = m.ModifierOptionId,
                        ModifierOptionName = m.ModifierOption != null ? m.ModifierOption.Name : null,
                        Price = m.Price
                    }).ToList()
                }).ToList()
            }).ToList();

            return Ok(cartDtos);
        }
    }
}
