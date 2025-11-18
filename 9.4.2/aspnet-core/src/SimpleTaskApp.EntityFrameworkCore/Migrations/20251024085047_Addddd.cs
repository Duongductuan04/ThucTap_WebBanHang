using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SimpleTaskApp.Migrations
{
    /// <inheritdoc />
    public partial class Addddd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MobilePhoneColorId",
                table: "AppCarts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppCarts_MobilePhoneColorId",
                table: "AppCarts",
                column: "MobilePhoneColorId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppCarts_AppMobilePhoneColors_MobilePhoneColorId",
                table: "AppCarts",
                column: "MobilePhoneColorId",
                principalTable: "AppMobilePhoneColors",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppCarts_AppMobilePhoneColors_MobilePhoneColorId",
                table: "AppCarts");

            migrationBuilder.DropIndex(
                name: "IX_AppCarts_MobilePhoneColorId",
                table: "AppCarts");

            migrationBuilder.DropColumn(
                name: "MobilePhoneColorId",
                table: "AppCarts");
        }
    }
}
