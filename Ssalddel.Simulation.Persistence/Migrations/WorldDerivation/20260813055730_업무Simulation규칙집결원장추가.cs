using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssalddel.Simulation.Persistence.Migrations.WorldDerivation
{
    /// <inheritdoc />
    public partial class 업무Simulation규칙집결원장추가 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "시뮬레이션월드_업무Simulation규칙대장",
                columns: table => new
                {
                    식별번호 = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    스키마버전 = table.Column<int>(type: "int", nullable: false),
                    규칙대장개정번호 = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    공간실행고유식별자 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    공간출력SHA256 = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    규칙대장SHA256 = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    생성시각UTC = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    저장시각UTC = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_시뮬레이션월드_업무Simulation규칙대장", x => x.식별번호);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "시뮬레이션월드_객체업무규칙연결",
                columns: table => new
                {
                    식별번호 = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    규칙대장식별번호 = table.Column<long>(type: "bigint", nullable: false),
                    연결고유식별자 = table.Column<string>(type: "varchar(360)", maxLength: 360, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    시설고유식별자 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    기능코드 = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    규칙고유식별자 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    규칙개정번호 = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    적용범위코드 = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    우선순위 = table.Column<int>(type: "int", nullable: false),
                    근거종류코드 = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    활성여부 = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_시뮬레이션월드_객체업무규칙연결", x => x.식별번호);
                    table.ForeignKey(
                        name: "FK_시뮬레이션월드_객체업무규칙연결_시뮬레이션월드_업무Simulation규칙대장_규칙대장식별번호",
                        column: x => x.규칙대장식별번호,
                        principalTable: "시뮬레이션월드_업무Simulation규칙대장",
                        principalColumn: "식별번호",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "시뮬레이션월드_시설기능대장",
                columns: table => new
                {
                    식별번호 = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    규칙대장식별번호 = table.Column<long>(type: "bigint", nullable: false),
                    시설기능고유식별자 = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    시설고유식별자 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    기능코드 = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    근거종류코드 = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_시뮬레이션월드_시설기능대장", x => x.식별번호);
                    table.ForeignKey(
                        name: "FK_시뮬레이션월드_시설기능대장_시뮬레이션월드_업무Simulation규칙대장_규칙대장식별번호",
                        column: x => x.규칙대장식별번호,
                        principalTable: "시뮬레이션월드_업무Simulation규칙대장",
                        principalColumn: "식별번호",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "시뮬레이션월드_시설의미대장",
                columns: table => new
                {
                    식별번호 = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    규칙대장식별번호 = table.Column<long>(type: "bigint", nullable: false),
                    시설고유식별자 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    공간노드고유식별자 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    시설종류코드 = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    근거종류코드 = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    근거원본고유식별자 = table.Column<string>(type: "varchar(240)", maxLength: 240, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    분류신뢰수준코드 = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Scenario지정여부 = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_시뮬레이션월드_시설의미대장", x => x.식별번호);
                    table.ForeignKey(
                        name: "FK_시뮬레이션월드_시설의미대장_시뮬레이션월드_업무Simulation규칙대장_규칙대장식별번호",
                        column: x => x.규칙대장식별번호,
                        principalTable: "시뮬레이션월드_업무Simulation규칙대장",
                        principalColumn: "식별번호",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "시뮬레이션월드_업무Simulation규칙",
                columns: table => new
                {
                    식별번호 = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    규칙대장식별번호 = table.Column<long>(type: "bigint", nullable: false),
                    규칙고유식별자 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    규칙개정번호 = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    규칙영역코드 = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    규칙종류코드 = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    규칙상태코드 = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    규칙Engine키 = table.Column<string>(type: "varchar(180)", maxLength: 180, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    입력계약키 = table.Column<string>(type: "varchar(180)", maxLength: 180, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    출력계약키 = table.Column<string>(type: "varchar(180)", maxLength: 180, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    결정적실행여부 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Simulation전용여부 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    규칙설명 = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_시뮬레이션월드_업무Simulation규칙", x => x.식별번호);
                    table.ForeignKey(
                        name: "FK_시뮬레이션월드_업무Simulation규칙_시뮬레이션월드_업무Simulation규칙대장_규칙대장식별번호",
                        column: x => x.규칙대장식별번호,
                        principalTable: "시뮬레이션월드_업무Simulation규칙대장",
                        principalColumn: "식별번호",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "시뮬레이션월드_업무Simulation규칙Parameter",
                columns: table => new
                {
                    식별번호 = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    규칙대장식별번호 = table.Column<long>(type: "bigint", nullable: false),
                    규칙고유식별자 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    규칙개정번호 = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Parameter코드 = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    값종류코드 = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    값 = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    단위코드 = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    근거종류코드 = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_시뮬레이션월드_업무Simulation규칙Parameter", x => x.식별번호);
                    table.ForeignKey(
                        name: "FK_시뮬레이션월드_업무Simulation규칙Parameter_시뮬레이션월드_업무Simulation규칙대장_규칙대~",
                        column: x => x.규칙대장식별번호,
                        principalTable: "시뮬레이션월드_업무Simulation규칙대장",
                        principalColumn: "식별번호",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "시뮬레이션월드_Scenario규칙묶음",
                columns: table => new
                {
                    식별번호 = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    규칙대장식별번호 = table.Column<long>(type: "bigint", nullable: false),
                    Scenario규칙묶음고유식별자 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    규칙묶음개정번호 = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AreaSet고유식별자 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_시뮬레이션월드_Scenario규칙묶음", x => x.식별번호);
                    table.ForeignKey(
                        name: "FK_시뮬레이션월드_Scenario규칙묶음_시뮬레이션월드_업무Simulation규칙대장_규칙대장식별번호",
                        column: x => x.규칙대장식별번호,
                        principalTable: "시뮬레이션월드_업무Simulation규칙대장",
                        principalColumn: "식별번호",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "시뮬레이션월드_Scenario규칙항목",
                columns: table => new
                {
                    식별번호 = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    규칙대장식별번호 = table.Column<long>(type: "bigint", nullable: false),
                    Scenario규칙묶음고유식별자 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    규칙묶음개정번호 = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    규칙고유식별자 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    규칙개정번호 = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    적용순서 = table.Column<int>(type: "int", nullable: false),
                    필수여부 = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_시뮬레이션월드_Scenario규칙항목", x => x.식별번호);
                    table.ForeignKey(
                        name: "FK_시뮬레이션월드_Scenario규칙항목_시뮬레이션월드_업무Simulation규칙대장_규칙대장식별번호",
                        column: x => x.규칙대장식별번호,
                        principalTable: "시뮬레이션월드_업무Simulation규칙대장",
                        principalColumn: "식별번호",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_시뮬레이션월드_객체업무규칙연결_규칙대장식별번호_연결고유식별자",
                table: "시뮬레이션월드_객체업무규칙연결",
                columns: new[] { "규칙대장식별번호", "연결고유식별자" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_시뮬레이션월드_시설기능대장_규칙대장식별번호_시설기능고유식별자",
                table: "시뮬레이션월드_시설기능대장",
                columns: new[] { "규칙대장식별번호", "시설기능고유식별자" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_시뮬레이션월드_시설의미대장_규칙대장식별번호_시설고유식별자",
                table: "시뮬레이션월드_시설의미대장",
                columns: new[] { "규칙대장식별번호", "시설고유식별자" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_시뮬레이션월드_업무Simulation규칙_규칙대장식별번호_규칙고유식별자_규칙개정번호",
                table: "시뮬레이션월드_업무Simulation규칙",
                columns: new[] { "규칙대장식별번호", "규칙고유식별자", "규칙개정번호" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_시뮬레이션월드_업무Simulation규칙대장_규칙대장개정번호",
                table: "시뮬레이션월드_업무Simulation규칙대장",
                column: "규칙대장개정번호",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_시뮬레이션월드_업무Simulation규칙Parameter_규칙대장식별번호_규칙고유식별자_규칙개정번호_Para~",
                table: "시뮬레이션월드_업무Simulation규칙Parameter",
                columns: new[] { "규칙대장식별번호", "규칙고유식별자", "규칙개정번호", "Parameter코드" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_시뮬레이션월드_Scenario규칙묶음_규칙대장식별번호_Scenario규칙묶음고유식별자_규칙묶음개정번호",
                table: "시뮬레이션월드_Scenario규칙묶음",
                columns: new[] { "규칙대장식별번호", "Scenario규칙묶음고유식별자", "규칙묶음개정번호" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_시뮬레이션월드_Scenario규칙항목_규칙대장식별번호_Scenario규칙묶음고유식별자_규칙묶음개정번호_규칙고~",
                table: "시뮬레이션월드_Scenario규칙항목",
                columns: new[] { "규칙대장식별번호", "Scenario규칙묶음고유식별자", "규칙묶음개정번호", "규칙고유식별자", "규칙개정번호" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "시뮬레이션월드_객체업무규칙연결");

            migrationBuilder.DropTable(
                name: "시뮬레이션월드_시설기능대장");

            migrationBuilder.DropTable(
                name: "시뮬레이션월드_시설의미대장");

            migrationBuilder.DropTable(
                name: "시뮬레이션월드_업무Simulation규칙");

            migrationBuilder.DropTable(
                name: "시뮬레이션월드_업무Simulation규칙Parameter");

            migrationBuilder.DropTable(
                name: "시뮬레이션월드_Scenario규칙묶음");

            migrationBuilder.DropTable(
                name: "시뮬레이션월드_Scenario규칙항목");

            migrationBuilder.DropTable(
                name: "시뮬레이션월드_업무Simulation규칙대장");
        }
    }
}
