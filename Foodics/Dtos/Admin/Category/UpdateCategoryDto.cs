using System.ComponentModel.DataAnnotations;

namespace Foodics.Dtos.Admin.Category
{
    public class UpdateCategoryDto
    {

        [Required]
        public string Name { get; set; }

        public string? Description { get; set; }
    }
}
