using System.ComponentModel.DataAnnotations;

namespace Foodics.Dtos.Adv
{
    public class CreateAdDto
    {

        [Required]
        public string TitleAr { get; set; } = string.Empty;
        [Required]
        public string TitleEn { get; set; } = string.Empty;

        public string DescriptionAr { get; set; } = string.Empty;
        public string DescriptionEn { get; set; } = string.Empty;

        [Required]
        public IFormFile Image { get; set; } = default!;

    }
}
