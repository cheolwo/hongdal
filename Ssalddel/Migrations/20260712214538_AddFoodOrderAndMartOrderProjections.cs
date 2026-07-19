using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssalddel.Migrations
{
    /// <inheritdoc />
    public partial class AddFoodOrderAndMartOrderProjections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "마트주문",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    주문참조번호 = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    주문_id = table.Column<long>(type: "bigint", nullable: true),
                    주문자_user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    판매자_user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    상태 = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    현재단계 = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    community_ledger_id = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    community_ledger_template_key = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    community_ledger_state = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    community_ledger_synced_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_마트주문", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "음식주문",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    주문번호 = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    음식점_id = table.Column<long>(type: "bigint", nullable: false),
                    음식점명 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    음식점주소 = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    음식점상세주소 = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    음식점위도 = table.Column<decimal>(type: "decimal(18,10)", nullable: true),
                    음식점경도 = table.Column<decimal>(type: "decimal(18,10)", nullable: true),
                    주문자_user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    수령인명 = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    수령인연락처 = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    수령지주소 = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    수령지상세주소 = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    수령요청사항 = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    주문자본인수령여부 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    총주문금액 = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    상태 = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    배차상태 = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    배차대기_id = table.Column<long>(type: "bigint", nullable: true),
                    결제수단 = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    음식점수락시각_utc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    조리예상완료시각_utc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    배차요청시각_utc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    수락메모 = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    community_ledger_id = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    community_ledger_template_key = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    community_ledger_state = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    community_ledger_synced_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_음식주문", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "마트주문상품",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    마트주문_id = table.Column<long>(type: "bigint", nullable: false),
                    출고예정_id = table.Column<long>(type: "bigint", nullable: true),
                    상품명 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    sku = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    수량 = table.Column<int>(type: "int", nullable: false),
                    상태 = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_마트주문상품", x => x.id);
                    table.ForeignKey(
                        name: "FK_마트주문상품_마트주문_마트주문_id",
                        column: x => x.마트주문_id,
                        principalTable: "마트주문",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "음식주문상태이력",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    음식주문_id = table.Column<long>(type: "bigint", nullable: false),
                    이전상태 = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    다음상태 = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    사유 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    전이시각_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_음식주문상태이력", x => x.id);
                    table.ForeignKey(
                        name: "FK_음식주문상태이력_음식주문_음식주문_id",
                        column: x => x.음식주문_id,
                        principalTable: "음식주문",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "음식주문상품",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    음식주문_id = table.Column<long>(type: "bigint", nullable: false),
                    상품명 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    수량 = table.Column<int>(type: "int", nullable: false),
                    단가 = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_음식주문상품", x => x.id);
                    table.ForeignKey(
                        name: "FK_음식주문상품_음식주문_음식주문_id",
                        column: x => x.음식주문_id,
                        principalTable: "음식주문",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_마트주문_주문자_user_id_상태_created_at",
                table: "마트주문",
                columns: new[] { "주문자_user_id", "상태", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_마트주문_주문참조번호",
                table: "마트주문",
                column: "주문참조번호",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_마트주문_판매자_user_id_상태_created_at",
                table: "마트주문",
                columns: new[] { "판매자_user_id", "상태", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_마트주문_community_ledger_id",
                table: "마트주문",
                column: "community_ledger_id");

            migrationBuilder.CreateIndex(
                name: "IX_마트주문상품_마트주문_id_출고예정_id",
                table: "마트주문상품",
                columns: new[] { "마트주문_id", "출고예정_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_마트주문상품_출고예정_id",
                table: "마트주문상품",
                column: "출고예정_id");

            migrationBuilder.CreateIndex(
                name: "IX_마트주문상품_sku",
                table: "마트주문상품",
                column: "sku");

            migrationBuilder.CreateIndex(
                name: "IX_음식주문_배차대기_id",
                table: "음식주문",
                column: "배차대기_id");

            migrationBuilder.CreateIndex(
                name: "IX_음식주문_음식점_id_상태_created_at",
                table: "음식주문",
                columns: new[] { "음식점_id", "상태", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_음식주문_주문번호",
                table: "음식주문",
                column: "주문번호",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_음식주문_주문자_user_id_상태_created_at",
                table: "음식주문",
                columns: new[] { "주문자_user_id", "상태", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_음식주문_community_ledger_id",
                table: "음식주문",
                column: "community_ledger_id");

            migrationBuilder.CreateIndex(
                name: "IX_음식주문상태이력_음식주문_id_전이시각_utc",
                table: "음식주문상태이력",
                columns: new[] { "음식주문_id", "전이시각_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_음식주문상품_상품명",
                table: "음식주문상품",
                column: "상품명");

            migrationBuilder.CreateIndex(
                name: "IX_음식주문상품_음식주문_id",
                table: "음식주문상품",
                column: "음식주문_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "마트주문상품");

            migrationBuilder.DropTable(
                name: "음식주문상태이력");

            migrationBuilder.DropTable(
                name: "음식주문상품");

            migrationBuilder.DropTable(
                name: "마트주문");

            migrationBuilder.DropTable(
                name: "음식주문");
        }
    }
}
