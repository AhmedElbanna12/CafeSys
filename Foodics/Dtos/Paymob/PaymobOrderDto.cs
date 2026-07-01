using System.Text.Json.Serialization;

namespace Foodics.Dtos.Paymob
{
    public class PaymobOrderDto
    {
        public long id { get; set; }
        public string? merchant_order_id { get; set; }

        // ✅ Add these to capture any variant
        [JsonPropertyName("order_id")]
        public string? order_id { get; set; }

        [JsonPropertyName("reference")]
        public string? reference { get; set; }
    }
}
