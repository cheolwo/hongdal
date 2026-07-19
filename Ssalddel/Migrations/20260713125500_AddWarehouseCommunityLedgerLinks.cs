using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using 살뜰.Data;

#nullable disable

namespace Ssalddel.Migrations
{
    [DbContext(typeof(SsalddelContext))]
    [Migration("20260713125500_AddWarehouseCommunityLedgerLinks")]
    public partial class AddWarehouseCommunityLedgerLinks : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            AddLedgerColumns(migrationBuilder, "입고요청");
            AddLedgerColumns(migrationBuilder, "입고상품");
            AddLedgerColumns(migrationBuilder, "출고예정");
            AddLedgerColumns(migrationBuilder, "출고묶음");

            migrationBuilder.CreateIndex(
                name: "IX_입고요청_community_ledger_id",
                table: "입고요청",
                column: "community_ledger_id");

            migrationBuilder.CreateIndex(
                name: "IX_입고상품_community_ledger_id",
                table: "입고상품",
                column: "community_ledger_id");

            migrationBuilder.CreateIndex(
                name: "IX_출고예정_community_ledger_id",
                table: "출고예정",
                column: "community_ledger_id");

            migrationBuilder.CreateIndex(
                name: "IX_출고묶음_community_ledger_id",
                table: "출고묶음",
                column: "community_ledger_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_입고요청_community_ledger_id",
                table: "입고요청");

            migrationBuilder.DropIndex(
                name: "IX_입고상품_community_ledger_id",
                table: "입고상품");

            migrationBuilder.DropIndex(
                name: "IX_출고예정_community_ledger_id",
                table: "출고예정");

            migrationBuilder.DropIndex(
                name: "IX_출고묶음_community_ledger_id",
                table: "출고묶음");

            DropLedgerColumns(migrationBuilder, "입고요청");
            DropLedgerColumns(migrationBuilder, "입고상품");
            DropLedgerColumns(migrationBuilder, "출고예정");
            DropLedgerColumns(migrationBuilder, "출고묶음");
        }

        private static void AddLedgerColumns(MigrationBuilder migrationBuilder, string table)
        {
            migrationBuilder.AddColumn<string>(
                name: "community_ledger_id",
                table: table,
                type: "varchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "community_ledger_template_key",
                table: table,
                type: "varchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "community_ledger_state",
                table: table,
                type: "varchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "community_ledger_synced_at_utc",
                table: table,
                type: "datetime(6)",
                nullable: true);
        }

        private static void DropLedgerColumns(MigrationBuilder migrationBuilder, string table)
        {
            migrationBuilder.DropColumn(
                name: "community_ledger_id",
                table: table);

            migrationBuilder.DropColumn(
                name: "community_ledger_template_key",
                table: table);

            migrationBuilder.DropColumn(
                name: "community_ledger_state",
                table: table);

            migrationBuilder.DropColumn(
                name: "community_ledger_synced_at_utc",
                table: table);
        }
    }
}
