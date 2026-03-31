namespace Foodics.Dtos.Admin.notification
{
    public class SendToUserDto
    {
        public string UserId { get; set; }  
        public string Title { get; set; }
        public string Body { get; set; }
    }
}
