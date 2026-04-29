namespace Foodics.Dtos.Cart.Cart
{
    public class CartItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public int? ProductSizeId { get; set; }
        public string ProductSizeName { get; set; }

        public decimal SizePrice { get; set; }
        public List<CartItemModifierDto> Modifiers { get; set; }
    }
}
