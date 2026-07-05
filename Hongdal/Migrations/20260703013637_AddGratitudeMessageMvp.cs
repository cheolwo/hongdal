using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hongdal.Migrations
{
    /// <inheritdoc />
    public partial class AddGratitudeMessageMvp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "감사메시지",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    상품_id = table.Column<long>(type: "bigint", nullable: false),
                    주문_id = table.Column<long>(type: "bigint", nullable: true),
                    통관절차_id = table.Column<long>(type: "bigint", nullable: true),
                    발신자구분 = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    발신참여자_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    대상역할 = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    대상참여자_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    대상표시명 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    메시지내용 = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    공개가능여부 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    수신자에게전달여부 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    검수상태 = table.Column<int>(type: "int", nullable: false),
                    작성일시 = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_감사메시지", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_감사메시지_대상역할_대상참여자_id_작성일시",
                table: "감사메시지",
                columns: new[] { "대상역할", "대상참여자_id", "작성일시" });

            migrationBuilder.CreateIndex(
                name: "IX_감사메시지_상품_id_작성일시",
                table: "감사메시지",
                columns: new[] { "상품_id", "작성일시" });

            migrationBuilder.CreateIndex(
                name: "IX_감사메시지_통관절차_id_주문_id",
                table: "감사메시지",
                columns: new[] { "통관절차_id", "주문_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "감사메시지");
        }
    }
}
