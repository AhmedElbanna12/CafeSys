using Foodics.Dtos.Location;
using Foodics.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POSSystem.Data;
using System.Security.Claims;

namespace Foodics.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LocationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LocationsController(ApplicationDbContext context)
        {
            _context = context;
        }


        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        }



        // =========================
        // 📍 Get My Locations
        // =========================
        [HttpGet]
        public async Task<IActionResult> GetMyLocations()
        {
            var userId = GetUserId();

            var locations = await _context.UserLocations
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.IsDefault)
                .ThenByDescending(x => x.Id)
                .ToListAsync();


            return Ok(locations);
        }




        // =========================
        // 📍 Get Location By Id
        // =========================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var userId = GetUserId();


            var location = await _context.UserLocations
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.UserId == userId);


            if (location == null)
                return NotFound("Location not found");


            return Ok(location);
        }




        // =========================
        // ➕ Add Location
        // =========================
        [HttpPost]
        public async Task<IActionResult> Add(UserLocationDto dto)
        {
            var userId = GetUserId();


            var location = new UserLocation
            {
                UserId = userId,

                City = dto.City,
                Street = dto.Street,

                BuildingNumber = dto.BuildingNumber,
                FloorNumber = dto.FloorNumber,
                ApartmentNumber = dto.ApartmentNumber,

                Landmark = dto.Landmark,
                PhoneNumber = dto.PhoneNumber,


                Latitude = dto.Latitude,
                Longitude = dto.Longitude,


                IsDefault = dto.IsDefault
            };


            // لو أول Location للمستخدم نخليه Default
            var hasLocations = await _context.UserLocations
                .AnyAsync(x => x.UserId == userId);


            if (!hasLocations)
                location.IsDefault = true;



            if (location.IsDefault)
            {
                var oldDefaults = await _context.UserLocations
                    .Where(x => x.UserId == userId)
                    .ToListAsync();


                foreach (var item in oldDefaults)
                {
                    item.IsDefault = false;
                }
            }



            _context.UserLocations.Add(location);

            await _context.SaveChangesAsync();


            return Ok(location);
        }





        // =========================
        // ✏️ Update Location
        // =========================
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            int id,
            UserLocationDto dto)
        {
            var userId = GetUserId();


            var location = await _context.UserLocations
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.UserId == userId);



            if (location == null)
                return NotFound("Location not found");



            location.City = dto.City;
            location.Street = dto.Street;

            location.BuildingNumber = dto.BuildingNumber;
            location.FloorNumber = dto.FloorNumber;
            location.ApartmentNumber = dto.ApartmentNumber;

            location.Landmark = dto.Landmark;
            location.PhoneNumber = dto.PhoneNumber;


            location.Latitude = dto.Latitude;
            location.Longitude = dto.Longitude;



            if (dto.IsDefault)
            {
                var oldLocations = await _context.UserLocations
                    .Where(x => x.UserId == userId)
                    .ToListAsync();


                foreach (var item in oldLocations)
                {
                    item.IsDefault = false;
                }

                location.IsDefault = true;
            }



            await _context.SaveChangesAsync();


            return Ok(location);
        }





        // =========================
        // ⭐ Set Default Location
        // =========================
        [HttpPut("{id}/default")]
        public async Task<IActionResult> SetDefault(int id)
        {
            var userId = GetUserId();


            var locations = await _context.UserLocations
                .Where(x => x.UserId == userId)
                .ToListAsync();



            var location = locations
                .FirstOrDefault(x => x.Id == id);



            if (location == null)
                return NotFound("Location not found");



            foreach (var item in locations)
            {
                item.IsDefault = false;
            }


            location.IsDefault = true;


            await _context.SaveChangesAsync();


            return Ok(new
            {
                message = "Default location updated"
            });
        }





        // =========================
        // 🗑 Delete Location
        // =========================
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetUserId();


            var location = await _context.UserLocations
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.UserId == userId);



            if (location == null)
                return NotFound("Location not found");



            _context.UserLocations.Remove(location);


            await _context.SaveChangesAsync();



            return Ok(new
            {
                message = "Location deleted"
            });
        }
    }
}