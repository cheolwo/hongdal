using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssalddel.Infrastructure.Persistence.AgriculturalFisheries.Migrations
{
    /// <inheritdoc />
    public partial class AddKamisPriceUnitProvenance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ComparisonUnit",
                table: "agri_kamis_price_observations",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "PriceNormalizationBasis",
                table: "agri_kamis_price_observations",
                type: "varchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "PriceNormalizationCode",
                table: "agri_kamis_price_observations",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "SourcePackageLabel",
                table: "agri_kamis_price_observations",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.Sql(
                """
                UPDATE agri_kamis_price_observations
                SET ComparisonUnit = CASE
                        WHEN Unit = '' THEN '1kg'
                        ELSE Unit
                    END,
                    PriceNormalizationCode = CASE
                        WHEN Unit = '1kg' THEN 'KamisSourceKilogramConversion'
                        ELSE 'Unverified'
                    END,
                    PriceNormalizationBasis = CASE
                        WHEN Unit = '1kg' THEN 'KAMIS 요청 p_convert_kg_yn=Y로 원천이 1kg 비교가격을 반환하며 서버는 가격을 재환산하지 않습니다.'
                        ELSE '기존 관측의 단위 환산 근거를 추가 확인해야 합니다.'
                    END,
                    SourcePackageLabel = CASE
                        WHEN KindName REGEXP '\\([0-9]+([.][0-9]+)?[[:space:]]*(kg|g|개|마리|포기|단|속|망|상자|봉|팩|묶음|l|ml)\\)[[:space:]]*$'
                        THEN TRIM(TRAILING ')' FROM SUBSTRING_INDEX(TRIM(KindName), '(', -1))
                        ELSE ''
                    END;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ComparisonUnit",
                table: "agri_kamis_price_observations");

            migrationBuilder.DropColumn(
                name: "PriceNormalizationBasis",
                table: "agri_kamis_price_observations");

            migrationBuilder.DropColumn(
                name: "PriceNormalizationCode",
                table: "agri_kamis_price_observations");

            migrationBuilder.DropColumn(
                name: "SourcePackageLabel",
                table: "agri_kamis_price_observations");
        }
    }
}
