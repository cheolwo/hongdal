using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hongdal.Migrations
{
    /// <inheritdoc />
    public partial class AddProductScanCodeMapAndJourney : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "상품식별코드맵",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    코드값 = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    코드유형 = table.Column<int>(type: "int", nullable: false),
                    상품_id = table.Column<long>(type: "bigint", nullable: false),
                    활성여부 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_상품식별코드맵", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "연락처공개동의",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    인연연결요청_id = table.Column<long>(type: "bigint", nullable: false),
                    동의자_참여자_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    프로필공개 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    업체명공개 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    이메일공개 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    전화번호공개 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    카카오채널공개 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    판매채널공개 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    제공목적 = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    동의일시 = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    철회일시 = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_연락처공개동의", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "인연연결요청",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    요청자_참여자_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    요청자_역할 = table.Column<int>(type: "int", nullable: false),
                    대상자_참여자_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    대상자_역할 = table.Column<int>(type: "int", nullable: false),
                    감사메시지_id = table.Column<long>(type: "bigint", nullable: true),
                    주문_id = table.Column<long>(type: "bigint", nullable: true),
                    통관절차_id = table.Column<long>(type: "bigint", nullable: true),
                    요청목적 = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    요청메시지 = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    상태 = table.Column<int>(type: "int", nullable: false),
                    요청일시 = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    응답일시 = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    거절사유 = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_인연연결요청", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_상품식별코드맵_상품_id_활성여부",
                table: "상품식별코드맵",
                columns: new[] { "상품_id", "활성여부" });

            migrationBuilder.CreateIndex(
                name: "IX_상품식별코드맵_코드값",
                table: "상품식별코드맵",
                column: "코드값",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_연락처공개동의_동의자_참여자_id_동의일시",
                table: "연락처공개동의",
                columns: new[] { "동의자_참여자_id", "동의일시" });

            migrationBuilder.CreateIndex(
                name: "IX_연락처공개동의_인연연결요청_id_동의자_참여자_id",
                table: "연락처공개동의",
                columns: new[] { "인연연결요청_id", "동의자_참여자_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_인연연결요청_감사메시지_id_주문_id_통관절차_id",
                table: "인연연결요청",
                columns: new[] { "감사메시지_id", "주문_id", "통관절차_id" });

            migrationBuilder.CreateIndex(
                name: "IX_인연연결요청_대상자_참여자_id_상태_요청일시",
                table: "인연연결요청",
                columns: new[] { "대상자_참여자_id", "상태", "요청일시" });

            migrationBuilder.CreateIndex(
                name: "IX_인연연결요청_요청자_참여자_id_상태_요청일시",
                table: "인연연결요청",
                columns: new[] { "요청자_참여자_id", "상태", "요청일시" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "상품식별코드맵");

            migrationBuilder.DropTable(
                name: "연락처공개동의");

            migrationBuilder.DropTable(
                name: "인연연결요청");
        }
    }
}
