using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using 홍달.Data;

#nullable disable

namespace Hongdal.Migrations
{
    [DbContext(typeof(HongdalContext))]
    [Migration("20260712010500_AddMissingDriverShiftReturnDestinationColumns")]
    public partial class AddMissingDriverShiftReturnDestinationColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            AddColumnIfMissing(migrationBuilder, "driver_shifts", "today_return_destination", "`today_return_destination` longtext NULL");
            AddColumnIfMissing(migrationBuilder, "driver_shifts", "today_return_latitude", "`today_return_latitude` decimal(65,30) NULL");
            AddColumnIfMissing(migrationBuilder, "driver_shifts", "today_return_longitude", "`today_return_longitude` decimal(65,30) NULL");
            AddColumnIfMissing(migrationBuilder, "driver_shifts", "return_destination_source", "`return_destination_source` longtext NULL");
            AddColumnIfMissing(migrationBuilder, "driver_shifts", "return_destination_recorded_at", "`return_destination_recorded_at` datetime(6) NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            DropColumnIfExists(migrationBuilder, "driver_shifts", "return_destination_recorded_at");
            DropColumnIfExists(migrationBuilder, "driver_shifts", "return_destination_source");
            DropColumnIfExists(migrationBuilder, "driver_shifts", "today_return_longitude");
            DropColumnIfExists(migrationBuilder, "driver_shifts", "today_return_latitude");
            DropColumnIfExists(migrationBuilder, "driver_shifts", "today_return_destination");
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

        private static void DropColumnIfExists(
            MigrationBuilder migrationBuilder,
            string tableName,
            string columnName)
        {
            migrationBuilder.Sql($@"
SET @column_exists = (
    SELECT COUNT(*)
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = '{tableName}'
      AND COLUMN_NAME = '{columnName}'
);
SET @sql = IF(@column_exists = 1,
    'ALTER TABLE `{tableName}` DROP COLUMN `{columnName}`',
    'SELECT 1'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;");
        }
    }
}
