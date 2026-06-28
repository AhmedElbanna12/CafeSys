namespace Foodics.Dtos.Admin.User
{
    public class UserDto
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public IList<string> Roles { get; set; }
        public bool IsBlocked { get; set; }
        public bool IsDeleted {  get; set; }

        public string PhotoImageUrl { get; set; }
    }
}
