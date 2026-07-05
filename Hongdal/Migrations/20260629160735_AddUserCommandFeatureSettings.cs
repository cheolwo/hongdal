using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hongdal.Migrations
{
    /// <inheritdoc />
    public partial class AddUserCommandFeatureSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "cargo_height_mm",
                table: "shipper_requests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "cargo_length_mm",
                table: "shipper_requests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "cargo_pallet_count",
                table: "shipper_requests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "cargo_width_mm",
                table: "shipper_requests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "cash_receipt_required",
                table: "shipper_requests",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "cash_settled_at",
                table: "shipper_requests",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "cash_settlement_memo",
                table: "shipper_requests",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "collector",
                table: "shipper_requests",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "estimated_payment_amount",
                table: "shipper_requests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "evidence_method",
                table: "shipper_requests",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "payment_method",
                table: "shipper_requests",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "receipt_issued_at",
                table: "shipper_requests",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "receipt_number",
                table: "shipper_requests",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "settlement_memo",
                table: "shipper_requests",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "settlement_status",
                table: "shipper_requests",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "settlement_time",
                table: "shipper_requests",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "tax_invoice_required",
                table: "shipper_requests",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "vehicle_type",
                table: "shipper_requests",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "사용자_Command_기능설정",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    command_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    feature_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_enabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_사용자_Command_기능설정", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "차량제원",
                columns: table => new
                {
                    차량코드 = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    차량명 = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    제조사 = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    모델명 = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    차급 = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    차체형태 = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    적재함길이Mm = table.Column<int>(type: "int", nullable: false),
                    적재함폭Mm = table.Column<int>(type: "int", nullable: false),
                    적재함높이Mm = table.Column<int>(type: "int", nullable: true),
                    최대적재중량Kg = table.Column<int>(type: "int", nullable: false),
                    운영권장중량Kg = table.Column<int>(type: "int", nullable: true),
                    차량전체높이Mm = table.Column<int>(type: "int", nullable: true),
                    바닥높이Mm = table.Column<int>(type: "int", nullable: true),
                    비눈보호가능 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    냉장가능 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    냉동가능 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    측면상하차가능 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    리프트가능 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    장재물유리 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    팔레트적재개수 = table.Column<int>(type: "int", nullable: true),
                    기준연비KmPerLiter = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    장점메모 = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    단점메모 = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_차량제원", x => x.차량코드);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "화물요구조건",
                columns: table => new
                {
                    의뢰Id = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    화물길이Mm = table.Column<int>(type: "int", nullable: true),
                    화물폭Mm = table.Column<int>(type: "int", nullable: true),
                    화물높이Mm = table.Column<int>(type: "int", nullable: true),
                    화물무게Kg = table.Column<int>(type: "int", nullable: true),
                    팔레트개수 = table.Column<int>(type: "int", nullable: true),
                    비맞으면안됨 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    냉장필요 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    냉동필요 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    리프트필요 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    측면상하차필요 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    장재물 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    혼적허용 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    독차필수 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    주의사항 = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_화물요구조건", x => x.의뢰Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_사용자_Command_기능설정_user_id_command_name_feature_name",
                table: "사용자_Command_기능설정",
                columns: new[] { "user_id", "command_name", "feature_name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "사용자_Command_기능설정");

            migrationBuilder.DropTable(
                name: "차량제원");

            migrationBuilder.DropTable(
                name: "화물요구조건");

            migrationBuilder.DropColumn(
                name: "cargo_height_mm",
                table: "shipper_requests");

            migrationBuilder.DropColumn(
                name: "cargo_length_mm",
                table: "shipper_requests");

            migrationBuilder.DropColumn(
                name: "cargo_pallet_count",
                table: "shipper_requests");

            migrationBuilder.DropColumn(
                name: "cargo_width_mm",
                table: "shipper_requests");

            migrationBuilder.DropColumn(
                name: "cash_receipt_required",
                table: "shipper_requests");

            migrationBuilder.DropColumn(
                name: "cash_settled_at",
                table: "shipper_requests");

            migrationBuilder.DropColumn(
                name: "cash_settlement_memo",
                table: "shipper_requests");

            migrationBuilder.DropColumn(
                name: "collector",
                table: "shipper_requests");

            migrationBuilder.DropColumn(
                name: "estimated_payment_amount",
                table: "shipper_requests");

            migrationBuilder.DropColumn(
                name: "evidence_method",
                table: "shipper_requests");

            migrationBuilder.DropColumn(
                name: "payment_method",
                table: "shipper_requests");

            migrationBuilder.DropColumn(
                name: "receipt_issued_at",
                table: "shipper_requests");

            migrationBuilder.DropColumn(
                name: "receipt_number",
                table: "shipper_requests");

            migrationBuilder.DropColumn(
                name: "settlement_memo",
                table: "shipper_requests");

            migrationBuilder.DropColumn(
                name: "settlement_status",
                table: "shipper_requests");

            migrationBuilder.DropColumn(
                name: "settlement_time",
                table: "shipper_requests");

            migrationBuilder.DropColumn(
                name: "tax_invoice_required",
                table: "shipper_requests");

            migrationBuilder.DropColumn(
                name: "vehicle_type",
                table: "shipper_requests");
        }
    }
}
