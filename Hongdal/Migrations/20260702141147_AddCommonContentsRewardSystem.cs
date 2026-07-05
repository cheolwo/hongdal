using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hongdal.Migrations
{
    /// <inheritdoc />
    public partial class AddCommonContentsRewardSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "홍달_콘텐츠보상정책",
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
                    table.PrimaryKey("PK_홍달_콘텐츠보상정책", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "홍달_콘텐츠보상지급",
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
                    table.PrimaryKey("PK_홍달_콘텐츠보상지급", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "홍달_공통콘텐츠",
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
                    table.PrimaryKey("PK_홍달_공통콘텐츠", x => x.id);
                    table.ForeignKey(
                        name: "FK_홍달_공통콘텐츠_홍달_콘텐츠보상정책_reward_policy_id",
                        column: x => x.reward_policy_id,
                        principalTable: "홍달_콘텐츠보상정책",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "홍달_콘텐츠시청세션",
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
                    table.PrimaryKey("PK_홍달_콘텐츠시청세션", x => x.id);
                    table.ForeignKey(
                        name: "FK_홍달_콘텐츠시청세션_홍달_공통콘텐츠_content_id",
                        column: x => x.content_id,
                        principalTable: "홍달_공통콘텐츠",
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
                name: "IX_홍달_공통콘텐츠_is_active_start_at_end_at",
                table: "홍달_공통콘텐츠",
                columns: new[] { "is_active", "start_at", "end_at" });

            migrationBuilder.CreateIndex(
                name: "IX_홍달_공통콘텐츠_reward_policy_id",
                table: "홍달_공통콘텐츠",
                column: "reward_policy_id");

            migrationBuilder.CreateIndex(
                name: "IX_홍달_콘텐츠보상지급_user_id_content_id",
                table: "홍달_콘텐츠보상지급",
                columns: new[] { "user_id", "content_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_홍달_콘텐츠보상지급_user_id_is_used_in_payment_granted_at",
                table: "홍달_콘텐츠보상지급",
                columns: new[] { "user_id", "is_used_in_payment", "granted_at" });

            migrationBuilder.CreateIndex(
                name: "IX_홍달_콘텐츠시청세션_content_id",
                table: "홍달_콘텐츠시청세션",
                column: "content_id");

            migrationBuilder.CreateIndex(
                name: "IX_홍달_콘텐츠시청세션_user_id_content_id_started_at",
                table: "홍달_콘텐츠시청세션",
                columns: new[] { "user_id", "content_id", "started_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "결제승인완료_Outbox");

            migrationBuilder.DropTable(
                name: "홍달_콘텐츠보상지급");

            migrationBuilder.DropTable(
                name: "홍달_콘텐츠시청세션");

            migrationBuilder.DropTable(
                name: "홍달_공통콘텐츠");

            migrationBuilder.DropTable(
                name: "홍달_콘텐츠보상정책");

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
        }
    }
}
