namespace Foodics.Dtos.Userproduct
{
    public class ProductListDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public decimal Price { get; set; }

        public decimal DiscountedPrice { get; set; }

        public string? ImageUrl { get; set; }

        public string? CategoryName { get; set; }
    }
}
