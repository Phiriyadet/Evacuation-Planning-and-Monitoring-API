using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Evacuation_Planning_and_Monitoring_API.Migrations
{
    /// <inheritdoc />
    public partial class mssql_migration_416 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "UrgencyLevel",
                table: "EvacuationZones",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "UrgencyLevel",
                table: "EvacuationZones",
                type: "int",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
