using Foodics.Dtos.Admin.Rewards;
using Foodics.ExtensionMethod;
using Foodics.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POSSystem.Data;
using QRCoder;
using System.Drawing.Imaging;

namespace Foodics.Controllers.Admin
{
    [Route("api/admin/rewards")]
    [ApiController]
    public class AdminRewardsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AdminRewardsController(ApplicationDbContext context)
        {
            _context = context;
        }


        private string GetLang()
        {
            var langHeader = Request.Headers["Accept-Language"].ToString();

            return langHeader
                .Split(',')[0]
                .Trim()
                .ToLower()
                .StartsWith("ar")
                ? "ar"
                : "en";
        }


        //[Authorize(Roles = "Admin")]
        //// Create Reward
        //[HttpPost]
        //public async Task<IActionResult> CreateReward(CreateRewardDto dto)
        //{
        //    var reward = new Reward
        //    {
        //        NameAr = dto.NameAr,
        //        NameEn = dto.NameEn,

        //        PointsRequired = dto.PointsRequired,
        //        ProductId = dto.ProductId,
        //        IsActive = true
        //    };

        //    _context.Rewards.Add(reward);
        //    await _context.SaveChangesAsync();

        //    // 🔹 Load Product navigation property
        //    var rewardWithProduct = await _context.Rewards
        //        .Include(r => r.Product)
        //        .FirstOrDefaultAsync(r => r.Id == reward.Id);

        //    return Ok(new RewardResponseDto
        //    {
        //        Id = reward.Id,
        //        Name = reward.Name,
        //        PointsRequired = reward.PointsRequired,
        //        ProductId = reward.ProductId,
        //        IsActive = reward.IsActive
        //    });
        //}


        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateReward(CreateRewardDto dto)
        {
            var reward = new Reward
            {
                Name   = dto.Name,
                NameAr = dto.NameAr,
                NameEn = dto.NameEn,
                PointsRequired = dto.PointsRequired,
                ProductId = dto.ProductId,
                IsActive = true
            };

            _context.Rewards.Add(reward);
            await _context.SaveChangesAsync();

            var lang = GetLang();
            return Ok(new RewardResponseDto
            {
                Id = reward.Id,
                NameAr = reward.NameAr,
                NameEn = reward.NameEn,
                PointsRequired = reward.PointsRequired,
                ProductId = reward.ProductId,
                IsActive = reward.IsActive
            });
        }


        //// Get All Rewards
        //[HttpGet]
        //public async Task<IActionResult> GetRewards()
        //{
        //    var rewards = await _context.Rewards.ToListAsync();

        //    var result = rewards.Select(r => new RewardResponseDto
        //    {
        //        Id = r.Id,
        //        Name = r.Name,
        //        PointsRequired = r.PointsRequired,
        //        ProductId = r.ProductId,   // بس ID
        //        IsActive = r.IsActive
        //    });

        //    return Ok(result);
        //}


        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetRewards()
        {
            var rewards = await _context.Rewards.ToListAsync();

            var result = rewards.Select(r => new RewardResponseDto
            {
                Id = r.Id,
                NameAr = r.NameAr,
                NameEn = r.NameEn,
                PointsRequired = r.PointsRequired,
                ProductId = r.ProductId,
                IsActive = r.IsActive
            });

            return Ok(result);
        }

        //// Get Reward By Id
        //[HttpGet("{id}")]
        //public async Task<IActionResult> GetReward(int id)
        //{
        //    var reward = await _context.Rewards.FindAsync(id);

        //    if (reward == null)
        //        return NotFound("Reward not found");

        //    var result = new RewardResponseDto
        //    {
        //        Id = reward.Id,
        //        Name = reward.Name,
        //        PointsRequired = reward.PointsRequired,
        //        ProductId = reward.ProductId,
        //        IsActive = reward.IsActive
        //    };

        //    return Ok(result);
        //}


        [HttpGet("{id}")]
        public async Task<IActionResult> GetReward(int id)
        {
            var reward = await _context.Rewards.FindAsync(id);

            if (reward == null)
                return NotFound("Reward not found");

            return Ok(new RewardResponseDto
            {
                Id = reward.Id,
                NameAr = reward.NameAr,
                NameEn = reward.NameEn,
                PointsRequired = reward.PointsRequired,
                ProductId = reward.ProductId,
                IsActive = reward.IsActive
            });
        }

        //[Authorize(Roles = "Admin")]
        //// Update Reward
        //[HttpPut("{id}")]
        //public async Task<IActionResult> UpdateReward(int id, UpdateRewardDto dto)
        //{
        //    var reward = await _context.Rewards.FindAsync(id);

        //    if (reward == null)
        //        return NotFound("Reward not found");

        //    if (dto.NameAr != null)
        //        reward.NameAr = dto.NameAr;

        //    if (dto.NameEn != null)
        //        reward.NameEn = dto.NameEn;

        //    if (dto.PointsRequired.HasValue)
        //        reward.PointsRequired = dto.PointsRequired.Value;

        //    if (dto.ProductId.HasValue)
        //        reward.ProductId = dto.ProductId.Value;

        //    if (dto.IsActive.HasValue)
        //        reward.IsActive = dto.IsActive.Value;

        //    await _context.SaveChangesAsync();

        //    var result = new RewardResponseDto
        //    {
        //        Id = reward.Id,
        //        Name = reward.Name,
        //        PointsRequired = reward.PointsRequired,
        //        ProductId = reward.ProductId,
        //        IsActive = reward.IsActive
        //    };

        //    return Ok(result);
        //}


        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReward(int id, UpdateRewardDto dto)
        {
            var reward = await _context.Rewards.FindAsync(id);

            if (reward == null)
                return NotFound("Reward not found");

            if (dto.NameAr != null)
                reward.NameAr = dto.NameAr;

            if (dto.NameEn != null)
                reward.NameEn = dto.NameEn;

            if (dto.PointsRequired.HasValue)
                reward.PointsRequired = dto.PointsRequired.Value;

            if (dto.ProductId.HasValue)
                reward.ProductId = dto.ProductId.Value;

            if (dto.IsActive.HasValue)
                reward.IsActive = dto.IsActive.Value;

            await _context.SaveChangesAsync();

            var lang = GetLang();

            return Ok(new RewardResponseDto
            {
                Id = reward.Id,
                NameAr = reward.NameAr,
                NameEn = reward.NameEn,
                PointsRequired = reward.PointsRequired,
                ProductId = reward.ProductId,
                IsActive = reward.IsActive
            });
        }

        [HttpGet("/api/rewards")]
        public async Task<IActionResult> GetUserRewards()
        {
            var lang = GetLang();

            var rewards = await _context.Rewards
                .Where(r => r.IsActive)
                .ToListAsync();

            var result = rewards.Select(r => new UserRewardDto
            {
                Id = r.Id,

                Name = LocalizationExtensions.Localize(
                    r.NameAr,
                    r.NameEn,
                    lang),

                PointsRequired = r.PointsRequired,
                ProductId = r.ProductId,
                IsActive = r.IsActive
            });

            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
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


        [Authorize(Roles = "Admin")]
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
        public async Task<IActionResult> RedeemReward(RedeemRewardDto dto)
        {
            var userId = User.FindFirst("userId")?.Value;

            var reward = await _context.Rewards
                .FirstOrDefaultAsync(r => r.Id == dto.RewardId && r.IsActive);

            if (reward == null)
                return NotFound("Reward not found or inactive");

            // حساب نقاط المستخدم
            var totalPoints = await _context.Orders
                .Where(o => o.UserId == userId && o.OrderStatus == OrderStatus.Completed)
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


        [Authorize(Roles = "Admin")]
        [HttpPost("redeemCashier")]

        public async Task<IActionResult> RedeemRewardWithQr(RedeemRewardDto dto)
        {
            var userId = User.FindFirst("userId")?.Value;

            var reward = await _context.Rewards
                .FirstOrDefaultAsync(r => r.Id == dto.RewardId && r.IsActive);

            if (reward == null)
                return NotFound("Reward not found or inactive");

            var totalPoints = await _context.Orders
                .Where(o => o.UserId == userId && o.OrderStatus == OrderStatus.Completed)
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
                PointsUsed = reward.PointsRequired,
                IsUsed = false // Flag للكاشير بعد ما يمسح QR
            };

            _context.RedeemedRewards.Add(redeemed);
            await _context.SaveChangesAsync();

            // توليد QR Code
            var qrData = $"RedeemedReward:{redeemed.Id}";
            using var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(qrData, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new QRCode(qrCodeData);
            using var qrBitmap = qrCode.GetGraphic(20);

            using var ms = new MemoryStream();
            qrBitmap.Save(ms, ImageFormat.Png);
            var qrBase64 = Convert.ToBase64String(ms.ToArray());

            return Ok(new
            {
                message = "Reward redeemed successfully",
                pointsUsed = reward.PointsRequired,
                remainingPoints = availablePoints - reward.PointsRequired,
                qrCodeBase64 = $"data:image/png;base64,{qrBase64}"
            });
        }


        // GET: api/admin/rewards/redeemed-rewards
        [HttpGet("redeemed-rewards")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllRedeemedRewards()
        {
            var lang = GetLang();

            var redeemedRewards = await _context.RedeemedRewards
                .Include(r => r.User)
                .Include(r => r.Reward)
                .OrderByDescending(r => r.Id)
                .Select(r => new
                {
                    id = r.Id,

                    user = new
                    {
                        id = r.UserId,
                        name = r.User.FullName,
                        phone = r.User.PhoneNumber,
                        email = r.User.Email
                    },

                    reward = new
                    {
                        id = r.RewardId,

                        name = LocalizationExtensions.Localize(
                            r.Reward.NameAr,
                            r.Reward.NameEn,
                            lang),

                        pointsRequired = r.Reward.PointsRequired
                    },

                    pointsUsed = r.PointsUsed,

                    status = new
                    {
                        isUsed = r.IsUsed,
                        usedAt = r.UsedAt
                    },

                    createdAt = r.CreatedAt
                })
                .ToListAsync();

            return Ok(redeemedRewards);
        }
    }
}