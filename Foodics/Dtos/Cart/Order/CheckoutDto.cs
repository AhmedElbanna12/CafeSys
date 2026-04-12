using Foodics.Models;

namespace Foodics.Dtos.Cart.Order
{
    public class CheckoutDto
    {
        public PaymentMethod PaymentMethod { get; set; }

        public OrderType OrderType { get; set; }
        public string? ShippingAddress { get; set; }
        public string? PromoCode { get; set; }
        public int PointsRedeemed { get; internal set; }
    }
}
