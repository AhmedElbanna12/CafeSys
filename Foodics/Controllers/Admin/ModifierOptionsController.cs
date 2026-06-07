using Foodics.Dtos.Admin.Product.ProductModifierOption;
using Foodics.ExtensionMethod;
using Foodics.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POSSystem.Data;

namespace Foodics.Controllers.Admin
{
    [Route("api/admin/modifier-groups/{groupId}/options")]
    [ApiController]
    //[Authorize(Roles = "Admin")]
    public class ModifierOptionsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ModifierOptionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ➕ Create Option
        [HttpPost]
        public async Task<IActionResult> Create(int groupId, CreateModifierOptionDto dto)
        {
            var group = await _context.ModifierGroups.FindAsync(groupId);
            if (group == null)
                return NotFound("Modifier group not found");

            var option = new ModifierOption
            {
                ModifierGroupId = groupId,
                NameAr = dto.NameAr,
                NameEn = dto.NameEn,
                ExtraPrice = dto.ExtraPrice
            };

            _context.ModifierOptions.Add(option);
            await _context.SaveChangesAsync();

            return Ok(Map(option, "en"));
        }

        // ✏️ Update Option
        [HttpPut("{optionId}")]
        public async Task<IActionResult> Update(int groupId, int optionId, UpdateModifierOptionDto dto)
        {
            var option = await _context.ModifierOptions
                .FirstOrDefaultAsync(x => x.Id == optionId && x.ModifierGroupId == groupId);

            if (option == null)
                return NotFound("Option not found");

            if (dto.NameAr != null) option.NameAr = dto.NameAr;
            if (dto.NameEn != null) option.NameEn = dto.NameEn;
            if (dto.ExtraPrice.HasValue) option.ExtraPrice = dto.ExtraPrice.Value;

            await _context.SaveChangesAsync();

            return Ok(Map(option, "en"));
        }

        // ❌ Delete Option
        [HttpDelete("{optionId}")]
        public async Task<IActionResult> Delete(int groupId, int optionId)
        {
            var option = await _context.ModifierOptions
                .FirstOrDefaultAsync(x => x.Id == optionId && x.ModifierGroupId == groupId);

            if (option == null)
                return NotFound("Option not found");

            _context.ModifierOptions.Remove(option);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Deleted successfully" });
        }

        // 📄 Get All Options
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll(int groupId, [FromQuery] string lang = "en")
        {
            var options = await _context.ModifierOptions
                .Where(x => x.ModifierGroupId == groupId)
                .ToListAsync();

            var result = options.Select(o => Map(o, lang));

            return Ok(result);
        }

        private ModifierOptionDto Map(ModifierOption o, string lang)
        {
            return new ModifierOptionDto
            {
                Id = o.Id,
                Name = LocalizationExtensions.Localize(o.NameAr, o.NameEn, lang),
                ExtraPrice = o.ExtraPrice
            };
        }
    }
}