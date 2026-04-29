using Foodics.Dtos.Cart.Promocode;
using Foodics.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POSSystem.Data;

namespace Foodics.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PromoCodeController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PromoCodeController(ApplicationDbContext context)
        {
            _context = context;
        }

        //[Authorize(Roles = "Admin")]
        [HttpPost("add")]
        public async Task<IActionResult> AddPromoCode(AddPromoCodeDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Code))
                return BadRequest("Promo code is required.");

            var normalizedCode = dto.Code.Trim().ToLower();

            var exists = await _context.PromoCodes
                .AnyAsync(p => p.Code != null && p.Code.Trim().ToLower() == normalizedCode);

            if (exists)
                return BadRequest("Promo code already exists.");

            var promo = new PromoCode
            {
                Code = normalizedCode,
                DiscountAmount = dto.DiscountAmount,
                StartDate = DateTime.SpecifyKind(dto.StartDate, DateTimeKind.Utc),
                EndDate = DateTime.SpecifyKind(dto.EndDate, DateTimeKind.Utc),
                IsActive = true
            };
            _context.PromoCodes.Add(promo);
            await _context.SaveChangesAsync();

            // logging هنا صح
            Console.WriteLine($"Local: {DateTime.Now}");
            Console.WriteLine($"UTC: {DateTime.UtcNow}");

            return Ok(new
            {
                promo.Id,
                promo.Code,
                promo.DiscountAmount,
                promo.StartDate,
                promo.EndDate
            });
        }

        [AllowAnonymous]
        [HttpGet("validate/{code}")]
        public async Task<IActionResult> ValidatePromoCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return BadRequest("Promo code is required.");

            var normalizedCode = code.Trim().ToLower();

            var now = DateTime.UtcNow; // 🔥 مهم جدًا

            var promo = await _context.PromoCodes
                .FirstOrDefaultAsync(p =>
                    p.Code != null &&
                    p.Code.Trim().ToLower() == normalizedCode &&
                    p.IsActive &&
                    p.StartDate <= now &&
                    p.EndDate >= now);

            if (promo == null)
                return NotFound("Invalid or expired promo code.");

            return Ok(new
            {
                promo.Id,
                promo.Code,
                promo.DiscountAmount
            });
        }
    }
}