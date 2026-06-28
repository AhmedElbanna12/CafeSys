using Foodics.Dtos.Admin.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AppUser = Foodics.Models.User;

namespace Foodics.Controllers.Admin
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminUsersController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;

        public AdminUsersController(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult GetAllUsers()
        {
            var users = _userManager.Users
                .Select(user => new UserDto
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    IsBlocked = user.IsBlocked,
                    IsDeleted = user.IsDeleted
                })
                .ToList();

            return Ok(users);
        }
        // ==========================
        // Get User By Id
        // ==========================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null || user.IsDeleted)
                return NotFound(new { message = "User not found." });

            return Ok(new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber
            });
        }

        // ==========================
        // Block User
        // ==========================
        [HttpPut("block/{id}")]
        public async Task<IActionResult> BlockUser(string id)
        {
            var currentAdminId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (currentAdminId == id)
                return BadRequest(new { message = "You cannot block yourself." });

            var user = await _userManager.FindByIdAsync(id);

            if (user == null || user.IsDeleted)
                return NotFound(new { message = "User not found." });

            if (user.IsBlocked)
                return BadRequest(new { message = "User is already blocked." });

            user.IsBlocked = true;
            user.BlockedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new
            {
                message = "User blocked successfully."
            });
        }

        // ==========================
        // Unblock User
        // ==========================
        [HttpPut("unblock/{id}")]
        public async Task<IActionResult> UnBlockUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null || user.IsDeleted)
                return NotFound(new { message = "User not found." });

            if (!user.IsBlocked)
                return BadRequest(new { message = "User is already active." });

            user.IsBlocked = false;
            user.BlockedAt = null;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new
            {
                message = "User unblocked successfully."
            });
        }

        // ==========================
        // Soft Delete User
        // ==========================
        [HttpDelete("{id}")]
        public async Task<IActionResult> SoftDeleteUser(string id)
        {
            var currentAdminId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (currentAdminId == id)
                return BadRequest(new { message = "You cannot delete yourself." });

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound(new { message = "User not found." });

            if (user.IsDeleted)
                return BadRequest(new { message = "User is already deleted." });

            user.IsDeleted = true;
            user.DeletedAt = DateTime.UtcNow;

            // أي User محذوف يعتبر Blocked تلقائياً
            user.IsBlocked = true;
            user.BlockedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new
            {
                message = "User deleted successfully."
            });
        }

        // ==========================
        // Restore User
        // ==========================
        [HttpPut("restore/{id}")]
        public async Task<IActionResult> RestoreUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound(new { message = "User not found." });

            if (!user.IsDeleted)
                return BadRequest(new { message = "User is already active." });

            user.IsDeleted = false;
            user.DeletedAt = null;

            user.IsBlocked = false;
            user.BlockedAt = null;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new
            {
                message = "User restored successfully."
            });
        }
    }
}