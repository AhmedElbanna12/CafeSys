namespace Foodics.Dtos.Cart.Cart
{
    public class CartItemModifierDto
    {
        public int Id { get; set; }
        public int ModifierOptionId { get; set; }
        public string ModifierOptionName { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }
}
