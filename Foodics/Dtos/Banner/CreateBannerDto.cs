using System.ComponentModel.DataAnnotations;

namespace Foodics.Dtos.Banner
{
    public class CreateBannerDto
    {
        public string TitleAr { get; set; } = null!;
        public string TitleEn { get; set; } = null!;

        public string DescriptionAr { get; set; } = null!;
        public string DescriptionEn { get; set; } = null!;

        public string SubDescriptionAr { get; set; } = null!;
        public string SubDescriptionEn { get; set; } = null!;

        public IFormFile? Photo { get; set; }
    }
}
