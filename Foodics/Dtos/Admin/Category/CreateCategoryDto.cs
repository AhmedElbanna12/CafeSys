using System.ComponentModel.DataAnnotations;

namespace Foodics.Dtos.Admin.Category
{
    public class CreateCategoryDto
    {
        [Required]
        public string Name { get; set; }

        public string? Description { get; set; }
    }
}
