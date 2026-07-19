using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssalddel.Migrations
{
    /// <inheritdoc />
    public partial class AddPlatformCommunityEngagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_platform_community_posts_AppKey_IsDeleted_CreatedAtUtc",
                table: "platform_community_posts");

            migrationBuilder.AddColumn<int>(
                name: "CommentCount",
                table: "platform_community_posts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsOperatorPinned",
                table: "platform_community_posts",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastEngagedAtUtc",
                table: "platform_community_posts",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OperatorPinnedAtUtc",
                table: "platform_community_posts",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RecommendationCount",
                table: "platform_community_posts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SharedLinkUrl",
                table: "platform_community_posts",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "platform_community_post_comments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PostId = table.Column<long>(type: "bigint", nullable: false),
                    Nickname = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Body = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PasswordHash = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platform_community_post_comments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_platform_community_post_comments_platform_community_posts_Po~",
                        column: x => x.PostId,
                        principalTable: "platform_community_posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "platform_community_post_recommendations",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PostId = table.Column<long>(type: "bigint", nullable: false),
                    RecommenderKey = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platform_community_post_recommendations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_platform_community_post_recommendations_platform_community_p~",
                        column: x => x.PostId,
                        principalTable: "platform_community_posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_platform_community_posts_AppKey_IsDeleted_IsOperatorPinned_O~",
                table: "platform_community_posts",
                columns: new[] { "AppKey", "IsDeleted", "IsOperatorPinned", "OperatorPinnedAtUtc", "RecommendationCount", "LastEngagedAtUtc", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_platform_community_post_comments_PostId_IsDeleted_CreatedAtU~",
                table: "platform_community_post_comments",
                columns: new[] { "PostId", "IsDeleted", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_platform_community_post_recommendations_PostId_CreatedAtUtc",
                table: "platform_community_post_recommendations",
                columns: new[] { "PostId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_platform_community_post_recommendations_PostId_RecommenderKey",
                table: "platform_community_post_recommendations",
                columns: new[] { "PostId", "RecommenderKey" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "platform_community_post_comments");

            migrationBuilder.DropTable(
                name: "platform_community_post_recommendations");

            migrationBuilder.DropIndex(
                name: "IX_platform_community_posts_AppKey_IsDeleted_IsOperatorPinned_O~",
                table: "platform_community_posts");

            migrationBuilder.DropColumn(
                name: "CommentCount",
                table: "platform_community_posts");

            migrationBuilder.DropColumn(
                name: "IsOperatorPinned",
                table: "platform_community_posts");

            migrationBuilder.DropColumn(
                name: "LastEngagedAtUtc",
                table: "platform_community_posts");

            migrationBuilder.DropColumn(
                name: "OperatorPinnedAtUtc",
                table: "platform_community_posts");

            migrationBuilder.DropColumn(
                name: "RecommendationCount",
                table: "platform_community_posts");

            migrationBuilder.DropColumn(
                name: "SharedLinkUrl",
                table: "platform_community_posts");

            migrationBuilder.CreateIndex(
                name: "IX_platform_community_posts_AppKey_IsDeleted_CreatedAtUtc",
                table: "platform_community_posts",
                columns: new[] { "AppKey", "IsDeleted", "CreatedAtUtc" });
        }
    }
}
