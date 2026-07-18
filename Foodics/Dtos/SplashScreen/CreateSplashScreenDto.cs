using System.ComponentModel.DataAnnotations;

namespace Foodics.Dtos.SplashScreen
{
    public class CreateSplashScreenDto
    {
        [Required]
        public string TitleAr { get; set; } = null!;

        [Required]
        public string TitleEn { get; set; } = null!;

        [Required]
        public string DescriptionAr { get; set; } = null!;

        [Required]
        public string DescriptionEn { get; set; } = null!;

        [Required]
        public IFormFile Photo { get; set; } = null!;

        public IFormFile? Video { get; set; }

        public bool IsVisible { get; set; } = true;
    }
}
