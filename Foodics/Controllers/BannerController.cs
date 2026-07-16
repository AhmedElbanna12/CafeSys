using Foodics.Dtos.Banner;
using Foodics.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POSSystem.Data;

namespace Foodics.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BannerController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public BannerController(
            ApplicationDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }


        // POST: api/Banner
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateBannerDto dto)
        {
            string? photoName = null;

            if (dto.Photo != null)
            {
                var folder = Path.Combine(_environment.WebRootPath, "Uploads", "Banners");

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                photoName = Guid.NewGuid() + Path.GetExtension(dto.Photo.FileName);

                var path = Path.Combine(folder, photoName);

                using var stream = new FileStream(path, FileMode.Create);
                await dto.Photo.CopyToAsync(stream);
            }

            var banner = new Banner
            {
                Title = dto.Title,
                Description = dto.Description,
                SubDescription = dto.SubDescription,
                Photo = photoName
            };

            _context.Banners.Add(banner);
            await _context.SaveChangesAsync();

            return Ok(banner);
        }

        // GET: api/Banner
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var banners = await _context.Banners
                .OrderByDescending(x => x.Id)
                .ToListAsync();

            return Ok(banners);
        }

        // PUT: api/Banner/{id}
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateBannerDto dto)
        {
            var banner = await _context.Banners.FindAsync(id);

            if (banner == null)
                return NotFound();

            if (!string.IsNullOrWhiteSpace(dto.Title))
                banner.Title = dto.Title;

            if (!string.IsNullOrWhiteSpace(dto.Description))
                banner.Description = dto.Description;

            if (!string.IsNullOrWhiteSpace(dto.SubDescription))
                banner.SubDescription = dto.SubDescription;

            if (dto.Photo != null)
            {
                if (!string.IsNullOrEmpty(banner.Photo))
                {
                    var oldPath = Path.Combine(
                        _environment.WebRootPath,
                        "Uploads",
                        "Banners",
                        banner.Photo);

                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
                }

                var folder = Path.Combine(_environment.WebRootPath, "Uploads", "Banners");

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                var fileName = Guid.NewGuid() + Path.GetExtension(dto.Photo.FileName);

                var path = Path.Combine(folder, fileName);

                using var stream = new FileStream(path, FileMode.Create);
                await dto.Photo.CopyToAsync(stream);

                banner.Photo = fileName;
            }

            await _context.SaveChangesAsync();

            return Ok(banner);
        }

        // DELETE: api/Banner/{id}
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var banner = await _context.Banners.FindAsync(id);

            if (banner == null)
                return NotFound();

            if (!string.IsNullOrEmpty(banner.Photo))
            {
                var path = Path.Combine(
                    _environment.WebRootPath,
                    "Uploads",
                    "Banners",
                    banner.Photo);

                if (System.IO.File.Exists(path))
                    System.IO.File.Delete(path);
            }

            _context.Banners.Remove(banner);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Banner deleted successfully."
            });
        }
    }
}