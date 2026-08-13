using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssalddel.Simulation.Persistence.Migrations.WorldDerivation
{
    /// <inheritdoc />
    public partial class Synty경관JobShell분리 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "시각자산대장개정번호",
                table: "시뮬레이션월드_파생실행",
                type: "varchar(120)",
                maxLength: 120,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(120)",
                oldMaxLength: 120)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "시뮬레이션월드_Synty경관실행",
                columns: table => new
                {
                    식별번호 = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    스키마버전 = table.Column<int>(type: "int", nullable: false),
                    시각실행고유식별자 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    작업고유식별자 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    공간실행고유식별자 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    공간출력SHA256 = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    영역묶음고유식별자 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    작업범위종류코드 = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    작업범위고유식별자 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    경관규칙개정번호 = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Synty구성대장개정번호 = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    URP표현대장개정번호 = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    배치시드 = table.Column<int>(type: "int", nullable: false),
                    대상플랫폼코드 = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    품질단계코드 = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    입력지문SHA256 = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    출력해시SHA256 = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    생성시각UTC = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    저장시각UTC = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    작업상태코드 = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_시뮬레이션월드_Synty경관실행", x => x.식별번호);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "시뮬레이션월드_Synty그래픽표현계획",
                columns: table => new
                {
                    식별번호 = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Synty경관실행식별번호 = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("PK_시뮬레이션월드_Synty그래픽표현계획", x => x.식별번호);
                    table.ForeignKey(
                        name: "FK_시뮬레이션월드_Synty그래픽표현계획_시뮬레이션월드_Synty경관실행_Synty경관실행식별번호",
                        column: x => x.Synty경관실행식별번호,
                        principalTable: "시뮬레이션월드_Synty경관실행",
                        principalColumn: "식별번호",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "시뮬레이션월드_Synty배치거부",
                columns: table => new
                {
                    식별번호 = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Synty경관실행식별번호 = table.Column<long>(type: "bigint", nullable: false),
                    배치거부고유식별자 = table.Column<string>(type: "varchar(240)", maxLength: 240, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    대상노드고유식별자 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    거부사유코드 = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    거부상세 = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_시뮬레이션월드_Synty배치거부", x => x.식별번호);
                    table.ForeignKey(
                        name: "FK_시뮬레이션월드_Synty배치거부_시뮬레이션월드_Synty경관실행_Synty경관실행식별번호",
                        column: x => x.Synty경관실행식별번호,
                        principalTable: "시뮬레이션월드_Synty경관실행",
                        principalColumn: "식별번호",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "시뮬레이션월드_Synty시각배치계획",
                columns: table => new
                {
                    식별번호 = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Synty경관실행식별번호 = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("PK_시뮬레이션월드_Synty시각배치계획", x => x.식별번호);
                    table.ForeignKey(
                        name: "FK_시뮬레이션월드_Synty시각배치계획_시뮬레이션월드_Synty경관실행_Synty경관실행식별번호",
                        column: x => x.Synty경관실행식별번호,
                        principalTable: "시뮬레이션월드_Synty경관실행",
                        principalColumn: "식별번호",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_시뮬레이션월드_Synty경관실행_공간실행고유식별자",
                table: "시뮬레이션월드_Synty경관실행",
                column: "공간실행고유식별자");

            migrationBuilder.CreateIndex(
                name: "IX_시뮬레이션월드_Synty경관실행_시각실행고유식별자",
                table: "시뮬레이션월드_Synty경관실행",
                column: "시각실행고유식별자",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_시뮬레이션월드_Synty경관실행_작업고유식별자",
                table: "시뮬레이션월드_Synty경관실행",
                column: "작업고유식별자");

            migrationBuilder.CreateIndex(
                name: "IX_시뮬레이션월드_Synty그래픽표현계획_Synty경관실행식별번호_그래픽표현고유식별자",
                table: "시뮬레이션월드_Synty그래픽표현계획",
                columns: new[] { "Synty경관실행식별번호", "그래픽표현고유식별자" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_시뮬레이션월드_Synty배치거부_Synty경관실행식별번호_배치거부고유식별자",
                table: "시뮬레이션월드_Synty배치거부",
                columns: new[] { "Synty경관실행식별번호", "배치거부고유식별자" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_시뮬레이션월드_Synty시각배치계획_Synty경관실행식별번호_시각배치고유식별자",
                table: "시뮬레이션월드_Synty시각배치계획",
                columns: new[] { "Synty경관실행식별번호", "시각배치고유식별자" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "시뮬레이션월드_Synty그래픽표현계획");

            migrationBuilder.DropTable(
                name: "시뮬레이션월드_Synty배치거부");

            migrationBuilder.DropTable(
                name: "시뮬레이션월드_Synty시각배치계획");

            migrationBuilder.DropTable(
                name: "시뮬레이션월드_Synty경관실행");

            migrationBuilder.UpdateData(
                table: "시뮬레이션월드_파생실행",
                keyColumn: "시각자산대장개정번호",
                keyValue: null,
                column: "시각자산대장개정번호",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "시각자산대장개정번호",
                table: "시뮬레이션월드_파생실행",
                type: "varchar(120)",
                maxLength: 120,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(120)",
                oldMaxLength: 120,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
