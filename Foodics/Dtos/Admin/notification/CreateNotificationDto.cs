using System.ComponentModel.DataAnnotations;

namespace Foodics.Dtos.Admin.notification
{
    public class CreateNotificationDto
    {
        [Required]
        public string TitleAr { get; set; } = string.Empty;

        [Required]
        public string TitleEn { get; set; } = string.Empty;

        [Required]
        public string BodyAr { get; set; } = string.Empty;

        [Required]
        public string BodyEn { get; set; } = string.Empty;
    }
}
