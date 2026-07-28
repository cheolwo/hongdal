using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssalddel.Migrations
{
    /// <inheritdoc />
    public partial class AddFoodOperationsReviewsAndPolicy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "음식운영정책",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    기본저평점게시일수 = table.Column<int>(type: "int", nullable: false),
                    기본요금 = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    포함거리_m = table.Column<int>(type: "int", nullable: false),
                    거리단위_m = table.Column<int>(type: "int", nullable: false),
                    거리단위요금 = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    최소요금 = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    기사기본지급액 = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    기사거리단위지급액 = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    기사최소지급액 = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    수정자_user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    updated_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_음식운영정책", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "음식점리뷰",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    음식점_id = table.Column<long>(type: "bigint", nullable: false),
                    주문자_user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    주문번호 = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    별점 = table.Column<int>(type: "int", nullable: false),
                    내용 = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    사진_urls_json = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    같은음식점_저평점3회연속 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    사장노출허용 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    관리자검토필요 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    관리자게시강제 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    현재노출 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    게시종료일시_utc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    최근조치사유 = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_음식점리뷰", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_음식점리뷰_관리자검토필요_created_at_utc",
                table: "음식점리뷰",
                columns: new[] { "관리자검토필요", "created_at_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_음식점리뷰_음식점_id_현재노출_created_at_utc",
                table: "음식점리뷰",
                columns: new[] { "음식점_id", "현재노출", "created_at_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_음식점리뷰_주문번호",
                table: "음식점리뷰",
                column: "주문번호",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "음식운영정책");

            migrationBuilder.DropTable(
                name: "음식점리뷰");
        }
    }
}
