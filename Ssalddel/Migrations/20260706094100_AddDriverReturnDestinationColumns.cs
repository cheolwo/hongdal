using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using 살뜰.Data;

#nullable disable

namespace Ssalddel.Migrations
{
    [DbContext(typeof(SsalddelContext))]
    [Migration("20260706094100_AddDriverReturnDestinationColumns")]
    /// <inheritdoc />
    public partial class AddDriverReturnDestinationColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            AddColumnIfMissing(migrationBuilder, "배차_대기", "confirmed_driver_id", "`confirmed_driver_id` longtext NULL");
            AddColumnIfMissing(migrationBuilder, "배차_대기", "current_recommended_driver_id", "`current_recommended_driver_id` longtext NULL");
            AddColumnIfMissing(migrationBuilder, "배차_대기", "exposure_state", "`exposure_state` int NOT NULL DEFAULT 100");
            AddColumnIfMissing(migrationBuilder, "배차_대기", "last_rejected_driver_id", "`last_rejected_driver_id` longtext NULL");
            AddColumnIfMissing(migrationBuilder, "배차_대기", "plan_attempts", "`plan_attempts` int NOT NULL DEFAULT 0");
            AddColumnIfMissing(migrationBuilder, "배차_대기", "public_transition_at", "`public_transition_at` datetime(6) NULL");
            AddColumnIfMissing(migrationBuilder, "배차_대기", "queue_stage", "`queue_stage` int NOT NULL DEFAULT 10");
            AddColumnIfMissing(migrationBuilder, "배차_대기", "recommendation_expires_at", "`recommendation_expires_at` datetime(6) NULL");
            AddColumnIfMissing(migrationBuilder, "배차_대기", "recommendation_round", "`recommendation_round` int NOT NULL DEFAULT 0");
            AddColumnIfMissing(migrationBuilder, "배차_대기", "recommendation_started_at", "`recommendation_started_at` datetime(6) NULL");
            AddColumnIfMissing(migrationBuilder, "배차_대기", "row_version", "`row_version` timestamp(6) NULL");

            migrationBuilder.AddColumn<decimal>(
                name: "기본복귀지경도",
                table: "용달기사",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "기본복귀지위도",
                table: "용달기사",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "기본복귀지주소",
                table: "용달기사",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "집주소를복귀지로사용허용",
                table: "용달기사",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "기본복귀지경도",
                table: "용달기사");

            migrationBuilder.DropColumn(
                name: "기본복귀지위도",
                table: "용달기사");

            migrationBuilder.DropColumn(
                name: "기본복귀지주소",
                table: "용달기사");

            migrationBuilder.DropColumn(
                name: "집주소를복귀지로사용허용",
                table: "용달기사");
        }

        private static void AddColumnIfMissing(
            MigrationBuilder migrationBuilder,
            string tableName,
            string columnName,
            string columnDefinition)
        {
            migrationBuilder.Sql($@"
SET @column_exists = (
    SELECT COUNT(*)
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = '{tableName}'
      AND COLUMN_NAME = '{columnName}'
);
SET @sql = IF(@column_exists = 0,
    'ALTER TABLE `{tableName}` ADD COLUMN {columnDefinition}',
    'SELECT 1'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;");
        }
    }
}
