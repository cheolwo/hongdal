using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssalddel.Migrations
{
    /// <inheritdoc />
    public partial class AddCommunityPostEmailNotificationOutbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "기사배차",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    notion_page_id = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    배차Id = table.Column<long>(type: "bigint", nullable: true),
                    배차명 = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    상태 = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    배차일 = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    배달기사_id = table.Column<long>(type: "bigint", nullable: true),
                    픽업지 = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    배송지 = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    기본요금 = table.Column<long>(type: "bigint", nullable: true),
                    거리추가_요금 = table.Column<long>(type: "bigint", nullable: true),
                    주문Id = table.Column<long>(type: "bigint", nullable: true),
                    기사Id = table.Column<long>(type: "bigint", nullable: true),
                    잠금여부 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    잠금시각 = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    시도횟수 = table.Column<int>(type: "int", nullable: true),
                    픽업거리_m = table.Column<int>(type: "int", nullable: true),
                    픽업예상시간_sec = table.Column<int>(type: "int", nullable: true),
                    배차점수 = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    실패사유 = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    메모 = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    배차생성시각 = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    배차완료시각 = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_기사배차", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "배달기사",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    notion_page_id = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    기사명 = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    기사Id = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    상태 = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    연락처 = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    차량 = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    운행상태 = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    주_활동지역 = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    메모 = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    등록일 = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_배달기사", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "배송_운송",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    notion_page_id = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    운송번호 = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    상태 = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    출발_픽업 = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    도착 = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    기사_운송자 = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    출발지 = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    도착지 = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    운임 = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    첨부_json = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    메모 = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_배송_운송", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "배차_최소",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    notion_page_id = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    배차Id = table.Column<long>(type: "bigint", nullable: true),
                    배차명 = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    기사Id = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    주문자_주소 = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    음식점_주소 = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    등록일 = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_배차_최소", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "업체",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    notion_page_id = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    업체명 = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    상태 = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    대표_연락처 = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    담당자 = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    이메일 = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    주소 = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    정산_결제_조건 = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    첨부_json = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    메모 = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    등록일 = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_업체", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "driver_shifts",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    driver_id = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    start_mode = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    started_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    start_location = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    return_destination = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_driver_shifts", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "배차계획신청",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    기사Id = table.Column<string>(type: "longtext", nullable: false),
                    출발지 = table.Column<string>(type: "longtext", nullable: false),
                    복귀지 = table.Column<string>(type: "longtext", nullable: false),
                    희망복귀시각 = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    배차가능시각 = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    상태 = table.Column<string>(type: "longtext", nullable: false),
                    메모 = table.Column<string>(type: "longtext", nullable: false),
                    신청일시 = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_배차계획신청", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false),
                    Name = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false),
                    UserName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    PasswordHash = table.Column<string>(type: "longtext", nullable: true),
                    SecurityStamp = table.Column<string>(type: "longtext", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "longtext", nullable: true),
                    PhoneNumber = table.Column<string>(type: "longtext", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RoleId = table.Column<string>(type: "varchar(255)", nullable: false),
                    ClaimType = table.Column<string>(type: "longtext", nullable: true),
                    ClaimValue = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false),
                    ClaimType = table.Column<string>(type: "longtext", nullable: true),
                    ClaimValue = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "varchar(255)", nullable: false),
                    ProviderKey = table.Column<string>(type: "varchar(255)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "longtext", nullable: true),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false),
                    RoleId = table.Column<string>(type: "varchar(255)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false),
                    LoginProvider = table.Column<string>(type: "varchar(255)", nullable: false),
                    Name = table.Column<string>(type: "varchar(255)", nullable: false),
                    Value = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessRegistrationNumber",
                table: "AspNetUsers",
                type: "longtext",
                nullable: true);

            migrationBuilder.Sql("\r\nCREATE TABLE IF NOT EXISTS `용달기사` (\r\n  `id` bigint NOT NULL AUTO_INCREMENT,\r\n  `notion_page_id` longtext NOT NULL,\r\n  `기사명` longtext NOT NULL,\r\n  `기사Id` longtext NOT NULL,\r\n  `상태` longtext NOT NULL,\r\n  `연락처` longtext NOT NULL,\r\n  `차량` longtext NOT NULL,\r\n  `운행상태` longtext NOT NULL,\r\n  `주_활동지역` longtext NOT NULL,\r\n  `메모` longtext NOT NULL,\r\n  `등록일` datetime(6) NULL,\r\n  `created_at` datetime(6) NOT NULL,\r\n  `updated_at` datetime(6) NOT NULL,\r\n  PRIMARY KEY (`id`)\r\n) CHARACTER SET=utf8mb4;");

            migrationBuilder.Sql("\r\nCREATE TABLE IF NOT EXISTS `기사월정산` (\r\n  `id` bigint NOT NULL AUTO_INCREMENT,\r\n  `driver_id` longtext NOT NULL,\r\n  `year` int NOT NULL,\r\n  `month` int NOT NULL,\r\n  `dispatch_count` int NOT NULL,\r\n  `usage_fee` decimal(65,30) NOT NULL,\r\n  `is_paid` tinyint(1) NOT NULL,\r\n  `created_at` datetime(6) NOT NULL,\r\n  `updated_at` datetime(6) NOT NULL,\r\n  PRIMARY KEY (`id`)\r\n) CHARACTER SET=utf8mb4;");

            migrationBuilder.Sql("\r\nCREATE TABLE IF NOT EXISTS `driver_location_history` (\r\n  `id` bigint NOT NULL AUTO_INCREMENT,\r\n  `driver_id` longtext NOT NULL,\r\n  `latitude` decimal(65,30) NOT NULL,\r\n  `longitude` decimal(65,30) NOT NULL,\r\n  `accuracy_m` decimal(65,30) NULL,\r\n  `recorded_at` datetime(6) NOT NULL,\r\n  `created_at` datetime(6) NOT NULL,\r\n  `updated_at` datetime(6) NOT NULL,\r\n  PRIMARY KEY (`id`)\r\n) CHARACTER SET=utf8mb4;");

            migrationBuilder.Sql("\r\nCREATE TABLE IF NOT EXISTS `결제` (\r\n  `id` bigint NOT NULL AUTO_INCREMENT,\r\n  `payment_id` longtext NOT NULL,\r\n  `request_id` longtext NOT NULL,\r\n  `shipper_id` longtext NOT NULL,\r\n  `pg_provider` longtext NOT NULL,\r\n  `payment_method` longtext NOT NULL,\r\n  `payment_status` longtext NOT NULL,\r\n  `amount` int NOT NULL,\r\n  `order_id` longtext NOT NULL,\r\n  `payment_key` longtext NULL,\r\n  `toss_response_json` longtext NULL,\r\n  `created_at` datetime(6) NOT NULL,\r\n  `approved_at` datetime(6) NULL,\r\n  PRIMARY KEY (`id`)\r\n) CHARACTER SET=utf8mb4;");

            migrationBuilder.Sql("\r\nCREATE TABLE IF NOT EXISTS `배차_대기` (\r\n  `id` bigint NOT NULL AUTO_INCREMENT,\r\n  `request_id` longtext NOT NULL,\r\n  `shipper_id` longtext NOT NULL,\r\n  `pickup_address` longtext NOT NULL,\r\n  `pickup_address_detail` longtext NOT NULL,\r\n  `pickup_latitude` decimal(65,30) NULL,\r\n  `pickup_longitude` decimal(65,30) NULL,\r\n  `dropoff_address` longtext NOT NULL,\r\n  `dropoff_address_detail` longtext NOT NULL,\r\n  `dropoff_latitude` decimal(65,30) NULL,\r\n  `dropoff_longitude` decimal(65,30) NULL,\r\n  `status` longtext NOT NULL,\r\n  `created_at` datetime(6) NOT NULL,\r\n  `updated_at` datetime(6) NOT NULL,\r\n  PRIMARY KEY (`id`)\r\n) CHARACTER SET=utf8mb4;");

            migrationBuilder.Sql("\r\nCREATE TABLE IF NOT EXISTS `운송이벤트` (\r\n  `id` bigint NOT NULL AUTO_INCREMENT,\r\n  `request_id` longtext NOT NULL,\r\n  `event_type` longtext NOT NULL,\r\n  `event_time` datetime(6) NOT NULL,\r\n  `metadata` longtext NOT NULL,\r\n  PRIMARY KEY (`id`)\r\n) CHARACTER SET=utf8mb4;");

            migrationBuilder.Sql("\r\nCREATE TABLE IF NOT EXISTS `운임구성` (\r\n  `id` bigint NOT NULL AUTO_INCREMENT,\r\n  `request_id` longtext NOT NULL,\r\n  `기본운임` decimal(65,30) NOT NULL,\r\n  `거리운임` decimal(65,30) NOT NULL,\r\n  `할증` decimal(65,30) NOT NULL,\r\n  `대기료` decimal(65,30) NOT NULL,\r\n  `수작업비` decimal(65,30) NOT NULL,\r\n  `최종운임` decimal(65,30) NOT NULL,\r\n  `created_at` datetime(6) NOT NULL,\r\n  `updated_at` datetime(6) NOT NULL,\r\n  PRIMARY KEY (`id`)\r\n) CHARACTER SET=utf8mb4;");

            migrationBuilder.Sql("\r\nCREATE TABLE IF NOT EXISTS `차량단가` (\r\n  `id` bigint NOT NULL AUTO_INCREMENT,\r\n  `차량종류` longtext NOT NULL,\r\n  `기본운임` decimal(65,30) NOT NULL,\r\n  `Km당단가` decimal(65,30) NOT NULL,\r\n  `야간할증` decimal(65,30) NOT NULL,\r\n  `우천할증` decimal(65,30) NOT NULL,\r\n  `최소운임` decimal(65,30) NOT NULL,\r\n  `created_at` datetime(6) NOT NULL,\r\n  `updated_at` datetime(6) NOT NULL,\r\n  PRIMARY KEY (`id`)\r\n) CHARACTER SET=utf8mb4;");

            migrationBuilder.Sql("\r\nCREATE TABLE IF NOT EXISTS `shipper_requests` (\r\n  `id` bigint NOT NULL AUTO_INCREMENT,\r\n  `request_id` longtext NOT NULL,\r\n  `shipper_id` longtext NOT NULL,\r\n  `cargo_type` longtext NOT NULL,\r\n  `cargo_description` longtext NOT NULL,\r\n  `cargo_quantity` int NULL,\r\n  `cargo_weight_kg` decimal(65,30) NULL,\r\n  `cargo_volume_cbm` decimal(65,30) NULL,\r\n  `cargo_fragile` tinyint(1) NOT NULL,\r\n  `cargo_temperature` longtext NOT NULL,\r\n  `transport_type` longtext NOT NULL,\r\n  `vehicle_type` longtext NOT NULL,\r\n  `payment_method` longtext NOT NULL,\r\n  `estimated_payment_amount` int NULL,\r\n  `pricing_config_id` bigint NULL,\r\n  `pickup_address` longtext NOT NULL,\r\n  `pickup_address_detail` longtext NOT NULL,\r\n  `pickup_latitude` decimal(65,30) NULL,\r\n  `pickup_longitude` decimal(65,30) NULL,\r\n  `pickup_contact_name` longtext NOT NULL,\r\n  `pickup_contact_phone` longtext NOT NULL,\r\n  `pickup_window_start` datetime(6) NOT NULL,\r\n  `pickup_window_end` datetime(6) NOT NULL,\r\n  `dropoff_address` longtext NOT NULL,\r\n  `dropoff_address_detail` longtext NOT NULL,\r\n  `dropoff_latitude` decimal(65,30) NULL,\r\n  `dropoff_longitude` decimal(65,30) NULL,\r\n  `dropoff_contact_name` longtext NOT NULL,\r\n  `dropoff_contact_phone` longtext NOT NULL,\r\n  `dropoff_window_start` datetime(6) NULL,\r\n  `dropoff_window_end` datetime(6) NULL,\r\n  `service_level` longtext NOT NULL,\r\n  `request_text` longtext NOT NULL,\r\n  `waiting_fee` decimal(65,30) NULL,\r\n  `manual_fee` decimal(65,30) NULL,\r\n  `surcharge` decimal(65,30) NULL,\r\n  `final_fare` decimal(65,30) NULL,\r\n  `client_request_id` longtext NOT NULL,\r\n  `status` longtext NOT NULL,\r\n  `payment_status` longtext NOT NULL,\r\n  `dispatch_status` longtext NOT NULL,\r\n  `created_at` datetime(6) NOT NULL,\r\n  `updated_at` datetime(6) NOT NULL,\r\n  PRIMARY KEY (`id`)\r\n) CHARACTER SET=utf8mb4;");

            migrationBuilder.AddColumn<int>(
                name: "cargo_height_mm",
                table: "shipper_requests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "cargo_length_mm",
                table: "shipper_requests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "cargo_pallet_count",
                table: "shipper_requests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "cargo_width_mm",
                table: "shipper_requests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "cash_receipt_required",
                table: "shipper_requests",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "cash_settled_at",
                table: "shipper_requests",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "cash_settlement_memo",
                table: "shipper_requests",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "collector",
                table: "shipper_requests",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "evidence_method",
                table: "shipper_requests",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "receipt_issued_at",
                table: "shipper_requests",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "receipt_number",
                table: "shipper_requests",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "settlement_memo",
                table: "shipper_requests",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "settlement_status",
                table: "shipper_requests",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "settlement_time",
                table: "shipper_requests",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "tax_invoice_required",
                table: "shipper_requests",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "사용자_Command_기능설정",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    command_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    feature_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_enabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_사용자_Command_기능설정", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "차량제원",
                columns: table => new
                {
                    차량코드 = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    차량명 = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    제조사 = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    모델명 = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    차급 = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    차체형태 = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    적재함길이Mm = table.Column<int>(type: "int", nullable: false),
                    적재함폭Mm = table.Column<int>(type: "int", nullable: false),
                    적재함높이Mm = table.Column<int>(type: "int", nullable: true),
                    최대적재중량Kg = table.Column<int>(type: "int", nullable: false),
                    운영권장중량Kg = table.Column<int>(type: "int", nullable: true),
                    차량전체높이Mm = table.Column<int>(type: "int", nullable: true),
                    바닥높이Mm = table.Column<int>(type: "int", nullable: true),
                    비눈보호가능 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    냉장가능 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    냉동가능 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    측면상하차가능 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    리프트가능 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    장재물유리 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    팔레트적재개수 = table.Column<int>(type: "int", nullable: true),
                    기준연비KmPerLiter = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    장점메모 = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    단점메모 = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_차량제원", x => x.차량코드);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "화물요구조건",
                columns: table => new
                {
                    의뢰Id = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    화물길이Mm = table.Column<int>(type: "int", nullable: true),
                    화물폭Mm = table.Column<int>(type: "int", nullable: true),
                    화물높이Mm = table.Column<int>(type: "int", nullable: true),
                    화물무게Kg = table.Column<int>(type: "int", nullable: true),
                    팔레트개수 = table.Column<int>(type: "int", nullable: true),
                    비맞으면안됨 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    냉장필요 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    냉동필요 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    리프트필요 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    측면상하차필요 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    장재물 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    혼적허용 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    독차필수 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    주의사항 = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_화물요구조건", x => x.의뢰Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_사용자_Command_기능설정_user_id_command_name_feature_name",
                table: "사용자_Command_기능설정",
                columns: new[] { "user_id", "command_name", "feature_name" },
                unique: true);

            migrationBuilder.CreateTable(
                name: "Command_알림_Outbox",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    command_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    event_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    feature_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    target = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    payload_json = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    retry_count = table.Column<int>(type: "int", nullable: false),
                    trace_id = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Command_알림_Outbox", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Command_알림_Outbox_status_created_at",
                table: "Command_알림_Outbox",
                columns: new[] { "status", "created_at" });

            migrationBuilder.AddColumn<string>(
                name: "orderer_user_id",
                table: "shipper_requests",
                type: "longtext",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.Sql("\r\nUPDATE shipper_requests\r\nSET orderer_user_id = shipper_id\r\nWHERE orderer_user_id = '' OR orderer_user_id IS NULL;");

            migrationBuilder.CreateTable(
                name: "사용자_View_설정",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    app_key = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    view_key = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_visible = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_사용자_View_설정", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "플랫폼_View_정책",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    app_key = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    view_key = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    display_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    route = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    icon_key = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    role_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_required = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    policy_enabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    sort_order = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_플랫폼_View_정책", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_사용자_View_설정_user_id_app_key_view_key",
                table: "사용자_View_설정",
                columns: new[] { "user_id", "app_key", "view_key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_플랫폼_View_정책_app_key_view_key_role_name",
                table: "플랫폼_View_정책",
                columns: new[] { "app_key", "view_key", "role_name" },
                unique: true);

            migrationBuilder.CreateTable(
                name: "주문자프로필",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    표시명 = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    연락처 = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    기본주소 = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_주문자프로필", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_주문자프로필_user_id",
                table: "주문자프로필",
                column: "user_id",
                unique: true);

            migrationBuilder.AddColumn<decimal>(
                name: "권장최대CBM",
                table: "차량제원",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "추천사용여부",
                table: "차량제원",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<int>(
                name: "추천우선순위",
                table: "차량제원",
                type: "int",
                nullable: false,
                defaultValue: 100);

            migrationBuilder.Sql("\r\nUPDATE `차량제원`\r\nSET `권장최대CBM` = ROUND((`적재함길이Mm` / 1000.0) * (`적재함폭Mm` / 1000.0) * (`적재함높이Mm` / 1000.0) * 0.8, 3)\r\nWHERE `적재함길이Mm` > 0\r\n  AND `적재함폭Mm` > 0\r\n  AND `적재함높이Mm` IS NOT NULL\r\n  AND `적재함높이Mm` > 0\r\n  AND `권장최대CBM` IS NULL;\r\n\r\nUPDATE `차량제원`\r\nSET `추천우선순위` = 100\r\nWHERE `추천우선순위` = 0;\r\n\r\nUPDATE `차량제원`\r\nSET `추천사용여부` = 1\r\nWHERE `추천사용여부` = 0;");

            migrationBuilder.CreateTable(
                name: "사용자_행위_로그",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    app_key = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    user_name = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    role_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    email_masked = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    phone_last4 = table.Column<string>(type: "varchar(4)", maxLength: 4, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    action_type = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    action_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    route = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    trace_id = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_success = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    error_code = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    error_message = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    client_ip = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    user_agent = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    metadata_json = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    occurred_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_사용자_행위_로그", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "기사운행탐색",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    기사Id = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    탐색명 = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    운행예정일 = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    출발권역 = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    희망도착권역 = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    경유권역Json = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    차량종류 = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    최대적재중량Kg = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    최대적재부피Cbm = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    모집대상수 = table.Column<int>(type: "int", nullable: false),
                    탐색상태 = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    응답요약 = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    실행판단사유 = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    메모 = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_기사운행탐색", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "기사화주인연집계",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    기사Id = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    화주UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    최근거래일시 = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    누적운송건수 = table.Column<int>(type: "int", nullable: false),
                    최근응답률 = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    최근30일접점수 = table.Column<int>(type: "int", nullable: false),
                    취소율 = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    인연점수 = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    최근연락일시 = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    선호출발권역 = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    선호도착권역 = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_기사화주인연집계", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "운송의뢰상품연결",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    운송의뢰_id = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    입고상품_id = table.Column<long>(type: "bigint", nullable: false),
                    할당수량 = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_운송의뢰상품연결", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "운행탐색대상화주",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    운행탐색Id = table.Column<long>(type: "bigint", nullable: false),
                    화주UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    인연점수Snapshot = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    반응가능성점수Snapshot = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    선정사유 = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    대상상태 = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    발송메시지 = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    발송일시 = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    마지막응답일시 = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    예상화물정보요약 = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_운행탐색대상화주", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "운행탐색응답요약",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    운행탐색Id = table.Column<long>(type: "bigint", nullable: false),
                    화주UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    응답유형 = table.Column<int>(type: "int", nullable: false),
                    희망상차일시 = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    출발지요약 = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    도착지요약 = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    예상중량Kg = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    예상부피Cbm = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    예상팔레트개수 = table.Column<int>(type: "int", nullable: true),
                    메모요약 = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    응답일시 = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_운행탐색응답요약", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "입고상품",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    입고요청_id = table.Column<long>(type: "bigint", nullable: false),
                    창고_id = table.Column<long>(type: "bigint", nullable: false),
                    소유자_user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    판매자_user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    상품명 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    sku = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    옵션명 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    입고수량 = table.Column<int>(type: "int", nullable: false),
                    가용수량 = table.Column<int>(type: "int", nullable: false),
                    예약수량 = table.Column<int>(type: "int", nullable: false),
                    불량수량 = table.Column<int>(type: "int", nullable: false),
                    보관위치 = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    상태 = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    입고완료일시 = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_입고상품", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "입고요청",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    창고_id = table.Column<long>(type: "bigint", nullable: false),
                    주문자_user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    공급처명 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    원주문참조번호 = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    상태 = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    예정도착일 = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    비고 = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    입고완료일시 = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_입고요청", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "재고이력",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    입고상품_id = table.Column<long>(type: "bigint", nullable: false),
                    이력유형 = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    변경수량 = table.Column<int>(type: "int", nullable: false),
                    변경후수량 = table.Column<int>(type: "int", nullable: false),
                    원인유형 = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    원인_id = table.Column<long>(type: "bigint", nullable: true),
                    처리_user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    메모 = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    처리일시 = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_재고이력", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "창고",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    소유자_user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    창고명 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    사업자번호 = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    주소 = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    담당자명 = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    연락처 = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_창고", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "창고사용자",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    창고_id = table.Column<long>(type: "bigint", nullable: false),
                    user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    역할명 = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_primary = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_창고사용자", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "채널출품",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    판매상품_id = table.Column<long>(type: "bigint", nullable: false),
                    판매채널계정_id = table.Column<long>(type: "bigint", nullable: false),
                    채널상품번호 = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    출품상태 = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    동기화상태 = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    에러메시지 = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_채널출품", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "판매상품",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    입고상품_id = table.Column<long>(type: "bigint", nullable: false),
                    소유자_user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    대표상품명 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    판매sku = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    판매가 = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    상태 = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_판매상품", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "판매채널계정",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    채널종류 = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    상점명 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    연결상태 = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    토큰암호화저장값 = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    마지막동기화일시 = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_판매채널계정", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_기사운행탐색_기사Id_운행예정일_탐색상태",
                table: "기사운행탐색",
                columns: new[] { "기사Id", "운행예정일", "탐색상태" });

            migrationBuilder.CreateIndex(
                name: "IX_기사화주인연집계_기사Id_화주UserId",
                table: "기사화주인연집계",
                columns: new[] { "기사Id", "화주UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_운송의뢰상품연결_운송의뢰_id_입고상품_id",
                table: "운송의뢰상품연결",
                columns: new[] { "운송의뢰_id", "입고상품_id" });

            migrationBuilder.CreateIndex(
                name: "IX_운행탐색대상화주_운행탐색Id_화주UserId",
                table: "운행탐색대상화주",
                columns: new[] { "운행탐색Id", "화주UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_운행탐색응답요약_운행탐색Id_화주UserId",
                table: "운행탐색응답요약",
                columns: new[] { "운행탐색Id", "화주UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_입고상품_창고_id_소유자_user_id_상태",
                table: "입고상품",
                columns: new[] { "창고_id", "소유자_user_id", "상태" });

            migrationBuilder.CreateIndex(
                name: "IX_입고요청_창고_id_주문자_user_id_상태",
                table: "입고요청",
                columns: new[] { "창고_id", "주문자_user_id", "상태" });

            migrationBuilder.CreateIndex(
                name: "IX_재고이력_입고상품_id_처리일시",
                table: "재고이력",
                columns: new[] { "입고상품_id", "처리일시" });

            migrationBuilder.CreateIndex(
                name: "IX_창고_소유자_user_id_창고명",
                table: "창고",
                columns: new[] { "소유자_user_id", "창고명" });

            migrationBuilder.CreateIndex(
                name: "IX_창고사용자_창고_id_user_id_역할명",
                table: "창고사용자",
                columns: new[] { "창고_id", "user_id", "역할명" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_채널출품_판매상품_id_판매채널계정_id",
                table: "채널출품",
                columns: new[] { "판매상품_id", "판매채널계정_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_판매상품_입고상품_id_판매sku",
                table: "판매상품",
                columns: new[] { "입고상품_id", "판매sku" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_판매채널계정_user_id_채널종류_상점명",
                table: "판매채널계정",
                columns: new[] { "user_id", "채널종류", "상점명" });

            migrationBuilder.CreateTable(
                name: "배차추천_알림_Outbox",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    dispatch_waiting_id = table.Column<long>(type: "bigint", nullable: false),
                    request_id = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    driver_id = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    recommendation_round = table.Column<int>(type: "int", nullable: false),
                    title = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    body = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    data_json = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    retry_count = table.Column<int>(type: "int", nullable: false),
                    last_attempted_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_배차추천_알림_Outbox", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_배차추천_알림_Outbox_dispatch_waiting_id_driver_id_recommendation_~",
                table: "배차추천_알림_Outbox",
                columns: new[] { "dispatch_waiting_id", "driver_id", "recommendation_round" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_배차추천_알림_Outbox_status_created_at",
                table: "배차추천_알림_Outbox",
                columns: new[] { "status", "created_at" });

            migrationBuilder.AddColumn<int>(
                name: "business_type",
                table: "배차_대기",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "source_request_id",
                table: "배차_대기",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "source_type",
                table: "배차_대기",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "canceled_at",
                table: "결제",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "common_status",
                table: "결제",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "currency",
                table: "결제",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "external_transaction_no",
                table: "결제",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "order_name",
                table: "결제",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "provider_type",
                table: "결제",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "raw_response_json",
                table: "결제",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "target_id",
                table: "결제",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "target_type",
                table: "결제",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "결제승인완료_Outbox",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    payment_record_id = table.Column<long>(type: "bigint", nullable: false),
                    payment_id = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    target_type = table.Column<int>(type: "int", nullable: false),
                    target_id = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    provider_type = table.Column<int>(type: "int", nullable: false),
                    amount = table.Column<int>(type: "int", nullable: false),
                    currency = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    approved_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    payload_json = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    retry_count = table.Column<int>(type: "int", nullable: false),
                    last_attempted_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_결제승인완료_Outbox", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "살뜰_콘텐츠보상정책",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    reward_type = table.Column<int>(type: "int", nullable: false),
                    point_amount = table.Column<int>(type: "int", nullable: false),
                    discount_rate = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    discount_amount = table.Column<int>(type: "int", nullable: false),
                    minimum_watch_seconds = table.Column<int>(type: "int", nullable: false),
                    required_watch_ratio = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    one_time_per_user = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    max_discount_amount = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_살뜰_콘텐츠보상정책", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "살뜰_콘텐츠보상지급",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    user_id = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    content_id = table.Column<long>(type: "bigint", nullable: false),
                    reward_type = table.Column<int>(type: "int", nullable: false),
                    granted_points = table.Column<int>(type: "int", nullable: false),
                    discount_rate = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    discount_amount = table.Column<int>(type: "int", nullable: false),
                    is_used_in_payment = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    granted_at = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    used_at = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_살뜰_콘텐츠보상지급", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "살뜰_공통콘텐츠",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    title = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    content_type = table.Column<int>(type: "int", nullable: false),
                    image_url = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    video_url = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    external_link_url = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    placement_flags = table.Column<int>(type: "int", nullable: false),
                    show_to_driver = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    show_to_shipper = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    show_to_admin = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    start_at = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    end_at = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    reward_policy_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_살뜰_공통콘텐츠", x => x.id);
                    table.ForeignKey(
                        name: "FK_살뜰_공통콘텐츠_살뜰_콘텐츠보상정책_reward_policy_id",
                        column: x => x.reward_policy_id,
                        principalTable: "살뜰_콘텐츠보상정책",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "살뜰_콘텐츠시청세션",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    user_id = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    content_id = table.Column<long>(type: "bigint", nullable: false),
                    video_total_seconds = table.Column<int>(type: "int", nullable: false),
                    watched_seconds = table.Column<int>(type: "int", nullable: false),
                    is_completed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    is_reward_granted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    started_at = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    last_progress_at = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    completed_at = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_살뜰_콘텐츠시청세션", x => x.id);
                    table.ForeignKey(
                        name: "FK_살뜰_콘텐츠시청세션_살뜰_공통콘텐츠_content_id",
                        column: x => x.content_id,
                        principalTable: "살뜰_공통콘텐츠",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_결제승인완료_Outbox_payment_record_id",
                table: "결제승인완료_Outbox",
                column: "payment_record_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_결제승인완료_Outbox_status_created_at",
                table: "결제승인완료_Outbox",
                columns: new[] { "status", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_살뜰_공통콘텐츠_is_active_start_at_end_at",
                table: "살뜰_공통콘텐츠",
                columns: new[] { "is_active", "start_at", "end_at" });

            migrationBuilder.CreateIndex(
                name: "IX_살뜰_공통콘텐츠_reward_policy_id",
                table: "살뜰_공통콘텐츠",
                column: "reward_policy_id");

            migrationBuilder.CreateIndex(
                name: "IX_살뜰_콘텐츠보상지급_user_id_content_id",
                table: "살뜰_콘텐츠보상지급",
                columns: new[] { "user_id", "content_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_살뜰_콘텐츠보상지급_user_id_is_used_in_payment_granted_at",
                table: "살뜰_콘텐츠보상지급",
                columns: new[] { "user_id", "is_used_in_payment", "granted_at" });

            migrationBuilder.CreateIndex(
                name: "IX_살뜰_콘텐츠시청세션_content_id",
                table: "살뜰_콘텐츠시청세션",
                column: "content_id");

            migrationBuilder.CreateIndex(
                name: "IX_살뜰_콘텐츠시청세션_user_id_content_id_started_at",
                table: "살뜰_콘텐츠시청세션",
                columns: new[] { "user_id", "content_id", "started_at" });

            migrationBuilder.AddColumn<decimal>(
                name: "경도",
                table: "창고",
                type: "decimal(10,7)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "기본창고여부",
                table: "창고",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "소유자유형",
                table: "창고",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "주문자")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "위도",
                table: "창고",
                type: "decimal(10,7)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "창고유형",
                table: "창고",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "가상창고")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "운송의뢰_id",
                table: "입고요청",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<long>(
                name: "주문_id",
                table: "입고요청",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "주문참조번호",
                table: "입고요청",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<long>(
                name: "출고예정_id",
                table: "입고요청",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "판매자_user_id",
                table: "입고요청",
                type: "varchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "재고이동",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    창고_id = table.Column<long>(type: "bigint", nullable: false),
                    입고상품_id = table.Column<long>(type: "bigint", nullable: true),
                    판매상품_id = table.Column<long>(type: "bigint", nullable: true),
                    상품명 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    sku = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    이동유형 = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    수량 = table.Column<int>(type: "int", nullable: false),
                    주문_id = table.Column<long>(type: "bigint", nullable: true),
                    주문참조번호 = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    출고예정_id = table.Column<long>(type: "bigint", nullable: true),
                    입고요청_id = table.Column<long>(type: "bigint", nullable: true),
                    운송의뢰_id = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    처리_user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    메모 = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    발생일시 = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_재고이동", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "출고예정",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    주문_id = table.Column<long>(type: "bigint", nullable: true),
                    주문참조번호 = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    판매상품_id = table.Column<long>(type: "bigint", nullable: true),
                    입고상품_id = table.Column<long>(type: "bigint", nullable: true),
                    판매자_user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    주문자_user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    출고창고_id = table.Column<long>(type: "bigint", nullable: false),
                    상품명 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    sku = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    수량 = table.Column<int>(type: "int", nullable: false),
                    상태 = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    운송의뢰_id = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    입고요청_id = table.Column<long>(type: "bigint", nullable: true),
                    출고처리일시 = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_출고예정", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "살뜰_혜택정책",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    policy_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    source_type = table.Column<int>(type: "int", nullable: false),
                    target_type = table.Column<int>(type: "int", nullable: false),
                    benefit_type = table.Column<int>(type: "int", nullable: false),
                    point_amount = table.Column<int>(type: "int", nullable: false),
                    discount_rate = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    discount_amount = table.Column<int>(type: "int", nullable: false),
                    max_discount_amount = table.Column<int>(type: "int", nullable: true),
                    allow_stack = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    start_at = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    end_at = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    per_user_limit = table.Column<int>(type: "int", nullable: true),
                    monthly_limit = table.Column<int>(type: "int", nullable: true),
                    expiry_days = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_살뜰_혜택정책", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "살뜰_오프라인모임",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    meeting_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    place_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    start_at = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    end_at = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    benefit_policy_id = table.Column<long>(type: "bigint", nullable: true),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_살뜰_오프라인모임", x => x.id);
                    table.ForeignKey(
                        name: "FK_살뜰_오프라인모임_살뜰_혜택정책_benefit_policy_id",
                        column: x => x.benefit_policy_id,
                        principalTable: "살뜰_혜택정책",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "살뜰_혜택자격",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    user_id = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    source_type = table.Column<int>(type: "int", nullable: false),
                    source_id = table.Column<long>(type: "bigint", nullable: false),
                    benefit_policy_id = table.Column<long>(type: "bigint", nullable: false),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    granted_at = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    usage_count = table.Column<int>(type: "int", nullable: false),
                    last_used_at = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_살뜰_혜택자격", x => x.id);
                    table.ForeignKey(
                        name: "FK_살뜰_혜택자격_살뜰_혜택정책_benefit_policy_id",
                        column: x => x.benefit_policy_id,
                        principalTable: "살뜰_혜택정책",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "살뜰_모임참석",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    meeting_id = table.Column<long>(type: "bigint", nullable: false),
                    user_id = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    attendance_status = table.Column<int>(type: "int", nullable: false),
                    requested_at = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    confirmed_at = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    confirmation_method = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    admin_memo = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_살뜰_모임참석", x => x.id);
                    table.ForeignKey(
                        name: "FK_살뜰_모임참석_살뜰_오프라인모임_meeting_id",
                        column: x => x.meeting_id,
                        principalTable: "살뜰_오프라인모임",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_창고_소유자_user_id_소유자유형_기본창고여부",
                table: "창고",
                columns: new[] { "소유자_user_id", "소유자유형", "기본창고여부" });

            migrationBuilder.CreateIndex(
                name: "IX_입고요청_주문_id_주문자_user_id",
                table: "입고요청",
                columns: new[] { "주문_id", "주문자_user_id" });

            migrationBuilder.CreateIndex(
                name: "IX_입고요청_출고예정_id",
                table: "입고요청",
                column: "출고예정_id");

            migrationBuilder.CreateIndex(
                name: "IX_재고이동_주문_id_이동유형",
                table: "재고이동",
                columns: new[] { "주문_id", "이동유형" });

            migrationBuilder.CreateIndex(
                name: "IX_재고이동_창고_id_sku_발생일시",
                table: "재고이동",
                columns: new[] { "창고_id", "sku", "발생일시" });

            migrationBuilder.CreateIndex(
                name: "IX_출고예정_입고요청_id",
                table: "출고예정",
                column: "입고요청_id");

            migrationBuilder.CreateIndex(
                name: "IX_출고예정_주문_id_판매자_user_id",
                table: "출고예정",
                columns: new[] { "주문_id", "판매자_user_id" });

            migrationBuilder.CreateIndex(
                name: "IX_출고예정_판매자_user_id_상태_created_at",
                table: "출고예정",
                columns: new[] { "판매자_user_id", "상태", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_살뜰_모임참석_meeting_id_user_id",
                table: "살뜰_모임참석",
                columns: new[] { "meeting_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_살뜰_모임참석_user_id_attendance_status_confirmed_at",
                table: "살뜰_모임참석",
                columns: new[] { "user_id", "attendance_status", "confirmed_at" });

            migrationBuilder.CreateIndex(
                name: "IX_살뜰_오프라인모임_benefit_policy_id",
                table: "살뜰_오프라인모임",
                column: "benefit_policy_id");

            migrationBuilder.CreateIndex(
                name: "IX_살뜰_오프라인모임_status_is_active_start_at_end_at",
                table: "살뜰_오프라인모임",
                columns: new[] { "status", "is_active", "start_at", "end_at" });

            migrationBuilder.CreateIndex(
                name: "IX_살뜰_혜택자격_benefit_policy_id",
                table: "살뜰_혜택자격",
                column: "benefit_policy_id");

            migrationBuilder.CreateIndex(
                name: "IX_살뜰_혜택자격_user_id_is_active_expires_at",
                table: "살뜰_혜택자격",
                columns: new[] { "user_id", "is_active", "expires_at" });

            migrationBuilder.CreateIndex(
                name: "IX_살뜰_혜택자격_user_id_source_type_source_id",
                table: "살뜰_혜택자격",
                columns: new[] { "user_id", "source_type", "source_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_살뜰_혜택정책_source_type_target_type_is_active",
                table: "살뜰_혜택정책",
                columns: new[] { "source_type", "target_type", "is_active" });

            migrationBuilder.CreateIndex(
                name: "IX_살뜰_혜택정책_start_at_end_at",
                table: "살뜰_혜택정책",
                columns: new[] { "start_at", "end_at" });

            migrationBuilder.DropTable(
                name: "살뜰_모임참석");

            migrationBuilder.DropTable(
                name: "살뜰_혜택자격");

            migrationBuilder.DropTable(
                name: "살뜰_오프라인모임");

            migrationBuilder.DropTable(
                name: "살뜰_혜택정책");

            migrationBuilder.AddColumn<string>(
                name: "국가코드",
                table: "창고",
                type: "varchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "관세사프로필",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    참여자_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    사무소명 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    관세사등록번호 = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    담당지역 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    전문품목메모 = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    수입전문여부 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    수출전문여부 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    수임가능여부 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    관리자승인여부 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_관세사프로필", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "상품물류자산",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    상품_id = table.Column<long>(type: "bigint", nullable: false),
                    주문_id = table.Column<long>(type: "bigint", nullable: true),
                    통관절차_id = table.Column<long>(type: "bigint", nullable: true),
                    자산유형 = table.Column<int>(type: "int", nullable: false),
                    파일url = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    설명 = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    등록자_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    상세이미지사용가능여부 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    등록시각 = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_상품물류자산", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "상품상세이미지생성작업",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    상품_id = table.Column<long>(type: "bigint", nullable: false),
                    주문_id = table.Column<long>(type: "bigint", nullable: true),
                    통관절차_id = table.Column<long>(type: "bigint", nullable: true),
                    요청자_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    상태 = table.Column<int>(type: "int", nullable: false),
                    생성프롬프트 = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    원본자산참조json = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    오류내용 = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    관련생성이미지작업_id = table.Column<long>(type: "bigint", nullable: true),
                    생성시각 = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    완료시각 = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_상품상세이미지생성작업", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "상품판매이미지초안",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    상품_id = table.Column<long>(type: "bigint", nullable: false),
                    생성작업_id = table.Column<long>(type: "bigint", nullable: false),
                    작성자_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    대표이미지url = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    이미지목록json = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    원본자산참조json = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    생성근거요약 = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    판매채널전송가능여부 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    생성시각 = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_상품판매이미지초안", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "통관수임",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    통관절차_id = table.Column<long>(type: "bigint", nullable: false),
                    관세사_참여자_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    상태 = table.Column<int>(type: "int", nullable: false),
                    요청시각 = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    확정시각 = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    메모 = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_통관수임", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "통관절차",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    주문_id = table.Column<long>(type: "bigint", nullable: true),
                    주문참조번호 = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    출고예정_id = table.Column<long>(type: "bigint", nullable: true),
                    입고요청_id = table.Column<long>(type: "bigint", nullable: true),
                    출고창고_id = table.Column<long>(type: "bigint", nullable: false),
                    입고창고_id = table.Column<long>(type: "bigint", nullable: false),
                    물류거래방향 = table.Column<int>(type: "int", nullable: false),
                    대표상품명 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    상태 = table.Column<int>(type: "int", nullable: false),
                    확정관세사_참여자_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    메모 = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_통관절차", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "통관조회연동",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    주문_id = table.Column<long>(type: "bigint", nullable: false),
                    사용자_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    통관절차_id = table.Column<long>(type: "bigint", nullable: false),
                    개인통관고유부호_암호문 = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    화물관리번호 = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    master_bl = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    house_bl = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    사용자조회동의여부 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    동의시각 = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    연동상태 = table.Column<int>(type: "int", nullable: false),
                    마지막진행단계 = table.Column<int>(type: "int", nullable: false),
                    마지막조회시각 = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    마지막오류 = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_통관조회연동", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "살뜰참여자",
                columns: table => new
                {
                    id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    표시이름 = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    가입시각 = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    활성화여부 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_살뜰참여자", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "살뜰참여자역할",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    참여자_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    역할유형 = table.Column<int>(type: "int", nullable: false),
                    활성화여부 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    부여시각 = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_살뜰참여자역할", x => x.id);
                    table.ForeignKey(
                        name: "FK_살뜰참여자역할_살뜰참여자_참여자_id",
                        column: x => x.참여자_id,
                        principalTable: "살뜰참여자",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_창고_국가코드",
                table: "창고",
                column: "국가코드");

            migrationBuilder.CreateIndex(
                name: "IX_관세사프로필_관리자승인여부_수임가능여부",
                table: "관세사프로필",
                columns: new[] { "관리자승인여부", "수임가능여부" });

            migrationBuilder.CreateIndex(
                name: "IX_관세사프로필_참여자_id",
                table: "관세사프로필",
                column: "참여자_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_상품물류자산_상품_id_자산유형_등록시각",
                table: "상품물류자산",
                columns: new[] { "상품_id", "자산유형", "등록시각" });

            migrationBuilder.CreateIndex(
                name: "IX_상품물류자산_주문_id_통관절차_id",
                table: "상품물류자산",
                columns: new[] { "주문_id", "통관절차_id" });

            migrationBuilder.CreateIndex(
                name: "IX_상품상세이미지생성작업_관련생성이미지작업_id",
                table: "상품상세이미지생성작업",
                column: "관련생성이미지작업_id");

            migrationBuilder.CreateIndex(
                name: "IX_상품상세이미지생성작업_상품_id_상태_생성시각",
                table: "상품상세이미지생성작업",
                columns: new[] { "상품_id", "상태", "생성시각" });

            migrationBuilder.CreateIndex(
                name: "IX_상품판매이미지초안_상품_id_생성시각",
                table: "상품판매이미지초안",
                columns: new[] { "상품_id", "생성시각" });

            migrationBuilder.CreateIndex(
                name: "IX_상품판매이미지초안_생성작업_id",
                table: "상품판매이미지초안",
                column: "생성작업_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_통관수임_통관절차_id_관세사_참여자_id_상태",
                table: "통관수임",
                columns: new[] { "통관절차_id", "관세사_참여자_id", "상태" });

            migrationBuilder.CreateIndex(
                name: "IX_통관절차_주문_id_주문참조번호_상태",
                table: "통관절차",
                columns: new[] { "주문_id", "주문참조번호", "상태" });

            migrationBuilder.CreateIndex(
                name: "IX_통관절차_출고예정_id_입고요청_id",
                table: "통관절차",
                columns: new[] { "출고예정_id", "입고요청_id" });

            migrationBuilder.CreateIndex(
                name: "IX_통관조회연동_주문_id_사용자_id",
                table: "통관조회연동",
                columns: new[] { "주문_id", "사용자_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_통관조회연동_통관절차_id_연동상태_마지막조회시각",
                table: "통관조회연동",
                columns: new[] { "통관절차_id", "연동상태", "마지막조회시각" });

            migrationBuilder.CreateIndex(
                name: "IX_통관조회연동_화물관리번호_master_bl_house_bl",
                table: "통관조회연동",
                columns: new[] { "화물관리번호", "master_bl", "house_bl" });

            migrationBuilder.CreateIndex(
                name: "IX_살뜰참여자_활성화여부",
                table: "살뜰참여자",
                column: "활성화여부");

            migrationBuilder.CreateIndex(
                name: "IX_살뜰참여자역할_참여자_id_역할유형_활성화여부",
                table: "살뜰참여자역할",
                columns: new[] { "참여자_id", "역할유형", "활성화여부" });

            migrationBuilder.CreateTable(
                name: "감사메시지",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    상품_id = table.Column<long>(type: "bigint", nullable: false),
                    주문_id = table.Column<long>(type: "bigint", nullable: true),
                    통관절차_id = table.Column<long>(type: "bigint", nullable: true),
                    발신자구분 = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    발신참여자_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    대상역할 = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    대상참여자_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    대상표시명 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    메시지내용 = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    공개가능여부 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    수신자에게전달여부 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    검수상태 = table.Column<int>(type: "int", nullable: false),
                    작성일시 = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_감사메시지", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_감사메시지_대상역할_대상참여자_id_작성일시",
                table: "감사메시지",
                columns: new[] { "대상역할", "대상참여자_id", "작성일시" });

            migrationBuilder.CreateIndex(
                name: "IX_감사메시지_상품_id_작성일시",
                table: "감사메시지",
                columns: new[] { "상품_id", "작성일시" });

            migrationBuilder.CreateIndex(
                name: "IX_감사메시지_통관절차_id_주문_id",
                table: "감사메시지",
                columns: new[] { "통관절차_id", "주문_id" });

            migrationBuilder.CreateTable(
                name: "상품식별코드맵",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    코드값 = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    코드유형 = table.Column<int>(type: "int", nullable: false),
                    상품_id = table.Column<long>(type: "bigint", nullable: false),
                    활성여부 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_상품식별코드맵", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "연락처공개동의",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    인연연결요청_id = table.Column<long>(type: "bigint", nullable: false),
                    동의자_참여자_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    프로필공개 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    업체명공개 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    이메일공개 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    전화번호공개 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    카카오채널공개 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    판매채널공개 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    제공목적 = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    동의일시 = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    철회일시 = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_연락처공개동의", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "인연연결요청",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    요청자_참여자_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    요청자_역할 = table.Column<int>(type: "int", nullable: false),
                    대상자_참여자_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    대상자_역할 = table.Column<int>(type: "int", nullable: false),
                    감사메시지_id = table.Column<long>(type: "bigint", nullable: true),
                    주문_id = table.Column<long>(type: "bigint", nullable: true),
                    통관절차_id = table.Column<long>(type: "bigint", nullable: true),
                    요청목적 = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    요청메시지 = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    상태 = table.Column<int>(type: "int", nullable: false),
                    요청일시 = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    응답일시 = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    거절사유 = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_인연연결요청", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_상품식별코드맵_상품_id_활성여부",
                table: "상품식별코드맵",
                columns: new[] { "상품_id", "활성여부" });

            migrationBuilder.CreateIndex(
                name: "IX_상품식별코드맵_코드값",
                table: "상품식별코드맵",
                column: "코드값",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_연락처공개동의_동의자_참여자_id_동의일시",
                table: "연락처공개동의",
                columns: new[] { "동의자_참여자_id", "동의일시" });

            migrationBuilder.CreateIndex(
                name: "IX_연락처공개동의_인연연결요청_id_동의자_참여자_id",
                table: "연락처공개동의",
                columns: new[] { "인연연결요청_id", "동의자_참여자_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_인연연결요청_감사메시지_id_주문_id_통관절차_id",
                table: "인연연결요청",
                columns: new[] { "감사메시지_id", "주문_id", "통관절차_id" });

            migrationBuilder.CreateIndex(
                name: "IX_인연연결요청_대상자_참여자_id_상태_요청일시",
                table: "인연연결요청",
                columns: new[] { "대상자_참여자_id", "상태", "요청일시" });

            migrationBuilder.CreateIndex(
                name: "IX_인연연결요청_요청자_참여자_id_상태_요청일시",
                table: "인연연결요청",
                columns: new[] { "요청자_참여자_id", "상태", "요청일시" });

            migrationBuilder.AddColumn<string>(
                name: "물류대행지분류",
                table: "창고",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "DeliveryAgency")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "계약메모",
                table: "입고요청",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "계약번호",
                table: "입고요청",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "계약상대방명",
                table: "입고요청",
                type: "varchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "계약시작일",
                table: "입고요청",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "계약유형",
                table: "입고요청",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "StorageOnly")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "계약종료일",
                table: "입고요청",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "보관료일단가",
                table: "입고요청",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "정산방식",
                table: "입고요청",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "통관필요여부",
                table: "입고요청",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "판매수수료율",
                table: "입고요청",
                type: "decimal(9,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "계약메모",
                table: "입고상품",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "계약번호",
                table: "입고상품",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "계약상대방명",
                table: "입고상품",
                type: "varchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "계약시작일",
                table: "입고상품",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "계약유형",
                table: "입고상품",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "StorageOnly")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "계약종료일",
                table: "입고상품",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "보관료일단가",
                table: "입고상품",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "정산방식",
                table: "입고상품",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "통관필요여부",
                table: "입고상품",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "판매수수료율",
                table: "입고상품",
                type: "decimal(9,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "hr_employment_contracts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    worker_user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    worker_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    employer_scope_type = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    employer_scope_id = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    employer_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    contract_type = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    contract_status = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    contract_start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    contract_end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    work_description = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    wage_type = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    wage_amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    minimum_wage_amount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    minimum_wage_check_passed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    minimum_wage_check_message = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    payment_cycle = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    payment_day_of_month = table.Column<int>(type: "int", nullable: false),
                    payment_method = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    bank_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    account_number = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    account_holder_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    signed_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    signed_by_user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    memo = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hr_employment_contracts", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "hr_role_assignments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    scope_type = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    scope_id = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    participant_category = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    role_code = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    role_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    assigned_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    assigned_by_user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    work_schedule_enabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    time_zone_id = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    allowed_days_of_week = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    work_start_local_time = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    work_end_local_time = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    worksite_ip_restriction_enabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    allowed_worksite_ip_ranges = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hr_role_assignments", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "platform_profit_return_policies",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    policy_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    target_participant_category = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    return_rate_percent = table.Column<decimal>(type: "decimal(9,4)", nullable: false),
                    company_reserve_amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    minimum_profit_threshold = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    effective_start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    effective_end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    memo = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platform_profit_return_policies", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "platform_revenue_entries",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    revenue_source = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    source_reference_type = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    source_reference_id = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    payer_user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    related_participant_user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    gross_amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    platform_revenue_amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    currency_code = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    occurred_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    memo = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platform_revenue_entries", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "hr_payroll_schedules",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    contract_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    worker_user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    employer_scope_type = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    employer_scope_id = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    work_period_start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    work_period_end_date = table.Column<DateOnly>(type: "date", nullable: false),
                    scheduled_payment_date = table.Column<DateOnly>(type: "date", nullable: false),
                    planned_amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    currency_code = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    payment_method = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    memo = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hr_payroll_schedules", x => x.id);
                    table.ForeignKey(
                        name: "FK_hr_payroll_schedules_hr_employment_contracts_contract_id",
                        column: x => x.contract_id,
                        principalTable: "hr_employment_contracts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "platform_profit_return_schedules",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    policy_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    participant_user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    participant_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    participant_category = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    period_start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    period_end_date = table.Column<DateOnly>(type: "date", nullable: false),
                    scheduled_payment_date = table.Column<DateOnly>(type: "date", nullable: false),
                    total_platform_revenue_amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    operating_cost_amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    estimated_profit_amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    return_pool_amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    participant_weight = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    planned_return_amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    status = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    memo = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platform_profit_return_schedules", x => x.id);
                    table.ForeignKey(
                        name: "FK_platform_profit_return_schedules_platform_profit_return_poli~",
                        column: x => x.policy_id,
                        principalTable: "platform_profit_return_policies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_hr_employment_contracts_worker_user_id_employer_scope_type_e~",
                table: "hr_employment_contracts",
                columns: new[] { "worker_user_id", "employer_scope_type", "contract_status" });

            migrationBuilder.CreateIndex(
                name: "IX_hr_payroll_schedules_contract_id",
                table: "hr_payroll_schedules",
                column: "contract_id");

            migrationBuilder.CreateIndex(
                name: "IX_hr_payroll_schedules_worker_user_id_scheduled_payment_date_s~",
                table: "hr_payroll_schedules",
                columns: new[] { "worker_user_id", "scheduled_payment_date", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_hr_role_assignments_user_id_scope_type_scope_id_role_code_is~",
                table: "hr_role_assignments",
                columns: new[] { "user_id", "scope_type", "role_code", "is_active" });

            migrationBuilder.CreateIndex(
                name: "IX_platform_profit_return_policies_target_participant_category_~",
                table: "platform_profit_return_policies",
                columns: new[] { "target_participant_category", "is_active", "effective_start_date" });

            migrationBuilder.CreateIndex(
                name: "IX_platform_profit_return_schedules_participant_user_id_schedul~",
                table: "platform_profit_return_schedules",
                columns: new[] { "participant_user_id", "scheduled_payment_date", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_platform_profit_return_schedules_policy_id",
                table: "platform_profit_return_schedules",
                column: "policy_id");

            migrationBuilder.CreateIndex(
                name: "IX_platform_revenue_entries_revenue_source_occurred_at_utc",
                table: "platform_revenue_entries",
                columns: new[] { "revenue_source", "occurred_at_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_platform_revenue_entries_source_reference_type_source_refere~",
                table: "platform_revenue_entries",
                columns: new[] { "source_reference_type", "source_reference_id" });

            migrationBuilder.CreateTable(
                name: "hs_code_catalog_versions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    StandardCode = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CountryCode = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CodeDigits = table.Column<int>(type: "int", nullable: false),
                    Revision = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SourceName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SourceUrl = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    EffectiveTo = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ImportedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Notes = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hs_code_catalog_versions", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "hs_code_platform_agency_experiences",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    HsCode = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AgencyType = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CountryRoute = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CaseStatus = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RiskLevel = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Summary = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RequiredDocumentsJson = table.Column<string>(type: "json", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ContributorUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ContributorConsented = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsPaidDetail = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    PaidAccessPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ContributorRewardRate = table.Column<decimal>(type: "decimal(9,6)", nullable: false),
                    DisclosurePolicy = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CompletedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hs_code_platform_agency_experiences", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "hs_code_entries",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CatalogVersionId = table.Column<long>(type: "bigint", nullable: false),
                    Code = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NormalizedCode = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ParentNormalizedCode = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Level = table.Column<int>(type: "int", nullable: false),
                    KoreanName = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EnglishName = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SearchKeywords = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hs_code_entries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hs_code_entries_hs_code_catalog_versions_CatalogVersionId",
                        column: x => x.CatalogVersionId,
                        principalTable: "hs_code_catalog_versions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "hs_code_classification_cases",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    HsCodeEntryId = table.Column<long>(type: "bigint", nullable: true),
                    HsCode = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CountryCode = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SourceType = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SourceReferenceNo = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SourceUrl = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IssuingAuthority = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DecidedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ProductName = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GoodsDescription = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DecisionReason = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsPublicOfficialCase = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hs_code_classification_cases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hs_code_classification_cases_hs_code_entries_HsCodeEntryId",
                        column: x => x.HsCodeEntryId,
                        principalTable: "hs_code_entries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_hs_code_catalog_versions_CountryCode_IsActive_EffectiveFrom",
                table: "hs_code_catalog_versions",
                columns: new[] { "CountryCode", "IsActive", "EffectiveFrom" });

            migrationBuilder.CreateIndex(
                name: "IX_hs_code_catalog_versions_StandardCode_CountryCode_Revision_C~",
                table: "hs_code_catalog_versions",
                columns: new[] { "StandardCode", "CountryCode", "Revision", "CodeDigits" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hs_code_classification_cases_CountryCode_HsCode_DecidedAt",
                table: "hs_code_classification_cases",
                columns: new[] { "CountryCode", "HsCode", "DecidedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_hs_code_classification_cases_HsCodeEntryId",
                table: "hs_code_classification_cases",
                column: "HsCodeEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_hs_code_classification_cases_ProductName",
                table: "hs_code_classification_cases",
                column: "ProductName");

            migrationBuilder.CreateIndex(
                name: "IX_hs_code_classification_cases_SourceType_SourceReferenceNo",
                table: "hs_code_classification_cases",
                columns: new[] { "SourceType", "SourceReferenceNo" });

            migrationBuilder.CreateIndex(
                name: "IX_hs_code_entries_CatalogVersionId_NormalizedCode",
                table: "hs_code_entries",
                columns: new[] { "CatalogVersionId", "NormalizedCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hs_code_entries_CatalogVersionId_ParentNormalizedCode",
                table: "hs_code_entries",
                columns: new[] { "CatalogVersionId", "ParentNormalizedCode" });

            migrationBuilder.CreateIndex(
                name: "IX_hs_code_entries_KoreanName",
                table: "hs_code_entries",
                column: "KoreanName");

            migrationBuilder.CreateIndex(
                name: "IX_hs_code_entries_NormalizedCode_IsActive",
                table: "hs_code_entries",
                columns: new[] { "NormalizedCode", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_hs_code_platform_agency_experiences_ContributorUserId_Contri~",
                table: "hs_code_platform_agency_experiences",
                columns: new[] { "ContributorUserId", "ContributorConsented" });

            migrationBuilder.CreateIndex(
                name: "IX_hs_code_platform_agency_experiences_HsCode_AgencyType_Countr~",
                table: "hs_code_platform_agency_experiences",
                columns: new[] { "HsCode", "AgencyType", "CountryRoute" });

            migrationBuilder.CreateIndex(
                name: "IX_hs_code_platform_agency_experiences_HsCode_ContributorConsen~",
                table: "hs_code_platform_agency_experiences",
                columns: new[] { "HsCode", "ContributorConsented", "IsPaidDetail" });

            migrationBuilder.AddColumn<int>(
                name: "BusinessCategory",
                table: "hs_code_entries",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCategoryReason",
                table: "hs_code_entries",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.Sql("UPDATE hs_code_entries\nSET BusinessCategory = CASE\n        WHEN CAST(SUBSTRING(NormalizedCode, 1, 2) AS UNSIGNED) BETWEEN 1 AND 24 THEN 10\n        WHEN NormalizedCode REGEXP '^[0-9]{2}' THEN 20\n        ELSE 0\n    END,\n    BusinessCategoryReason = CASE\n        WHEN CAST(SUBSTRING(NormalizedCode, 1, 2) AS UNSIGNED) BETWEEN 1 AND 24 THEN 'HS chapter 01-24 is treated as food or food-adjacent cargo.'\n        WHEN NormalizedCode REGEXP '^[0-9]{2}' THEN 'HS chapter is outside 01-24 and treated as general cargo.'\n        ELSE 'HS chapter could not be parsed.'\n    END;");

            migrationBuilder.CreateIndex(
                name: "IX_hs_code_entries_CatalogVersionId_BusinessCategory_IsActive",
                table: "hs_code_entries",
                columns: new[] { "CatalogVersionId", "BusinessCategory", "IsActive" });

            migrationBuilder.CreateTable(
                name: "hs_code_entry_risk_tags",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    HsCodeEntryId = table.Column<long>(type: "bigint", nullable: false),
                    TagType = table.Column<int>(type: "int", nullable: false),
                    Label = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Reason = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Source = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hs_code_entry_risk_tags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hs_code_entry_risk_tags_hs_code_entries_HsCodeEntryId",
                        column: x => x.HsCodeEntryId,
                        principalTable: "hs_code_entries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.Sql("INSERT INTO hs_code_entry_risk_tags\n    (HsCodeEntryId, TagType, Label, Reason, Source, IsActive, CreatedAtUtc, UpdatedAtUtc)\nSELECT Id, 10, '식품 관련', 'HS chapter 01-24 is treated as food or food-adjacent cargo.', 10, TRUE, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6)\nFROM hs_code_entries\nWHERE NormalizedCode REGEXP '^[0-9]{2}'\n  AND CAST(SUBSTRING(NormalizedCode, 1, 2) AS UNSIGNED) BETWEEN 1 AND 24;");

            migrationBuilder.Sql("INSERT INTO hs_code_entry_risk_tags\n    (HsCodeEntryId, TagType, Label, Reason, Source, IsActive, CreatedAtUtc, UpdatedAtUtc)\nSELECT Id, 20, '검역/식품신고 확인', 'Food-related HS codes may require quarantine, ingredient, label, or import notification review.', 10, TRUE, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6)\nFROM hs_code_entries\nWHERE NormalizedCode REGEXP '^[0-9]{2}'\n  AND CAST(SUBSTRING(NormalizedCode, 1, 2) AS UNSIGNED) BETWEEN 1 AND 24;");

            migrationBuilder.Sql("INSERT INTO hs_code_entry_risk_tags\n    (HsCodeEntryId, TagType, Label, Reason, Source, IsActive, CreatedAtUtc, UpdatedAtUtc)\nSELECT Id, 30, '조제식품/보충제 검토', 'Chapter 21 can include prepared food products that need ingredient and claim review.', 10, TRUE, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6)\nFROM hs_code_entries\nWHERE NormalizedCode REGEXP '^[0-9]{2}'\n  AND CAST(SUBSTRING(NormalizedCode, 1, 2) AS UNSIGNED) = 21;");

            migrationBuilder.Sql("INSERT INTO hs_code_entry_risk_tags\n    (HsCodeEntryId, TagType, Label, Reason, Source, IsActive, CreatedAtUtc, UpdatedAtUtc)\nSELECT Id, 50, '화학물질 확인', 'Chemical chapters may require substance, safety, or hazardous cargo review.', 10, TRUE, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6)\nFROM hs_code_entries\nWHERE NormalizedCode REGEXP '^[0-9]{2}'\n  AND CAST(SUBSTRING(NormalizedCode, 1, 2) AS UNSIGNED) BETWEEN 28 AND 38;");

            migrationBuilder.Sql("INSERT INTO hs_code_entry_risk_tags\n    (HsCodeEntryId, TagType, Label, Reason, Source, IsActive, CreatedAtUtc, UpdatedAtUtc)\nSELECT Id, 40, '섬유/의류', 'Textile chapters often need material composition and origin checks.', 10, TRUE, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6)\nFROM hs_code_entries\nWHERE NormalizedCode REGEXP '^[0-9]{2}'\n  AND CAST(SUBSTRING(NormalizedCode, 1, 2) AS UNSIGNED) BETWEEN 50 AND 63;");

            migrationBuilder.Sql("INSERT INTO hs_code_entry_risk_tags\n    (HsCodeEntryId, TagType, Label, Reason, Source, IsActive, CreatedAtUtc, UpdatedAtUtc)\nSELECT Id, 60, '전기/인증 확인', 'Electrical goods may require certification, radio, or product safety checks.', 10, TRUE, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6)\nFROM hs_code_entries\nWHERE NormalizedCode REGEXP '^[0-9]{2}'\n  AND CAST(SUBSTRING(NormalizedCode, 1, 2) AS UNSIGNED) = 85;");

            migrationBuilder.Sql("INSERT INTO hs_code_entry_risk_tags\n    (HsCodeEntryId, TagType, Label, Reason, Source, IsActive, CreatedAtUtc, UpdatedAtUtc)\nSELECT Id, 70, '배터리 포함 가능', 'Battery-related HS codes need transport and safety document checks.', 10, TRUE, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6)\nFROM hs_code_entries\nWHERE NormalizedCode LIKE '8506%'\n   OR NormalizedCode LIKE '8507%';");

            migrationBuilder.Sql("INSERT INTO hs_code_entry_risk_tags\n    (HsCodeEntryId, TagType, Label, Reason, Source, IsActive, CreatedAtUtc, UpdatedAtUtc)\nSELECT Id, 80, '가구/생활용품', 'Furniture and fixture chapters may need material and component checks.', 10, TRUE, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6)\nFROM hs_code_entries\nWHERE NormalizedCode REGEXP '^[0-9]{2}'\n  AND CAST(SUBSTRING(NormalizedCode, 1, 2) AS UNSIGNED) = 94;");

            migrationBuilder.Sql("INSERT INTO hs_code_entry_risk_tags\n    (HsCodeEntryId, TagType, Label, Reason, Source, IsActive, CreatedAtUtc, UpdatedAtUtc)\nSELECT Id, 900, '관세사 검토 권장', 'At least one operational risk tag was detected, so broker review is recommended before agency confirmation.', 10, TRUE, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6)\nFROM hs_code_entries\nWHERE NormalizedCode REGEXP '^[0-9]{2}'\n  AND (\n    CAST(SUBSTRING(NormalizedCode, 1, 2) AS UNSIGNED) BETWEEN 1 AND 24\n    OR CAST(SUBSTRING(NormalizedCode, 1, 2) AS UNSIGNED) BETWEEN 28 AND 38\n    OR CAST(SUBSTRING(NormalizedCode, 1, 2) AS UNSIGNED) BETWEEN 50 AND 63\n    OR CAST(SUBSTRING(NormalizedCode, 1, 2) AS UNSIGNED) = 85\n    OR CAST(SUBSTRING(NormalizedCode, 1, 2) AS UNSIGNED) = 94\n  );");

            migrationBuilder.CreateIndex(
                name: "IX_hs_code_entry_risk_tags_HsCodeEntryId_TagType_IsActive",
                table: "hs_code_entry_risk_tags",
                columns: new[] { "HsCodeEntryId", "TagType", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_hs_code_entry_risk_tags_TagType_IsActive",
                table: "hs_code_entry_risk_tags",
                columns: new[] { "TagType", "IsActive" });

            migrationBuilder.CreateTable(
                name: "platform_community_posts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AppKey = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Category = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Title = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Body = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Nickname = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PasswordHash = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platform_community_posts", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_platform_community_posts_AppKey_IsDeleted_CreatedAtUtc",
                table: "platform_community_posts",
                columns: new[] { "AppKey", "IsDeleted", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_platform_community_posts_Category_IsDeleted_CreatedAtUtc",
                table: "platform_community_posts",
                columns: new[] { "Category", "IsDeleted", "CreatedAtUtc" });

            migrationBuilder.CreateTable(
                name: "platform_community_post_attachments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PostId = table.Column<long>(type: "bigint", nullable: false),
                    BucketName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ObjectName = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Url = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OriginalFileName = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ContentType = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    UploadedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platform_community_post_attachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_platform_community_post_attachments_platform_community_posts~",
                        column: x => x.PostId,
                        principalTable: "platform_community_posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_platform_community_post_attachments_PostId_UploadedAtUtc",
                table: "platform_community_post_attachments",
                columns: new[] { "PostId", "UploadedAtUtc" });

            migrationBuilder.DropIndex(
                name: "IX_platform_community_posts_AppKey_IsDeleted_CreatedAtUtc",
                table: "platform_community_posts");

            migrationBuilder.AddColumn<int>(
                name: "CommentCount",
                table: "platform_community_posts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsOperatorPinned",
                table: "platform_community_posts",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastEngagedAtUtc",
                table: "platform_community_posts",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OperatorPinnedAtUtc",
                table: "platform_community_posts",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RecommendationCount",
                table: "platform_community_posts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SharedLinkUrl",
                table: "platform_community_posts",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "platform_community_post_comments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PostId = table.Column<long>(type: "bigint", nullable: false),
                    Nickname = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Body = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PasswordHash = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platform_community_post_comments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_platform_community_post_comments_platform_community_posts_Po~",
                        column: x => x.PostId,
                        principalTable: "platform_community_posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "platform_community_post_recommendations",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PostId = table.Column<long>(type: "bigint", nullable: false),
                    RecommenderKey = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platform_community_post_recommendations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_platform_community_post_recommendations_platform_community_p~",
                        column: x => x.PostId,
                        principalTable: "platform_community_posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_platform_community_posts_AppKey_IsDeleted_IsOperatorPinned_O~",
                table: "platform_community_posts",
                columns: new[] { "AppKey", "IsDeleted", "IsOperatorPinned", "OperatorPinnedAtUtc", "RecommendationCount", "LastEngagedAtUtc", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_platform_community_post_comments_PostId_IsDeleted_CreatedAtU~",
                table: "platform_community_post_comments",
                columns: new[] { "PostId", "IsDeleted", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_platform_community_post_recommendations_PostId_CreatedAtUtc",
                table: "platform_community_post_recommendations",
                columns: new[] { "PostId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_platform_community_post_recommendations_PostId_RecommenderKey",
                table: "platform_community_post_recommendations",
                columns: new[] { "PostId", "RecommenderKey" },
                unique: true);

            migrationBuilder.AddColumn<int>(
                name: "CommentCount",
                table: "platform_community_post_attachments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "platform_community_post_attachment_comments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AttachmentId = table.Column<long>(type: "bigint", nullable: false),
                    Nickname = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Body = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PasswordHash = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platform_community_post_attachment_comments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_platform_community_post_attachment_comments_platform_communi~",
                        column: x => x.AttachmentId,
                        principalTable: "platform_community_post_attachments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_platform_community_post_attachment_comments_AttachmentId_IsD~",
                table: "platform_community_post_attachment_comments",
                columns: new[] { "AttachmentId", "IsDeleted", "CreatedAtUtc" });

            migrationBuilder.AddColumn<bool>(
                name: "IsOperatorHidden",
                table: "platform_community_post_comments",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ReportCount",
                table: "platform_community_post_comments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsOperatorHidden",
                table: "platform_community_post_attachment_comments",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ReportCount",
                table: "platform_community_post_attachment_comments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_post_comments_visible_created",
                table: "platform_community_post_comments",
                columns: new[] { "PostId", "IsDeleted", "IsOperatorHidden", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_attachment_comments_visible_created",
                table: "platform_community_post_attachment_comments",
                columns: new[] { "AttachmentId", "IsDeleted", "IsOperatorHidden", "CreatedAtUtc" });

            migrationBuilder.CreateTable(
                name: "work_relationship_snapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ActorUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ActorAnonymousLabel = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ActorRoleCode = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ActorRoleName = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    WorkDomain = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    WorkProcess = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ActionCode = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ActionLabel = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RelatedEntityType = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RelatedEntityId = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RelatedDisplayLabel = table.Column<string>(type: "varchar(240)", maxLength: 240, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CounterpartyUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CounterpartyAnonymousLabel = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CounterpartyRoleCode = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PrivacyLevel = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Memo = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AppKey = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TraceId = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ClientIpSnapshot = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OccurredAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_work_relationship_snapshots", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_work_relationship_snapshots_ActorUserId_OccurredAtUtc",
                table: "work_relationship_snapshots",
                columns: new[] { "ActorUserId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_work_relationship_snapshots_RelatedEntityType_RelatedEntityI~",
                table: "work_relationship_snapshots",
                columns: new[] { "RelatedEntityType", "RelatedEntityId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_work_relationship_snapshots_WorkDomain_WorkProcess_ActionCod~",
                table: "work_relationship_snapshots",
                columns: new[] { "WorkDomain", "WorkProcess", "ActionCode", "OccurredAtUtc" });

            migrationBuilder.AddColumn<bool>(
                name: "IsReportBoardPost",
                table: "platform_community_posts",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ReportedDisplayName",
                table: "platform_community_posts",
                type: "varchar(40)",
                maxLength: 40,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ReporterDisplayName",
                table: "platform_community_posts",
                type: "varchar(40)",
                maxLength: 40,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_platform_community_posts_IsReportBoardPost_IsDeleted_Created~",
                table: "platform_community_posts",
                columns: new[] { "IsReportBoardPost", "IsDeleted", "CreatedAtUtc" });

            migrationBuilder.AddColumn<bool>(
                name: "계약선행여부",
                table: "입고요청",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "입고생성경로",
                table: "입고요청",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "계약 DB 기반 등록")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "입고흐름유형",
                table: "입고요청",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "ContractBased")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "자동생성여부",
                table: "입고요청",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql("UPDATE `입고요청`\nSET\n    `입고흐름유형` = CASE\n        WHEN `주문_id` IS NOT NULL THEN 'OrderAutoExpected'\n        WHEN COALESCE(`계약번호`, '') = '' THEN 'Unplanned'\n        ELSE 'ContractBased'\n    END,\n    `입고생성경로` = CASE\n        WHEN `주문_id` IS NOT NULL THEN '주문/구매 흐름 자동 생성'\n        WHEN COALESCE(`계약번호`, '') = '' THEN '창고 관리자 수기 등록'\n        ELSE '계약 DB 기반 등록'\n    END,\n    `계약선행여부` = CASE\n        WHEN `주문_id` IS NOT NULL THEN 0\n        WHEN COALESCE(`계약번호`, '') = '' THEN 0\n        ELSE 1\n    END,\n    `자동생성여부` = CASE\n        WHEN `주문_id` IS NOT NULL THEN 1\n        ELSE 0\n    END");

            migrationBuilder.CreateIndex(
                name: "IX_입고요청_입고흐름유형_자동생성여부",
                table: "입고요청",
                columns: new[] { "입고흐름유형", "자동생성여부" });

            migrationBuilder.Sql("\nSET @column_exists = (\n    SELECT COUNT(*)\n    FROM INFORMATION_SCHEMA.COLUMNS\n    WHERE TABLE_SCHEMA = DATABASE()\n      AND TABLE_NAME = '배차_대기'\n      AND COLUMN_NAME = 'confirmed_driver_id'\n);\nSET @sql = IF(@column_exists = 0,\n    'ALTER TABLE `배차_대기` ADD COLUMN `confirmed_driver_id` longtext NULL',\n    'SELECT 1'\n);\nPREPARE stmt FROM @sql;\nEXECUTE stmt;\nDEALLOCATE PREPARE stmt;");

            migrationBuilder.Sql("\nSET @column_exists = (\n    SELECT COUNT(*)\n    FROM INFORMATION_SCHEMA.COLUMNS\n    WHERE TABLE_SCHEMA = DATABASE()\n      AND TABLE_NAME = '배차_대기'\n      AND COLUMN_NAME = 'current_recommended_driver_id'\n);\nSET @sql = IF(@column_exists = 0,\n    'ALTER TABLE `배차_대기` ADD COLUMN `current_recommended_driver_id` longtext NULL',\n    'SELECT 1'\n);\nPREPARE stmt FROM @sql;\nEXECUTE stmt;\nDEALLOCATE PREPARE stmt;");

            migrationBuilder.Sql("\nSET @column_exists = (\n    SELECT COUNT(*)\n    FROM INFORMATION_SCHEMA.COLUMNS\n    WHERE TABLE_SCHEMA = DATABASE()\n      AND TABLE_NAME = '배차_대기'\n      AND COLUMN_NAME = 'exposure_state'\n);\nSET @sql = IF(@column_exists = 0,\n    'ALTER TABLE `배차_대기` ADD COLUMN `exposure_state` int NOT NULL DEFAULT 100',\n    'SELECT 1'\n);\nPREPARE stmt FROM @sql;\nEXECUTE stmt;\nDEALLOCATE PREPARE stmt;");

            migrationBuilder.Sql("\nSET @column_exists = (\n    SELECT COUNT(*)\n    FROM INFORMATION_SCHEMA.COLUMNS\n    WHERE TABLE_SCHEMA = DATABASE()\n      AND TABLE_NAME = '배차_대기'\n      AND COLUMN_NAME = 'last_rejected_driver_id'\n);\nSET @sql = IF(@column_exists = 0,\n    'ALTER TABLE `배차_대기` ADD COLUMN `last_rejected_driver_id` longtext NULL',\n    'SELECT 1'\n);\nPREPARE stmt FROM @sql;\nEXECUTE stmt;\nDEALLOCATE PREPARE stmt;");

            migrationBuilder.Sql("\nSET @column_exists = (\n    SELECT COUNT(*)\n    FROM INFORMATION_SCHEMA.COLUMNS\n    WHERE TABLE_SCHEMA = DATABASE()\n      AND TABLE_NAME = '배차_대기'\n      AND COLUMN_NAME = 'plan_attempts'\n);\nSET @sql = IF(@column_exists = 0,\n    'ALTER TABLE `배차_대기` ADD COLUMN `plan_attempts` int NOT NULL DEFAULT 0',\n    'SELECT 1'\n);\nPREPARE stmt FROM @sql;\nEXECUTE stmt;\nDEALLOCATE PREPARE stmt;");

            migrationBuilder.Sql("\nSET @column_exists = (\n    SELECT COUNT(*)\n    FROM INFORMATION_SCHEMA.COLUMNS\n    WHERE TABLE_SCHEMA = DATABASE()\n      AND TABLE_NAME = '배차_대기'\n      AND COLUMN_NAME = 'public_transition_at'\n);\nSET @sql = IF(@column_exists = 0,\n    'ALTER TABLE `배차_대기` ADD COLUMN `public_transition_at` datetime(6) NULL',\n    'SELECT 1'\n);\nPREPARE stmt FROM @sql;\nEXECUTE stmt;\nDEALLOCATE PREPARE stmt;");

            migrationBuilder.Sql("\nSET @column_exists = (\n    SELECT COUNT(*)\n    FROM INFORMATION_SCHEMA.COLUMNS\n    WHERE TABLE_SCHEMA = DATABASE()\n      AND TABLE_NAME = '배차_대기'\n      AND COLUMN_NAME = 'queue_stage'\n);\nSET @sql = IF(@column_exists = 0,\n    'ALTER TABLE `배차_대기` ADD COLUMN `queue_stage` int NOT NULL DEFAULT 10',\n    'SELECT 1'\n);\nPREPARE stmt FROM @sql;\nEXECUTE stmt;\nDEALLOCATE PREPARE stmt;");

            migrationBuilder.Sql("\nSET @column_exists = (\n    SELECT COUNT(*)\n    FROM INFORMATION_SCHEMA.COLUMNS\n    WHERE TABLE_SCHEMA = DATABASE()\n      AND TABLE_NAME = '배차_대기'\n      AND COLUMN_NAME = 'recommendation_expires_at'\n);\nSET @sql = IF(@column_exists = 0,\n    'ALTER TABLE `배차_대기` ADD COLUMN `recommendation_expires_at` datetime(6) NULL',\n    'SELECT 1'\n);\nPREPARE stmt FROM @sql;\nEXECUTE stmt;\nDEALLOCATE PREPARE stmt;");

            migrationBuilder.Sql("\nSET @column_exists = (\n    SELECT COUNT(*)\n    FROM INFORMATION_SCHEMA.COLUMNS\n    WHERE TABLE_SCHEMA = DATABASE()\n      AND TABLE_NAME = '배차_대기'\n      AND COLUMN_NAME = 'recommendation_round'\n);\nSET @sql = IF(@column_exists = 0,\n    'ALTER TABLE `배차_대기` ADD COLUMN `recommendation_round` int NOT NULL DEFAULT 0',\n    'SELECT 1'\n);\nPREPARE stmt FROM @sql;\nEXECUTE stmt;\nDEALLOCATE PREPARE stmt;");

            migrationBuilder.Sql("\nSET @column_exists = (\n    SELECT COUNT(*)\n    FROM INFORMATION_SCHEMA.COLUMNS\n    WHERE TABLE_SCHEMA = DATABASE()\n      AND TABLE_NAME = '배차_대기'\n      AND COLUMN_NAME = 'recommendation_started_at'\n);\nSET @sql = IF(@column_exists = 0,\n    'ALTER TABLE `배차_대기` ADD COLUMN `recommendation_started_at` datetime(6) NULL',\n    'SELECT 1'\n);\nPREPARE stmt FROM @sql;\nEXECUTE stmt;\nDEALLOCATE PREPARE stmt;");

            migrationBuilder.Sql("\nSET @column_exists = (\n    SELECT COUNT(*)\n    FROM INFORMATION_SCHEMA.COLUMNS\n    WHERE TABLE_SCHEMA = DATABASE()\n      AND TABLE_NAME = '배차_대기'\n      AND COLUMN_NAME = 'row_version'\n);\nSET @sql = IF(@column_exists = 0,\n    'ALTER TABLE `배차_대기` ADD COLUMN `row_version` timestamp(6) NULL',\n    'SELECT 1'\n);\nPREPARE stmt FROM @sql;\nEXECUTE stmt;\nDEALLOCATE PREPARE stmt;");

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

            migrationBuilder.CreateTable(
                name: "platform_community_board_requests",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AppKey = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BoardKey = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Title = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RequestedBy = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RequestReason = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OperatorMemo = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ApprovedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    RejectedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platform_community_board_requests", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_platform_community_board_requests_AppKey_BoardKey_IsDeleted",
                table: "platform_community_board_requests",
                columns: new[] { "AppKey", "BoardKey", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_platform_community_board_requests_AppKey_Status_IsDeleted_Up~",
                table: "platform_community_board_requests",
                columns: new[] { "AppKey", "Status", "IsDeleted", "UpdatedAtUtc" });

            migrationBuilder.AddColumn<string>(
                name: "RoleTag",
                table: "platform_community_posts",
                type: "varchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "WorkflowTag",
                table: "platform_community_posts",
                type: "varchar(60)",
                maxLength: 60,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_platform_community_posts_WorkflowTag_RoleTag_IsDeleted_Creat~",
                table: "platform_community_posts",
                columns: new[] { "WorkflowTag", "RoleTag", "IsDeleted", "CreatedAtUtc" });

            migrationBuilder.AddColumn<string>(
                name: "group_purchase_destination_type_code",
                table: "배차_대기",
                type: "longtext",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "group_purchase_driver_unit_distribution",
                table: "배차_대기",
                type: "tinyint(1)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "group_purchase_unit_distribution_mode_code",
                table: "배차_대기",
                type: "longtext",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "group_purchase_unit_delivery_count",
                table: "배차_대기",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "group_purchase_distribution_responsibility_code",
                table: "배차_대기",
                type: "longtext",
                nullable: true);

            migrationBuilder.Sql("\nSET @column_exists = (\n    SELECT COUNT(*)\n    FROM INFORMATION_SCHEMA.COLUMNS\n    WHERE TABLE_SCHEMA = DATABASE()\n      AND TABLE_NAME = 'driver_shifts'\n      AND COLUMN_NAME = 'today_return_destination'\n);\nSET @sql = IF(@column_exists = 0,\n    'ALTER TABLE `driver_shifts` ADD COLUMN `today_return_destination` longtext NULL',\n    'SELECT 1'\n);\nPREPARE stmt FROM @sql;\nEXECUTE stmt;\nDEALLOCATE PREPARE stmt;");

            migrationBuilder.Sql("\nSET @column_exists = (\n    SELECT COUNT(*)\n    FROM INFORMATION_SCHEMA.COLUMNS\n    WHERE TABLE_SCHEMA = DATABASE()\n      AND TABLE_NAME = 'driver_shifts'\n      AND COLUMN_NAME = 'today_return_latitude'\n);\nSET @sql = IF(@column_exists = 0,\n    'ALTER TABLE `driver_shifts` ADD COLUMN `today_return_latitude` decimal(65,30) NULL',\n    'SELECT 1'\n);\nPREPARE stmt FROM @sql;\nEXECUTE stmt;\nDEALLOCATE PREPARE stmt;");

            migrationBuilder.Sql("\nSET @column_exists = (\n    SELECT COUNT(*)\n    FROM INFORMATION_SCHEMA.COLUMNS\n    WHERE TABLE_SCHEMA = DATABASE()\n      AND TABLE_NAME = 'driver_shifts'\n      AND COLUMN_NAME = 'today_return_longitude'\n);\nSET @sql = IF(@column_exists = 0,\n    'ALTER TABLE `driver_shifts` ADD COLUMN `today_return_longitude` decimal(65,30) NULL',\n    'SELECT 1'\n);\nPREPARE stmt FROM @sql;\nEXECUTE stmt;\nDEALLOCATE PREPARE stmt;");

            migrationBuilder.Sql("\nSET @column_exists = (\n    SELECT COUNT(*)\n    FROM INFORMATION_SCHEMA.COLUMNS\n    WHERE TABLE_SCHEMA = DATABASE()\n      AND TABLE_NAME = 'driver_shifts'\n      AND COLUMN_NAME = 'return_destination_source'\n);\nSET @sql = IF(@column_exists = 0,\n    'ALTER TABLE `driver_shifts` ADD COLUMN `return_destination_source` longtext NULL',\n    'SELECT 1'\n);\nPREPARE stmt FROM @sql;\nEXECUTE stmt;\nDEALLOCATE PREPARE stmt;");

            migrationBuilder.Sql("\nSET @column_exists = (\n    SELECT COUNT(*)\n    FROM INFORMATION_SCHEMA.COLUMNS\n    WHERE TABLE_SCHEMA = DATABASE()\n      AND TABLE_NAME = 'driver_shifts'\n      AND COLUMN_NAME = 'return_destination_recorded_at'\n);\nSET @sql = IF(@column_exists = 0,\n    'ALTER TABLE `driver_shifts` ADD COLUMN `return_destination_recorded_at` datetime(6) NULL',\n    'SELECT 1'\n);\nPREPARE stmt FROM @sql;\nEXECUTE stmt;\nDEALLOCATE PREPARE stmt;");

            migrationBuilder.Sql("\nSET @column_exists = (\n    SELECT COUNT(*)\n    FROM INFORMATION_SCHEMA.COLUMNS\n    WHERE TABLE_SCHEMA = DATABASE()\n      AND TABLE_NAME = '기사배차'\n      AND COLUMN_NAME = 'notion_page_id'\n);\nSET @sql = IF(@column_exists = 1,\n    'ALTER TABLE `기사배차` DROP COLUMN `notion_page_id`',\n    'SELECT 1'\n);\nPREPARE stmt FROM @sql;\nEXECUTE stmt;\nDEALLOCATE PREPARE stmt;");

            migrationBuilder.Sql("\nSET @column_exists = (\n    SELECT COUNT(*)\n    FROM INFORMATION_SCHEMA.COLUMNS\n    WHERE TABLE_SCHEMA = DATABASE()\n      AND TABLE_NAME = '배달기사'\n      AND COLUMN_NAME = 'notion_page_id'\n);\nSET @sql = IF(@column_exists = 1,\n    'ALTER TABLE `배달기사` DROP COLUMN `notion_page_id`',\n    'SELECT 1'\n);\nPREPARE stmt FROM @sql;\nEXECUTE stmt;\nDEALLOCATE PREPARE stmt;");

            migrationBuilder.Sql("\nSET @column_exists = (\n    SELECT COUNT(*)\n    FROM INFORMATION_SCHEMA.COLUMNS\n    WHERE TABLE_SCHEMA = DATABASE()\n      AND TABLE_NAME = '배송_운송'\n      AND COLUMN_NAME = 'notion_page_id'\n);\nSET @sql = IF(@column_exists = 1,\n    'ALTER TABLE `배송_운송` DROP COLUMN `notion_page_id`',\n    'SELECT 1'\n);\nPREPARE stmt FROM @sql;\nEXECUTE stmt;\nDEALLOCATE PREPARE stmt;");

            migrationBuilder.Sql("\nSET @column_exists = (\n    SELECT COUNT(*)\n    FROM INFORMATION_SCHEMA.COLUMNS\n    WHERE TABLE_SCHEMA = DATABASE()\n      AND TABLE_NAME = '배차_최소'\n      AND COLUMN_NAME = 'notion_page_id'\n);\nSET @sql = IF(@column_exists = 1,\n    'ALTER TABLE `배차_최소` DROP COLUMN `notion_page_id`',\n    'SELECT 1'\n);\nPREPARE stmt FROM @sql;\nEXECUTE stmt;\nDEALLOCATE PREPARE stmt;");

            migrationBuilder.Sql("\nSET @column_exists = (\n    SELECT COUNT(*)\n    FROM INFORMATION_SCHEMA.COLUMNS\n    WHERE TABLE_SCHEMA = DATABASE()\n      AND TABLE_NAME = '업체'\n      AND COLUMN_NAME = 'notion_page_id'\n);\nSET @sql = IF(@column_exists = 1,\n    'ALTER TABLE `업체` DROP COLUMN `notion_page_id`',\n    'SELECT 1'\n);\nPREPARE stmt FROM @sql;\nEXECUTE stmt;\nDEALLOCATE PREPARE stmt;");

            migrationBuilder.Sql("\nSET @column_exists = (\n    SELECT COUNT(*)\n    FROM INFORMATION_SCHEMA.COLUMNS\n    WHERE TABLE_SCHEMA = DATABASE()\n      AND TABLE_NAME = '용달기사'\n      AND COLUMN_NAME = 'notion_page_id'\n);\nSET @sql = IF(@column_exists = 1,\n    'ALTER TABLE `용달기사` DROP COLUMN `notion_page_id`',\n    'SELECT 1'\n);\nPREPARE stmt FROM @sql;\nEXECUTE stmt;\nDEALLOCATE PREPARE stmt;");

            migrationBuilder.CreateTable(
                name: "마트주문",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    주문참조번호 = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    주문_id = table.Column<long>(type: "bigint", nullable: true),
                    주문자_user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    판매자_user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    상태 = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    현재단계 = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    community_ledger_id = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    community_ledger_template_key = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    community_ledger_state = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    community_ledger_synced_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_마트주문", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "음식주문",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    주문번호 = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    음식점_id = table.Column<long>(type: "bigint", nullable: false),
                    음식점명 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    음식점주소 = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    음식점상세주소 = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    음식점위도 = table.Column<decimal>(type: "decimal(18,10)", nullable: true),
                    음식점경도 = table.Column<decimal>(type: "decimal(18,10)", nullable: true),
                    주문자_user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    수령인명 = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    수령인연락처 = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    수령지주소 = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    수령지상세주소 = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    수령요청사항 = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    주문자본인수령여부 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    총주문금액 = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    상태 = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    배차상태 = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    배차대기_id = table.Column<long>(type: "bigint", nullable: true),
                    결제수단 = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    음식점수락시각_utc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    조리예상완료시각_utc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    배차요청시각_utc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    수락메모 = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    community_ledger_id = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    community_ledger_template_key = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    community_ledger_state = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    community_ledger_synced_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_음식주문", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "마트주문상품",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    마트주문_id = table.Column<long>(type: "bigint", nullable: false),
                    출고예정_id = table.Column<long>(type: "bigint", nullable: true),
                    상품명 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    sku = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    수량 = table.Column<int>(type: "int", nullable: false),
                    상태 = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_마트주문상품", x => x.id);
                    table.ForeignKey(
                        name: "FK_마트주문상품_마트주문_마트주문_id",
                        column: x => x.마트주문_id,
                        principalTable: "마트주문",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "음식주문상태이력",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    음식주문_id = table.Column<long>(type: "bigint", nullable: false),
                    이전상태 = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    다음상태 = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    사유 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    전이시각_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_음식주문상태이력", x => x.id);
                    table.ForeignKey(
                        name: "FK_음식주문상태이력_음식주문_음식주문_id",
                        column: x => x.음식주문_id,
                        principalTable: "음식주문",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "음식주문상품",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    음식주문_id = table.Column<long>(type: "bigint", nullable: false),
                    상품명 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    수량 = table.Column<int>(type: "int", nullable: false),
                    단가 = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_음식주문상품", x => x.id);
                    table.ForeignKey(
                        name: "FK_음식주문상품_음식주문_음식주문_id",
                        column: x => x.음식주문_id,
                        principalTable: "음식주문",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_마트주문_주문자_user_id_상태_created_at",
                table: "마트주문",
                columns: new[] { "주문자_user_id", "상태", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_마트주문_주문참조번호",
                table: "마트주문",
                column: "주문참조번호",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_마트주문_판매자_user_id_상태_created_at",
                table: "마트주문",
                columns: new[] { "판매자_user_id", "상태", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_마트주문_community_ledger_id",
                table: "마트주문",
                column: "community_ledger_id");

            migrationBuilder.CreateIndex(
                name: "IX_마트주문상품_마트주문_id_출고예정_id",
                table: "마트주문상품",
                columns: new[] { "마트주문_id", "출고예정_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_마트주문상품_출고예정_id",
                table: "마트주문상품",
                column: "출고예정_id");

            migrationBuilder.CreateIndex(
                name: "IX_마트주문상품_sku",
                table: "마트주문상품",
                column: "sku");

            migrationBuilder.CreateIndex(
                name: "IX_음식주문_배차대기_id",
                table: "음식주문",
                column: "배차대기_id");

            migrationBuilder.CreateIndex(
                name: "IX_음식주문_음식점_id_상태_created_at",
                table: "음식주문",
                columns: new[] { "음식점_id", "상태", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_음식주문_주문번호",
                table: "음식주문",
                column: "주문번호",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_음식주문_주문자_user_id_상태_created_at",
                table: "음식주문",
                columns: new[] { "주문자_user_id", "상태", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_음식주문_community_ledger_id",
                table: "음식주문",
                column: "community_ledger_id");

            migrationBuilder.CreateIndex(
                name: "IX_음식주문상태이력_음식주문_id_전이시각_utc",
                table: "음식주문상태이력",
                columns: new[] { "음식주문_id", "전이시각_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_음식주문상품_상품명",
                table: "음식주문상품",
                column: "상품명");

            migrationBuilder.CreateIndex(
                name: "IX_음식주문상품_음식주문_id",
                table: "음식주문상품",
                column: "음식주문_id");

            migrationBuilder.CreateTable(
                name: "피킹포장작업",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    작업_key = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    작업유형 = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    처리방식 = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    상태 = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    출고묶음_id = table.Column<long>(type: "bigint", nullable: true),
                    출고예정_id = table.Column<long>(type: "bigint", nullable: true),
                    입고상품_id = table.Column<long>(type: "bigint", nullable: true),
                    창고_id = table.Column<long>(type: "bigint", nullable: false),
                    창고명 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    작업자_user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    작업자표시명 = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    상대작업자_user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    이전작업_key = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    다음작업_key = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    주문참조번호 = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    라인_key = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    상품명 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    sku = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    수량 = table.Column<int>(type: "int", nullable: false),
                    적재대코드 = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    보관위치코드 = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    묶음바코드 = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    할당사유 = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    community_ledger_id = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    community_ledger_block_id = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    started_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    completed_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_피킹포장작업", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "community_ledger_state_events",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    EventId = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    커뮤니티원장Id = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    커뮤니티Id = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    원장템플릿Key = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EventType = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    이전상태 = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    상태 = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    현재단계Key = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    변경사유 = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedBy = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CorrelationId = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SnapshotJson = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OccurredAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_community_ledger_state_events", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_피킹포장작업_입고상품_id",
                table: "피킹포장작업",
                column: "입고상품_id");

            migrationBuilder.CreateIndex(
                name: "IX_피킹포장작업_작업_key",
                table: "피킹포장작업",
                column: "작업_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_피킹포장작업_작업자_user_id_상태_created_at",
                table: "피킹포장작업",
                columns: new[] { "작업자_user_id", "상태", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_피킹포장작업_창고_id_상태_작업유형_created_at",
                table: "피킹포장작업",
                columns: new[] { "창고_id", "상태", "작업유형", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_피킹포장작업_출고묶음_id_작업유형",
                table: "피킹포장작업",
                columns: new[] { "출고묶음_id", "작업유형" });

            migrationBuilder.CreateIndex(
                name: "IX_피킹포장작업_출고예정_id",
                table: "피킹포장작업",
                column: "출고예정_id");

            migrationBuilder.CreateIndex(
                name: "IX_피킹포장작업_community_ledger_id",
                table: "피킹포장작업",
                column: "community_ledger_id");

            migrationBuilder.CreateIndex(
                name: "IX_community_ledger_state_events_커뮤니티원장Id_OccurredAtUtc",
                table: "community_ledger_state_events",
                columns: new[] { "커뮤니티원장Id", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_community_ledger_state_events_커뮤니티Id_원장템플릿Key_상태",
                table: "community_ledger_state_events",
                columns: new[] { "커뮤니티Id", "원장템플릿Key", "상태" });

            migrationBuilder.CreateIndex(
                name: "IX_community_ledger_state_events_CorrelationId",
                table: "community_ledger_state_events",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_community_ledger_state_events_EventId",
                table: "community_ledger_state_events",
                column: "EventId",
                unique: true);

            migrationBuilder.Sql("\nSET @old_table_exists = (\n    SELECT COUNT(*)\n    FROM INFORMATION_SCHEMA.TABLES\n    WHERE TABLE_SCHEMA = DATABASE()\n      AND TABLE_NAME = '배송_운송'\n);\nSET @new_table_exists = (\n    SELECT COUNT(*)\n    FROM INFORMATION_SCHEMA.TABLES\n    WHERE TABLE_SCHEMA = DATABASE()\n      AND TABLE_NAME = '운송원장'\n);\nSET @sql = IF(@old_table_exists = 1 AND @new_table_exists = 0,\n    'RENAME TABLE `배송_운송` TO `운송원장`',\n    'SELECT 1'\n);\nPREPARE stmt FROM @sql;\nEXECUTE stmt;\nDEALLOCATE PREPARE stmt;");

            migrationBuilder.Sql("\nSET @table_exists = (\n    SELECT COUNT(*)\n    FROM INFORMATION_SCHEMA.TABLES\n    WHERE TABLE_SCHEMA = DATABASE()\n      AND TABLE_NAME = '운송문서'\n);\nSET @old_column_exists = (\n    SELECT COUNT(*)\n    FROM INFORMATION_SCHEMA.COLUMNS\n    WHERE TABLE_SCHEMA = DATABASE()\n      AND TABLE_NAME = '운송문서'\n      AND COLUMN_NAME = '배송_운송_id'\n);\nSET @new_column_exists = (\n    SELECT COUNT(*)\n    FROM INFORMATION_SCHEMA.COLUMNS\n    WHERE TABLE_SCHEMA = DATABASE()\n      AND TABLE_NAME = '운송문서'\n      AND COLUMN_NAME = '운송원장_id'\n);\nSET @sql = IF(@table_exists = 1 AND @old_column_exists = 1 AND @new_column_exists = 0,\n    'ALTER TABLE `운송문서` CHANGE COLUMN `배송_운송_id` `운송원장_id` bigint NULL',\n    'SELECT 1'\n);\nPREPARE stmt FROM @sql;\nEXECUTE stmt;\nDEALLOCATE PREPARE stmt;");

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

            migrationBuilder.AddForeignKey(
                name: "FK_피킹포장작업_출고묶음_출고묶음_id",
                table: "피킹포장작업",
                column: "출고묶음_id",
                principalTable: "출고묶음",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

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

            migrationBuilder.Sql("UPDATE `운송원장` t\nJOIN `배차_대기` q ON t.`운송번호` = q.`request_id`\nSET\n    t.`request_id` = q.`request_id`,\n    t.`shipper_id` = q.`shipper_id`,\n    t.`business_type` = q.`business_type`,\n    t.`source_type` = q.`source_type`,\n    t.`source_request_id` = q.`source_request_id`,\n    t.`group_purchase_destination_type_code` = q.`group_purchase_destination_type_code`,\n    t.`group_purchase_driver_unit_distribution` = q.`group_purchase_driver_unit_distribution`,\n    t.`group_purchase_unit_distribution_mode_code` = q.`group_purchase_unit_distribution_mode_code`,\n    t.`group_purchase_unit_delivery_count` = q.`group_purchase_unit_delivery_count`,\n    t.`group_purchase_distribution_responsibility_code` = q.`group_purchase_distribution_responsibility_code`,\n    t.`상태` = q.`status`,\n    t.`queue_stage` = q.`queue_stage`,\n    t.`exposure_state` = q.`exposure_state`,\n    t.`current_recommended_driver_id` = q.`current_recommended_driver_id`,\n    t.`recommendation_started_at` = q.`recommendation_started_at`,\n    t.`recommendation_expires_at` = q.`recommendation_expires_at`,\n    t.`recommendation_round` = q.`recommendation_round`,\n    t.`plan_attempts` = q.`plan_attempts`,\n    t.`last_rejected_driver_id` = q.`last_rejected_driver_id`,\n    t.`public_transition_at` = q.`public_transition_at`,\n    t.`confirmed_driver_id` = q.`confirmed_driver_id`,\n    t.`pickup_address` = q.`pickup_address`,\n    t.`pickup_address_detail` = q.`pickup_address_detail`,\n    t.`pickup_latitude` = q.`pickup_latitude`,\n    t.`pickup_longitude` = q.`pickup_longitude`,\n    t.`dropoff_address` = q.`dropoff_address`,\n    t.`dropoff_address_detail` = q.`dropoff_address_detail`,\n    t.`dropoff_latitude` = q.`dropoff_latitude`,\n    t.`dropoff_longitude` = q.`dropoff_longitude`,\n    t.`기사_운송자` = COALESCE(NULLIF(t.`기사_운송자`, ''), q.`confirmed_driver_id`, ''),\n    t.`출발지` = COALESCE(NULLIF(t.`출발지`, ''), q.`pickup_address`, ''),\n    t.`도착지` = COALESCE(NULLIF(t.`도착지`, ''), q.`dropoff_address`, ''),\n    t.`updated_at` = q.`updated_at`;");

            migrationBuilder.Sql("INSERT INTO `운송원장` (\n    `운송번호`,\n    `request_id`,\n    `shipper_id`,\n    `business_type`,\n    `source_type`,\n    `source_request_id`,\n    `group_purchase_destination_type_code`,\n    `group_purchase_driver_unit_distribution`,\n    `group_purchase_unit_distribution_mode_code`,\n    `group_purchase_unit_delivery_count`,\n    `group_purchase_distribution_responsibility_code`,\n    `상태`,\n    `queue_stage`,\n    `exposure_state`,\n    `current_recommended_driver_id`,\n    `recommendation_started_at`,\n    `recommendation_expires_at`,\n    `recommendation_round`,\n    `plan_attempts`,\n    `last_rejected_driver_id`,\n    `public_transition_at`,\n    `confirmed_driver_id`,\n    `pickup_address`,\n    `pickup_address_detail`,\n    `pickup_latitude`,\n    `pickup_longitude`,\n    `dropoff_address`,\n    `dropoff_address_detail`,\n    `dropoff_latitude`,\n    `dropoff_longitude`,\n    `기사_운송자`,\n    `출발지`,\n    `도착지`,\n    `첨부_json`,\n    `메모`,\n    `created_at`,\n    `updated_at`\n)\nSELECT\n    q.`request_id`,\n    q.`request_id`,\n    q.`shipper_id`,\n    q.`business_type`,\n    q.`source_type`,\n    q.`source_request_id`,\n    q.`group_purchase_destination_type_code`,\n    q.`group_purchase_driver_unit_distribution`,\n    q.`group_purchase_unit_distribution_mode_code`,\n    q.`group_purchase_unit_delivery_count`,\n    q.`group_purchase_distribution_responsibility_code`,\n    q.`status`,\n    q.`queue_stage`,\n    q.`exposure_state`,\n    q.`current_recommended_driver_id`,\n    q.`recommendation_started_at`,\n    q.`recommendation_expires_at`,\n    q.`recommendation_round`,\n    q.`plan_attempts`,\n    q.`last_rejected_driver_id`,\n    q.`public_transition_at`,\n    q.`confirmed_driver_id`,\n    q.`pickup_address`,\n    q.`pickup_address_detail`,\n    q.`pickup_latitude`,\n    q.`pickup_longitude`,\n    q.`dropoff_address`,\n    q.`dropoff_address_detail`,\n    q.`dropoff_latitude`,\n    q.`dropoff_longitude`,\n    COALESCE(q.`confirmed_driver_id`, ''),\n    COALESCE(q.`pickup_address`, ''),\n    COALESCE(q.`dropoff_address`, ''),\n    '[]',\n    '배차대기에서 운송원장으로 통합',\n    q.`created_at`,\n    q.`updated_at`\nFROM `배차_대기` q\nLEFT JOIN `운송원장` t ON t.`운송번호` = q.`request_id`\nWHERE t.`id` IS NULL;");

            migrationBuilder.DropTable(
                name: "배차_대기");

            migrationBuilder.CreateTable(
                name: "community_ledger_block_projections",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    커뮤니티원장Id = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    커뮤니티Id = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    원장템플릿Key = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BlockId = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BlockType = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Title = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    State = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UiSectionHint = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DiagramNodeId = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RelatedRoute = table.Column<string>(type: "varchar(400)", maxLength: 400, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    속성Json = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_community_ledger_block_projections", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "community_ledger_block_relation_projections",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    커뮤니티원장Id = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    관계유형 = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Cardinality = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    필수여부 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    FromBlockId = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ToBlockId = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FromBlockProjectionId = table.Column<long>(type: "bigint", nullable: false),
                    ToBlockProjectionId = table.Column<long>(type: "bigint", nullable: false),
                    DiagramEdgeId = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Label = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MeaningCode = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    조건식Json = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_community_ledger_block_relation_projections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_community_ledger_block_relation_projections_community_ledger~",
                        column: x => x.FromBlockProjectionId,
                        principalTable: "community_ledger_block_projections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_community_ledger_block_relation_projections_community_ledge~1",
                        column: x => x.ToBlockProjectionId,
                        principalTable: "community_ledger_block_projections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_community_ledger_block_projections_커뮤니티원장Id_BlockId",
                table: "community_ledger_block_projections",
                columns: new[] { "커뮤니티원장Id", "BlockId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_community_ledger_block_projections_커뮤니티원장Id_SortOrder",
                table: "community_ledger_block_projections",
                columns: new[] { "커뮤니티원장Id", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_community_ledger_block_projections_커뮤니티Id_원장템플릿Key_BlockType",
                table: "community_ledger_block_projections",
                columns: new[] { "커뮤니티Id", "원장템플릿Key", "BlockType" });

            migrationBuilder.CreateIndex(
                name: "IX_community_ledger_block_projections_DiagramNodeId",
                table: "community_ledger_block_projections",
                column: "DiagramNodeId");

            migrationBuilder.CreateIndex(
                name: "IX_community_ledger_block_relation_projections_커뮤니티원장Id_Cardina~",
                table: "community_ledger_block_relation_projections",
                columns: new[] { "커뮤니티원장Id", "Cardinality" });

            migrationBuilder.CreateIndex(
                name: "IX_community_ledger_block_relation_projections_커뮤니티원장Id_FromBlo~",
                table: "community_ledger_block_relation_projections",
                columns: new[] { "커뮤니티원장Id", "FromBlockId", "ToBlockId", "관계유형" });

            migrationBuilder.CreateIndex(
                name: "IX_community_ledger_block_relation_projections_DiagramEdgeId",
                table: "community_ledger_block_relation_projections",
                column: "DiagramEdgeId");

            migrationBuilder.CreateIndex(
                name: "IX_community_ledger_block_relation_projections_FromBlockProject~",
                table: "community_ledger_block_relation_projections",
                column: "FromBlockProjectionId");

            migrationBuilder.CreateIndex(
                name: "IX_community_ledger_block_relation_projections_ToBlockProjectio~",
                table: "community_ledger_block_relation_projections",
                column: "ToBlockProjectionId");

            migrationBuilder.Sql("\nSET @old_table_exists = (\n    SELECT COUNT(*)\n    FROM INFORMATION_SCHEMA.TABLES\n    WHERE TABLE_SCHEMA = DATABASE()\n      AND TABLE_NAME = '운송원장'\n);\nSET @new_table_exists = (\n    SELECT COUNT(*)\n    FROM INFORMATION_SCHEMA.TABLES\n    WHERE TABLE_SCHEMA = DATABASE()\n      AND TABLE_NAME = '운송실행투영'\n);\nSET @sql = IF(@old_table_exists = 1 AND @new_table_exists = 0,\n    'RENAME TABLE `운송원장` TO `운송실행투영`',\n    'SELECT 1'\n);\nPREPARE stmt FROM @sql;\nEXECUTE stmt;\nDEALLOCATE PREPARE stmt;");

            migrationBuilder.Sql("\nSET @table_exists = (\n    SELECT COUNT(*)\n    FROM INFORMATION_SCHEMA.TABLES\n    WHERE TABLE_SCHEMA = DATABASE()\n      AND TABLE_NAME = '운송문서'\n);\nSET @old_column_exists = (\n    SELECT COUNT(*)\n    FROM INFORMATION_SCHEMA.COLUMNS\n    WHERE TABLE_SCHEMA = DATABASE()\n      AND TABLE_NAME = '운송문서'\n      AND COLUMN_NAME = '운송원장_id'\n);\nSET @new_column_exists = (\n    SELECT COUNT(*)\n    FROM INFORMATION_SCHEMA.COLUMNS\n    WHERE TABLE_SCHEMA = DATABASE()\n      AND TABLE_NAME = '운송문서'\n      AND COLUMN_NAME = '운송실행투영_id'\n);\nSET @sql = IF(@table_exists = 1 AND @old_column_exists = 1 AND @new_column_exists = 0,\n    'ALTER TABLE `운송문서` CHANGE COLUMN `운송원장_id` `운송실행투영_id` bigint NULL',\n    'SELECT 1'\n);\nPREPARE stmt FROM @sql;\nEXECUTE stmt;\nDEALLOCATE PREPARE stmt;");

            migrationBuilder.AddColumn<string>(
                name: "community_ledger_id",
                table: "운송실행투영",
                type: "varchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "community_ledger_template_key",
                table: "운송실행투영",
                type: "varchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "community_ledger_state",
                table: "운송실행투영",
                type: "varchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "community_ledger_synced_at_utc",
                table: "운송실행투영",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_운송실행투영_community_ledger_id",
                table: "운송실행투영",
                column: "community_ledger_id");

            migrationBuilder.AddColumn<string>(
                name: "community_ledger_id",
                table: "입고요청",
                type: "varchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "community_ledger_template_key",
                table: "입고요청",
                type: "varchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "community_ledger_state",
                table: "입고요청",
                type: "varchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "community_ledger_synced_at_utc",
                table: "입고요청",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "community_ledger_id",
                table: "입고상품",
                type: "varchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "community_ledger_template_key",
                table: "입고상품",
                type: "varchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "community_ledger_state",
                table: "입고상품",
                type: "varchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "community_ledger_synced_at_utc",
                table: "입고상품",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "community_ledger_id",
                table: "출고예정",
                type: "varchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "community_ledger_template_key",
                table: "출고예정",
                type: "varchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "community_ledger_state",
                table: "출고예정",
                type: "varchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "community_ledger_synced_at_utc",
                table: "출고예정",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "community_ledger_id",
                table: "출고묶음",
                type: "varchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "community_ledger_template_key",
                table: "출고묶음",
                type: "varchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "community_ledger_state",
                table: "출고묶음",
                type: "varchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "community_ledger_synced_at_utc",
                table: "출고묶음",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_입고요청_community_ledger_id",
                table: "입고요청",
                column: "community_ledger_id");

            migrationBuilder.CreateIndex(
                name: "IX_입고상품_community_ledger_id",
                table: "입고상품",
                column: "community_ledger_id");

            migrationBuilder.CreateIndex(
                name: "IX_출고예정_community_ledger_id",
                table: "출고예정",
                column: "community_ledger_id");

            migrationBuilder.CreateIndex(
                name: "IX_출고묶음_community_ledger_id",
                table: "출고묶음",
                column: "community_ledger_id");

            migrationBuilder.CreateTable(
                name: "typecast_voices",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    voice_id = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    gender = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    age_group = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    voice_type = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    last_synced_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_typecast_voices", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "typecast_voice_models",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    typecast_voice_id = table.Column<long>(type: "bigint", nullable: false),
                    model_version = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    emotions_json = table.Column<string>(type: "text", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_typecast_voice_models", x => x.Id);
                    table.ForeignKey(
                        name: "FK_typecast_voice_models_typecast_voices_typecast_voice_id",
                        column: x => x.typecast_voice_id,
                        principalTable: "typecast_voices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "typecast_voice_use_cases",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    typecast_voice_id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_typecast_voice_use_cases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_typecast_voice_use_cases_typecast_voices_typecast_voice_id",
                        column: x => x.typecast_voice_id,
                        principalTable: "typecast_voices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_typecast_voice_models_model_version",
                table: "typecast_voice_models",
                column: "model_version");

            migrationBuilder.CreateIndex(
                name: "IX_typecast_voice_models_typecast_voice_id_model_version",
                table: "typecast_voice_models",
                columns: new[] { "typecast_voice_id", "model_version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_typecast_voice_use_cases_name",
                table: "typecast_voice_use_cases",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "IX_typecast_voice_use_cases_typecast_voice_id_name",
                table: "typecast_voice_use_cases",
                columns: new[] { "typecast_voice_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_typecast_voices_is_active_voice_type_gender_age_group",
                table: "typecast_voices",
                columns: new[] { "is_active", "voice_type", "gender", "age_group" });

            migrationBuilder.CreateIndex(
                name: "IX_typecast_voices_voice_id",
                table: "typecast_voices",
                column: "voice_id",
                unique: true);

            migrationBuilder.CreateTable(
                name: "platform_community_post_audio",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    post_id = table.Column<long>(type: "bigint", nullable: false),
                    status = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    provider = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    voice_id = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    model_version = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    language_code = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    audio_format = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    attempt_count = table.Column<int>(type: "int", nullable: false),
                    processing_token = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    last_error = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    next_attempt_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    completed_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platform_community_post_audio", x => x.Id);
                    table.ForeignKey(
                        name: "FK_platform_community_post_audio_platform_community_posts_post_~",
                        column: x => x.post_id,
                        principalTable: "platform_community_posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "platform_community_post_audio_access_logs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    audio_id = table.Column<long>(type: "bigint", nullable: false),
                    post_id = table.Column<long>(type: "bigint", nullable: false),
                    segment_sequence = table.Column<int>(type: "int", nullable: true),
                    access_type = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    requester_user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    trace_id = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    accessed_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platform_community_post_audio_access_logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_platform_community_post_audio_access_logs_platform_community~",
                        column: x => x.audio_id,
                        principalTable: "platform_community_post_audio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "platform_community_post_audio_segments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    audio_id = table.Column<long>(type: "bigint", nullable: false),
                    sequence = table.Column<int>(type: "int", nullable: false),
                    character_count = table.Column<int>(type: "int", nullable: false),
                    bucket_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    object_name = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    content_type = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    file_size_bytes = table.Column<long>(type: "bigint", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platform_community_post_audio_segments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_platform_community_post_audio_segments_platform_community_po~",
                        column: x => x.audio_id,
                        principalTable: "platform_community_post_audio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_platform_community_post_audio_post_id",
                table: "platform_community_post_audio",
                column: "post_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_platform_community_post_audio_processing_token",
                table: "platform_community_post_audio",
                column: "processing_token");

            migrationBuilder.CreateIndex(
                name: "IX_platform_community_post_audio_status_next_attempt_at_utc_upd~",
                table: "platform_community_post_audio",
                columns: new[] { "status", "next_attempt_at_utc", "updated_at_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_platform_community_post_audio_access_logs_audio_id",
                table: "platform_community_post_audio_access_logs",
                column: "audio_id");

            migrationBuilder.CreateIndex(
                name: "IX_platform_community_post_audio_access_logs_post_id_accessed_a~",
                table: "platform_community_post_audio_access_logs",
                columns: new[] { "post_id", "accessed_at_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_platform_community_post_audio_access_logs_requester_user_id_~",
                table: "platform_community_post_audio_access_logs",
                columns: new[] { "requester_user_id", "accessed_at_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_platform_community_post_audio_segments_audio_id_sequence",
                table: "platform_community_post_audio_segments",
                columns: new[] { "audio_id", "sequence" },
                unique: true);

            migrationBuilder.CreateTable(
                name: "youtube_watched_channels",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    channel_id = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    channel_name = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    uploads_playlist_id = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    thumbnail_url = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    initial_sync_completed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    last_synced_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    latest_video_id = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    latest_video_published_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_youtube_watched_channels", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "youtube_channel_videos",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    watched_channel_id = table.Column<long>(type: "bigint", nullable: false),
                    video_id = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    channel_id = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    title = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "text", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    published_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    thumbnail_url = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_new_upload = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    sharing_status = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    first_detected_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_youtube_channel_videos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_youtube_channel_videos_youtube_watched_channels_watched_chan~",
                        column: x => x.watched_channel_id,
                        principalTable: "youtube_watched_channels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_youtube_channel_videos_channel_id_published_at_utc",
                table: "youtube_channel_videos",
                columns: new[] { "channel_id", "published_at_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_youtube_channel_videos_is_new_upload_sharing_status_first_de~",
                table: "youtube_channel_videos",
                columns: new[] { "is_new_upload", "sharing_status", "first_detected_at_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_youtube_channel_videos_video_id",
                table: "youtube_channel_videos",
                column: "video_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_youtube_channel_videos_watched_channel_id",
                table: "youtube_channel_videos",
                column: "watched_channel_id");

            migrationBuilder.CreateIndex(
                name: "IX_youtube_watched_channels_channel_id",
                table: "youtube_watched_channels",
                column: "channel_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_youtube_watched_channels_is_active_last_synced_at_utc",
                table: "youtube_watched_channels",
                columns: new[] { "is_active", "last_synced_at_utc" });

            migrationBuilder.CreateTable(
                name: "education_courses",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    course_code = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    course_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "text", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    delivery_mode = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    minimum_months = table.Column<int>(type: "int", nullable: false),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    source_url = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_education_courses", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "education_course_applications",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    course_id = table.Column<long>(type: "bigint", nullable: false),
                    applicant_user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    name_ciphertext = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nickname_ciphertext = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    email_ciphertext = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    phone_ciphertext = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    gender_ciphertext = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    birth_year_ciphertext = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    country_ciphertext = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    membership_confirmed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    entry_pledge_agreed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    personal_data_agreed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    third_party_data_agreed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    personal_data_consent_version = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    third_party_consent_version = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    consented_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    status = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    reviewer_user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    review_note_ciphertext = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    applied_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    reviewed_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    personal_data_deleted_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_education_course_applications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_education_course_applications_education_courses_course_id",
                        column: x => x.course_id,
                        principalTable: "education_courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "education_course_forms",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    course_id = table.Column<long>(type: "bigint", nullable: false),
                    form_code = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    form_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    purpose = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    version = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    submission_cycle = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    minimum_submission_count = table.Column<int>(type: "int", nullable: false),
                    is_required = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    field_definition_json = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    source_url = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_education_course_forms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_education_course_forms_education_courses_course_id",
                        column: x => x.course_id,
                        principalTable: "education_courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "education_course_subjects",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    course_id = table.Column<long>(type: "bigint", nullable: false),
                    subject_code = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    subject_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    display_order = table.Column<int>(type: "int", nullable: false),
                    minimum_attendance_count = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_education_course_subjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_education_course_subjects_education_courses_course_id",
                        column: x => x.course_id,
                        principalTable: "education_courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "education_course_enrollments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    course_id = table.Column<long>(type: "bigint", nullable: false),
                    application_id = table.Column<long>(type: "bigint", nullable: false),
                    participant_user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    mentor_user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    started_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ended_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_education_course_enrollments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_education_course_enrollments_education_course_applications_a~",
                        column: x => x.application_id,
                        principalTable: "education_course_applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_education_course_enrollments_education_courses_course_id",
                        column: x => x.course_id,
                        principalTable: "education_courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "education_course_attendances",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    enrollment_id = table.Column<long>(type: "bigint", nullable: false),
                    subject_id = table.Column<long>(type: "bigint", nullable: false),
                    session_key = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    session_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    session_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    attended = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    recorded_by_user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    recorded_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_education_course_attendances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_education_course_attendances_education_course_enrollments_en~",
                        column: x => x.enrollment_id,
                        principalTable: "education_course_enrollments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_education_course_attendances_education_course_subjects_subje~",
                        column: x => x.subject_id,
                        principalTable: "education_course_subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "education_course_submissions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    enrollment_id = table.Column<long>(type: "bigint", nullable: false),
                    form_id = table.Column<long>(type: "bigint", nullable: false),
                    period_key = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    answers_ciphertext = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    reviewer_user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    review_note_ciphertext = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    submitted_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    reviewed_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_education_course_submissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_education_course_submissions_education_course_enrollments_en~",
                        column: x => x.enrollment_id,
                        principalTable: "education_course_enrollments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_education_course_submissions_education_course_forms_form_id",
                        column: x => x.form_id,
                        principalTable: "education_course_forms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_education_course_applications_course_id_applicant_user_id_st~",
                table: "education_course_applications",
                columns: new[] { "course_id", "applicant_user_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_education_course_applications_status_applied_at_utc",
                table: "education_course_applications",
                columns: new[] { "status", "applied_at_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_education_course_attendances_enrollment_id_subject_id_sessio~",
                table: "education_course_attendances",
                columns: new[] { "enrollment_id", "subject_id", "session_key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_education_course_attendances_subject_id",
                table: "education_course_attendances",
                column: "subject_id");

            migrationBuilder.CreateIndex(
                name: "IX_education_course_enrollments_application_id",
                table: "education_course_enrollments",
                column: "application_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_education_course_enrollments_course_id",
                table: "education_course_enrollments",
                column: "course_id");

            migrationBuilder.CreateIndex(
                name: "IX_education_course_enrollments_participant_user_id_status",
                table: "education_course_enrollments",
                columns: new[] { "participant_user_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_education_course_forms_course_id_form_code",
                table: "education_course_forms",
                columns: new[] { "course_id", "form_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_education_course_subjects_course_id_subject_code",
                table: "education_course_subjects",
                columns: new[] { "course_id", "subject_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_education_course_submissions_enrollment_id_form_id_period_key",
                table: "education_course_submissions",
                columns: new[] { "enrollment_id", "form_id", "period_key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_education_course_submissions_form_id",
                table: "education_course_submissions",
                column: "form_id");

            migrationBuilder.CreateIndex(
                name: "IX_education_course_submissions_status_submitted_at_utc",
                table: "education_course_submissions",
                columns: new[] { "status", "submitted_at_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_education_courses_course_code",
                table: "education_courses",
                column: "course_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_education_courses_is_active_course_name",
                table: "education_courses",
                columns: new[] { "is_active", "course_name" });

            migrationBuilder.DropTable(
                name: "community_ledger_block_relation_projections");

            migrationBuilder.DropTable(
                name: "community_ledger_block_projections");

            migrationBuilder.AddColumn<string>(
                name: "커뮤니티원장Id",
                table: "platform_community_posts",
                type: "varchar(120)",
                maxLength: 120,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_platform_community_posts_커뮤니티원장Id",
                table: "platform_community_posts",
                column: "커뮤니티원장Id");

            migrationBuilder.CreateTable(
                name: "hongik_hakdang_card_collections",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    source_key = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    name = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    sort_order = table.Column<int>(type: "int", nullable: false),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    last_seen_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hongik_hakdang_card_collections", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "hongik_hakdang_cards",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    source_key = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    title = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "text", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    original_image_url = table.Column<string>(type: "varchar(1500)", maxLength: 1500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    thumbnail_image_url = table.Column<string>(type: "varchar(1500)", maxLength: 1500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    related_url = table.Column<string>(type: "varchar(1500)", maxLength: 1500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    local_image_path = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    image_content_type = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    image_size_bytes = table.Column<long>(type: "bigint", nullable: true),
                    image_sha256 = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    image_download_status = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    image_download_error = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    image_downloaded_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    last_seen_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hongik_hakdang_cards", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "hongik_hakdang_card_collection_items",
                columns: table => new
                {
                    collection_id = table.Column<long>(type: "bigint", nullable: false),
                    card_id = table.Column<long>(type: "bigint", nullable: false),
                    sort_order = table.Column<int>(type: "int", nullable: false),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    last_seen_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hongik_hakdang_card_collection_items", x => new { x.collection_id, x.card_id });
                    table.ForeignKey(
                        name: "FK_hh_card_items_collections",
                        column: x => x.collection_id,
                        principalTable: "hongik_hakdang_card_collections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_hh_card_items_cards",
                        column: x => x.card_id,
                        principalTable: "hongik_hakdang_cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_hh_card_items_card_active",
                table: "hongik_hakdang_card_collection_items",
                columns: new[] { "card_id", "is_active" });

            migrationBuilder.CreateIndex(
                name: "IX_hh_card_items_collection_active_order",
                table: "hongik_hakdang_card_collection_items",
                columns: new[] { "collection_id", "is_active", "sort_order" });

            migrationBuilder.CreateIndex(
                name: "IX_hh_card_collections_active_order",
                table: "hongik_hakdang_card_collections",
                columns: new[] { "is_active", "sort_order" });

            migrationBuilder.CreateIndex(
                name: "IX_hh_card_collections_source_key",
                table: "hongik_hakdang_card_collections",
                column: "source_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hh_cards_download_status",
                table: "hongik_hakdang_cards",
                column: "image_download_status");

            migrationBuilder.CreateIndex(
                name: "IX_hh_cards_active_last_seen",
                table: "hongik_hakdang_cards",
                columns: new[] { "is_active", "last_seen_at_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_hh_cards_source_key",
                table: "hongik_hakdang_cards",
                column: "source_key",
                unique: true);

            migrationBuilder.CreateTable(
                name: "ssalddel_mobile_push_installations",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    installation_id = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    app_key = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    platform = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    push_token = table.Column<string>(type: "varchar(4096)", maxLength: 4096, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    push_token_hash = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    app_version = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    device_model = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    last_seen_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ssalddel_mobile_push_installations", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "hongik_hakdang_card_delivery_preferences",
                columns: table => new
                {
                    user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    enabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    delivery_mode = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    push_enabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    local_delivery_minute = table.Column<int>(type: "int", nullable: false),
                    time_zone_id = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    shuffle_without_repeats = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    preferred_collection_key = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hongik_hakdang_card_delivery_preferences", x => x.user_id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "hongik_hakdang_card_image_variants",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    card_id = table.Column<long>(type: "bigint", nullable: false),
                    variant_kind = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    width = table.Column<int>(type: "int", nullable: false),
                    height = table.Column<int>(type: "int", nullable: false),
                    local_image_path = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    content_type = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    size_bytes = table.Column<long>(type: "bigint", nullable: false),
                    sha256 = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    source_image_sha256 = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hongik_hakdang_card_image_variants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hh_card_variants_cards",
                        column: x => x.card_id,
                        principalTable: "hongik_hakdang_cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "hongik_hakdang_daily_card_selections",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    selection_date = table.Column<DateOnly>(type: "date", nullable: false),
                    time_zone_id = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    card_id = table.Column<long>(type: "bigint", nullable: false),
                    selected_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hongik_hakdang_daily_card_selections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hh_daily_cards_cards",
                        column: x => x.card_id,
                        principalTable: "hongik_hakdang_cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "hongik_hakdang_card_delivery_outbox",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    idempotency_key = table.Column<string>(type: "varchar(240)", maxLength: 240, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    installation_id = table.Column<long>(type: "bigint", nullable: false),
                    card_id = table.Column<long>(type: "bigint", nullable: false),
                    selection_date = table.Column<DateOnly>(type: "date", nullable: false),
                    status = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    attempt_count = table.Column<int>(type: "int", nullable: false),
                    next_attempt_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    last_error = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    sent_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hongik_hakdang_card_delivery_outbox", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hh_card_outbox_cards",
                        column: x => x.card_id,
                        principalTable: "hongik_hakdang_cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_hh_card_outbox_installations",
                        column: x => x.installation_id,
                        principalTable: "ssalddel_mobile_push_installations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "UX_mobile_push_app_installation",
                table: "ssalddel_mobile_push_installations",
                columns: new[] { "app_key", "installation_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_mobile_push_token_hash",
                table: "ssalddel_mobile_push_installations",
                column: "push_token_hash");

            migrationBuilder.CreateIndex(
                name: "IX_mobile_push_user_active",
                table: "ssalddel_mobile_push_installations",
                columns: new[] { "user_id", "is_active" });

            migrationBuilder.CreateIndex(
                name: "IX_hh_card_preferences_delivery",
                table: "hongik_hakdang_card_delivery_preferences",
                columns: new[] { "enabled", "push_enabled" });

            migrationBuilder.CreateIndex(
                name: "UX_hh_card_variants_card_kind",
                table: "hongik_hakdang_card_image_variants",
                columns: new[] { "card_id", "variant_kind" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hh_card_variants_sha256",
                table: "hongik_hakdang_card_image_variants",
                column: "sha256");

            migrationBuilder.CreateIndex(
                name: "UX_hh_daily_cards_date_zone",
                table: "hongik_hakdang_daily_card_selections",
                columns: new[] { "selection_date", "time_zone_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hh_daily_cards_card",
                table: "hongik_hakdang_daily_card_selections",
                column: "card_id");

            migrationBuilder.CreateIndex(
                name: "UX_hh_card_outbox_idempotency",
                table: "hongik_hakdang_card_delivery_outbox",
                column: "idempotency_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hh_card_outbox_due",
                table: "hongik_hakdang_card_delivery_outbox",
                columns: new[] { "status", "next_attempt_at_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_hh_card_outbox_installation",
                table: "hongik_hakdang_card_delivery_outbox",
                column: "installation_id");

            migrationBuilder.CreateIndex(
                name: "IX_hh_card_outbox_card",
                table: "hongik_hakdang_card_delivery_outbox",
                column: "card_id");

            migrationBuilder.AddColumn<string>(
                name: "AuthorUserId",
                table: "platform_community_posts",
                type: "varchar(450)",
                maxLength: 450,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "community_keyword_subscriptions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    app_key = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    keyword = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    normalized_keyword = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_community_keyword_subscriptions", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "platform_community_post_keyword_scans",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    post_id = table.Column<long>(type: "bigint", nullable: false),
                    status = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    attempt_count = table.Column<int>(type: "int", nullable: false),
                    processing_token = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    last_error = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    next_attempt_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    completed_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platform_community_post_keyword_scans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_community_keyword_scan_post",
                        column: x => x.post_id,
                        principalTable: "platform_community_posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "community_keyword_notifications",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    post_id = table.Column<long>(type: "bigint", nullable: false),
                    post_app_key = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    post_category = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    post_title = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    post_excerpt = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    post_author_nickname = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    matched_keywords_json = table.Column<string>(type: "varchar(4096)", maxLength: 4096, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_read = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    read_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_community_keyword_notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_community_keyword_notification_post",
                        column: x => x.post_id,
                        principalTable: "platform_community_posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "community_keyword_notification_deliveries",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    notification_id = table.Column<long>(type: "bigint", nullable: false),
                    installation_id = table.Column<long>(type: "bigint", nullable: false),
                    status = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    attempt_count = table.Column<int>(type: "int", nullable: false),
                    processing_token = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    last_error = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    next_attempt_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    sent_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_community_keyword_notification_deliveries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_community_keyword_delivery_installation",
                        column: x => x.installation_id,
                        principalTable: "ssalddel_mobile_push_installations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_community_keyword_delivery_notification",
                        column: x => x.notification_id,
                        principalTable: "community_keyword_notifications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_platform_community_posts_AuthorUserId",
                table: "platform_community_posts",
                column: "AuthorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_community_keyword_subscription_match",
                table: "community_keyword_subscriptions",
                columns: new[] { "app_key", "is_active" });

            migrationBuilder.CreateIndex(
                name: "UX_community_keyword_subscription",
                table: "community_keyword_subscriptions",
                columns: new[] { "user_id", "app_key", "normalized_keyword" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_community_keyword_scan_due",
                table: "platform_community_post_keyword_scans",
                columns: new[] { "status", "next_attempt_at_utc" });

            migrationBuilder.CreateIndex(
                name: "UX_community_keyword_scan_post",
                table: "platform_community_post_keyword_scans",
                column: "post_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_community_keyword_notifications_post_id",
                table: "community_keyword_notifications",
                column: "post_id");

            migrationBuilder.CreateIndex(
                name: "IX_community_keyword_notification_inbox",
                table: "community_keyword_notifications",
                columns: new[] { "user_id", "is_read", "created_at_utc" });

            migrationBuilder.CreateIndex(
                name: "UX_community_keyword_notification_user_post",
                table: "community_keyword_notifications",
                columns: new[] { "user_id", "post_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_community_keyword_delivery_due",
                table: "community_keyword_notification_deliveries",
                columns: new[] { "status", "next_attempt_at_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_community_keyword_delivery_installation",
                table: "community_keyword_notification_deliveries",
                column: "installation_id");

            migrationBuilder.CreateIndex(
                name: "UX_community_keyword_delivery_target",
                table: "community_keyword_notification_deliveries",
                columns: new[] { "notification_id", "installation_id" },
                unique: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_admin_enabled",
                table: "hongik_hakdang_card_collections",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_admin_enabled",
                table: "hongik_hakdang_cards",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "공급처코드",
                table: "입고요청",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "AuthorDisplayCountryCode",
                table: "platform_community_posts",
                type: "varchar(2)",
                maxLength: 2,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "AuthorDisplayCountryName",
                table: "platform_community_posts",
                type: "varchar(80)",
                maxLength: 80,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "IsAuthorDisplayCountryPublic",
                table: "platform_community_posts",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "CommunityMomentumCode",
                table: "platform_community_posts",
                type: "varchar(40)",
                maxLength: 40,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CommunityMomentumMessage",
                table: "platform_community_posts",
                type: "varchar(240)",
                maxLength: 240,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "CommunityMomentumRoleParticipantCount",
                table: "platform_community_posts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CommunityMomentumUpdatedAtUtc",
                table: "platform_community_posts",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCommunityMomentumPromoted",
                table: "platform_community_posts",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "channel_handle",
                table: "youtube_watched_channels",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "country_code",
                table: "youtube_watched_channels",
                type: "varchar(2)",
                maxLength: 2,
                nullable: false,
                defaultValue: "ZZ")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "default_language_code",
                table: "youtube_watched_channels",
                type: "varchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "food_category_codes",
                table: "youtube_watched_channels",
                type: "varchar(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "import_discovery_score",
                table: "youtube_watched_channels",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "is_food_channel",
                table: "youtube_watched_channels",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "purchase_discovery_score",
                table: "youtube_watched_channels",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "research_note",
                table: "youtube_watched_channels",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "research_source_url",
                table: "youtube_watched_channels",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "research_verified_at_utc",
                table: "youtube_watched_channels",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "youtube_video_product_candidates",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    youtube_channel_video_id = table.Column<long>(type: "bigint", nullable: false),
                    product_key = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    product_name = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    brand_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    origin_country_code = table.Column<string>(type: "varchar(2)", maxLength: 2, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    hs_code_candidate = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    temperature_code = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    logistics_mode = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    candidate_type = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    video_timestamp_seconds = table.Column<int>(type: "int", nullable: true),
                    discovery_evidence = table.Column<string>(type: "text", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    extraction_method = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    confidence = table.Column<decimal>(type: "decimal(5,4)", precision: 5, scale: 4, nullable: false),
                    review_status = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    sponsorship_disclosure_status = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    allowed_intent_types = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    official_purchase_url = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    review_note = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    reviewer_user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    reviewed_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_youtube_video_product_candidates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_youtube_video_product_candidates_youtube_channel_videos_yout~",
                        column: x => x.youtube_channel_video_id,
                        principalTable: "youtube_channel_videos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_youtube_watched_channels_is_food_channel_purchase_discovery_~",
                table: "youtube_watched_channels",
                columns: new[] { "is_food_channel", "purchase_discovery_score", "import_discovery_score" });

            migrationBuilder.CreateIndex(
                name: "IX_youtube_watched_channels_country_active_sync",
                table: "youtube_watched_channels",
                columns: new[] { "country_code", "is_active", "last_synced_at_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_youtube_video_product_candidates_candidate_type_review_status",
                table: "youtube_video_product_candidates",
                columns: new[] { "candidate_type", "review_status" });

            migrationBuilder.CreateIndex(
                name: "IX_youtube_video_product_candidates_review_status_updated_at_utc",
                table: "youtube_video_product_candidates",
                columns: new[] { "review_status", "updated_at_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_youtube_video_product_candidates_youtube_channel_video_id_pr~",
                table: "youtube_video_product_candidates",
                columns: new[] { "youtube_channel_video_id", "product_key" },
                unique: true);

            migrationBuilder.AddColumn<string>(
                name: "SalesOfferJson",
                table: "platform_community_posts",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "OriginalLanguageCode",
                table: "platform_community_posts",
                type: "varchar(16)",
                maxLength: 16,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "platform_community_post_translations",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PostId = table.Column<long>(type: "bigint", nullable: false),
                    SourceLanguageCode = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TargetLanguageCode = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SourceContentHash = table.Column<string>(type: "char(64)", fixedLength: true, maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TranslatedTitle = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TranslatedBody = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Provider = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProviderModelVersion = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsHumanReviewed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platform_community_post_translations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_platform_community_post_translations_posts",
                        column: x => x.PostId,
                        principalTable: "platform_community_posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_community_post_translation_post_created",
                table: "platform_community_post_translations",
                columns: new[] { "PostId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "UX_community_post_translation_content",
                table: "platform_community_post_translations",
                columns: new[] { "PostId", "TargetLanguageCode", "SourceContentHash" },
                unique: true);

            migrationBuilder.AddColumn<string>(
                name: "RequestedByUserId",
                table: "platform_community_board_requests",
                type: "varchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ReviewedByUserId",
                table: "platform_community_board_requests",
                type: "varchar(450)",
                maxLength: 450,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_community_board_requests_requester_status",
                table: "platform_community_board_requests",
                columns: new[] { "RequestedByUserId", "Status", "IsDeleted", "CreatedAtUtc" });

            migrationBuilder.AddColumn<bool>(
                name: "is_community_publication_approved",
                table: "hongik_hakdang_cards",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_hh_cards_community_publication",
                table: "hongik_hakdang_cards",
                columns: new[] { "is_community_publication_approved", "is_active", "is_admin_enabled" });

            migrationBuilder.AddColumn<bool>(
                name: "is_knowledge_reflection_channel",
                table: "youtube_watched_channels",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "knowledge_reflection_category_codes",
                table: "youtube_watched_channels",
                type: "varchar(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "perspective_label",
                table: "youtube_watched_channels",
                type: "varchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "official_source_url",
                table: "youtube_watched_channels",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "source_verified_at_utc",
                table: "youtube_watched_channels",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_prajna_publication_allowed",
                table: "youtube_watched_channels",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_youtube_watched_channels_knowledge_prajna_active",
                table: "youtube_watched_channels",
                columns: new[] { "is_knowledge_reflection_channel", "is_prajna_publication_allowed", "is_active" });

            migrationBuilder.Sql("UPDATE youtube_watched_channels\nSET is_knowledge_reflection_channel = 1,\n    knowledge_reflection_category_codes = 'philosophy,ethics,self-development',\n    perspective_label = '홍익·양심 공부',\n    official_source_url = 'https://www.youtube.com/channel/UCI8HW08rOSlvweOjJ9Gp2Ng',\n    source_verified_at_utc = '2026-07-18 00:00:00'\nWHERE channel_id = 'UCI8HW08rOSlvweOjJ9Gp2Ng';");

            migrationBuilder.AddColumn<int>(
                name: "PublicationAttemptCount",
                table: "platform_community_posts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "PublicationClaimedAtUtc",
                table: "platform_community_posts",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PublicationLastError",
                table: "platform_community_posts",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "PublicationNextAttemptAtUtc",
                table: "platform_community_posts",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PublicationStatusCode",
                table: "platform_community_posts",
                type: "varchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "published")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "PublishedAtUtc",
                table: "platform_community_posts",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ScheduledPublishAtUtc",
                table: "platform_community_posts",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.Sql("UPDATE `platform_community_posts` SET `PublishedAtUtc` = `CreatedAtUtc` WHERE `PublishedAtUtc` IS NULL;");

            migrationBuilder.CreateIndex(
                name: "IX_platform_community_posts_publication_due",
                table: "platform_community_posts",
                columns: new[] { "PublicationStatusCode", "PublicationNextAttemptAtUtc", "PublicationClaimedAtUtc" });

            migrationBuilder.AddColumn<DateTime>(
                name: "PrivacyConsentedAtUtc",
                table: "AspNetUsers",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrivacyConsentVersion",
                table: "AspNetUsers",
                type: "varchar(64)",
                maxLength: 64,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "음식점공개프로필",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    업체_id = table.Column<long>(type: "bigint", nullable: true),
                    상호명 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    카테고리 = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    소개 = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    공개주소 = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    위도 = table.Column<decimal>(type: "decimal(18,10)", nullable: false),
                    경도 = table.Column<decimal>(type: "decimal(18,10)", nullable: false),
                    대표이미지_url = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    최소주문금액 = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    예상조리분 = table.Column<int>(type: "int", nullable: false),
                    공개여부 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    주문가능여부 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_음식점공개프로필", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "음식점메뉴",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    음식점공개프로필_id = table.Column<long>(type: "bigint", nullable: false),
                    메뉴명 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    설명 = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    판매가 = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    대표이미지_url = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    공개여부 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    품절여부 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    표시순서 = table.Column<int>(type: "int", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_음식점메뉴", x => x.id);
                    table.ForeignKey(
                        name: "FK_음식점메뉴_음식점공개프로필_음식점공개프로필_id",
                        column: x => x.음식점공개프로필_id,
                        principalTable: "음식점공개프로필",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_음식점공개프로필_공개여부_주문가능여부_updated_at_utc",
                table: "음식점공개프로필",
                columns: new[] { "공개여부", "주문가능여부", "updated_at_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_음식점공개프로필_업체_id",
                table: "음식점공개프로필",
                column: "업체_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_음식점공개프로필_위도_경도",
                table: "음식점공개프로필",
                columns: new[] { "위도", "경도" });

            migrationBuilder.CreateIndex(
                name: "IX_음식점메뉴_음식점공개프로필_id_공개여부_표시순서",
                table: "음식점메뉴",
                columns: new[] { "음식점공개프로필_id", "공개여부", "표시순서" });

            migrationBuilder.CreateIndex(
                name: "IX_음식점메뉴_음식점공개프로필_id_메뉴명",
                table: "음식점메뉴",
                columns: new[] { "음식점공개프로필_id", "메뉴명" },
                unique: true);

            migrationBuilder.CreateTable(
                name: "마트공개상품",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    판매상품_id = table.Column<long>(type: "bigint", nullable: true),
                    상품명 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    카테고리 = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    짧은설명 = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    설명 = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    판매단위 = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    판매가 = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    대표이미지_url = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    판매가능수량 = table.Column<int>(type: "int", nullable: false),
                    공개여부 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    판매허용여부 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    재고기준시각_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_마트공개상품", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_마트공개상품_공개여부_판매허용여부_updated_at_utc",
                table: "마트공개상품",
                columns: new[] { "공개여부", "판매허용여부", "updated_at_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_마트공개상품_카테고리_상품명",
                table: "마트공개상품",
                columns: new[] { "카테고리", "상품명" });

            migrationBuilder.CreateIndex(
                name: "IX_마트공개상품_판매상품_id",
                table: "마트공개상품",
                column: "판매상품_id",
                unique: true);

            migrationBuilder.CreateTable(
                name: "hr_role_applications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    applicant_user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    participant_category = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    requested_role_code = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    requested_role_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    scope_type = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    scope_id = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status_code = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    submission_request_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    active_application_key = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    confirmed_voluntary_application = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    confirmed_no_role_or_employment_guarantee = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    confirmed_review_data_use = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    consent_version = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    submitted_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    withdrawn_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hr_role_applications", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_hr_role_applications_active_application_key",
                table: "hr_role_applications",
                column: "active_application_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hr_role_applications_applicant_user_id_submission_request_id",
                table: "hr_role_applications",
                columns: new[] { "applicant_user_id", "submission_request_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hr_role_applications_applicant_user_id_submitted_at_utc",
                table: "hr_role_applications",
                columns: new[] { "applicant_user_id", "submitted_at_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_hr_role_applications_status_code_submitted_at_utc",
                table: "hr_role_applications",
                columns: new[] { "status_code", "submitted_at_utc" });

            migrationBuilder.CreateTable(
                name: "마트주문요청",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    요청자_user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    클라이언트_요청_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    공개상품_id = table.Column<long>(type: "bigint", nullable: false),
                    상품명_snapshot = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    판매단위_snapshot = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    단가_snapshot = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    수량 = table.Column<int>(type: "int", nullable: false),
                    합계_snapshot = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    통화 = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    제출시_판매가능수량 = table.Column<int>(type: "int", nullable: false),
                    재고기준시각_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    상태_code = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    비구속_주문요청_확인 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    안내_version = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_마트주문요청", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_마트주문요청_공개상품_id_created_at_utc",
                table: "마트주문요청",
                columns: new[] { "공개상품_id", "created_at_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_마트주문요청_요청자_user_id_클라이언트_요청_id",
                table: "마트주문요청",
                columns: new[] { "요청자_user_id", "클라이언트_요청_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_마트주문요청_요청자_user_id_created_at_utc",
                table: "마트주문요청",
                columns: new[] { "요청자_user_id", "created_at_utc" });

            migrationBuilder.AddColumn<string>(
                name: "보관조건",
                table: "입고요청",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "예정_sku",
                table: "입고요청",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "예정상품명",
                table: "입고요청",
                type: "varchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "예정수량",
                table: "입고요청",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "입고묶음바코드",
                table: "입고요청",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "현장입고_안내_version",
                table: "입고요청",
                type: "varchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<Guid>(
                name: "현장입고_클라이언트_요청_id",
                table: "입고요청",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<string>(
                name: "현장입고사유",
                table: "입고요청",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_입고요청_주문자_user_id_현장입고_클라이언트_요청_id",
                table: "입고요청",
                columns: new[] { "주문자_user_id", "현장입고_클라이언트_요청_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_입고요청_창고_id_예정_sku_상태",
                table: "입고요청",
                columns: new[] { "창고_id", "예정_sku", "상태" });

            migrationBuilder.CreateIndex(
                name: "IX_입고상품_입고요청_id",
                table: "입고상품",
                column: "입고요청_id");

            migrationBuilder.CreateIndex(
                name: "IX_재고이동_입고상품_id_발생일시",
                table: "재고이동",
                columns: new[] { "입고상품_id", "발생일시" });

            migrationBuilder.AddColumn<long>(
                name: "ViewCount",
                table: "platform_community_posts",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<bool>(
                name: "IsInterestGatheringEnabled",
                table: "platform_community_posts",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "community_post_email_notification_outbox",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PostId = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AttemptCount = table.Column<int>(type: "int", nullable: false),
                    NextAttemptAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ProcessingToken = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LockedUntilUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    LastError = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ProcessedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_community_post_email_notification_outbox", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_community_post_email_notification_outbox_LockedUntilUtc",
                table: "community_post_email_notification_outbox",
                column: "LockedUntilUtc");

            migrationBuilder.CreateIndex(
                name: "IX_community_post_email_notification_outbox_PostId",
                table: "community_post_email_notification_outbox",
                column: "PostId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_community_post_email_notification_outbox_Status_NextAttemptA~",
                table: "community_post_email_notification_outbox",
                columns: new[] { "Status", "NextAttemptAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "community_post_email_notification_outbox");

            migrationBuilder.DropColumn(
                name: "IsInterestGatheringEnabled",
                table: "platform_community_posts");

            migrationBuilder.DropColumn(
                name: "ViewCount",
                table: "platform_community_posts");

            migrationBuilder.DropIndex(
                name: "IX_재고이동_입고상품_id_발생일시",
                table: "재고이동");

            migrationBuilder.DropIndex(
                name: "IX_입고상품_입고요청_id",
                table: "입고상품");

            migrationBuilder.DropIndex(
                name: "IX_입고요청_주문자_user_id_현장입고_클라이언트_요청_id",
                table: "입고요청");

            migrationBuilder.DropIndex(
                name: "IX_입고요청_창고_id_예정_sku_상태",
                table: "입고요청");

            migrationBuilder.DropColumn(
                name: "보관조건",
                table: "입고요청");

            migrationBuilder.DropColumn(
                name: "예정_sku",
                table: "입고요청");

            migrationBuilder.DropColumn(
                name: "예정상품명",
                table: "입고요청");

            migrationBuilder.DropColumn(
                name: "예정수량",
                table: "입고요청");

            migrationBuilder.DropColumn(
                name: "입고묶음바코드",
                table: "입고요청");

            migrationBuilder.DropColumn(
                name: "현장입고_안내_version",
                table: "입고요청");

            migrationBuilder.DropColumn(
                name: "현장입고_클라이언트_요청_id",
                table: "입고요청");

            migrationBuilder.DropColumn(
                name: "현장입고사유",
                table: "입고요청");

            migrationBuilder.DropTable(
                name: "마트주문요청");

            migrationBuilder.DropTable(
                name: "hr_role_applications");

            migrationBuilder.DropTable(
                name: "마트공개상품");

            migrationBuilder.DropTable(
                name: "음식점메뉴");

            migrationBuilder.DropTable(
                name: "음식점공개프로필");

            migrationBuilder.DropColumn(
                name: "PrivacyConsentedAtUtc",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PrivacyConsentVersion",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_platform_community_posts_publication_due",
                table: "platform_community_posts");

            migrationBuilder.DropColumn(
                name: "PublicationAttemptCount",
                table: "platform_community_posts");

            migrationBuilder.DropColumn(
                name: "PublicationClaimedAtUtc",
                table: "platform_community_posts");

            migrationBuilder.DropColumn(
                name: "PublicationLastError",
                table: "platform_community_posts");

            migrationBuilder.DropColumn(
                name: "PublicationNextAttemptAtUtc",
                table: "platform_community_posts");

            migrationBuilder.DropColumn(
                name: "PublicationStatusCode",
                table: "platform_community_posts");

            migrationBuilder.DropColumn(
                name: "PublishedAtUtc",
                table: "platform_community_posts");

            migrationBuilder.DropColumn(
                name: "ScheduledPublishAtUtc",
                table: "platform_community_posts");

            migrationBuilder.DropIndex(
                name: "IX_youtube_watched_channels_knowledge_prajna_active",
                table: "youtube_watched_channels");

            migrationBuilder.DropColumn(
                name: "is_knowledge_reflection_channel",
                table: "youtube_watched_channels");

            migrationBuilder.DropColumn(
                name: "knowledge_reflection_category_codes",
                table: "youtube_watched_channels");

            migrationBuilder.DropColumn(
                name: "perspective_label",
                table: "youtube_watched_channels");

            migrationBuilder.DropColumn(
                name: "official_source_url",
                table: "youtube_watched_channels");

            migrationBuilder.DropColumn(
                name: "source_verified_at_utc",
                table: "youtube_watched_channels");

            migrationBuilder.DropColumn(
                name: "is_prajna_publication_allowed",
                table: "youtube_watched_channels");

            migrationBuilder.DropIndex(
                name: "IX_hh_cards_community_publication",
                table: "hongik_hakdang_cards");

            migrationBuilder.DropColumn(
                name: "is_community_publication_approved",
                table: "hongik_hakdang_cards");

            migrationBuilder.DropIndex(
                name: "IX_community_board_requests_requester_status",
                table: "platform_community_board_requests");

            migrationBuilder.DropColumn(
                name: "RequestedByUserId",
                table: "platform_community_board_requests");

            migrationBuilder.DropColumn(
                name: "ReviewedByUserId",
                table: "platform_community_board_requests");

            migrationBuilder.DropTable(
                name: "platform_community_post_translations");

            migrationBuilder.DropColumn(
                name: "OriginalLanguageCode",
                table: "platform_community_posts");

            migrationBuilder.DropColumn(
                name: "SalesOfferJson",
                table: "platform_community_posts");

            migrationBuilder.DropTable(
                name: "youtube_video_product_candidates");

            migrationBuilder.DropIndex(
                name: "IX_youtube_watched_channels_is_food_channel_purchase_discovery_~",
                table: "youtube_watched_channels");

            migrationBuilder.DropIndex(
                name: "IX_youtube_watched_channels_country_active_sync",
                table: "youtube_watched_channels");

            migrationBuilder.DropColumn(
                name: "channel_handle",
                table: "youtube_watched_channels");

            migrationBuilder.DropColumn(
                name: "country_code",
                table: "youtube_watched_channels");

            migrationBuilder.DropColumn(
                name: "default_language_code",
                table: "youtube_watched_channels");

            migrationBuilder.DropColumn(
                name: "food_category_codes",
                table: "youtube_watched_channels");

            migrationBuilder.DropColumn(
                name: "import_discovery_score",
                table: "youtube_watched_channels");

            migrationBuilder.DropColumn(
                name: "is_food_channel",
                table: "youtube_watched_channels");

            migrationBuilder.DropColumn(
                name: "purchase_discovery_score",
                table: "youtube_watched_channels");

            migrationBuilder.DropColumn(
                name: "research_note",
                table: "youtube_watched_channels");

            migrationBuilder.DropColumn(
                name: "research_source_url",
                table: "youtube_watched_channels");

            migrationBuilder.DropColumn(
                name: "research_verified_at_utc",
                table: "youtube_watched_channels");

            migrationBuilder.DropColumn(
                name: "CommunityMomentumCode",
                table: "platform_community_posts");

            migrationBuilder.DropColumn(
                name: "CommunityMomentumMessage",
                table: "platform_community_posts");

            migrationBuilder.DropColumn(
                name: "CommunityMomentumRoleParticipantCount",
                table: "platform_community_posts");

            migrationBuilder.DropColumn(
                name: "CommunityMomentumUpdatedAtUtc",
                table: "platform_community_posts");

            migrationBuilder.DropColumn(
                name: "IsCommunityMomentumPromoted",
                table: "platform_community_posts");

            migrationBuilder.DropColumn(
                name: "AuthorDisplayCountryCode",
                table: "platform_community_posts");

            migrationBuilder.DropColumn(
                name: "AuthorDisplayCountryName",
                table: "platform_community_posts");

            migrationBuilder.DropColumn(
                name: "IsAuthorDisplayCountryPublic",
                table: "platform_community_posts");

            migrationBuilder.DropColumn(
                name: "공급처코드",
                table: "입고요청");

            migrationBuilder.DropColumn(
                name: "is_admin_enabled",
                table: "hongik_hakdang_card_collections");

            migrationBuilder.DropColumn(
                name: "is_admin_enabled",
                table: "hongik_hakdang_cards");

            migrationBuilder.DropTable(
                name: "community_keyword_notification_deliveries");

            migrationBuilder.DropTable(
                name: "community_keyword_subscriptions");

            migrationBuilder.DropTable(
                name: "platform_community_post_keyword_scans");

            migrationBuilder.DropTable(
                name: "community_keyword_notifications");

            migrationBuilder.DropIndex(
                name: "IX_platform_community_posts_AuthorUserId",
                table: "platform_community_posts");

            migrationBuilder.DropColumn(
                name: "AuthorUserId",
                table: "platform_community_posts");

            migrationBuilder.DropTable(
                name: "hongik_hakdang_card_delivery_outbox");

            migrationBuilder.DropTable(
                name: "hongik_hakdang_card_delivery_preferences");

            migrationBuilder.DropTable(
                name: "hongik_hakdang_card_image_variants");

            migrationBuilder.DropTable(
                name: "hongik_hakdang_daily_card_selections");

            migrationBuilder.DropTable(
                name: "ssalddel_mobile_push_installations");

            migrationBuilder.DropTable(
                name: "hongik_hakdang_card_collection_items");

            migrationBuilder.DropTable(
                name: "hongik_hakdang_card_collections");

            migrationBuilder.DropTable(
                name: "hongik_hakdang_cards");

            migrationBuilder.DropIndex(
                name: "IX_platform_community_posts_커뮤니티원장Id",
                table: "platform_community_posts");

            migrationBuilder.DropColumn(
                name: "커뮤니티원장Id",
                table: "platform_community_posts");

            migrationBuilder.CreateTable(
                name: "community_ledger_block_projections",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    BlockId = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BlockType = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DiagramNodeId = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RelatedRoute = table.Column<string>(type: "varchar(400)", maxLength: 400, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    State = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Title = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UiSectionHint = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    속성Json = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    원장템플릿Key = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    커뮤니티Id = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    커뮤니티원장Id = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_community_ledger_block_projections", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "community_ledger_block_relation_projections",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FromBlockProjectionId = table.Column<long>(type: "bigint", nullable: false),
                    ToBlockProjectionId = table.Column<long>(type: "bigint", nullable: false),
                    Cardinality = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DiagramEdgeId = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FromBlockId = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Label = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MeaningCode = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    ToBlockId = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    관계유형 = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    조건식Json = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    커뮤니티원장Id = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    필수여부 = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_community_ledger_block_relation_projections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_community_ledger_block_relation_projections_community_ledger~",
                        column: x => x.FromBlockProjectionId,
                        principalTable: "community_ledger_block_projections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_community_ledger_block_relation_projections_community_ledge~1",
                        column: x => x.ToBlockProjectionId,
                        principalTable: "community_ledger_block_projections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_community_ledger_block_projections_커뮤니티원장Id_BlockId",
                table: "community_ledger_block_projections",
                columns: new[] { "커뮤니티원장Id", "BlockId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_community_ledger_block_projections_커뮤니티원장Id_SortOrder",
                table: "community_ledger_block_projections",
                columns: new[] { "커뮤니티원장Id", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_community_ledger_block_projections_커뮤니티Id_원장템플릿Key_BlockType",
                table: "community_ledger_block_projections",
                columns: new[] { "커뮤니티Id", "원장템플릿Key", "BlockType" });

            migrationBuilder.CreateIndex(
                name: "IX_community_ledger_block_projections_DiagramNodeId",
                table: "community_ledger_block_projections",
                column: "DiagramNodeId");

            migrationBuilder.CreateIndex(
                name: "IX_community_ledger_block_relation_projections_커뮤니티원장Id_Cardina~",
                table: "community_ledger_block_relation_projections",
                columns: new[] { "커뮤니티원장Id", "Cardinality" });

            migrationBuilder.CreateIndex(
                name: "IX_community_ledger_block_relation_projections_커뮤니티원장Id_FromBlo~",
                table: "community_ledger_block_relation_projections",
                columns: new[] { "커뮤니티원장Id", "FromBlockId", "ToBlockId", "관계유형" });

            migrationBuilder.CreateIndex(
                name: "IX_community_ledger_block_relation_projections_DiagramEdgeId",
                table: "community_ledger_block_relation_projections",
                column: "DiagramEdgeId");

            migrationBuilder.CreateIndex(
                name: "IX_community_ledger_block_relation_projections_FromBlockProject~",
                table: "community_ledger_block_relation_projections",
                column: "FromBlockProjectionId");

            migrationBuilder.CreateIndex(
                name: "IX_community_ledger_block_relation_projections_ToBlockProjectio~",
                table: "community_ledger_block_relation_projections",
                column: "ToBlockProjectionId");

            migrationBuilder.DropTable(
                name: "education_course_attendances");

            migrationBuilder.DropTable(
                name: "education_course_submissions");

            migrationBuilder.DropTable(
                name: "education_course_subjects");

            migrationBuilder.DropTable(
                name: "education_course_enrollments");

            migrationBuilder.DropTable(
                name: "education_course_forms");

            migrationBuilder.DropTable(
                name: "education_course_applications");

            migrationBuilder.DropTable(
                name: "education_courses");

            migrationBuilder.DropTable(
                name: "youtube_channel_videos");

            migrationBuilder.DropTable(
                name: "youtube_watched_channels");

            migrationBuilder.DropTable(
                name: "platform_community_post_audio_access_logs");

            migrationBuilder.DropTable(
                name: "platform_community_post_audio_segments");

            migrationBuilder.DropTable(
                name: "platform_community_post_audio");

            migrationBuilder.DropTable(
                name: "typecast_voice_models");

            migrationBuilder.DropTable(
                name: "typecast_voice_use_cases");

            migrationBuilder.DropTable(
                name: "typecast_voices");

            migrationBuilder.DropIndex(
                name: "IX_입고요청_community_ledger_id",
                table: "입고요청");

            migrationBuilder.DropIndex(
                name: "IX_입고상품_community_ledger_id",
                table: "입고상품");

            migrationBuilder.DropIndex(
                name: "IX_출고예정_community_ledger_id",
                table: "출고예정");

            migrationBuilder.DropIndex(
                name: "IX_출고묶음_community_ledger_id",
                table: "출고묶음");

            migrationBuilder.DropColumn(
                name: "community_ledger_id",
                table: "입고요청");

            migrationBuilder.DropColumn(
                name: "community_ledger_template_key",
                table: "입고요청");

            migrationBuilder.DropColumn(
                name: "community_ledger_state",
                table: "입고요청");

            migrationBuilder.DropColumn(
                name: "community_ledger_synced_at_utc",
                table: "입고요청");

            migrationBuilder.DropColumn(
                name: "community_ledger_id",
                table: "입고상품");

            migrationBuilder.DropColumn(
                name: "community_ledger_template_key",
                table: "입고상품");

            migrationBuilder.DropColumn(
                name: "community_ledger_state",
                table: "입고상품");

            migrationBuilder.DropColumn(
                name: "community_ledger_synced_at_utc",
                table: "입고상품");

            migrationBuilder.DropColumn(
                name: "community_ledger_id",
                table: "출고예정");

            migrationBuilder.DropColumn(
                name: "community_ledger_template_key",
                table: "출고예정");

            migrationBuilder.DropColumn(
                name: "community_ledger_state",
                table: "출고예정");

            migrationBuilder.DropColumn(
                name: "community_ledger_synced_at_utc",
                table: "출고예정");

            migrationBuilder.DropColumn(
                name: "community_ledger_id",
                table: "출고묶음");

            migrationBuilder.DropColumn(
                name: "community_ledger_template_key",
                table: "출고묶음");

            migrationBuilder.DropColumn(
                name: "community_ledger_state",
                table: "출고묶음");

            migrationBuilder.DropColumn(
                name: "community_ledger_synced_at_utc",
                table: "출고묶음");

            migrationBuilder.DropIndex(
                name: "IX_운송실행투영_community_ledger_id",
                table: "운송실행투영");

            migrationBuilder.DropColumn(
                name: "community_ledger_id",
                table: "운송실행투영");

            migrationBuilder.DropColumn(
                name: "community_ledger_template_key",
                table: "운송실행투영");

            migrationBuilder.DropColumn(
                name: "community_ledger_state",
                table: "운송실행투영");

            migrationBuilder.DropColumn(
                name: "community_ledger_synced_at_utc",
                table: "운송실행투영");

            migrationBuilder.Sql("\nSET @table_exists = (\n    SELECT COUNT(*)\n    FROM INFORMATION_SCHEMA.TABLES\n    WHERE TABLE_SCHEMA = DATABASE()\n      AND TABLE_NAME = '운송문서'\n);\nSET @old_column_exists = (\n    SELECT COUNT(*)\n    FROM INFORMATION_SCHEMA.COLUMNS\n    WHERE TABLE_SCHEMA = DATABASE()\n      AND TABLE_NAME = '운송문서'\n      AND COLUMN_NAME = '운송실행투영_id'\n);\nSET @new_column_exists = (\n    SELECT COUNT(*)\n    FROM INFORMATION_SCHEMA.COLUMNS\n    WHERE TABLE_SCHEMA = DATABASE()\n      AND TABLE_NAME = '운송문서'\n      AND COLUMN_NAME = '운송원장_id'\n);\nSET @sql = IF(@table_exists = 1 AND @old_column_exists = 1 AND @new_column_exists = 0,\n    'ALTER TABLE `운송문서` CHANGE COLUMN `운송실행투영_id` `운송원장_id` bigint NULL',\n    'SELECT 1'\n);\nPREPARE stmt FROM @sql;\nEXECUTE stmt;\nDEALLOCATE PREPARE stmt;");

            migrationBuilder.Sql("\nSET @old_table_exists = (\n    SELECT COUNT(*)\n    FROM INFORMATION_SCHEMA.TABLES\n    WHERE TABLE_SCHEMA = DATABASE()\n      AND TABLE_NAME = '운송실행투영'\n);\nSET @new_table_exists = (\n    SELECT COUNT(*)\n    FROM INFORMATION_SCHEMA.TABLES\n    WHERE TABLE_SCHEMA = DATABASE()\n      AND TABLE_NAME = '운송원장'\n);\nSET @sql = IF(@old_table_exists = 1 AND @new_table_exists = 0,\n    'RENAME TABLE `운송실행투영` TO `운송원장`',\n    'SELECT 1'\n);\nPREPARE stmt FROM @sql;\nEXECUTE stmt;\nDEALLOCATE PREPARE stmt;");

            migrationBuilder.DropTable(
                name: "community_ledger_block_relation_projections");

            migrationBuilder.DropTable(
                name: "community_ledger_block_projections");

            migrationBuilder.DropForeignKey(
                name: "FK_피킹포장작업_출고묶음_출고묶음_id",
                table: "피킹포장작업");

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

            migrationBuilder.Sql("\nSET @table_exists = (\n    SELECT COUNT(*)\n    FROM INFORMATION_SCHEMA.TABLES\n    WHERE TABLE_SCHEMA = DATABASE()\n      AND TABLE_NAME = '운송문서'\n);\nSET @old_column_exists = (\n    SELECT COUNT(*)\n    FROM INFORMATION_SCHEMA.COLUMNS\n    WHERE TABLE_SCHEMA = DATABASE()\n      AND TABLE_NAME = '운송문서'\n      AND COLUMN_NAME = '운송원장_id'\n);\nSET @new_column_exists = (\n    SELECT COUNT(*)\n    FROM INFORMATION_SCHEMA.COLUMNS\n    WHERE TABLE_SCHEMA = DATABASE()\n      AND TABLE_NAME = '운송문서'\n      AND COLUMN_NAME = '배송_운송_id'\n);\nSET @sql = IF(@table_exists = 1 AND @old_column_exists = 1 AND @new_column_exists = 0,\n    'ALTER TABLE `운송문서` CHANGE COLUMN `운송원장_id` `배송_운송_id` bigint NULL',\n    'SELECT 1'\n);\nPREPARE stmt FROM @sql;\nEXECUTE stmt;\nDEALLOCATE PREPARE stmt;");

            migrationBuilder.Sql("\nSET @old_table_exists = (\n    SELECT COUNT(*)\n    FROM INFORMATION_SCHEMA.TABLES\n    WHERE TABLE_SCHEMA = DATABASE()\n      AND TABLE_NAME = '운송원장'\n);\nSET @new_table_exists = (\n    SELECT COUNT(*)\n    FROM INFORMATION_SCHEMA.TABLES\n    WHERE TABLE_SCHEMA = DATABASE()\n      AND TABLE_NAME = '배송_운송'\n);\nSET @sql = IF(@old_table_exists = 1 AND @new_table_exists = 0,\n    'RENAME TABLE `운송원장` TO `배송_운송`',\n    'SELECT 1'\n);\nPREPARE stmt FROM @sql;\nEXECUTE stmt;\nDEALLOCATE PREPARE stmt;");

            migrationBuilder.DropTable(
                name: "피킹포장작업");

            migrationBuilder.DropTable(
                name: "community_ledger_state_events");

            migrationBuilder.DropTable(
                name: "마트주문상품");

            migrationBuilder.DropTable(
                name: "음식주문상태이력");

            migrationBuilder.DropTable(
                name: "음식주문상품");

            migrationBuilder.DropTable(
                name: "마트주문");

            migrationBuilder.DropTable(
                name: "음식주문");

            migrationBuilder.Sql("\nSET @column_exists = (\n    SELECT COUNT(*)\n    FROM INFORMATION_SCHEMA.COLUMNS\n    WHERE TABLE_SCHEMA = DATABASE()\n      AND TABLE_NAME = '기사배차'\n      AND COLUMN_NAME = 'notion_page_id'\n);\nSET @sql = IF(@column_exists = 0,\n    'ALTER TABLE `기사배차` ADD COLUMN `notion_page_id` longtext NULL',\n    'SELECT 1'\n);\nPREPARE stmt FROM @sql;\nEXECUTE stmt;\nDEALLOCATE PREPARE stmt;");

            migrationBuilder.Sql("\nSET @column_exists = (\n    SELECT COUNT(*)\n    FROM INFORMATION_SCHEMA.COLUMNS\n    WHERE TABLE_SCHEMA = DATABASE()\n      AND TABLE_NAME = '배달기사'\n      AND COLUMN_NAME = 'notion_page_id'\n);\nSET @sql = IF(@column_exists = 0,\n    'ALTER TABLE `배달기사` ADD COLUMN `notion_page_id` longtext NULL',\n    'SELECT 1'\n);\nPREPARE stmt FROM @sql;\nEXECUTE stmt;\nDEALLOCATE PREPARE stmt;");

            migrationBuilder.Sql("\nSET @column_exists = (\n    SELECT COUNT(*)\n    FROM INFORMATION_SCHEMA.COLUMNS\n    WHERE TABLE_SCHEMA = DATABASE()\n      AND TABLE_NAME = '배송_운송'\n      AND COLUMN_NAME = 'notion_page_id'\n);\nSET @sql = IF(@column_exists = 0,\n    'ALTER TABLE `배송_운송` ADD COLUMN `notion_page_id` longtext NULL',\n    'SELECT 1'\n);\nPREPARE stmt FROM @sql;\nEXECUTE stmt;\nDEALLOCATE PREPARE stmt;");

            migrationBuilder.Sql("\nSET @column_exists = (\n    SELECT COUNT(*)\n    FROM INFORMATION_SCHEMA.COLUMNS\n    WHERE TABLE_SCHEMA = DATABASE()\n      AND TABLE_NAME = '배차_최소'\n      AND COLUMN_NAME = 'notion_page_id'\n);\nSET @sql = IF(@column_exists = 0,\n    'ALTER TABLE `배차_최소` ADD COLUMN `notion_page_id` longtext NULL',\n    'SELECT 1'\n);\nPREPARE stmt FROM @sql;\nEXECUTE stmt;\nDEALLOCATE PREPARE stmt;");

            migrationBuilder.Sql("\nSET @column_exists = (\n    SELECT COUNT(*)\n    FROM INFORMATION_SCHEMA.COLUMNS\n    WHERE TABLE_SCHEMA = DATABASE()\n      AND TABLE_NAME = '업체'\n      AND COLUMN_NAME = 'notion_page_id'\n);\nSET @sql = IF(@column_exists = 0,\n    'ALTER TABLE `업체` ADD COLUMN `notion_page_id` longtext NULL',\n    'SELECT 1'\n);\nPREPARE stmt FROM @sql;\nEXECUTE stmt;\nDEALLOCATE PREPARE stmt;");

            migrationBuilder.Sql("\nSET @column_exists = (\n    SELECT COUNT(*)\n    FROM INFORMATION_SCHEMA.COLUMNS\n    WHERE TABLE_SCHEMA = DATABASE()\n      AND TABLE_NAME = '용달기사'\n      AND COLUMN_NAME = 'notion_page_id'\n);\nSET @sql = IF(@column_exists = 0,\n    'ALTER TABLE `용달기사` ADD COLUMN `notion_page_id` longtext NULL',\n    'SELECT 1'\n);\nPREPARE stmt FROM @sql;\nEXECUTE stmt;\nDEALLOCATE PREPARE stmt;");

            migrationBuilder.Sql("\nSET @column_exists = (\n    SELECT COUNT(*)\n    FROM INFORMATION_SCHEMA.COLUMNS\n    WHERE TABLE_SCHEMA = DATABASE()\n      AND TABLE_NAME = 'driver_shifts'\n      AND COLUMN_NAME = 'return_destination_recorded_at'\n);\nSET @sql = IF(@column_exists = 1,\n    'ALTER TABLE `driver_shifts` DROP COLUMN `return_destination_recorded_at`',\n    'SELECT 1'\n);\nPREPARE stmt FROM @sql;\nEXECUTE stmt;\nDEALLOCATE PREPARE stmt;");

            migrationBuilder.Sql("\nSET @column_exists = (\n    SELECT COUNT(*)\n    FROM INFORMATION_SCHEMA.COLUMNS\n    WHERE TABLE_SCHEMA = DATABASE()\n      AND TABLE_NAME = 'driver_shifts'\n      AND COLUMN_NAME = 'return_destination_source'\n);\nSET @sql = IF(@column_exists = 1,\n    'ALTER TABLE `driver_shifts` DROP COLUMN `return_destination_source`',\n    'SELECT 1'\n);\nPREPARE stmt FROM @sql;\nEXECUTE stmt;\nDEALLOCATE PREPARE stmt;");

            migrationBuilder.Sql("\nSET @column_exists = (\n    SELECT COUNT(*)\n    FROM INFORMATION_SCHEMA.COLUMNS\n    WHERE TABLE_SCHEMA = DATABASE()\n      AND TABLE_NAME = 'driver_shifts'\n      AND COLUMN_NAME = 'today_return_longitude'\n);\nSET @sql = IF(@column_exists = 1,\n    'ALTER TABLE `driver_shifts` DROP COLUMN `today_return_longitude`',\n    'SELECT 1'\n);\nPREPARE stmt FROM @sql;\nEXECUTE stmt;\nDEALLOCATE PREPARE stmt;");

            migrationBuilder.Sql("\nSET @column_exists = (\n    SELECT COUNT(*)\n    FROM INFORMATION_SCHEMA.COLUMNS\n    WHERE TABLE_SCHEMA = DATABASE()\n      AND TABLE_NAME = 'driver_shifts'\n      AND COLUMN_NAME = 'today_return_latitude'\n);\nSET @sql = IF(@column_exists = 1,\n    'ALTER TABLE `driver_shifts` DROP COLUMN `today_return_latitude`',\n    'SELECT 1'\n);\nPREPARE stmt FROM @sql;\nEXECUTE stmt;\nDEALLOCATE PREPARE stmt;");

            migrationBuilder.Sql("\nSET @column_exists = (\n    SELECT COUNT(*)\n    FROM INFORMATION_SCHEMA.COLUMNS\n    WHERE TABLE_SCHEMA = DATABASE()\n      AND TABLE_NAME = 'driver_shifts'\n      AND COLUMN_NAME = 'today_return_destination'\n);\nSET @sql = IF(@column_exists = 1,\n    'ALTER TABLE `driver_shifts` DROP COLUMN `today_return_destination`',\n    'SELECT 1'\n);\nPREPARE stmt FROM @sql;\nEXECUTE stmt;\nDEALLOCATE PREPARE stmt;");

            migrationBuilder.DropColumn(
                name: "group_purchase_destination_type_code",
                table: "배차_대기");

            migrationBuilder.DropColumn(
                name: "group_purchase_driver_unit_distribution",
                table: "배차_대기");

            migrationBuilder.DropColumn(
                name: "group_purchase_unit_distribution_mode_code",
                table: "배차_대기");

            migrationBuilder.DropColumn(
                name: "group_purchase_unit_delivery_count",
                table: "배차_대기");

            migrationBuilder.DropColumn(
                name: "group_purchase_distribution_responsibility_code",
                table: "배차_대기");

            migrationBuilder.DropIndex(
                name: "IX_platform_community_posts_WorkflowTag_RoleTag_IsDeleted_Creat~",
                table: "platform_community_posts");

            migrationBuilder.DropColumn(
                name: "RoleTag",
                table: "platform_community_posts");

            migrationBuilder.DropColumn(
                name: "WorkflowTag",
                table: "platform_community_posts");

            migrationBuilder.DropTable(
                name: "platform_community_board_requests");

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

            migrationBuilder.DropIndex(
                name: "IX_입고요청_입고흐름유형_자동생성여부",
                table: "입고요청");

            migrationBuilder.DropColumn(
                name: "계약선행여부",
                table: "입고요청");

            migrationBuilder.DropColumn(
                name: "입고생성경로",
                table: "입고요청");

            migrationBuilder.DropColumn(
                name: "입고흐름유형",
                table: "입고요청");

            migrationBuilder.DropColumn(
                name: "자동생성여부",
                table: "입고요청");

            migrationBuilder.DropIndex(
                name: "IX_platform_community_posts_IsReportBoardPost_IsDeleted_Created~",
                table: "platform_community_posts");

            migrationBuilder.DropColumn(
                name: "IsReportBoardPost",
                table: "platform_community_posts");

            migrationBuilder.DropColumn(
                name: "ReportedDisplayName",
                table: "platform_community_posts");

            migrationBuilder.DropColumn(
                name: "ReporterDisplayName",
                table: "platform_community_posts");

            migrationBuilder.DropTable(
                name: "work_relationship_snapshots");

            migrationBuilder.DropIndex(
                name: "IX_post_comments_visible_created",
                table: "platform_community_post_comments");

            migrationBuilder.DropIndex(
                name: "IX_attachment_comments_visible_created",
                table: "platform_community_post_attachment_comments");

            migrationBuilder.DropColumn(
                name: "IsOperatorHidden",
                table: "platform_community_post_comments");

            migrationBuilder.DropColumn(
                name: "ReportCount",
                table: "platform_community_post_comments");

            migrationBuilder.DropColumn(
                name: "IsOperatorHidden",
                table: "platform_community_post_attachment_comments");

            migrationBuilder.DropColumn(
                name: "ReportCount",
                table: "platform_community_post_attachment_comments");

            migrationBuilder.DropTable(
                name: "platform_community_post_attachment_comments");

            migrationBuilder.DropColumn(
                name: "CommentCount",
                table: "platform_community_post_attachments");

            migrationBuilder.DropTable(
                name: "platform_community_post_comments");

            migrationBuilder.DropTable(
                name: "platform_community_post_recommendations");

            migrationBuilder.DropIndex(
                name: "IX_platform_community_posts_AppKey_IsDeleted_IsOperatorPinned_O~",
                table: "platform_community_posts");

            migrationBuilder.DropColumn(
                name: "CommentCount",
                table: "platform_community_posts");

            migrationBuilder.DropColumn(
                name: "IsOperatorPinned",
                table: "platform_community_posts");

            migrationBuilder.DropColumn(
                name: "LastEngagedAtUtc",
                table: "platform_community_posts");

            migrationBuilder.DropColumn(
                name: "OperatorPinnedAtUtc",
                table: "platform_community_posts");

            migrationBuilder.DropColumn(
                name: "RecommendationCount",
                table: "platform_community_posts");

            migrationBuilder.DropColumn(
                name: "SharedLinkUrl",
                table: "platform_community_posts");

            migrationBuilder.CreateIndex(
                name: "IX_platform_community_posts_AppKey_IsDeleted_CreatedAtUtc",
                table: "platform_community_posts",
                columns: new[] { "AppKey", "IsDeleted", "CreatedAtUtc" });

            migrationBuilder.DropTable(
                name: "platform_community_post_attachments");

            migrationBuilder.DropTable(
                name: "platform_community_posts");

            migrationBuilder.DropTable(
                name: "hs_code_entry_risk_tags");

            migrationBuilder.DropIndex(
                name: "IX_hs_code_entries_CatalogVersionId_BusinessCategory_IsActive",
                table: "hs_code_entries");

            migrationBuilder.DropColumn(
                name: "BusinessCategory",
                table: "hs_code_entries");

            migrationBuilder.DropColumn(
                name: "BusinessCategoryReason",
                table: "hs_code_entries");

            migrationBuilder.DropTable(
                name: "hs_code_classification_cases");

            migrationBuilder.DropTable(
                name: "hs_code_platform_agency_experiences");

            migrationBuilder.DropTable(
                name: "hs_code_entries");

            migrationBuilder.DropTable(
                name: "hs_code_catalog_versions");

            migrationBuilder.DropTable(
                name: "hr_role_assignments");

            migrationBuilder.DropTable(
                name: "platform_profit_return_schedules");

            migrationBuilder.DropTable(
                name: "platform_revenue_entries");

            migrationBuilder.DropTable(
                name: "hr_employment_contracts");

            migrationBuilder.DropTable(
                name: "platform_profit_return_policies");

            migrationBuilder.DropColumn(
                name: "계약메모",
                table: "입고요청");

            migrationBuilder.DropColumn(
                name: "계약번호",
                table: "입고요청");

            migrationBuilder.DropColumn(
                name: "계약상대방명",
                table: "입고요청");

            migrationBuilder.DropColumn(
                name: "계약시작일",
                table: "입고요청");

            migrationBuilder.DropColumn(
                name: "계약유형",
                table: "입고요청");

            migrationBuilder.DropColumn(
                name: "계약종료일",
                table: "입고요청");

            migrationBuilder.DropColumn(
                name: "보관료일단가",
                table: "입고요청");

            migrationBuilder.DropColumn(
                name: "정산방식",
                table: "입고요청");

            migrationBuilder.DropColumn(
                name: "통관필요여부",
                table: "입고요청");

            migrationBuilder.DropColumn(
                name: "판매수수료율",
                table: "입고요청");

            migrationBuilder.DropColumn(
                name: "계약메모",
                table: "입고상품");

            migrationBuilder.DropColumn(
                name: "계약번호",
                table: "입고상품");

            migrationBuilder.DropColumn(
                name: "계약상대방명",
                table: "입고상품");

            migrationBuilder.DropColumn(
                name: "계약시작일",
                table: "입고상품");

            migrationBuilder.DropColumn(
                name: "계약유형",
                table: "입고상품");

            migrationBuilder.DropColumn(
                name: "계약종료일",
                table: "입고상품");

            migrationBuilder.DropColumn(
                name: "보관료일단가",
                table: "입고상품");

            migrationBuilder.DropColumn(
                name: "정산방식",
                table: "입고상품");

            migrationBuilder.DropColumn(
                name: "통관필요여부",
                table: "입고상품");

            migrationBuilder.DropColumn(
                name: "판매수수료율",
                table: "입고상품");

            migrationBuilder.DropColumn(
                name: "물류대행지분류",
                table: "창고");

            migrationBuilder.DropTable(
                name: "상품식별코드맵");

            migrationBuilder.DropTable(
                name: "연락처공개동의");

            migrationBuilder.DropTable(
                name: "인연연결요청");

            migrationBuilder.DropTable(
                name: "감사메시지");

            migrationBuilder.DropTable(
                name: "관세사프로필");

            migrationBuilder.DropTable(
                name: "상품물류자산");

            migrationBuilder.DropTable(
                name: "상품상세이미지생성작업");

            migrationBuilder.DropTable(
                name: "상품판매이미지초안");

            migrationBuilder.DropTable(
                name: "통관수임");

            migrationBuilder.DropTable(
                name: "통관절차");

            migrationBuilder.DropTable(
                name: "통관조회연동");

            migrationBuilder.DropTable(
                name: "살뜰참여자역할");

            migrationBuilder.DropTable(
                name: "살뜰참여자");

            migrationBuilder.DropIndex(
                name: "IX_창고_국가코드",
                table: "창고");

            migrationBuilder.DropColumn(
                name: "국가코드",
                table: "창고");

            migrationBuilder.CreateTable(
                name: "살뜰_혜택정책",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    target_type = table.Column<int>(type: "int", nullable: false),
                    expiry_days = table.Column<int>(type: "int", nullable: true),
                    per_user_limit = table.Column<int>(type: "int", nullable: true),
                    source_type = table.Column<int>(type: "int", nullable: false),
                    monthly_limit = table.Column<int>(type: "int", nullable: true),
                    start_at = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    end_at = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    policy_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    allow_stack = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    point_amount = table.Column<int>(type: "int", nullable: false),
                    max_discount_amount = table.Column<int>(type: "int", nullable: true),
                    discount_amount = table.Column<int>(type: "int", nullable: false),
                    discount_rate = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    benefit_type = table.Column<int>(type: "int", nullable: false),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_살뜰_혜택정책", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "살뜰_오프라인모임",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    benefit_policy_id = table.Column<long>(type: "bigint", nullable: true),
                    meeting_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status = table.Column<int>(type: "int", nullable: false),
                    description = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    start_at = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    place_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    end_at = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_살뜰_오프라인모임", x => x.id);
                    table.ForeignKey(
                        name: "FK_살뜰_오프라인모임_살뜰_혜택정책_benefit_policy_id",
                        column: x => x.benefit_policy_id,
                        principalTable: "살뜰_혜택정책",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "살뜰_혜택자격",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    benefit_policy_id = table.Column<long>(type: "bigint", nullable: false),
                    last_used_at = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    expires_at = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    granted_at = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    user_id = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    usage_count = table.Column<int>(type: "int", nullable: false),
                    source_id = table.Column<long>(type: "bigint", nullable: false),
                    source_type = table.Column<int>(type: "int", nullable: false),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_살뜰_혜택자격", x => x.id);
                    table.ForeignKey(
                        name: "FK_살뜰_혜택자격_살뜰_혜택정책_benefit_policy_id",
                        column: x => x.benefit_policy_id,
                        principalTable: "살뜰_혜택정책",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "살뜰_모임참석",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    meeting_id = table.Column<long>(type: "bigint", nullable: false),
                    admin_memo = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    user_id = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    requested_at = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    attendance_status = table.Column<int>(type: "int", nullable: false),
                    confirmation_method = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    confirmed_at = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_살뜰_모임참석", x => x.id);
                    table.ForeignKey(
                        name: "FK_살뜰_모임참석_살뜰_오프라인모임_meeting_id",
                        column: x => x.meeting_id,
                        principalTable: "살뜰_오프라인모임",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_살뜰_모임참석_meeting_id_user_id",
                table: "살뜰_모임참석",
                columns: new[] { "meeting_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_살뜰_모임참석_user_id_attendance_status_confirmed_at",
                table: "살뜰_모임참석",
                columns: new[] { "user_id", "attendance_status", "confirmed_at" });

            migrationBuilder.CreateIndex(
                name: "IX_살뜰_오프라인모임_benefit_policy_id",
                table: "살뜰_오프라인모임",
                column: "benefit_policy_id");

            migrationBuilder.CreateIndex(
                name: "IX_살뜰_오프라인모임_status_is_active_start_at_end_at",
                table: "살뜰_오프라인모임",
                columns: new[] { "status", "is_active", "start_at", "end_at" });

            migrationBuilder.CreateIndex(
                name: "IX_살뜰_혜택자격_benefit_policy_id",
                table: "살뜰_혜택자격",
                column: "benefit_policy_id");

            migrationBuilder.CreateIndex(
                name: "IX_살뜰_혜택자격_user_id_is_active_expires_at",
                table: "살뜰_혜택자격",
                columns: new[] { "user_id", "is_active", "expires_at" });

            migrationBuilder.CreateIndex(
                name: "IX_살뜰_혜택자격_user_id_source_type_source_id",
                table: "살뜰_혜택자격",
                columns: new[] { "user_id", "source_type", "source_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_살뜰_혜택정책_source_type_target_type_is_active",
                table: "살뜰_혜택정책",
                columns: new[] { "source_type", "target_type", "is_active" });

            migrationBuilder.CreateIndex(
                name: "IX_살뜰_혜택정책_start_at_end_at",
                table: "살뜰_혜택정책",
                columns: new[] { "start_at", "end_at" });

            migrationBuilder.DropTable(
                name: "재고이동");

            migrationBuilder.DropTable(
                name: "출고예정");

            migrationBuilder.DropTable(
                name: "살뜰_모임참석");

            migrationBuilder.DropTable(
                name: "살뜰_혜택자격");

            migrationBuilder.DropTable(
                name: "살뜰_오프라인모임");

            migrationBuilder.DropTable(
                name: "살뜰_혜택정책");

            migrationBuilder.DropIndex(
                name: "IX_창고_소유자_user_id_소유자유형_기본창고여부",
                table: "창고");

            migrationBuilder.DropIndex(
                name: "IX_입고요청_주문_id_주문자_user_id",
                table: "입고요청");

            migrationBuilder.DropIndex(
                name: "IX_입고요청_출고예정_id",
                table: "입고요청");

            migrationBuilder.DropColumn(
                name: "경도",
                table: "창고");

            migrationBuilder.DropColumn(
                name: "기본창고여부",
                table: "창고");

            migrationBuilder.DropColumn(
                name: "소유자유형",
                table: "창고");

            migrationBuilder.DropColumn(
                name: "위도",
                table: "창고");

            migrationBuilder.DropColumn(
                name: "창고유형",
                table: "창고");

            migrationBuilder.DropColumn(
                name: "운송의뢰_id",
                table: "입고요청");

            migrationBuilder.DropColumn(
                name: "주문_id",
                table: "입고요청");

            migrationBuilder.DropColumn(
                name: "주문참조번호",
                table: "입고요청");

            migrationBuilder.DropColumn(
                name: "출고예정_id",
                table: "입고요청");

            migrationBuilder.DropColumn(
                name: "판매자_user_id",
                table: "입고요청");

            migrationBuilder.DropTable(
                name: "결제승인완료_Outbox");

            migrationBuilder.DropTable(
                name: "살뜰_콘텐츠보상지급");

            migrationBuilder.DropTable(
                name: "살뜰_콘텐츠시청세션");

            migrationBuilder.DropTable(
                name: "살뜰_공통콘텐츠");

            migrationBuilder.DropTable(
                name: "살뜰_콘텐츠보상정책");

            migrationBuilder.DropColumn(
                name: "canceled_at",
                table: "결제");

            migrationBuilder.DropColumn(
                name: "common_status",
                table: "결제");

            migrationBuilder.DropColumn(
                name: "currency",
                table: "결제");

            migrationBuilder.DropColumn(
                name: "external_transaction_no",
                table: "결제");

            migrationBuilder.DropColumn(
                name: "order_name",
                table: "결제");

            migrationBuilder.DropColumn(
                name: "provider_type",
                table: "결제");

            migrationBuilder.DropColumn(
                name: "raw_response_json",
                table: "결제");

            migrationBuilder.DropColumn(
                name: "target_id",
                table: "결제");

            migrationBuilder.DropColumn(
                name: "target_type",
                table: "결제");

            migrationBuilder.DropColumn(
                name: "business_type",
                table: "배차_대기");

            migrationBuilder.DropColumn(
                name: "source_request_id",
                table: "배차_대기");

            migrationBuilder.DropColumn(
                name: "source_type",
                table: "배차_대기");

            migrationBuilder.DropTable(
                name: "배차추천_알림_Outbox");

            migrationBuilder.DropTable(
                name: "기사운행탐색");

            migrationBuilder.DropTable(
                name: "기사화주인연집계");

            migrationBuilder.DropTable(
                name: "운송의뢰상품연결");

            migrationBuilder.DropTable(
                name: "운행탐색대상화주");

            migrationBuilder.DropTable(
                name: "운행탐색응답요약");

            migrationBuilder.DropTable(
                name: "입고상품");

            migrationBuilder.DropTable(
                name: "입고요청");

            migrationBuilder.DropTable(
                name: "재고이력");

            migrationBuilder.DropTable(
                name: "창고");

            migrationBuilder.DropTable(
                name: "창고사용자");

            migrationBuilder.DropTable(
                name: "채널출품");

            migrationBuilder.DropTable(
                name: "판매상품");

            migrationBuilder.DropTable(
                name: "판매채널계정");

            migrationBuilder.DropTable(
                name: "사용자_행위_로그");

            migrationBuilder.DropColumn(
                name: "권장최대CBM",
                table: "차량제원");

            migrationBuilder.DropColumn(
                name: "추천사용여부",
                table: "차량제원");

            migrationBuilder.DropColumn(
                name: "추천우선순위",
                table: "차량제원");

            migrationBuilder.DropTable(
                name: "주문자프로필");

            migrationBuilder.DropTable(
                name: "사용자_View_설정");

            migrationBuilder.DropTable(
                name: "플랫폼_View_정책");

            migrationBuilder.DropColumn(
                name: "orderer_user_id",
                table: "shipper_requests");

            migrationBuilder.DropTable(
                name: "Command_알림_Outbox");

            migrationBuilder.DropTable(
                name: "사용자_Command_기능설정");

            migrationBuilder.DropTable(
                name: "차량제원");

            migrationBuilder.DropTable(
                name: "화물요구조건");

            migrationBuilder.DropColumn(
                name: "cargo_height_mm",
                table: "shipper_requests");

            migrationBuilder.DropColumn(
                name: "cargo_length_mm",
                table: "shipper_requests");

            migrationBuilder.DropColumn(
                name: "cargo_pallet_count",
                table: "shipper_requests");

            migrationBuilder.DropColumn(
                name: "cargo_width_mm",
                table: "shipper_requests");

            migrationBuilder.DropColumn(
                name: "cash_receipt_required",
                table: "shipper_requests");

            migrationBuilder.DropColumn(
                name: "cash_settled_at",
                table: "shipper_requests");

            migrationBuilder.DropColumn(
                name: "cash_settlement_memo",
                table: "shipper_requests");

            migrationBuilder.DropColumn(
                name: "collector",
                table: "shipper_requests");

            migrationBuilder.DropColumn(
                name: "evidence_method",
                table: "shipper_requests");

            migrationBuilder.DropColumn(
                name: "receipt_issued_at",
                table: "shipper_requests");

            migrationBuilder.DropColumn(
                name: "receipt_number",
                table: "shipper_requests");

            migrationBuilder.DropColumn(
                name: "settlement_memo",
                table: "shipper_requests");

            migrationBuilder.DropColumn(
                name: "settlement_status",
                table: "shipper_requests");

            migrationBuilder.DropColumn(
                name: "settlement_time",
                table: "shipper_requests");

            migrationBuilder.DropColumn(
                name: "tax_invoice_required",
                table: "shipper_requests");

            migrationBuilder.Sql("DROP TABLE IF EXISTS `shipper_requests`;");

            migrationBuilder.Sql("DROP TABLE IF EXISTS `차량단가`;");

            migrationBuilder.Sql("DROP TABLE IF EXISTS `운임구성`;");

            migrationBuilder.Sql("DROP TABLE IF EXISTS `운송이벤트`;");

            migrationBuilder.Sql("DROP TABLE IF EXISTS `배차_대기`;");

            migrationBuilder.Sql("DROP TABLE IF EXISTS `결제`;");

            migrationBuilder.Sql("DROP TABLE IF EXISTS `driver_location_history`;");

            migrationBuilder.Sql("DROP TABLE IF EXISTS `기사월정산`;");

            migrationBuilder.Sql("DROP TABLE IF EXISTS `용달기사`;");

            migrationBuilder.DropColumn(
                name: "BusinessRegistrationNumber",
                table: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "배차계획신청");

            migrationBuilder.DropTable(
                name: "driver_shifts");

            migrationBuilder.DropTable(
                name: "기사배차");

            migrationBuilder.DropTable(
                name: "배달기사");

            migrationBuilder.DropTable(
                name: "배송_운송");

            migrationBuilder.DropTable(
                name: "배차_최소");

            migrationBuilder.DropTable(
                name: "업체");
        }
    }
}
