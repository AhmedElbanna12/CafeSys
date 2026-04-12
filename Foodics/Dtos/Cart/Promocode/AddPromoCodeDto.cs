namespace Foodics.Dtos.Cart.Promocode
{
    public class AddPromoCodeDto
    {
        public string Code { get; set; }
        public decimal DiscountAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
