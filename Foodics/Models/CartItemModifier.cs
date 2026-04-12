namespace Foodics.Models
{
    public class CartItemModifier
    {
        public int Id { get; set; }

        public int CartItemId { get; set; }
        public CartItem CartItem { get; set; }

        public int ModifierOptionId { get; set; }
        public ModifierOption ModifierOption { get; set; }

        public decimal Price { get; set; }
    }
}
