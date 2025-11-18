using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SimpleTaskApp.Migrations
{
    /// <inheritdoc />
    public partial class Add_MobilePhoneColor_Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppMobilePhoneColors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MobilePhoneId = table.Column<int>(type: "int", nullable: false),
                    ColorName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ColorHex = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppMobilePhoneColors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppMobilePhoneColors_AppMobilePhones_MobilePhoneId",
                        column: x => x.MobilePhoneId,
                        principalTable: "AppMobilePhones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppMobilePhoneColors_MobilePhoneId",
                table: "AppMobilePhoneColors",
                column: "MobilePhoneId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppMobilePhoneColors");
        }
    }
}
