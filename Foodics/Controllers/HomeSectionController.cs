using Foodics.Dtos.HomeSection;
using Foodics.Models;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POSSystem.Data;
using Twilio.Rest.PreviewIam.Organizations;

namespace Foodics.Controllers.Admin
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeSectionController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public HomeSectionController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/HomeSection
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var sections = await _context.HomeSections
       .OrderBy(x => x.DisplayOrder)
       .Select(x => new HomeSectionDto
       {
           Id = x.Id,
           SectionName = x.SectionName.ToString(),
           DisplayOrder = x.DisplayOrder,
           IsVisible = x.IsVisible
       })
       .ToListAsync();

            return Ok(sections);
        }

        // PUT: api/HomeSection/order
        [Authorize(Roles = "Admin")]
        [HttpPut("order")]
        public async Task<IActionResult> UpdateOrder([FromBody] List<UpdateHomeSectionOrderDto> model)
        {
            if (model == null || !model.Any())
                return BadRequest("Invalid request.");

            foreach (var item in model)
            {
                var section = await _context.HomeSections.FindAsync(item.Id);

                if (section == null)
                    return NotFound($"Section with id {item.Id} was not found.");

                section.DisplayOrder = item.DisplayOrder;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Home sections order updated successfully."
            });
        }

        // PUT: api/HomeSection/visibility
        [Authorize(Roles = "Admin")]
        [HttpPut("visibility")]
        public async Task<IActionResult> UpdateVisibility([FromBody] UpdateHomeSectionVisibilityDto model)
        {
            var section = await _context.HomeSections.FindAsync(model.Id);

            if (section == null)
                return NotFound();

            section.IsVisible = model.IsVisible;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Home section visibility updated successfully."
            });
        }

        // GET: api/HomeSection/types
        // (اختياري) لو الفرونت محتاج يعرف أسماء الأقسام
        [HttpGet("types")]
        public IActionResult GetSectionTypes()
        {
            var types = Enum.GetValues<HomeSectionType>()
                .Select(x => new
                {
                    Id = (int)x,
                    Name = x.ToString()
                });

            return Ok(types);
        }
    }
}