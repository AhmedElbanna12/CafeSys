namespace Foodics.Models
{
    public class HomeSection
    {
        public int Id { get; set; }

        public HomeSectionType SectionName { get; set; }
        public int DisplayOrder { get; set; }

        public bool IsVisible { get; set; }
    }


    public enum HomeSectionType
    {
        UserPoints = 1,
        Banners = 2,
        Advertisements = 3,
        TopSelling = 4
    }
}
