using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Ssalddel.Infrastructure.Persistence.PublicData.Migrations
{
    /// <inheritdoc />
    public partial class SeedWorldBankCountryRegionMappings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "public_data_region_mappings",
                columns: new[] { "Id", "ExternalRegionCode", "MappingRevision", "RegionStableId", "SourceId", "SpatialPrecisionCode", "ValidFromUtc", "ValidToUtc" },
                values: new object[,]
                {
                    { -1003L, "CHN", "iso3166-1-alpha3-v2026-08", "country:cn", "world-bank-indicators", "country", null, null },
                    { -1002L, "USA", "iso3166-1-alpha3-v2026-08", "country:us", "world-bank-indicators", "country", null, null },
                    { -1001L, "KOR", "iso3166-1-alpha3-v2026-08", "country:kr", "world-bank-indicators", "country", null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "public_data_region_mappings",
                keyColumn: "Id",
                keyValue: -1003L);

            migrationBuilder.DeleteData(
                table: "public_data_region_mappings",
                keyColumn: "Id",
                keyValue: -1002L);

            migrationBuilder.DeleteData(
                table: "public_data_region_mappings",
                keyColumn: "Id",
                keyValue: -1001L);
        }
    }
}
