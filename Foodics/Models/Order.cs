using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Foodics.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; }
        public User User { get; set; }

        [ForeignKey("Branch")]
        public int BranchId { get; set; }
        public Branch Branch { get; set; }

        [ForeignKey("POSDevice")]
        public int POSDeviceId { get; set; }
        public POSDevice POSDevice { get; set; }

        public decimal TotalAmount { get; set; }
        public string Status { get; set; } // Pending / Completed / Cancelled
        public string PaymentStatus { get; set; } // Paid / Unpaid
        public int PointsEarned { get; set; }
        public int PointsRedeemed { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<OrderItem> OrderItems { get; set; }
        public Payment Payment { get; set; }
    }
}
