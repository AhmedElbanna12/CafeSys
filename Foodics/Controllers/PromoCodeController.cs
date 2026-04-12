using Foodics.Dtos.Cart.Promocode;
using Foodics.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POSSystem.Data;

namespace Foodics.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]

    public class PromoCodeController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PromoCodeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ➕ Add PromoCode
        [HttpPost("add")]
        public async Task<IActionResult> AddPromoCode(AddPromoCodeDto dto)
        {
            if (await _context.PromoCodes.AnyAsync(p => p.Code == dto.Code))
                return BadRequest("Promo code already exists.");

            var promo = new PromoCode
            {
                Code = dto.Code,
                DiscountAmount = dto.DiscountAmount,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                IsActive = true
            };

            _context.PromoCodes.Add(promo);
            await _context.SaveChangesAsync();

            return Ok(new { promo.Id, promo.Code, promo.DiscountAmount, promo.StartDate, promo.EndDate });
        }

        // ✅ Validate PromoCode for Cart
        [HttpGet("validate/{code}")]
        public async Task<IActionResult> ValidatePromoCode(string code)
        {
            var promo = await _context.PromoCodes
                .Where(p => p.Code == code && p.IsActive && p.StartDate <= DateTime.UtcNow && p.EndDate >= DateTime.UtcNow)
                .FirstOrDefaultAsync();

            if (promo == null)
                return NotFound("Invalid or expired promo code.");

            return Ok(new { promo.Id, promo.Code, promo.DiscountAmount });
        }
    }
}
