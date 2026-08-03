using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssalddel.Infrastructure.Persistence.AgriculturalFisheries.Migrations
{
    /// <inheritdoc />
    public partial class AddMofFishingAreaCatalogSnapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "agri_mof_fishing_area_snapshots",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SourceKey = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SourceUrl = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DatasetVersion = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ContentSha256 = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CollectedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    StoredAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    LastSeenAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    SourceRowCount = table.Column<int>(type: "int", nullable: false),
                    NormalizedRecordCount = table.Column<int>(type: "int", nullable: false),
                    FreshnessCode = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NormalizedRecordsJson = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agri_mof_fishing_area_snapshots", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "agri_mof_fishing_area_collection_runs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RunKey = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SourceKey = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DatasetVersion = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StatusCode = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ContentSha256 = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SourceRowCount = table.Column<int>(type: "int", nullable: false),
                    ErrorMessage = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StartedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    SnapshotId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agri_mof_fishing_area_collection_runs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_agri_mof_fishing_area_collection_runs_agri_mof_fishing_area_~",
                        column: x => x.SnapshotId,
                        principalTable: "agri_mof_fishing_area_snapshots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_agri_mof_fishing_area_collection_runs_RunKey",
                table: "agri_mof_fishing_area_collection_runs",
                column: "RunKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_agri_mof_fishing_area_collection_runs_SnapshotId",
                table: "agri_mof_fishing_area_collection_runs",
                column: "SnapshotId");

            migrationBuilder.CreateIndex(
                name: "IX_agri_mof_fishing_area_collection_runs_SourceKey_StatusCode_C~",
                table: "agri_mof_fishing_area_collection_runs",
                columns: new[] { "SourceKey", "StatusCode", "CompletedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_agri_mof_fishing_area_snapshots_SourceKey_DatasetVersion_Con~",
                table: "agri_mof_fishing_area_snapshots",
                columns: new[] { "SourceKey", "DatasetVersion", "ContentSha256" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_agri_mof_fishing_area_snapshots_SourceKey_LastSeenAtUtc",
                table: "agri_mof_fishing_area_snapshots",
                columns: new[] { "SourceKey", "LastSeenAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "agri_mof_fishing_area_collection_runs");

            migrationBuilder.DropTable(
                name: "agri_mof_fishing_area_snapshots");
        }
    }
}
