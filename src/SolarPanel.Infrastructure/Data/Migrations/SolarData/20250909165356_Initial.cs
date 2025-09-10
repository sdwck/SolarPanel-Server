#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace SolarPanel.Infrastructure.Data.Migrations.SolarData
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SolarData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Command = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CommandDescription = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    InverterHeatSinkTemperature = table.Column<int>(type: "int", nullable: false),
                    BusVoltage = table.Column<double>(type: "float", nullable: false),
                    IsSbuPriorityVersionAdded = table.Column<bool>(type: "bit", nullable: false),
                    IsConfigurationChanged = table.Column<bool>(type: "bit", nullable: false),
                    IsSccFirmwareUpdated = table.Column<bool>(type: "bit", nullable: false),
                    IsLoadOn = table.Column<bool>(type: "bit", nullable: false),
                    IsBatteryVoltageToSteadyWhileCharging = table.Column<bool>(type: "bit", nullable: false),
                    IsChargingOn = table.Column<bool>(type: "bit", nullable: false),
                    IsSccChargingOn = table.Column<bool>(type: "bit", nullable: false),
                    IsAcChargingOn = table.Column<bool>(type: "bit", nullable: false),
                    IsChargingToFloat = table.Column<bool>(type: "bit", nullable: false),
                    IsSwitchedOn = table.Column<bool>(type: "bit", nullable: false),
                    IsReserved = table.Column<bool>(type: "bit", nullable: false),
                    Rsv1 = table.Column<int>(type: "int", nullable: false),
                    Rsv2 = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolarData", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BatteryData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SolarDataId = table.Column<int>(type: "int", nullable: false),
                    BatteryVoltage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    BatteryVoltageFromScc = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    BatteryChargingCurrent = table.Column<decimal>(type: "decimal(5,1)", nullable: false),
                    BatteryCapacity = table.Column<int>(type: "int", nullable: false),
                    BatteryDischargeCurrent = table.Column<decimal>(type: "decimal(5,1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BatteryData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BatteryData_SolarData_SolarDataId",
                        column: x => x.SolarDataId,
                        principalTable: "SolarData",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PowerData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SolarDataId = table.Column<int>(type: "int", nullable: false),
                    AcInputVoltage = table.Column<decimal>(type: "decimal(6,2)", nullable: false),
                    AcInputFrequency = table.Column<decimal>(type: "decimal(4,1)", nullable: false),
                    AcOutputVoltage = table.Column<decimal>(type: "decimal(6,2)", nullable: false),
                    AcOutputFrequency = table.Column<decimal>(type: "decimal(4,1)", nullable: false),
                    AcOutputApparentPower = table.Column<int>(type: "int", nullable: false),
                    AcOutputActivePower = table.Column<int>(type: "int", nullable: false),
                    AcOutputLoad = table.Column<int>(type: "int", nullable: false),
                    PvInputCurrent = table.Column<decimal>(type: "decimal(5,1)", nullable: false),
                    PvInputVoltage = table.Column<decimal>(type: "decimal(6,2)", nullable: false),
                    PvInputPower = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PowerData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PowerData_SolarData_SolarDataId",
                        column: x => x.SolarDataId,
                        principalTable: "SolarData",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BatteryData_SolarDataId",
                table: "BatteryData",
                column: "SolarDataId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PowerData_SolarDataId",
                table: "PowerData",
                column: "SolarDataId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SolarData_Timestamp",
                table: "SolarData",
                column: "Timestamp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BatteryData");

            migrationBuilder.DropTable(
                name: "PowerData");

            migrationBuilder.DropTable(
                name: "SolarData");
        }
    }
}
