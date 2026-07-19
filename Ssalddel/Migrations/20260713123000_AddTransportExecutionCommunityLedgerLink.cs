using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using 살뜰.Data;

#nullable disable

namespace Ssalddel.Migrations
{
    [DbContext(typeof(SsalddelContext))]
    [Migration("20260713123000_AddTransportExecutionCommunityLedgerLink")]
    public partial class AddTransportExecutionCommunityLedgerLink : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "community_ledger_id",
                table: "운송실행투영",
                type: "varchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "community_ledger_template_key",
                table: "운송실행투영",
                type: "varchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "community_ledger_state",
                table: "운송실행투영",
                type: "varchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "community_ledger_synced_at_utc",
                table: "운송실행투영",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_운송실행투영_community_ledger_id",
                table: "운송실행투영",
                column: "community_ledger_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_운송실행투영_community_ledger_id",
                table: "운송실행투영");

            migrationBuilder.DropColumn(
                name: "community_ledger_id",
                table: "운송실행투영");

            migrationBuilder.DropColumn(
                name: "community_ledger_template_key",
                table: "운송실행투영");

            migrationBuilder.DropColumn(
                name: "community_ledger_state",
                table: "운송실행투영");

            migrationBuilder.DropColumn(
                name: "community_ledger_synced_at_utc",
                table: "운송실행투영");
        }
    }
}
