using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssalddel.Infrastructure.Persistence.AgriculturalFisheries.Migrations
{
    /// <inheritdoc />
    public partial class AddChinaImportedFoodManufacturerRegions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ManufacturerRegionClassificationMethod",
                table: "food_ingredient_company_evidence",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ManufacturerRegionCode",
                table: "food_ingredient_company_evidence",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "ManufacturerRegionConfidence",
                table: "food_ingredient_company_evidence",
                type: "decimal(5,4)",
                precision: 5,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ManufacturerRegionEvidence",
                table: "food_ingredient_company_evidence",
                type: "varchar(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ManufacturerRegionName",
                table: "food_ingredient_company_evidence",
                type: "varchar(160)",
                maxLength: 160,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ManufacturerRegionScope",
                table: "food_ingredient_company_evidence",
                type: "varchar(800)",
                maxLength: 800,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_food_ingredient_company_evidence_CountryCode_ManufacturerReg~",
                table: "food_ingredient_company_evidence",
                columns: new[] { "CountryCode", "ManufacturerRegionCode", "IsCurrent" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_food_ingredient_company_evidence_CountryCode_ManufacturerReg~",
                table: "food_ingredient_company_evidence");

            migrationBuilder.DropColumn(
                name: "ManufacturerRegionClassificationMethod",
                table: "food_ingredient_company_evidence");

            migrationBuilder.DropColumn(
                name: "ManufacturerRegionCode",
                table: "food_ingredient_company_evidence");

            migrationBuilder.DropColumn(
                name: "ManufacturerRegionConfidence",
                table: "food_ingredient_company_evidence");

            migrationBuilder.DropColumn(
                name: "ManufacturerRegionEvidence",
                table: "food_ingredient_company_evidence");

            migrationBuilder.DropColumn(
                name: "ManufacturerRegionName",
                table: "food_ingredient_company_evidence");

            migrationBuilder.DropColumn(
                name: "ManufacturerRegionScope",
                table: "food_ingredient_company_evidence");
        }
    }
}
