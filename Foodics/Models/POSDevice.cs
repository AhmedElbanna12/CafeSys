using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Foodics.Models
{
    public class POSDevice
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string DeviceCode { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime LastSync { get; set; }

        [ForeignKey("Branch")]
        public int BranchId { get; set; }
        public Branch Branch { get; set; }

        public ICollection<Order> Orders { get; set; }
    }
}
