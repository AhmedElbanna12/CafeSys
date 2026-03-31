namespace Foodics.Dtos.Admin.Product
{
    public class TopSellingProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; }
        public int TotalSold { get; set; }
    }
}
