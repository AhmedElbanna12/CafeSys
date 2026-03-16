using DotNetEnv;
using Foodics.Dtos.Admin;
using Foodics.Dtos.Admin.Product;
using Foodics.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POSSystem.Data;

namespace Foodics.Controllers
{
    [Route("api/admin/products")]
    [ApiController]
   // [Authorize(Roles = "Admin")]

    public class AdminProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public AdminProductsController(ApplicationDbContext context , IWebHostEnvironment env)
        {
            _context = context;
            _env = env;

        }

        // Create Product with Image Upload
        [HttpPost]
        [RequestSizeLimit(10_000_000)] // 10 MB limit
        public async Task<IActionResult> CreateProduct([FromForm] CreateProductDto dto)
        {
            string imageUrl = null;

            if (dto.Image != null)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "products");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{Guid.NewGuid()}_{dto.Image.FileName}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.Image.CopyToAsync(fileStream);
                }

                imageUrl = $"/images/products/{uniqueFileName}";
            }

            var product = new Product
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                CategoryId = dto.CategoryId,
                Calories = dto.Calories,
                PointsReward = dto.PointsReward,
                ImageUrl = imageUrl
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, new
            {
                product.Id,
                product.Name,
                product.ImageUrl
            });
        }


        // Add Size
        [HttpPost("{productId}/sizes")]
        public async Task<IActionResult> AddSize(int productId, CreateSizeDto dto)
        {
            var product = await _context.Products.FindAsync(productId);

            if (product == null)
                return NotFound("Product not found");

            var size = new ProductSize
            {
                Name = dto.Name,
                Price = dto.Price,
                IsDefault = dto.IsDefault,
                ProductId = productId
            };

            _context.ProductSizes.Add(size);
            await _context.SaveChangesAsync();

            return Ok(size);
        }

        // Create Modifier Group
        [HttpPost("{productId}/modifier-groups")]
        public async Task<IActionResult> CreateModifierGroup(int productId, CreateModifierGroupDto dto)
        {
            var product = await _context.Products.FindAsync(productId);

            if (product == null)
                return NotFound("Product not found");

            var group = new ModifierGroup
            {
                Name = dto.Name,
                IsRequired = dto.IsRequired,
                MaxSelections = dto.MaxSelections,
                ProductId = productId
            };

            _context.ModifierGroups.Add(group);
            await _context.SaveChangesAsync();

            return Ok(group);
        }

        // Add Modifier Option
        [HttpPost("modifier-groups/{groupId}/options")]
        public async Task<IActionResult> AddOption(int groupId, CreateModifierOptionDto dto)
        {
            var group = await _context.ModifierGroups.FindAsync(groupId);

            if (group == null)
                return NotFound("Modifier group not found");

            var option = new ModifierOption
            {
                Name = dto.Name,
                ExtraPrice = dto.ExtraPrice,
                ModifierGroupId = groupId
            };

            _context.ModifierOptions.Add(option);
            await _context.SaveChangesAsync();

            return Ok(option);
        }

        // Get Single Product
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Sizes)
                .Include(p => p.ModifierGroups)
                    .ThenInclude(g => g.Options)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound();

            var result = new ProductResponseDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Calories = product.Calories,
                PointsReward = product.PointsReward,
                ImageUrl = product.ImageUrl,
                IsAvailable = product.IsAvailable,
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
            };

            return Ok(result);
        }

        // Get All Products
        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Sizes)
                .ToListAsync();

            var result = products.Select(product => new ProductResponseDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Calories = product.Calories,
                PointsReward = product.PointsReward,
                ImageUrl = product.ImageUrl,
                IsAvailable = product.IsAvailable,
                CategoryName = product.Category?.Name,

                Sizes = product.Sizes.Select(s => new ProductSizeDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Price = s.Price,
                    IsDefault = s.IsDefault
                }).ToList()
            });

            return Ok(result);
     
       }

        [HttpPut("{id}")]
        [RequestSizeLimit(10_000_000)] // 10 MB limit
        public async Task<IActionResult> UpdateProduct(int id, [FromForm] UpdateProductDto dto)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound("Product not found");

            if (dto.Image != null)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "products");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                // Delete old image if exists
                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    var oldFile = Path.Combine(_env.WebRootPath, product.ImageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                    if (System.IO.File.Exists(oldFile))
                        System.IO.File.Delete(oldFile);
                }

                var uniqueFileName = $"{Guid.NewGuid()}_{dto.Image.FileName}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.Image.CopyToAsync(fileStream);
                }

                product.ImageUrl = $"/images/products/{uniqueFileName}";
            }

            product.Name = dto.Name;
            product.Description = dto.Description;
            product.Price = dto.Price;
            product.Calories = dto.Calories;
            product.PointsReward = dto.PointsReward;
            product.CategoryId = dto.CategoryId;

            await _context.SaveChangesAsync();

            return Ok(product);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
                return NotFound("Product not found");

            _context.Products.Remove(product);

            await _context.SaveChangesAsync();

            return Ok("Product deleted successfully");
        }


        [HttpPatch("{id}/toggle-availability")]
        public async Task<IActionResult> ToggleAvailability(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
                return NotFound("Product not found");

            product.IsAvailable = !product.IsAvailable;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                product.Id,
                product.Name,
                product.IsAvailable
            });
        }


        // Update Size
        [HttpPut("sizes/{sizeId}")]
        public async Task<IActionResult> UpdateSize(int sizeId, UpdateSizeDto dto)
        {
            var size = await _context.ProductSizes.FindAsync(sizeId);
            if (size == null)
                return NotFound("Size not found");

            size.Name = dto.Name;
            size.Price = dto.Price;
            size.IsDefault = dto.IsDefault;

            await _context.SaveChangesAsync();
            return Ok(size);
        }

        // Update Modifier Group
        [HttpPut("modifier-groups/{groupId}")]
        public async Task<IActionResult> UpdateModifierGroup(int groupId, UpdateModifierGroupDto dto)
        {
            var group = await _context.ModifierGroups.FindAsync(groupId);
            if (group == null)
                return NotFound("Modifier group not found");

            group.Name = dto.Name;
            group.IsRequired = dto.IsRequired;
            group.MaxSelections = dto.MaxSelections;

            await _context.SaveChangesAsync();
            return Ok(group);
        }

        // Update Modifier Option
        [HttpPut("modifier-options/{optionId}")]
        public async Task<IActionResult> UpdateModifierOption(int optionId, UpdateModifierOptionDto dto)
        {
            var option = await _context.ModifierOptions.FindAsync(optionId);
            if (option == null)
                return NotFound("Modifier option not found");

            option.Name = dto.Name;
            option.ExtraPrice = dto.ExtraPrice;

            await _context.SaveChangesAsync();
            return Ok(option);
        }

    }
}
