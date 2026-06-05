using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foodics.Migrations
{
    /// <inheritdoc />
    public partial class AddModelstLocalization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "ModifierOptions");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "ModifierGroups");

            migrationBuilder.RenameColumn(
                name: "ProductName",
                table: "OrderItems",
                newName: "ProductNameEn");

            migrationBuilder.AddColumn<string>(
                name: "ProductNameAr",
                table: "OrderItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BodyAr",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BodyEn",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TitleAr",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TitleEn",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameAr",
                table: "ModifierOptions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameEn",
                table: "ModifierOptions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameAr",
                table: "ModifierGroups",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameEn",
                table: "ModifierGroups",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescriptionAr",
                table: "Categories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescriptionEn",
                table: "Categories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameAr",
                table: "Categories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameEn",
                table: "Categories",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProductNameAr",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "BodyAr",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "BodyEn",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "TitleAr",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "TitleEn",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "NameAr",
                table: "ModifierOptions");

            migrationBuilder.DropColumn(
                name: "NameEn",
                table: "ModifierOptions");

            migrationBuilder.DropColumn(
                name: "NameAr",
                table: "ModifierGroups");

            migrationBuilder.DropColumn(
                name: "NameEn",
                table: "ModifierGroups");

            migrationBuilder.DropColumn(
                name: "DescriptionAr",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "DescriptionEn",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "NameAr",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "NameEn",
                table: "Categories");

            migrationBuilder.RenameColumn(
                name: "ProductNameEn",
                table: "OrderItems",
                newName: "ProductName");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "ModifierOptions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "ModifierGroups",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
