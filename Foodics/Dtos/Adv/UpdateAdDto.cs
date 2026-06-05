namespace Foodics.Dtos.Adv
{
    public class UpdateAdDto
    {
        public string ? TitleAr { get; set; } = string.Empty;
        public string ?TitleEn { get; set; } = string.Empty;

        public string ? DescriptionAr { get; set; } = string.Empty;
        public string ?DescriptionEn { get; set; } = string.Empty;
        public IFormFile? Image { get; set; } // optional
    }
}
