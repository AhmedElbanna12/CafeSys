using System.ComponentModel.DataAnnotations;

namespace Foodics.Dtos.Admin.Rewards
{
    public class RedeemRewardDto
    {
        [Required]
        public int RewardId { get; set; }
    }
}
