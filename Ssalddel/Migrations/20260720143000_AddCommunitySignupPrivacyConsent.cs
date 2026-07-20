using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using 살뜰.Data;

#nullable disable

namespace Ssalddel.Migrations;

[DbContext(typeof(SsalddelContext))]
[Migration("20260720143000_AddCommunitySignupPrivacyConsent")]
public sealed class AddCommunitySignupPrivacyConsent : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTime>(
            name: "PrivacyConsentedAtUtc",
            table: "AspNetUsers",
            type: "datetime(6)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "PrivacyConsentVersion",
            table: "AspNetUsers",
            type: "varchar(64)",
            maxLength: 64,
            nullable: true)
            .Annotation("MySql:CharSet", "utf8mb4");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "PrivacyConsentedAtUtc", table: "AspNetUsers");
        migrationBuilder.DropColumn(name: "PrivacyConsentVersion", table: "AspNetUsers");
    }
}
