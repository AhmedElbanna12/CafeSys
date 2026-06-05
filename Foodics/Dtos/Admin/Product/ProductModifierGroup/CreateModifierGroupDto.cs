using System.ComponentModel.DataAnnotations;

namespace Foodics.Dtos.Admin.Product.ProductModifierGroup
{
    public class CreateModifierGroupDto
    {

        [Required]
        public string? NameAr { get; set; }

        [Required]
        public string? NameEn { get; set; }
        public bool IsRequired { get; set; }

        public int MaxSelections { get; set; }
    }
}
