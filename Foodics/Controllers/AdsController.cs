using Foodics.Dtos.Adv;
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
    public class AdsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public AdsController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: api/Ads
        [HttpGet]
        public async Task<IActionResult> GetAds()
        {
            var ads = await _context.Advertisements
                .Where(a => a.IsActive)
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new AdDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    Description = a.Description,
                    ImageUrl = $"/uploads/{Path.GetFileName(a.ImagePath)}",
                    CreatedAt = a.CreatedAt
                })
                .ToListAsync();

            return Ok(ads);
        }


        [Authorize(Roles = "Admin")]
        // POST: api/Ads
        [HttpPost]
        public async Task<IActionResult> CreateAd([FromForm] CreateAdDto dto)
        {
            if (dto.Image == null || dto.Image.Length == 0)
                return BadRequest("Image is required");

            // حفظ الصورة في مجلد wwwroot/uploads
            var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var uploadsFolder = Path.Combine(webRoot, "images", "uploadsadv");

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.Image.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await dto.Image.CopyToAsync(stream);

            // إنشاء الإعلان
            var ad = new Advertisement
            {
                Title = dto.Title,
                Description = dto.Description,
                ImagePath = filePath
            };

            _context.Advertisements.Add(ad);
            await _context.SaveChangesAsync();

            return Ok(new AdDto
            {
                Id = ad.Id,
                Title = ad.Title,
                Description = ad.Description,
                ImageUrl = $"/uploads/{fileName}",
                CreatedAt = ad.CreatedAt
            });
        }



        [Authorize(Roles = "Admin")]
        // DELETE: api/Ads/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAd(int id)
        {
            var ad = await _context.Advertisements.FindAsync(id);
            if (ad == null)
                return NotFound("Ad not found");

            // حذف الصورة من المجلد لو موجودة
            if (!string.IsNullOrEmpty(ad.ImagePath) && System.IO.File.Exists(ad.ImagePath))
            {
                System.IO.File.Delete(ad.ImagePath);
            }

            _context.Advertisements.Remove(ad);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Ad deleted successfully" });
        }
    }
}
