namespace Foodics.Dtos.Admin.Orders
{
    public class OfflineOrderDto
    {

        public int OfflineOrderId { get; set; }

        public string CustomerCode { get; set; }

        public int POSDeviceId { get; set; }

        public List<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();

        public decimal TotalAmount { get; set; }

        public int TotalPointsEarned { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsSynced { get; set; } = false;

        public int RetryCount { get; set; } = 0;
    }
}
