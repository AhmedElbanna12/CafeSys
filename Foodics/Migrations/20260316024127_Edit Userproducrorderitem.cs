using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foodics.Migrations
{
    /// <inheritdoc />
    public partial class EditUserproducrorderitem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DiscountEnd",
                table: "Products",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountPercentage",
                table: "Products",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DiscountStart",
                table: "Products",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountAmount",
                table: "OrderItems",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "ProductSizeId",
                table: "OrderItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerCode",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_ProductSizeId",
                table: "OrderItems",
                column: "ProductSizeId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_ProductSizes_ProductSizeId",
                table: "OrderItems",
                column: "ProductSizeId",
                principalTable: "ProductSizes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_ProductSizes_ProductSizeId",
                table: "OrderItems");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_ProductSizeId",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "DiscountEnd",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "DiscountPercentage",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "DiscountStart",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "DiscountAmount",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "ProductSizeId",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "CustomerCode",
                table: "AspNetUsers");
        }
    }
}
