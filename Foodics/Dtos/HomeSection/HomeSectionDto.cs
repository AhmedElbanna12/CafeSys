using Foodics.Models;

namespace Foodics.Dtos.HomeSection
{
    public class HomeSectionDto
    {
        public int Id { get; set; }

        public string SectionName { get; set; } = null!;

        public int DisplayOrder { get; set; }

        public bool IsVisible { get; set; }
    }
}
