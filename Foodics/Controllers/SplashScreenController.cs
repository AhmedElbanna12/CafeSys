using Foodics.Dtos.SplashScreen;
using Foodics.ExtensionMethod;
using Foodics.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POSSystem.Data;

namespace Foodics.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SplashScreenController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public SplashScreenController(
            ApplicationDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }


        private string GetLang()
        {
            return Request.Headers["Accept-Language"].ToString().StartsWith("ar")
                ? "ar"
                : "en";
        }



        //======================== Get ========================
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var lang = GetLang();

            var data = await _context.SplashScreens
                .Where(x => x.IsVisible)
                .Select(x => new SplashScreenDto
                {
                    Id = x.Id,
                    Title = LocalizationExtensions.Localize(x.TitleAr, x.TitleEn, lang),
                    Description = LocalizationExtensions.Localize(x.DescriptionAr, x.DescriptionEn, lang),
                    Photo = x.Photo,
                    Video = x.Video
                })
                .ToListAsync();

            return Ok(data);
        }


        [Authorize(Roles = "Admin")]
        [HttpGet("admin")]
        public async Task<IActionResult> GetForAdmin()
        {
            var data = await _context.SplashScreens
                .OrderByDescending(x => x.Id)
                .Select(x => new
                {
                    x.Id,
                    x.TitleAr,
                    x.TitleEn,
                    x.DescriptionAr,
                    x.DescriptionEn,
                    x.Photo,
                    x.Video,
                    x.IsVisible
                })
                .ToListAsync();

            return Ok(data);
        }



        [Authorize(Roles = "Admin")]
        [HttpPatch("{id}/visibility")]
        public async Task<IActionResult> ChangeVisibility(
    int id,
    [FromBody] ChangeSplashVisibilityDto dto)
        {
            var splash = await _context.SplashScreens.FindAsync(id);

            if (splash == null)
                return NotFound(new
                {
                    Message = "Splash screen not found."
                });

            splash.IsVisible = dto.IsVisible;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = $"Splash screen visibility updated to {(dto.IsVisible ? "Visible" : "Hidden")}.",
                splash.Id,
                splash.IsVisible
            });
        }

        //======================== Create ========================
        [Authorize(Roles = "Admin")]
        [HttpPost]

        public async Task<IActionResult> Create([FromForm] CreateSplashScreenDto dto)
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            string? photoPath = null;
            string? videoPath = null;

            // Upload Photo
            if (dto.Photo != null)
            {
                var photoName = Guid.NewGuid() + Path.GetExtension(dto.Photo.FileName);
                var photoFilePath = Path.Combine(uploadsFolder, photoName);

                using (var stream = new FileStream(photoFilePath, FileMode.Create))
                {
                    await dto.Photo.CopyToAsync(stream);
                }

                photoPath = "/uploads/" + photoName;
            }

            // Upload Video
            if (dto.Video != null)
            {
                var videoName = Guid.NewGuid() + Path.GetExtension(dto.Video.FileName);
                var videoFilePath = Path.Combine(uploadsFolder, videoName);

                using (var stream = new FileStream(videoFilePath, FileMode.Create))
                {
                    await dto.Video.CopyToAsync(stream);
                }

                videoPath = "/uploads/" + videoName;
            }

            var splash = new SplashScreen
            {
                TitleAr = dto.TitleAr,
                TitleEn = dto.TitleEn,
                DescriptionAr = dto.DescriptionAr,
                DescriptionEn = dto.DescriptionEn,
                Photo = photoPath,
                Video = videoPath,
                IsVisible = dto.IsVisible
            };

            _context.SplashScreens.Add(splash);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Created Successfully"
            });
        }

        //======================== Edit ========================
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id, [FromForm] EditSplashScreenDto dto)
        {
            var splash = await _context.SplashScreens.FindAsync(id);

            if (splash == null)
                return NotFound();

            if (!string.IsNullOrWhiteSpace(dto.TitleAr))
                splash.TitleAr = dto.TitleAr;

            if (!string.IsNullOrWhiteSpace(dto.TitleEn))
                splash.TitleEn = dto.TitleEn;

            if (!string.IsNullOrWhiteSpace(dto.DescriptionAr))
                splash.DescriptionAr = dto.DescriptionAr;

            if (!string.IsNullOrWhiteSpace(dto.DescriptionEn))
                splash.DescriptionEn = dto.DescriptionEn;

            if (dto.IsVisible.HasValue)
                splash.IsVisible = dto.IsVisible.Value;

            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            // Update Photo
            if (dto.Photo != null)
            {
                var photoName = Guid.NewGuid() + Path.GetExtension(dto.Photo.FileName);
                var photoFilePath = Path.Combine(uploadsFolder, photoName);

                using (var stream = new FileStream(photoFilePath, FileMode.Create))
                {
                    await dto.Photo.CopyToAsync(stream);
                }

                splash.Photo = "/uploads/" + photoName;
            }

            // Update Video
            if (dto.Video != null)
            {
                var videoName = Guid.NewGuid() + Path.GetExtension(dto.Video.FileName);
                var videoFilePath = Path.Combine(uploadsFolder, videoName);

                using (var stream = new FileStream(videoFilePath, FileMode.Create))
                {
                    await dto.Video.CopyToAsync(stream);
                }

                splash.Video = "/uploads/" + videoName;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Updated Successfully"
            });
        }

        //======================== Delete ========================
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var splash = await _context.SplashScreens.FindAsync(id);

            if (splash == null)
                return NotFound();

            _context.SplashScreens.Remove(splash);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Deleted Successfully"
            });
        }
    }
}