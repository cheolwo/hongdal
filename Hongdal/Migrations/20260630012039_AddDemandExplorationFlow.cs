using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hongdal.Migrations
{
    /// <inheritdoc />
    public partial class AddDemandExplorationFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "기사운행탐색",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    기사Id = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    탐색명 = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    운행예정일 = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    출발권역 = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    희망도착권역 = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    경유권역Json = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    차량종류 = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    최대적재중량Kg = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    최대적재부피Cbm = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    모집대상수 = table.Column<int>(type: "int", nullable: false),
                    탐색상태 = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    응답요약 = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    실행판단사유 = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    메모 = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_기사운행탐색", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "기사화주인연집계",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    기사Id = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    화주UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    최근거래일시 = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    누적운송건수 = table.Column<int>(type: "int", nullable: false),
                    최근응답률 = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    최근30일접점수 = table.Column<int>(type: "int", nullable: false),
                    취소율 = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    인연점수 = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    최근연락일시 = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    선호출발권역 = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    선호도착권역 = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_기사화주인연집계", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "운송의뢰상품연결",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    운송의뢰_id = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    입고상품_id = table.Column<long>(type: "bigint", nullable: false),
                    할당수량 = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_운송의뢰상품연결", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "운행탐색대상화주",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    운행탐색Id = table.Column<long>(type: "bigint", nullable: false),
                    화주UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    인연점수Snapshot = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    반응가능성점수Snapshot = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    선정사유 = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    대상상태 = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    발송메시지 = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    발송일시 = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    마지막응답일시 = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    예상화물정보요약 = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_운행탐색대상화주", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "운행탐색응답요약",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    운행탐색Id = table.Column<long>(type: "bigint", nullable: false),
                    화주UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    응답유형 = table.Column<int>(type: "int", nullable: false),
                    희망상차일시 = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    출발지요약 = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    도착지요약 = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    예상중량Kg = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    예상부피Cbm = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    예상팔레트개수 = table.Column<int>(type: "int", nullable: true),
                    메모요약 = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    응답일시 = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_운행탐색응답요약", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "입고상품",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    입고요청_id = table.Column<long>(type: "bigint", nullable: false),
                    창고_id = table.Column<long>(type: "bigint", nullable: false),
                    소유자_user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    판매자_user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    상품명 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    sku = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    옵션명 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    입고수량 = table.Column<int>(type: "int", nullable: false),
                    가용수량 = table.Column<int>(type: "int", nullable: false),
                    예약수량 = table.Column<int>(type: "int", nullable: false),
                    불량수량 = table.Column<int>(type: "int", nullable: false),
                    보관위치 = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    상태 = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    입고완료일시 = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_입고상품", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "입고요청",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    창고_id = table.Column<long>(type: "bigint", nullable: false),
                    주문자_user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    공급처명 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    원주문참조번호 = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    상태 = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    예정도착일 = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    비고 = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    입고완료일시 = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_입고요청", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "재고이력",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    입고상품_id = table.Column<long>(type: "bigint", nullable: false),
                    이력유형 = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    변경수량 = table.Column<int>(type: "int", nullable: false),
                    변경후수량 = table.Column<int>(type: "int", nullable: false),
                    원인유형 = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    원인_id = table.Column<long>(type: "bigint", nullable: true),
                    처리_user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    메모 = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    처리일시 = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_재고이력", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "창고",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    소유자_user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    창고명 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    사업자번호 = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    주소 = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    담당자명 = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    연락처 = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_창고", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "창고사용자",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    창고_id = table.Column<long>(type: "bigint", nullable: false),
                    user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    역할명 = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_primary = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_창고사용자", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "채널출품",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    판매상품_id = table.Column<long>(type: "bigint", nullable: false),
                    판매채널계정_id = table.Column<long>(type: "bigint", nullable: false),
                    채널상품번호 = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    출품상태 = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    동기화상태 = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    에러메시지 = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_채널출품", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "판매상품",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    입고상품_id = table.Column<long>(type: "bigint", nullable: false),
                    소유자_user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    대표상품명 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    판매sku = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    판매가 = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    상태 = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_판매상품", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "판매채널계정",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    채널종류 = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    상점명 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    연결상태 = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    토큰암호화저장값 = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    마지막동기화일시 = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_판매채널계정", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_기사운행탐색_기사Id_운행예정일_탐색상태",
                table: "기사운행탐색",
                columns: new[] { "기사Id", "운행예정일", "탐색상태" });

            migrationBuilder.CreateIndex(
                name: "IX_기사화주인연집계_기사Id_화주UserId",
                table: "기사화주인연집계",
                columns: new[] { "기사Id", "화주UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_운송의뢰상품연결_운송의뢰_id_입고상품_id",
                table: "운송의뢰상품연결",
                columns: new[] { "운송의뢰_id", "입고상품_id" });

            migrationBuilder.CreateIndex(
                name: "IX_운행탐색대상화주_운행탐색Id_화주UserId",
                table: "운행탐색대상화주",
                columns: new[] { "운행탐색Id", "화주UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_운행탐색응답요약_운행탐색Id_화주UserId",
                table: "운행탐색응답요약",
                columns: new[] { "운행탐색Id", "화주UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_입고상품_창고_id_소유자_user_id_상태",
                table: "입고상품",
                columns: new[] { "창고_id", "소유자_user_id", "상태" });

            migrationBuilder.CreateIndex(
                name: "IX_입고요청_창고_id_주문자_user_id_상태",
                table: "입고요청",
                columns: new[] { "창고_id", "주문자_user_id", "상태" });

            migrationBuilder.CreateIndex(
                name: "IX_재고이력_입고상품_id_처리일시",
                table: "재고이력",
                columns: new[] { "입고상품_id", "처리일시" });

            migrationBuilder.CreateIndex(
                name: "IX_창고_소유자_user_id_창고명",
                table: "창고",
                columns: new[] { "소유자_user_id", "창고명" });

            migrationBuilder.CreateIndex(
                name: "IX_창고사용자_창고_id_user_id_역할명",
                table: "창고사용자",
                columns: new[] { "창고_id", "user_id", "역할명" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_채널출품_판매상품_id_판매채널계정_id",
                table: "채널출품",
                columns: new[] { "판매상품_id", "판매채널계정_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_판매상품_입고상품_id_판매sku",
                table: "판매상품",
                columns: new[] { "입고상품_id", "판매sku" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_판매채널계정_user_id_채널종류_상점명",
                table: "판매채널계정",
                columns: new[] { "user_id", "채널종류", "상점명" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "기사운행탐색");

            migrationBuilder.DropTable(
                name: "기사화주인연집계");

            migrationBuilder.DropTable(
                name: "운송의뢰상품연결");

            migrationBuilder.DropTable(
                name: "운행탐색대상화주");

            migrationBuilder.DropTable(
                name: "운행탐색응답요약");

            migrationBuilder.DropTable(
                name: "입고상품");

            migrationBuilder.DropTable(
                name: "입고요청");

            migrationBuilder.DropTable(
                name: "재고이력");

            migrationBuilder.DropTable(
                name: "창고");

            migrationBuilder.DropTable(
                name: "창고사용자");

            migrationBuilder.DropTable(
                name: "채널출품");

            migrationBuilder.DropTable(
                name: "판매상품");

            migrationBuilder.DropTable(
                name: "판매채널계정");
        }
    }
}
