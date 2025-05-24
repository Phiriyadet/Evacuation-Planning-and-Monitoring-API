using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Evacuation_Planning_and_Monitoring_API.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EvacuationZones",
                columns: table => new
                {
                    ZoneID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    LocationCoordinates_Latitude = table.Column<double>(type: "float", nullable: false),
                    LocationCoordinates_Longitude = table.Column<double>(type: "float", nullable: false),
                    NumberOfPeople = table.Column<int>(type: "int", nullable: false),
                    UrgencyLevel = table.Column<int>(type: "int", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvacuationZones", x => x.ZoneID);
                });

            migrationBuilder.CreateTable(
                name: "Vehicles",
                columns: table => new
                {
                    VehicleID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Capacity = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LocationCoordinates_Latitude = table.Column<double>(type: "float", nullable: false),
                    LocationCoordinates_Longitude = table.Column<double>(type: "float", nullable: false),
                    Speed = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehicles", x => x.VehicleID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EvacuationZones");

            migrationBuilder.DropTable(
                name: "Vehicles");
        }
    }
}
