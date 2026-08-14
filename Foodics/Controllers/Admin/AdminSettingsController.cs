using Foodics.Dtos.Admin.Settings;
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
            var pointsSettings = await _context.PointsSettings.FirstOrDefaultAsync();

            var egpPerPoint = pointsSettings?.EgpPerPoint ?? 20m;

            if (settings == null)
            {
                return Ok(new
                {
                    DeliveryFee = 0,
                    IsDeliveryEnabled = true,
                    IsPickupEnabled = true,
                    EgpPerPoint = egpPerPoint
                });
            }

            return Ok(new
            {
                settings.DeliveryFee,
                settings.IsDeliveryEnabled,
                settings.IsPickupEnabled,
                EgpPerPoint = egpPerPoint
            });
        }

        // 🔹 Get points settings
        [Authorize]
        [HttpGet("points")]
        public async Task<IActionResult> GetPointsSettings()
        {
            var pointsSettings = await _context.PointsSettings.FirstOrDefaultAsync();

            if (pointsSettings == null)
            {
                return Ok(new PointsSettingsDto
                {
                    EgpPerPoint = 20m
                });
            }

            return Ok(new PointsSettingsDto
            {
                EgpPerPoint = pointsSettings.EgpPerPoint
            });
        }

        // 🔹 Update points settings
        [Authorize(Roles = "Admin")]
        [HttpPost("points")]
        public async Task<IActionResult> UpdatePointsSettings([FromBody] UpdatePointsSettingsDto dto)
        {
            if (dto == null || dto.EgpPerPoint <= 0)
            {
                return BadRequest("Point value must be greater than 0.");
            }

            var pointsSettings = await _context.PointsSettings.FirstOrDefaultAsync();

            if (pointsSettings == null)
            {
                pointsSettings = new PointsSettings
                {
                    EgpPerPoint = dto.EgpPerPoint,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.PointsSettings.Add(pointsSettings);
            }
            else
            {
                pointsSettings.EgpPerPoint = dto.EgpPerPoint;
                pointsSettings.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Points settings updated successfully",
                egpPerPoint = pointsSettings.EgpPerPoint
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("points")]
        public async Task<IActionResult> UpdatePointsSettingsPut([FromBody] UpdatePointsSettingsDto dto)
        {
            return await UpdatePointsSettings(dto);
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