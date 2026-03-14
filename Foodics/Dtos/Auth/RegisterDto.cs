namespace Foodics.Dtos.Auth
{
    public class RegisterDto
    {

        public string FullName { get; set; }
        public  string PhoneNumber { get; set; } = null!;
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
