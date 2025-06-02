using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Evacuation_Planning_and_Monitoring_API.Migrations
{
    /// <inheritdoc />
    public partial class EvaStatusPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EvacuationPlans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ZoneID = table.Column<string>(type: "nvarchar(10)", nullable: false),
                    VehicleID = table.Column<string>(type: "nvarchar(10)", nullable: false),
                    ETA = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NumberOfPeople = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvacuationPlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EvacuationPlans_EvacuationZones_ZoneID",
                        column: x => x.ZoneID,
                        principalTable: "EvacuationZones",
                        principalColumn: "ZoneID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EvacuationPlans_Vehicles_VehicleID",
                        column: x => x.VehicleID,
                        principalTable: "Vehicles",
                        principalColumn: "VehicleID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EvacuationStatuses",
                columns: table => new
                {
                    ZoneID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TotalEvacuated = table.Column<int>(type: "int", nullable: false),
                    RemainingPeople = table.Column<int>(type: "int", nullable: false),
                    LastVehicleIDUsed = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvacuationStatuses", x => x.ZoneID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EvacuationPlans_VehicleID",
                table: "EvacuationPlans",
                column: "VehicleID");

            migrationBuilder.CreateIndex(
                name: "IX_EvacuationPlans_ZoneID",
                table: "EvacuationPlans",
                column: "ZoneID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EvacuationPlans");

            migrationBuilder.DropTable(
                name: "EvacuationStatuses");
        }
    }
}
