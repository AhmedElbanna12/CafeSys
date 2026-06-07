//using Foodics.Dtos.Admin.Category;
//using Foodics.ExtensionMethod;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using POSSystem.Data;

//namespace Foodics.Controllers
//{
//    [Route("api/categories")]
//    [ApiController]
//    public class CategoriesController : ControllerBase
//    {
//        private readonly ApplicationDbContext _context;

//        public CategoriesController(ApplicationDbContext context)
//        {
//            _context = context;
//        }

//        // GET: api/categories
//        [HttpGet]
//        public async Task<IActionResult> GetCategories()
//        {
//            var lang = Request.Headers["Accept-Language"]
//                .ToString()
//                .StartsWith("ar")
//                ? "ar"
//                : "en";

//            var categories = await _context.Categories
//                .Where(c => c.IsActive)
//                .ToListAsync();

//            var result = categories.Select(c => new CategoryResponseDto
//            {
//                Id = c.Id,

//                Name = LocalizationExtensions.Localize(
//                    c.NameAr,
//                    c.NameEn,
//                    lang),

//                Description = LocalizationExtensions.Localize(
//                    c.DescriptionAr,
//                    c.DescriptionEn,
//                    lang)
//            });

//            return Ok(result);
//        }

//        // GET: api/categories/5
//        [HttpGet("{id}")]
//        public async Task<IActionResult> GetCategory(int id)
//        {
//            var lang = Request.Headers["Accept-Language"]
//                .ToString()
//                .StartsWith("ar")
//                ? "ar"
//                : "en";

//            var category = await _context.Categories
//                .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);

//            if (category == null)
//                return NotFound("Category not found");

//            return Ok(new CategoryResponseDto
//            {
//                Id = category.Id,

//                Name = LocalizationExtensions.Localize(
//                    category.NameAr,
//                    category.NameEn,
//                    lang),

//                Description = LocalizationExtensions.Localize(
//                    category.DescriptionAr,
//                    category.DescriptionEn,
//                    lang)
//            });
//        }
//    }
//}


using Foodics.Dtos.Admin.Category;
using Foodics.ExtensionMethod;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POSSystem.Data;

namespace Foodics.Controllers
{
    [Route("api/categories")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 🔹 Extract language from Accept-Language header
        private string GetLang()
        {
            var langHeader = Request.Headers["Accept-Language"].ToString();

            return langHeader
                .Split(',')[0]
                .Trim()
                .ToLower()
                .StartsWith("ar")
                ? "ar"
                : "en";
        }

        // GET: api/categories
        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var lang = GetLang();

            var categories = await _context.Categories
                .Where(c => c.IsActive)
                .ToListAsync();

            var result = categories.Select(c => new CategoryResponseDto
            {
                Id = c.Id,

                Name = LocalizationExtensions.Localize(
                    c.NameAr,
                    c.NameEn,
                    lang),

                Description = LocalizationExtensions.Localize(
                    c.DescriptionAr,
                    c.DescriptionEn,
                    lang)
            });

            return Ok(result);
        }

        // GET: api/categories/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategory(int id)
        {
            var lang = GetLang();

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);

            if (category == null)
                return NotFound("Category not found");

            var result = new CategoryResponseDto
            {
                Id = category.Id,

                Name = LocalizationExtensions.Localize(
                    category.NameAr,
                    category.NameEn,
                    lang),

                Description = LocalizationExtensions.Localize(
                    category.DescriptionAr,
                    category.DescriptionEn,
                    lang)
            };

            return Ok(result);
        }
    }
}