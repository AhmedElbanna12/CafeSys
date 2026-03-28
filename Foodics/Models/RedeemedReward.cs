using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Foodics.Models
{
    public class RedeemedReward
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; }
        public User User { get; set; }

        [ForeignKey("Reward")]
        public int RewardId { get; set; }
        public Reward Reward { get; set; }

        public int PointsUsed { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsUsed { get;  set; }
    }
}
