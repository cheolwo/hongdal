using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssalddel.Migrations
{
    /// <inheritdoc />
    public partial class AddCommunityActivityPaidDetailPurchaseWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "community_activity_detail_purchases",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    purchase_id = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    detail_id = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    buyer_user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    seller_user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    idempotency_key = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    requested_amount = table.Column<int>(type: "int", nullable: false),
                    currency_code = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    current_status = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    payment_id = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    entitlement_id = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    requested_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    completed_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_community_activity_detail_purchases", x => x.Id);
                    table.UniqueConstraint("AK_community_activity_detail_purchases_purchase_id", x => x.purchase_id);
                    table.ForeignKey(
                        name: "FK_community_activity_detail_purchases_community_activity_paid_~",
                        column: x => x.detail_id,
                        principalTable: "community_activity_paid_details",
                        principalColumn: "detail_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "community_activity_detail_purchase_status_history",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    purchase_id = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    sequence = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    reason_code = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    recorded_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_community_activity_detail_purchase_status_history", x => x.Id);
                    table.ForeignKey(
                        name: "FK_community_activity_detail_purchase_status_history_community_~",
                        column: x => x.purchase_id,
                        principalTable: "community_activity_detail_purchases",
                        principalColumn: "purchase_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_community_activity_detail_purchase_status_history_purchase_i~",
                table: "community_activity_detail_purchase_status_history",
                columns: new[] { "purchase_id", "sequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_community_activity_detail_purchases_detail_id_buyer_user_id",
                table: "community_activity_detail_purchases",
                columns: new[] { "detail_id", "buyer_user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_community_activity_detail_purchases_entitlement_id",
                table: "community_activity_detail_purchases",
                column: "entitlement_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_community_activity_detail_purchases_idempotency_key",
                table: "community_activity_detail_purchases",
                column: "idempotency_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_community_activity_detail_purchases_payment_id",
                table: "community_activity_detail_purchases",
                column: "payment_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_community_activity_detail_purchases_purchase_id",
                table: "community_activity_detail_purchases",
                column: "purchase_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "community_activity_detail_purchase_status_history");

            migrationBuilder.DropTable(
                name: "community_activity_detail_purchases");
        }
    }
}
