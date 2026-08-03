using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssalddel.Infrastructure.Persistence.AgriculturalFisheries.Migrations
{
    /// <inheritdoc />
    public partial class OptimizeRegionalAgriculturalMapMarkerLookup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_agri_ams_map_market_lookup",
                table: "agri_usda_ams_market_price_observations",
                columns: new[] { "SourceKey", "MarketLocationState", "MarketLocationName", "MarketType", "ReportBeginDate", "ReportEndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_agri_ams_map_shipping_lookup",
                table: "agri_usda_ams_market_price_observations",
                columns: new[] { "MarketType", "SourceKey", "District", "ReportBeginDate", "ReportEndDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_agri_ams_map_market_lookup",
                table: "agri_usda_ams_market_price_observations");

            migrationBuilder.DropIndex(
                name: "IX_agri_ams_map_shipping_lookup",
                table: "agri_usda_ams_market_price_observations");
        }
    }
}
