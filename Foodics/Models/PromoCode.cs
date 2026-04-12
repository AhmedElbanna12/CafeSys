namespace Foodics.Models
{
    public class PromoCode
    {
        public int Id { get; set; }
        public string Code { get; set; }           // الكود اللي المستخدم هيكتبه
        public decimal DiscountAmount { get; set; } // قيمة الخصم
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; } = true;

    }
}
