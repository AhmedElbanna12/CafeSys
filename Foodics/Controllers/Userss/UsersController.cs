using Foodics.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AppUser = Foodics.Models.User;

namespace Foodics.Controllers.User
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;

        public UsersController(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = User.FindFirstValue("userId");
            // أو استخدم ClaimTypes.NameIdentifier لو ده الموجود في الـ JWT

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return NotFound(new { message = "User not found" });

            // المستخدم المحذوف يخرج من التطبيق
            if (user.IsDeleted)
            {
                return Unauthorized(new
                {
                    message = "This account has been deleted."
                });
            }

            // المستخدم المحظور يدخل عادي لكن يرجع حالته
            return Ok(new
            {
                user.Id,
                user.UserName,
                user.FullName,
                user.Email,
                user.PhoneNumber,
                user.CustomerCode,
                user.ProfileImageUrl,
                user.IsBlocked
            });
        }
    }
}