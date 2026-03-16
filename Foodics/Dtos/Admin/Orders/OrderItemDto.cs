namespace Foodics.Dtos.Admin.Orders
{
    public class OrderItemDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public int? ProductSizeId { get; set; } // لو فيه أحجام
        public List<int> ModifierOptionIds { get; set; } = new List<int>();
    }
}
