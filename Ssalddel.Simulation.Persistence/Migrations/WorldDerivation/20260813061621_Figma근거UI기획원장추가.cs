using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssalddel.Simulation.Persistence.Migrations.WorldDerivation
{
    /// <inheritdoc />
    public partial class Figma근거UI기획원장추가 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "시뮬레이션월드_UI기획대장",
                columns: table => new
                {
                    식별번호 = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    스키마버전 = table.Column<int>(type: "int", nullable: false),
                    UI기획개정번호 = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    업무규칙대장개정번호 = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    업무규칙대장SHA256 = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UI기획SHA256 = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    생성시각UTC = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    저장시각UTC = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_시뮬레이션월드_UI기획대장", x => x.식별번호);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "시뮬레이션월드_UI상태표현기획",
                columns: table => new
                {
                    식별번호 = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UI기획대장식별번호 = table.Column<long>(type: "bigint", nullable: false),
                    상태표현고유식별자 = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    화면영역고유식별자 = table.Column<string>(type: "varchar(240)", maxLength: 240, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    상태코드 = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    한글표시명 = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    심각도코드 = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    표현의도코드 = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    변경행동차단여부 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    표시순서 = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_시뮬레이션월드_UI상태표현기획", x => x.식별번호);
                    table.ForeignKey(
                        name: "FK_시뮬레이션월드_UI상태표현기획_시뮬레이션월드_UI기획대장_UI기획대장식별번호",
                        column: x => x.UI기획대장식별번호,
                        principalTable: "시뮬레이션월드_UI기획대장",
                        principalColumn: "식별번호",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "시뮬레이션월드_UI설계근거",
                columns: table => new
                {
                    식별번호 = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UI기획대장식별번호 = table.Column<long>(type: "bigint", nullable: false),
                    설계근거고유식별자 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    제공자코드 = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Figma파일키 = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FigmaNode식별자 = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    한글제목 = table.Column<string>(type: "varchar(240)", maxLength: 240, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    관측구조코드 = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    확인시각UTC = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_시뮬레이션월드_UI설계근거", x => x.식별번호);
                    table.ForeignKey(
                        name: "FK_시뮬레이션월드_UI설계근거_시뮬레이션월드_UI기획대장_UI기획대장식별번호",
                        column: x => x.UI기획대장식별번호,
                        principalTable: "시뮬레이션월드_UI기획대장",
                        principalColumn: "식별번호",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "시뮬레이션월드_UI업무규칙연결",
                columns: table => new
                {
                    식별번호 = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UI기획대장식별번호 = table.Column<long>(type: "bigint", nullable: false),
                    연결고유식별자 = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    업무규칙고유식별자 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    업무규칙개정번호 = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    화면영역고유식별자 = table.Column<string>(type: "varchar(240)", maxLength: 240, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    연결목적코드 = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    우선순위 = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_시뮬레이션월드_UI업무규칙연결", x => x.식별번호);
                    table.ForeignKey(
                        name: "FK_시뮬레이션월드_UI업무규칙연결_시뮬레이션월드_UI기획대장_UI기획대장식별번호",
                        column: x => x.UI기획대장식별번호,
                        principalTable: "시뮬레이션월드_UI기획대장",
                        principalColumn: "식별번호",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "시뮬레이션월드_UI정보항목기획",
                columns: table => new
                {
                    식별번호 = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UI기획대장식별번호 = table.Column<long>(type: "bigint", nullable: false),
                    정보항목고유식별자 = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    화면영역고유식별자 = table.Column<string>(type: "varchar(240)", maxLength: 240, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    정보종류코드 = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    한글표시명 = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    값의미코드 = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    원본계약키 = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    표시형식코드 = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    단위코드 = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    우선순위 = table.Column<int>(type: "int", nullable: false),
                    계보표시필수여부 = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_시뮬레이션월드_UI정보항목기획", x => x.식별번호);
                    table.ForeignKey(
                        name: "FK_시뮬레이션월드_UI정보항목기획_시뮬레이션월드_UI기획대장_UI기획대장식별번호",
                        column: x => x.UI기획대장식별번호,
                        principalTable: "시뮬레이션월드_UI기획대장",
                        principalColumn: "식별번호",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "시뮬레이션월드_UI행동후보기획",
                columns: table => new
                {
                    식별번호 = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UI기획대장식별번호 = table.Column<long>(type: "bigint", nullable: false),
                    행동후보고유식별자 = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    화면영역고유식별자 = table.Column<string>(type: "varchar(240)", maxLength: 240, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    행동종류코드 = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    한글표시명 = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    기능키 = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    서버Command키 = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Preview필수여부 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    명시적확인필수여부 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    기대개정번호필수여부 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Simulation전용여부 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    표시순서 = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_시뮬레이션월드_UI행동후보기획", x => x.식별번호);
                    table.ForeignKey(
                        name: "FK_시뮬레이션월드_UI행동후보기획_시뮬레이션월드_UI기획대장_UI기획대장식별번호",
                        column: x => x.UI기획대장식별번호,
                        principalTable: "시뮬레이션월드_UI기획대장",
                        principalColumn: "식별번호",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "시뮬레이션월드_UI화면영역기획",
                columns: table => new
                {
                    식별번호 = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UI기획대장식별번호 = table.Column<long>(type: "bigint", nullable: false),
                    화면영역고유식별자 = table.Column<string>(type: "varchar(240)", maxLength: 240, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    시설고유식별자 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    화면종류코드 = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    관점코드 = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    역할코드 = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    업무단계코드 = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    한글제목 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    공간Anchor의미코드 = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    표시순서 = table.Column<int>(type: "int", nullable: false),
                    기본표시여부 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    설계근거고유식별자 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_시뮬레이션월드_UI화면영역기획", x => x.식별번호);
                    table.ForeignKey(
                        name: "FK_시뮬레이션월드_UI화면영역기획_시뮬레이션월드_UI기획대장_UI기획대장식별번호",
                        column: x => x.UI기획대장식별번호,
                        principalTable: "시뮬레이션월드_UI기획대장",
                        principalColumn: "식별번호",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_시뮬레이션월드_UI기획대장_UI기획개정번호",
                table: "시뮬레이션월드_UI기획대장",
                column: "UI기획개정번호",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_시뮬레이션월드_UI상태표현기획_UI기획대장식별번호_상태표현고유식별자",
                table: "시뮬레이션월드_UI상태표현기획",
                columns: new[] { "UI기획대장식별번호", "상태표현고유식별자" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_시뮬레이션월드_UI설계근거_UI기획대장식별번호_설계근거고유식별자",
                table: "시뮬레이션월드_UI설계근거",
                columns: new[] { "UI기획대장식별번호", "설계근거고유식별자" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_시뮬레이션월드_UI업무규칙연결_UI기획대장식별번호_연결고유식별자",
                table: "시뮬레이션월드_UI업무규칙연결",
                columns: new[] { "UI기획대장식별번호", "연결고유식별자" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_시뮬레이션월드_UI정보항목기획_UI기획대장식별번호_정보항목고유식별자",
                table: "시뮬레이션월드_UI정보항목기획",
                columns: new[] { "UI기획대장식별번호", "정보항목고유식별자" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_시뮬레이션월드_UI행동후보기획_UI기획대장식별번호_행동후보고유식별자",
                table: "시뮬레이션월드_UI행동후보기획",
                columns: new[] { "UI기획대장식별번호", "행동후보고유식별자" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_시뮬레이션월드_UI화면영역기획_UI기획대장식별번호_화면영역고유식별자",
                table: "시뮬레이션월드_UI화면영역기획",
                columns: new[] { "UI기획대장식별번호", "화면영역고유식별자" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "시뮬레이션월드_UI상태표현기획");

            migrationBuilder.DropTable(
                name: "시뮬레이션월드_UI설계근거");

            migrationBuilder.DropTable(
                name: "시뮬레이션월드_UI업무규칙연결");

            migrationBuilder.DropTable(
                name: "시뮬레이션월드_UI정보항목기획");

            migrationBuilder.DropTable(
                name: "시뮬레이션월드_UI행동후보기획");

            migrationBuilder.DropTable(
                name: "시뮬레이션월드_UI화면영역기획");

            migrationBuilder.DropTable(
                name: "시뮬레이션월드_UI기획대장");
        }
    }
}
