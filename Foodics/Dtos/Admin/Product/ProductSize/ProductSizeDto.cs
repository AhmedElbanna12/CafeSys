namespace Foodics.Dtos.Admin.Product.ProductSize
{
    public class ProductSizeDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }

        public bool IsDefault { get; set; }
    }
}
