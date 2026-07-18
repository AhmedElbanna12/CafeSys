using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foodics.Migrations
{
    /// <inheritdoc />
    public partial class EditSplashBanner : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Title",
                table: "SplashScreens",
                newName: "TitleEn");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "SplashScreens",
                newName: "TitleAr");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "Banners",
                newName: "TitleEn");

            migrationBuilder.RenameColumn(
                name: "SubDescription",
                table: "Banners",
                newName: "TitleAr");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Banners",
                newName: "SubDescriptionEn");

            migrationBuilder.AlterColumn<string>(
                name: "Photo",
                table: "SplashScreens",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "DescriptionAr",
                table: "SplashScreens",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DescriptionEn",
                table: "SplashScreens",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsVisible",
                table: "SplashScreens",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "Photo",
                table: "Banners",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "DescriptionAr",
                table: "Banners",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DescriptionEn",
                table: "Banners",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SubDescriptionAr",
                table: "Banners",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DescriptionAr",
                table: "SplashScreens");

            migrationBuilder.DropColumn(
                name: "DescriptionEn",
                table: "SplashScreens");

            migrationBuilder.DropColumn(
                name: "IsVisible",
                table: "SplashScreens");

            migrationBuilder.DropColumn(
                name: "DescriptionAr",
                table: "Banners");

            migrationBuilder.DropColumn(
                name: "DescriptionEn",
                table: "Banners");

            migrationBuilder.DropColumn(
                name: "SubDescriptionAr",
                table: "Banners");

            migrationBuilder.RenameColumn(
                name: "TitleEn",
                table: "SplashScreens",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "TitleAr",
                table: "SplashScreens",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "TitleEn",
                table: "Banners",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "TitleAr",
                table: "Banners",
                newName: "SubDescription");

            migrationBuilder.RenameColumn(
                name: "SubDescriptionEn",
                table: "Banners",
                newName: "Description");

            migrationBuilder.AlterColumn<string>(
                name: "Photo",
                table: "SplashScreens",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Photo",
                table: "Banners",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
