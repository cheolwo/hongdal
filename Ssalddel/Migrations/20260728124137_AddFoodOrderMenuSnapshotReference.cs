using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssalddel.Migrations
{
    /// <inheritdoc />
    public partial class AddFoodOrderMenuSnapshotReference : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "메뉴_id",
                table: "음식주문상품",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_음식주문상품_음식주문_id_메뉴_id",
                table: "음식주문상품",
                columns: new[] { "음식주문_id", "메뉴_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_음식주문상품_음식주문_id_메뉴_id",
                table: "음식주문상품");

            migrationBuilder.DropColumn(
                name: "메뉴_id",
                table: "음식주문상품");
        }
    }
}
