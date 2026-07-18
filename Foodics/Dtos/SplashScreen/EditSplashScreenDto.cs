using System.ComponentModel.DataAnnotations;

namespace Foodics.Dtos.SplashScreen
{
    public class EditSplashScreenDto
    {
        public string? TitleAr { get; set; }

        public string? TitleEn { get; set; }

        public string? DescriptionAr { get; set; }

        public string? DescriptionEn { get; set; }

        public IFormFile? Photo { get; set; }

        public IFormFile? Video { get; set; }

        public bool? IsVisible { get; set; }

    }
}
