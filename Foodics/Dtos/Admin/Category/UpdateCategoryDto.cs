using System.ComponentModel.DataAnnotations;

namespace Foodics.Dtos.Admin.Category
{
    public class UpdateCategoryDto 
    {

        public string? NameAr { get; set; }

        public string? NameEn { get; set; }

        public string? DescriptionAr { get; set; }

        public string? DescriptionEn { get; set; }

        public bool? IsActive { get; set; } = true;

        // يخفيها من اليوزر
        public bool? IsVisible { get; set; } = true;

        // ترتيب ظهورها
        public int? DisplayOrder { get; set; } = 0;

        // Soft Delete
        public bool? IsDeleted { get; set; } = false;
    }
}
