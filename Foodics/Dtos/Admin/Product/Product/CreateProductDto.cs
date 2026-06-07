using System.ComponentModel.DataAnnotations;

namespace Foodics.Dtos.Admin.Product.Product
{
    public class CreateProductDto
    {


        public String? Name { get; set; }
        public string? Description{ get; set; }


        [Required]

        public string? NameAr { get; set; }

        [Required]

        public string? NameEn { get; set; }

        public string? DescriptionAr { get; set; }
        public string? DescriptionEn { get; set; }
        [Required]

        public decimal Price { get; set; }

        public decimal? DiscountPercentage { get; set; }
        public DateTime? DiscountStart { get; set; }
        public DateTime? DiscountEnd { get; set; }

        [Required]
        public int CategoryId { get; set; }


        [Required]
        public int Calories { get; set; }


        [Required]
        public IFormFile Image { get; set; }

        public int PointsReward { get; set; }
    }
}
