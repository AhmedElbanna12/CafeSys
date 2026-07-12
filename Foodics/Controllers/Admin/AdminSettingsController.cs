using Foodics.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POSSystem.Data;

namespace Foodics.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/settings")]
    public class AdminSettingsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AdminSettingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 🔹 Get current settings
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetSettings()
        {
            var settings = await _context.AppSettings.FirstOrDefaultAsync();

            if (settings == null)
            {
                return Ok(new
                {
                    DeliveryFee = 0,
                    IsDeliveryEnabled = true,
                    IsPickupEnabled = true
                });
            }

            return Ok(new
            {
                settings.DeliveryFee,
                settings.IsDeliveryEnabled,
                settings.IsPickupEnabled
            });
        }

        // 🔹 Update delivery fee
        [Authorize(Roles = "Admin")]
        [HttpPost("delivery-fee")]
        public async Task<IActionResult> UpdateDeliveryFee(decimal fee)
        {
            var settings = await _context.AppSettings.FirstOrDefaultAsync();

            if (settings == null)
            {
                settings = new AppSettings
                {
                    DeliveryFee = fee,
                    IsDeliveryEnabled = true,
                    IsPickupEnabled = true
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
                message = "Delivery fee updated successfully",
                deliveryFee = settings.DeliveryFee
            });
        }

        // 🔹 Enable / Disable Delivery
        [Authorize(Roles = "Admin")]
        [HttpPatch("delivery")]
        public async Task<IActionResult> ToggleDelivery([FromBody] bool isEnabled)
        {
            var settings = await _context.AppSettings.FirstOrDefaultAsync();

            if (settings == null)
                return NotFound("AppSettings not found");

            settings.IsDeliveryEnabled = isEnabled;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Delivery status updated",
                isEnabled
            });
        }

        // 🔹 Enable / Disable Pickup
        [Authorize(Roles = "Admin")]
        [HttpPatch("pickup")]
        public async Task<IActionResult> TogglePickup([FromBody] bool isEnabled)
        {
            var settings = await _context.AppSettings.FirstOrDefaultAsync();

            if (settings == null)
                return NotFound("AppSettings not found");

            settings.IsPickupEnabled = isEnabled;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Pickup status updated",
                isEnabled
            });
        }
    }
}