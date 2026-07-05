using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hongdal.Migrations
{
    /// <inheritdoc />
    public partial class AddHsCodeBusinessCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BusinessCategory",
                table: "hs_code_entries",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCategoryReason",
                table: "hs_code_entries",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.Sql("""
                UPDATE hs_code_entries
                SET BusinessCategory = CASE
                        WHEN CAST(SUBSTRING(NormalizedCode, 1, 2) AS UNSIGNED) BETWEEN 1 AND 24 THEN 10
                        WHEN NormalizedCode REGEXP '^[0-9]{2}' THEN 20
                        ELSE 0
                    END,
                    BusinessCategoryReason = CASE
                        WHEN CAST(SUBSTRING(NormalizedCode, 1, 2) AS UNSIGNED) BETWEEN 1 AND 24 THEN 'HS chapter 01-24 is treated as food or food-adjacent cargo.'
                        WHEN NormalizedCode REGEXP '^[0-9]{2}' THEN 'HS chapter is outside 01-24 and treated as general cargo.'
                        ELSE 'HS chapter could not be parsed.'
                    END;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_hs_code_entries_CatalogVersionId_BusinessCategory_IsActive",
                table: "hs_code_entries",
                columns: new[] { "CatalogVersionId", "BusinessCategory", "IsActive" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_hs_code_entries_CatalogVersionId_BusinessCategory_IsActive",
                table: "hs_code_entries");

            migrationBuilder.DropColumn(
                name: "BusinessCategory",
                table: "hs_code_entries");

            migrationBuilder.DropColumn(
                name: "BusinessCategoryReason",
                table: "hs_code_entries");
        }
    }
}
