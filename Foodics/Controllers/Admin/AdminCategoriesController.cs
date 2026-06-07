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



        //        [Authorize(Roles = "Admin")]
        //        // Create Category
        //        [HttpPost]
        //        public async Task<IActionResult> CreateCategory(CreateCategoryDto dto)
        //        {
        //            var category = new Category
        //            {
        //                Name = dto.Name,
        //                Description = dto.Description
        //            };

        //            _context.Categories.Add(category);
        //            await _context.SaveChangesAsync();

        //            return Ok(new CategoryResponseDto
        //            {
        //                Id = category.Id,
        //                Name = category.Name,
        //                Description = category.Description
        //            });
        //        }

        //        // Get All Categories
        //        [HttpGet]
        //        public async Task<IActionResult> GetCategories()
        //        {
        //            var categories = await _context.Categories.ToListAsync();

        //            var result = categories.Select(c => new CategoryResponseDto
        //            {
        //                Id = c.Id,
        //                Name = c.Name,
        //                Description = c.Description
        //            });

        //            return Ok(result);
        //        }

        //        // Get Single Category
        //        [HttpGet("{id}")]
        //        public async Task<IActionResult> GetCategory(int id)
        //        {
        //            var category = await _context.Categories.FindAsync(id);
        //            if (category == null)
        //                return NotFound("Category not found");

        //            return Ok(new CategoryResponseDto
        //            {
        //                Id = category.Id,
        //                Name = category.Name,
        //                Description = category.Description
        //            });
        //        }


        //        [Authorize(Roles = "Admin")]
        //        // Update Category
        //        [HttpPut("{id}")]
        //        public async Task<IActionResult> UpdateCategory(int id, UpdateCategoryDto dto)
        //        {
        //            var category = await _context.Categories.FindAsync(id);
        //            if (category == null)
        //                return NotFound("Category not found");

        //            category.Name = dto.Name;
        //            category.Description = dto.Description;

        //            await _context.SaveChangesAsync();

        //            return Ok(new CategoryResponseDto
        //            {
        //                Id = category.Id,
        //                Name = category.Name,
        //                Description = category.Description
        //            });
        //        }


        //        [Authorize(Roles = "Admin")]
        //        // Delete Category
        //        [HttpDelete("{id}")]
        //        public async Task<IActionResult> DeleteCategory(int id)
        //        {
        //            var category = await _context.Categories.FindAsync(id);
        //            if (category == null)
        //                return NotFound("Category not found");

        //            _context.Categories.Remove(category);
        //            await _context.SaveChangesAsync();

        //            return Ok(new { message = "Category deleted successfully" });
        //        }
        //    }
        //}


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
                DescriptionEn = dto.DescriptionEn
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
                IsActive = true
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
                DescriptionEn = c.DescriptionEn
            });

            return Ok(result);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
                return NotFound("Category not found");

            return Ok(new CategoryAdminDto
            {
                Id = category.Id,
                NameAr = category.NameAr ?? category.Name ?? string.Empty,
                NameEn = category.NameEn ?? category.Name ?? string.Empty,
                DescriptionAr = category.DescriptionAr,
                DescriptionEn = category.DescriptionEn
            });
        }



        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, UpdateCategoryDto dto)
        {
            var category = await _context.Categories.FindAsync(id);

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
                DescriptionEn = category.DescriptionEn
            });
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
                return NotFound("Category not found");

            _context.Categories.Remove(category);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Category deleted successfully"
            });
        }
    }
}


