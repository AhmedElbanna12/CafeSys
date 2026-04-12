using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Foodics.Models
{
    public class OrderItemModifier
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("OrderItem")]
        public int OrderItemId { get; set; }

        public OrderItem OrderItem { get; set; }

        [ForeignKey("ModifierOption")]
        public int ModifierOptionId { get; set; }

        public ModifierOption ModifierOption { get; set; }

        public decimal Price { get; set; }

    }
}
