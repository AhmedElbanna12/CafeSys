using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Foodics.Models
{
    public class Product
    {

        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public decimal Price { get; set; }

        public int PointsReward { get; set; }

        public int Calories { get; set; }

        public string ImageUrl { get; set; }

        public bool IsAvailable { get; set; } = true;

        [ForeignKey("Category")]
        public int CategoryId { get; set; }
        public Category Category { get; set; }

        public ICollection<ProductSize> Sizes { get; set; }

        public ICollection<ModifierGroup> ModifierGroups { get; set; }

        public ICollection<ProductIngredient> ProductIngredients { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; }
    }
}
