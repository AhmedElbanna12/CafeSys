using Foodics.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POSSystem.Data;

namespace Foodics.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/settings")]
    [Authorize(Roles = "Admin")]
    public class AdminSettingsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AdminSettingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 🔹 Get current settings
        [HttpGet]
        public async Task<IActionResult> GetSettings()
        {
            var settings = await _context.AppSettings.FirstOrDefaultAsync();

            if (settings == null)
                return Ok(new { DeliveryFee = 0 });

            return Ok(settings);
        }

        // 🔹 Update delivery fee
        [HttpPost("delivery-fee")]
        public async Task<IActionResult> UpdateDeliveryFee(decimal fee)
        {
            var settings = await _context.AppSettings.FirstOrDefaultAsync();

            if (settings == null)
            {
                settings = new AppSettings
                {
                    DeliveryFee = fee
                };

                _context.AppSettings.Add(settings);
            }
            else
            {
                settings.DeliveryFee = fee;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Delivery fee updated successfully",
                DeliveryFee = fee
            });
        }
    }
}
