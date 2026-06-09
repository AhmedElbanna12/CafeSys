namespace Foodics.Dtos.Admin.Rewards
{
    public class UserRewardDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public int PointsRequired { get; set; }

        public int? ProductId { get; set; }

        public bool IsActive { get; set; }
    }
}
