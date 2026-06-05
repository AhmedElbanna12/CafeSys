using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Foodics.Models
{
    public class ModifierOption
    {
        [Key]
        public int Id { get; set; }

        public string? NameAr { get; set; }
        public string? NameEn { get; set; }
        public decimal ExtraPrice { get; set; }

        [ForeignKey("ModifierGroup")]
        public int ModifierGroupId { get; set; }

        public ModifierGroup ModifierGroup { get; set; }
    }
}
