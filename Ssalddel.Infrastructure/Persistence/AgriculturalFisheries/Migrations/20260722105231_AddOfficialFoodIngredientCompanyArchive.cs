using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssalddel.Infrastructure.Persistence.AgriculturalFisheries.Migrations
{
    /// <inheritdoc />
    public partial class AddOfficialFoodIngredientCompanyArchive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "food_ingredient_company_research_runs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RunKey = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TriggerCode = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StatusCode = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RequestedIngredientCount = table.Column<int>(type: "int", nullable: false),
                    ProcessedIngredientCount = table.Column<int>(type: "int", nullable: false),
                    SkippedIngredientCount = table.Column<int>(type: "int", nullable: false),
                    AvailableIngredientCount = table.Column<int>(type: "int", nullable: false),
                    PartialIngredientCount = table.Column<int>(type: "int", nullable: false),
                    NoResultIngredientCount = table.Column<int>(type: "int", nullable: false),
                    NotConfiguredIngredientCount = table.Column<int>(type: "int", nullable: false),
                    FailedIngredientCount = table.Column<int>(type: "int", nullable: false),
                    ObservedEvidenceCount = table.Column<int>(type: "int", nullable: false),
                    StartedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ErrorMessage = table.Column<string>(type: "text", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_food_ingredient_company_research_runs", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "food_ingredient_company_evidence",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IngredientId = table.Column<long>(type: "bigint", nullable: false),
                    LastResearchRunId = table.Column<long>(type: "bigint", nullable: false),
                    CandidateKey = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OrganizationKey = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OrganizationName = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NormalizedOrganizationName = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CountryCode = table.Column<string>(type: "varchar(8)", maxLength: 8, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CountryName = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RelationCode = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EvidenceCode = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EvidenceSummary = table.Column<string>(type: "text", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RelatedProductName = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProductCategory = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OfficialIdentifier = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EvidenceRecordIdentifier = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    VerificationStatusCode = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RawIngredientText = table.Column<string>(type: "text", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EvidenceDate = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EvidenceLastChangedDate = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EvidenceSequence = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RequiresAttention = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AttentionReason = table.Column<string>(type: "text", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SourceKey = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SourceName = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SourceUrl = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ResearchQueryTerm = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FirstObservedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    LastObservedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ObservationCount = table.Column<int>(type: "int", nullable: false),
                    IsCurrent = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RequiresLiveRecheck = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CanAutoSelect = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CanAutoContact = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_food_ingredient_company_evidence", x => x.Id);
                    table.ForeignKey(
                        name: "FK_food_ingredient_company_evidence_food_ingredient_company_res~",
                        column: x => x.LastResearchRunId,
                        principalTable: "food_ingredient_company_research_runs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_food_ingredient_company_evidence_food_official_ingredients_I~",
                        column: x => x.IngredientId,
                        principalTable: "food_official_ingredients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "food_ingredient_company_profiles",
                columns: table => new
                {
                    IngredientId = table.Column<long>(type: "bigint", nullable: false),
                    LastResearchRunId = table.Column<long>(type: "bigint", nullable: false),
                    StatusCode = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ResearchQueryTerm = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastResearchedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    OrganizationCount = table.Column<int>(type: "int", nullable: false),
                    EvidenceCount = table.Column<int>(type: "int", nullable: false),
                    DomesticManufacturerCount = table.Column<int>(type: "int", nullable: false),
                    DomesticImporterCount = table.Column<int>(type: "int", nullable: false),
                    ForeignManufacturerCount = table.Column<int>(type: "int", nullable: false),
                    AvailableSourceCount = table.Column<int>(type: "int", nullable: false),
                    FailedSourceCount = table.Column<int>(type: "int", nullable: false),
                    NotConfiguredSourceCount = table.Column<int>(type: "int", nullable: false),
                    ConsecutiveFailureCount = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_food_ingredient_company_profiles", x => x.IngredientId);
                    table.ForeignKey(
                        name: "FK_food_ingredient_company_profiles_food_ingredient_company_res~",
                        column: x => x.LastResearchRunId,
                        principalTable: "food_ingredient_company_research_runs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_food_ingredient_company_profiles_food_official_ingredients_I~",
                        column: x => x.IngredientId,
                        principalTable: "food_official_ingredients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "food_ingredient_company_source_observations",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ResearchRunId = table.Column<long>(type: "bigint", nullable: false),
                    IngredientId = table.Column<long>(type: "bigint", nullable: false),
                    SourceKey = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Provider = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DisplayName = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CountryScope = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OfficialUrl = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StatusCode = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StatusMessage = table.Column<string>(type: "text", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProvidesDirectIngredientEvidence = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CanVerifyCurrentOrganizationStatus = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RequiresLiveRecheck = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ObservedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_food_ingredient_company_source_observations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_food_ingredient_company_source_observations_food_ingredient_~",
                        column: x => x.ResearchRunId,
                        principalTable: "food_ingredient_company_research_runs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_food_ingredient_company_source_observations_food_official_in~",
                        column: x => x.IngredientId,
                        principalTable: "food_official_ingredients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_food_ingredient_company_evidence_CountryCode_RelationCode_Is~",
                table: "food_ingredient_company_evidence",
                columns: new[] { "CountryCode", "RelationCode", "IsCurrent" });

            migrationBuilder.CreateIndex(
                name: "IX_food_ingredient_company_evidence_IngredientId_CandidateKey",
                table: "food_ingredient_company_evidence",
                columns: new[] { "IngredientId", "CandidateKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_food_ingredient_company_evidence_IngredientId_IsCurrent_Rela~",
                table: "food_ingredient_company_evidence",
                columns: new[] { "IngredientId", "IsCurrent", "RelationCode" });

            migrationBuilder.CreateIndex(
                name: "IX_food_ingredient_company_evidence_IngredientId_OrganizationKe~",
                table: "food_ingredient_company_evidence",
                columns: new[] { "IngredientId", "OrganizationKey", "IsCurrent" });

            migrationBuilder.CreateIndex(
                name: "IX_food_ingredient_company_evidence_LastResearchRunId",
                table: "food_ingredient_company_evidence",
                column: "LastResearchRunId");

            migrationBuilder.CreateIndex(
                name: "IX_food_ingredient_company_evidence_SourceKey_IsCurrent_LastObs~",
                table: "food_ingredient_company_evidence",
                columns: new[] { "SourceKey", "IsCurrent", "LastObservedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_food_ingredient_company_profiles_LastResearchRunId",
                table: "food_ingredient_company_profiles",
                column: "LastResearchRunId");

            migrationBuilder.CreateIndex(
                name: "IX_food_ingredient_company_profiles_StatusCode_LastResearchedAt~",
                table: "food_ingredient_company_profiles",
                columns: new[] { "StatusCode", "LastResearchedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_food_ingredient_company_research_runs_RunKey",
                table: "food_ingredient_company_research_runs",
                column: "RunKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_food_ingredient_company_research_runs_TriggerCode_StatusCode~",
                table: "food_ingredient_company_research_runs",
                columns: new[] { "TriggerCode", "StatusCode", "StartedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_food_ingredient_company_source_observations_IngredientId_Obs~",
                table: "food_ingredient_company_source_observations",
                columns: new[] { "IngredientId", "ObservedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_food_ingredient_company_source_observations_ResearchRunId_In~",
                table: "food_ingredient_company_source_observations",
                columns: new[] { "ResearchRunId", "IngredientId", "SourceKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_food_ingredient_company_source_observations_SourceKey_Status~",
                table: "food_ingredient_company_source_observations",
                columns: new[] { "SourceKey", "StatusCode", "ObservedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "food_ingredient_company_evidence");

            migrationBuilder.DropTable(
                name: "food_ingredient_company_profiles");

            migrationBuilder.DropTable(
                name: "food_ingredient_company_source_observations");

            migrationBuilder.DropTable(
                name: "food_ingredient_company_research_runs");
        }
    }
}
