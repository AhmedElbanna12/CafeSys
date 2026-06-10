namespace Foodics.Dtos.Admin.notification
{
    public class SendToUserDto
    {
        public string UserId { get; set; } = string.Empty;

        public string TitleAr { get; set; } = string.Empty;

        public string TitleEn { get; set; } = string.Empty;

        public string BodyAr { get; set; } = string.Empty;

        public string BodyEn { get; set; } = string.Empty;
    }
}
