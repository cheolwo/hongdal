using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Ssalddel.Infrastructure.Persistence.AgriculturalFisheries.Migrations
{
    /// <inheritdoc />
    public partial class AddChinaImportedFoodManufacturerRegions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "agri_hs_usda_commodity_mappings",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MappingKey = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HsCode6 = table.Column<string>(type: "varchar(6)", maxLength: 6, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProductNameKo = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HsDescriptionEn = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UsdaCommodityDesc = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UsdaClassDesc = table.Column<string>(type: "varchar(180)", maxLength: 180, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UsdaUtilPracticeDesc = table.Column<string>(type: "varchar(180)", maxLength: 180, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UsdaProductionPracticeDesc = table.Column<string>(type: "varchar(180)", maxLength: 180, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MatchQualityCode = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReviewStatusCode = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReviewOwnerUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReviewNote = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SourceUrl = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agri_hs_usda_commodity_mappings", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "agri_usda_nass_collection_runs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RunKey = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StatusCode = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    YearFrom = table.Column<int>(type: "int", nullable: false),
                    QuerySummary = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SourceUrl = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StartedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    LatestSourceLoadTimeUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    FetchedCount = table.Column<int>(type: "int", nullable: false),
                    InsertedCount = table.Column<int>(type: "int", nullable: false),
                    ExistingCount = table.Column<int>(type: "int", nullable: false),
                    ErrorMessage = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agri_usda_nass_collection_runs", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "agri_usda_nass_price_observations",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FirstCollectionRunId = table.Column<long>(type: "bigint", nullable: false),
                    RecordKey = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SourceDesc = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SectorDesc = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GroupDesc = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CommodityDesc = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ClassDesc = table.Column<string>(type: "varchar(180)", maxLength: 180, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UtilPracticeDesc = table.Column<string>(type: "varchar(180)", maxLength: 180, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProductionPracticeDesc = table.Column<string>(type: "varchar(180)", maxLength: 180, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StatisticCategoryDesc = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UnitDesc = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ShortDesc = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DomainDesc = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DomainCategoryDesc = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AggregationLevelDesc = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CountryCode = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CountryName = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Year = table.Column<int>(type: "int", nullable: false),
                    FrequencyDesc = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BeginCode = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EndCode = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReferencePeriodDesc = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ValueRaw = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NumericValue = table.Column<decimal>(type: "decimal(24,6)", precision: 24, scale: 6, nullable: true),
                    IsSuppressed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CvPercentRaw = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SourceLoadTimeUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    SourceUrl = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RawJson = table.Column<string>(type: "json", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FirstCollectedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    LastSeenAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agri_usda_nass_price_observations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_agri_usda_nass_price_observations_agri_usda_nass_collection_~",
                        column: x => x.FirstCollectionRunId,
                        principalTable: "agri_usda_nass_collection_runs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_agri_hs_usda_commodity_mappings_HsCode6_IsActive",
                table: "agri_hs_usda_commodity_mappings",
                columns: new[] { "HsCode6", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_agri_hs_usda_commodity_mappings_MappingKey",
                table: "agri_hs_usda_commodity_mappings",
                column: "MappingKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_agri_hs_usda_commodity_mappings_ReviewStatusCode_IsActive",
                table: "agri_hs_usda_commodity_mappings",
                columns: new[] { "ReviewStatusCode", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_agri_hs_usda_commodity_mappings_UsdaCommodityDesc_IsActive",
                table: "agri_hs_usda_commodity_mappings",
                columns: new[] { "UsdaCommodityDesc", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_agri_usda_nass_collection_runs_RunKey",
                table: "agri_usda_nass_collection_runs",
                column: "RunKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_agri_usda_nass_collection_runs_StatusCode_StartedAtUtc",
                table: "agri_usda_nass_collection_runs",
                columns: new[] { "StatusCode", "StartedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_agri_usda_nass_price_observations_CommodityDesc_Year_Frequen~",
                table: "agri_usda_nass_price_observations",
                columns: new[] { "CommodityDesc", "Year", "FrequencyDesc", "ReferencePeriodDesc" });

            migrationBuilder.CreateIndex(
                name: "IX_agri_usda_nass_price_observations_FirstCollectionRunId",
                table: "agri_usda_nass_price_observations",
                column: "FirstCollectionRunId");

            migrationBuilder.CreateIndex(
                name: "IX_agri_usda_nass_price_observations_LastSeenAtUtc",
                table: "agri_usda_nass_price_observations",
                column: "LastSeenAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_agri_usda_nass_price_observations_RecordKey",
                table: "agri_usda_nass_price_observations",
                column: "RecordKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_agri_usda_nass_price_observations_SourceLoadTimeUtc",
                table: "agri_usda_nass_price_observations",
                column: "SourceLoadTimeUtc");

            migrationBuilder.CreateTable(
                name: "agri_kamis_price_collection_runs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RunKey = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StatusCode = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RequestedDate = table.Column<DateOnly>(type: "date", nullable: false),
                    LatestSurveyDate = table.Column<DateOnly>(type: "date", nullable: true),
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
                    ErrorMessage = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agri_kamis_price_collection_runs", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "agri_kamis_price_observations",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FirstCollectionRunId = table.Column<long>(type: "bigint", nullable: false),
                    RecordKey = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProductClassCode = table.Column<string>(type: "varchar(2)", maxLength: 2, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProductClassName = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CategoryCode = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CategoryName = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CountryCode = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CountryName = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RequestedDate = table.Column<DateOnly>(type: "date", nullable: false),
                    SurveyDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ItemName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ItemCode = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    KindName = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    KindCode = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RankName = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RankCode = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Unit = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PriceRaw = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PriceKrw = table.Column<decimal>(type: "decimal(20,4)", precision: 20, scale: 4, nullable: true),
                    PreviousDayLabel = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PreviousDayPriceKrw = table.Column<decimal>(type: "decimal(20,4)", precision: 20, scale: 4, nullable: true),
                    OneWeekAgoLabel = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OneWeekAgoPriceKrw = table.Column<decimal>(type: "decimal(20,4)", precision: 20, scale: 4, nullable: true),
                    TwoWeeksAgoLabel = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TwoWeeksAgoPriceKrw = table.Column<decimal>(type: "decimal(20,4)", precision: 20, scale: 4, nullable: true),
                    OneMonthAgoLabel = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OneMonthAgoPriceKrw = table.Column<decimal>(type: "decimal(20,4)", precision: 20, scale: 4, nullable: true),
                    OneYearAgoLabel = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OneYearAgoPriceKrw = table.Column<decimal>(type: "decimal(20,4)", precision: 20, scale: 4, nullable: true),
                    NormalYearLabel = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NormalYearPriceKrw = table.Column<decimal>(type: "decimal(20,4)", precision: 20, scale: 4, nullable: true),
                    IsPriceMissing = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    SourceUrl = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RawJson = table.Column<string>(type: "json", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FirstCollectedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    LastSeenAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agri_kamis_price_observations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_agri_kamis_price_observations_agri_kamis_price_collection_ru~",
                        column: x => x.FirstCollectionRunId,
                        principalTable: "agri_kamis_price_collection_runs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_agri_kamis_price_collection_runs_RequestedDate",
                table: "agri_kamis_price_collection_runs",
                column: "RequestedDate");

            migrationBuilder.CreateIndex(
                name: "IX_agri_kamis_price_collection_runs_RunKey",
                table: "agri_kamis_price_collection_runs",
                column: "RunKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_agri_kamis_price_collection_runs_StatusCode_StartedAtUtc",
                table: "agri_kamis_price_collection_runs",
                columns: new[] { "StatusCode", "StartedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_agri_kamis_price_observations_FirstCollectionRunId",
                table: "agri_kamis_price_observations",
                column: "FirstCollectionRunId");

            migrationBuilder.CreateIndex(
                name: "IX_agri_kamis_price_observations_ItemName_SurveyDate",
                table: "agri_kamis_price_observations",
                columns: new[] { "ItemName", "SurveyDate" });

            migrationBuilder.CreateIndex(
                name: "IX_agri_kamis_price_observations_LastSeenAtUtc",
                table: "agri_kamis_price_observations",
                column: "LastSeenAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_agri_kamis_price_observations_RecordKey",
                table: "agri_kamis_price_observations",
                column: "RecordKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_agri_kamis_price_observations_SurveyDate_ProductClassCode_Ca~",
                table: "agri_kamis_price_observations",
                columns: new[] { "SurveyDate", "ProductClassCode", "CategoryCode", "ItemCode" });

            migrationBuilder.DropIndex(
                name: "IX_agri_kamis_price_observations_SurveyDate_ProductClassCode_Ca~",
                table: "agri_kamis_price_observations");

            migrationBuilder.AddColumn<string>(
                name: "FrequencyCode",
                table: "agri_kamis_price_observations",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Daily")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_agri_kamis_price_observations_SurveyDate_FrequencyCode_Produ~",
                table: "agri_kamis_price_observations",
                columns: new[] { "SurveyDate", "FrequencyCode", "ProductClassCode", "CategoryCode", "ItemCode" });

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

            migrationBuilder.CreateTable(
                name: "food_official_ingredient_price_mappings",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IngredientId = table.Column<long>(type: "bigint", nullable: false),
                    CountryCode = table.Column<string>(type: "varchar(2)", maxLength: 2, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SourceKey = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ExternalCategoryCode = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ExternalItemCode = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ExternalItemName = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ExternalVariantCode = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ExternalVariantName = table.Column<string>(type: "varchar(180)", maxLength: 180, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MatchMethod = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MatchQualityCode = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MatchConfidence = table.Column<decimal>(type: "decimal(5,4)", precision: 5, scale: 4, nullable: false),
                    MappingState = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MappingNote = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SourceUrl = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_food_official_ingredient_price_mappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_food_official_ingredient_price_mappings_food_official_ingred~",
                        column: x => x.IngredientId,
                        principalTable: "food_official_ingredients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_food_official_ingredient_price_mappings_IngredientId_Country~",
                table: "food_official_ingredient_price_mappings",
                columns: new[] { "IngredientId", "CountryCode", "SourceKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_food_official_ingredient_price_mappings_MappingState_IsActive",
                table: "food_official_ingredient_price_mappings",
                columns: new[] { "MappingState", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_food_official_ingredient_price_mappings_SourceKey_ExternalIt~",
                table: "food_official_ingredient_price_mappings",
                columns: new[] { "SourceKey", "ExternalItemCode", "IsActive" });

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

            migrationBuilder.AddColumn<string>(
                name: "ManufacturerRegionClassificationMethod",
                table: "food_ingredient_company_evidence",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ManufacturerRegionCode",
                table: "food_ingredient_company_evidence",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "ManufacturerRegionConfidence",
                table: "food_ingredient_company_evidence",
                type: "decimal(5,4)",
                precision: 5,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ManufacturerRegionEvidence",
                table: "food_ingredient_company_evidence",
                type: "varchar(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ManufacturerRegionName",
                table: "food_ingredient_company_evidence",
                type: "varchar(160)",
                maxLength: 160,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ManufacturerRegionScope",
                table: "food_ingredient_company_evidence",
                type: "varchar(800)",
                maxLength: 800,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_food_ingredient_company_evidence_CountryCode_ManufacturerReg~",
                table: "food_ingredient_company_evidence",
                columns: new[] { "CountryCode", "ManufacturerRegionCode", "IsCurrent" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_food_ingredient_company_evidence_CountryCode_ManufacturerReg~",
                table: "food_ingredient_company_evidence");

            migrationBuilder.DropColumn(
                name: "ManufacturerRegionClassificationMethod",
                table: "food_ingredient_company_evidence");

            migrationBuilder.DropColumn(
                name: "ManufacturerRegionCode",
                table: "food_ingredient_company_evidence");

            migrationBuilder.DropColumn(
                name: "ManufacturerRegionConfidence",
                table: "food_ingredient_company_evidence");

            migrationBuilder.DropColumn(
                name: "ManufacturerRegionEvidence",
                table: "food_ingredient_company_evidence");

            migrationBuilder.DropColumn(
                name: "ManufacturerRegionName",
                table: "food_ingredient_company_evidence");

            migrationBuilder.DropColumn(
                name: "ManufacturerRegionScope",
                table: "food_ingredient_company_evidence");

            migrationBuilder.DropTable(
                name: "food_official_ingredient_hs_mappings");

            migrationBuilder.DropTable(
                name: "food_ingredient_company_evidence");

            migrationBuilder.DropTable(
                name: "food_ingredient_company_profiles");

            migrationBuilder.DropTable(
                name: "food_ingredient_company_source_observations");

            migrationBuilder.DropTable(
                name: "food_ingredient_company_research_runs");

            migrationBuilder.DropTable(
                name: "food_official_ingredient_price_mappings");

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

            migrationBuilder.DropTable(
                name: "food_official_recipe_variants");

            migrationBuilder.DropTable(
                name: "food_official_dishes");

            migrationBuilder.DropTable(
                name: "food_official_recipe_collection_runs");

            migrationBuilder.DropTable(
                name: "food_official_recipe_sources");

            migrationBuilder.DropIndex(
                name: "IX_agri_kamis_price_observations_SurveyDate_FrequencyCode_Produ~",
                table: "agri_kamis_price_observations");

            migrationBuilder.DropColumn(
                name: "FrequencyCode",
                table: "agri_kamis_price_observations");

            migrationBuilder.CreateIndex(
                name: "IX_agri_kamis_price_observations_SurveyDate_ProductClassCode_Ca~",
                table: "agri_kamis_price_observations",
                columns: new[] { "SurveyDate", "ProductClassCode", "CategoryCode", "ItemCode" });

            migrationBuilder.DropTable(
                name: "agri_kamis_price_observations");

            migrationBuilder.DropTable(
                name: "agri_kamis_price_collection_runs");

            migrationBuilder.DropTable(
                name: "agri_hs_usda_commodity_mappings");

            migrationBuilder.DropTable(
                name: "agri_usda_nass_price_observations");

            migrationBuilder.DropTable(
                name: "agri_usda_nass_collection_runs");
        }
    }
}
