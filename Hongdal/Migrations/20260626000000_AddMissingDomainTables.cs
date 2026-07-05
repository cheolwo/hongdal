using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using 홍달.Data;

#nullable disable

namespace Hongdal.Migrations
{
    [DbContext(typeof(HongdalContext))]
    [Migration("20260626000000_AddMissingDomainTables")]
    public partial class AddMissingDomainTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE TABLE IF NOT EXISTS `용달기사` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `notion_page_id` longtext NOT NULL,
  `기사명` longtext NOT NULL,
  `기사Id` longtext NOT NULL,
  `상태` longtext NOT NULL,
  `연락처` longtext NOT NULL,
  `차량` longtext NOT NULL,
  `운행상태` longtext NOT NULL,
  `주_활동지역` longtext NOT NULL,
  `메모` longtext NOT NULL,
  `등록일` datetime(6) NULL,
  `created_at` datetime(6) NOT NULL,
  `updated_at` datetime(6) NOT NULL,
  PRIMARY KEY (`id`)
) CHARACTER SET=utf8mb4;");

            migrationBuilder.Sql(@"
CREATE TABLE IF NOT EXISTS `기사월정산` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `driver_id` longtext NOT NULL,
  `year` int NOT NULL,
  `month` int NOT NULL,
  `dispatch_count` int NOT NULL,
  `usage_fee` decimal(65,30) NOT NULL,
  `is_paid` tinyint(1) NOT NULL,
  `created_at` datetime(6) NOT NULL,
  `updated_at` datetime(6) NOT NULL,
  PRIMARY KEY (`id`)
) CHARACTER SET=utf8mb4;");

            migrationBuilder.Sql(@"
CREATE TABLE IF NOT EXISTS `driver_location_history` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `driver_id` longtext NOT NULL,
  `latitude` decimal(65,30) NOT NULL,
  `longitude` decimal(65,30) NOT NULL,
  `accuracy_m` decimal(65,30) NULL,
  `recorded_at` datetime(6) NOT NULL,
  `created_at` datetime(6) NOT NULL,
  `updated_at` datetime(6) NOT NULL,
  PRIMARY KEY (`id`)
) CHARACTER SET=utf8mb4;");

            migrationBuilder.Sql(@"
CREATE TABLE IF NOT EXISTS `결제` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `payment_id` longtext NOT NULL,
  `request_id` longtext NOT NULL,
  `shipper_id` longtext NOT NULL,
  `pg_provider` longtext NOT NULL,
  `payment_method` longtext NOT NULL,
  `payment_status` longtext NOT NULL,
  `amount` int NOT NULL,
  `order_id` longtext NOT NULL,
  `payment_key` longtext NULL,
  `toss_response_json` longtext NULL,
  `created_at` datetime(6) NOT NULL,
  `approved_at` datetime(6) NULL,
  PRIMARY KEY (`id`)
) CHARACTER SET=utf8mb4;");

            migrationBuilder.Sql(@"
CREATE TABLE IF NOT EXISTS `배차_대기` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `request_id` longtext NOT NULL,
  `shipper_id` longtext NOT NULL,
  `pickup_address` longtext NOT NULL,
  `pickup_address_detail` longtext NOT NULL,
  `pickup_latitude` decimal(65,30) NULL,
  `pickup_longitude` decimal(65,30) NULL,
  `dropoff_address` longtext NOT NULL,
  `dropoff_address_detail` longtext NOT NULL,
  `dropoff_latitude` decimal(65,30) NULL,
  `dropoff_longitude` decimal(65,30) NULL,
  `status` longtext NOT NULL,
  `created_at` datetime(6) NOT NULL,
  `updated_at` datetime(6) NOT NULL,
  PRIMARY KEY (`id`)
) CHARACTER SET=utf8mb4;");

            migrationBuilder.Sql(@"
CREATE TABLE IF NOT EXISTS `운송이벤트` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `request_id` longtext NOT NULL,
  `event_type` longtext NOT NULL,
  `event_time` datetime(6) NOT NULL,
  `metadata` longtext NOT NULL,
  PRIMARY KEY (`id`)
) CHARACTER SET=utf8mb4;");

            migrationBuilder.Sql(@"
CREATE TABLE IF NOT EXISTS `운임구성` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `request_id` longtext NOT NULL,
  `기본운임` decimal(65,30) NOT NULL,
  `거리운임` decimal(65,30) NOT NULL,
  `할증` decimal(65,30) NOT NULL,
  `대기료` decimal(65,30) NOT NULL,
  `수작업비` decimal(65,30) NOT NULL,
  `최종운임` decimal(65,30) NOT NULL,
  `created_at` datetime(6) NOT NULL,
  `updated_at` datetime(6) NOT NULL,
  PRIMARY KEY (`id`)
) CHARACTER SET=utf8mb4;");

            migrationBuilder.Sql(@"
CREATE TABLE IF NOT EXISTS `차량단가` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `차량종류` longtext NOT NULL,
  `기본운임` decimal(65,30) NOT NULL,
  `Km당단가` decimal(65,30) NOT NULL,
  `야간할증` decimal(65,30) NOT NULL,
  `우천할증` decimal(65,30) NOT NULL,
  `최소운임` decimal(65,30) NOT NULL,
  `created_at` datetime(6) NOT NULL,
  `updated_at` datetime(6) NOT NULL,
  PRIMARY KEY (`id`)
) CHARACTER SET=utf8mb4;");

            migrationBuilder.Sql(@"
CREATE TABLE IF NOT EXISTS `shipper_requests` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `request_id` longtext NOT NULL,
  `shipper_id` longtext NOT NULL,
  `cargo_type` longtext NOT NULL,
  `cargo_description` longtext NOT NULL,
  `cargo_quantity` int NULL,
  `cargo_weight_kg` decimal(65,30) NULL,
  `cargo_volume_cbm` decimal(65,30) NULL,
  `cargo_fragile` tinyint(1) NOT NULL,
  `cargo_temperature` longtext NOT NULL,
  `transport_type` longtext NOT NULL,
  `vehicle_type` longtext NOT NULL,
  `payment_method` longtext NOT NULL,
  `estimated_payment_amount` int NULL,
  `pricing_config_id` bigint NULL,
  `pickup_address` longtext NOT NULL,
  `pickup_address_detail` longtext NOT NULL,
  `pickup_latitude` decimal(65,30) NULL,
  `pickup_longitude` decimal(65,30) NULL,
  `pickup_contact_name` longtext NOT NULL,
  `pickup_contact_phone` longtext NOT NULL,
  `pickup_window_start` datetime(6) NOT NULL,
  `pickup_window_end` datetime(6) NOT NULL,
  `dropoff_address` longtext NOT NULL,
  `dropoff_address_detail` longtext NOT NULL,
  `dropoff_latitude` decimal(65,30) NULL,
  `dropoff_longitude` decimal(65,30) NULL,
  `dropoff_contact_name` longtext NOT NULL,
  `dropoff_contact_phone` longtext NOT NULL,
  `dropoff_window_start` datetime(6) NULL,
  `dropoff_window_end` datetime(6) NULL,
  `service_level` longtext NOT NULL,
  `request_text` longtext NOT NULL,
  `waiting_fee` decimal(65,30) NULL,
  `manual_fee` decimal(65,30) NULL,
  `surcharge` decimal(65,30) NULL,
  `final_fare` decimal(65,30) NULL,
  `client_request_id` longtext NOT NULL,
  `status` longtext NOT NULL,
  `payment_status` longtext NOT NULL,
  `dispatch_status` longtext NOT NULL,
  `created_at` datetime(6) NOT NULL,
  `updated_at` datetime(6) NOT NULL,
  PRIMARY KEY (`id`)
) CHARACTER SET=utf8mb4;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TABLE IF EXISTS `shipper_requests`;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS `차량단가`;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS `운임구성`;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS `운송이벤트`;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS `배차_대기`;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS `결제`;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS `driver_location_history`;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS `기사월정산`;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS `용달기사`;");
        }
    }
}
