using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;
using 살뜰.Data;

#nullable disable

namespace Ssalddel.Migrations;

[DbContext(typeof(SsalddelContext))]
[Migration("20260714143000_AddHongikHakdangCards")]
public sealed class AddHongikHakdangCards : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
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
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "hongik_hakdang_card_collection_items");
        migrationBuilder.DropTable(name: "hongik_hakdang_card_collections");
        migrationBuilder.DropTable(name: "hongik_hakdang_cards");
    }
}
