using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssalddel.Migrations
{
    /// <inheritdoc />
    public partial class AddCommunityPostSourceEvidence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SourceDatasetCode",
                table: "platform_community_posts",
                type: "varchar(80)",
                maxLength: 80,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "SourceEvidenceJson",
                table: "platform_community_posts",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "SourceObservationStableId",
                table: "platform_community_posts",
                type: "varchar(200)",
                maxLength: 200,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "SourceSnapshotRevision",
                table: "platform_community_posts",
                type: "varchar(128)",
                maxLength: 128,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_platform_community_posts_source_observation",
                table: "platform_community_posts",
                columns: new[] { "SourceDatasetCode", "SourceObservationStableId", "IsDeleted" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_platform_community_posts_source_observation",
                table: "platform_community_posts");

            migrationBuilder.DropColumn(
                name: "SourceDatasetCode",
                table: "platform_community_posts");

            migrationBuilder.DropColumn(
                name: "SourceEvidenceJson",
                table: "platform_community_posts");

            migrationBuilder.DropColumn(
                name: "SourceObservationStableId",
                table: "platform_community_posts");

            migrationBuilder.DropColumn(
                name: "SourceSnapshotRevision",
                table: "platform_community_posts");
        }
    }
}
