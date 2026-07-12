using System.ComponentModel.DataAnnotations;

namespace Foodics.Dtos.SplashScreen
{
    public class EditSplashScreenDto
    {
        [Required]
        public string Title { get; set; } = null!;

        [Required]
        public string Description { get; set; } = null!;

        public IFormFile? Photo { get; set; }
    }
}
