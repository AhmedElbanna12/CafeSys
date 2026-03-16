using Foodics.Dtos.Admin;
using Foodics.Dtos.Admin.Category;
using Foodics.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POSSystem.Data;

namespace Foodics.Controllers
{
    [Route("api/admin/categories")]
    [ApiController]
    //[Authorize(Roles = "Admin")]
    public class AdminCategoriesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AdminCategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Create Category
        [HttpPost]
        public async Task<IActionResult> CreateCategory(CreateCategoryDto dto)
        {
            var category = new Category
            {
                Name = dto.Name,
                Description = dto.Description
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return Ok(new CategoryResponseDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description
            });
        }

        // Get All Categories
        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _context.Categories.ToListAsync();

            var result = categories.Select(c => new CategoryResponseDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description
            });

            return Ok(result);
        }

        // Get Single Category
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return NotFound("Category not found");

            return Ok(new CategoryResponseDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description
            });
        }

        // Update Category
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, UpdateCategoryDto dto)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return NotFound("Category not found");

            category.Name = dto.Name;
            category.Description = dto.Description;

            await _context.SaveChangesAsync();

            return Ok(new CategoryResponseDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description
            });
        }

        // Delete Category
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return NotFound("Category not found");

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Category deleted successfully" });
        }
    }
}