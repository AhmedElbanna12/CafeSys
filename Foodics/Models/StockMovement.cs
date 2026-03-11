using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Foodics.Models
{
    public class StockMovement
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Ingredient")]
        public int IngredientId { get; set; }
        public Ingredient Ingredient { get; set; }

        public decimal Quantity { get; set; }
        public string Type { get; set; } // Order / Purchase / Waste / Adjustment
        public int? ReferenceId { get; set; } // OrderId or PurchaseId
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
