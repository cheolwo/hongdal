using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using 홍달.Data;

#nullable disable

namespace Hongdal.Migrations
{
    [DbContext(typeof(HongdalContext))]
    [Migration("20260712010700_DropNotionPageIdColumns")]
    public partial class DropNotionPageIdColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            DropColumnIfExists(migrationBuilder, "기사배차", "notion_page_id");
            DropColumnIfExists(migrationBuilder, "배달기사", "notion_page_id");
            DropColumnIfExists(migrationBuilder, "배송_운송", "notion_page_id");
            DropColumnIfExists(migrationBuilder, "배차_최소", "notion_page_id");
            DropColumnIfExists(migrationBuilder, "업체", "notion_page_id");
            DropColumnIfExists(migrationBuilder, "용달기사", "notion_page_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            AddColumnIfMissing(migrationBuilder, "기사배차", "notion_page_id", "`notion_page_id` longtext NULL");
            AddColumnIfMissing(migrationBuilder, "배달기사", "notion_page_id", "`notion_page_id` longtext NULL");
            AddColumnIfMissing(migrationBuilder, "배송_운송", "notion_page_id", "`notion_page_id` longtext NULL");
            AddColumnIfMissing(migrationBuilder, "배차_최소", "notion_page_id", "`notion_page_id` longtext NULL");
            AddColumnIfMissing(migrationBuilder, "업체", "notion_page_id", "`notion_page_id` longtext NULL");
            AddColumnIfMissing(migrationBuilder, "용달기사", "notion_page_id", "`notion_page_id` longtext NULL");
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
