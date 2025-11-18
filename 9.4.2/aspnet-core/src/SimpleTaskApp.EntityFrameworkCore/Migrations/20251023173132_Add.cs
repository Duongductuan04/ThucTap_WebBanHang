using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SimpleTaskApp.Migrations
{
    /// <inheritdoc />
    public partial class Add : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ColorId",
                table: "AppOrderDetails");

            migrationBuilder.DropColumn(
                name: "ColorImageUrl",
                table: "AppOrderDetails");

            migrationBuilder.DropColumn(
                name: "ColorName",
                table: "AppOrderDetails");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ColorId",
                table: "AppOrderDetails",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ColorImageUrl",
                table: "AppOrderDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ColorName",
                table: "AppOrderDetails",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
