using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Foodics.Migrations
{
    /// <inheritdoc />
    public partial class AddHomeSectionSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "HomeSections",
                columns: new[] { "Id", "DisplayOrder", "IsVisible", "SectionName" },
                values: new object[,]
                {
                    { 1, 1, true, 1 },
                    { 2, 2, true, 2 },
                    { 3, 3, true, 3 },
                    { 4, 4, true, 4 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "HomeSections",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "HomeSections",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "HomeSections",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "HomeSections",
                keyColumn: "Id",
                keyValue: 4);
        }
    }
}
