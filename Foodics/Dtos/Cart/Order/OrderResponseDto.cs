using Foodics.Dtos.Admin.Orders;
using Foodics.Models;

namespace Foodics.Dtos.Cart.Order
{
    public class OrderResponseDto
    {

        public int OrderId { get; set; }

        public decimal TotalAmount { get; set; }

        public decimal DiscountAmount { get; set; }

        public string? PromoCode { get; set; }

        public decimal? PromoDiscountPercentage { get; set; }

        public PaymentStatus PaymentStatus { get; set; }

        public string? PaymentUrl { get; set; }

        public int PointsEarned { get; set; }

        public DateTime CreatedAt { get; set; }

        public string? CustomerName { get; set; }

        public string? CustomerPhone { get; set; }

        public List<OrderItemResponseDto> Items { get; set; } = new();

    }
}
