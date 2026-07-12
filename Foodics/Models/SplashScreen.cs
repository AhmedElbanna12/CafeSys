using System.ComponentModel.DataAnnotations;

namespace Foodics.Models
{
    public class SplashScreen
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = null!;

        [Required]
        public string Description { get; set; } = null!;

        [Required]
        public string Photo { get; set; } = null!;

        public string? Video { get; set; }
    }
}
