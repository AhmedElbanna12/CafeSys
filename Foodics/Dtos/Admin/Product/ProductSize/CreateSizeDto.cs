namespace Foodics.Dtos.Admin.Product.ProductSize
{
    public class CreateSizeDto
    {
        public string? NameAr { get; set; }
        public string? NameEn { get; set; }
        public decimal Price { get; set; }
        public bool IsDefault { get; set; }

    }
}
