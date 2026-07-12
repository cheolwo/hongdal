using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hongdal.Migrations
{
    /// <inheritdoc />
    public partial class ConsolidateDispatchWaitIntoTransportLedger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "출고묶음_id",
                table: "출고예정",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "business_type",
                table: "운송원장",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "confirmed_driver_id",
                table: "운송원장",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "current_recommended_driver_id",
                table: "운송원장",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "dropoff_address",
                table: "운송원장",
                type: "longtext",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "dropoff_address_detail",
                table: "운송원장",
                type: "longtext",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "dropoff_latitude",
                table: "운송원장",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "dropoff_longitude",
                table: "운송원장",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "exposure_state",
                table: "운송원장",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "group_purchase_destination_type_code",
                table: "운송원장",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "group_purchase_distribution_responsibility_code",
                table: "운송원장",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "group_purchase_driver_unit_distribution",
                table: "운송원장",
                type: "tinyint(1)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "group_purchase_unit_delivery_count",
                table: "운송원장",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "group_purchase_unit_distribution_mode_code",
                table: "운송원장",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "last_rejected_driver_id",
                table: "운송원장",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "pickup_address",
                table: "운송원장",
                type: "longtext",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "pickup_address_detail",
                table: "운송원장",
                type: "longtext",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "pickup_latitude",
                table: "운송원장",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "pickup_longitude",
                table: "운송원장",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "plan_attempts",
                table: "운송원장",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "public_transition_at",
                table: "운송원장",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "queue_stage",
                table: "운송원장",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "recommendation_expires_at",
                table: "운송원장",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "recommendation_round",
                table: "운송원장",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "recommendation_started_at",
                table: "운송원장",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "request_id",
                table: "운송원장",
                type: "longtext",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "row_version",
                table: "운송원장",
                type: "timestamp(6)",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "shipper_id",
                table: "운송원장",
                type: "longtext",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "source_request_id",
                table: "운송원장",
                type: "longtext",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "source_type",
                table: "운송원장",
                type: "longtext",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "출고묶음",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    출고묶음번호 = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    주문참조번호 = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    출고창고_id = table.Column<long>(type: "bigint", nullable: false),
                    판매자_user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    주문자_user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    상태 = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    피킹시작일시 = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    피킹완료일시 = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    포장완료일시 = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    출고완료일시 = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    운송의뢰_id = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_출고묶음", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_출고예정_출고묶음_id",
                table: "출고예정",
                column: "출고묶음_id");

            migrationBuilder.CreateIndex(
                name: "IX_출고묶음_운송의뢰_id",
                table: "출고묶음",
                column: "운송의뢰_id");

            migrationBuilder.CreateIndex(
                name: "IX_출고묶음_출고묶음번호",
                table: "출고묶음",
                column: "출고묶음번호",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_출고묶음_출고창고_id_상태_created_at",
                table: "출고묶음",
                columns: new[] { "출고창고_id", "상태", "created_at" });

            migrationBuilder.Sql(
                """
                UPDATE `운송원장` t
                JOIN `배차_대기` q ON t.`운송번호` = q.`request_id`
                SET
                    t.`request_id` = q.`request_id`,
                    t.`shipper_id` = q.`shipper_id`,
                    t.`business_type` = q.`business_type`,
                    t.`source_type` = q.`source_type`,
                    t.`source_request_id` = q.`source_request_id`,
                    t.`group_purchase_destination_type_code` = q.`group_purchase_destination_type_code`,
                    t.`group_purchase_driver_unit_distribution` = q.`group_purchase_driver_unit_distribution`,
                    t.`group_purchase_unit_distribution_mode_code` = q.`group_purchase_unit_distribution_mode_code`,
                    t.`group_purchase_unit_delivery_count` = q.`group_purchase_unit_delivery_count`,
                    t.`group_purchase_distribution_responsibility_code` = q.`group_purchase_distribution_responsibility_code`,
                    t.`상태` = q.`status`,
                    t.`queue_stage` = q.`queue_stage`,
                    t.`exposure_state` = q.`exposure_state`,
                    t.`current_recommended_driver_id` = q.`current_recommended_driver_id`,
                    t.`recommendation_started_at` = q.`recommendation_started_at`,
                    t.`recommendation_expires_at` = q.`recommendation_expires_at`,
                    t.`recommendation_round` = q.`recommendation_round`,
                    t.`plan_attempts` = q.`plan_attempts`,
                    t.`last_rejected_driver_id` = q.`last_rejected_driver_id`,
                    t.`public_transition_at` = q.`public_transition_at`,
                    t.`confirmed_driver_id` = q.`confirmed_driver_id`,
                    t.`pickup_address` = q.`pickup_address`,
                    t.`pickup_address_detail` = q.`pickup_address_detail`,
                    t.`pickup_latitude` = q.`pickup_latitude`,
                    t.`pickup_longitude` = q.`pickup_longitude`,
                    t.`dropoff_address` = q.`dropoff_address`,
                    t.`dropoff_address_detail` = q.`dropoff_address_detail`,
                    t.`dropoff_latitude` = q.`dropoff_latitude`,
                    t.`dropoff_longitude` = q.`dropoff_longitude`,
                    t.`기사_운송자` = COALESCE(NULLIF(t.`기사_운송자`, ''), q.`confirmed_driver_id`, ''),
                    t.`출발지` = COALESCE(NULLIF(t.`출발지`, ''), q.`pickup_address`, ''),
                    t.`도착지` = COALESCE(NULLIF(t.`도착지`, ''), q.`dropoff_address`, ''),
                    t.`updated_at` = q.`updated_at`;
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO `운송원장` (
                    `운송번호`,
                    `request_id`,
                    `shipper_id`,
                    `business_type`,
                    `source_type`,
                    `source_request_id`,
                    `group_purchase_destination_type_code`,
                    `group_purchase_driver_unit_distribution`,
                    `group_purchase_unit_distribution_mode_code`,
                    `group_purchase_unit_delivery_count`,
                    `group_purchase_distribution_responsibility_code`,
                    `상태`,
                    `queue_stage`,
                    `exposure_state`,
                    `current_recommended_driver_id`,
                    `recommendation_started_at`,
                    `recommendation_expires_at`,
                    `recommendation_round`,
                    `plan_attempts`,
                    `last_rejected_driver_id`,
                    `public_transition_at`,
                    `confirmed_driver_id`,
                    `pickup_address`,
                    `pickup_address_detail`,
                    `pickup_latitude`,
                    `pickup_longitude`,
                    `dropoff_address`,
                    `dropoff_address_detail`,
                    `dropoff_latitude`,
                    `dropoff_longitude`,
                    `기사_운송자`,
                    `출발지`,
                    `도착지`,
                    `첨부_json`,
                    `메모`,
                    `created_at`,
                    `updated_at`
                )
                SELECT
                    q.`request_id`,
                    q.`request_id`,
                    q.`shipper_id`,
                    q.`business_type`,
                    q.`source_type`,
                    q.`source_request_id`,
                    q.`group_purchase_destination_type_code`,
                    q.`group_purchase_driver_unit_distribution`,
                    q.`group_purchase_unit_distribution_mode_code`,
                    q.`group_purchase_unit_delivery_count`,
                    q.`group_purchase_distribution_responsibility_code`,
                    q.`status`,
                    q.`queue_stage`,
                    q.`exposure_state`,
                    q.`current_recommended_driver_id`,
                    q.`recommendation_started_at`,
                    q.`recommendation_expires_at`,
                    q.`recommendation_round`,
                    q.`plan_attempts`,
                    q.`last_rejected_driver_id`,
                    q.`public_transition_at`,
                    q.`confirmed_driver_id`,
                    q.`pickup_address`,
                    q.`pickup_address_detail`,
                    q.`pickup_latitude`,
                    q.`pickup_longitude`,
                    q.`dropoff_address`,
                    q.`dropoff_address_detail`,
                    q.`dropoff_latitude`,
                    q.`dropoff_longitude`,
                    COALESCE(q.`confirmed_driver_id`, ''),
                    COALESCE(q.`pickup_address`, ''),
                    COALESCE(q.`dropoff_address`, ''),
                    '[]',
                    '배차대기에서 운송원장으로 통합',
                    q.`created_at`,
                    q.`updated_at`
                FROM `배차_대기` q
                LEFT JOIN `운송원장` t ON t.`운송번호` = q.`request_id`
                WHERE t.`id` IS NULL;
                """);

            migrationBuilder.DropTable(
                name: "배차_대기");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "출고묶음");

            migrationBuilder.DropIndex(
                name: "IX_출고예정_출고묶음_id",
                table: "출고예정");

            migrationBuilder.DropColumn(
                name: "출고묶음_id",
                table: "출고예정");

            migrationBuilder.DropColumn(
                name: "business_type",
                table: "운송원장");

            migrationBuilder.DropColumn(
                name: "confirmed_driver_id",
                table: "운송원장");

            migrationBuilder.DropColumn(
                name: "current_recommended_driver_id",
                table: "운송원장");

            migrationBuilder.DropColumn(
                name: "dropoff_address",
                table: "운송원장");

            migrationBuilder.DropColumn(
                name: "dropoff_address_detail",
                table: "운송원장");

            migrationBuilder.DropColumn(
                name: "dropoff_latitude",
                table: "운송원장");

            migrationBuilder.DropColumn(
                name: "dropoff_longitude",
                table: "운송원장");

            migrationBuilder.DropColumn(
                name: "exposure_state",
                table: "운송원장");

            migrationBuilder.DropColumn(
                name: "group_purchase_destination_type_code",
                table: "운송원장");

            migrationBuilder.DropColumn(
                name: "group_purchase_distribution_responsibility_code",
                table: "운송원장");

            migrationBuilder.DropColumn(
                name: "group_purchase_driver_unit_distribution",
                table: "운송원장");

            migrationBuilder.DropColumn(
                name: "group_purchase_unit_delivery_count",
                table: "운송원장");

            migrationBuilder.DropColumn(
                name: "group_purchase_unit_distribution_mode_code",
                table: "운송원장");

            migrationBuilder.DropColumn(
                name: "last_rejected_driver_id",
                table: "운송원장");

            migrationBuilder.DropColumn(
                name: "pickup_address",
                table: "운송원장");

            migrationBuilder.DropColumn(
                name: "pickup_address_detail",
                table: "운송원장");

            migrationBuilder.DropColumn(
                name: "pickup_latitude",
                table: "운송원장");

            migrationBuilder.DropColumn(
                name: "pickup_longitude",
                table: "운송원장");

            migrationBuilder.DropColumn(
                name: "plan_attempts",
                table: "운송원장");

            migrationBuilder.DropColumn(
                name: "public_transition_at",
                table: "운송원장");

            migrationBuilder.DropColumn(
                name: "queue_stage",
                table: "운송원장");

            migrationBuilder.DropColumn(
                name: "recommendation_expires_at",
                table: "운송원장");

            migrationBuilder.DropColumn(
                name: "recommendation_round",
                table: "운송원장");

            migrationBuilder.DropColumn(
                name: "recommendation_started_at",
                table: "운송원장");

            migrationBuilder.DropColumn(
                name: "request_id",
                table: "운송원장");

            migrationBuilder.DropColumn(
                name: "row_version",
                table: "운송원장");

            migrationBuilder.DropColumn(
                name: "shipper_id",
                table: "운송원장");

            migrationBuilder.DropColumn(
                name: "source_request_id",
                table: "운송원장");

            migrationBuilder.DropColumn(
                name: "source_type",
                table: "운송원장");

            migrationBuilder.CreateTable(
                name: "배차_대기",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    row_version = table.Column<DateTime>(type: "timestamp(6)", rowVersion: true, nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    plan_attempts = table.Column<int>(type: "int", nullable: false),
                    public_transition_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    group_purchase_driver_unit_distribution = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    group_purchase_destination_type_code = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    group_purchase_distribution_responsibility_code = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    group_purchase_unit_delivery_count = table.Column<int>(type: "int", nullable: true),
                    group_purchase_unit_distribution_mode_code = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    last_rejected_driver_id = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    exposure_state = table.Column<int>(type: "int", nullable: false),
                    business_type = table.Column<int>(type: "int", nullable: false),
                    queue_stage = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    source_request_id = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    source_type = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    request_id = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    recommendation_round = table.Column<int>(type: "int", nullable: false),
                    recommendation_expires_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    recommendation_started_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    pickup_longitude = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    pickup_address = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    pickup_address_detail = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    pickup_latitude = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    dropoff_longitude = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    dropoff_address = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    dropoff_address_detail = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    dropoff_latitude = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    current_recommended_driver_id = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    shipper_id = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    confirmed_driver_id = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_배차_대기", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
