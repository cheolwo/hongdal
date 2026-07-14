using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hongdal.Infrastructure.Persistence.TraditionalMarkets.Migrations
{
    /// <inheritdoc />
    public partial class AddTraditionalMarketPublicDataModule : Migration
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "public_data_traditional_market_sync_runs");

            migrationBuilder.DropTable(
                name: "public_data_traditional_markets");
        }
    }
}
