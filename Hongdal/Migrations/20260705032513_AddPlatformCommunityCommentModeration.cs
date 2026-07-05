using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hongdal.Migrations
{
    /// <inheritdoc />
    public partial class AddPlatformCommunityCommentModeration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_platform_community_post_comments_PostId_IsDeleted_CreatedAtU~",
                table: "platform_community_post_comments");

            migrationBuilder.DropIndex(
                name: "IX_platform_community_post_attachment_comments_AttachmentId_IsD~",
                table: "platform_community_post_attachment_comments");

            migrationBuilder.AddColumn<bool>(
                name: "IsOperatorHidden",
                table: "platform_community_post_comments",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ReportCount",
                table: "platform_community_post_comments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsOperatorHidden",
                table: "platform_community_post_attachment_comments",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ReportCount",
                table: "platform_community_post_attachment_comments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_platform_community_post_comments_PostId_IsDeleted_IsOperator~",
                table: "platform_community_post_comments",
                columns: new[] { "PostId", "IsDeleted", "IsOperatorHidden", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_platform_community_post_attachment_comments_AttachmentId_IsD~",
                table: "platform_community_post_attachment_comments",
                columns: new[] { "AttachmentId", "IsDeleted", "IsOperatorHidden", "CreatedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_platform_community_post_comments_PostId_IsDeleted_IsOperator~",
                table: "platform_community_post_comments");

            migrationBuilder.DropIndex(
                name: "IX_platform_community_post_attachment_comments_AttachmentId_IsD~",
                table: "platform_community_post_attachment_comments");

            migrationBuilder.DropColumn(
                name: "IsOperatorHidden",
                table: "platform_community_post_comments");

            migrationBuilder.DropColumn(
                name: "ReportCount",
                table: "platform_community_post_comments");

            migrationBuilder.DropColumn(
                name: "IsOperatorHidden",
                table: "platform_community_post_attachment_comments");

            migrationBuilder.DropColumn(
                name: "ReportCount",
                table: "platform_community_post_attachment_comments");

            migrationBuilder.CreateIndex(
                name: "IX_platform_community_post_comments_PostId_IsDeleted_CreatedAtU~",
                table: "platform_community_post_comments",
                columns: new[] { "PostId", "IsDeleted", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_platform_community_post_attachment_comments_AttachmentId_IsD~",
                table: "platform_community_post_attachment_comments",
                columns: new[] { "AttachmentId", "IsDeleted", "CreatedAtUtc" });
        }
    }
}
