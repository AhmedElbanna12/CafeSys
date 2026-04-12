using Foodics.Dtos.Admin.Rewards;
using Foodics.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POSSystem.Data;
using QRCoder;
using System.Drawing.Imaging;

namespace Foodics.Controllers.User
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserRewardsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UserRewardsController(ApplicationDbContext context)
        {
            _context = context;
        }

        private async Task<int> GetAvailablePoints(string userId)
        {
            var transactions = await _context.PointsTransactions
                .Where(t => t.UserId == userId)
                .ToListAsync();

            var earned = transactions.Where(t => t.Type == "Earn").Sum(t => t.Points);
            var redeemed = transactions.Where(t => t.Type == "Redeem").Sum(t => t.Points);
            var expired = transactions.Where(t => t.Type == "Expire").Sum(t => t.Points);

            return earned - redeemed - expired;
        }

        // GET: api/userrewards/points
        [HttpGet("points")]
        [Authorize]
        public async Task<IActionResult> GetUserPoints()
        {
            var userId = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not identified");

            var transactions = await _context.PointsTransactions
                .Where(t => t.UserId == userId)
                .ToListAsync();

            var totalEarned = transactions.Where(t => t.Type == "Earn").Sum(t => t.Points);
            var totalRedeemed = transactions.Where(t => t.Type == "Redeem").Sum(t => t.Points);
            var totalExpired = transactions.Where(t => t.Type == "Expire").Sum(t => t.Points);

            var availablePoints = totalEarned - totalRedeemed - totalExpired;

            return Ok(new
            {
                totalEarned,
                totalRedeemed,
                totalExpired,
                availablePoints
            });
        }

        // POST: api/userrewards/redeem
        [HttpPost("redeem")]
        [Authorize]
        public async Task<IActionResult> RedeemRewardWithQr(RedeemRewardDto dto)
        {
            var userId = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not identified");

            var reward = await _context.Rewards
                .FirstOrDefaultAsync(r => r.Id == dto.RewardId && r.IsActive);

            if (reward == null)
                return NotFound("Reward not found or inactive");

            var availablePoints = await GetAvailablePoints(userId);

            if (availablePoints < reward.PointsRequired)
                return BadRequest("Not enough points");

            // إنشاء RedeemedReward
            var redeemed = new RedeemedReward
            {
                UserId = userId,
                RewardId = reward.Id,
                PointsUsed = reward.PointsRequired,
                IsUsed = false,
                UsedAt = null, // ✅ تصحيح مهم
                // يفضل تضيف CreatedAt في الموديل
                // CreatedAt = DateTime.UtcNow
            };

            _context.RedeemedRewards.Add(redeemed);

            // تسجيل Transaction نوع Redeem
            var transaction = new PointsTransaction
            {
                UserId = userId,
                Points = reward.PointsRequired,
                Type = "Redeem",
                CreatedAt = DateTime.UtcNow
            };

            _context.PointsTransactions.Add(transaction);

            await _context.SaveChangesAsync();

            // توليد QR Code
            var qrData = $"RedeemedReward:{redeemed.Id}:User:{userId}:Reward:{reward.Id}";

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

                redeemedReward = new
                {
                    id = redeemed.Id,
                    isUsed = redeemed.IsUsed,
                    createdAt = DateTime.UtcNow // أو CreatedAt لو ضفته
                },

                reward = new
                {
                    id = reward.Id,
                    name = reward.Name,
                    pointsRequired = reward.PointsRequired
                },

                points = new
                {
                    before = availablePoints,
                    used = reward.PointsRequired,
                    after = availablePoints - reward.PointsRequired
                },

                qr = new
                {
                    rawData = qrData, // 🔥 مهم جدًا للـ testing و scanning
                    base64 = $"data:image/png;base64,{qrBase64}"
                }
            });
        }

        // POST: api/userrewards/validate-qr
        [HttpPost("validate-qr")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ValidateQr([FromBody] string qrData)
        {
            var parts = qrData.Split(':');

            if (parts.Length < 4 || parts[0] != "RedeemedReward")
                return BadRequest("Invalid QR Code");

            if (!int.TryParse(parts[1], out int redeemedId))
                return BadRequest("Invalid QR Code format");

            var redeemed = await _context.RedeemedRewards
                .Include(r => r.Reward)
                .FirstOrDefaultAsync(r => r.Id == redeemedId);

            if (redeemed == null)
                return NotFound("Redeemed reward not found");

            if (redeemed.IsUsed)
            {
                return BadRequest(new
                {
                    message = "QR Code already used",
                    redeemedRewardId = redeemed.Id,
                    usedAt = redeemed.UsedAt
                });
            }

            redeemed.IsUsed = true;
            redeemed.UsedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "QR validated successfully",

                redeemedRewardId = redeemed.Id,

                user = new
                {
                    id = redeemed.UserId
                },

                reward = new
                {
                    id = redeemed.Reward.Id,
                    name = redeemed.Reward.Name,
                    pointsUsed = redeemed.PointsUsed
                },

                status = new
                {
                    isUsed = redeemed.IsUsed,
                    usedAt = redeemed.UsedAt
                }
            });
        }
    }
}