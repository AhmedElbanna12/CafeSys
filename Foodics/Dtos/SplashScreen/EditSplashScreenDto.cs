using System.ComponentModel.DataAnnotations;

namespace Foodics.Dtos.SplashScreen
{
    public class EditSplashScreenDto
    {
        public string? Title{ get; set; }

        public string? Description { get; set; }

        public IFormFile? Photo { get; set; }

        public IFormFile? Video { get; set; }

        public bool? IsVisible { get; set; }

    }
}
