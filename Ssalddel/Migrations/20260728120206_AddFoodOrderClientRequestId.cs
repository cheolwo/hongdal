using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssalddel.Migrations
{
    /// <inheritdoc />
    public partial class AddFoodOrderClientRequestId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "클라이언트요청_id",
                table: "음식주문",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_음식주문_주문자_user_id_클라이언트요청_id",
                table: "음식주문",
                columns: new[] { "주문자_user_id", "클라이언트요청_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_음식주문_주문자_user_id_클라이언트요청_id",
                table: "음식주문");

            migrationBuilder.DropColumn(
                name: "클라이언트요청_id",
                table: "음식주문");
        }
    }
}
