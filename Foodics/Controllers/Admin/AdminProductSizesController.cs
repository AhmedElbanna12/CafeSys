using Foodics.Dtos.Admin.Product.ProductSize;
using Foodics.ExtensionMethod;
using Foodics.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POSSystem.Data;

namespace Foodics.Controllers.Admin
{
    [Route("api/admin/products/{productId}/sizes")]
    [ApiController]
   // [Authorize(Roles = "Admin")]
    public class AdminProductSizesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AdminProductSizesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ➕ Create Size
        [HttpPost]
        public async Task<IActionResult> Create(int productId, CreateSizeDto dto)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
                return NotFound("Product not found");

            var size = new ProductSize
            {
                Name = product.Name,
                ProductId = productId,
                NameAr = dto.NameAr,
                NameEn = dto.NameEn,
                Price = dto.Price,
                IsDefault = dto.IsDefault
            };

            _context.ProductSizes.Add(size);
            await _context.SaveChangesAsync();

            return Ok(Map(size, "en"));
        }

        // ✏️ Update Size
        [HttpPut("{sizeId}")]
        public async Task<IActionResult> Update(int productId, int sizeId, UpdateSizeDto dto)
        {
            var size = await _context.ProductSizes
                .FirstOrDefaultAsync(x => x.Id == sizeId && x.ProductId == productId);

            if (size == null)
                return NotFound("Size not found");

            if (dto.Name != null) size.Name = dto.Name;
            if (dto.NameAr != null) size.NameAr = dto.NameAr;
            if (dto.NameEn != null) size.NameEn = dto.NameEn;
            if (dto.Price.HasValue) size.Price = dto.Price.Value;
            if (dto.IsDefault.HasValue) size.IsDefault = dto.IsDefault.Value;

            await _context.SaveChangesAsync();

            return Ok(Map(size, "en"));
        }

        // ❌ Delete
        [HttpDelete("{sizeId}")]
        public async Task<IActionResult> Delete(int productId, int sizeId)
        {
            var size = await _context.ProductSizes
                .FirstOrDefaultAsync(x => x.Id == sizeId && x.ProductId == productId);

            if (size == null)
                return NotFound("Size not found");

            _context.ProductSizes.Remove(size);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Deleted successfully" });
        }

        // 📄 Get All
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll(int productId, [FromQuery] string lang = "en")
        {
            var sizes = await _context.ProductSizes
                .Where(x => x.ProductId == productId)
                .ToListAsync();

            var result = sizes.Select(x => Map(x, lang));

            return Ok(result);
        }

        private ProductSizeDto Map(ProductSize size, string lang)
        {
            return new ProductSizeDto
            {
                Id = size.Id,
                Name = LocalizationExtensions.Localize(size.NameAr, size.NameEn, lang),
                Price = size.Price,
                IsDefault = size.IsDefault
            };
        }
    }
}