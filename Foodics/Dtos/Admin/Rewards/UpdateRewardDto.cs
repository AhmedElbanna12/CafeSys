using System.ComponentModel.DataAnnotations;

namespace Foodics.Dtos.Admin.Rewards
{
    public class UpdateRewardDto
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public int PointsRequired { get; set; }

        public int? ProductId { get; set; }

        public bool IsActive { get; set; }
    }
}
