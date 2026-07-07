using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hongdal.Migrations
{
    /// <inheritdoc />
    public partial class AddPlatformCommunityPostTags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RoleTag",
                table: "platform_community_posts",
                type: "varchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "WorkflowTag",
                table: "platform_community_posts",
                type: "varchar(60)",
                maxLength: 60,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_platform_community_posts_WorkflowTag_RoleTag_IsDeleted_Creat~",
                table: "platform_community_posts",
                columns: new[] { "WorkflowTag", "RoleTag", "IsDeleted", "CreatedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_platform_community_posts_WorkflowTag_RoleTag_IsDeleted_Creat~",
                table: "platform_community_posts");

            migrationBuilder.DropColumn(
                name: "RoleTag",
                table: "platform_community_posts");

            migrationBuilder.DropColumn(
                name: "WorkflowTag",
                table: "platform_community_posts");
        }
    }
}
