namespace Foodics.Dtos.Admin.Product
{
    public class CreateProductDto
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public decimal Price { get; set; }

        public decimal? DiscountPercentage { get; set; }
        public DateTime? DiscountStart { get; set; }
        public DateTime? DiscountEnd { get; set; }
        public int CategoryId { get; set; }

        public int Calories { get; set; }

        public IFormFile Image { get; set; }

        public int PointsReward { get; set; }
    }
}
