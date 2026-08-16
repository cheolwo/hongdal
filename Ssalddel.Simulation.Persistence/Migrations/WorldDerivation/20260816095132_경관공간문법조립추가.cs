using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssalddel.Simulation.Persistence.Migrations.WorldDerivation
{
    /// <inheritdoc />
    public partial class 경관공간문법조립추가 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "시뮬레이션월드_경관조립실행",
                columns: table => new
                {
                    식별번호 = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    경관Graph생성고유식별자 = table.Column<string>(type: "varchar(240)", maxLength: 240, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    타일키 = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    영역묶음고유식별자 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    경관문법개정번호 = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    경관문법SHA256 = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    경관GraphSHA256 = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    생성상태코드 = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    표현전용여부 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    저장시각UTC = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_시뮬레이션월드_경관조립실행", x => x.식별번호);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "시뮬레이션월드_경관공간Edge",
                columns: table => new
                {
                    식별번호 = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    경관조립실행식별번호 = table.Column<long>(type: "bigint", nullable: false),
                    공간Edge고유식별자 = table.Column<string>(type: "varchar(220)", maxLength: 220, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    출발공간Node고유식별자 = table.Column<string>(type: "varchar(220)", maxLength: 220, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    관계코드 = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    도착공간Node고유식별자 = table.Column<string>(type: "varchar(220)", maxLength: 220, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    연결자종류코드 = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    근거종류코드 = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    외부연결Stub여부 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    인접타일키 = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    모판배치고유식별자 = table.Column<string>(type: "varchar(220)", maxLength: 220, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    경로서명 = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    방향코드 = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    세계동쪽좌표미터 = table.Column<double>(type: "double", nullable: false),
                    세계북쪽좌표미터 = table.Column<double>(type: "double", nullable: false),
                    연결너비미터 = table.Column<double>(type: "double", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_시뮬레이션월드_경관공간Edge", x => x.식별번호);
                    table.ForeignKey(
                        name: "FK_시뮬레이션월드_경관공간Edge_시뮬레이션월드_경관조립실행_경관조립실행식별번호",
                        column: x => x.경관조립실행식별번호,
                        principalTable: "시뮬레이션월드_경관조립실행",
                        principalColumn: "식별번호",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "시뮬레이션월드_경관공간Node",
                columns: table => new
                {
                    식별번호 = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    경관조립실행식별번호 = table.Column<long>(type: "bigint", nullable: false),
                    공간Node고유식별자 = table.Column<string>(type: "varchar(220)", maxLength: 220, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    상위공간Node고유식별자 = table.Column<string>(type: "varchar(220)", maxLength: 220, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    공간위상코드 = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    공간의미코드 = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    근거종류코드 = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    중심동쪽좌표미터 = table.Column<double>(type: "double", nullable: false),
                    중심북쪽좌표미터 = table.Column<double>(type: "double", nullable: false),
                    너비미터 = table.Column<double>(type: "double", nullable: false),
                    깊이미터 = table.Column<double>(type: "double", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_시뮬레이션월드_경관공간Node", x => x.식별번호);
                    table.ForeignKey(
                        name: "FK_시뮬레이션월드_경관공간Node_시뮬레이션월드_경관조립실행_경관조립실행식별번호",
                        column: x => x.경관조립실행식별번호,
                        principalTable: "시뮬레이션월드_경관조립실행",
                        principalColumn: "식별번호",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "시뮬레이션월드_경관모판배치",
                columns: table => new
                {
                    식별번호 = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    경관조립실행식별번호 = table.Column<long>(type: "bigint", nullable: false),
                    모판배치고유식별자 = table.Column<string>(type: "varchar(220)", maxLength: 220, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    공간Node고유식별자 = table.Column<string>(type: "varchar(220)", maxLength: 220, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    소유타일키 = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CompositionKey = table.Column<string>(type: "varchar(240)", maxLength: 240, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    공간위상코드 = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    근거종류코드 = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    동쪽좌표미터 = table.Column<double>(type: "double", nullable: false),
                    북쪽좌표미터 = table.Column<double>(type: "double", nullable: false),
                    물리표고미터 = table.Column<double>(type: "double", nullable: false),
                    회전각도 = table.Column<double>(type: "double", nullable: false),
                    대칭여부 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    결정시드 = table.Column<int>(type: "int", nullable: false),
                    점유너비미터 = table.Column<double>(type: "double", nullable: false),
                    점유깊이미터 = table.Column<double>(type: "double", nullable: false),
                    표현전용여부 = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_시뮬레이션월드_경관모판배치", x => x.식별번호);
                    table.ForeignKey(
                        name: "FK_시뮬레이션월드_경관모판배치_시뮬레이션월드_경관조립실행_경관조립실행식별번호",
                        column: x => x.경관조립실행식별번호,
                        principalTable: "시뮬레이션월드_경관조립실행",
                        principalColumn: "식별번호",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "시뮬레이션월드_경관조립미해결",
                columns: table => new
                {
                    식별번호 = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    경관조립실행식별번호 = table.Column<long>(type: "bigint", nullable: false),
                    미해결고유식별자 = table.Column<string>(type: "varchar(240)", maxLength: 240, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    공간Node고유식별자 = table.Column<string>(type: "varchar(220)", maxLength: 220, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    미해결사유코드 = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    필요공간의미코드 = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    근거종류코드 = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    상세설명 = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_시뮬레이션월드_경관조립미해결", x => x.식별번호);
                    table.ForeignKey(
                        name: "FK_시뮬레이션월드_경관조립미해결_시뮬레이션월드_경관조립실행_경관조립실행식별번호",
                        column: x => x.경관조립실행식별번호,
                        principalTable: "시뮬레이션월드_경관조립실행",
                        principalColumn: "식별번호",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_시뮬레이션월드_경관공간Edge_경관조립실행식별번호_공간Edge고유식별자",
                table: "시뮬레이션월드_경관공간Edge",
                columns: new[] { "경관조립실행식별번호", "공간Edge고유식별자" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_시뮬레이션월드_경관공간Node_경관조립실행식별번호_공간Node고유식별자",
                table: "시뮬레이션월드_경관공간Node",
                columns: new[] { "경관조립실행식별번호", "공간Node고유식별자" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_시뮬레이션월드_경관모판배치_경관조립실행식별번호_모판배치고유식별자",
                table: "시뮬레이션월드_경관모판배치",
                columns: new[] { "경관조립실행식별번호", "모판배치고유식별자" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_시뮬레이션월드_경관조립미해결_경관조립실행식별번호_미해결고유식별자",
                table: "시뮬레이션월드_경관조립미해결",
                columns: new[] { "경관조립실행식별번호", "미해결고유식별자" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_시뮬레이션월드_경관조립실행_경관Graph생성고유식별자",
                table: "시뮬레이션월드_경관조립실행",
                column: "경관Graph생성고유식별자",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_시뮬레이션월드_경관조립실행_타일키_저장시각UTC",
                table: "시뮬레이션월드_경관조립실행",
                columns: new[] { "타일키", "저장시각UTC" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "시뮬레이션월드_경관공간Edge");

            migrationBuilder.DropTable(
                name: "시뮬레이션월드_경관공간Node");

            migrationBuilder.DropTable(
                name: "시뮬레이션월드_경관모판배치");

            migrationBuilder.DropTable(
                name: "시뮬레이션월드_경관조립미해결");

            migrationBuilder.DropTable(
                name: "시뮬레이션월드_경관조립실행");
        }
    }
}
