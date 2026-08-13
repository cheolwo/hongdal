using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Ssalddel.Infrastructure.Persistence.PublicData.Migrations
{
    /// <inheritdoc />
    public partial class AddAdministrativeBuildingClassificationLedger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "public_building_category_catalog",
                columns: table => new
                {
                    CategoryCode = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DisplayNameKo = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DescriptionKo = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    WorldRoleCode = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    PresentationEligible = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_public_building_category_catalog", x => x.CategoryCode);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "public_building_register_titles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    RegisterManagementPk = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RegisterKindCode = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RegisterTypeCode = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SigunguCode = table.Column<string>(type: "varchar(5)", maxLength: 5, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LegalDongCode = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LandLot = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RoadAddress = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BuildingName = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DongName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MainPurposeCode = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MainPurposeName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StructureCode = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StructureName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BuildingAreaSquareMeters = table.Column<decimal>(type: "decimal(20,4)", precision: 20, scale: 4, nullable: true),
                    TotalFloorAreaSquareMeters = table.Column<decimal>(type: "decimal(20,4)", precision: 20, scale: 4, nullable: true),
                    HeightMeters = table.Column<decimal>(type: "decimal(12,4)", precision: 12, scale: 4, nullable: true),
                    AboveGroundFloorCount = table.Column<int>(type: "int", nullable: true),
                    UndergroundFloorCount = table.Column<int>(type: "int", nullable: true),
                    ApprovalDate = table.Column<DateOnly>(type: "date", nullable: true),
                    SourceRevision = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EvidenceSnapshotId = table.Column<long>(type: "bigint", nullable: false),
                    ObservedAtUtc = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    ValidToUtc = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_public_building_register_titles", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "public_administrative_building_category_aggregates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    AdministrativeRegionStableId = table.Column<string>(type: "varchar(240)", maxLength: 240, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SourceVintage = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CategoryCode = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BuildingCount = table.Column<long>(type: "bigint", nullable: false),
                    BuildingAreaSquareMeters = table.Column<decimal>(type: "decimal(24,4)", precision: 24, scale: 4, nullable: false),
                    TotalFloorAreaSquareMeters = table.Column<decimal>(type: "decimal(24,4)", precision: 24, scale: 4, nullable: false),
                    NamedBuildingCount = table.Column<long>(type: "bigint", nullable: false),
                    GeometryLinkedCount = table.Column<long>(type: "bigint", nullable: false),
                    UnresolvedBuildingCount = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("PK_public_administrative_building_category_aggregates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_public_administrative_building_category_aggregates_public_bu~",
                        column: x => x.CategoryCode,
                        principalTable: "public_building_category_catalog",
                        principalColumn: "CategoryCode",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "public_building_category_assignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    BuildingRecordId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CategoryCode = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsPrimary = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AssignmentMethodCode = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EvidenceKindCode = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RuleRevision = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SourceMainPurposeCode = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SourceMainPurposeName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ClassifiedAtUtc = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_public_building_category_assignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_public_building_category_assignments_public_building_categor~",
                        column: x => x.CategoryCode,
                        principalTable: "public_building_category_catalog",
                        principalColumn: "CategoryCode",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_public_building_category_assignments_public_building_registe~",
                        column: x => x.BuildingRecordId,
                        principalTable: "public_building_register_titles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "public_building_region_assignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    BuildingRecordId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    LegalRegionStableId = table.Column<string>(type: "varchar(240)", maxLength: 240, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AdministrativeRegionStableId = table.Column<string>(type: "varchar(240)", maxLength: 240, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AssignmentMethodCode = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ConfidenceCode = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SourceVintage = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RuleRevision = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ValidFromUtc = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    ValidToUtc = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_public_building_region_assignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_public_building_region_assignments_public_building_register_~",
                        column: x => x.BuildingRecordId,
                        principalTable: "public_building_register_titles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "public_building_category_catalog",
                columns: new[] { "CategoryCode", "DescriptionKo", "DisplayNameKo", "PresentationEligible", "SortOrder", "WorldRoleCode" },
                values: new object[,]
                {
                    { "agriculture", "동물·식물 관련 시설 등 농업 생산을 지원하는 건축물", "농업", true, 20, "farm" },
                    { "business-office", "업무시설 등 사무 기능의 건축물", "업무", true, 50, "town" },
                    { "commercial", "근린생활·판매시설 등 생활권 상업 건축물", "상업·생활", true, 40, "town" },
                    { "culture-tourism", "문화·집회·숙박·관광·운동 관련 건축물", "문화·관광", true, 100, "town" },
                    { "education-research", "교육연구시설", "교육·연구", true, 80, "civic" },
                    { "industrial", "공장 등 제조·산업 기능의 건축물", "산업", true, 70, "industrial" },
                    { "logistics-storage", "창고시설 등 보관·적재와 관계되는 건축물", "물류·창고", true, 30, "hub" },
                    { "medical-welfare", "의료시설과 노유자시설", "의료·복지", true, 90, "civic" },
                    { "other", "공식 주용도는 있으나 현재 규칙에 대응하지 않는 건축물", "기타", false, 900, "generic" },
                    { "public-community", "공공·안전·공동체 기능으로 검토할 건축물", "공공·공동체", true, 60, "civic" },
                    { "religious", "종교시설", "종교", true, 130, "settlement" },
                    { "residential", "단독·공동주택 등 사람이 거주하는 건축물", "주거", true, 10, "settlement" },
                    { "transport", "운수시설과 자동차 관련 시설", "교통", true, 110, "transport" },
                    { "unresolved", "공식 주용도가 없거나 행정동 배정·분류가 해결되지 않은 건축물", "미분류", false, 999, "unresolved" },
                    { "utility-infrastructure", "발전·방송통신·자원순환·위험물 처리 관련 건축물", "기반시설", true, 120, "infrastructure" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_public_administrative_building_category_aggregates_Administr~",
                table: "public_administrative_building_category_aggregates",
                columns: new[] { "AdministrativeRegionStableId", "SourceVintage", "CategoryCode", "RuleRevision" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_public_administrative_building_category_aggregates_CategoryC~",
                table: "public_administrative_building_category_aggregates",
                column: "CategoryCode");

            migrationBuilder.CreateIndex(
                name: "IX_public_building_category_assignments_BuildingRecordId_RuleRe~",
                table: "public_building_category_assignments",
                columns: new[] { "BuildingRecordId", "RuleRevision", "IsPrimary" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_public_building_category_assignments_CategoryCode_RuleRevisi~",
                table: "public_building_category_assignments",
                columns: new[] { "CategoryCode", "RuleRevision" });

            migrationBuilder.CreateIndex(
                name: "IX_public_building_region_assignments_AdministrativeRegionStabl~",
                table: "public_building_region_assignments",
                columns: new[] { "AdministrativeRegionStableId", "SourceVintage" });

            migrationBuilder.CreateIndex(
                name: "IX_public_building_region_assignments_BuildingRecordId_RuleRevi~",
                table: "public_building_region_assignments",
                columns: new[] { "BuildingRecordId", "RuleRevision" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_public_building_register_titles_RegisterManagementPk_SourceR~",
                table: "public_building_register_titles",
                columns: new[] { "RegisterManagementPk", "SourceRevision" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_public_building_register_titles_SigunguCode_LegalDongCode_Va~",
                table: "public_building_register_titles",
                columns: new[] { "SigunguCode", "LegalDongCode", "ValidToUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "public_administrative_building_category_aggregates");

            migrationBuilder.DropTable(
                name: "public_building_category_assignments");

            migrationBuilder.DropTable(
                name: "public_building_region_assignments");

            migrationBuilder.DropTable(
                name: "public_building_category_catalog");

            migrationBuilder.DropTable(
                name: "public_building_register_titles");
        }
    }
}
