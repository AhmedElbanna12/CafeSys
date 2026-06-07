namespace Foodics.Dtos.Admin.Product.ProductSize
{
    public class UpdateSizeDto
    {
        public int Id { get; set; }


        public string Name { get; set; }
        public string? NameAr { get; set; }

        public string? NameEn { get; set; }

        public decimal? Price { get; set; }

        public bool? IsDefault { get; set; }

    }
}
