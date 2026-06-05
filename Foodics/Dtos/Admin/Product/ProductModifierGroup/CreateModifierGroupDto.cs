namespace Foodics.Dtos.Admin.Product.ProductModifierGroup
{
    public class CreateModifierGroupDto
    {
        public string? NameAr { get; set; }
        public string? NameEn { get; set; }
        public bool IsRequired { get; set; }

        public int MaxSelections { get; set; }
    }
}
