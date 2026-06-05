using System.ComponentModel.DataAnnotations;

namespace Foodics.Dtos.Admin.Category
{
    public class CreateCategoryDto
    {
        [Required]
        public string NameAr { get; set; }

        [Required]
        public string NameEn { get; set; }

        public string? DescriptionAr { get; set; }
        public string? DescriptionEn { get; set; }
    }
}
