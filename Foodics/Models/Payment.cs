using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Foodics.Models
{
    public class Payment
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Order")]
        public int OrderId { get; set; }
        public Order Order { get; set; }

        public decimal Amount { get; set; }
        public string Method { get; set; } // Cash / Card / Wallet
        public string Status { get; set; } // Success / Failed
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
