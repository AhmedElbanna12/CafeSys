namespace Foodics.Dtos.Cart.Cart
{
    public class CartResponseDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Discount { get; set; }
        public decimal Total { get; set; }
        public string PromoCode { get; set; }
        public List<CartItemDto> Items { get; set; } = new();
    }
}
