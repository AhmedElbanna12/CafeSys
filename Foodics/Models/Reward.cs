using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Foodics.Models
{
    public class Reward
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public int PointsRequired { get; set; }

        [ForeignKey("Product")]
        public int? ProductId { get; set; }
        public Product Product { get; set; }

        public bool IsActive { get; set; } = true;
        public ICollection<RedeemedReward> RedeemedRewards { get; set; }
    }
}
