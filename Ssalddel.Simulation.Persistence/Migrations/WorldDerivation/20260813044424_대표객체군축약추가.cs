using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssalddel.Simulation.Persistence.Migrations.WorldDerivation
{
    /// <inheritdoc />
    public partial class 대표객체군축약추가 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "대표군코드",
                table: "시뮬레이션월드_파생노드",
                type: "varchar(300)",
                maxLength: 300,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "대표순위",
                table: "시뮬레이션월드_파생노드",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "대표원본건수",
                table: "시뮬레이션월드_파생노드",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "대표군코드",
                table: "시뮬레이션월드_파생노드");

            migrationBuilder.DropColumn(
                name: "대표순위",
                table: "시뮬레이션월드_파생노드");

            migrationBuilder.DropColumn(
                name: "대표원본건수",
                table: "시뮬레이션월드_파생노드");
        }
    }
}
