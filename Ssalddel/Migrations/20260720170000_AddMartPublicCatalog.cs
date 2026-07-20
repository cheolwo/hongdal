using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssalddel.Migrations
{
    /// <inheritdoc />
    public partial class AddMartPublicCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "마트공개상품",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    판매상품_id = table.Column<long>(type: "bigint", nullable: true),
                    상품명 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    카테고리 = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    짧은설명 = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    설명 = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    판매단위 = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    판매가 = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    대표이미지_url = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    판매가능수량 = table.Column<int>(type: "int", nullable: false),
                    공개여부 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    판매허용여부 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    재고기준시각_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_마트공개상품", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_마트공개상품_공개여부_판매허용여부_updated_at_utc",
                table: "마트공개상품",
                columns: new[] { "공개여부", "판매허용여부", "updated_at_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_마트공개상품_카테고리_상품명",
                table: "마트공개상품",
                columns: new[] { "카테고리", "상품명" });

            migrationBuilder.CreateIndex(
                name: "IX_마트공개상품_판매상품_id",
                table: "마트공개상품",
                column: "판매상품_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "마트공개상품");
        }
    }
}
