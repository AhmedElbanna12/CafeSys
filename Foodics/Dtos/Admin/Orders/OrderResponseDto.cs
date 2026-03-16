namespace Foodics.Dtos.Admin.Orders
{
    public class OrderResponseDto
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public decimal TotalAmount { get; set; }
        public int PointsEarned { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<OrderItemResponseDto> Items { get; set; }
    }
}
