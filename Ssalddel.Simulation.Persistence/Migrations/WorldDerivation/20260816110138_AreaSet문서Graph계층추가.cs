using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssalddel.Simulation.Persistence.Migrations.WorldDerivation
{
    /// <inheritdoc />
    public partial class AreaSet문서Graph계층추가 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "경관Graph개정번호",
                table: "시뮬레이션월드_경관조립실행",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "경관Graph고유식별자",
                table: "시뮬레이션월드_경관조립실행",
                type: "varchar(240)",
                maxLength: 240,
                nullable: false,
                defaultValue: "legacy-landscape-graph:pending")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "경관Graph역할코드",
                table: "시뮬레이션월드_경관조립실행",
                type: "varchar(80)",
                maxLength: 80,
                nullable: false,
                defaultValue: "LegacyTile")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "경관Graph정의SHA256",
                table: "시뮬레이션월드_경관조립실행",
                type: "varchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "0000000000000000000000000000000000000000000000000000000000000000")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "생성범위코드",
                table: "시뮬레이션월드_경관조립실행",
                type: "varchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "LegacyTile")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.Sql(
                "UPDATE `시뮬레이션월드_경관조립실행` " +
                "SET `경관Graph고유식별자` = CONCAT('legacy-landscape-graph:', `타일키`) " +
                "WHERE `경관Graph고유식별자` = 'legacy-landscape-graph:pending';");

            migrationBuilder.CreateTable(
                name: "시뮬레이션월드_경관Graph공간참조",
                columns: table => new
                {
                    식별번호 = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    경관Graph고유식별자 = table.Column<string>(type: "varchar(240)", maxLength: 240, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    경관Graph개정번호 = table.Column<int>(type: "int", nullable: false),
                    참조종류코드 = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    공간참조고유식별자 = table.Column<string>(type: "varchar(240)", maxLength: 240, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    참조순서 = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_시뮬레이션월드_경관Graph공간참조", x => x.식별번호);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "시뮬레이션월드_경관Graph관계",
                columns: table => new
                {
                    식별번호 = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AreaSet고유식별자 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AreaSet개정번호 = table.Column<int>(type: "int", nullable: false),
                    Graph관계고유식별자 = table.Column<string>(type: "varchar(240)", maxLength: 240, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    출발경관Graph고유식별자 = table.Column<string>(type: "varchar(240)", maxLength: 240, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    도착경관Graph고유식별자 = table.Column<string>(type: "varchar(240)", maxLength: 240, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    관계코드 = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    출발연결지점고유식별자 = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    도착연결지점고유식별자 = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    연결지점종류코드 = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    경로서명 = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_시뮬레이션월드_경관Graph관계", x => x.식별번호);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "시뮬레이션월드_경관Graph정의",
                columns: table => new
                {
                    식별번호 = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AreaSet고유식별자 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    경관Graph고유식별자 = table.Column<string>(type: "varchar(240)", maxLength: 240, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    경관Graph역할코드 = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    경관Graph개정번호 = table.Column<int>(type: "int", nullable: false),
                    경관Graph정의SHA256 = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    생성상태코드 = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    경관GraphSHA256 = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    경계범위보유여부 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    최소동쪽좌표미터 = table.Column<double>(type: "double", nullable: false),
                    최소북쪽좌표미터 = table.Column<double>(type: "double", nullable: false),
                    최대동쪽좌표미터 = table.Column<double>(type: "double", nullable: false),
                    최대북쪽좌표미터 = table.Column<double>(type: "double", nullable: false),
                    저장시각UTC = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_시뮬레이션월드_경관Graph정의", x => x.식별번호);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "시뮬레이션월드_경관GraphTile참조",
                columns: table => new
                {
                    식별번호 = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    경관Graph고유식별자 = table.Column<string>(type: "varchar(240)", maxLength: 240, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    경관Graph개정번호 = table.Column<int>(type: "int", nullable: false),
                    타일키 = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    참조순서 = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_시뮬레이션월드_경관GraphTile참조", x => x.식별번호);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "시뮬레이션월드_AreaSet공간참조",
                columns: table => new
                {
                    식별번호 = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AreaSet고유식별자 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AreaSet개정번호 = table.Column<int>(type: "int", nullable: false),
                    참조종류코드 = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    공간참조고유식별자 = table.Column<string>(type: "varchar(240)", maxLength: 240, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    참조순서 = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_시뮬레이션월드_AreaSet공간참조", x => x.식별번호);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "시뮬레이션월드_AreaSet정의",
                columns: table => new
                {
                    식별번호 = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AreaSet고유식별자 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AreaSet개정번호 = table.Column<int>(type: "int", nullable: false),
                    제목 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    요약 = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    실행정의SHA256 = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    작성문서SHA256 = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    정의상태코드 = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    표현전용여부 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    저장시각UTC = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_시뮬레이션월드_AreaSet정의", x => x.식별번호);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "시뮬레이션월드_AreaSetGraph참조",
                columns: table => new
                {
                    식별번호 = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AreaSet고유식별자 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AreaSet개정번호 = table.Column<int>(type: "int", nullable: false),
                    경관Graph고유식별자 = table.Column<string>(type: "varchar(240)", maxLength: 240, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    참조순서 = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_시뮬레이션월드_AreaSetGraph참조", x => x.식별번호);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_시뮬레이션월드_경관Graph공간참조_경관Graph고유식별자_경관Graph개정번호_참조종류코드_공간참조고유식별자",
                table: "시뮬레이션월드_경관Graph공간참조",
                columns: new[] { "경관Graph고유식별자", "경관Graph개정번호", "참조종류코드", "공간참조고유식별자" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_시뮬레이션월드_경관Graph관계_AreaSet고유식별자_AreaSet개정번호_Graph관계고유식별자",
                table: "시뮬레이션월드_경관Graph관계",
                columns: new[] { "AreaSet고유식별자", "AreaSet개정번호", "Graph관계고유식별자" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_시뮬레이션월드_경관Graph정의_경관Graph고유식별자_경관Graph개정번호",
                table: "시뮬레이션월드_경관Graph정의",
                columns: new[] { "경관Graph고유식별자", "경관Graph개정번호" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_시뮬레이션월드_경관GraphTile참조_경관Graph고유식별자_경관Graph개정번호_타일키",
                table: "시뮬레이션월드_경관GraphTile참조",
                columns: new[] { "경관Graph고유식별자", "경관Graph개정번호", "타일키" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_시뮬레이션월드_경관GraphTile참조_타일키",
                table: "시뮬레이션월드_경관GraphTile참조",
                column: "타일키");

            migrationBuilder.CreateIndex(
                name: "IX_시뮬레이션월드_AreaSet공간참조_AreaSet고유식별자_AreaSet개정번호_참조종류코드_공간참조고유식별자",
                table: "시뮬레이션월드_AreaSet공간참조",
                columns: new[] { "AreaSet고유식별자", "AreaSet개정번호", "참조종류코드", "공간참조고유식별자" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_시뮬레이션월드_AreaSet정의_AreaSet고유식별자_AreaSet개정번호",
                table: "시뮬레이션월드_AreaSet정의",
                columns: new[] { "AreaSet고유식별자", "AreaSet개정번호" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_시뮬레이션월드_AreaSetGraph참조_AreaSet고유식별자_AreaSet개정번호_경관Graph고유식별자",
                table: "시뮬레이션월드_AreaSetGraph참조",
                columns: new[] { "AreaSet고유식별자", "AreaSet개정번호", "경관Graph고유식별자" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "시뮬레이션월드_경관Graph공간참조");

            migrationBuilder.DropTable(
                name: "시뮬레이션월드_경관Graph관계");

            migrationBuilder.DropTable(
                name: "시뮬레이션월드_경관Graph정의");

            migrationBuilder.DropTable(
                name: "시뮬레이션월드_경관GraphTile참조");

            migrationBuilder.DropTable(
                name: "시뮬레이션월드_AreaSet공간참조");

            migrationBuilder.DropTable(
                name: "시뮬레이션월드_AreaSet정의");

            migrationBuilder.DropTable(
                name: "시뮬레이션월드_AreaSetGraph참조");

            migrationBuilder.DropColumn(
                name: "경관Graph개정번호",
                table: "시뮬레이션월드_경관조립실행");

            migrationBuilder.DropColumn(
                name: "경관Graph고유식별자",
                table: "시뮬레이션월드_경관조립실행");

            migrationBuilder.DropColumn(
                name: "경관Graph역할코드",
                table: "시뮬레이션월드_경관조립실행");

            migrationBuilder.DropColumn(
                name: "경관Graph정의SHA256",
                table: "시뮬레이션월드_경관조립실행");

            migrationBuilder.DropColumn(
                name: "생성범위코드",
                table: "시뮬레이션월드_경관조립실행");
        }
    }
}
