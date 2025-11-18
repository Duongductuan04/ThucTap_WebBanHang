using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SimpleTaskApp.Migrations
{
    /// <inheritdoc />
    public partial class AddImportTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppImports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ImportCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SupplierName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    KeeperName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    KeeperPhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppImports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppImportDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ImportId = table.Column<int>(type: "int", nullable: false),
                    MobilePhoneId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    ImportPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppImportDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppImportDetails_AppImports_ImportId",
                        column: x => x.ImportId,
                        principalTable: "AppImports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppImportDetails_AppMobilePhones_MobilePhoneId",
                        column: x => x.MobilePhoneId,
                        principalTable: "AppMobilePhones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppImportDetails_ImportId",
                table: "AppImportDetails",
                column: "ImportId");

            migrationBuilder.CreateIndex(
                name: "IX_AppImportDetails_MobilePhoneId",
                table: "AppImportDetails",
                column: "MobilePhoneId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppImportDetails");

            migrationBuilder.DropTable(
                name: "AppImports");
        }
    }
}
