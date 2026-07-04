using Foodics.Dtos.Admin;
using Foodics.Dtos.Admin.Category;
using Foodics.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POSSystem.Data;

namespace Foodics.Controllers.Admin
{
    [Route("api/admin/categories")]
    [ApiController]
    [Authorize(Roles = "Admin")]

    public class AdminCategoriesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AdminCategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory(CreateCategoryDto dto)
        {
            var category = new Category
            {
                // للحفاظ على البيانات القديمة
                Name = dto.NameAr ?? dto.NameEn ?? string.Empty,

                NameAr = dto.NameAr,
                NameEn = dto.NameEn,

                DescriptionAr = dto.DescriptionAr,
                DescriptionEn = dto.DescriptionEn,

                IsActive = true,
                IsVisible = true,
                IsDeleted = false,
                DisplayOrder = 0


            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return Ok(new CategoryAdminDto
            {
                Id = category.Id,
                NameAr = category.NameAr ?? string.Empty,
                NameEn = category.NameEn ?? string.Empty,
                DescriptionAr = category.DescriptionAr,
                DescriptionEn = category.DescriptionEn ,
                 IsActive = category.IsActive,

                IsVisible = category.IsVisible,

                DisplayOrder = category.DisplayOrder
            });
        }


        
        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _context.Categories.ToListAsync();

            var result = categories.Select(c => new CategoryAdminDto
            {
                Id = c.Id,
                NameAr = c.NameAr ?? c.Name ?? string.Empty,
                NameEn = c.NameEn ?? c.Name ?? string.Empty,
                DescriptionAr = c.DescriptionAr,
                DescriptionEn = c.DescriptionEn,

                IsActive = c.IsActive,

                IsVisible = c.IsVisible,

                DisplayOrder = c.DisplayOrder
            });

            return Ok(result);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategory(int id)
        {
            var category = await _context.Categories

                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
            if (category == null)
                return NotFound("Category not found");

            return Ok(new CategoryAdminDto
            {
                Id = category.Id,
                NameAr = category.NameAr ?? category.Name ?? string.Empty,
                NameEn = category.NameEn ?? category.Name ?? string.Empty,
                DescriptionAr = category.DescriptionAr,
                DescriptionEn = category.DescriptionEn,
                IsActive = category.IsActive,

                IsVisible = category.IsVisible,

                DisplayOrder = category.DisplayOrder
            });
        }



        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, UpdateCategoryDto dto)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

            if (category == null)
                return NotFound("Category not found");

            if (!string.IsNullOrWhiteSpace(dto.NameAr))
                category.NameAr = dto.NameAr;

            if (!string.IsNullOrWhiteSpace(dto.NameEn))
                category.NameEn = dto.NameEn;

            if (!string.IsNullOrWhiteSpace(dto.DescriptionAr))
                category.DescriptionAr = dto.DescriptionAr;

            if (!string.IsNullOrWhiteSpace(dto.DescriptionEn))
                category.DescriptionEn = dto.DescriptionEn;

            if (dto.IsVisible.HasValue)
                category.IsVisible = dto.IsVisible.Value;

            if (dto.DisplayOrder.HasValue)
                category.DisplayOrder = dto.DisplayOrder.Value;

            if (dto.IsActive.HasValue)
                category.IsActive = dto.IsActive.Value;

            // Backward Compatibility
            category.Name = category.NameAr
                ?? category.NameEn
                ?? category.Name;

            await _context.SaveChangesAsync();

            return Ok(new CategoryAdminDto
            {
                Id = category.Id,
                NameAr = category.NameAr ?? category.Name ?? string.Empty,
                NameEn = category.NameEn ?? category.Name ?? string.Empty,
                DescriptionAr = category.DescriptionAr,
                DescriptionEn = category.DescriptionEn,
                IsActive = category.IsActive,

                IsVisible = category.IsVisible,

                DisplayOrder = category.DisplayOrder
            });
        }

        [HttpPatch("{id}/visibility")]
        public async Task<IActionResult> ToggleVisibility(int id)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null || category.IsDeleted)
                return NotFound();

            category.IsVisible = !category.IsVisible;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                category.Id,
                category.IsVisible
            });
        }


        [HttpPatch("{id}/order")]
        public async Task<IActionResult> UpdateOrder(int id, UpdateCategoryOrderDto dto)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null || category.IsDeleted)
                return NotFound();

            category.DisplayOrder = dto.DisplayOrder;

            await _context.SaveChangesAsync();

            return Ok();
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories
     .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

            if (category == null)
                return NotFound("Category not found");

            category.IsDeleted = true;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Category deleted successfully"
            });

        }
    }
}


