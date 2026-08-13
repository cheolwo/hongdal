using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssalddel.Infrastructure.Persistence.PublicData.Migrations
{
    /// <inheritdoc />
    public partial class AddPublicLicensedBusinessBuildingLedger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NormalizedRoadAddressKey",
                table: "public_building_register_titles",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "public_building_business_aggregates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    BuildingRecordId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    SourceRevision = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TotalBusinessCount = table.Column<int>(type: "int", nullable: false),
                    OpenBusinessCount = table.Column<int>(type: "int", nullable: false),
                    SuspendedBusinessCount = table.Column<int>(type: "int", nullable: false),
                    ClosedBusinessCount = table.Column<int>(type: "int", nullable: false),
                    UnresolvedStatusCount = table.Column<int>(type: "int", nullable: false),
                    EvidenceKindCode = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RuleRevision = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AggregateHashSha256 = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GeneratedAtUtc = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_public_building_business_aggregates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_public_building_business_aggregates_public_building_register~",
                        column: x => x.BuildingRecordId,
                        principalTable: "public_building_register_titles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "public_licensed_business_records",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    SourceId = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SourceDatasetId = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OpenServiceId = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OpenServiceName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ManagementNumber = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BusinessName = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BusinessTypeName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LicenseCategoryName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BusinessStatusCode = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BusinessStatusName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DetailedStatusCode = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DetailedStatusName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LotAddress = table.Column<string>(type: "varchar(600)", maxLength: 600, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RoadAddress = table.Column<string>(type: "varchar(600)", maxLength: 600, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NormalizedRoadAddressKey = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SourceCoordinateX = table.Column<decimal>(type: "decimal(20,8)", precision: 20, scale: 8, nullable: true),
                    SourceCoordinateY = table.Column<decimal>(type: "decimal(20,8)", precision: 20, scale: 8, nullable: true),
                    SourceCoordinateReferenceSystem = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LicenseDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ClosureDate = table.Column<DateOnly>(type: "date", nullable: true),
                    SourceLastModifiedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    SourceRevision = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SourceHashSha256 = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EvidenceSnapshotId = table.Column<long>(type: "bigint", nullable: true),
                    ObservedAtUtc = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_public_licensed_business_records", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "public_business_building_assignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    BusinessRecordId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    BuildingRecordId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    AssignmentStatusCode = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AssignmentMethodCode = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ConfidenceCode = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CandidateBuildingCount = table.Column<int>(type: "int", nullable: false),
                    RuleRevision = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EvaluatedAtUtc = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_public_business_building_assignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_public_business_building_assignments_public_building_registe~",
                        column: x => x.BuildingRecordId,
                        principalTable: "public_building_register_titles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_public_business_building_assignments_public_licensed_busines~",
                        column: x => x.BusinessRecordId,
                        principalTable: "public_licensed_business_records",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_public_building_business_aggregates_BuildingRecordId_SourceR~",
                table: "public_building_business_aggregates",
                columns: new[] { "BuildingRecordId", "SourceRevision", "RuleRevision" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_public_business_building_assignments_BuildingRecordId_Assign~",
                table: "public_business_building_assignments",
                columns: new[] { "BuildingRecordId", "AssignmentStatusCode" });

            migrationBuilder.CreateIndex(
                name: "IX_public_business_building_assignments_BusinessRecordId_RuleRe~",
                table: "public_business_building_assignments",
                columns: new[] { "BusinessRecordId", "RuleRevision" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_public_licensed_business_records_BusinessStatusCode_SourceRe~",
                table: "public_licensed_business_records",
                columns: new[] { "BusinessStatusCode", "SourceRevision" });

            migrationBuilder.CreateIndex(
                name: "IX_public_licensed_business_records_NormalizedRoadAddressKey_So~",
                table: "public_licensed_business_records",
                columns: new[] { "NormalizedRoadAddressKey", "SourceRevision" });

            migrationBuilder.CreateIndex(
                name: "IX_public_licensed_business_records_SourceId_OpenServiceId_Mana~",
                table: "public_licensed_business_records",
                columns: new[] { "SourceId", "OpenServiceId", "ManagementNumber", "SourceRevision" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "public_building_business_aggregates");

            migrationBuilder.DropTable(
                name: "public_business_building_assignments");

            migrationBuilder.DropTable(
                name: "public_licensed_business_records");

            migrationBuilder.DropColumn(
                name: "NormalizedRoadAddressKey",
                table: "public_building_register_titles");
        }
    }
}
