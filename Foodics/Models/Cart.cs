namespace Foodics.Models
{
    public class Cart
    {
        public int Id { get; set; }
        public string UserId { get; set; }

        public List<CartItem> Items { get; set; }

        public decimal SubTotal { get; set; }
        public decimal Discount { get; set; }
        public decimal Total { get; set; }

        public string? PromoCode { get; set; }
    }
}
