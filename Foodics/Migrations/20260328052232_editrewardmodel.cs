using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foodics.Migrations
{
    /// <inheritdoc />
    public partial class editrewardmodel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsUsed",
                table: "RedeemedRewards",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsUsed",
                table: "RedeemedRewards");
        }
    }
}
