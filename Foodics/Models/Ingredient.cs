using System.ComponentModel.DataAnnotations;

namespace Foodics.Models
{
    public class Ingredient
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Unit { get; set; }
        public decimal Quantity { get; set; }
        public decimal MinQuantity { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<ProductIngredient> ProductIngredients { get; set; }
        public ICollection<StockMovement> StockMovements { get; set; }
    }
}
