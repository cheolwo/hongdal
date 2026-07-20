using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssalddel.Migrations
{
    /// <inheritdoc />
    public partial class AddUnplannedInboundRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "보관조건",
                table: "입고요청",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "예정_sku",
                table: "입고요청",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "예정상품명",
                table: "입고요청",
                type: "varchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "예정수량",
                table: "입고요청",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "입고묶음바코드",
                table: "입고요청",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "현장입고_안내_version",
                table: "입고요청",
                type: "varchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<Guid>(
                name: "현장입고_클라이언트_요청_id",
                table: "입고요청",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<string>(
                name: "현장입고사유",
                table: "입고요청",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_입고요청_주문자_user_id_현장입고_클라이언트_요청_id",
                table: "입고요청",
                columns: new[] { "주문자_user_id", "현장입고_클라이언트_요청_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_입고요청_창고_id_예정_sku_상태",
                table: "입고요청",
                columns: new[] { "창고_id", "예정_sku", "상태" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_입고요청_주문자_user_id_현장입고_클라이언트_요청_id",
                table: "입고요청");

            migrationBuilder.DropIndex(
                name: "IX_입고요청_창고_id_예정_sku_상태",
                table: "입고요청");

            migrationBuilder.DropColumn(
                name: "보관조건",
                table: "입고요청");

            migrationBuilder.DropColumn(
                name: "예정_sku",
                table: "입고요청");

            migrationBuilder.DropColumn(
                name: "예정상품명",
                table: "입고요청");

            migrationBuilder.DropColumn(
                name: "예정수량",
                table: "입고요청");

            migrationBuilder.DropColumn(
                name: "입고묶음바코드",
                table: "입고요청");

            migrationBuilder.DropColumn(
                name: "현장입고_안내_version",
                table: "입고요청");

            migrationBuilder.DropColumn(
                name: "현장입고_클라이언트_요청_id",
                table: "입고요청");

            migrationBuilder.DropColumn(
                name: "현장입고사유",
                table: "입고요청");
        }
    }
}
