using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssalddel.Migrations
{
    /// <inheritdoc />
    public partial class AddPlatformCommunityReportSubjects : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsReportBoardPost",
                table: "platform_community_posts",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ReportedDisplayName",
                table: "platform_community_posts",
                type: "varchar(40)",
                maxLength: 40,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ReporterDisplayName",
                table: "platform_community_posts",
                type: "varchar(40)",
                maxLength: 40,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_platform_community_posts_IsReportBoardPost_IsDeleted_Created~",
                table: "platform_community_posts",
                columns: new[] { "IsReportBoardPost", "IsDeleted", "CreatedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_platform_community_posts_IsReportBoardPost_IsDeleted_Created~",
                table: "platform_community_posts");

            migrationBuilder.DropColumn(
                name: "IsReportBoardPost",
                table: "platform_community_posts");

            migrationBuilder.DropColumn(
                name: "ReportedDisplayName",
                table: "platform_community_posts");

            migrationBuilder.DropColumn(
                name: "ReporterDisplayName",
                table: "platform_community_posts");
        }
    }
}
