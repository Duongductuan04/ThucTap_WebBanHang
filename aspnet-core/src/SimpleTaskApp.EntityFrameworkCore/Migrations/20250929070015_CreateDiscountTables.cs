using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SimpleTaskApp.Migrations
{
    /// <inheritdoc />
    public partial class CreateDiscountTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DiscountAmount",
                table: "AppOrders",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "DiscountId",
                table: "AppOrders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FinalAmount",
                table: "AppOrders",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "AppOrders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ShippingFee",
                table: "AppOrders",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "AppDiscounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Percentage = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MinOrderValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ApplyType = table.Column<int>(type: "int", nullable: false),
                    MaxUsage = table.Column<int>(type: "int", nullable: false),
                    CurrentUsage = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppDiscounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppDiscountCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DiscountId = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppDiscountCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppDiscountCategories_AppDiscounts_DiscountId",
                        column: x => x.DiscountId,
                        principalTable: "AppDiscounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppDiscountCategories_AppMobilePhoneCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "AppMobilePhoneCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppDiscountProducts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DiscountId = table.Column<int>(type: "int", nullable: false),
                    MobilePhoneId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppDiscountProducts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppDiscountProducts_AppDiscounts_DiscountId",
                        column: x => x.DiscountId,
                        principalTable: "AppDiscounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppDiscountProducts_AppMobilePhones_MobilePhoneId",
                        column: x => x.MobilePhoneId,
                        principalTable: "AppMobilePhones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppOrders_DiscountId",
                table: "AppOrders",
                column: "DiscountId");

            migrationBuilder.CreateIndex(
                name: "IX_AppDiscountCategories_CategoryId",
                table: "AppDiscountCategories",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_AppDiscountCategories_DiscountId",
                table: "AppDiscountCategories",
                column: "DiscountId");

            migrationBuilder.CreateIndex(
                name: "IX_AppDiscountProducts_DiscountId",
                table: "AppDiscountProducts",
                column: "DiscountId");

            migrationBuilder.CreateIndex(
                name: "IX_AppDiscountProducts_MobilePhoneId",
                table: "AppDiscountProducts",
                column: "MobilePhoneId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppOrders_AppDiscounts_DiscountId",
                table: "AppOrders",
                column: "DiscountId",
                principalTable: "AppDiscounts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppOrders_AppDiscounts_DiscountId",
                table: "AppOrders");

            migrationBuilder.DropTable(
                name: "AppDiscountCategories");

            migrationBuilder.DropTable(
                name: "AppDiscountProducts");

            migrationBuilder.DropTable(
                name: "AppDiscounts");

            migrationBuilder.DropIndex(
                name: "IX_AppOrders_DiscountId",
                table: "AppOrders");

            migrationBuilder.DropColumn(
                name: "DiscountAmount",
                table: "AppOrders");

            migrationBuilder.DropColumn(
                name: "DiscountId",
                table: "AppOrders");

            migrationBuilder.DropColumn(
                name: "FinalAmount",
                table: "AppOrders");

            migrationBuilder.DropColumn(
                name: "Note",
                table: "AppOrders");

            migrationBuilder.DropColumn(
                name: "ShippingFee",
                table: "AppOrders");
        }
    }
}
