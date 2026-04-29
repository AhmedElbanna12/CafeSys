namespace Foodics.Dtos.Admin.Product
{
    public class ProductResponseDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public decimal Price { get; set; }

        public int Calories { get; set; }

        public int PointsReward { get; set; }

        public string ImageUrl { get; set; }

        public bool IsAvailable { get; set; }

        public string CategoryName { get; set; }

        public List<ProductSizeDto> Sizes { get; set; }

        public List<ModifierGroupDto> ModifierGroups { get; set; }

       public decimal ? DiscountedPrice { get; set; }

        public decimal? DiscountPercentage { get; set; }

        public DateTime? DiscountStart { get; set; }

        public DateTime? DiscountEnd { get; set; }
    }
}
