using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;
using 살뜰.Data;

#nullable disable

namespace Ssalddel.Migrations;

[DbContext(typeof(SsalddelContext))]
[Migration("20260714235500_AddHongikHakdangCardDeliveryFoundation")]
public sealed class AddHongikHakdangCardDeliveryFoundation : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
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
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "hongik_hakdang_card_delivery_outbox");
        migrationBuilder.DropTable(name: "hongik_hakdang_card_delivery_preferences");
        migrationBuilder.DropTable(name: "hongik_hakdang_card_image_variants");
        migrationBuilder.DropTable(name: "hongik_hakdang_daily_card_selections");
        migrationBuilder.DropTable(name: "ssalddel_mobile_push_installations");
    }
}
