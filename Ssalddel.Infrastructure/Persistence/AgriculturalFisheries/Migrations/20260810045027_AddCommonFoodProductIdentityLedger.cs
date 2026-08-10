using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Ssalddel.Infrastructure.Persistence.AgriculturalFisheries.Migrations
{
    /// <inheritdoc />
    public partial class AddCommonFoodProductIdentityLedger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "agri_common_food_product_identities",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CanonicalProductStableId = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DisplayName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Revision = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agri_common_food_product_identities", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "agri_common_food_product_code_relations",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ProductIdentityId = table.Column<long>(type: "bigint", nullable: false),
                    RelationStableId = table.Column<string>(type: "varchar(180)", maxLength: 180, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SourceKey = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CodeScheme = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ExternalCode = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ParentCode = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Label = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RelationStatusCode = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MatchQualityCode = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EvidenceNote = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Revision = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agri_common_food_product_code_relations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_agri_common_food_product_code_relations_agri_common_food_pro~",
                        column: x => x.ProductIdentityId,
                        principalTable: "agri_common_food_product_identities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "agri_common_food_product_code_relation_reviews",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CodeRelationId = table.Column<long>(type: "bigint", nullable: false),
                    Revision = table.Column<int>(type: "int", nullable: false),
                    RelationStatusCode = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ExternalCode = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReviewActionCode = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReviewReason = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReviewedBySubjectId = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReviewedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agri_common_food_product_code_relation_reviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_agri_common_food_product_code_relation_reviews_agri_common_f~",
                        column: x => x.CodeRelationId,
                        principalTable: "agri_common_food_product_code_relations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "agri_common_food_product_identities",
                columns: new[] { "Id", "CanonicalProductStableId", "CreatedAtUtc", "DisplayName", "IsActive", "Revision", "UpdatedAtUtc" },
                values: new object[] { 1L, "product:potato", new DateTime(2026, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "감자", true, "common-food-product-identity.v1", new DateTime(2026, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.InsertData(
                table: "agri_common_food_product_code_relations",
                columns: new[] { "Id", "CodeScheme", "CreatedAtUtc", "EvidenceNote", "ExternalCode", "IsActive", "Label", "MatchQualityCode", "ParentCode", "ProductIdentityId", "RelationStableId", "RelationStatusCode", "Revision", "SourceKey", "UpdatedAtUtc" },
                values: new object[,]
                {
                    { 1L, "KAMIS_ITEM", new DateTime(2026, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "KAMIS 식량작물 100의 품목코드 152로 저장·조회되는 관계입니다.", "152", true, "감자", "SourceCodeConfirmed", "100", 1L, "relation:product:potato:kamis", "Confirmed", 1, "kamis", new DateTime(2026, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2L, "HS4", new DateTime(2026, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "국제 HS 4단위 후보이며 종자용 여부·가공도·용도에 따라 국가 세번이 달라질 수 있습니다.", "0701", true, "감자", "ExactCommodityCandidate", null, 1L, "relation:product:potato:hs4", "Candidate", 1, "wco-hs", new DateTime(2026, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 3L, "USDA_AMS_COMMODITY", new DateTime(2026, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "식용 감자 공통 품목 후보이며 종서용 감자와 품종·등급·시장 단계는 별도로 검토합니다.", "Potatoes", true, "Potatoes", "DirectCommodityCandidate", null, 1L, "relation:product:potato:usda-ams", "Candidate", 1, "usda-ams-market-news", new DateTime(2026, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 4L, "NONGSARO_KIND_OF_COMMODITY", new DateTime(2026, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "농사로 공식 품목구분Code를 현재 근거에서 확인하지 못해 이름으로 연결하지 않습니다.", null, true, "농사로 감자 품목구분", "OfficialCodeRequired", null, 1L, "relation:product:potato:nongsaro", "Unlinked", 1, "nongsaro:farm-working-plan-new", new DateTime(2026, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "agri_common_food_product_code_relation_reviews",
                columns: new[] { "Id", "CodeRelationId", "ExternalCode", "RelationStatusCode", "ReviewActionCode", "ReviewReason", "ReviewedAtUtc", "ReviewedBySubjectId", "Revision" },
                values: new object[,]
                {
                    { 1L, 1L, "152", "Confirmed", "Initialized", "공식 source code 초기 등록", new DateTime(2026, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", 1 },
                    { 2L, 2L, "0701", "Candidate", "Initialized", "국제 HS 후보 초기 등록", new DateTime(2026, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", 1 },
                    { 3L, 3L, "Potatoes", "Candidate", "Initialized", "USDA AMS Commodity 후보 초기 등록", new DateTime(2026, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", 1 },
                    { 4L, 4L, null, "Unlinked", "Initialized", "공식 농사로 품목구분Code 확인 전 미연결 등록", new DateTime(2026, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", 1 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_agri_common_food_product_code_relation_reviews_CodeRelationI~",
                table: "agri_common_food_product_code_relation_reviews",
                columns: new[] { "CodeRelationId", "Revision" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_agri_common_food_product_code_relation_reviews_ReviewedAtUtc",
                table: "agri_common_food_product_code_relation_reviews",
                column: "ReviewedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_agri_common_food_product_code_relations_ProductIdentityId_So~",
                table: "agri_common_food_product_code_relations",
                columns: new[] { "ProductIdentityId", "SourceKey", "CodeScheme" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_agri_common_food_product_code_relations_RelationStableId",
                table: "agri_common_food_product_code_relations",
                column: "RelationStableId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_agri_common_food_product_code_relations_SourceKey_CodeScheme~",
                table: "agri_common_food_product_code_relations",
                columns: new[] { "SourceKey", "CodeScheme", "ExternalCode", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_agri_common_food_product_identities_CanonicalProductStableId",
                table: "agri_common_food_product_identities",
                column: "CanonicalProductStableId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_agri_common_food_product_identities_IsActive_DisplayName",
                table: "agri_common_food_product_identities",
                columns: new[] { "IsActive", "DisplayName" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "agri_common_food_product_code_relation_reviews");

            migrationBuilder.DropTable(
                name: "agri_common_food_product_code_relations");

            migrationBuilder.DropTable(
                name: "agri_common_food_product_identities");
        }
    }
}
