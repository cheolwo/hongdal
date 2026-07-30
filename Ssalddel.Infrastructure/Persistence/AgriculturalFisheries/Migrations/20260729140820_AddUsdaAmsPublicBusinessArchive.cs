using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssalddel.Infrastructure.Persistence.AgriculturalFisheries.Migrations
{
    /// <inheritdoc />
    public partial class AddUsdaAmsPublicBusinessArchive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "agri_usda_ams_public_business_collection_runs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RunKey = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StatusCode = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RequestedDirectoryTypesJson = table.Column<string>(type: "json", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CompletedDirectoryCount = table.Column<int>(type: "int", nullable: false),
                    FetchedCount = table.Column<long>(type: "bigint", nullable: false),
                    InsertedCount = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedCount = table.Column<long>(type: "bigint", nullable: false),
                    UnchangedCount = table.Column<long>(type: "bigint", nullable: false),
                    NoLongerListedCount = table.Column<long>(type: "bigint", nullable: false),
                    RejectedCount = table.Column<long>(type: "bigint", nullable: false),
                    SourceUrl = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SourceMessagesJson = table.Column<string>(type: "json", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StartedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ErrorMessage = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agri_usda_ams_public_business_collection_runs", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "agri_usda_ams_public_business_profiles",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FirstCollectionRunId = table.Column<long>(type: "bigint", nullable: false),
                    LastCollectionRunId = table.Column<long>(type: "bigint", nullable: false),
                    ProfileKey = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SourceKey = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DirectoryTypeCode = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ExternalListingId = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BusinessName = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BusinessNameNormalized = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CityName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StateCode = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LocationPrecisionCode = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EstablishedYear = table.Column<int>(type: "int", nullable: true),
                    LegalStatus = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProductSummary = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HasRetailChannel = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    HasWholesaleChannel = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    HasProducerService = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    HasProcurementService = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsCurrentlyListed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    SourceUpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    OfficialListingUrl = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SourceFingerprint = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FirstSeenAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    LastSeenAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    LastChangedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agri_usda_ams_public_business_profiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_agri_usda_ams_public_business_profiles_agri_usda_ams_public_~",
                        column: x => x.FirstCollectionRunId,
                        principalTable: "agri_usda_ams_public_business_collection_runs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_agri_usda_ams_public_business_profiles_agri_usda_ams_public~1",
                        column: x => x.LastCollectionRunId,
                        principalTable: "agri_usda_ams_public_business_collection_runs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "agri_usda_ams_public_business_products",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ProfileId = table.Column<long>(type: "bigint", nullable: false),
                    ProductKey = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProductName = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agri_usda_ams_public_business_products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_agri_usda_ams_public_business_products_agri_usda_ams_public_~",
                        column: x => x.ProfileId,
                        principalTable: "agri_usda_ams_public_business_profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_agri_usda_ams_public_business_collection_runs_RunKey",
                table: "agri_usda_ams_public_business_collection_runs",
                column: "RunKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_agri_usda_ams_public_business_collection_runs_StatusCode_Sta~",
                table: "agri_usda_ams_public_business_collection_runs",
                columns: new[] { "StatusCode", "StartedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_agri_usda_ams_public_business_products_ProductKey_ProfileId",
                table: "agri_usda_ams_public_business_products",
                columns: new[] { "ProductKey", "ProfileId" });

            migrationBuilder.CreateIndex(
                name: "IX_agri_usda_ams_public_business_products_ProfileId_ProductKey",
                table: "agri_usda_ams_public_business_products",
                columns: new[] { "ProfileId", "ProductKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_agri_usda_ams_public_business_profiles_DirectoryTypeCode_Sta~",
                table: "agri_usda_ams_public_business_profiles",
                columns: new[] { "DirectoryTypeCode", "StateCode", "BusinessNameNormalized" });

            migrationBuilder.CreateIndex(
                name: "IX_agri_usda_ams_public_business_profiles_FirstCollectionRunId",
                table: "agri_usda_ams_public_business_profiles",
                column: "FirstCollectionRunId");

            migrationBuilder.CreateIndex(
                name: "IX_agri_usda_ams_public_business_profiles_LastCollectionRunId",
                table: "agri_usda_ams_public_business_profiles",
                column: "LastCollectionRunId");

            migrationBuilder.CreateIndex(
                name: "IX_agri_usda_ams_public_business_profiles_LastSeenAtUtc",
                table: "agri_usda_ams_public_business_profiles",
                column: "LastSeenAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_agri_usda_ams_public_business_profiles_ProfileKey",
                table: "agri_usda_ams_public_business_profiles",
                column: "ProfileKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_agri_usda_ams_public_business_profiles_SourceKey_DirectoryTy~",
                table: "agri_usda_ams_public_business_profiles",
                columns: new[] { "SourceKey", "DirectoryTypeCode", "ExternalListingId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_agri_usda_ams_public_business_profiles_StateCode_IsCurrently~",
                table: "agri_usda_ams_public_business_profiles",
                columns: new[] { "StateCode", "IsCurrentlyListed", "DirectoryTypeCode" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "agri_usda_ams_public_business_products");

            migrationBuilder.DropTable(
                name: "agri_usda_ams_public_business_profiles");

            migrationBuilder.DropTable(
                name: "agri_usda_ams_public_business_collection_runs");
        }
    }
}
