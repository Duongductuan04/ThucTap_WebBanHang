using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SimpleTaskApp.Migrations
{
    /// <inheritdoc />
    public partial class AddKhoaNgoai : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MobilePhoneColorId",
                table: "AppImportDetails",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppImportDetails_MobilePhoneColorId",
                table: "AppImportDetails",
                column: "MobilePhoneColorId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppImportDetails_AppMobilePhoneColors_MobilePhoneColorId",
                table: "AppImportDetails",
                column: "MobilePhoneColorId",
                principalTable: "AppMobilePhoneColors",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppImportDetails_AppMobilePhoneColors_MobilePhoneColorId",
                table: "AppImportDetails");

            migrationBuilder.DropIndex(
                name: "IX_AppImportDetails_MobilePhoneColorId",
                table: "AppImportDetails");

            migrationBuilder.DropColumn(
                name: "MobilePhoneColorId",
                table: "AppImportDetails");
        }
    }
}
