using Foodics.Models;

namespace Foodics.Dtos.Cart.Order
{
    public class UpdateOrderStatusDto
    {
        public int OrderId { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public bool PaymentStatusUpdate { get; set; } = false; // Admin يحط النقاط لو الدفع عند الاستلام
    }
}
