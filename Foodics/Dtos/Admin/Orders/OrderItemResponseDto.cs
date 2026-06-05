namespace Foodics.Dtos.Admin.Orders
{
    public class OrderItemResponseDto
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        //public List<string> Modifiers { get; set; }

        // بدل string list → structured modifiers
        public List<OrderItemModifierDto> Modifiers { get; set; } = new();
    }
}
