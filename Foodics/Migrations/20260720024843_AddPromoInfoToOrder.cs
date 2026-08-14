using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foodics.Migrations
{
    /// <inheritdoc />
    public partial class AddPromoInfoToOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PromoCode",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PromoDiscountPercentage",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PromoCode",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PromoDiscountPercentage",
                table: "Orders");
        }
    }
}
