using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssalddel.Simulation.Persistence.Migrations.WorldDerivation
{
    /// <inheritdoc />
    public partial class 시뮬레이션월드공간건물그래픽원장추가 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "시뮬레이션월드_파생실행",
                columns: table => new
                {
                    식별번호 = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    스키마버전 = table.Column<int>(type: "int", nullable: false),
                    파생실행고유식별자 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    영역묶음고유식별자 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    생성조리법개정번호 = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    관계규칙개정번호 = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    시각자산대장개정번호 = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    배치시드 = table.Column<int>(type: "int", nullable: false),
                    입력지문SHA256 = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    출력해시SHA256 = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    생성시각UTC = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    저장시각UTC = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_시뮬레이션월드_파생실행", x => x.식별번호);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "시뮬레이션월드_건물배치계획",
                columns: table => new
                {
                    식별번호 = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    파생실행식별번호 = table.Column<long>(type: "bigint", nullable: false),
                    건물배치고유식별자 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    영역노드고유식별자 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    건물노드고유식별자 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    배치근거코드 = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    근거종류코드 = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    건물분류코드 = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    시각Family코드 = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    표현층수 = table.Column<int>(type: "int", nullable: false),
                    건물바닥면적제곱미터 = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    높이미터 = table.Column<decimal>(type: "decimal(12,4)", precision: 12, scale: 4, nullable: true),
                    위치X = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    위치Y = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    위치Z = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Y축회전 = table.Column<decimal>(type: "decimal(9,4)", precision: 9, scale: 4, nullable: false),
                    표현전용여부 = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_시뮬레이션월드_건물배치계획", x => x.식별번호);
                    table.ForeignKey(
                        name: "FK_시뮬레이션월드_건물배치계획_시뮬레이션월드_파생실행_파생실행식별번호",
                        column: x => x.파생실행식별번호,
                        principalTable: "시뮬레이션월드_파생실행",
                        principalColumn: "식별번호",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "시뮬레이션월드_그래픽표현계획",
                columns: table => new
                {
                    식별번호 = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    파생실행식별번호 = table.Column<long>(type: "bigint", nullable: false),
                    그래픽표현고유식별자 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    대상노드고유식별자 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    표현범위코드 = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    질감세트키 = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    재질변형키 = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    색조팔레트키 = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    배경Profile키 = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    조명Profile키 = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    시간대Profile키 = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    그림자정책코드 = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    그림자투사여부 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    그림자수신여부 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    접지그림자강도 = table.Column<decimal>(type: "decimal(5,4)", precision: 5, scale: 4, nullable: false),
                    그림자거리미터 = table.Column<decimal>(type: "decimal(12,4)", precision: 12, scale: 4, nullable: true),
                    주변광차폐강도 = table.Column<decimal>(type: "decimal(5,4)", precision: 5, scale: 4, nullable: false),
                    세부표현단계코드 = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    품질단계코드 = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    표현전용여부 = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_시뮬레이션월드_그래픽표현계획", x => x.식별번호);
                    table.ForeignKey(
                        name: "FK_시뮬레이션월드_그래픽표현계획_시뮬레이션월드_파생실행_파생실행식별번호",
                        column: x => x.파생실행식별번호,
                        principalTable: "시뮬레이션월드_파생실행",
                        principalColumn: "식별번호",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "시뮬레이션월드_시각배치계획",
                columns: table => new
                {
                    식별번호 = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    파생실행식별번호 = table.Column<long>(type: "bigint", nullable: false),
                    시각배치고유식별자 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    대상노드고유식별자 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    시각키 = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    세부표현단계코드 = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    위치X = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    위치Y = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    위치Z = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Y축회전 = table.Column<decimal>(type: "decimal(9,4)", precision: 9, scale: 4, nullable: false),
                    균일축척 = table.Column<decimal>(type: "decimal(9,4)", precision: 9, scale: 4, nullable: false),
                    표현전용여부 = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_시뮬레이션월드_시각배치계획", x => x.식별번호);
                    table.ForeignKey(
                        name: "FK_시뮬레이션월드_시각배치계획_시뮬레이션월드_파생실행_파생실행식별번호",
                        column: x => x.파생실행식별번호,
                        principalTable: "시뮬레이션월드_파생실행",
                        principalColumn: "식별번호",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "시뮬레이션월드_원본계보",
                columns: table => new
                {
                    식별번호 = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    파생실행식별번호 = table.Column<long>(type: "bigint", nullable: false),
                    원본계보고유식별자 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    원본DB코드 = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    자료코드 = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    원본개정번호 = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    원본SHA256 = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    자료기준시각UTC = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_시뮬레이션월드_원본계보", x => x.식별번호);
                    table.ForeignKey(
                        name: "FK_시뮬레이션월드_원본계보_시뮬레이션월드_파생실행_파생실행식별번호",
                        column: x => x.파생실행식별번호,
                        principalTable: "시뮬레이션월드_파생실행",
                        principalColumn: "식별번호",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "시뮬레이션월드_파생관계",
                columns: table => new
                {
                    식별번호 = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    파생실행식별번호 = table.Column<long>(type: "bigint", nullable: false),
                    관계고유식별자 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    시작노드고유식별자 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    관계코드 = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    도착노드고유식별자 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    근거종류코드 = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    원본계보고유식별자 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    신뢰도 = table.Column<decimal>(type: "decimal(6,5)", precision: 6, scale: 5, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_시뮬레이션월드_파생관계", x => x.식별번호);
                    table.ForeignKey(
                        name: "FK_시뮬레이션월드_파생관계_시뮬레이션월드_파생실행_파생실행식별번호",
                        column: x => x.파생실행식별번호,
                        principalTable: "시뮬레이션월드_파생실행",
                        principalColumn: "식별번호",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "시뮬레이션월드_파생노드",
                columns: table => new
                {
                    식별번호 = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    파생실행식별번호 = table.Column<long>(type: "bigint", nullable: false),
                    노드고유식별자 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    노드종류코드 = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    원본계보고유식별자 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    원본레코드고유식별자 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    근거종류코드 = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    행정구역코드 = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    타일키 = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    영역고유식별자 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    표시이름 = table.Column<string>(type: "varchar(240)", maxLength: 240, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_시뮬레이션월드_파생노드", x => x.식별번호);
                    table.ForeignKey(
                        name: "FK_시뮬레이션월드_파생노드_시뮬레이션월드_파생실행_파생실행식별번호",
                        column: x => x.파생실행식별번호,
                        principalTable: "시뮬레이션월드_파생실행",
                        principalColumn: "식별번호",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_시뮬레이션월드_건물배치계획_파생실행식별번호_건물배치고유식별자",
                table: "시뮬레이션월드_건물배치계획",
                columns: new[] { "파생실행식별번호", "건물배치고유식별자" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_시뮬레이션월드_그래픽표현계획_파생실행식별번호_그래픽표현고유식별자",
                table: "시뮬레이션월드_그래픽표현계획",
                columns: new[] { "파생실행식별번호", "그래픽표현고유식별자" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_시뮬레이션월드_시각배치계획_파생실행식별번호_시각배치고유식별자",
                table: "시뮬레이션월드_시각배치계획",
                columns: new[] { "파생실행식별번호", "시각배치고유식별자" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_시뮬레이션월드_원본계보_파생실행식별번호_원본계보고유식별자",
                table: "시뮬레이션월드_원본계보",
                columns: new[] { "파생실행식별번호", "원본계보고유식별자" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_시뮬레이션월드_파생관계_파생실행식별번호_관계고유식별자",
                table: "시뮬레이션월드_파생관계",
                columns: new[] { "파생실행식별번호", "관계고유식별자" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_시뮬레이션월드_파생노드_파생실행식별번호_노드고유식별자",
                table: "시뮬레이션월드_파생노드",
                columns: new[] { "파생실행식별번호", "노드고유식별자" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_시뮬레이션월드_파생실행_파생실행고유식별자",
                table: "시뮬레이션월드_파생실행",
                column: "파생실행고유식별자",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "시뮬레이션월드_건물배치계획");

            migrationBuilder.DropTable(
                name: "시뮬레이션월드_그래픽표현계획");

            migrationBuilder.DropTable(
                name: "시뮬레이션월드_시각배치계획");

            migrationBuilder.DropTable(
                name: "시뮬레이션월드_원본계보");

            migrationBuilder.DropTable(
                name: "시뮬레이션월드_파생관계");

            migrationBuilder.DropTable(
                name: "시뮬레이션월드_파생노드");

            migrationBuilder.DropTable(
                name: "시뮬레이션월드_파생실행");
        }
    }
}
