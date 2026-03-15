using System.ComponentModel.DataAnnotations;

namespace Foodics.Dtos.Admin.Rewards
{
    public class CreateRewardDto
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public int PointsRequired { get; set; }

        public int? ProductId { get; set; }
    }
}
