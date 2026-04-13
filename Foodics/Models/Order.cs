using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Foodics.Models
{
    [Index(nameof(UserId))]
    public class Order
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; }
        public User User { get; set; }

        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public int PointsEarned { get; set; }
        public int PointsRedeemed { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        public int? PaymentId { get; set; }


        [InverseProperty("Order")]
        public Payment? Payment { get; set; }
        public OrderStatus OrderStatus { get; set; } = OrderStatus.Pending;

        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;

        public PaymentMethod PaymentMethod { get; set; }
        public string? TransactionId { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? ShippingAddress { get; set; }
        public OrderType OrderType { get; internal set; }

        public decimal DeliveryFee { get; set; } 

    }

    public enum PaymentMethod
    {
        CashOnDelivery = 1,
        Online = 2 , 
        Onsite =3
    }

    public enum OrderStatus
    {
        Pending,
        Completed,
        Cancelled
    }

    public enum OrderType
    {
        Pickup = 1,      // عند الكاشير
        Delivery = 2     // دليفري
    }

    public enum PaymentStatus
    {
        Unpaid,
        Paid,
        Failed,
        Pending
    }
}
