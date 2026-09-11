namespace Foodics.Dtos.Admin.Product.ProductModifierOption
{
    public class UpdateModifierOptionDto
    {
        public string? NameAr { get; set; }

        public string? NameEn { get; set; }

        public decimal? ExtraPrice { get; set; }
        public bool? IsCountable { get; set; }
    }
}
