using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hongdal.Migrations
{
    /// <inheritdoc />
    public partial class AddHsCodeRiskTags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "hs_code_entry_risk_tags",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    HsCodeEntryId = table.Column<long>(type: "bigint", nullable: false),
                    TagType = table.Column<int>(type: "int", nullable: false),
                    Label = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Reason = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Source = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hs_code_entry_risk_tags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hs_code_entry_risk_tags_hs_code_entries_HsCodeEntryId",
                        column: x => x.HsCodeEntryId,
                        principalTable: "hs_code_entries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.Sql("""
                INSERT INTO hs_code_entry_risk_tags
                    (HsCodeEntryId, TagType, Label, Reason, Source, IsActive, CreatedAtUtc, UpdatedAtUtc)
                SELECT Id, 10, '식품 관련', 'HS chapter 01-24 is treated as food or food-adjacent cargo.', 10, TRUE, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6)
                FROM hs_code_entries
                WHERE NormalizedCode REGEXP '^[0-9]{2}'
                  AND CAST(SUBSTRING(NormalizedCode, 1, 2) AS UNSIGNED) BETWEEN 1 AND 24;
                """);

            migrationBuilder.Sql("""
                INSERT INTO hs_code_entry_risk_tags
                    (HsCodeEntryId, TagType, Label, Reason, Source, IsActive, CreatedAtUtc, UpdatedAtUtc)
                SELECT Id, 20, '검역/식품신고 확인', 'Food-related HS codes may require quarantine, ingredient, label, or import notification review.', 10, TRUE, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6)
                FROM hs_code_entries
                WHERE NormalizedCode REGEXP '^[0-9]{2}'
                  AND CAST(SUBSTRING(NormalizedCode, 1, 2) AS UNSIGNED) BETWEEN 1 AND 24;
                """);

            migrationBuilder.Sql("""
                INSERT INTO hs_code_entry_risk_tags
                    (HsCodeEntryId, TagType, Label, Reason, Source, IsActive, CreatedAtUtc, UpdatedAtUtc)
                SELECT Id, 30, '조제식품/보충제 검토', 'Chapter 21 can include prepared food products that need ingredient and claim review.', 10, TRUE, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6)
                FROM hs_code_entries
                WHERE NormalizedCode REGEXP '^[0-9]{2}'
                  AND CAST(SUBSTRING(NormalizedCode, 1, 2) AS UNSIGNED) = 21;
                """);

            migrationBuilder.Sql("""
                INSERT INTO hs_code_entry_risk_tags
                    (HsCodeEntryId, TagType, Label, Reason, Source, IsActive, CreatedAtUtc, UpdatedAtUtc)
                SELECT Id, 50, '화학물질 확인', 'Chemical chapters may require substance, safety, or hazardous cargo review.', 10, TRUE, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6)
                FROM hs_code_entries
                WHERE NormalizedCode REGEXP '^[0-9]{2}'
                  AND CAST(SUBSTRING(NormalizedCode, 1, 2) AS UNSIGNED) BETWEEN 28 AND 38;
                """);

            migrationBuilder.Sql("""
                INSERT INTO hs_code_entry_risk_tags
                    (HsCodeEntryId, TagType, Label, Reason, Source, IsActive, CreatedAtUtc, UpdatedAtUtc)
                SELECT Id, 40, '섬유/의류', 'Textile chapters often need material composition and origin checks.', 10, TRUE, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6)
                FROM hs_code_entries
                WHERE NormalizedCode REGEXP '^[0-9]{2}'
                  AND CAST(SUBSTRING(NormalizedCode, 1, 2) AS UNSIGNED) BETWEEN 50 AND 63;
                """);

            migrationBuilder.Sql("""
                INSERT INTO hs_code_entry_risk_tags
                    (HsCodeEntryId, TagType, Label, Reason, Source, IsActive, CreatedAtUtc, UpdatedAtUtc)
                SELECT Id, 60, '전기/인증 확인', 'Electrical goods may require certification, radio, or product safety checks.', 10, TRUE, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6)
                FROM hs_code_entries
                WHERE NormalizedCode REGEXP '^[0-9]{2}'
                  AND CAST(SUBSTRING(NormalizedCode, 1, 2) AS UNSIGNED) = 85;
                """);

            migrationBuilder.Sql("""
                INSERT INTO hs_code_entry_risk_tags
                    (HsCodeEntryId, TagType, Label, Reason, Source, IsActive, CreatedAtUtc, UpdatedAtUtc)
                SELECT Id, 70, '배터리 포함 가능', 'Battery-related HS codes need transport and safety document checks.', 10, TRUE, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6)
                FROM hs_code_entries
                WHERE NormalizedCode LIKE '8506%'
                   OR NormalizedCode LIKE '8507%';
                """);

            migrationBuilder.Sql("""
                INSERT INTO hs_code_entry_risk_tags
                    (HsCodeEntryId, TagType, Label, Reason, Source, IsActive, CreatedAtUtc, UpdatedAtUtc)
                SELECT Id, 80, '가구/생활용품', 'Furniture and fixture chapters may need material and component checks.', 10, TRUE, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6)
                FROM hs_code_entries
                WHERE NormalizedCode REGEXP '^[0-9]{2}'
                  AND CAST(SUBSTRING(NormalizedCode, 1, 2) AS UNSIGNED) = 94;
                """);

            migrationBuilder.Sql("""
                INSERT INTO hs_code_entry_risk_tags
                    (HsCodeEntryId, TagType, Label, Reason, Source, IsActive, CreatedAtUtc, UpdatedAtUtc)
                SELECT Id, 900, '관세사 검토 권장', 'At least one operational risk tag was detected, so broker review is recommended before agency confirmation.', 10, TRUE, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6)
                FROM hs_code_entries
                WHERE NormalizedCode REGEXP '^[0-9]{2}'
                  AND (
                    CAST(SUBSTRING(NormalizedCode, 1, 2) AS UNSIGNED) BETWEEN 1 AND 24
                    OR CAST(SUBSTRING(NormalizedCode, 1, 2) AS UNSIGNED) BETWEEN 28 AND 38
                    OR CAST(SUBSTRING(NormalizedCode, 1, 2) AS UNSIGNED) BETWEEN 50 AND 63
                    OR CAST(SUBSTRING(NormalizedCode, 1, 2) AS UNSIGNED) = 85
                    OR CAST(SUBSTRING(NormalizedCode, 1, 2) AS UNSIGNED) = 94
                  );
                """);

            migrationBuilder.CreateIndex(
                name: "IX_hs_code_entry_risk_tags_HsCodeEntryId_TagType_IsActive",
                table: "hs_code_entry_risk_tags",
                columns: new[] { "HsCodeEntryId", "TagType", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_hs_code_entry_risk_tags_TagType_IsActive",
                table: "hs_code_entry_risk_tags",
                columns: new[] { "TagType", "IsActive" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "hs_code_entry_risk_tags");
        }
    }
}
