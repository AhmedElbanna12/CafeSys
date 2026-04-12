using Foodics.Models;

namespace Foodics.Dtos.Cart.Order
{
    public class OrderResponseDto
    {
        public int OrderId { get; set; }
        public decimal TotalAmount { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public string? PaymentUrl { get; set; }
    }
}
