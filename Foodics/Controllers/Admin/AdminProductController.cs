using Foodics.Dtos.Admin.Product.Product;
using Foodics.ExtensionMethod;
using Foodics.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POSSystem.Data;

namespace Foodics.Controllers.Admin
{
    [Route("api/admin/products")]
    [ApiController]
    [Authorize(Roles = "Admin")]

    public class AdminProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public AdminProductsController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
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



        // =========================
        // CREATE PRODUCT

        // =========================
        [HttpPost]
        [RequestSizeLimit(10_000_000)]
        public async Task<IActionResult> Create([FromForm] CreateProductDto dto)
        {
            string? imageUrl = null;

            if (dto.Image != null)
            {
                var folder = Path.Combine(_env.WebRootPath, "images/products");
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                var fileName = $"{Guid.NewGuid()}_{dto.Image.FileName}";
                var path = Path.Combine(folder, fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await dto.Image.CopyToAsync(stream);
                }

                imageUrl = $"/images/products/{fileName}";
            }

            var product = new Product
            {
                Name = dto.NameEn, // أو خليه default EN
                Description = dto.DescriptionEn,

                NameAr = dto.NameAr,
                NameEn = dto.NameEn,
                DescriptionAr = dto.DescriptionAr,
                DescriptionEn = dto.DescriptionEn,

                Price = dto.Price,
                DiscountPercentage = dto.DiscountPercentage,
                DiscountStart = dto.DiscountStart,
                DiscountEnd = dto.DiscountEnd,
                CategoryId = dto.CategoryId,
                Calories = dto.Calories,
                PointsReward = dto.PointsReward,
                ImageUrl = imageUrl
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return Ok(await Map(product.Id));
        }

        // =========================
        // GET ALL
        // =========================
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var products = await _context.Products
                .Where(p => !p.IsDeleted)
                .ToListAsync();

            var result = new List<object>();

            foreach (var p in products)
                result.Add(await Map(p.Id));

            return Ok(result);
        }

        // =========================
        // GET BY ID
        // =========================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await Map(id);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        // =========================
        // UPDATE PRODUCT
        // =========================
        [HttpPut("{id}")]
        [RequestSizeLimit(10_000_000)]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateProductDto dto)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
                return NotFound();

            if (dto.NameAr != null) product.NameAr = dto.NameAr;
            if (dto.NameEn != null) product.NameEn = dto.NameEn;

            if (dto.DescriptionAr != null) product.DescriptionAr = dto.DescriptionAr;
            if (dto.DescriptionEn != null) product.DescriptionEn = dto.DescriptionEn;

            if (dto.Price.HasValue) product.Price = dto.Price.Value;
            if (dto.CategoryId.HasValue) product.CategoryId = dto.CategoryId.Value;
            if (dto.Calories.HasValue) product.Calories = dto.Calories.Value;
            if (dto.PointsReward.HasValue) product.PointsReward = dto.PointsReward.Value;

            if (dto.DiscountPercentage.HasValue)
                product.DiscountPercentage = dto.DiscountPercentage;

            product.DiscountStart = dto.DiscountStart ?? product.DiscountStart;
            product.DiscountEnd = dto.DiscountEnd ?? product.DiscountEnd;

            // image update
            if (dto.Image != null)
            {
                var folder = Path.Combine(_env.WebRootPath, "images/products");
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                var fileName = $"{Guid.NewGuid()}_{dto.Image.FileName}";
                var path = Path.Combine(folder, fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await dto.Image.CopyToAsync(stream);
                }

                product.ImageUrl = $"/images/products/{fileName}";
            }

            await _context.SaveChangesAsync();

            return Ok(await Map(product.Id));
        }

        // =========================
        // DELETE (SOFT)
        // =========================
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
                return NotFound();

            product.IsDeleted = true;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Deleted successfully" });
        }

        // =========================
        // TOGGLE
        // =========================
        [HttpPatch("{id}/toggle")]
        public async Task<IActionResult> Toggle(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
                return NotFound();

            product.IsAvailable = !product.IsAvailable;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                product.Id,
                product.NameAr,
                product.NameEn,
                product.IsAvailable
            });
        }

        ///// <summary>
        ///// Get Top Selling Products
        ///// </summary>
        ///// <param name="count">Number of products to return (default = 10)</param>
        ///// <param name="days">Filter by last X days (optional)</param>
        //[HttpGet("top-selling")]
        //public async Task<IActionResult> GetTopSellingProducts(int count = 10, int? days = null)
        //{
        //    var query = _context.OrderItems
        //        .Include(oi => oi.Order)
        //        .AsQueryable();

        //    // ✅ فلترة بالتاريخ (اختياري)
        //    if (days.HasValue)
        //    {
        //        var fromDate = DateTime.UtcNow.AddDays(-days.Value);
        //        query = query.Where(oi => oi.Order.CreatedAt >= fromDate);
        //    }

        //    var topProducts = await query
        //        .GroupBy(oi => oi.ProductId)
        //        .Select(g => new
        //        {
        //            ProductId = g.Key,
        //            TotalSold = g.Sum(x => x.Quantity)
        //        })
        //        .OrderByDescending(x => x.TotalSold)
        //        .Take(count)
        //        .Join(_context.Products,
        //              g => g.ProductId,
        //              p => p.Id,
        //              (g, p) => new Dtos.Admin.Product.TopSellingProductDto
        //              {
        //                  Id = p.Id,
        //                  Name = p.Name,
        //                  Price = p.Price,
        //                  ImageUrl = p.ImageUrl,
        //                  TotalSold = g.TotalSold
        //              })
        //        .ToListAsync();

        //    return Ok(topProducts);
        //}


        /// <summary>
        /// Get Top Selling Products
        /// </summary>
        [AllowAnonymous]
        [HttpGet("top-selling")]
        public async Task<IActionResult> GetTopSellingProducts(
            int count = 10,
            int? days = null)
        {
            // 🌍 تحديد اللغة من الهيدر
            var lang = Request.Headers["Accept-Language"].ToString();

            var query = _context.OrderItems
                .Include(oi => oi.Order)
                .Include(oi => oi.Product)
                .AsNoTracking()
                .AsQueryable();

            // 📅 فلترة اختيارية بالوقت
            if (days.HasValue)
            {
                var fromDate = DateTime.UtcNow.AddDays(-days.Value);
                query = query.Where(oi => oi.Order.CreatedAt >= fromDate);
            }

            var topProducts = await query
                .GroupBy(oi => new
                {
                    oi.ProductId,
                    oi.Product.NameAr,
                    oi.Product.NameEn,
                    oi.Product.Price,
                    oi.Product.ImageUrl
                })
                .Select(g => new
                {
                    ProductId = g.Key.ProductId,
                    NameAr = g.Key.NameAr,
                    NameEn = g.Key.NameEn,
                    Price = g.Key.Price,
                    ImageUrl = g.Key.ImageUrl,
                    TotalSold = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(x => x.TotalSold)
                .Take(count)
                .ToListAsync();

            // 🌍 Apply localization in memory using your extension
            var result = topProducts.Select(p => new Dtos.Admin.Product.TopSellingProductDto
            {
                Id = p.ProductId,

                Name = LocalizationExtensions.Localize(
                    p.NameAr,
                    p.NameEn,
                    lang),

                Price = p.Price,
                ImageUrl = p.ImageUrl,
                TotalSold = p.TotalSold
            });

            return Ok(result);
        }


        // =========================
        // MAPPER
        // =========================
        private async Task<object?> Map(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Sizes)
                .Include(p => p.ModifierGroups)
                    .ThenInclude(g => g.Options)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return null;

            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            return new
            {
                product.Id,

                product.NameAr,
                product.NameEn,

                product.DescriptionAr,
                product.DescriptionEn,

                product.Price,
                product.DiscountPercentage,
                product.DiscountStart,
                product.DiscountEnd,

                product.Calories,
                product.PointsReward,

                product.IsAvailable,

                DiscountedPrice = IsDiscountActive(product)
    ? CalculateDiscountedPrice(product)
    : product.Price,

                ImageUrl = product.ImageUrl == null
                    ? null
                    : $"{baseUrl}{product.ImageUrl}",

                CategoryName = product.Category?.Name,

                Sizes = product.Sizes.Select(s => new
                {
                    s.Id,
                    s.NameAr,
                    s.NameEn,
                    s.Price,
                    s.IsDefault
                }),
                ModifierGroups = product.ModifierGroups.Select(g => new
                {
                    g.Id,
                    g.NameAr,
                    g.NameEn,
                    g.IsRequired,
                    g.MaxSelections,

                    Options = g.Options.Select(o => new
                    {
                        o.Id,
                        o.NameAr,
                        o.NameEn,
                        o.ExtraPrice
                    })
                })
            
            };
        }
    }
}
