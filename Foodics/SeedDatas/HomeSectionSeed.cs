using Foodics.Models;
using Microsoft.EntityFrameworkCore;

namespace Foodics.SeedDatas
{
    public static class HomeSectionSeed
    {
        public static void Seed(ModelBuilder builder)
        {
            builder.Entity<HomeSection>().HasData(
                new HomeSection
                {
                    Id = 1,
                    SectionName = HomeSectionType.UserPoints,
                    DisplayOrder = 1,
                    IsVisible = true
                },
                new HomeSection
                {
                    Id = 2,
                    SectionName = HomeSectionType.Banners,
                    DisplayOrder = 2,
                    IsVisible = true
                },
                new HomeSection
                {
                    Id = 3,
                    SectionName = HomeSectionType.Advertisements,
                    DisplayOrder = 3,
                    IsVisible = true
                },
                new HomeSection
                {
                    Id = 4,
                    SectionName = HomeSectionType.TopSelling,
                    DisplayOrder = 4,
                    IsVisible = true
                }
            );
        }
    }
}
