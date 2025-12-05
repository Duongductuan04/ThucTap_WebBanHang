using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SimpleTaskApp.Migrations
{
    /// <inheritdoc />
    public partial class AddSupplierToImport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SupplierName",
                table: "AppImports");

      migrationBuilder.AddColumn<int>(
name: "SupplierId",
table: "AppImports",
type: "int",
nullable: true);

      migrationBuilder.CreateIndex(
                name: "IX_AppImports_SupplierId",
                table: "AppImports",
                column: "SupplierId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppImports_AppSuppliers_SupplierId",
                table: "AppImports",
                column: "SupplierId",
                principalTable: "AppSuppliers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppImports_AppSuppliers_SupplierId",
                table: "AppImports");

            migrationBuilder.DropIndex(
                name: "IX_AppImports_SupplierId",
                table: "AppImports");

            migrationBuilder.DropColumn(
                name: "SupplierId",
                table: "AppImports");

            migrationBuilder.AddColumn<string>(
                name: "SupplierName",
                table: "AppImports",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
