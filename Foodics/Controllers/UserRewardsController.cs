using Foodics.Dtos.Admin.Rewards;
using Foodics.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POSSystem.Data;
using QRCoder;
using System.Drawing.Imaging;

namespace Foodics.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserRewardsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UserRewardsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/user/rewards/points
        [HttpGet("points")]
        public async Task<IActionResult> GetUserPoints()
        {
            var userId = User.FindFirst("userId")?.Value;

            // نقاط المستخدم من الأوردرات المكتملة
            var totalPoints = await _context.Orders
                .Where(o => o.UserId == userId && o.Status == "Completed")
                .SumAsync(o => o.PointsEarned);

            // نقاط المستخدم المستهلكة
            var usedPoints = await _context.RedeemedRewards
                .Where(r => r.UserId == userId)
                .SumAsync(r => r.PointsUsed);

            var availablePoints = totalPoints - usedPoints;

            return Ok(new
            {
                totalPoints,
                usedPoints,
                availablePoints
            });
        }

        [HttpPost("redeem")]
        [Authorize]
        public async Task<IActionResult> RedeemRewardWithQr(RedeemRewardDto dto)
        {
            var userId = User.FindFirst("userId")?.Value;

            var reward = await _context.Rewards
                .FirstOrDefaultAsync(r => r.Id == dto.RewardId && r.IsActive);

            if (reward == null)
                return NotFound("Reward not found or inactive");

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
    }
}
