namespace Foodics.Dtos.Cart.Cart
{
    public class CartItemModifierDto
    {
        public int Id { get; set; }
        public int ModifierOptionId { get; set; }
        public string ModifierOptionNameAr { get; set; } = string.Empty;
        public string ModifierOptionNameEn { get; set; } = string.Empty;

        public decimal Price { get; set; }
    }
}
