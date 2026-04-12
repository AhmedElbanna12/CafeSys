namespace Foodics.Dtos.Cart.Cart
{
    public class CartItemModifierDto
    {
        public int Id { get; set; }
        public int ModifierOptionId { get; set; }
        public string ModifierOptionName { get; set; }
        public decimal Price { get; set; }
    }
}
