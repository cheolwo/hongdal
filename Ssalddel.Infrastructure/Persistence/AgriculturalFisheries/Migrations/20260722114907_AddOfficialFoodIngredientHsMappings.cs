using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssalddel.Infrastructure.Persistence.AgriculturalFisheries.Migrations
{
    /// <inheritdoc />
    public partial class AddOfficialFoodIngredientHsMappings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "food_official_ingredient_hs_mappings",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IngredientId = table.Column<long>(type: "bigint", nullable: false),
                    HsCodeCatalogVersionId = table.Column<long>(type: "bigint", nullable: false),
                    HsCodeEntryId = table.Column<long>(type: "bigint", nullable: false),
                    CountryCode = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    JurisdictionUseCode = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StandardCode = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CatalogRevision = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CodeDigits = table.Column<int>(type: "int", nullable: false),
                    CatalogEffectiveFrom = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CatalogEffectiveTo = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CatalogImportedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    HsCode = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NormalizedHsCode = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HsCodeLevel = table.Column<int>(type: "int", nullable: false),
                    KoreanName = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EnglishName = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "text", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MatchMethod = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MatchQualityCode = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MatchConfidence = table.Column<decimal>(type: "decimal(5,4)", precision: 5, scale: 4, nullable: false),
                    MappingState = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MatchBasis = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReviewReason = table.Column<string>(type: "text", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RequiredProductDetailsJson = table.Column<string>(type: "json", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SourceName = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SourceUrl = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RequiresProfessionalReview = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    LastCheckedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_food_official_ingredient_hs_mappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_food_official_ingredient_hs_mappings_food_official_ingredien~",
                        column: x => x.IngredientId,
                        principalTable: "food_official_ingredients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_food_official_ingredient_hs_mappings_IngredientId_CountryCod~",
                table: "food_official_ingredient_hs_mappings",
                columns: new[] { "IngredientId", "CountryCode", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_food_official_ingredient_hs_mappings_IngredientId_HsCodeCata~",
                table: "food_official_ingredient_hs_mappings",
                columns: new[] { "IngredientId", "HsCodeCatalogVersionId", "HsCodeEntryId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_food_official_ingredient_hs_mappings_MappingState_IsActive_L~",
                table: "food_official_ingredient_hs_mappings",
                columns: new[] { "MappingState", "IsActive", "LastCheckedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_food_official_ingredient_hs_mappings_NormalizedHsCode_Countr~",
                table: "food_official_ingredient_hs_mappings",
                columns: new[] { "NormalizedHsCode", "CountryCode", "IsActive" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "food_official_ingredient_hs_mappings");
        }
    }
}
