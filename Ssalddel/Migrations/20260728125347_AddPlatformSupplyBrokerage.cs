using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssalddel.Migrations
{
    /// <inheritdoc />
    public partial class AddPlatformSupplyBrokerage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "플랫폼공급조건계약",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    client_request_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    contract_number = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    supplier_key = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    supplier_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    contract_document_version = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status_code = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    effective_from_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    effective_until_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    currency_code = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    settlement_terms = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    return_terms = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    platform_role_code = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    platform_is_seller = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    platform_is_reseller = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    contract_evidence_reference = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_by_user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    activated_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    updated_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_플랫폼공급조건계약", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "공급계약이용등록",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    client_request_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    supply_agreement_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    organization_type_code = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    organization_reference_key = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    operator_user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    contract_document_version = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status_code = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    agreement_use_consent = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    separate_order_confirmation_consent = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    guidance_version = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_공급계약이용등록", x => x.id);
                    table.ForeignKey(
                        name: "FK_공급계약이용등록_플랫폼공급조건계약_supply_agreement_id",
                        column: x => x.supply_agreement_id,
                        principalTable: "플랫폼공급조건계약",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "플랫폼공급조건계약품목",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    supply_agreement_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    contract_item_key = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    sku = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    item_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    supply_unit = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    contract_unit_price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    minimum_order_quantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    maximum_order_quantity = table.Column<decimal>(type: "decimal(18,3)", nullable: true),
                    origin_label = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    storage_condition = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    allowed_organization_types = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_플랫폼공급조건계약품목", x => x.id);
                    table.ForeignKey(
                        name: "FK_플랫폼공급조건계약품목_플랫폼공급조건계약_supply_agreement_id",
                        column: x => x.supply_agreement_id,
                        principalTable: "플랫폼공급조건계약",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "조직개별공급발주",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    client_request_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    agreement_participation_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    supply_agreement_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    supply_agreement_item_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    buyer_organization_type_code = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    buyer_organization_reference_key = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    requested_by_user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    contract_number_snapshot = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    contract_document_version_snapshot = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    supplier_key_snapshot = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    supplier_name_snapshot = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    item_name_snapshot = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    sku_snapshot = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    supply_unit_snapshot = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    order_quantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    supplier_accepted_quantity = table.Column<decimal>(type: "decimal(18,3)", nullable: true),
                    contract_unit_price_snapshot = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    order_amount_snapshot = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    currency_code_snapshot = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    requested_delivery_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    delivery_destination_reference_key = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status_code = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    platform_role_code = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    platform_is_seller = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    platform_is_reseller = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    payment_executed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    inventory_reserved = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    inbound_created = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    individual_order_confirmed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    supplier_is_seller_confirmed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    platform_is_broker_confirmed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    guidance_version = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    supplier_response_evidence_reference = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    supplier_response_recorded_by_user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    submitted_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    supplier_responded_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    updated_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_조직개별공급발주", x => x.id);
                    table.ForeignKey(
                        name: "FK_조직개별공급발주_공급계약이용등록_agreement_participation_id",
                        column: x => x.agreement_participation_id,
                        principalTable: "공급계약이용등록",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_조직개별공급발주_플랫폼공급조건계약_supply_agreement_id",
                        column: x => x.supply_agreement_id,
                        principalTable: "플랫폼공급조건계약",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_조직개별공급발주_플랫폼공급조건계약품목_supply_agreement_item_id",
                        column: x => x.supply_agreement_item_id,
                        principalTable: "플랫폼공급조건계약품목",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_공급계약이용등록_organization_type_code_organization_reference_key_c~",
                table: "공급계약이용등록",
                columns: new[] { "organization_type_code", "organization_reference_key", "client_request_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_공급계약이용등록_supply_agreement_id_organization_type_code_organiza~",
                table: "공급계약이용등록",
                columns: new[] { "supply_agreement_id", "organization_type_code", "organization_reference_key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_조직개별공급발주_agreement_participation_id",
                table: "조직개별공급발주",
                column: "agreement_participation_id");

            migrationBuilder.CreateIndex(
                name: "IX_조직개별공급발주_buyer_organization_type_code_buyer_organization_re~1",
                table: "조직개별공급발주",
                columns: new[] { "buyer_organization_type_code", "buyer_organization_reference_key", "status_code", "submitted_at_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_조직개별공급발주_buyer_organization_type_code_buyer_organization_ref~",
                table: "조직개별공급발주",
                columns: new[] { "buyer_organization_type_code", "buyer_organization_reference_key", "client_request_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_조직개별공급발주_supply_agreement_id",
                table: "조직개별공급발주",
                column: "supply_agreement_id");

            migrationBuilder.CreateIndex(
                name: "IX_조직개별공급발주_supply_agreement_item_id",
                table: "조직개별공급발주",
                column: "supply_agreement_item_id");

            migrationBuilder.CreateIndex(
                name: "IX_플랫폼공급조건계약_contract_number",
                table: "플랫폼공급조건계약",
                column: "contract_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_플랫폼공급조건계약_created_by_user_id_client_request_id",
                table: "플랫폼공급조건계약",
                columns: new[] { "created_by_user_id", "client_request_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_플랫폼공급조건계약_status_code_effective_from_utc_effective_until_utc",
                table: "플랫폼공급조건계약",
                columns: new[] { "status_code", "effective_from_utc", "effective_until_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_플랫폼공급조건계약품목_supply_agreement_id_contract_item_key",
                table: "플랫폼공급조건계약품목",
                columns: new[] { "supply_agreement_id", "contract_item_key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_플랫폼공급조건계약품목_supply_agreement_id_sku",
                table: "플랫폼공급조건계약품목",
                columns: new[] { "supply_agreement_id", "sku" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "조직개별공급발주");

            migrationBuilder.DropTable(
                name: "공급계약이용등록");

            migrationBuilder.DropTable(
                name: "플랫폼공급조건계약품목");

            migrationBuilder.DropTable(
                name: "플랫폼공급조건계약");
        }
    }
}
