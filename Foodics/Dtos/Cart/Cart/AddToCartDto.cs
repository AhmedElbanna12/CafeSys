namespace Foodics.Dtos.Cart.Cart
{
    public class AddToCartDto
    {
        public int ProductId { get; set; }
        public int? ProductSizeId { get; set; }
        public int Quantity { get; set; }
        public List<int> ModifierOptionIds { get; set; } = new();
    }
}
