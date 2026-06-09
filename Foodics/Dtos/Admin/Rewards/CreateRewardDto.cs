using System.ComponentModel.DataAnnotations;

namespace Foodics.Dtos.Admin.Rewards
{
    public class CreateRewardDto
    {



        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string NameAr { get; set; } = string.Empty;

        [Required]
        public string NameEn { get; set; } = string.Empty;

        [Required]
        public int PointsRequired { get; set; }

        public int? ProductId { get; set; }
    }
}
