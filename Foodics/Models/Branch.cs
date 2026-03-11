using System.ComponentModel.DataAnnotations;

namespace Foodics.Models
{
    public class Branch
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<POSDevice> POSDevices { get; set; }
        public ICollection<Order> Orders { get; set; }
    }
}
