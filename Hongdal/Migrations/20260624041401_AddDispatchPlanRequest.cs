using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hongdal.Migrations
{
    public partial class AddDispatchPlanRequest : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "배차계획신청",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    기사Id = table.Column<string>(type: "longtext", nullable: false),
                    출발지 = table.Column<string>(type: "longtext", nullable: false),
                    복귀지 = table.Column<string>(type: "longtext", nullable: false),
                    희망복귀시각 = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    배차가능시각 = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    상태 = table.Column<string>(type: "longtext", nullable: false),
                    메모 = table.Column<string>(type: "longtext", nullable: false),
                    신청일시 = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_배차계획신청", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "배차계획신청");
        }
    }
}
