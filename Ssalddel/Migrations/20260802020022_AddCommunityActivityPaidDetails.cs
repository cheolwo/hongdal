using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssalddel.Migrations
{
    /// <inheritdoc />
    public partial class AddCommunityActivityPaidDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "community_activity_paid_details",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    detail_id = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    post_id = table.Column<long>(type: "bigint", nullable: false),
                    seller_user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    public_preview = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    detail_content = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    price_amount = table.Column<int>(type: "int", nullable: false),
                    currency_code = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    sale_status = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_community_activity_paid_details", x => x.Id);
                    table.UniqueConstraint("AK_community_activity_paid_details_detail_id", x => x.detail_id);
                    table.ForeignKey(
                        name: "FK_community_activity_paid_details_platform_community_posts_pos~",
                        column: x => x.post_id,
                        principalTable: "platform_community_posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "community_activity_detail_entitlements",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    entitlement_id = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    detail_id = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    buyer_user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    payment_id = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    granted_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    revoked_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_community_activity_detail_entitlements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_community_activity_detail_entitlements_community_activity_pa~",
                        column: x => x.detail_id,
                        principalTable: "community_activity_paid_details",
                        principalColumn: "detail_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_community_activity_detail_entitlements_detail_id_buyer_user_~",
                table: "community_activity_detail_entitlements",
                columns: new[] { "detail_id", "buyer_user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_community_activity_detail_entitlements_entitlement_id",
                table: "community_activity_detail_entitlements",
                column: "entitlement_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_community_activity_detail_entitlements_payment_id",
                table: "community_activity_detail_entitlements",
                column: "payment_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_community_activity_paid_details_detail_id",
                table: "community_activity_paid_details",
                column: "detail_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_community_activity_paid_details_post_id",
                table: "community_activity_paid_details",
                column: "post_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_community_activity_paid_details_seller_user_id_sale_status_c~",
                table: "community_activity_paid_details",
                columns: new[] { "seller_user_id", "sale_status", "created_at_utc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "community_activity_detail_entitlements");

            migrationBuilder.DropTable(
                name: "community_activity_paid_details");
        }
    }
}
