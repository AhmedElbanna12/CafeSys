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

    public class AdminProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public AdminProductsController(ApplicationDbContext context , IWebHostEnvironment env)
        {
            _context = context;
            _env = env;

        }



        // 🔹 Helper لحساب السعر بعد الخصم
        private decimal CalculateDiscountedPrice(Product product)
        {
            if (product.DiscountPercentage.HasValue
                && product.DiscountStart.HasValue
                && product.DiscountEnd.HasValue)
            {
                var now = DateTime.UtcNow;
                if (now >= product.DiscountStart.Value && now <= product.DiscountEnd.Value)
                {
                    return product.Price * (1 - product.DiscountPercentage.Value / 100);
                }
            }
            return product.Price;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [RequestSizeLimit(10_000_000)] // 10 MB limit
        public async Task<IActionResult> CreateProduct([FromForm] CreateProductDto dto)
        {
            string imageUrl = null;

            // ✅ رفع الصورة
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

            // ✅ إنشاء المنتج
            var product = new Product
            {
                Name = dto.Name,
                Description = dto.Description,
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

            // ✅ تحميل المنتج بعد الحفظ مع جميع الـ Includes
            var createdProduct = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Sizes)
                .Include(p => p.ModifierGroups)
                    .ThenInclude(g => g.Options)
                .FirstOrDefaultAsync(p => p.Id == product.Id);

            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            // ✅ تجهيز الـ DTO للرد
            var result = new ProductResponseDto
            {
                Id = createdProduct.Id,
                Name = createdProduct.Name,
                Description = createdProduct.Description,
                Price = createdProduct.Price,
                Calories = createdProduct.Calories,
                PointsReward = createdProduct.PointsReward,
                ImageUrl = string.IsNullOrEmpty(createdProduct.ImageUrl) ? null : $"{baseUrl}{createdProduct.ImageUrl}",
                IsAvailable = createdProduct.IsAvailable,
                CategoryName = createdProduct.Category?.Name,
                Sizes = createdProduct.Sizes?.Select(s => new ProductSizeDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Price = s.Price,
                    IsDefault = s.IsDefault
                }).ToList(),
                ModifierGroups = createdProduct.ModifierGroups?.Select(g => new ModifierGroupDto
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
                }).ToList(),
                DiscountedPrice = CalculateDiscountedPrice(createdProduct)
            };

            return CreatedAtAction(nameof(GetProductById), new { id = createdProduct.Id }, result);
        }


        [Authorize(Roles = "Admin")]
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

            // ✅ Load product with all navigation properties
            var updatedProduct = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Sizes)
                .Include(p => p.ModifierGroups)
                    .ThenInclude(g => g.Options)
                .FirstOrDefaultAsync(p => p.Id == productId);

            // Map to DTO
            var result = new ProductResponseDto
            {
                Id = updatedProduct.Id,
                Name = updatedProduct.Name,
                Description = updatedProduct.Description,
                Price = updatedProduct.Price,
                Calories = updatedProduct.Calories,
                PointsReward = updatedProduct.PointsReward,
                ImageUrl = updatedProduct.ImageUrl,
                IsAvailable = updatedProduct.IsAvailable,
                CategoryName = updatedProduct.Category?.Name,

                Sizes = updatedProduct.Sizes?.Select(s => new ProductSizeDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Price = s.Price,
                    IsDefault = s.IsDefault
                }).ToList(),

                ModifierGroups = updatedProduct.ModifierGroups?.Select(g => new ModifierGroupDto
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



        [Authorize(Roles = "Admin")]
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

            // 🔹 رجع DTO فقط بدون navigation properties
            var result = new ModifierGroupDto
            {
                Id = group.Id,
                Name = group.Name,
                IsRequired = group.IsRequired,
                MaxSelections = group.MaxSelections,
                Options = new List<ModifierOptionDto>() // خالي أول مرة
            };

            return Ok(result);
        }


        [Authorize(Roles = "Admin")]
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

    // 🔹 Load group with all options
    var updatedGroup = await _context.ModifierGroups
        .Include(g => g.Options)
        .FirstOrDefaultAsync(g => g.Id == groupId);

    var result = new ModifierGroupDto
    {
        Id = updatedGroup.Id,
        Name = updatedGroup.Name,
        IsRequired = updatedGroup.IsRequired,
        MaxSelections = updatedGroup.MaxSelections,
        Options = updatedGroup.Options.Select(o => new ModifierOptionDto
        {
            Id = o.Id,
            Name = o.Name,
            ExtraPrice = o.ExtraPrice
        }).ToList()
    };

    return Ok(result);
}


        // 🔹 Get all products with discounted price
        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Sizes)
                .Include(p => p.ModifierGroups)
                    .ThenInclude(g => g.Options)
                .ToListAsync();

            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            var result = products.Select(product => new ProductResponseDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Calories = product.Calories,
                PointsReward = product.PointsReward,
                ImageUrl = string.IsNullOrEmpty(product.ImageUrl) ? null : $"{baseUrl}{product.ImageUrl}",
                IsAvailable = product.IsAvailable,
                CategoryName = product.Category?.Name,
                Sizes = product.Sizes.Select(s => new ProductSizeDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Price = s.Price,
                    IsDefault = s.IsDefault
                }).ToList(),
                ModifierGroups = product.ModifierGroups?.Select(g => new ModifierGroupDto
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
                }).ToList(),
                DiscountedPrice = CalculateDiscountedPrice(product)
            });

            return Ok(result);
        }

        // 🔹 Get product by Id with discounted price
        [HttpGet("product/{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Sizes)
                .Include(p => p.ModifierGroups)
                    .ThenInclude(g => g.Options)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound("Product not found");

            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            var result = new ProductResponseDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Calories = product.Calories,
                PointsReward = product.PointsReward,
                ImageUrl = string.IsNullOrEmpty(product.ImageUrl) ? null : $"{baseUrl}{product.ImageUrl}",
                IsAvailable = product.IsAvailable,
                CategoryName = product.Category?.Name,
                Sizes = product.Sizes?.Select(s => new ProductSizeDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Price = s.Price,
                    IsDefault = s.IsDefault
                }).ToList(),
                ModifierGroups = product.ModifierGroups?.Select(g => new ModifierGroupDto
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
                }).ToList(),
                DiscountedPrice = CalculateDiscountedPrice(product)
            };

            return Ok(result);
        }




        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        [RequestSizeLimit(10_000_000)]
        public async Task<IActionResult> UpdateProduct(int id, [FromForm] UpdateProductDto dto)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound("Product not found");

            // Handle Image Upload
            if (dto.Image != null)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "products");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                // Delete old image
                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    var oldFile = Path.Combine(
                        _env.WebRootPath,
                        product.ImageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar)
                    );
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

            // Update باقي البيانات
            product.Name = dto.Name;
            product.Description = dto.Description;
            product.Price = dto.Price;
            product.DiscountPercentage = dto.DiscountPercentage;
            product.DiscountStart = dto.DiscountStart;
            product.DiscountEnd = dto.DiscountEnd;
            product.Calories = dto.Calories;
            product.PointsReward = dto.PointsReward;
            product.CategoryId = dto.CategoryId;

            await _context.SaveChangesAsync();

            // ✅ رجع المنتج بالـ Includes
            var updatedProduct = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Sizes)
                .Include(p => p.ModifierGroups)
                    .ThenInclude(g => g.Options)
                .FirstOrDefaultAsync(p => p.Id == id);

            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            var result = new ProductResponseDto
            {
                Id = updatedProduct.Id,
                Name = updatedProduct.Name,
                Description = updatedProduct.Description,
                Price = updatedProduct.Price,
                Calories = updatedProduct.Calories,
                PointsReward = updatedProduct.PointsReward,
                ImageUrl = string.IsNullOrEmpty(updatedProduct.ImageUrl) ? null : $"{baseUrl}{updatedProduct.ImageUrl}",
                IsAvailable = updatedProduct.IsAvailable,
                CategoryName = updatedProduct.Category?.Name,
                Sizes = updatedProduct.Sizes?.Select(s => new ProductSizeDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Price = s.Price,
                    IsDefault = s.IsDefault
                }).ToList(),
                ModifierGroups = updatedProduct.ModifierGroups?.Select(g => new ModifierGroupDto
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
                }).ToList(),
                DiscountedPrice = CalculateDiscountedPrice(updatedProduct)
            };

            return Ok(result);
        }


        [Authorize(Roles = "Admin")]
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



        [Authorize(Roles = "Admin")]
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


        [Authorize(Roles = "Admin")]
        [HttpPut("sizes/{sizeId}")]
        public async Task<IActionResult> UpdateSize(int sizeId, UpdateSizeDto dto)
        {
            var size = await _context.ProductSizes.FindAsync(sizeId);
            if (size == null)
                return NotFound("Size not found");

            // Update
            size.Name = dto.Name;
            size.Price = dto.Price;
            size.IsDefault = dto.IsDefault;

            await _context.SaveChangesAsync();

            // Return فقط بيانات الحجم
            var result = new
            {
                size.Id,
                size.Name,
                size.Price,
                size.IsDefault
            };

            return Ok(result);
        }


        [Authorize(Roles = "Admin")]
        [HttpPut("modifier-groups/{groupId}")]
        public async Task<IActionResult> UpdateModifierGroup(int groupId, UpdateModifierGroupDto dto)
        {
            var group = await _context.ModifierGroups.FindAsync(groupId);
            if (group == null)
                return NotFound("Modifier group not found");

            // Update
            group.Name = dto.Name;
            group.IsRequired = dto.IsRequired;
            group.MaxSelections = dto.MaxSelections;

            await _context.SaveChangesAsync();

            // 🔹 Load group with Options after update
            var updatedGroup = await _context.ModifierGroups
                .Include(g => g.Options)
                .FirstOrDefaultAsync(g => g.Id == groupId);

            var result = new ModifierGroupDto
            {
                Id = updatedGroup.Id,
                Name = updatedGroup.Name,
                IsRequired = updatedGroup.IsRequired,
                MaxSelections = updatedGroup.MaxSelections,
                Options = updatedGroup.Options?.Select(o => new ModifierOptionDto
                {
                    Id = o.Id,
                    Name = o.Name,
                    ExtraPrice = o.ExtraPrice
                }).ToList() ?? new List<ModifierOptionDto>()
            };

            return Ok(result);
        }


        [Authorize(Roles = "Admin")]
        // Update Modifier Option
        [HttpPut("modifier-options/{optionId}")]
        public async Task<IActionResult> UpdateModifierOption(int optionId, UpdateModifierOptionDto dto)
        {
            var option = await _context.ModifierOptions.FindAsync(optionId);
            if (option == null)
                return NotFound("Modifier option not found");

            // Update
            option.Name = dto.Name;
            option.ExtraPrice = dto.ExtraPrice;

            await _context.SaveChangesAsync();

            // 🔹 Load modifier group with all options
            var updatedGroup = await _context.ModifierGroups
                .Include(g => g.Options)
                .FirstOrDefaultAsync(g => g.Id == option.ModifierGroupId);

            var result = new ModifierGroupDto
            {
                Id = updatedGroup.Id,
                Name = updatedGroup.Name,
                IsRequired = updatedGroup.IsRequired,
                MaxSelections = updatedGroup.MaxSelections,
                Options = updatedGroup.Options.Select(o => new ModifierOptionDto
                {
                    Id = o.Id,
                    Name = o.Name,
                    ExtraPrice = o.ExtraPrice
                }).ToList()
            };

            return Ok(result);
        }


        [HttpGet("{productId}/modifier-groups")]
        public async Task<IActionResult> GetModifierGroupsByProduct(int productId)
        {
            var product = await _context.Products
                .Include(p => p.ModifierGroups)
                    .ThenInclude(g => g.Options)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
                return NotFound("Product not found");

            var result = product.ModifierGroups.Select(g => new ModifierGroupDto
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
            });

            return Ok(result);
        }


        [HttpGet("modifier-groups/{groupId}")]
        public async Task<IActionResult> GetModifierGroup(int groupId)
        {
            var group = await _context.ModifierGroups
                .Include(g => g.Options)
                .FirstOrDefaultAsync(g => g.Id == groupId);

            if (group == null)
                return NotFound("Modifier group not found");

            var result = new ModifierGroupDto
            {
                Id = group.Id,
                Name = group.Name,
                IsRequired = group.IsRequired,
                MaxSelections = group.MaxSelections,
                Options = group.Options.Select(o => new ModifierOptionDto
                {
                    Id = o.Id,
                    Name = o.Name,
                    ExtraPrice = o.ExtraPrice
                }).ToList()
            };

            return Ok(result);
        }


        [HttpGet("modifier-options/{optionId}")]
        public async Task<IActionResult> GetOption(int optionId)
        {
            var option = await _context.ModifierOptions.FindAsync(optionId);

            if (option == null)
                return NotFound("Modifier option not found");

            return Ok(new ModifierOptionDto
            {
                Id = option.Id,
                Name = option.Name,
                ExtraPrice = option.ExtraPrice
            });
        }

    }
}
