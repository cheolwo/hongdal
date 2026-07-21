using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Ssalddel.Infrastructure.Persistence.AgriculturalFisheries.Migrations
{
    /// <inheritdoc />
    public partial class AddOfficialFoodRecipeIngredients : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IngredientCount",
                table: "food_official_recipe_variants",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "IngredientParserVersion",
                table: "food_official_recipe_variants",
                type: "varchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "IngredientsIndexedAtUtc",
                table: "food_official_recipe_variants",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "food_official_ingredient_categories",
                columns: table => new
                {
                    CategoryCode = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    KoreanName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EnglishName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_food_official_ingredient_categories", x => x.CategoryCode);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "food_official_ingredients",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IngredientKey = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LanguageCode = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CanonicalName = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NormalizedName = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CategoryCode = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ClassificationMethod = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ClassificationConfidence = table.Column<decimal>(type: "decimal(5,4)", precision: 5, scale: 4, nullable: false),
                    ClassificationState = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_food_official_ingredients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_food_official_ingredients_food_official_ingredient_categorie~",
                        column: x => x.CategoryCode,
                        principalTable: "food_official_ingredient_categories",
                        principalColumn: "CategoryCode",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "food_official_recipe_ingredients",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RecipeVariantId = table.Column<long>(type: "bigint", nullable: false),
                    IngredientId = table.Column<long>(type: "bigint", nullable: false),
                    GroupName = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OriginalText = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SourceName = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    QuantityText = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    QuantityValue = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: true),
                    QuantityMaxValue = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: true),
                    UnitCode = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UnitText = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HouseholdMeasureText = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PreparationNote = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    ParserVersion = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ParseConfidence = table.Column<decimal>(type: "decimal(5,4)", precision: 5, scale: 4, nullable: false),
                    RequiresReview = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_food_official_recipe_ingredients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_food_official_recipe_ingredients_food_official_ingredients_I~",
                        column: x => x.IngredientId,
                        principalTable: "food_official_ingredients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_food_official_recipe_ingredients_food_official_recipe_varian~",
                        column: x => x.RecipeVariantId,
                        principalTable: "food_official_recipe_variants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "food_official_ingredient_categories",
                columns: new[] { "CategoryCode", "Description", "EnglishName", "IsActive", "KoreanName", "SortOrder" },
                values: new object[,]
                {
                    { "beverage-alcohol", "음료, 와인과 조리용 주류", "Beverages and alcohol", true, "음료·주류", 160 },
                    { "dairy", "우유, 치즈, 요구르트와 크림", "Dairy", true, "유제품", 100 },
                    { "fruit", "생과일, 건과일과 과일즙", "Fruits", true, "과일류", 40 },
                    { "grain-starch", "쌀, 밀, 면, 떡, 가루와 전분류", "Grains and starches", true, "곡류·전분", 10 },
                    { "legume-soy", "콩, 두류, 두부와 두유 등 콩 가공품", "Legumes and soy", true, "콩·두류·두부", 20 },
                    { "meat", "소·돼지 등 포유류 고기와 부위", "Meat", true, "육류", 70 },
                    { "mushroom", "생버섯과 건버섯", "Mushrooms", true, "버섯류", 50 },
                    { "nut-seed", "견과류, 깨와 식용 씨앗", "Nuts and seeds", true, "견과·종실류", 110 },
                    { "oil-fat", "식용유, 참기름, 버터 등 조리용 지방", "Oils and fats", true, "유지류", 120 },
                    { "other", "규칙으로 분류하지 못해 운영자 검토가 필요한 재료", "Other or review required", true, "기타·검토 필요", 999 },
                    { "poultry-egg", "닭·오리 등 가금류와 달걀", "Poultry and eggs", true, "가금류·알류", 80 },
                    { "processed-food", "햄, 소시지, 김치, 피클 등 가공 재료", "Processed foods", true, "가공식품", 150 },
                    { "sauce-fermented", "간장, 된장, 고추장, 식초와 소스", "Sauces and fermented condiments", true, "장류·소스류", 140 },
                    { "seafood", "생선, 조개, 갑각류와 수산 건제품", "Seafood", true, "수산물", 90 },
                    { "seasoning-spice", "소금, 설탕, 후추와 향신 재료", "Seasonings and spices", true, "조미료·향신료", 130 },
                    { "seaweed", "김, 미역, 다시마 등 해조류", "Seaweeds", true, "해조류", 60 },
                    { "vegetable", "잎·뿌리·열매·줄기 채소와 생채소", "Vegetables", true, "채소류", 30 },
                    { "water-stock", "물, 쌀뜨물과 조리용 육수", "Water and stocks", true, "물·육수", 170 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_food_official_ingredient_categories_IsActive_SortOrder",
                table: "food_official_ingredient_categories",
                columns: new[] { "IsActive", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_food_official_ingredients_CategoryCode_ClassificationState_C~",
                table: "food_official_ingredients",
                columns: new[] { "CategoryCode", "ClassificationState", "CanonicalName" });

            migrationBuilder.CreateIndex(
                name: "IX_food_official_ingredients_IngredientKey",
                table: "food_official_ingredients",
                column: "IngredientKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_food_official_ingredients_LanguageCode_NormalizedName",
                table: "food_official_ingredients",
                columns: new[] { "LanguageCode", "NormalizedName" });

            migrationBuilder.CreateIndex(
                name: "IX_food_official_recipe_ingredients_IngredientId_RecipeVariantId",
                table: "food_official_recipe_ingredients",
                columns: new[] { "IngredientId", "RecipeVariantId" });

            migrationBuilder.CreateIndex(
                name: "IX_food_official_recipe_ingredients_RecipeVariantId_DisplayOrder",
                table: "food_official_recipe_ingredients",
                columns: new[] { "RecipeVariantId", "DisplayOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_food_official_recipe_ingredients_RequiresReview_ParseConfide~",
                table: "food_official_recipe_ingredients",
                columns: new[] { "RequiresReview", "ParseConfidence" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "food_official_recipe_ingredients");

            migrationBuilder.DropTable(
                name: "food_official_ingredients");

            migrationBuilder.DropTable(
                name: "food_official_ingredient_categories");

            migrationBuilder.DropColumn(
                name: "IngredientCount",
                table: "food_official_recipe_variants");

            migrationBuilder.DropColumn(
                name: "IngredientParserVersion",
                table: "food_official_recipe_variants");

            migrationBuilder.DropColumn(
                name: "IngredientsIndexedAtUtc",
                table: "food_official_recipe_variants");
        }
    }
}
