using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using 살뜰.Data;

#nullable disable

namespace Ssalddel.Migrations;

[DbContext(typeof(SsalddelContext))]
[Migration("20260725143000_AddCommunityCommentDisplayCountry")]
public sealed class AddCommunityCommentDisplayCountry : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        AddCountryColumns(migrationBuilder, "platform_community_post_comments");
        AddCountryColumns(migrationBuilder, "platform_community_post_attachment_comments");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        DropCountryColumns(migrationBuilder, "platform_community_post_comments");
        DropCountryColumns(migrationBuilder, "platform_community_post_attachment_comments");
    }

    private static void AddCountryColumns(MigrationBuilder migrationBuilder, string table)
    {
        migrationBuilder.AddColumn<string>(
                name: "AuthorDisplayCountryCode",
                table: table,
                type: "varchar(2)",
                maxLength: 2,
                nullable: true)
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.AddColumn<bool>(
            name: "IsAuthorDisplayCountryPublic",
            table: table,
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);
    }

    private static void DropCountryColumns(MigrationBuilder migrationBuilder, string table)
    {
        migrationBuilder.DropColumn(
            name: "AuthorDisplayCountryCode",
            table: table);
        migrationBuilder.DropColumn(
            name: "IsAuthorDisplayCountryPublic",
            table: table);
    }
}
