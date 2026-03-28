using Foodics.Dtos.Admin.Rewards;
using Foodics.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POSSystem.Data;

namespace Foodics.Controllers.Admin
{
    [Route("api/admin/rewards")]
    [ApiController]
   // [Authorize(Roles = "Admin")]
    public class AdminRewardsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AdminRewardsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Create Reward
        [HttpPost]
        public async Task<IActionResult> CreateReward(CreateRewardDto dto)
        {
            var reward = new Reward
            {
                Name = dto.Name,
                PointsRequired = dto.PointsRequired,
                ProductId = dto.ProductId,
                IsActive = true
            };

            _context.Rewards.Add(reward);
            await _context.SaveChangesAsync();

            // 🔹 Load Product navigation property
            var rewardWithProduct = await _context.Rewards
                .Include(r => r.Product)
                .FirstOrDefaultAsync(r => r.Id == reward.Id);

            return Ok(new RewardResponseDto
            {
                Id = reward.Id,
                Name = reward.Name,
                PointsRequired = reward.PointsRequired,
                ProductId = reward.ProductId,
                IsActive = reward.IsActive
            });
        }

        // Get All Rewards
        [HttpGet]
        public async Task<IActionResult> GetRewards()
        {
            var rewards = await _context.Rewards.ToListAsync();

            var result = rewards.Select(r => new RewardResponseDto
            {
                Id = r.Id,
                Name = r.Name,
                PointsRequired = r.PointsRequired,
                ProductId = r.ProductId,   // بس ID
                IsActive = r.IsActive
            });

            return Ok(result);
        }

        // Get Reward By Id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetReward(int id)
        {
            var reward = await _context.Rewards.FindAsync(id);

            if (reward == null)
                return NotFound("Reward not found");

            var result = new RewardResponseDto
            {
                Id = reward.Id,
                Name = reward.Name,
                PointsRequired = reward.PointsRequired,
                ProductId = reward.ProductId,
                IsActive = reward.IsActive
            };

            return Ok(result);
        }

        // Update Reward
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReward(int id, UpdateRewardDto dto)
        {
            var reward = await _context.Rewards.FindAsync(id);

            if (reward == null)
                return NotFound("Reward not found");

            reward.Name = dto.Name;
            reward.PointsRequired = dto.PointsRequired;
            reward.ProductId = dto.ProductId;
            reward.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();

            var result = new RewardResponseDto
            {
                Id = reward.Id,
                Name = reward.Name,
                PointsRequired = reward.PointsRequired,
                ProductId = reward.ProductId,
                IsActive = reward.IsActive
            };

            return Ok(result);
        }

        // Toggle Reward
        [HttpPatch("{id}/toggle")]
        public async Task<IActionResult> ToggleReward(int id)
        {
            var reward = await _context.Rewards.FindAsync(id);

            if (reward == null)
                return NotFound("Reward not found");

            reward.IsActive = !reward.IsActive;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Reward status updated",
                reward.IsActive
            });
        }

        // Delete Reward
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReward(int id)
        {
            var reward = await _context.Rewards.FindAsync(id);

            if (reward == null)
                return NotFound("Reward not found");

            _context.Rewards.Remove(reward);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Reward deleted successfully" });
     
       }


        //Wait For User Dashboard 

        [HttpPost("redeem")]
        [Authorize]
        public async Task<IActionResult> RedeemReward(RedeemRewardDto dto)
        {
            var userId = User.FindFirst("userId")?.Value;

            var reward = await _context.Rewards
                .FirstOrDefaultAsync(r => r.Id == dto.RewardId && r.IsActive);

            if (reward == null)
                return NotFound("Reward not found or inactive");

            // حساب نقاط المستخدم
            var totalPoints = await _context.Orders
                .Where(o => o.UserId == userId && o.Status == "Completed")
                .SumAsync(o => o.PointsEarned);

            var usedPoints = await _context.RedeemedRewards
                .Where(r => r.UserId == userId)
                .SumAsync(r => r.PointsUsed);

            var availablePoints = totalPoints - usedPoints;

            if (availablePoints < reward.PointsRequired)
                return BadRequest("Not enough points");

            // إنشاء RedeemedReward
            var redeemed = new RedeemedReward
            {
                UserId = userId,
                RewardId = reward.Id,
                PointsUsed = reward.PointsRequired
            };

            _context.RedeemedRewards.Add(redeemed);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Reward redeemed successfully",
                pointsUsed = reward.PointsRequired,
                remainingPoints = availablePoints - reward.PointsRequired
            });
        }
    }
}