namespace Foodics.Models
{
    public class PaymobOptions
    {
        public string ApiKey { get; set; } = string.Empty;

        public string SecretKey { get; set; } = string.Empty;

        public string PublicKey { get; set; } = string.Empty;

        public int IntegrationId { get; set; }

        public string HmacSecret { get; set; } = string.Empty;

        public string BaseUrl { get; set; } = "https://accept.paymob.com";

        public string WebhookUrl { get; set; } = string.Empty;

        public string RedirectionUrl { get; set; } = string.Empty;
    }
}
