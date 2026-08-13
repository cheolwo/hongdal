using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssalddel.Infrastructure.Persistence.PublicData.Migrations
{
    /// <inheritdoc />
    public partial class AddBuildingMassingAndVisualComposition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "OfficialBuildingCoveragePercent",
                table: "public_building_register_titles",
                type: "decimal(12,4)",
                precision: 12,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "OfficialFloorAreaRatioPercent",
                table: "public_building_register_titles",
                type: "decimal(12,4)",
                precision: 12,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SiteAreaSquareMeters",
                table: "public_building_register_titles",
                type: "decimal(20,4)",
                precision: 20,
                scale: 4,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "public_building_massing_profiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    BuildingRecordId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ObservedAboveGroundFloorCount = table.Column<int>(type: "int", nullable: true),
                    EstimatedAboveGroundFloorCount = table.Column<int>(type: "int", nullable: true),
                    PresentationAboveGroundFloorCount = table.Column<int>(type: "int", nullable: false),
                    OfficialBuildingCoveragePercent = table.Column<decimal>(type: "decimal(12,4)", precision: 12, scale: 4, nullable: true),
                    OfficialFloorAreaRatioPercent = table.Column<decimal>(type: "decimal(12,4)", precision: 12, scale: 4, nullable: true),
                    SimpleBuildingToSiteRatioPercent = table.Column<decimal>(type: "decimal(12,4)", precision: 12, scale: 4, nullable: true),
                    SimpleGrossFloorToSiteRatioPercent = table.Column<decimal>(type: "decimal(12,4)", precision: 12, scale: 4, nullable: true),
                    SiteAreaSquareMeters = table.Column<decimal>(type: "decimal(20,4)", precision: 20, scale: 4, nullable: true),
                    BuildingAreaSquareMeters = table.Column<decimal>(type: "decimal(20,4)", precision: 20, scale: 4, nullable: true),
                    TotalFloorAreaSquareMeters = table.Column<decimal>(type: "decimal(20,4)", precision: 20, scale: 4, nullable: true),
                    HeightMeters = table.Column<decimal>(type: "decimal(12,4)", precision: 12, scale: 4, nullable: true),
                    EstimatedFloorHeightMeters = table.Column<decimal>(type: "decimal(8,4)", precision: 8, scale: 4, nullable: true),
                    FootprintTierCode = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HeightTierCode = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DensityTierCode = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EvidenceKindCode = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RuleRevision = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProfileHashSha256 = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GeneratedAtUtc = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_public_building_massing_profiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_public_building_massing_profiles_public_building_register_ti~",
                        column: x => x.BuildingRecordId,
                        principalTable: "public_building_register_titles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "public_building_visual_composition_plans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    BuildingMassingProfileId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    VisualFamilyCode = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PresentationFloorCount = table.Column<int>(type: "int", nullable: false),
                    MiddleFloorRepeatCount = table.Column<int>(type: "int", nullable: false),
                    SiteCoverageTierCode = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SurroundingSpaceTierCode = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LodTierCode = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PresentationOnly = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RuleRevision = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PlanHashSha256 = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GeneratedAtUtc = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_public_building_visual_composition_plans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_public_building_visual_composition_plans_public_building_mas~",
                        column: x => x.BuildingMassingProfileId,
                        principalTable: "public_building_massing_profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_public_building_massing_profiles_BuildingRecordId_RuleRevisi~",
                table: "public_building_massing_profiles",
                columns: new[] { "BuildingRecordId", "RuleRevision" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_public_building_visual_composition_plans_BuildingMassingProf~",
                table: "public_building_visual_composition_plans",
                columns: new[] { "BuildingMassingProfileId", "RuleRevision" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "public_building_visual_composition_plans");

            migrationBuilder.DropTable(
                name: "public_building_massing_profiles");

            migrationBuilder.DropColumn(
                name: "OfficialBuildingCoveragePercent",
                table: "public_building_register_titles");

            migrationBuilder.DropColumn(
                name: "OfficialFloorAreaRatioPercent",
                table: "public_building_register_titles");

            migrationBuilder.DropColumn(
                name: "SiteAreaSquareMeters",
                table: "public_building_register_titles");
        }
    }
}
