using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hongdal.Migrations
{
    /// <inheritdoc />
    public partial class AddCommunityPostAudioPipeline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "platform_community_post_audio",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    post_id = table.Column<long>(type: "bigint", nullable: false),
                    status = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    provider = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    voice_id = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    model_version = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    language_code = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    audio_format = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    attempt_count = table.Column<int>(type: "int", nullable: false),
                    processing_token = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
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
                    table.PrimaryKey("PK_platform_community_post_audio", x => x.Id);
                    table.ForeignKey(
                        name: "FK_platform_community_post_audio_platform_community_posts_post_~",
                        column: x => x.post_id,
                        principalTable: "platform_community_posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "platform_community_post_audio_access_logs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    audio_id = table.Column<long>(type: "bigint", nullable: false),
                    post_id = table.Column<long>(type: "bigint", nullable: false),
                    segment_sequence = table.Column<int>(type: "int", nullable: true),
                    access_type = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    requester_user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    trace_id = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    accessed_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platform_community_post_audio_access_logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_platform_community_post_audio_access_logs_platform_community~",
                        column: x => x.audio_id,
                        principalTable: "platform_community_post_audio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "platform_community_post_audio_segments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    audio_id = table.Column<long>(type: "bigint", nullable: false),
                    sequence = table.Column<int>(type: "int", nullable: false),
                    character_count = table.Column<int>(type: "int", nullable: false),
                    bucket_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    object_name = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    content_type = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    file_size_bytes = table.Column<long>(type: "bigint", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platform_community_post_audio_segments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_platform_community_post_audio_segments_platform_community_po~",
                        column: x => x.audio_id,
                        principalTable: "platform_community_post_audio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_platform_community_post_audio_post_id",
                table: "platform_community_post_audio",
                column: "post_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_platform_community_post_audio_processing_token",
                table: "platform_community_post_audio",
                column: "processing_token");

            migrationBuilder.CreateIndex(
                name: "IX_platform_community_post_audio_status_next_attempt_at_utc_upd~",
                table: "platform_community_post_audio",
                columns: new[] { "status", "next_attempt_at_utc", "updated_at_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_platform_community_post_audio_access_logs_audio_id",
                table: "platform_community_post_audio_access_logs",
                column: "audio_id");

            migrationBuilder.CreateIndex(
                name: "IX_platform_community_post_audio_access_logs_post_id_accessed_a~",
                table: "platform_community_post_audio_access_logs",
                columns: new[] { "post_id", "accessed_at_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_platform_community_post_audio_access_logs_requester_user_id_~",
                table: "platform_community_post_audio_access_logs",
                columns: new[] { "requester_user_id", "accessed_at_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_platform_community_post_audio_segments_audio_id_sequence",
                table: "platform_community_post_audio_segments",
                columns: new[] { "audio_id", "sequence" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "platform_community_post_audio_access_logs");

            migrationBuilder.DropTable(
                name: "platform_community_post_audio_segments");

            migrationBuilder.DropTable(
                name: "platform_community_post_audio");
        }
    }
}
