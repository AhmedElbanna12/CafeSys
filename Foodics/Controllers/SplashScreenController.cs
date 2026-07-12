using Foodics.Dtos.SplashScreen;
using Foodics.Models;
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

        public SplashScreenController(ApplicationDbContext context,
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
                    Photo = x.Photo
                })
                .ToListAsync();

            return Ok(data);
        }

        //======================== Create ========================

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateSplashScreenDto dto)
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = Guid.NewGuid() + Path.GetExtension(dto.Photo.FileName);

            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await dto.Photo.CopyToAsync(stream);
            }

            var splash = new SplashScreen
            {
                Title = dto.Title,
                Description = dto.Description,
                Photo = "/uploads/" + fileName
            };

            _context.SplashScreens.Add(splash);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Created Successfully"
            });
        }

        //======================== Edit ========================

        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id, [FromForm] EditSplashScreenDto dto)
        {
            var splash = await _context.SplashScreens.FindAsync(id);

            if (splash == null)
                return NotFound();

            splash.Title = dto.Title;
            splash.Description = dto.Description;

            if (dto.Photo != null)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");

                var fileName = Guid.NewGuid() + Path.GetExtension(dto.Photo.FileName);

                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.Photo.CopyToAsync(stream);
                }

                splash.Photo = "/uploads/" + fileName;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Updated Successfully"
            });
        }

        //======================== Delete ========================

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