namespace Foodics.Dtos.Cart.Cart
{
    public class CartDto
    {
        //public int Id { get; set; }
        //public string UserId { get; set; }
        //public List<CartItemDto> Items { get; set; }
        //public decimal SubTotal { get; set; }
        //public decimal Discount { get; set; }
        //public decimal Total { get; set; }
        //public string PromoCode { get; set; }

        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;

        public List<CartItemDto> Items { get; set; } = new();

        public decimal SubTotal { get; set; }

        public decimal Discount { get; set; }

        public decimal Total { get; set; }

        public string? PromoCode { get; set; }
    }
}
