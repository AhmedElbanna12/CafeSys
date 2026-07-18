using System.ComponentModel.DataAnnotations;

namespace Foodics.Models
{
    public class SplashScreen
    {
        public int Id { get; set; }

        public string TitleAr { get; set; } = null!;
        public string TitleEn { get; set; } = null!;

        public string DescriptionAr { get; set; } = null!;
        public string DescriptionEn { get; set; } = null!;

        public string? Photo { get; set; }
        public string? Video { get; set; }

        public bool IsVisible { get; set; } = true;
    }
}
