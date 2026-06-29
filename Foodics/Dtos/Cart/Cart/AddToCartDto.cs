using System.ComponentModel.DataAnnotations;

namespace Foodics.Dtos.Cart.Cart
{
    public class AddToCartDto
    {

        [Required]
        public int ProductId { get; set; }
        public int? ProductSizeId { get; set; }
        public int Quantity { get; set; }
        //  public List<int> ModifierOptionIds { get; set; } = new();

        public List<CartModifierDto> Modifiers { get; set; } = new();

        public string? Comment { get; set; }
    }
}
