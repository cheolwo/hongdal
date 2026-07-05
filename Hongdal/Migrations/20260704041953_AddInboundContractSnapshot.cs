using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hongdal.Migrations
{
    /// <inheritdoc />
    public partial class AddInboundContractSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "계약메모",
                table: "입고요청",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "계약번호",
                table: "입고요청",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "계약상대방명",
                table: "입고요청",
                type: "varchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "계약시작일",
                table: "입고요청",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "계약유형",
                table: "입고요청",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "StorageOnly")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "계약종료일",
                table: "입고요청",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "보관료일단가",
                table: "입고요청",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "정산방식",
                table: "입고요청",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "통관필요여부",
                table: "입고요청",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "판매수수료율",
                table: "입고요청",
                type: "decimal(9,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "계약메모",
                table: "입고상품",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "계약번호",
                table: "입고상품",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "계약상대방명",
                table: "입고상품",
                type: "varchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "계약시작일",
                table: "입고상품",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "계약유형",
                table: "입고상품",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "StorageOnly")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "계약종료일",
                table: "입고상품",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "보관료일단가",
                table: "입고상품",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "정산방식",
                table: "입고상품",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "통관필요여부",
                table: "입고상품",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "판매수수료율",
                table: "입고상품",
                type: "decimal(9,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "계약메모",
                table: "입고요청");

            migrationBuilder.DropColumn(
                name: "계약번호",
                table: "입고요청");

            migrationBuilder.DropColumn(
                name: "계약상대방명",
                table: "입고요청");

            migrationBuilder.DropColumn(
                name: "계약시작일",
                table: "입고요청");

            migrationBuilder.DropColumn(
                name: "계약유형",
                table: "입고요청");

            migrationBuilder.DropColumn(
                name: "계약종료일",
                table: "입고요청");

            migrationBuilder.DropColumn(
                name: "보관료일단가",
                table: "입고요청");

            migrationBuilder.DropColumn(
                name: "정산방식",
                table: "입고요청");

            migrationBuilder.DropColumn(
                name: "통관필요여부",
                table: "입고요청");

            migrationBuilder.DropColumn(
                name: "판매수수료율",
                table: "입고요청");

            migrationBuilder.DropColumn(
                name: "계약메모",
                table: "입고상품");

            migrationBuilder.DropColumn(
                name: "계약번호",
                table: "입고상품");

            migrationBuilder.DropColumn(
                name: "계약상대방명",
                table: "입고상품");

            migrationBuilder.DropColumn(
                name: "계약시작일",
                table: "입고상품");

            migrationBuilder.DropColumn(
                name: "계약유형",
                table: "입고상품");

            migrationBuilder.DropColumn(
                name: "계약종료일",
                table: "입고상품");

            migrationBuilder.DropColumn(
                name: "보관료일단가",
                table: "입고상품");

            migrationBuilder.DropColumn(
                name: "정산방식",
                table: "입고상품");

            migrationBuilder.DropColumn(
                name: "통관필요여부",
                table: "입고상품");

            migrationBuilder.DropColumn(
                name: "판매수수료율",
                table: "입고상품");
        }
    }
}
