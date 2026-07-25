using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssalddel.Infrastructure.Persistence.AgriculturalFisheries.Migrations
{
    /// <inheritdoc />
    public partial class AddDomesticAgriculturalRegionalPriceComparisonIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_AuctionPrice_Item_Date_Market",
                table: "agri_domestic_auction_price_observations",
                columns: new[] { "ItemName", "SettlementDate", "WholesaleMarketCode" });

            migrationBuilder.CreateIndex(
                name: "IX_AuctionPrice_Item_Date_Origin",
                table: "agri_domestic_auction_price_observations",
                columns: new[] { "ItemName", "SettlementDate", "OriginName" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AuctionPrice_Item_Date_Market",
                table: "agri_domestic_auction_price_observations");

            migrationBuilder.DropIndex(
                name: "IX_AuctionPrice_Item_Date_Origin",
                table: "agri_domestic_auction_price_observations");
        }
    }
}
