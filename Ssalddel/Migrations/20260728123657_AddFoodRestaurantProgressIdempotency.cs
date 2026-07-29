using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssalddel.Migrations
{
    /// <inheritdoc />
    public partial class AddFoodRestaurantProgressIdempotency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "처리_user_id",
                table: "음식주문상태이력",
                type: "varchar(450)",
                maxLength: 450,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<Guid>(
                name: "클라이언트요청_id",
                table: "음식주문상태이력",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_음식주문상태이력_음식주문_id_클라이언트요청_id",
                table: "음식주문상태이력",
                columns: new[] { "음식주문_id", "클라이언트요청_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_음식주문상태이력_음식주문_id_클라이언트요청_id",
                table: "음식주문상태이력");

            migrationBuilder.DropColumn(
                name: "처리_user_id",
                table: "음식주문상태이력");

            migrationBuilder.DropColumn(
                name: "클라이언트요청_id",
                table: "음식주문상태이력");
        }
    }
}
