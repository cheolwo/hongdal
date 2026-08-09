using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssalddel.Infrastructure.Persistence.PublicData.Migrations
{
    /// <inheritdoc />
    public partial class AddExternalPublicDataIngestionFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "public_data_ingestion_runs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RunKey = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SourceId = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DatasetId = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StatusCode = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StartedAtUtc = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    CompletedAtUtc = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    AttemptCount = table.Column<int>(type: "int", nullable: false),
                    FetchedCount = table.Column<int>(type: "int", nullable: false),
                    NormalizedCount = table.Column<int>(type: "int", nullable: false),
                    RejectedCount = table.Column<int>(type: "int", nullable: false),
                    InsertedCount = table.Column<int>(type: "int", nullable: false),
                    UpdatedCount = table.Column<int>(type: "int", nullable: false),
                    ExistingCount = table.Column<int>(type: "int", nullable: false),
                    SourceVersion = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataRevision = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ErrorCode = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ErrorSummary = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_public_data_ingestion_runs", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "public_data_region_mappings",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SourceId = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ExternalRegionCode = table.Column<string>(type: "varchar(240)", maxLength: 240, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RegionStableId = table.Column<string>(type: "varchar(240)", maxLength: 240, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SpatialPrecisionCode = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MappingRevision = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ValidFromUtc = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    ValidToUtc = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_public_data_region_mappings", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "public_data_raw_snapshots",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FirstCollectionRunId = table.Column<long>(type: "bigint", nullable: false),
                    SourceId = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DatasetId = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SourceVersion = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CollectedAtUtc = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    EvidenceAsOfUtc = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    ContentHashSha256 = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ContentLength = table.Column<long>(type: "bigint", nullable: false),
                    ContentType = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OriginalFileName = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StorageContainer = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StorageObjectName = table.Column<string>(type: "varchar(1024)", maxLength: 1024, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StorageLocation = table.Column<string>(type: "varchar(2048)", maxLength: 2048, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FirstSeenAtUtc = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    LastSeenAtUtc = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_public_data_raw_snapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_public_data_raw_snapshots_public_data_ingestion_runs_FirstCo~",
                        column: x => x.FirstCollectionRunId,
                        principalTable: "public_data_ingestion_runs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "public_data_normalized_records",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RawSnapshotId = table.Column<long>(type: "bigint", nullable: false),
                    RecordKey = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StableId = table.Column<string>(type: "varchar(240)", maxLength: 240, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SourceId = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DatasetId = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RegionStableId = table.Column<string>(type: "varchar(240)", maxLength: 240, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MetricCode = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NumericValue = table.Column<decimal>(type: "decimal(28,10)", precision: 28, scale: 10, nullable: true),
                    TextValue = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UnitCode = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EvidenceAsOfUtc = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    CollectedAtUtc = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    SpatialPrecisionCode = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    QualityCode = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LimitationCode = table.Column<string>(type: "varchar(240)", maxLength: 240, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DimensionKey = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SourceVersion = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataRevision = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FirstSeenAtUtc = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    LastSeenAtUtc = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_public_data_normalized_records", x => x.Id);
                    table.ForeignKey(
                        name: "FK_public_data_normalized_records_public_data_raw_snapshots_Raw~",
                        column: x => x.RawSnapshotId,
                        principalTable: "public_data_raw_snapshots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_public_data_ingestion_runs_RunKey",
                table: "public_data_ingestion_runs",
                column: "RunKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_public_data_ingestion_runs_SourceId_DatasetId_StartedAtUtc",
                table: "public_data_ingestion_runs",
                columns: new[] { "SourceId", "DatasetId", "StartedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_public_data_normalized_records_RawSnapshotId",
                table: "public_data_normalized_records",
                column: "RawSnapshotId");

            migrationBuilder.CreateIndex(
                name: "IX_public_data_normalized_records_RecordKey",
                table: "public_data_normalized_records",
                column: "RecordKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_public_data_normalized_records_RegionStableId_MetricCode_Evi~",
                table: "public_data_normalized_records",
                columns: new[] { "RegionStableId", "MetricCode", "EvidenceAsOfUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_public_data_raw_snapshots_FirstCollectionRunId",
                table: "public_data_raw_snapshots",
                column: "FirstCollectionRunId");

            migrationBuilder.CreateIndex(
                name: "IX_public_data_raw_snapshots_SourceId_DatasetId_ContentHashSha2~",
                table: "public_data_raw_snapshots",
                columns: new[] { "SourceId", "DatasetId", "ContentHashSha256" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_public_data_region_mappings_SourceId_ExternalRegionCode",
                table: "public_data_region_mappings",
                columns: new[] { "SourceId", "ExternalRegionCode" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "public_data_normalized_records");

            migrationBuilder.DropTable(
                name: "public_data_region_mappings");

            migrationBuilder.DropTable(
                name: "public_data_raw_snapshots");

            migrationBuilder.DropTable(
                name: "public_data_ingestion_runs");
        }
    }
}
