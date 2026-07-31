using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssalddel.Migrations
{
    /// <inheritdoc />
    public partial class AddDriverShiftTransportExecutionType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "transport_execution_type",
                table: "driver_shifts",
                type: "varchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "CargoTransport")
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "transport_execution_type",
                table: "driver_shifts");
        }
    }
}
