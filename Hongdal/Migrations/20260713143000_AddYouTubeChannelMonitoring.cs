using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hongdal.Migrations
{
    /// <inheritdoc />
    public partial class AddYouTubeChannelMonitoring : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "youtube_watched_channels",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    channel_id = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    channel_name = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    uploads_playlist_id = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    thumbnail_url = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    initial_sync_completed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    last_synced_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    latest_video_id = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    latest_video_published_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_youtube_watched_channels", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "youtube_channel_videos",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    watched_channel_id = table.Column<long>(type: "bigint", nullable: false),
                    video_id = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    channel_id = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    title = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "text", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    published_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    thumbnail_url = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_new_upload = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    sharing_status = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    first_detected_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_youtube_channel_videos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_youtube_channel_videos_youtube_watched_channels_watched_chan~",
                        column: x => x.watched_channel_id,
                        principalTable: "youtube_watched_channels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_youtube_channel_videos_channel_id_published_at_utc",
                table: "youtube_channel_videos",
                columns: new[] { "channel_id", "published_at_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_youtube_channel_videos_is_new_upload_sharing_status_first_de~",
                table: "youtube_channel_videos",
                columns: new[] { "is_new_upload", "sharing_status", "first_detected_at_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_youtube_channel_videos_video_id",
                table: "youtube_channel_videos",
                column: "video_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_youtube_channel_videos_watched_channel_id",
                table: "youtube_channel_videos",
                column: "watched_channel_id");

            migrationBuilder.CreateIndex(
                name: "IX_youtube_watched_channels_channel_id",
                table: "youtube_watched_channels",
                column: "channel_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_youtube_watched_channels_is_active_last_synced_at_utc",
                table: "youtube_watched_channels",
                columns: new[] { "is_active", "last_synced_at_utc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "youtube_channel_videos");

            migrationBuilder.DropTable(
                name: "youtube_watched_channels");
        }
    }
}
