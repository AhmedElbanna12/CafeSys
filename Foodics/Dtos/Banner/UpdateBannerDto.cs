namespace Foodics.Dtos.Banner
{
    public class UpdateBannerDto
    {
        public string? Title { get; set; }

        public string? Description { get; set; }

        public string? SubDescription { get; set; }

        public IFormFile? Photo { get; set; }
    }
}
