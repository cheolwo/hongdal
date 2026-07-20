using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssalddel.Migrations
{
    /// <inheritdoc />
    public partial class AddMartOrderRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "마트주문요청",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    요청자_user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    클라이언트_요청_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    공개상품_id = table.Column<long>(type: "bigint", nullable: false),
                    상품명_snapshot = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    판매단위_snapshot = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    단가_snapshot = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    수량 = table.Column<int>(type: "int", nullable: false),
                    합계_snapshot = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    통화 = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    제출시_판매가능수량 = table.Column<int>(type: "int", nullable: false),
                    재고기준시각_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    상태_code = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    비구속_주문요청_확인 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    안내_version = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_마트주문요청", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_마트주문요청_공개상품_id_created_at_utc",
                table: "마트주문요청",
                columns: new[] { "공개상품_id", "created_at_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_마트주문요청_요청자_user_id_클라이언트_요청_id",
                table: "마트주문요청",
                columns: new[] { "요청자_user_id", "클라이언트_요청_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_마트주문요청_요청자_user_id_created_at_utc",
                table: "마트주문요청",
                columns: new[] { "요청자_user_id", "created_at_utc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "마트주문요청");
        }
    }
}
