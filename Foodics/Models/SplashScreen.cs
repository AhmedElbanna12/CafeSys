using System.ComponentModel.DataAnnotations;

namespace Foodics.Models
{
    public class SplashScreen
    {
        public int Id { get; set; }

        public string Title { get; set; } = null!;

        public string Description { get; set; } = null!;

        public string? Photo { get; set; }
        public string? Video { get; set; }

        public bool IsVisible { get; set; } = true;
    }
}
