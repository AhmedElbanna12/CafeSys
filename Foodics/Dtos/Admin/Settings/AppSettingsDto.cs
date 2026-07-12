namespace Foodics.Dtos.Admin.Settings
{
    public class AppSettingsDto
    {
        public bool IsDeliveryEnabled { get; set; }

        public bool IsPickupEnabled { get; set; }

        public decimal DeliveryFee { get; set; }
    }
}
