namespace Foodics.Dtos.Adv
{
    public class UpdateAdDto
    {
        public string? TitleAr { get; set; }

        public string? TitleEn { get; set; }

        public string? DescriptionAr { get; set; }

        public string? DescriptionEn { get; set; }

        public IFormFile? Image { get; set; }
    }
}
