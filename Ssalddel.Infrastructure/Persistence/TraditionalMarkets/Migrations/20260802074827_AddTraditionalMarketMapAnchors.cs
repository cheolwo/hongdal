using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssalddel.Infrastructure.Persistence.TraditionalMarkets.Migrations
{
    /// <inheritdoc />
    public partial class AddTraditionalMarketMapAnchors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "MapLatitude",
                table: "traditional_market_logistics_hubs",
                type: "decimal(10,7)",
                precision: 10,
                scale: 7,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MapLocationPrecisionCode",
                table: "traditional_market_logistics_hubs",
                type: "varchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "MapLocationSourceHref",
                table: "traditional_market_logistics_hubs",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "MapLocationSourceName",
                table: "traditional_market_logistics_hubs",
                type: "varchar(160)",
                maxLength: 160,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "MapLocationVerifiedAtUtc",
                table: "traditional_market_logistics_hubs",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MapLocationVerifiedByUserId",
                table: "traditional_market_logistics_hubs",
                type: "varchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "MapLongitude",
                table: "traditional_market_logistics_hubs",
                type: "decimal(10,7)",
                precision: 10,
                scale: 7,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_traditional_market_logistics_hubs_Status_MapLocationVerified~",
                table: "traditional_market_logistics_hubs",
                columns: new[] { "Status", "MapLocationVerifiedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_traditional_market_logistics_hubs_Status_MapLocationVerified~",
                table: "traditional_market_logistics_hubs");

            migrationBuilder.DropColumn(
                name: "MapLatitude",
                table: "traditional_market_logistics_hubs");

            migrationBuilder.DropColumn(
                name: "MapLocationPrecisionCode",
                table: "traditional_market_logistics_hubs");

            migrationBuilder.DropColumn(
                name: "MapLocationSourceHref",
                table: "traditional_market_logistics_hubs");

            migrationBuilder.DropColumn(
                name: "MapLocationSourceName",
                table: "traditional_market_logistics_hubs");

            migrationBuilder.DropColumn(
                name: "MapLocationVerifiedAtUtc",
                table: "traditional_market_logistics_hubs");

            migrationBuilder.DropColumn(
                name: "MapLocationVerifiedByUserId",
                table: "traditional_market_logistics_hubs");

            migrationBuilder.DropColumn(
                name: "MapLongitude",
                table: "traditional_market_logistics_hubs");
        }
    }
}
