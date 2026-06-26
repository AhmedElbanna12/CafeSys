namespace Foodics.Dtos.Paymob
{
    public class PaymentMethodDto
    {
        public int integration_id { get; set; }

        public string name { get; set; } = "";

        public string method_type { get; set; } = "";

        public string currency { get; set; } = "";
    }
}
