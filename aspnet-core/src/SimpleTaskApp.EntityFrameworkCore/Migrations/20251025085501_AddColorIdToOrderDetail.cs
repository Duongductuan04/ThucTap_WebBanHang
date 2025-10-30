using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SimpleTaskApp.Migrations
{
    /// <inheritdoc />
    public partial class AddColorIdToOrderDetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MobilePhoneColorId",
                table: "AppOrderDetails",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppOrderDetails_MobilePhoneColorId",
                table: "AppOrderDetails",
                column: "MobilePhoneColorId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppOrderDetails_AppMobilePhoneColors_MobilePhoneColorId",
                table: "AppOrderDetails",
                column: "MobilePhoneColorId",
                principalTable: "AppMobilePhoneColors",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppOrderDetails_AppMobilePhoneColors_MobilePhoneColorId",
                table: "AppOrderDetails");

            migrationBuilder.DropIndex(
                name: "IX_AppOrderDetails_MobilePhoneColorId",
                table: "AppOrderDetails");

            migrationBuilder.DropColumn(
                name: "MobilePhoneColorId",
                table: "AppOrderDetails");
        }
    }
}
