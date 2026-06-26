namespace Foodics.Dtos.Paymob
{
    public class CreateIntentionRequest
    {
        public int amount { get; set; }

        public string currency { get; set; } = "EGP";

        public List<int> payment_methods { get; set; } = new();

        public List<PaymobItem> items { get; set; } = new();

        public BillingData billing_data { get; set; } = new();

        public Dictionary<string, object>? extras { get; set; }

        public string special_reference { get; set; } = "";

        public int expiration { get; set; } = 3600;

        public string notification_url { get; set; } = "";

        public string redirection_url { get; set; } = "";
    }
}
