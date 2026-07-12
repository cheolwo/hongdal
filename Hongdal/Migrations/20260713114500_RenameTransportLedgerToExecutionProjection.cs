using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using 홍달.Data;

#nullable disable

namespace Hongdal.Migrations
{
    [DbContext(typeof(HongdalContext))]
    [Migration("20260713114500_RenameTransportLedgerToExecutionProjection")]
    public partial class RenameTransportLedgerToExecutionProjection : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            RenameTableIfExists(migrationBuilder, "운송원장", "운송실행투영");
            RenameColumnIfExists(migrationBuilder, "운송문서", "운송원장_id", "운송실행투영_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            RenameColumnIfExists(migrationBuilder, "운송문서", "운송실행투영_id", "운송원장_id");
            RenameTableIfExists(migrationBuilder, "운송실행투영", "운송원장");
        }

        private static void RenameTableIfExists(
            MigrationBuilder migrationBuilder,
            string oldTableName,
            string newTableName)
        {
            migrationBuilder.Sql($@"
SET @old_table_exists = (
    SELECT COUNT(*)
    FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = '{oldTableName}'
);
SET @new_table_exists = (
    SELECT COUNT(*)
    FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = '{newTableName}'
);
SET @sql = IF(@old_table_exists = 1 AND @new_table_exists = 0,
    'RENAME TABLE `{oldTableName}` TO `{newTableName}`',
    'SELECT 1'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;");
        }

        private static void RenameColumnIfExists(
            MigrationBuilder migrationBuilder,
            string tableName,
            string oldColumnName,
            string newColumnName)
        {
            migrationBuilder.Sql($@"
SET @table_exists = (
    SELECT COUNT(*)
    FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = '{tableName}'
);
SET @old_column_exists = (
    SELECT COUNT(*)
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = '{tableName}'
      AND COLUMN_NAME = '{oldColumnName}'
);
SET @new_column_exists = (
    SELECT COUNT(*)
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = '{tableName}'
      AND COLUMN_NAME = '{newColumnName}'
);
SET @sql = IF(@table_exists = 1 AND @old_column_exists = 1 AND @new_column_exists = 0,
    'ALTER TABLE `{tableName}` CHANGE COLUMN `{oldColumnName}` `{newColumnName}` bigint NULL',
    'SELECT 1'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;");
        }
    }
}
