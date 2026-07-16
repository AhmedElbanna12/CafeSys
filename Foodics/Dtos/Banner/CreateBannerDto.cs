using System.ComponentModel.DataAnnotations;

namespace Foodics.Dtos.Banner
{
    public class CreateBannerDto
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        public string? SubDescription { get; set; }

        [Required]
        public IFormFile Photo { get; set; }
    }
}
