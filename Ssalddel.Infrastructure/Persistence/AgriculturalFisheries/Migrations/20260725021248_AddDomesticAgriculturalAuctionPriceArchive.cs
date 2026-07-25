using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssalddel.Infrastructure.Persistence.AgriculturalFisheries.Migrations
{
    /// <inheritdoc />
    public partial class AddDomesticAgriculturalAuctionPriceArchive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "agri_domestic_auction_price_collection_runs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RunKey = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SourceKey = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StatusCode = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SettlementDate = table.Column<DateOnly>(type: "date", nullable: false),
                    FetchedCount = table.Column<int>(type: "int", nullable: false),
                    InsertedCount = table.Column<int>(type: "int", nullable: false),
                    UpdatedCount = table.Column<int>(type: "int", nullable: false),
                    ExistingCount = table.Column<int>(type: "int", nullable: false),
                    CompletedPages = table.Column<int>(type: "int", nullable: false),
                    IsTruncated = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ErrorMessage = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StartedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agri_domestic_auction_price_collection_runs", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "agri_domestic_auction_price_observations",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FirstCollectionRunId = table.Column<long>(type: "bigint", nullable: false),
                    RecordKey = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SourceKey = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SettlementDate = table.Column<DateOnly>(type: "date", nullable: false),
                    WholesaleMarketCode = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CorporationCode = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SlipNumber = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AuctionSequence1 = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AuctionSequence2 = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TradingMethodCode = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LargeCategoryCode = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MiddleCategoryCode = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SmallCategoryCode = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CorporationItemCode = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ItemName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    VarietyName = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UnitWeight = table.Column<decimal>(type: "decimal(20,6)", precision: 20, scale: 6, nullable: true),
                    UnitCode = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PackageCode = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SizeCode = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GradeCode = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Quantity = table.Column<decimal>(type: "decimal(24,6)", precision: 24, scale: 6, nullable: true),
                    AuctionPriceKrw = table.Column<decimal>(type: "decimal(24,4)", precision: 24, scale: 4, nullable: true),
                    OriginCode = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OriginName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TotalQuantity = table.Column<decimal>(type: "decimal(24,6)", precision: 24, scale: 6, nullable: true),
                    TotalAmountKrw = table.Column<decimal>(type: "decimal(24,4)", precision: 24, scale: 4, nullable: true),
                    AwardedTime = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FirstCollectedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    LastSeenAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agri_domestic_auction_price_observations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_agri_domestic_auction_price_observations_agri_domestic_aucti~",
                        column: x => x.FirstCollectionRunId,
                        principalTable: "agri_domestic_auction_price_collection_runs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_agri_domestic_auction_price_collection_runs_RunKey",
                table: "agri_domestic_auction_price_collection_runs",
                column: "RunKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_agri_domestic_auction_price_collection_runs_SourceKey_Settle~",
                table: "agri_domestic_auction_price_collection_runs",
                columns: new[] { "SourceKey", "SettlementDate", "StatusCode" });

            migrationBuilder.CreateIndex(
                name: "IX_agri_domestic_auction_price_observations_FirstCollectionRunId",
                table: "agri_domestic_auction_price_observations",
                column: "FirstCollectionRunId");

            migrationBuilder.CreateIndex(
                name: "IX_agri_domestic_auction_price_observations_ItemName_VarietyNam~",
                table: "agri_domestic_auction_price_observations",
                columns: new[] { "ItemName", "VarietyName", "SettlementDate" });

            migrationBuilder.CreateIndex(
                name: "IX_agri_domestic_auction_price_observations_LastSeenAtUtc",
                table: "agri_domestic_auction_price_observations",
                column: "LastSeenAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_agri_domestic_auction_price_observations_RecordKey",
                table: "agri_domestic_auction_price_observations",
                column: "RecordKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_agri_domestic_auction_price_observations_SettlementDate_Whol~",
                table: "agri_domestic_auction_price_observations",
                columns: new[] { "SettlementDate", "WholesaleMarketCode", "CorporationCode", "ItemName" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "agri_domestic_auction_price_observations");

            migrationBuilder.DropTable(
                name: "agri_domestic_auction_price_collection_runs");
        }
    }
}
