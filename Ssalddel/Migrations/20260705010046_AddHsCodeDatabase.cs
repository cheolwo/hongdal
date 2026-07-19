using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssalddel.Migrations
{
    /// <inheritdoc />
    public partial class AddHsCodeDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "hs_code_catalog_versions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    StandardCode = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CountryCode = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CodeDigits = table.Column<int>(type: "int", nullable: false),
                    Revision = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SourceName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SourceUrl = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    EffectiveTo = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ImportedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Notes = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hs_code_catalog_versions", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "hs_code_platform_agency_experiences",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    HsCode = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AgencyType = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CountryRoute = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CaseStatus = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RiskLevel = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Summary = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RequiredDocumentsJson = table.Column<string>(type: "json", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ContributorUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ContributorConsented = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsPaidDetail = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    PaidAccessPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ContributorRewardRate = table.Column<decimal>(type: "decimal(9,6)", nullable: false),
                    DisclosurePolicy = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CompletedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hs_code_platform_agency_experiences", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "hs_code_entries",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CatalogVersionId = table.Column<long>(type: "bigint", nullable: false),
                    Code = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NormalizedCode = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ParentNormalizedCode = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Level = table.Column<int>(type: "int", nullable: false),
                    KoreanName = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EnglishName = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SearchKeywords = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hs_code_entries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hs_code_entries_hs_code_catalog_versions_CatalogVersionId",
                        column: x => x.CatalogVersionId,
                        principalTable: "hs_code_catalog_versions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "hs_code_classification_cases",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    HsCodeEntryId = table.Column<long>(type: "bigint", nullable: true),
                    HsCode = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CountryCode = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SourceType = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SourceReferenceNo = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SourceUrl = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IssuingAuthority = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DecidedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ProductName = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GoodsDescription = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DecisionReason = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsPublicOfficialCase = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hs_code_classification_cases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hs_code_classification_cases_hs_code_entries_HsCodeEntryId",
                        column: x => x.HsCodeEntryId,
                        principalTable: "hs_code_entries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_hs_code_catalog_versions_CountryCode_IsActive_EffectiveFrom",
                table: "hs_code_catalog_versions",
                columns: new[] { "CountryCode", "IsActive", "EffectiveFrom" });

            migrationBuilder.CreateIndex(
                name: "IX_hs_code_catalog_versions_StandardCode_CountryCode_Revision_C~",
                table: "hs_code_catalog_versions",
                columns: new[] { "StandardCode", "CountryCode", "Revision", "CodeDigits" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hs_code_classification_cases_CountryCode_HsCode_DecidedAt",
                table: "hs_code_classification_cases",
                columns: new[] { "CountryCode", "HsCode", "DecidedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_hs_code_classification_cases_HsCodeEntryId",
                table: "hs_code_classification_cases",
                column: "HsCodeEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_hs_code_classification_cases_ProductName",
                table: "hs_code_classification_cases",
                column: "ProductName");

            migrationBuilder.CreateIndex(
                name: "IX_hs_code_classification_cases_SourceType_SourceReferenceNo",
                table: "hs_code_classification_cases",
                columns: new[] { "SourceType", "SourceReferenceNo" });

            migrationBuilder.CreateIndex(
                name: "IX_hs_code_entries_CatalogVersionId_NormalizedCode",
                table: "hs_code_entries",
                columns: new[] { "CatalogVersionId", "NormalizedCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hs_code_entries_CatalogVersionId_ParentNormalizedCode",
                table: "hs_code_entries",
                columns: new[] { "CatalogVersionId", "ParentNormalizedCode" });

            migrationBuilder.CreateIndex(
                name: "IX_hs_code_entries_KoreanName",
                table: "hs_code_entries",
                column: "KoreanName");

            migrationBuilder.CreateIndex(
                name: "IX_hs_code_entries_NormalizedCode_IsActive",
                table: "hs_code_entries",
                columns: new[] { "NormalizedCode", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_hs_code_platform_agency_experiences_ContributorUserId_Contri~",
                table: "hs_code_platform_agency_experiences",
                columns: new[] { "ContributorUserId", "ContributorConsented" });

            migrationBuilder.CreateIndex(
                name: "IX_hs_code_platform_agency_experiences_HsCode_AgencyType_Countr~",
                table: "hs_code_platform_agency_experiences",
                columns: new[] { "HsCode", "AgencyType", "CountryRoute" });

            migrationBuilder.CreateIndex(
                name: "IX_hs_code_platform_agency_experiences_HsCode_ContributorConsen~",
                table: "hs_code_platform_agency_experiences",
                columns: new[] { "HsCode", "ContributorConsented", "IsPaidDetail" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "hs_code_classification_cases");

            migrationBuilder.DropTable(
                name: "hs_code_platform_agency_experiences");

            migrationBuilder.DropTable(
                name: "hs_code_entries");

            migrationBuilder.DropTable(
                name: "hs_code_catalog_versions");
        }
    }
}