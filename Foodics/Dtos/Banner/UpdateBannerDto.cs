namespace Foodics.Dtos.Banner
{
    public class UpdateBannerDto
    {
        public string? TitleAr { get; set; }
        public string? TitleEn { get; set; }

        public string? DescriptionAr { get; set; }
        public string? DescriptionEn { get; set; }

        public string? SubDescriptionAr { get; set; }
        public string? SubDescriptionEn { get; set; }

        public IFormFile? Photo { get; set; }
    }
}
