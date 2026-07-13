namespace Foodics.Models
{
    public class UserLocation
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        public string City { get; set; }

        public string Street { get; set; }

        public string BuildingNumber { get; set; }

        public string FloorNumber { get; set; }

        public string ApartmentNumber { get; set; }

        public string Landmark { get; set; }

        public string PhoneNumber { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public bool IsDefault { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;


        public User User { get; set; }
    }
}
