using System.ComponentModel.DataAnnotations;

namespace Foodics.Dtos.Admin.Product.ProductSize
{
    public class CreateSizeDto
    {
        [Required]
        public string NameAr { get; set; } = string.Empty;

        [Required]
        public string NameEn { get; set; } = string.Empty;

        [Required]
        public decimal Price { get; set; }

        public bool IsDefault { get; set; }

    }
}
