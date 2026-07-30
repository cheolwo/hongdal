using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssalddel.Infrastructure.Persistence.AgriculturalFisheries.Migrations
{
    /// <inheritdoc />
    public partial class OptimizeUsdaAmsArchiveLookup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_agri_usda_ams_market_price_observations_ReportBeginDate_Comm~",
                table: "agri_usda_ams_market_price_observations",
                columns: new[] { "ReportBeginDate", "Commodity", "MarketLocationName" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_agri_usda_ams_market_price_observations_ReportBeginDate_Comm~",
                table: "agri_usda_ams_market_price_observations");
        }
    }
}
