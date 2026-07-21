using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Ssalddel.Infrastructure.Persistence.AgriculturalFisheries.Migrations
{
    /// <inheritdoc />
    public partial class AddOfficialFoodRecipeArchive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "food_official_dishes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    DishKey = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CountryCode = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RegionName = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OriginalName = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EnglishName = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Category = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Summary = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RepresentationState = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReviewState = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_food_official_dishes", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "food_official_recipe_collection_runs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RunKey = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SourceKey = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StatusCode = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    QuerySummary = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SourceUrl = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StartedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    FetchedCount = table.Column<int>(type: "int", nullable: false),
                    InsertedCount = table.Column<int>(type: "int", nullable: false),
                    UpdatedCount = table.Column<int>(type: "int", nullable: false),
                    ExistingCount = table.Column<int>(type: "int", nullable: false),
                    ErrorMessage = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_food_official_recipe_collection_runs", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "food_official_recipe_sources",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SourceKey = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Provider = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DisplayName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CountryCode = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LanguageCode = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AccessMethod = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DocumentationUrl = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TermsUrl = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LicenseCode = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TextReusePolicy = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ImageReusePolicy = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AttributionTemplate = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdateCycle = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AutomationState = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FullTextStorageAllowed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ImageBinaryStorageAllowed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RequiresEditorialReview = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RightsVerifiedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    LastCollectedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_food_official_recipe_sources", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "food_official_recipe_variants",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SourceId = table.Column<long>(type: "bigint", nullable: false),
                    DishId = table.Column<long>(type: "bigint", nullable: false),
                    FirstCollectionRunId = table.Column<long>(type: "bigint", nullable: false),
                    RecordKey = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ExternalId = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Title = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Summary = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RegionName = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Category = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ServingText = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IngredientsJson = table.Column<string>(type: "json", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    InstructionsJson = table.Column<string>(type: "json", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NutritionJson = table.Column<string>(type: "json", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TagsJson = table.Column<string>(type: "json", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Tips = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OriginalUrl = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ImageReferenceUrl = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RawPayload = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ContentChecksum = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LicenseCodeAtCollection = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TextReusePolicyAtCollection = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ImageReusePolicyAtCollection = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AttributionText = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SourceModifiedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    FirstCollectedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    LastCollectedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ContentExpiresAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsRemovedAtSource = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_food_official_recipe_variants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_food_official_recipe_variants_food_official_dishes_DishId",
                        column: x => x.DishId,
                        principalTable: "food_official_dishes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_food_official_recipe_variants_food_official_recipe_collectio~",
                        column: x => x.FirstCollectionRunId,
                        principalTable: "food_official_recipe_collection_runs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_food_official_recipe_variants_food_official_recipe_sources_S~",
                        column: x => x.SourceId,
                        principalTable: "food_official_recipe_sources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "food_official_recipe_sources",
                columns: new[] { "Id", "AccessMethod", "AttributionTemplate", "AutomationState", "CountryCode", "DisplayName", "DocumentationUrl", "FullTextStorageAllowed", "ImageBinaryStorageAllowed", "ImageReusePolicy", "LanguageCode", "LastCollectedAtUtc", "LicenseCode", "Provider", "RequiresEditorialReview", "RightsVerifiedAtUtc", "SourceKey", "TermsUrl", "TextReusePolicy", "UpdateCycle", "UpdatedAtUtc" },
                values: new object[,]
                {
                    { 1L, "JSON Open API", "출처: 식품의약품안전처 식품안전나라 COOKRCP01 ({url}, 수집일 {date})", "EnabledWhenConfigured", "KR", "식약처 조리식품 레시피 COOKRCP01", "https://www.foodsafetykorea.go.kr/api/openApiInfo.do?menu_grp=MENU_GRP31&menu_no=661&show_cnt=10&start_idx=1&svc_no=COOKRCP01", true, false, "초기 수집에서는 이미지 URL만 기록하며 이미지 파일은 복제하지 않습니다.", "ko", null, "공공데이터포털 이용허락범위 제한 없음", "식품의약품안전처 식품안전나라", true, new DateTime(2026, 7, 21, 0, 0, 0, 0, DateTimeKind.Utc), "mfds-cookrcp01", "https://www.data.go.kr/data/15060073/openapi.do", "원문과 구조화 레시피를 내부 아카이브에 저장할 수 있으나 커뮤니티 게시 전 출처·영양 단위·최신성을 검토합니다.", "실시간 원천, 운영 수집 주기는 일 1회 이하 권장", new DateTime(2026, 7, 21, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2L, "XML Open API", "출처: 농촌진흥청 농사로 향토 음식 ({url}, 수집일 {date})", "EnabledWhenConfigured", "KR", "농촌진흥청 향토 음식", "https://www.data.go.kr/data/15101449/openapi.do", true, false, "사진 권리는 별도 확인 대상으로 보고 URL만 기록하며 파일은 복제하지 않습니다.", "ko", null, "공공데이터포털 제한 없음 / 농사로 목록 공공누리 유형3 표시", "농촌진흥청 농사로", true, new DateTime(2026, 7, 21, 0, 0, 0, 0, DateTimeKind.Utc), "rda-local-food", "https://www.nongsaro.go.kr/portal/ps/psn/psnj/openApiLst.ps?menuId=PS65428&pageIndex=1&pageSize=&sLclasCode=&sText=%ED%96%A5%ED%86%A0+%EC%9D%8C%EC%8B%9D", "두 공식 페이지의 권리 표기가 달라 내부 검토 아카이브로만 저장하고 외부 게시에는 별도 권리 확인이 필요합니다.", "실시간 원천, 월 1회 변경 확인 권장", new DateTime(2026, 7, 21, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 3L, "Official HTML", "Created by Ssalddel by extracting text from the Ministry of Agriculture, Forestry and Fisheries website ({url}, accessed {date}); images are not reused.", "Enabled", "JP", "일본 Our Regional Cuisines", "https://www.maff.go.jp/e/policies/market/k_ryouri/", true, false, "대부분의 사진은 제3자 권리이므로 URL만 기록하고 파일은 복제하지 않습니다.", "en", null, "Public Data License 1.0 (별도 표시 없는 정부 텍스트)", "일본 농림수산성(MAFF)", true, new DateTime(2026, 7, 21, 0, 0, 0, 0, DateTimeKind.Utc), "maff-regional-cuisines", "https://www.maff.go.jp/e/use/term_use.html", "출처와 편집 여부를 표시한 텍스트 아카이브만 허용하며 제3자 표기가 있는 내용은 수동 검토합니다.", "페이지 변경 시 갱신, 월 1회 확인 권장", new DateTime(2026, 7, 21, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 4L, "Official HTML / JSON-LD", "Information from the NHS website ({url}), copied on {date}. Licensed under the Open Government Licence v3.0.", "Enabled", "GB", "NHS Healthier Families recipes", "https://www.nhs.uk/healthier-families/recipes/", true, false, "로고·시각물·다수 사진은 표준 허락에서 제외되므로 이미지 URL도 외부 게시에 사용하지 않습니다.", "en", null, "Open Government Licence v3.0 with NHS terms", "NHS England", true, new DateTime(2026, 7, 21, 0, 0, 0, 0, DateTimeKind.Utc), "nhs-healthier-families-recipes", "https://www.nhs.uk/our-policies/terms-and-conditions/", "복사일과 개별 원문 링크를 기록하며 7일이 지난 사본은 다시 수집하기 전 게시 후보에서 제외합니다.", "최소 7일마다 갱신, NHS 권장 주기는 24시간", new DateTime(2026, 7, 21, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 5L, "Metadata link only", "공식 원문 링크: {url} (확인일 {date})", "MetadataOnly", "US", "미국 MyPlate Kitchen 레시피", "https://www.myplate.gov/myplate-kitchen/recipes", false, false, "이미지 저장·재사용 금지", "en", null, "항목별 확인 필요", "USDA MyPlate", true, new DateTime(2026, 7, 21, 0, 0, 0, 0, DateTimeKind.Utc), "usda-myplate-recipes", "https://www.usda.gov/policies-and-links", "연방정부·제3자 제공 레시피가 혼재하므로 항목별 권리 확인 전 링크 메타데이터만 저장합니다.", "수동 권리 검토 시 갱신", new DateTime(2026, 7, 21, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 6L, "Metadata link only", "공식 원문 링크: {url} (확인일 {date})", "MetadataOnly", "CA", "캐나다 Food Guide 레시피", "https://food-guide.canada.ca/en/recipes/", false, false, "이미지 저장·재사용 금지", "en", null, "항목별 확인 필요", "Health Canada", true, new DateTime(2026, 7, 21, 0, 0, 0, 0, DateTimeKind.Utc), "health-canada-recipes", "https://www.canada.ca/en/transparency/terms.html", "상업적 복제 허가와 제3자 제공 여부를 항목별로 확인하기 전 링크 메타데이터만 저장합니다.", "수동 권리 검토 시 갱신", new DateTime(2026, 7, 21, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 7L, "Metadata link only", "공식 원문 링크: {url} (확인일 {date})", "MetadataOnly", "FR", "프랑스 농업부 음식·레시피 자료", "https://agriculture.gouv.fr/recettes", false, false, "이미지 저장·재사용 금지", "fr", null, "항목별 확인 필요", "Ministère de l'Agriculture et de la Souveraineté alimentaire", true, new DateTime(2026, 7, 21, 0, 0, 0, 0, DateTimeKind.Utc), "france-agriculture-recipes", "https://agriculture.gouv.fr/mentions-legales", "재이용 범위와 사진 권리를 항목별로 확인하기 전 링크 메타데이터만 저장합니다.", "수동 권리 검토 시 갱신", new DateTime(2026, 7, 21, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_food_official_dishes_CountryCode_RegionName_ReviewState",
                table: "food_official_dishes",
                columns: new[] { "CountryCode", "RegionName", "ReviewState" });

            migrationBuilder.CreateIndex(
                name: "IX_food_official_dishes_DishKey",
                table: "food_official_dishes",
                column: "DishKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_food_official_dishes_Name",
                table: "food_official_dishes",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_food_official_recipe_collection_runs_RunKey",
                table: "food_official_recipe_collection_runs",
                column: "RunKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_food_official_recipe_collection_runs_SourceKey_StatusCode_St~",
                table: "food_official_recipe_collection_runs",
                columns: new[] { "SourceKey", "StatusCode", "StartedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_food_official_recipe_sources_CountryCode_AutomationState",
                table: "food_official_recipe_sources",
                columns: new[] { "CountryCode", "AutomationState" });

            migrationBuilder.CreateIndex(
                name: "IX_food_official_recipe_sources_SourceKey",
                table: "food_official_recipe_sources",
                column: "SourceKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_food_official_recipe_variants_ContentExpiresAtUtc_IsRemovedA~",
                table: "food_official_recipe_variants",
                columns: new[] { "ContentExpiresAtUtc", "IsRemovedAtSource" });

            migrationBuilder.CreateIndex(
                name: "IX_food_official_recipe_variants_DishId_LastCollectedAtUtc",
                table: "food_official_recipe_variants",
                columns: new[] { "DishId", "LastCollectedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_food_official_recipe_variants_FirstCollectionRunId",
                table: "food_official_recipe_variants",
                column: "FirstCollectionRunId");

            migrationBuilder.CreateIndex(
                name: "IX_food_official_recipe_variants_RecordKey",
                table: "food_official_recipe_variants",
                column: "RecordKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_food_official_recipe_variants_SourceId_ExternalId",
                table: "food_official_recipe_variants",
                columns: new[] { "SourceId", "ExternalId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "food_official_recipe_variants");

            migrationBuilder.DropTable(
                name: "food_official_dishes");

            migrationBuilder.DropTable(
                name: "food_official_recipe_collection_runs");

            migrationBuilder.DropTable(
                name: "food_official_recipe_sources");
        }
    }
}
