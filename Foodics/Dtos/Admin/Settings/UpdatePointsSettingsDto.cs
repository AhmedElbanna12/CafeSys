using System.ComponentModel.DataAnnotations;

namespace Foodics.Dtos.Admin.Settings
{
    public class UpdatePointsSettingsDto
    {
        [Range(0.00001, double.MaxValue, ErrorMessage = "Point value must be greater than 0.")]
        public decimal EgpPerPoint { get; set; }
    }
}
