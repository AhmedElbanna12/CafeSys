using Foodics.Dtos.Admin.Product.ProductModifierGroup;
using Foodics.ExtensionMethod;
using Foodics.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POSSystem.Data;

namespace Foodics.Controllers.Admin
{
    [Route("api/admin/products/{productId}/modifier-groups")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class ModifierGroupsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ModifierGroupsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ➕ Create Group
        [HttpPost]
        public async Task<IActionResult> Create(int productId, CreateModifierGroupDto dto)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
                return NotFound("Product not found");

            var group = new ModifierGroup
            {
                ProductId = productId,
                NameAr = dto.NameAr,
                NameEn = dto.NameEn,
                IsRequired = dto.IsRequired,
                MaxSelections = dto.MaxSelections
            };

            _context.ModifierGroups.Add(group);
            await _context.SaveChangesAsync();

            return Ok(Map(group, "en"));
        }

        // ✏️ Update Group
        [HttpPut("{groupId}")]
        public async Task<IActionResult> Update(int productId, int groupId, UpdateModifierGroupDto dto)
        {
            var group = await _context.ModifierGroups
                .FirstOrDefaultAsync(x => x.Id == groupId && x.ProductId == productId);

            if (group == null)
                return NotFound("Modifier group not found");

            if (dto.NameAr != null) group.NameAr = dto.NameAr;
            if (dto.NameEn != null) group.NameEn = dto.NameEn;
            if (dto.IsRequired.HasValue) group.IsRequired = dto.IsRequired.Value;
            if (dto.MaxSelections.HasValue) group.MaxSelections = dto.MaxSelections.Value;

            await _context.SaveChangesAsync();

            return Ok(Map(group, "en"));
        }

        // ❌ Delete Group
        [HttpDelete("{groupId}")]
        public async Task<IActionResult> Delete(int productId, int groupId)
        {
            var group = await _context.ModifierGroups
                .Include(x => x.Options)
                .FirstOrDefaultAsync(x => x.Id == groupId && x.ProductId == productId);

            if (group == null)
                return NotFound("Modifier group not found");

            _context.ModifierGroups.Remove(group);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Deleted successfully" });
        }

        // 📄 Get All Groups
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll(int productId, [FromQuery] string lang = "en")
        {
            var groups = await _context.ModifierGroups
                .Include(x => x.Options)
                .Where(x => x.ProductId == productId)
                .ToListAsync();

            var result = groups.Select(g => Map(g, lang));

            return Ok(result);
        }

        private ModifierGroupDto Map(ModifierGroup g, string lang)
        {
            return new ModifierGroupDto
            {
                Id = g.Id,
                Name = LocalizationExtensions.Localize(g.NameAr, g.NameEn, lang),
                IsRequired = g.IsRequired,
                MaxSelections = g.MaxSelections,
                Options = g.Options?.Select(o => new Foodics.Dtos.Admin.Product.ProductModifierOption.ModifierOptionDto
                {
                    Id = o.Id,
                    Name = LocalizationExtensions.Localize(o.NameAr, o.NameEn, lang),
                    ExtraPrice = o.ExtraPrice
                }).ToList() ?? new List<Foodics.Dtos.Admin.Product.ProductModifierOption.ModifierOptionDto>()
            };
        }
    }
}