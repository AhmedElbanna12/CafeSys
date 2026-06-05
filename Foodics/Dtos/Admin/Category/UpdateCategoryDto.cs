using System.ComponentModel.DataAnnotations;

namespace Foodics.Dtos.Admin.Category
{
    public class UpdateCategoryDto 
    {

        public string? NameAr { get; set; }

        public string? NameEn { get; set; }

        public string? DescriptionAr { get; set; }

        public string? DescriptionEn { get; set; }
    }
}
