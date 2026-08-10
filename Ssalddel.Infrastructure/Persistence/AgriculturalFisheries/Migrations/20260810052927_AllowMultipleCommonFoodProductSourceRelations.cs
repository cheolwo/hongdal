using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssalddel.Infrastructure.Persistence.AgriculturalFisheries.Migrations
{
    /// <inheritdoc />
    public partial class AllowMultipleCommonFoodProductSourceRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_cfpr_product_source_scheme_code",
                table: "agri_common_food_product_code_relations",
                columns: new[] { "ProductIdentityId", "SourceKey", "CodeScheme", "ExternalCode" });

            migrationBuilder.DropIndex(
                name: "IX_agri_common_food_product_code_relations_ProductIdentityId_So~",
                table: "agri_common_food_product_code_relations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_agri_common_food_product_code_relations_ProductIdentityId_So~",
                table: "agri_common_food_product_code_relations",
                columns: new[] { "ProductIdentityId", "SourceKey", "CodeScheme" },
                unique: true);

            migrationBuilder.DropIndex(
                name: "IX_cfpr_product_source_scheme_code",
                table: "agri_common_food_product_code_relations");
        }
    }
}
