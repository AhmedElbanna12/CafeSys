namespace Foodics.Dtos.Paymob
{
    public class PaymentKey
    {
        public int integration { get; set; }

        public string key { get; set; } = "";

        public string gateway_type { get; set; } = "";

        public long order_id { get; set; }
    }
}
