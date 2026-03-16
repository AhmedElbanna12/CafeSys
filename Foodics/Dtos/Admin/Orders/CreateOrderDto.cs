namespace Foodics.Dtos.Admin.Orders
{
    public class CreateOrderDto
    {
        public string CustomerCode { get; set; } // من QR
        public int POSDeviceId { get; set; }     // الجهاز اللي عامل الأوردر
        public List<OrderItemDto> Items { get; set; }
    }
}
