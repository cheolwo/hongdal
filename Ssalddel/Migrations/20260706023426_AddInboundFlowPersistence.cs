using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssalddel.Migrations
{
    /// <inheritdoc />
    public partial class AddInboundFlowPersistence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "계약선행여부",
                table: "입고요청",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "입고생성경로",
                table: "입고요청",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "계약 DB 기반 등록")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "입고흐름유형",
                table: "입고요청",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "ContractBased")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "자동생성여부",
                table: "입고요청",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql("""
                UPDATE `입고요청`
                SET
                    `입고흐름유형` = CASE
                        WHEN `주문_id` IS NOT NULL THEN 'OrderAutoExpected'
                        WHEN COALESCE(`계약번호`, '') = '' THEN 'Unplanned'
                        ELSE 'ContractBased'
                    END,
                    `입고생성경로` = CASE
                        WHEN `주문_id` IS NOT NULL THEN '주문/구매 흐름 자동 생성'
                        WHEN COALESCE(`계약번호`, '') = '' THEN '창고 관리자 수기 등록'
                        ELSE '계약 DB 기반 등록'
                    END,
                    `계약선행여부` = CASE
                        WHEN `주문_id` IS NOT NULL THEN 0
                        WHEN COALESCE(`계약번호`, '') = '' THEN 0
                        ELSE 1
                    END,
                    `자동생성여부` = CASE
                        WHEN `주문_id` IS NOT NULL THEN 1
                        ELSE 0
                    END
                """);

            migrationBuilder.CreateIndex(
                name: "IX_입고요청_입고흐름유형_자동생성여부",
                table: "입고요청",
                columns: new[] { "입고흐름유형", "자동생성여부" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_입고요청_입고흐름유형_자동생성여부",
                table: "입고요청");

            migrationBuilder.DropColumn(
                name: "계약선행여부",
                table: "입고요청");

            migrationBuilder.DropColumn(
                name: "입고생성경로",
                table: "입고요청");

            migrationBuilder.DropColumn(
                name: "입고흐름유형",
                table: "입고요청");

            migrationBuilder.DropColumn(
                name: "자동생성여부",
                table: "입고요청");
        }
    }
}
