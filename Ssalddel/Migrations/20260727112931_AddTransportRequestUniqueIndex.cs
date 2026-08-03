using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssalddel.Migrations
{
    /// <inheritdoc />
    public partial class AddTransportRequestUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "request_id",
                table: "운송실행투영",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            // Older projections could be created before a source request was
            // linked. Empty request IDs must not make the new uniqueness
            // boundary reject otherwise independent historical projections.
            migrationBuilder.Sql(@"
UPDATE `운송실행투영`
SET `request_id` = CONCAT('legacy-unlinked-transport-', `id`)
WHERE TRIM(`request_id`) = '';");

            migrationBuilder.CreateIndex(
                name: "ux_운송실행투영_request_id",
                table: "운송실행투영",
                column: "request_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ux_운송실행투영_request_id",
                table: "운송실행투영");

            migrationBuilder.AlterColumn<string>(
                name: "request_id",
                table: "운송실행투영",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
