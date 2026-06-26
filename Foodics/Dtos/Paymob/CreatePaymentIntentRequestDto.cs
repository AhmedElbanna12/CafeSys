namespace Foodics.Dtos.Paymob
{
    public class CreatePaymentIntentRequestDto
    {
        public int OrderId { get; set; }

        public decimal Amount { get; set; }

        public string Currency { get; set; } = "EGP";

        public string CustomerName { get; set; } = "";

        public string Email { get; set; } = "";

        public string PhoneNumber { get; set; } = "";
    }
}
