using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssalddel.Infrastructure.Persistence.AgriculturalFisheries.Migrations
{
    /// <inheritdoc />
    public partial class OptimizeKamisAmsComparisonLookup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "agri_usda_ams_year_commodity_catalog",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Commodity = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FirstObservedDate = table.Column<DateOnly>(type: "date", nullable: false),
                    LastObservedDate = table.Column<DateOnly>(type: "date", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agri_usda_ams_year_commodity_catalog", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_agri_kamis_price_observations_ItemCode_FrequencyCode_Product~",
                table: "agri_kamis_price_observations",
                columns: new[] { "ItemCode", "FrequencyCode", "ProductClassCode", "KindCode", "RankCode", "Unit", "SurveyDate", "Id" },
                descending: new[] { false, false, false, false, false, false, true, true });

            migrationBuilder.CreateIndex(
                name: "IX_agri_usda_ams_year_commodity_catalog_Commodity_Year",
                table: "agri_usda_ams_year_commodity_catalog",
                columns: new[] { "Commodity", "Year" });

            migrationBuilder.CreateIndex(
                name: "IX_agri_usda_ams_year_commodity_catalog_Year_Commodity",
                table: "agri_usda_ams_year_commodity_catalog",
                columns: new[] { "Year", "Commodity" },
                unique: true);

            migrationBuilder.Sql(
                """
                INSERT INTO `agri_usda_ams_year_commodity_catalog`
                    (`Year`, `Commodity`, `FirstObservedDate`, `LastObservedDate`, `UpdatedAtUtc`)
                SELECT
                    YEAR(`ReportBeginDate`),
                    `Commodity`,
                    MIN(`ReportBeginDate`),
                    MAX(`ReportBeginDate`),
                    UTC_TIMESTAMP(6)
                FROM `agri_usda_ams_market_price_observations`
                WHERE `Commodity` <> ''
                  AND `Commodity` <> 'N/A'
                GROUP BY YEAR(`ReportBeginDate`), `Commodity`
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "agri_usda_ams_year_commodity_catalog");

            migrationBuilder.DropIndex(
                name: "IX_agri_kamis_price_observations_ItemCode_FrequencyCode_Product~",
                table: "agri_kamis_price_observations");
        }
    }
}
