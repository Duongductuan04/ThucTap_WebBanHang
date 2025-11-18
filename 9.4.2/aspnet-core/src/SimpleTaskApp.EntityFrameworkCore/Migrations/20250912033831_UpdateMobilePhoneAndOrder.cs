using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SimpleTaskApp.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMobilePhoneAndOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PaymentMethod",
                table: "AppOrders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "RecipientAddress",
                table: "AppOrders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RecipientName",
                table: "AppOrders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Brand",
                table: "AppMobilePhones",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountPrice",
                table: "AppMobilePhones",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsNew",
                table: "AppMobilePhones",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsOnSale",
                table: "AppMobilePhones",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ProductType",
                table: "AppMobilePhones",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SaleEnd",
                table: "AppMobilePhones",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SaleStart",
                table: "AppMobilePhones",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "AppOrders");

            migrationBuilder.DropColumn(
                name: "RecipientAddress",
                table: "AppOrders");

            migrationBuilder.DropColumn(
                name: "RecipientName",
                table: "AppOrders");

            migrationBuilder.DropColumn(
                name: "Brand",
                table: "AppMobilePhones");

            migrationBuilder.DropColumn(
                name: "DiscountPrice",
                table: "AppMobilePhones");

            migrationBuilder.DropColumn(
                name: "IsNew",
                table: "AppMobilePhones");

            migrationBuilder.DropColumn(
                name: "IsOnSale",
                table: "AppMobilePhones");

            migrationBuilder.DropColumn(
                name: "ProductType",
                table: "AppMobilePhones");

            migrationBuilder.DropColumn(
                name: "SaleEnd",
                table: "AppMobilePhones");

            migrationBuilder.DropColumn(
                name: "SaleStart",
                table: "AppMobilePhones");
        }
    }
}
