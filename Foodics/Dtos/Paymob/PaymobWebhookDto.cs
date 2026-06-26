namespace Foodics.Dtos.Paymob
{
    public class PaymobWebhookDto
    {
        public bool success { get; set; }

        public bool pending { get; set; }

        public bool is_auth { get; set; }

        public bool is_capture { get; set; }

        public bool is_standalone_payment { get; set; }

        public bool is_voided { get; set; }

        public bool is_refunded { get; set; }

        public bool is_3d_secure { get; set; }

        public bool has_parent_transaction { get; set; }

        public bool error_occured { get; set; }

        public long id { get; set; }

        public long integration_id { get; set; }

        public long amount_cents { get; set; }

        public string currency { get; set; } = "";

        public string created_at { get; set; } = "";

        public string owner { get; set; } = "";

        public OrderData order { get; set; } = new();

        public SourceData source_data { get; set; } = new();

        public string hmac { get; set; } = "";
    }
}
