using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssalddel.Infrastructure.Persistence.AgriculturalFisheries.Migrations
{
    /// <inheritdoc />
    public partial class AddUsdaAmsKamisComparisonLookupIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_agri_usda_ams_market_price_observations_Commodity_MarketStag~",
                table: "agri_usda_ams_market_price_observations",
                columns: new[] { "Commodity", "MarketStageCode", "ReportBeginDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_agri_usda_ams_market_price_observations_Commodity_MarketStag~",
                table: "agri_usda_ams_market_price_observations");
        }
    }
}
