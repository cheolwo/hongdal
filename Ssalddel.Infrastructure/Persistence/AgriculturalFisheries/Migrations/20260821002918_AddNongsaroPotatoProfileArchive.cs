using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssalddel.Infrastructure.Persistence.AgriculturalFisheries.Migrations
{
    /// <inheritdoc />
    public partial class AddNongsaroPotatoProfileArchive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "agri_nongsaro_potato_profiles",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    StableId = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Revision = table.Column<int>(type: "int", nullable: false),
                    CanonicalProductStableId = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    WorkScheduleGroupCode = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    WorkScheduleContentNo = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProductRelationStatusCode = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReviewStatusCode = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ApprovedForSimulationContext = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ProfileJson = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SourceSetHashSha256 = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DisasterPreventionHashSha256 = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DisasterPreventionRetrievedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    RetrievedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ArchivedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ApprovedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agri_nongsaro_potato_profiles", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_agri_nongsaro_potato_profiles_CanonicalProductStableId_Appro~",
                table: "agri_nongsaro_potato_profiles",
                columns: new[] { "CanonicalProductStableId", "ApprovedForSimulationContext", "RetrievedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_agri_nongsaro_potato_profiles_StableId_Revision",
                table: "agri_nongsaro_potato_profiles",
                columns: new[] { "StableId", "Revision" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "agri_nongsaro_potato_profiles");
        }
    }
}
