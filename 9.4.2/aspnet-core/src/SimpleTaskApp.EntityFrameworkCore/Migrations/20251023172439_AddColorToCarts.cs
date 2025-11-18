using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SimpleTaskApp.Migrations
{
    /// <inheritdoc />
    public partial class AddColorToCarts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ColorId",
                table: "AppCarts");

            migrationBuilder.DropColumn(
                name: "ColorImageUrl",
                table: "AppCarts");

            migrationBuilder.DropColumn(
                name: "ColorName",
                table: "AppCarts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ColorId",
                table: "AppCarts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ColorImageUrl",
                table: "AppCarts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ColorName",
                table: "AppCarts",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
