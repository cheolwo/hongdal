using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssalddel.Migrations
{
    /// <inheritdoc />
    public partial class AddRegionalAgriculturalMapGeography : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "regional_agricultural_map_regions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    public_region_key = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    country_code = table.Column<string>(type: "varchar(2)", maxLength: 2, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    region_type_code = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    parent_region_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    display_name_ko = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    display_name_en = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    display_name_local = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    valid_from = table.Column<DateOnly>(type: "date", nullable: true),
                    valid_to = table.Column<DateOnly>(type: "date", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_regional_agricultural_map_regions", x => x.id);
                    table.ForeignKey(
                        name: "FK_regional_agricultural_map_regions_regional_agricultural_map_~",
                        column: x => x.parent_region_id,
                        principalTable: "regional_agricultural_map_regions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "regional_agricultural_map_region_boundaries",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    region_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    boundary_source_code = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    boundary_vintage = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    geometry_reference = table.Column<string>(type: "varchar(1000)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    anchor_latitude = table.Column<decimal>(type: "decimal(10,7)", nullable: false),
                    anchor_longitude = table.Column<decimal>(type: "decimal(10,7)", nullable: false),
                    simplification_level = table.Column<int>(type: "int", nullable: false),
                    source_url = table.Column<string>(type: "varchar(1000)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    verified_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_regional_agricultural_map_region_boundaries", x => x.id);
                    table.ForeignKey(
                        name: "FK_regional_agricultural_map_region_boundaries_regional_agricul~",
                        column: x => x.region_id,
                        principalTable: "regional_agricultural_map_regions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "regional_agricultural_map_region_codes",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    region_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    scheme_code = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    external_code = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    source_vintage = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    valid_from = table.Column<DateOnly>(type: "date", nullable: true),
                    valid_to = table.Column<DateOnly>(type: "date", nullable: true),
                    source_url = table.Column<string>(type: "varchar(1000)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    verified_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_regional_agricultural_map_region_codes", x => x.id);
                    table.ForeignKey(
                        name: "FK_regional_agricultural_map_region_codes_regional_agricultural~",
                        column: x => x.region_id,
                        principalTable: "regional_agricultural_map_regions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "regional_agricultural_map_region_crosswalks",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    source_scheme_code = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    source_code = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    source_name_raw = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    source_vintage = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    target_region_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    match_method_code = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    confidence_code = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    valid_from = table.Column<DateOnly>(type: "date", nullable: true),
                    valid_to = table.Column<DateOnly>(type: "date", nullable: true),
                    reviewed_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    evidence_url = table.Column<string>(type: "varchar(1000)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_regional_agricultural_map_region_crosswalks", x => x.id);
                    table.ForeignKey(
                        name: "FK_regional_agricultural_map_region_crosswalks_regional_agricul~",
                        column: x => x.target_region_id,
                        principalTable: "regional_agricultural_map_regions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_regional_agricultural_map_region_boundaries_region_id_bounda~",
                table: "regional_agricultural_map_region_boundaries",
                columns: new[] { "region_id", "boundary_source_code", "boundary_vintage", "simplification_level" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_regional_agricultural_map_region_codes_region_id",
                table: "regional_agricultural_map_region_codes",
                column: "region_id");

            migrationBuilder.CreateIndex(
                name: "IX_regional_agricultural_map_region_codes_scheme_code_external_~",
                table: "regional_agricultural_map_region_codes",
                columns: new[] { "scheme_code", "external_code" });

            migrationBuilder.CreateIndex(
                name: "IX_regional_agricultural_map_region_codes_scheme_code_external~1",
                table: "regional_agricultural_map_region_codes",
                columns: new[] { "scheme_code", "external_code", "source_vintage" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_regional_agricultural_map_region_crosswalks_source_scheme_c~1",
                table: "regional_agricultural_map_region_crosswalks",
                columns: new[] { "source_scheme_code", "source_code", "source_vintage" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_regional_agricultural_map_region_crosswalks_source_scheme_co~",
                table: "regional_agricultural_map_region_crosswalks",
                columns: new[] { "source_scheme_code", "source_code" });

            migrationBuilder.CreateIndex(
                name: "IX_regional_agricultural_map_region_crosswalks_target_region_id",
                table: "regional_agricultural_map_region_crosswalks",
                column: "target_region_id");

            migrationBuilder.CreateIndex(
                name: "IX_regional_agricultural_map_regions_country_code_region_type_c~",
                table: "regional_agricultural_map_regions",
                columns: new[] { "country_code", "region_type_code" });

            migrationBuilder.CreateIndex(
                name: "IX_regional_agricultural_map_regions_parent_region_id",
                table: "regional_agricultural_map_regions",
                column: "parent_region_id");

            migrationBuilder.CreateIndex(
                name: "IX_regional_agricultural_map_regions_public_region_key",
                table: "regional_agricultural_map_regions",
                column: "public_region_key",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "regional_agricultural_map_region_boundaries");

            migrationBuilder.DropTable(
                name: "regional_agricultural_map_region_codes");

            migrationBuilder.DropTable(
                name: "regional_agricultural_map_region_crosswalks");

            migrationBuilder.DropTable(
                name: "regional_agricultural_map_regions");
        }
    }
}
