using System.ComponentModel.DataAnnotations;

namespace Foodics.Dtos.SplashScreen
{
    public class CreateSplashScreenDto
    {
        [Required]
        public string Title { get; set; } = null!;

        [Required]
        public string Description { get; set; } = null!;

        [Required]
        public IFormFile Photo { get; set; } = null!;
    }
}
