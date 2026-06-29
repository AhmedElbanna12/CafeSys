using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foodics.Migrations
{
    /// <inheritdoc />
    public partial class AddQuantityOrderItemModifier : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "OrderItemModifiers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "OrderItemModifiers");
        }
    }
}
