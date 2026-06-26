namespace Foodics.Dtos.Paymob
{
    public class CreatePaymentIntentResponseDto
    {
        public bool Success { get; set; }

        public string? Message { get; set; }

        public string? ClientSecret { get; set; }

        public string? CheckoutUrl { get; set; }

        public string? PaymobOrderId { get; set; }
    }
}
