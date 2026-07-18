namespace Foodics.Models
{
    public class Banner
    {

        public int Id { get; set; }

        public string TitleAr { get; set; } = null!;
        public string TitleEn { get; set; } = null!;

        public string DescriptionAr { get; set; } = null!;
        public string DescriptionEn { get; set; } = null!;

        public string SubDescriptionAr { get; set; } = null!;
        public string SubDescriptionEn { get; set; } = null!;

        public string? Photo { get; set; }
    }
}
