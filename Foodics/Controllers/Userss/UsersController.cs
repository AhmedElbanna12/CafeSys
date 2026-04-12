using Foodics.Models;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using AppUser = Foodics.Models.User;

namespace Foodics.Controllers.User
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;

        public UsersController(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }


        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetUserById(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound(new { message = "User not found" });

            var result = new
            {
                user.Id,
                user.UserName,
                user.Email,
                user.PhoneNumber
            };

            return Ok(result);
        }
    }
}