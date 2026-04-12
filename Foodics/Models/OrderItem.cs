using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Foodics.Models
{
    [Index(nameof(OrderId))]
    public class OrderItem
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Order")]
        public int OrderId { get; set; }
        public Order Order { get; set; }

        [ForeignKey("Product")]
        public int ProductId { get; set; }
        public Product Product { get; set; }

        public string ProductName { get; set; } // snapshot

        public int? ProductSizeId { get; set; }
        public ProductSize? ProductSize { get; set; }

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalPrice { get; set; }

        public ICollection<OrderItemModifier> Modifiers { get; set; } = new List<OrderItemModifier>();
    }
}
