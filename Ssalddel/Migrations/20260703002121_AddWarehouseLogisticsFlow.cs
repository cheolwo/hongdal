using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssalddel.Migrations
{
    /// <inheritdoc />
    public partial class AddWarehouseLogisticsFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
        }
    }
}
