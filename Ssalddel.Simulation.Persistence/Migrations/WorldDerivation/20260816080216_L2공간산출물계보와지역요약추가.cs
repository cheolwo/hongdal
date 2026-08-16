using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssalddel.Simulation.Persistence.Migrations.WorldDerivation
{
    /// <inheritdoc />
    public partial class L2공간산출물계보와지역요약추가 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NoData값",
                table: "시뮬레이션월드_Unity산출물",
                type: "varchar(80)",
                maxLength: 80,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "높이기준코드",
                table: "시뮬레이션월드_Unity산출물",
                type: "varchar(80)",
                maxLength: 80,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<long>(
                name: "산출물바이트길이",
                table: "시뮬레이션월드_Unity산출물",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "산출물형식코드",
                table: "시뮬레이션월드_Unity산출물",
                type: "varchar(80)",
                maxLength: 80,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "수평좌표계코드",
                table: "시뮬레이션월드_Unity산출물",
                type: "varchar(40)",
                maxLength: 40,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "원본SHA256",
                table: "시뮬레이션월드_Unity산출물",
                type: "varchar(64)",
                maxLength: 64,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "원본개정번호",
                table: "시뮬레이션월드_Unity산출물",
                type: "varchar(160)",
                maxLength: 160,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "원본기준일",
                table: "시뮬레이션월드_Unity산출물",
                type: "varchar(40)",
                maxLength: 40,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "원본해상도미터",
                table: "시뮬레이션월드_Unity산출물",
                type: "decimal(12,4)",
                precision: 12,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "표본너비",
                table: "시뮬레이션월드_Unity산출물",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "표본높이",
                table: "시뮬레이션월드_Unity산출물",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "시뮬레이션월드_지역표현요약프로필",
                columns: table => new
                {
                    식별번호 = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    파생실행식별번호 = table.Column<long>(type: "bigint", nullable: false),
                    요약프로필개정번호 = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    요약프로필SHA256 = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    결정적배치Seed = table.Column<int>(type: "int", nullable: false),
                    분류별최대표현비율 = table.Column<decimal>(type: "decimal(8,6)", precision: 8, scale: 6, nullable: false),
                    L0표현슬롯수 = table.Column<int>(type: "int", nullable: false),
                    L1표현슬롯수 = table.Column<int>(type: "int", nullable: false),
                    L2표현슬롯수 = table.Column<int>(type: "int", nullable: false),
                    LOD별표현예산JSON = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_시뮬레이션월드_지역표현요약프로필", x => x.식별번호);
                    table.ForeignKey(
                        name: "FK_시뮬레이션월드_지역표현요약프로필_시뮬레이션월드_파생실행_파생실행식별번호",
                        column: x => x.파생실행식별번호,
                        principalTable: "시뮬레이션월드_파생실행",
                        principalColumn: "식별번호",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "시뮬레이션월드_지역표현요약실행",
                columns: table => new
                {
                    식별번호 = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    파생실행식별번호 = table.Column<long>(type: "bigint", nullable: false),
                    요약프로필식별번호 = table.Column<long>(type: "bigint", nullable: false),
                    지역고유식별자 = table.Column<string>(type: "varchar(240)", maxLength: 240, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    타일키 = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    세부표현단계코드 = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    입력지문SHA256 = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    요약결과SHA256 = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    요약상태코드 = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    생성일시UTC = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    저장일시UTC = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    전체후보수 = table.Column<int>(type: "int", nullable: false),
                    선정항목수 = table.Column<int>(type: "int", nullable: false),
                    전체대표원본수 = table.Column<int>(type: "int", nullable: false),
                    선정대표원본수 = table.Column<int>(type: "int", nullable: false),
                    화면생략대표원본수 = table.Column<int>(type: "int", nullable: false),
                    요청표현슬롯수 = table.Column<int>(type: "int", nullable: false),
                    배정표현슬롯수 = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_시뮬레이션월드_지역표현요약실행", x => x.식별번호);
                    table.ForeignKey(
                        name: "FK_시뮬레이션월드_지역표현요약실행_시뮬레이션월드_지역표현요약프로필_요약프로필식별번호",
                        column: x => x.요약프로필식별번호,
                        principalTable: "시뮬레이션월드_지역표현요약프로필",
                        principalColumn: "식별번호",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_시뮬레이션월드_지역표현요약실행_시뮬레이션월드_파생실행_파생실행식별번호",
                        column: x => x.파생실행식별번호,
                        principalTable: "시뮬레이션월드_파생실행",
                        principalColumn: "식별번호",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "시뮬레이션월드_지역표현요약분류보고서",
                columns: table => new
                {
                    식별번호 = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    요약실행식별번호 = table.Column<long>(type: "bigint", nullable: false),
                    표현분류코드 = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    후보수 = table.Column<int>(type: "int", nullable: false),
                    전체대표원본수 = table.Column<int>(type: "int", nullable: false),
                    선정대표원본수 = table.Column<int>(type: "int", nullable: false),
                    화면생략대표원본수 = table.Column<int>(type: "int", nullable: false),
                    전체대표면적제곱미터 = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    선정대표면적제곱미터 = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    배정표현슬롯수 = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_시뮬레이션월드_지역표현요약분류보고서", x => x.식별번호);
                    table.ForeignKey(
                        name: "FK_시뮬레이션월드_지역표현요약분류보고서_시뮬레이션월드_지역표현요약실행_요약실행식별번호",
                        column: x => x.요약실행식별번호,
                        principalTable: "시뮬레이션월드_지역표현요약실행",
                        principalColumn: "식별번호",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "시뮬레이션월드_지역표현요약항목",
                columns: table => new
                {
                    식별번호 = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    요약실행식별번호 = table.Column<long>(type: "bigint", nullable: false),
                    요약항목고유식별자 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    원본객체고유식별자 = table.Column<string>(type: "varchar(240)", maxLength: 240, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    표현분류코드 = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    객체종류코드 = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    선정이유코드 = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    근거수준코드 = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    시각의미키 = table.Column<string>(type: "varchar(240)", maxLength: 240, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    대표원본수 = table.Column<int>(type: "int", nullable: false),
                    대표면적제곱미터 = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    표현슬롯수 = table.Column<int>(type: "int", nullable: false),
                    최소가시표현수 = table.Column<int>(type: "int", nullable: false),
                    공개상세연결여부 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    표현전용여부 = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_시뮬레이션월드_지역표현요약항목", x => x.식별번호);
                    table.ForeignKey(
                        name: "FK_시뮬레이션월드_지역표현요약항목_시뮬레이션월드_지역표현요약실행_요약실행식별번호",
                        column: x => x.요약실행식별번호,
                        principalTable: "시뮬레이션월드_지역표현요약실행",
                        principalColumn: "식별번호",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_시뮬레이션월드_지역표현요약분류보고서_요약실행식별번호_표현분류코드",
                table: "시뮬레이션월드_지역표현요약분류보고서",
                columns: new[] { "요약실행식별번호", "표현분류코드" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_시뮬레이션월드_지역표현요약실행_요약프로필식별번호",
                table: "시뮬레이션월드_지역표현요약실행",
                column: "요약프로필식별번호");

            migrationBuilder.CreateIndex(
                name: "IX_시뮬레이션월드_지역표현요약실행_지역고유식별자_타일키_세부표현단계코드",
                table: "시뮬레이션월드_지역표현요약실행",
                columns: new[] { "지역고유식별자", "타일키", "세부표현단계코드" });

            migrationBuilder.CreateIndex(
                name: "IX_시뮬레이션월드_지역표현요약실행_파생실행식별번호_지역고유식별자_타일키_세부표현단계코드",
                table: "시뮬레이션월드_지역표현요약실행",
                columns: new[] { "파생실행식별번호", "지역고유식별자", "타일키", "세부표현단계코드" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_시뮬레이션월드_지역표현요약프로필_파생실행식별번호_요약프로필개정번호",
                table: "시뮬레이션월드_지역표현요약프로필",
                columns: new[] { "파생실행식별번호", "요약프로필개정번호" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_시뮬레이션월드_지역표현요약항목_요약실행식별번호_요약항목고유식별자",
                table: "시뮬레이션월드_지역표현요약항목",
                columns: new[] { "요약실행식별번호", "요약항목고유식별자" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "시뮬레이션월드_지역표현요약분류보고서");

            migrationBuilder.DropTable(
                name: "시뮬레이션월드_지역표현요약항목");

            migrationBuilder.DropTable(
                name: "시뮬레이션월드_지역표현요약실행");

            migrationBuilder.DropTable(
                name: "시뮬레이션월드_지역표현요약프로필");

            migrationBuilder.DropColumn(
                name: "NoData값",
                table: "시뮬레이션월드_Unity산출물");

            migrationBuilder.DropColumn(
                name: "높이기준코드",
                table: "시뮬레이션월드_Unity산출물");

            migrationBuilder.DropColumn(
                name: "산출물바이트길이",
                table: "시뮬레이션월드_Unity산출물");

            migrationBuilder.DropColumn(
                name: "산출물형식코드",
                table: "시뮬레이션월드_Unity산출물");

            migrationBuilder.DropColumn(
                name: "수평좌표계코드",
                table: "시뮬레이션월드_Unity산출물");

            migrationBuilder.DropColumn(
                name: "원본SHA256",
                table: "시뮬레이션월드_Unity산출물");

            migrationBuilder.DropColumn(
                name: "원본개정번호",
                table: "시뮬레이션월드_Unity산출물");

            migrationBuilder.DropColumn(
                name: "원본기준일",
                table: "시뮬레이션월드_Unity산출물");

            migrationBuilder.DropColumn(
                name: "원본해상도미터",
                table: "시뮬레이션월드_Unity산출물");

            migrationBuilder.DropColumn(
                name: "표본너비",
                table: "시뮬레이션월드_Unity산출물");

            migrationBuilder.DropColumn(
                name: "표본높이",
                table: "시뮬레이션월드_Unity산출물");
        }
    }
}
