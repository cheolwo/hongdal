using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssalddel.Simulation.Persistence.Migrations.WorldDerivation
{
    /// <inheritdoc />
    public partial class 객체표현규칙결합원장추가 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "시뮬레이션월드_객체표현규칙대장",
                columns: table => new
                {
                    식별번호 = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    스키마버전 = table.Column<int>(type: "int", nullable: false),
                    규칙대장개정번호 = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    규칙대장SHA256 = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    생성시각UTC = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    저장시각UTC = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_시뮬레이션월드_객체표현규칙대장", x => x.식별번호);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "시뮬레이션월드_객체표현해석실행",
                columns: table => new
                {
                    식별번호 = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    스키마버전 = table.Column<int>(type: "int", nullable: false),
                    해석실행고유식별자 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    공간실행고유식별자 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    공간출력SHA256 = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Simulation세션고유식별자 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Simulation세션개정번호 = table.Column<long>(type: "bigint", nullable: true),
                    WorldTick = table.Column<long>(type: "bigint", nullable: true),
                    규칙대장개정번호 = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    입력FingerprintSHA256 = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    출력SHA256 = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    해석시각UTC = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    저장시각UTC = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_시뮬레이션월드_객체표현해석실행", x => x.식별번호);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "시뮬레이션월드_객체표현결합규칙",
                columns: table => new
                {
                    식별번호 = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    규칙대장식별번호 = table.Column<long>(type: "bigint", nullable: false),
                    결합규칙고유식별자 = table.Column<string>(type: "varchar(180)", maxLength: 180, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    결합규칙개정번호 = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    규칙상태코드 = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    객체의미코드 = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    적용범위코드 = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    공간규칙고유식별자 = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    공간규칙개정번호 = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Simulation규칙고유식별자 = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Simulation규칙개정번호 = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Simulation규칙필수여부 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    최소근거종류코드 = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    기본구성키 = table.Column<string>(type: "varchar(180)", maxLength: 180, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    동적표현의도묶음키 = table.Column<string>(type: "varchar(180)", maxLength: 180, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    규칙미충족처리코드 = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    우선순위 = table.Column<int>(type: "int", nullable: false),
                    표현전용여부 = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_시뮬레이션월드_객체표현결합규칙", x => x.식별번호);
                    table.ForeignKey(
                        name: "FK_시뮬레이션월드_객체표현결합규칙_시뮬레이션월드_객체표현규칙대장_규칙대장식별번호",
                        column: x => x.규칙대장식별번호,
                        principalTable: "시뮬레이션월드_객체표현규칙대장",
                        principalColumn: "식별번호",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "시뮬레이션월드_공간규칙Metadata",
                columns: table => new
                {
                    식별번호 = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    규칙대장식별번호 = table.Column<long>(type: "bigint", nullable: false),
                    공간규칙고유식별자 = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    공간규칙개정번호 = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    규칙상태코드 = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    공간사실종류코드 = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    연산자코드 = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    기대값코드 = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    필수근거종류코드 = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    규칙설명 = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_시뮬레이션월드_공간규칙Metadata", x => x.식별번호);
                    table.ForeignKey(
                        name: "FK_시뮬레이션월드_공간규칙Metadata_시뮬레이션월드_객체표현규칙대장_규칙대장식별번호",
                        column: x => x.규칙대장식별번호,
                        principalTable: "시뮬레이션월드_객체표현규칙대장",
                        principalColumn: "식별번호",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "시뮬레이션월드_Simulation규칙Metadata",
                columns: table => new
                {
                    식별번호 = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    규칙대장식별번호 = table.Column<long>(type: "bigint", nullable: false),
                    Simulation규칙고유식별자 = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Simulation규칙개정번호 = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    규칙상태코드 = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    상태종류코드 = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    기대상태코드 = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    규칙설명 = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_시뮬레이션월드_Simulation규칙Metadata", x => x.식별번호);
                    table.ForeignKey(
                        name: "FK_시뮬레이션월드_Simulation규칙Metadata_시뮬레이션월드_객체표현규칙대장_규칙대장식별번호",
                        column: x => x.규칙대장식별번호,
                        principalTable: "시뮬레이션월드_객체표현규칙대장",
                        principalColumn: "식별번호",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "시뮬레이션월드_객체표현해석결과",
                columns: table => new
                {
                    식별번호 = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    해석실행식별번호 = table.Column<long>(type: "bigint", nullable: false),
                    해석결과고유식별자 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    대상노드고유식별자 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    객체의미코드 = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    적용범위코드 = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    해석결과코드 = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    적용결합규칙고유식별자 = table.Column<string>(type: "varchar(180)", maxLength: 180, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    적용결합규칙개정번호 = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    적용공간규칙고유식별자 = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    적용Simulation규칙고유식별자 = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    기본구성키 = table.Column<string>(type: "varchar(180)", maxLength: 180, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    동적표현의도묶음키 = table.Column<string>(type: "varchar(180)", maxLength: 180, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    규칙미충족처리코드 = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    근거종류코드 = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    표현전용여부 = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_시뮬레이션월드_객체표현해석결과", x => x.식별번호);
                    table.ForeignKey(
                        name: "FK_시뮬레이션월드_객체표현해석결과_시뮬레이션월드_객체표현해석실행_해석실행식별번호",
                        column: x => x.해석실행식별번호,
                        principalTable: "시뮬레이션월드_객체표현해석실행",
                        principalColumn: "식별번호",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_시뮬레이션월드_객체표현결합규칙_규칙대장식별번호_결합규칙고유식별자_결합규칙개정번호",
                table: "시뮬레이션월드_객체표현결합규칙",
                columns: new[] { "규칙대장식별번호", "결합규칙고유식별자", "결합규칙개정번호" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_시뮬레이션월드_객체표현규칙대장_규칙대장개정번호",
                table: "시뮬레이션월드_객체표현규칙대장",
                column: "규칙대장개정번호",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_시뮬레이션월드_객체표현해석결과_해석실행식별번호_해석결과고유식별자",
                table: "시뮬레이션월드_객체표현해석결과",
                columns: new[] { "해석실행식별번호", "해석결과고유식별자" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_시뮬레이션월드_객체표현해석실행_해석실행고유식별자",
                table: "시뮬레이션월드_객체표현해석실행",
                column: "해석실행고유식별자",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_시뮬레이션월드_공간규칙Metadata_규칙대장식별번호_공간규칙고유식별자_공간규칙개정번호",
                table: "시뮬레이션월드_공간규칙Metadata",
                columns: new[] { "규칙대장식별번호", "공간규칙고유식별자", "공간규칙개정번호" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_시뮬레이션월드_Simulation규칙Metadata_규칙대장식별번호_Simulation규칙고유식별자_Simu~",
                table: "시뮬레이션월드_Simulation규칙Metadata",
                columns: new[] { "규칙대장식별번호", "Simulation규칙고유식별자", "Simulation규칙개정번호" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "시뮬레이션월드_객체표현결합규칙");

            migrationBuilder.DropTable(
                name: "시뮬레이션월드_객체표현해석결과");

            migrationBuilder.DropTable(
                name: "시뮬레이션월드_공간규칙Metadata");

            migrationBuilder.DropTable(
                name: "시뮬레이션월드_Simulation규칙Metadata");

            migrationBuilder.DropTable(
                name: "시뮬레이션월드_객체표현해석실행");

            migrationBuilder.DropTable(
                name: "시뮬레이션월드_객체표현규칙대장");
        }
    }
}
