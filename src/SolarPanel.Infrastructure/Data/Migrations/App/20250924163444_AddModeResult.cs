using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SolarPanel.Infrastructure.Data.Migrations.App
{
    /// <inheritdoc />
    public partial class AddModeResult : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ModeResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BatteryMode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LoadMode = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModeResults", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ModeResults");
        }
    }
}
