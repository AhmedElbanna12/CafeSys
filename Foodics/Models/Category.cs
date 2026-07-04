using System.ComponentModel.DataAnnotations;

namespace Foodics.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }

        public string? NameAr { get; set; }
        public string? NameEn { get; set; }

        public string Description { get; set; } = string.Empty;

        public string? DescriptionAr { get; set; }
        public string? DescriptionEn { get; set; }
        public bool IsActive { get; set; } = true;



        // يخفيها من اليوزر
        public bool IsVisible { get; set; } = true;

        // ترتيب ظهورها
        public int DisplayOrder { get; set; } = 0;

        // Soft Delete
        public bool IsDeleted { get; set; } = false;

        public ICollection<Product> Products { get; set; }
    }
}
