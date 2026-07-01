namespace Foodics.Dtos.Paymob
{
    public class PaymobWebhookRequestDto
    {

        public string type { get; set; } = "";

        public PaymobWebhookDto obj { get; set; }

        public string? hmac { get; set; }
    }
}
