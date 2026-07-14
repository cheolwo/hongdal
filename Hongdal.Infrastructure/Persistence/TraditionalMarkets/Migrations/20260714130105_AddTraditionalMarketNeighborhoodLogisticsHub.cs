using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hongdal.Infrastructure.Persistence.TraditionalMarkets.Migrations
{
    /// <inheritdoc />
    public partial class AddTraditionalMarketNeighborhoodLogisticsHub : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "traditional_market_logistics_hubs",
                columns: table => new
                {
                    MarketCode = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OperatorOrganizationName = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ServiceRadiusKm = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: false),
                    DailyGroupPurchaseCapacity = table.Column<int>(type: "int", nullable: false),
                    SupportsBulkReceiving = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    SupportsSorting = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    SupportsResidentPickup = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    SupportsLastMileDelivery = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    SupportsRefrigeratedStorage = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    SupportsFrozenStorage = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ReceivingWindow = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PickupWindow = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OperatingNotes = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HasOperatorConsent = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    OperatorConsentedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    SiteVerifiedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    SiteVerifiedByUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StatusReason = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedByUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Revision = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    StatusChangedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_traditional_market_logistics_hubs", x => x.MarketCode);
                    table.ForeignKey(
                        name: "FK_traditional_market_logistics_hubs_public_data_traditional_ma~",
                        column: x => x.MarketCode,
                        principalTable: "public_data_traditional_markets",
                        principalColumn: "MarketCode",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_traditional_market_logistics_hubs_Status",
                table: "traditional_market_logistics_hubs",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_traditional_market_logistics_hubs_Status_UpdatedAtUtc",
                table: "traditional_market_logistics_hubs",
                columns: new[] { "Status", "UpdatedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "traditional_market_logistics_hubs");
        }
    }
}
