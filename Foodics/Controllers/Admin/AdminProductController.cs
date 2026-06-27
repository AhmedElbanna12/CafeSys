//using DotNetEnv;
//using Foodics.Dtos.Admin;
//using Foodics.Dtos.Admin.Product;
//using Foodics.Dtos.Admin.Product.Product;
//using Foodics.Models;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using POSSystem.Data;

//namespace Foodics.Controllers.Admin
//{
//    [Route("api/admin/products")]
//    [ApiController]

//    public class AdminProductsController : ControllerBase
//    {
//        private readonly ApplicationDbContext _context;
//        private readonly IWebHostEnvironment _env;

//        public AdminProductsController(ApplicationDbContext context, IWebHostEnvironment env)
//        {
//            _context = context;
//            _env = env;

//        }


//        private decimal CalculateDiscountedPrice(Product product)
//        {
//            if (!product.DiscountPercentage.HasValue ||
//                !product.DiscountStart.HasValue ||
//                !product.DiscountEnd.HasValue)
//                return product.Price;

//            // ✅ استخدم Local Time بدل UTC
//            var cairoZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time"); // Windows
//                                                                                        // var cairoZone = TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo"); // Linux/Docker

//            var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, cairoZone);

//            // ✅ التواريخ المخزنة هي لوكال، مش UTC
//            var start = product.DiscountStart.Value;
//            var end = product.DiscountEnd.Value;

//            if (now >= start && now <= end)
//                return product.Price - (product.Price * (product.DiscountPercentage.Value / 100m));

//            return product.Price;
//        }


//        [Authorize(Roles = "Admin")]
//        [HttpPost]
//        [RequestSizeLimit(10_000_000)] // 10 MB limit
//        public async Task<IActionResult> CreateProduct([FromForm] CreateProductDto dto)
//        {
//            string imageUrl = null;

//            // ✅ رفع الصورة
//            if (dto.Image != null)
//            {
//                var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "products");
//                if (!Directory.Exists(uploadsFolder))
//                    Directory.CreateDirectory(uploadsFolder);

//                var uniqueFileName = $"{Guid.NewGuid()}_{dto.Image.FileName}";
//                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

//                using (var fileStream = new FileStream(filePath, FileMode.Create))
//                {
//                    await dto.Image.CopyToAsync(fileStream);
//                }

//                imageUrl = $"/images/products/{uniqueFileName}";
//            }


//            var cairoZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");

//            var product = new Product
//            {
//                Name = dto.Name,
//                Description = dto.Description,
//                Price = dto.Price,
//                DiscountPercentage = dto.DiscountPercentage,

//                // ✅ لو الـ DTO بييجي UTC، حوّله للوكال قبل التخزين
//                DiscountStart = dto.DiscountStart.HasValue
//                    ? TimeZoneInfo.ConvertTimeFromUtc(
//                        DateTime.SpecifyKind(dto.DiscountStart.Value, DateTimeKind.Utc), cairoZone)
//                    : null,

//                DiscountEnd = dto.DiscountEnd.HasValue
//                    ? TimeZoneInfo.ConvertTimeFromUtc(
//                        DateTime.SpecifyKind(dto.DiscountEnd.Value, DateTimeKind.Utc), cairoZone)
//                    : null,

//                CategoryId = dto.CategoryId,
//                Calories = dto.Calories,
//                PointsReward = dto.PointsReward,
//                ImageUrl = imageUrl
//            };

//            _context.Products.Add(product);
//            await _context.SaveChangesAsync();

//            // ✅ تحميل المنتج بعد الحفظ مع جميع الـ Includes
//            var createdProduct = await _context.Products
//                .Include(p => p.Category)
//                .Include(p => p.Sizes)
//                .Include(p => p.ModifierGroups)
//                    .ThenInclude(g => g.Options)
//                .FirstOrDefaultAsync(p => p.Id == product.Id);

//            var baseUrl = $"{Request.Scheme}://{Request.Host}";

//            // ✅ تجهيز الـ DTO للرد
//            var result = new ProductResponseDto
//            {
//                Id = createdProduct.Id,
//                Name = createdProduct.Name,
//                Description = createdProduct.Description,
//                Price = createdProduct.Price,
//                DiscountPercentage = createdProduct.DiscountPercentage,
//                DiscountStart = createdProduct.DiscountStart,
//                DiscountEnd = createdProduct.DiscountEnd,
//                Calories = createdProduct.Calories,
//                PointsReward = createdProduct.PointsReward,
//                ImageUrl = string.IsNullOrEmpty(createdProduct.ImageUrl) ? null : $"{baseUrl}{createdProduct.ImageUrl}",
//                IsAvailable = createdProduct.IsAvailable,
//                CategoryName = createdProduct.Category?.Name,
//                Sizes = createdProduct.Sizes?.Select(s => new ProductSizeDto
//                {
//                    Id = s.Id,
//                    Name = s.Name,
//                    Price = s.Price,
//                    IsDefault = s.IsDefault
//                }).ToList(),
//                ModifierGroups = createdProduct.ModifierGroups?.Select(g => new ModifierGroupDto
//                {
//                    Id = g.Id,
//                    Name = g.Name,
//                    IsRequired = g.IsRequired,
//                    MaxSelections = g.MaxSelections,
//                    Options = g.Options.Select(o => new ModifierOptionDto
//                    {
//                        Id = o.Id,
//                        Name = o.Name,
//                        ExtraPrice = o.ExtraPrice
//                    }).ToList()
//                }).ToList(),
//                DiscountedPrice = CalculateDiscountedPrice(createdProduct)
//            };

//            return CreatedAtAction(nameof(GetProductById), new { id = createdProduct.Id }, result);
//        }


//        [Authorize(Roles = "Admin")]
//        [HttpPost("{productId}/sizes")]
//        public async Task<IActionResult> AddSize(int productId, CreateSizeDto dto)
//        {
//            var product = await _context.Products.FindAsync(productId);

//            if (product == null)
//                return NotFound("Product not found");

//            var size = new ProductSize
//            {
//                Name = dto.Name,
//                Price = dto.Price,
//                IsDefault = dto.IsDefault,
//                ProductId = productId
//            };

//            _context.ProductSizes.Add(size);
//            await _context.SaveChangesAsync();

//            // ✅ Load product with all navigation properties
//            var updatedProduct = await _context.Products
//                .Include(p => p.Category)
//                .Include(p => p.Sizes)
//                .Include(p => p.ModifierGroups)
//                    .ThenInclude(g => g.Options)
//                .FirstOrDefaultAsync(p => p.Id == productId);

//            // Map to DTO
//            var result = new ProductResponseDto
//            {
//                Id = updatedProduct.Id,
//                Name = updatedProduct.Name,
//                Description = updatedProduct.Description,
//                Price = updatedProduct.Price,
//                Calories = updatedProduct.Calories,
//                PointsReward = updatedProduct.PointsReward,
//                ImageUrl = updatedProduct.ImageUrl,
//                IsAvailable = updatedProduct.IsAvailable,
//                CategoryName = updatedProduct.Category?.Name,

//                Sizes = updatedProduct.Sizes?.Select(s => new ProductSizeDto
//                {
//                    Id = s.Id,
//                    Name = s.Name,
//                    Price = s.Price,
//                    IsDefault = s.IsDefault
//                }).ToList(),

//                ModifierGroups = updatedProduct.ModifierGroups?.Select(g => new ModifierGroupDto
//                {
//                    Id = g.Id,
//                    Name = g.Name,
//                    IsRequired = g.IsRequired,
//                    MaxSelections = g.MaxSelections,
//                    Options = g.Options.Select(o => new ModifierOptionDto
//                    {
//                        Id = o.Id,
//                        Name = o.Name,
//                        ExtraPrice = o.ExtraPrice
//                    }).ToList()
//                }).ToList()
//            };

//            return Ok(result);
//        }



//        [Authorize(Roles = "Admin")]
//        [HttpPost("{productId}/modifier-groups")]
//        public async Task<IActionResult> CreateModifierGroup(int productId, CreateModifierGroupDto dto)
//        {
//            var product = await _context.Products.FindAsync(productId);
//            if (product == null)
//                return NotFound("Product not found");

//            var group = new ModifierGroup
//            {
//                Name = dto.Name,
//                IsRequired = dto.IsRequired,
//                MaxSelections = dto.MaxSelections,
//                ProductId = productId
//            };

//            _context.ModifierGroups.Add(group);
//            await _context.SaveChangesAsync();

//            // 🔹 رجع DTO فقط بدون navigation properties
//            var result = new ModifierGroupDto
//            {
//                Id = group.Id,
//                Name = group.Name,
//                IsRequired = group.IsRequired,
//                MaxSelections = group.MaxSelections,
//                Options = new List<ModifierOptionDto>() // خالي أول مرة
//            };

//            return Ok(result);
//        }


//        [Authorize(Roles = "Admin")]
//        [HttpPost("modifier-groups/{groupId}/options")]
//        public async Task<IActionResult> AddOption(int groupId, CreateModifierOptionDto dto)
//        {
//            var group = await _context.ModifierGroups.FindAsync(groupId);
//            if (group == null)
//                return NotFound("Modifier group not found");

//            var option = new ModifierOption
//            {
//                Name = dto.Name,
//                ExtraPrice = dto.ExtraPrice,
//                ModifierGroupId = groupId
//            };

//            _context.ModifierOptions.Add(option);
//            await _context.SaveChangesAsync();

//            // 🔹 Load group with all options
//            var updatedGroup = await _context.ModifierGroups
//                .Include(g => g.Options)
//                .FirstOrDefaultAsync(g => g.Id == groupId);

//            var result = new ModifierGroupDto
//            {
//                Id = updatedGroup.Id,
//                Name = updatedGroup.Name,
//                IsRequired = updatedGroup.IsRequired,
//                MaxSelections = updatedGroup.MaxSelections,
//                Options = updatedGroup.Options.Select(o => new ModifierOptionDto
//                {
//                    Id = o.Id,
//                    Name = o.Name,
//                    ExtraPrice = o.ExtraPrice
//                }).ToList()
//            };

//            return Ok(result);
//        }



//        //    // 🔹 Get all products with discounted price
//        //    [HttpGet]
//        //    [Authorize(Roles = "Admin")]
//        //    public async Task<IActionResult> GetProductsAdmin()
//        //    {
//        //        var products = await _context.Products
//        //.Where(p => !p.IsDeleted)
//        //            .Include(p => p.Category)
//        //            .Include(p => p.Sizes)
//        //            .Include(p => p.ModifierGroups)
//        //                .ThenInclude(g => g.Options)
//        //            .ToListAsync();

//        //        var baseUrl = $"{Request.Scheme}://{Request.Host}";

//        //        var result = products.Select(product => new ProductResponseDto
//        //        {
//        //            Id = product.Id,
//        //            Name = product.Name,
//        //            Description = product.Description,
//        //            Price = product.Price,
//        //            Calories = product.Calories,
//        //            PointsReward = product.PointsReward,
//        //            ImageUrl = string.IsNullOrEmpty(product.ImageUrl) ? null : $"{baseUrl}{product.ImageUrl}",
//        //            IsAvailable = product.IsAvailable,
//        //            CategoryName = product.Category?.Name,
//        //            Sizes = product.Sizes.Select(s => new ProductSizeDto
//        //            {
//        //                Id = s.Id,
//        //                Name = s.Name,
//        //                Price = s.Price,
//        //                IsDefault = s.IsDefault
//        //            }).ToList(),
//        //            ModifierGroups = product.ModifierGroups?.Select(g => new ModifierGroupDto
//        //            {
//        //                Id = g.Id,
//        //                Name = g.Name,
//        //                IsRequired = g.IsRequired,
//        //                MaxSelections = g.MaxSelections,
//        //                Options = g.Options.Select(o => new ModifierOptionDto
//        //                {
//        //                    Id = o.Id,
//        //                    Name = o.Name,
//        //                    ExtraPrice = o.ExtraPrice
//        //                }).ToList()
//        //            }).ToList(),
//        //            DiscountedPrice = CalculateDiscountedPrice(product),
//        //            DiscountPercentage = product.DiscountPercentage,
//        //            DiscountStart = product.DiscountStart,
//        //            DiscountEnd = product.DiscountEnd
//        //        });

//        //        return Ok(result);
//        //    }



//        // 🔹 Get all products with discounted price
//        [HttpGet]
//        [Authorize(Roles = "Admin")]
//        public async Task<IActionResult> GetProductsAdmin()
//        {
//            var products = await _context.Products
//    .Where(p => !p.IsDeleted)
//                .Include(p => p.Category)
//                .Include(p => p.Sizes)
//                .Include(p => p.ModifierGroups)
//                    .ThenInclude(g => g.Options)
//                .ToListAsync();

//            var baseUrl = $"{Request.Scheme}://{Request.Host}";

//            var result = products.Select(product => new ProductResponseDto
//            {
//                Id = product.Id,
//                Name = product.Name,
//                Description = product.Description,
//                Price = product.Price,
//                Calories = product.Calories,
//                PointsReward = product.PointsReward,
//                ImageUrl = string.IsNullOrEmpty(product.ImageUrl) ? null : $"{baseUrl}{product.ImageUrl}",
//                IsAvailable = product.IsAvailable,
//                Sizes = product.Sizes.Select(s => new ProductSizeDto
//                {
//                    Id = s.Id,
//                    Name = s.Name,
//                    Price = s.Price,
//                    IsDefault = s.IsDefault
//                }).ToList(),
//                ModifierGroups = product.ModifierGroups?.Select(g => new ModifierGroupDto
//                {
//                    Id = g.Id,
//                    Name = g.Name,
//                    IsRequired = g.IsRequired,
//                    MaxSelections = g.MaxSelections,
//                    Options = g.Options.Select(o => new ModifierOptionDto
//                    {
//                        Id = o.Id,
//                        Name = o.Name,
//                        ExtraPrice = o.ExtraPrice
//                    }).ToList()
//                }).ToList(),
//                DiscountedPrice = CalculateDiscountedPrice(product),
//                DiscountPercentage = product.DiscountPercentage,
//                DiscountStart = product.DiscountStart,
//                DiscountEnd = product.DiscountEnd
//            });

//            return Ok(result);
//        }




//        // 🔹 Get product by Id with discounted price
//        [HttpGet("product/{id}")]
//        [Authorize(Roles = "Admin")]

//        public async Task<IActionResult> GetProductById(int id)
//        {
//            var product = await _context.Products
//                .Include(p => p.Category)
//                .Include(p => p.Sizes)
//                .Include(p => p.ModifierGroups)
//                    .ThenInclude(g => g.Options)
//                .FirstOrDefaultAsync(p => p.Id == id);

//            if (product == null)
//                return NotFound("Product not found");

//            var baseUrl = $"{Request.Scheme}://{Request.Host}";

//            var result = new ProductResponseDto
//            {
//                Id = product.Id,
//                Name = product.Name,
//                Description = product.Description,
//                Price = product.Price,
//                DiscountPercentage = product.DiscountPercentage,
//                DiscountStart = product.DiscountStart,
//                DiscountEnd = product.DiscountEnd,
//                Calories = product.Calories,
//                PointsReward = product.PointsReward,
//                ImageUrl = string.IsNullOrEmpty(product.ImageUrl) ? null : $"{baseUrl}{product.ImageUrl}",
//                IsAvailable = product.IsAvailable,
//                CategoryName = product.Category?.Name,
//                Sizes = product.Sizes?.Select(s => new ProductSizeDto
//                {
//                    Id = s.Id,
//                    Name = s.Name,
//                    Price = s.Price,
//                    IsDefault = s.IsDefault
//                }).ToList(),
//                ModifierGroups = product.ModifierGroups?.Select(g => new ModifierGroupDto
//                {
//                    Id = g.Id,
//                    Name = g.Name,
//                    IsRequired = g.IsRequired,
//                    MaxSelections = g.MaxSelections,
//                    Options = g.Options.Select(o => new ModifierOptionDto
//                    {
//                        Id = o.Id,
//                        Name = o.Name,
//                        ExtraPrice = o.ExtraPrice
//                    }).ToList()
//                }).ToList(),
//                DiscountedPrice = CalculateDiscountedPrice(product)
//            };

//            return Ok(result);
//        }


//        [Authorize(Roles = "Admin")]
//        [HttpPut("{id}")]
//        [RequestSizeLimit(10_000_000)]
//        public async Task<IActionResult> UpdateProduct(int id, [FromForm] UpdateProductDto dto)
//        {
//            var product = await _context.Products.FindAsync(id);
//            if (product == null)
//                return NotFound("Product not found");

//            // ✅ Handle Image Upload (اختياري)
//            if (dto.Image != null)
//            {
//                var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "products");
//                if (!Directory.Exists(uploadsFolder))
//                    Directory.CreateDirectory(uploadsFolder);

//                // حذف الصورة القديمة
//                if (!string.IsNullOrEmpty(product.ImageUrl))
//                {
//                    var oldFile = Path.Combine(
//                        _env.WebRootPath,
//                        product.ImageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar)
//                    );

//                    if (System.IO.File.Exists(oldFile))
//                        System.IO.File.Delete(oldFile);
//                }

//                var uniqueFileName = $"{Guid.NewGuid()}_{dto.Image.FileName}";
//                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

//                using (var fileStream = new FileStream(filePath, FileMode.Create))
//                {
//                    await dto.Image.CopyToAsync(fileStream);
//                }

//                product.ImageUrl = $"/images/products/{uniqueFileName}";
//            }

//            // ✅ Partial Update (مهم)
//            if (dto.Name != null)
//                product.Name = dto.Name;

//            if (dto.Description != null)
//                product.Description = dto.Description;

//            if (dto.Price.HasValue)
//                product.Price = dto.Price.Value;

//            if (dto.DiscountPercentage.HasValue)
//                product.DiscountPercentage = dto.DiscountPercentage;

//            var cairoZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");

//            if (dto.DiscountStart.HasValue)
//                product.DiscountStart = TimeZoneInfo.ConvertTimeFromUtc(
//                    DateTime.SpecifyKind(dto.DiscountStart.Value, DateTimeKind.Utc), cairoZone);

//            if (dto.DiscountEnd.HasValue)
//                product.DiscountEnd = TimeZoneInfo.ConvertTimeFromUtc(
//                    DateTime.SpecifyKind(dto.DiscountEnd.Value, DateTimeKind.Utc), cairoZone);

//            if (dto.Calories.HasValue)
//                product.Calories = dto.Calories.Value;

//            if (dto.PointsReward.HasValue)
//                product.PointsReward = dto.PointsReward.Value;

//            if (dto.CategoryId.HasValue)
//                product.CategoryId = dto.CategoryId.Value;

//            await _context.SaveChangesAsync();

//            // ✅ رجع المنتج بالـ Includes
//            var updatedProduct = await _context.Products
//                .Include(p => p.Category)
//                .Include(p => p.Sizes)
//                .Include(p => p.ModifierGroups)
//                    .ThenInclude(g => g.Options)
//                .FirstOrDefaultAsync(p => p.Id == id);

//            var baseUrl = $"{Request.Scheme}://{Request.Host}";

//            var result = new ProductResponseDto
//            {
//                Id = updatedProduct.Id,
//                Name = updatedProduct.Name,
//                Description = updatedProduct.Description,
//                Price = updatedProduct.Price,
//                DiscountPercentage = updatedProduct.DiscountPercentage,
//                DiscountStart = updatedProduct.DiscountStart,
//                DiscountEnd = updatedProduct.DiscountEnd,
//                Calories = updatedProduct.Calories,
//                PointsReward = updatedProduct.PointsReward,
//                ImageUrl = string.IsNullOrEmpty(updatedProduct.ImageUrl) ? null : $"{baseUrl}{updatedProduct.ImageUrl}",
//                IsAvailable = updatedProduct.IsAvailable,
//                CategoryName = updatedProduct.Category?.Name,

//                Sizes = updatedProduct.Sizes?.Select(s => new ProductSizeDto
//                {
//                    Id = s.Id,
//                    Name = s.Name,
//                    Price = s.Price,
//                    IsDefault = s.IsDefault
//                }).ToList(),

//                ModifierGroups = updatedProduct.ModifierGroups?.Select(g => new ModifierGroupDto
//                {
//                    Id = g.Id,
//                    Name = g.Name,
//                    IsRequired = g.IsRequired,
//                    MaxSelections = g.MaxSelections,
//                    Options = g.Options.Select(o => new ModifierOptionDto
//                    {
//                        Id = o.Id,
//                        Name = o.Name,
//                        ExtraPrice = o.ExtraPrice
//                    }).ToList()
//                }).ToList(),

//                DiscountedPrice = CalculateDiscountedPrice(updatedProduct)
//            };

//            return Ok(result);
//        }


//        //[Authorize(Roles = "Admin")]
//        //[HttpDelete("{id}")]
//        //public async Task<IActionResult> DeleteProduct(int id)
//        //{
//        //    var product = await _context.Products.FindAsync(id);

//        //    if (product == null)
//        //        return NotFound("Product not found");

//        //    _context.Products.Remove(product);

//        //    await _context.SaveChangesAsync();

//        //    return Ok("Product deleted successfully");
//        //}



//        [Authorize(Roles = "Admin")]
//        [HttpPatch("{id}/toggle-availability")]
//        public async Task<IActionResult> ToggleAvailability(int id)
//        {
//            var product = await _context.Products.FindAsync(id);

//            if (product == null)
//                return NotFound("Product not found");

//            product.IsAvailable = !product.IsAvailable;

//            await _context.SaveChangesAsync();

//            return Ok(new
//            {
//                product.Id,
//                product.Name,
//                product.IsAvailable
//            });
//        }


//        [Authorize(Roles = "Admin")]
//        [HttpPut("sizes/{sizeId}")]
//        public async Task<IActionResult> UpdateSize(int sizeId, UpdateSizeDto dto)
//        {
//            var size = await _context.ProductSizes.FindAsync(sizeId);
//            if (size == null)
//                return NotFound("Size not found");

//            // Update
//            size.Name = dto.Name;
//            size.Price = dto.Price;
//            size.IsDefault = dto.IsDefault;

//            await _context.SaveChangesAsync();

//            // Return فقط بيانات الحجم
//            var result = new
//            {
//                size.Id,
//                size.Name,
//                size.Price,
//                size.IsDefault
//            };

//            return Ok(result);
//        }


//        [Authorize(Roles = "Admin")]
//        [HttpPut("modifier-groups/{groupId}")]
//        public async Task<IActionResult> UpdateModifierGroup(int groupId, UpdateModifierGroupDto dto)
//        {
//            var group = await _context.ModifierGroups.FindAsync(groupId);
//            if (group == null)
//                return NotFound("Modifier group not found");

//            // Update
//            group.Name = dto.Name;
//            group.IsRequired = dto.IsRequired;
//            group.MaxSelections = dto.MaxSelections;

//            await _context.SaveChangesAsync();

//            // 🔹 Load group with Options after update
//            var updatedGroup = await _context.ModifierGroups
//                .Include(g => g.Options)
//                .FirstOrDefaultAsync(g => g.Id == groupId);

//            var result = new ModifierGroupDto
//            {
//                Id = updatedGroup.Id,
//                Name = updatedGroup.Name,
//                IsRequired = updatedGroup.IsRequired,
//                MaxSelections = updatedGroup.MaxSelections,
//                Options = updatedGroup.Options?.Select(o => new ModifierOptionDto
//                {
//                    Id = o.Id,
//                    Name = o.Name,
//                    ExtraPrice = o.ExtraPrice
//                }).ToList() ?? new List<ModifierOptionDto>()
//            };

//            return Ok(result);
//        }


//        [Authorize(Roles = "Admin")]
//        // Update Modifier Option
//        [HttpPut("modifier-options/{optionId}")]
//        public async Task<IActionResult> UpdateModifierOption(int optionId, UpdateModifierOptionDto dto)
//        {
//            var option = await _context.ModifierOptions.FindAsync(optionId);
//            if (option == null)
//                return NotFound("Modifier option not found");

//            // Update
//            option.Name = dto.Name;
//            option.ExtraPrice = dto.ExtraPrice;

//            await _context.SaveChangesAsync();

//            // 🔹 Load modifier group with all options
//            var updatedGroup = await _context.ModifierGroups
//                .Include(g => g.Options)
//                .FirstOrDefaultAsync(g => g.Id == option.ModifierGroupId);

//            var result = new ModifierGroupDto
//            {
//                Id = updatedGroup.Id,
//                Name = updatedGroup.Name,
//                IsRequired = updatedGroup.IsRequired,
//                MaxSelections = updatedGroup.MaxSelections,
//                Options = updatedGroup.Options.Select(o => new ModifierOptionDto
//                {
//                    Id = o.Id,
//                    Name = o.Name,
//                    ExtraPrice = o.ExtraPrice
//                }).ToList()
//            };

//            return Ok(result);
//        }


//        [HttpGet("{productId}/modifier-groups")]
//        public async Task<IActionResult> GetModifierGroupsByProduct(int productId)
//        {
//            var product = await _context.Products
//                .Include(p => p.ModifierGroups)
//                    .ThenInclude(g => g.Options)
//                .FirstOrDefaultAsync(p => p.Id == productId);

//            if (product == null)
//                return NotFound("Product not found");

//            var result = product.ModifierGroups.Select(g => new ModifierGroupDto
//            {
//                Id = g.Id,
//                Name = g.Name,
//                IsRequired = g.IsRequired,
//                MaxSelections = g.MaxSelections,
//                Options = g.Options.Select(o => new ModifierOptionDto
//                {
//                    Id = o.Id,
//                    Name = o.Name,
//                    ExtraPrice = o.ExtraPrice
//                }).ToList()
//            });

//            return Ok(result);
//        }


//        [HttpGet("modifier-groups/{groupId}")]
//        public async Task<IActionResult> GetModifierGroup(int groupId)
//        {
//            var group = await _context.ModifierGroups
//                .Include(g => g.Options)
//                .FirstOrDefaultAsync(g => g.Id == groupId);

//            if (group == null)
//                return NotFound("Modifier group not found");

//            var result = new ModifierGroupDto
//            {
//                Id = group.Id,
//                Name = group.Name,
//                IsRequired = group.IsRequired,
//                MaxSelections = group.MaxSelections,
//                Options = group.Options.Select(o => new ModifierOptionDto
//                {
//                    Id = o.Id,
//                    Name = o.Name,
//                    ExtraPrice = o.ExtraPrice
//                }).ToList()
//            };

//            return Ok(result);
//        }


//        [HttpGet("modifier-options/{optionId}")]
//        public async Task<IActionResult> GetOption(int optionId)
//        {
//            var option = await _context.ModifierOptions.FindAsync(optionId);

//            if (option == null)
//                return NotFound("Modifier option not found");

//            return Ok(new ModifierOptionDto
//            {
//                Id = option.Id,
//                Name = option.Name,
//                ExtraPrice = option.ExtraPrice
//            });
//        }


//        [Authorize(Roles = "Admin")]
//        [HttpDelete("sizes/{sizeId}")]
//        public async Task<IActionResult> DeleteSize(int sizeId)
//        {
//            var size = await _context.ProductSizes.FindAsync(sizeId);

//            if (size == null)
//                return NotFound();

//            // 🔥 Remove from CartItems
//            var cartItems = await _context.CartItems
//                .Where(c => c.ProductSizeId == sizeId)
//                .ToListAsync();

//            _context.CartItems.RemoveRange(cartItems);

//            // 🔥 Remove from OrderItems (or just null it logically)
//            var orderItems = await _context.OrderItems
//                .Where(o => o.ProductSizeId == sizeId)
//                .ToListAsync();

//            foreach (var item in orderItems)
//                item.ProductSizeId = null;

//            // 🔥 Finally delete size
//            _context.ProductSizes.Remove(size);

//            await _context.SaveChangesAsync();

//            return Ok("Size deleted successfully");
//        }


//        [Authorize(Roles = "Admin")]
//        [HttpDelete("modifier-groups/{groupId}")]
//        public async Task<IActionResult> DeleteModifierGroup(int groupId)
//        {
//            var group = await _context.ModifierGroups
//                .Include(g => g.Options)
//                .FirstOrDefaultAsync(g => g.Id == groupId);

//            if (group == null)
//                return NotFound("Modifier group not found");

//            var optionIds = group.Options.Select(o => o.Id).ToList();

//            var cartItems = _context.CartItemModifiers
//                .Where(c => optionIds.Contains(c.ModifierOptionId));

//            _context.CartItemModifiers.RemoveRange(cartItems);

//            _context.ModifierOptions.RemoveRange(group.Options);

//            _context.ModifierGroups.Remove(group);

//            await _context.SaveChangesAsync();

//            return Ok("Modifier group deleted successfully");
//        }


//        [Authorize(Roles = "Admin")]
//        [HttpDelete("modifier-options/{optionId}")]
//        public async Task<IActionResult> DeleteModifierOption(int optionId)
//        {
//            var option = await _context.ModifierOptions.FindAsync(optionId);

//            if (option == null)
//                return NotFound("Modifier option not found");

//            // 🔥 delete cart references first
//            var cartItems = _context.CartItemModifiers
//                .Where(c => c.ModifierOptionId == optionId);

//            _context.CartItemModifiers.RemoveRange(cartItems);

//            // 🔥 delete option
//            _context.ModifierOptions.Remove(option);

//            await _context.SaveChangesAsync();

//            return Ok("Modifier option deleted successfully");
//        }

//        [Authorize(Roles = "Admin")]
//        [HttpDelete("{id}")]
//        public async Task<IActionResult> DeleteProduct(int id)
//        {
//            var product = await _context.Products
//                .FirstOrDefaultAsync(p => p.Id == id);

//            if (product == null)
//                return NotFound(new { message = "Product not found" });

//            product.IsDeleted = true;

//            await _context.SaveChangesAsync();

//            return Ok(new
//            {
//                message = "Product deleted successfully"
//            });
//        }
//    }
//    }



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
                    g.NameEn,
                    g.IsRequired,
                    g.MaxSelections,
                    Options = g.Options.Select(o => new
                    {
                        o.Id,
                        o.NameEn,
                        o.ExtraPrice
                    })
                })
            };
        }
    }
}
