using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssalddel.Migrations
{
    /// <inheritdoc />
    public partial class AddWarehouseInventoryMovementLookupIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_재고이동_입고상품_id_발생일시",
                table: "재고이동",
                columns: new[] { "입고상품_id", "발생일시" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_재고이동_입고상품_id_발생일시",
                table: "재고이동");
        }
    }
}
