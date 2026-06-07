namespace Foodics.Dtos.Admin.Category
{
    public class CategoryAdminDto
    {
        public int Id { get; set; }

        public string NameAr { get; set; } = string.Empty;

        public string NameEn { get; set; } = string.Empty;

        public string? DescriptionAr { get; set; }

        public string? DescriptionEn { get; set; }

        public bool IsActive { get; set; }
    }
}
