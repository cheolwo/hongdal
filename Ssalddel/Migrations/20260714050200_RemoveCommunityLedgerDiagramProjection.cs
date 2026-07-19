using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssalddel.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCommunityLedgerDiagramProjection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "community_ledger_block_relation_projections");

            migrationBuilder.DropTable(
                name: "community_ledger_block_projections");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
        }
    }
}
