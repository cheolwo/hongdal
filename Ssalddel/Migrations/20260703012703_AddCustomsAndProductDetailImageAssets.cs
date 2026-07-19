using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssalddel.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomsAndProductDetailImageAssets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "살뜰_모임참석");

            migrationBuilder.DropTable(
                name: "살뜰_혜택자격");

            migrationBuilder.DropTable(
                name: "살뜰_오프라인모임");

            migrationBuilder.DropTable(
                name: "살뜰_혜택정책");

            migrationBuilder.AddColumn<string>(
                name: "국가코드",
                table: "창고",
                type: "varchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "관세사프로필",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    참여자_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    사무소명 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    관세사등록번호 = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    담당지역 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    전문품목메모 = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    수입전문여부 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    수출전문여부 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    수임가능여부 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    관리자승인여부 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_관세사프로필", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "상품물류자산",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    상품_id = table.Column<long>(type: "bigint", nullable: false),
                    주문_id = table.Column<long>(type: "bigint", nullable: true),
                    통관절차_id = table.Column<long>(type: "bigint", nullable: true),
                    자산유형 = table.Column<int>(type: "int", nullable: false),
                    파일url = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    설명 = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    등록자_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    상세이미지사용가능여부 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    등록시각 = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_상품물류자산", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "상품상세이미지생성작업",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    상품_id = table.Column<long>(type: "bigint", nullable: false),
                    주문_id = table.Column<long>(type: "bigint", nullable: true),
                    통관절차_id = table.Column<long>(type: "bigint", nullable: true),
                    요청자_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    상태 = table.Column<int>(type: "int", nullable: false),
                    생성프롬프트 = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    원본자산참조json = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    오류내용 = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    관련생성이미지작업_id = table.Column<long>(type: "bigint", nullable: true),
                    생성시각 = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    완료시각 = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_상품상세이미지생성작업", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "상품판매이미지초안",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    상품_id = table.Column<long>(type: "bigint", nullable: false),
                    생성작업_id = table.Column<long>(type: "bigint", nullable: false),
                    작성자_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    대표이미지url = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    이미지목록json = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    원본자산참조json = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    생성근거요약 = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    판매채널전송가능여부 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    생성시각 = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_상품판매이미지초안", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "통관수임",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    통관절차_id = table.Column<long>(type: "bigint", nullable: false),
                    관세사_참여자_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    상태 = table.Column<int>(type: "int", nullable: false),
                    요청시각 = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    확정시각 = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    메모 = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_통관수임", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "통관절차",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    주문_id = table.Column<long>(type: "bigint", nullable: true),
                    주문참조번호 = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    출고예정_id = table.Column<long>(type: "bigint", nullable: true),
                    입고요청_id = table.Column<long>(type: "bigint", nullable: true),
                    출고창고_id = table.Column<long>(type: "bigint", nullable: false),
                    입고창고_id = table.Column<long>(type: "bigint", nullable: false),
                    물류거래방향 = table.Column<int>(type: "int", nullable: false),
                    대표상품명 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    상태 = table.Column<int>(type: "int", nullable: false),
                    확정관세사_참여자_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    메모 = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_통관절차", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "통관조회연동",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    주문_id = table.Column<long>(type: "bigint", nullable: false),
                    사용자_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    통관절차_id = table.Column<long>(type: "bigint", nullable: false),
                    개인통관고유부호_암호문 = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    화물관리번호 = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    master_bl = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    house_bl = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    사용자조회동의여부 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    동의시각 = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    연동상태 = table.Column<int>(type: "int", nullable: false),
                    마지막진행단계 = table.Column<int>(type: "int", nullable: false),
                    마지막조회시각 = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    마지막오류 = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_통관조회연동", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "살뜰참여자",
                columns: table => new
                {
                    id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    표시이름 = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    가입시각 = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    활성화여부 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_살뜰참여자", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "살뜰참여자역할",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    참여자_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    역할유형 = table.Column<int>(type: "int", nullable: false),
                    활성화여부 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    부여시각 = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_살뜰참여자역할", x => x.id);
                    table.ForeignKey(
                        name: "FK_살뜰참여자역할_살뜰참여자_참여자_id",
                        column: x => x.참여자_id,
                        principalTable: "살뜰참여자",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_창고_국가코드",
                table: "창고",
                column: "국가코드");

            migrationBuilder.CreateIndex(
                name: "IX_관세사프로필_관리자승인여부_수임가능여부",
                table: "관세사프로필",
                columns: new[] { "관리자승인여부", "수임가능여부" });

            migrationBuilder.CreateIndex(
                name: "IX_관세사프로필_참여자_id",
                table: "관세사프로필",
                column: "참여자_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_상품물류자산_상품_id_자산유형_등록시각",
                table: "상품물류자산",
                columns: new[] { "상품_id", "자산유형", "등록시각" });

            migrationBuilder.CreateIndex(
                name: "IX_상품물류자산_주문_id_통관절차_id",
                table: "상품물류자산",
                columns: new[] { "주문_id", "통관절차_id" });

            migrationBuilder.CreateIndex(
                name: "IX_상품상세이미지생성작업_관련생성이미지작업_id",
                table: "상품상세이미지생성작업",
                column: "관련생성이미지작업_id");

            migrationBuilder.CreateIndex(
                name: "IX_상품상세이미지생성작업_상품_id_상태_생성시각",
                table: "상품상세이미지생성작업",
                columns: new[] { "상품_id", "상태", "생성시각" });

            migrationBuilder.CreateIndex(
                name: "IX_상품판매이미지초안_상품_id_생성시각",
                table: "상품판매이미지초안",
                columns: new[] { "상품_id", "생성시각" });

            migrationBuilder.CreateIndex(
                name: "IX_상품판매이미지초안_생성작업_id",
                table: "상품판매이미지초안",
                column: "생성작업_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_통관수임_통관절차_id_관세사_참여자_id_상태",
                table: "통관수임",
                columns: new[] { "통관절차_id", "관세사_참여자_id", "상태" });

            migrationBuilder.CreateIndex(
                name: "IX_통관절차_주문_id_주문참조번호_상태",
                table: "통관절차",
                columns: new[] { "주문_id", "주문참조번호", "상태" });

            migrationBuilder.CreateIndex(
                name: "IX_통관절차_출고예정_id_입고요청_id",
                table: "통관절차",
                columns: new[] { "출고예정_id", "입고요청_id" });

            migrationBuilder.CreateIndex(
                name: "IX_통관조회연동_주문_id_사용자_id",
                table: "통관조회연동",
                columns: new[] { "주문_id", "사용자_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_통관조회연동_통관절차_id_연동상태_마지막조회시각",
                table: "통관조회연동",
                columns: new[] { "통관절차_id", "연동상태", "마지막조회시각" });

            migrationBuilder.CreateIndex(
                name: "IX_통관조회연동_화물관리번호_master_bl_house_bl",
                table: "통관조회연동",
                columns: new[] { "화물관리번호", "master_bl", "house_bl" });

            migrationBuilder.CreateIndex(
                name: "IX_살뜰참여자_활성화여부",
                table: "살뜰참여자",
                column: "활성화여부");

            migrationBuilder.CreateIndex(
                name: "IX_살뜰참여자역할_참여자_id_역할유형_활성화여부",
                table: "살뜰참여자역할",
                columns: new[] { "참여자_id", "역할유형", "활성화여부" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "관세사프로필");

            migrationBuilder.DropTable(
                name: "상품물류자산");

            migrationBuilder.DropTable(
                name: "상품상세이미지생성작업");

            migrationBuilder.DropTable(
                name: "상품판매이미지초안");

            migrationBuilder.DropTable(
                name: "통관수임");

            migrationBuilder.DropTable(
                name: "통관절차");

            migrationBuilder.DropTable(
                name: "통관조회연동");

            migrationBuilder.DropTable(
                name: "살뜰참여자역할");

            migrationBuilder.DropTable(
                name: "살뜰참여자");

            migrationBuilder.DropIndex(
                name: "IX_창고_국가코드",
                table: "창고");

            migrationBuilder.DropColumn(
                name: "국가코드",
                table: "창고");

            migrationBuilder.CreateTable(
                name: "살뜰_혜택정책",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    target_type = table.Column<int>(type: "int", nullable: false),
                    expiry_days = table.Column<int>(type: "int", nullable: true),
                    per_user_limit = table.Column<int>(type: "int", nullable: true),
                    source_type = table.Column<int>(type: "int", nullable: false),
                    monthly_limit = table.Column<int>(type: "int", nullable: true),
                    start_at = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    end_at = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    policy_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    allow_stack = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    point_amount = table.Column<int>(type: "int", nullable: false),
                    max_discount_amount = table.Column<int>(type: "int", nullable: true),
                    discount_amount = table.Column<int>(type: "int", nullable: false),
                    discount_rate = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    benefit_type = table.Column<int>(type: "int", nullable: false),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false)
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
                    benefit_policy_id = table.Column<long>(type: "bigint", nullable: true),
                    meeting_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status = table.Column<int>(type: "int", nullable: false),
                    description = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    start_at = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    place_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    end_at = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
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
                    benefit_policy_id = table.Column<long>(type: "bigint", nullable: false),
                    last_used_at = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    expires_at = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    granted_at = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    user_id = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    usage_count = table.Column<int>(type: "int", nullable: false),
                    source_id = table.Column<long>(type: "bigint", nullable: false),
                    source_type = table.Column<int>(type: "int", nullable: false),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false)
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
                    admin_memo = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    user_id = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    requested_at = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    attendance_status = table.Column<int>(type: "int", nullable: false),
                    confirmation_method = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    confirmed_at = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true)
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
    }
}
