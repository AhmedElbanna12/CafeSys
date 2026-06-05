using System.ComponentModel.DataAnnotations;

namespace Foodics.Dtos.Admin.Product.ProductModifierOption
{
    public class CreateModifierOptionDto
    {

        [Required]
        public string NameAr { get; set; } = string.Empty;

        [Required]
        public string NameEn { get; set; } = string.Empty;

        public decimal ExtraPrice { get; set; }
    }
}
