using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssalddel.Infrastructure.Persistence.TraditionalMarkets.Migrations
{
    /// <inheritdoc />
    public partial class AddTraditionalMarketNeighborhoodCouncil : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "public_data_traditional_market_sync_runs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Status = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SourceDatasetKey = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SourceReferenceDate = table.Column<DateOnly>(type: "date", nullable: false),
                    FetchedCount = table.Column<int>(type: "int", nullable: false),
                    InsertedCount = table.Column<int>(type: "int", nullable: false),
                    UpdatedCount = table.Column<int>(type: "int", nullable: false),
                    UnchangedCount = table.Column<int>(type: "int", nullable: false),
                    DeactivatedCount = table.Column<int>(type: "int", nullable: false),
                    ErrorMessage = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StartedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_public_data_traditional_market_sync_runs", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "public_data_traditional_markets",
                columns: table => new
                {
                    MarketCode = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MarketType = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LotNumberAddress = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RoadAddress = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Province = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CityCounty = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    has_arcade = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    has_elevator_or_escalator = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    has_customer_support_center = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    has_sprinkler = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    has_fire_detector = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    has_children_playroom = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    has_call_center = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    has_customer_lounge = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    has_nursing_center = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    has_locker = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    has_bicycle_storage = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    has_sports_facility = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    has_library = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    has_shopping_cart = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    has_foreign_visitor_center = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    has_customer_path = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    has_broadcast_center = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    has_culture_classroom = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    has_shared_logistics_warehouse = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    has_dedicated_parking = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    has_training_room = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    has_meeting_room = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    has_aed = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    SourceDatasetKey = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SourceReferenceDate = table.Column<DateOnly>(type: "date", nullable: false),
                    SourceHash = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    LastSyncedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_public_data_traditional_markets", x => x.MarketCode);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_public_data_traditional_market_sync_runs_SourceDatasetKey_St~",
                table: "public_data_traditional_market_sync_runs",
                columns: new[] { "SourceDatasetKey", "StartedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_public_data_traditional_market_sync_runs_Status",
                table: "public_data_traditional_market_sync_runs",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_public_data_traditional_markets_Name",
                table: "public_data_traditional_markets",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_public_data_traditional_markets_Province_CityCounty_IsActive",
                table: "public_data_traditional_markets",
                columns: new[] { "Province", "CityCounty", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_public_data_traditional_markets_SourceDatasetKey_SourceRefer~",
                table: "public_data_traditional_markets",
                columns: new[] { "SourceDatasetKey", "SourceReferenceDate" });

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

            migrationBuilder.CreateTable(
                name: "traditional_market_neighborhood_councils",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    MarketCode = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CouncilName = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ApartmentCommunityName = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ApartmentAddress = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ApartmentRepresentativeUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ApartmentRepresentativeName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ApartmentRepresentativeAcceptedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    MerchantAssociationName = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MerchantRepresentativeUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MerchantRepresentativeName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MerchantRepresentativeAcceptedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Purpose = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedByUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Revision = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_traditional_market_neighborhood_councils", x => x.Id);
                    table.ForeignKey(
                        name: "FK_traditional_market_neighborhood_councils_public_data_traditi~",
                        column: x => x.MarketCode,
                        principalTable: "public_data_traditional_markets",
                        principalColumn: "MarketCode",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "traditional_market_trade_agendas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CouncilId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TradeDirection = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ItemName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ItemDescription = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RequestedQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    QuantityUnit = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OriginCountry = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DestinationCountry = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DesiredStartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    DesiredEndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    LogisticsTerms = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EstimatedAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    CurrencyCode = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RequiresCustomsReview = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ProposalText = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ApartmentDecision = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ApartmentDecisionMemo = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ApartmentDecidedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    MerchantDecision = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MerchantDecisionMemo = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MerchantDecidedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Revision = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_traditional_market_trade_agendas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_traditional_market_trade_agendas_traditional_market_neighbor~",
                        column: x => x.CouncilId,
                        principalTable: "traditional_market_neighborhood_councils",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_traditional_market_neighborhood_councils_ApartmentRepresenta~",
                table: "traditional_market_neighborhood_councils",
                columns: new[] { "ApartmentRepresentativeUserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_traditional_market_neighborhood_councils_MarketCode_Status",
                table: "traditional_market_neighborhood_councils",
                columns: new[] { "MarketCode", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_traditional_market_neighborhood_councils_MerchantRepresentat~",
                table: "traditional_market_neighborhood_councils",
                columns: new[] { "MerchantRepresentativeUserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_traditional_market_trade_agendas_CouncilId_Status_UpdatedAtU~",
                table: "traditional_market_trade_agendas",
                columns: new[] { "CouncilId", "Status", "UpdatedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "traditional_market_trade_agendas");

            migrationBuilder.DropTable(
                name: "traditional_market_neighborhood_councils");

            migrationBuilder.DropTable(
                name: "traditional_market_logistics_hubs");

            migrationBuilder.DropTable(
                name: "public_data_traditional_market_sync_runs");

            migrationBuilder.DropTable(
                name: "public_data_traditional_markets");
        }
    }
}
