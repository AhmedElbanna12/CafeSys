namespace Foodics.Dtos.Paymob
{
    public class CreateIntentionResponse
    {
        public List<PaymentKey> payment_keys { get; set; } = new();

        public long intention_order_id { get; set; }

        public string id { get; set; } = "";

        public string client_secret { get; set; } = "";

        public List<PaymentMethodDto> payment_methods { get; set; } = new();

        public bool confirmed { get; set; }

        public string status { get; set; } = "";
    }
}
