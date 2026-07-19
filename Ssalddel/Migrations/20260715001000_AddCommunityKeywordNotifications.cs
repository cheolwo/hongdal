using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;
using 살뜰.Data;

#nullable disable

namespace Ssalddel.Migrations;

[DbContext(typeof(SsalddelContext))]
[Migration("20260715001000_AddCommunityKeywordNotifications")]
public sealed class AddCommunityKeywordNotifications : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "AuthorUserId",
            table: "platform_community_posts",
            type: "varchar(450)",
            maxLength: 450,
            nullable: true)
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "community_keyword_subscriptions",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                app_key = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                keyword = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                normalized_keyword = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                is_active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                created_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                updated_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_community_keyword_subscriptions", x => x.Id);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "platform_community_post_keyword_scans",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                post_id = table.Column<long>(type: "bigint", nullable: false),
                status = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                attempt_count = table.Column<int>(type: "int", nullable: false),
                processing_token = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                last_error = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                next_attempt_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                created_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                updated_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                completed_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_platform_community_post_keyword_scans", x => x.Id);
                table.ForeignKey(
                    name: "FK_community_keyword_scan_post",
                    column: x => x.post_id,
                    principalTable: "platform_community_posts",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "community_keyword_notifications",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                post_id = table.Column<long>(type: "bigint", nullable: false),
                post_app_key = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                post_category = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                post_title = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                post_excerpt = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                post_author_nickname = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                matched_keywords_json = table.Column<string>(type: "varchar(4096)", maxLength: 4096, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                is_read = table.Column<bool>(type: "tinyint(1)", nullable: false),
                read_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                created_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                updated_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_community_keyword_notifications", x => x.Id);
                table.ForeignKey(
                    name: "FK_community_keyword_notification_post",
                    column: x => x.post_id,
                    principalTable: "platform_community_posts",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "community_keyword_notification_deliveries",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                notification_id = table.Column<long>(type: "bigint", nullable: false),
                installation_id = table.Column<long>(type: "bigint", nullable: false),
                status = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                attempt_count = table.Column<int>(type: "int", nullable: false),
                processing_token = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                last_error = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                next_attempt_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                sent_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                created_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                updated_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_community_keyword_notification_deliveries", x => x.Id);
                table.ForeignKey(
                    name: "FK_community_keyword_delivery_installation",
                    column: x => x.installation_id,
                    principalTable: "ssalddel_mobile_push_installations",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_community_keyword_delivery_notification",
                    column: x => x.notification_id,
                    principalTable: "community_keyword_notifications",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateIndex(
            name: "IX_platform_community_posts_AuthorUserId",
            table: "platform_community_posts",
            column: "AuthorUserId");

        migrationBuilder.CreateIndex(
            name: "IX_community_keyword_subscription_match",
            table: "community_keyword_subscriptions",
            columns: new[] { "app_key", "is_active" });

        migrationBuilder.CreateIndex(
            name: "UX_community_keyword_subscription",
            table: "community_keyword_subscriptions",
            columns: new[] { "user_id", "app_key", "normalized_keyword" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_community_keyword_scan_due",
            table: "platform_community_post_keyword_scans",
            columns: new[] { "status", "next_attempt_at_utc" });

        migrationBuilder.CreateIndex(
            name: "UX_community_keyword_scan_post",
            table: "platform_community_post_keyword_scans",
            column: "post_id",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_community_keyword_notifications_post_id",
            table: "community_keyword_notifications",
            column: "post_id");

        migrationBuilder.CreateIndex(
            name: "IX_community_keyword_notification_inbox",
            table: "community_keyword_notifications",
            columns: new[] { "user_id", "is_read", "created_at_utc" });

        migrationBuilder.CreateIndex(
            name: "UX_community_keyword_notification_user_post",
            table: "community_keyword_notifications",
            columns: new[] { "user_id", "post_id" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_community_keyword_delivery_due",
            table: "community_keyword_notification_deliveries",
            columns: new[] { "status", "next_attempt_at_utc" });

        migrationBuilder.CreateIndex(
            name: "IX_community_keyword_delivery_installation",
            table: "community_keyword_notification_deliveries",
            column: "installation_id");

        migrationBuilder.CreateIndex(
            name: "UX_community_keyword_delivery_target",
            table: "community_keyword_notification_deliveries",
            columns: new[] { "notification_id", "installation_id" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "community_keyword_notification_deliveries");
        migrationBuilder.DropTable(name: "community_keyword_subscriptions");
        migrationBuilder.DropTable(name: "platform_community_post_keyword_scans");
        migrationBuilder.DropTable(name: "community_keyword_notifications");

        migrationBuilder.DropIndex(
            name: "IX_platform_community_posts_AuthorUserId",
            table: "platform_community_posts");

        migrationBuilder.DropColumn(
            name: "AuthorUserId",
            table: "platform_community_posts");
    }
}
