using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using 살뜰.Data;

#nullable disable

namespace Ssalddel.Migrations;

[DbContext(typeof(SsalddelContext))]
[Migration("20260720150000_AddRestaurantPublicCatalog")]
public sealed class AddRestaurantPublicCatalog : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "음식점공개프로필",
            columns: table => new
            {
                id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                업체_id = table.Column<long>(type: "bigint", nullable: true),
                상호명 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                카테고리 = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                소개 = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                공개주소 = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                위도 = table.Column<decimal>(type: "decimal(18,10)", nullable: false),
                경도 = table.Column<decimal>(type: "decimal(18,10)", nullable: false),
                대표이미지_url = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                최소주문금액 = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                예상조리분 = table.Column<int>(type: "int", nullable: false),
                공개여부 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                주문가능여부 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                created_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                updated_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_음식점공개프로필", item => item.id);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "음식점메뉴",
            columns: table => new
            {
                id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                음식점공개프로필_id = table.Column<long>(type: "bigint", nullable: false),
                메뉴명 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                설명 = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                판매가 = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                대표이미지_url = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                공개여부 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                품절여부 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                표시순서 = table.Column<int>(type: "int", nullable: false),
                created_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                updated_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_음식점메뉴", item => item.id);
                table.ForeignKey(
                    name: "FK_음식점메뉴_음식점공개프로필_음식점공개프로필_id",
                    column: item => item.음식점공개프로필_id,
                    principalTable: "음식점공개프로필",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateIndex(
            name: "IX_음식점공개프로필_공개여부_주문가능여부_updated_at_utc",
            table: "음식점공개프로필",
            columns: ["공개여부", "주문가능여부", "updated_at_utc"]);
        migrationBuilder.CreateIndex(
            name: "IX_음식점공개프로필_업체_id",
            table: "음식점공개프로필",
            column: "업체_id",
            unique: true);
        migrationBuilder.CreateIndex(
            name: "IX_음식점공개프로필_위도_경도",
            table: "음식점공개프로필",
            columns: ["위도", "경도"]);
        migrationBuilder.CreateIndex(
            name: "IX_음식점메뉴_음식점공개프로필_id_공개여부_표시순서",
            table: "음식점메뉴",
            columns: ["음식점공개프로필_id", "공개여부", "표시순서"]);
        migrationBuilder.CreateIndex(
            name: "IX_음식점메뉴_음식점공개프로필_id_메뉴명",
            table: "음식점메뉴",
            columns: ["음식점공개프로필_id", "메뉴명"],
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "음식점메뉴");
        migrationBuilder.DropTable(name: "음식점공개프로필");
    }
}
