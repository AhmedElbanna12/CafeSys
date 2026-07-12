using Foodics.Dtos.SplashScreen;
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

        //======================== Get ========================

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var data = await _context.SplashScreens
                .Select(x => new SplashScreenDto
                {
                    Id = x.Id,
                    Title = x.Title,
                    Description = x.Description,
                    Photo = x.Photo,
                    Video = x.Video
                })
                .ToListAsync();

            return Ok(data);
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
                Title = dto.Title,
                Description = dto.Description,
                Photo = photoPath,
                Video = videoPath
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

            splash.Title = dto.Title;
            splash.Description = dto.Description;

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