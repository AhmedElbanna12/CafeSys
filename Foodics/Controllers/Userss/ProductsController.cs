using Foodics.Dtos.Admin.Product;
using Foodics.Dtos.Userproduct;
using Foodics.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POSSystem.Data;

namespace Foodics.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetProducts()
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            var products = await _context.Products
                .Where(p => p.IsAvailable)
                .Include(p => p.Category)
                .Include(p => p.Sizes)
                .Include(p => p.ModifierGroups)
                    .ThenInclude(g => g.Options)
                .ToListAsync();

            var result = products.Select(product => new ProductResponseDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,

                // 🔥 Discount Info
                DiscountPercentage = product.DiscountPercentage,
                DiscountStart = product.DiscountStart,
                DiscountEnd = product.DiscountEnd,

                DiscountedPrice = CalculateDiscountedPrice(product),

                Calories = product.Calories,
                PointsReward = product.PointsReward,

                ImageUrl = string.IsNullOrEmpty(product.ImageUrl)
                    ? null
                    : $"{baseUrl}{product.ImageUrl}",

                CategoryName = product.Category?.Name,

                Sizes = product.Sizes.Select(s => new ProductSizeDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Price = s.Price,
                    IsDefault = s.IsDefault
                }).ToList(),

                ModifierGroups = product.ModifierGroups.Select(g => new ModifierGroupDto
                {
                    Id = g.Id,
                    Name = g.Name,
                    IsRequired = g.IsRequired,
                    MaxSelections = g.MaxSelections,
                    Options = g.Options.Select(o => new ModifierOptionDto
                    {
                        Id = o.Id,
                        Name = o.Name,
                        ExtraPrice = o.ExtraPrice
                    }).ToList()
                }).ToList()
            });

            return Ok(result);
        }

        // 🔹 Get products grouped by category (Menu)
        [HttpGet("menu")]
        public async Task<IActionResult> GetMenu()
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            var data = await _context.Categories
                .Include(c => c.Products.Where(p => p.IsAvailable))
                .Select(c => new
                {
                    categoryId = c.Id,
                    categoryName = c.Name,

                    products = c.Products.Select(p => new ProductListDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Price = p.Price,
                        DiscountedPrice = CalculateDiscountedPrice(p),
                        ImageUrl = string.IsNullOrEmpty(p.ImageUrl)
                            ? null
                            : $"{baseUrl}{p.ImageUrl}"
                    }).ToList()
                })
                .ToListAsync();

            return Ok(data);
        }

        // 🔹 Discount logic
        private decimal CalculateDiscountedPrice(Product product)
        {
            if (product.DiscountPercentage.HasValue)
            {
                return product.Price - (product.Price * product.DiscountPercentage.Value / 100);
            }

            return product.Price;
        }
    }
}