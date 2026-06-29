using Foodics.Dtos.Admin.Product;
using Foodics.Dtos.Admin.Product.Product;
using Foodics.Dtos.Userproduct;
using Foodics.ExtensionMethod;
using Foodics.Models;
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

        // 🔹 Get language from query (?lang=ar OR ?lang=en)
        private string GetLang()
        {
            var lang = HttpContext.Request.Query["lang"].ToString();
            return string.IsNullOrWhiteSpace(lang) ? "en" : lang.ToLower();
        }

        private bool IsDiscountActive(Product product)
        {
            if (!product.DiscountPercentage.HasValue ||
                !product.DiscountStart.HasValue ||
                !product.DiscountEnd.HasValue)
                return false;

            var now = DateTime.UtcNow.AddHours(3);

            return now >= product.DiscountStart.Value &&
                   now <= product.DiscountEnd.Value;
        }

        // 🔹 Get all products
        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            var lang = GetLang();
            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            var products = await _context.Products
                .Where(p => !p.IsDeleted)
                .Include(p => p.Category)
                .Include(p => p.Sizes)
                .Include(p => p.ModifierGroups)
                    .ThenInclude(g => g.Options)
                .ToListAsync();

            var result = products.Select(product =>
            {
                var isDiscountActive = IsDiscountActive(product);

                return new ProductResponseDto
                {
                    Id = product.Id,

                    Name = LocalizationExtensions.Localize(
                        product.NameAr,
                        product.NameEn,
                        lang),

                    Description = LocalizationExtensions.Localize(
                        product.DescriptionAr,
                        product.DescriptionEn,
                        lang),

                    Price = product.Price,
                    Calories = product.Calories,
                    PointsReward = product.PointsReward,

                    ImageUrl = string.IsNullOrEmpty(product.ImageUrl)
                        ? null
                        : $"{baseUrl}{product.ImageUrl}",

                    IsAvailable = product.IsAvailable,

                    CategoryName = LocalizationExtensions.Localize(
                        product.Category?.NameAr,
                        product.Category?.NameEn,
                        lang),

                    Sizes = product.Sizes.Select(s => new Dtos.Admin.Product.ProductSize.ProductSizeDto
                    {
                        Id = s.Id,

                        Name = LocalizationExtensions.Localize(
                            s.NameAr,
                            s.NameEn,
                            lang),

                        Price = s.Price,
                        IsDefault = s.IsDefault
                    }).ToList(),

                    ModifierGroups = product.ModifierGroups?.Select(g => new Dtos.Admin.Product.ProductModifierGroup.ModifierGroupDto
                    {
                        Id = g.Id,

                        Name = LocalizationExtensions.Localize(
                            g.NameAr,
                            g.NameEn,
                            lang),

                        IsRequired = g.IsRequired,
                        MaxSelections = g.MaxSelections,

                        Options = g.Options.Select(o => new Dtos.Admin.Product.ProductModifierOption.ModifierOptionDto
                        {
                            Id = o.Id,

                            Name = LocalizationExtensions.Localize(
                                o.NameAr,
                                o.NameEn,
                                lang),

                            ExtraPrice = o.ExtraPrice
                        }).ToList()
                    }).ToList(),

                    DiscountedPrice = isDiscountActive
                        ? CalculateDiscountedPrice(product)
                        : null,

                    DiscountPercentage = isDiscountActive
                        ? product.DiscountPercentage
                        : null,

                    DiscountStart = product.DiscountStart,
                    DiscountEnd = product.DiscountEnd
                };
            });

            return Ok(result);
        }

        // 🔹 Menu grouped by category
        [HttpGet("menu")]
        public async Task<IActionResult> GetMenu()
        {
            var lang = GetLang();
            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            var data = await _context.Categories
                .Include(c => c.Products.Where(p => p.IsAvailable && !p.IsDeleted))
                .Select(c => new
                {
                    categoryId = c.Id,

                    categoryName = LocalizationExtensions.Localize(
                        c.NameAr,
                        c.NameEn,
                        lang),

                    products = c.Products.Select(p => new ProductListDto
                    {
                        Id = p.Id,

                        Name = LocalizationExtensions.Localize(
                            p.NameAr,
                            p.NameEn,
                            lang),

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
                return product.Price -
                       (product.Price * product.DiscountPercentage.Value / 100);
            }

            return product.Price;
        }
    }
}