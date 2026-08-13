using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssalddel.Simulation.Persistence.Migrations.WorldDerivation
{
    /// <inheritdoc />
    public partial class Unity공간변환타일산출물원장추가 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "시뮬레이션월드_Unity공간변환Profile",
                columns: table => new
                {
                    식별번호 = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    파생실행식별번호 = table.Column<long>(type: "bigint", nullable: false),
                    공간변환고유식별자 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    영역묶음고유식별자 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    원본좌표계코드 = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    좌표축변환코드 = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Unity원점동쪽좌표미터 = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    Unity원점북쪽좌표미터 = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    기준표고미터 = table.Column<decimal>(type: "decimal(12,4)", precision: 12, scale: 4, nullable: true),
                    수평축척률 = table.Column<decimal>(type: "decimal(12,6)", precision: 12, scale: 6, nullable: false),
                    높이과장률 = table.Column<decimal>(type: "decimal(12,6)", precision: 12, scale: 6, nullable: false),
                    Unity단위당미터 = table.Column<decimal>(type: "decimal(12,6)", precision: 12, scale: 6, nullable: false),
                    변환규칙개정번호 = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    변환상태코드 = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    변환ProfileSHA256 = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_시뮬레이션월드_Unity공간변환Profile", x => x.식별번호);
                    table.ForeignKey(
                        name: "FK_시뮬레이션월드_Unity공간변환Profile_시뮬레이션월드_파생실행_파생실행식별번호",
                        column: x => x.파생실행식별번호,
                        principalTable: "시뮬레이션월드_파생실행",
                        principalColumn: "식별번호",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "시뮬레이션월드_Unity산출물",
                columns: table => new
                {
                    식별번호 = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    파생실행식별번호 = table.Column<long>(type: "bigint", nullable: false),
                    산출물고유식별자 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    타일Manifest고유식별자 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    산출물종류코드 = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    세부표현단계코드 = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    산출물보관객체키 = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    산출물SHA256 = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    정점수 = table.Column<long>(type: "bigint", nullable: true),
                    삼각형수 = table.Column<long>(type: "bigint", nullable: true),
                    재질슬롯수 = table.Column<int>(type: "int", nullable: true),
                    예상DrawCall수 = table.Column<int>(type: "int", nullable: true),
                    경계정점SHA256 = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    생성상태코드 = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_시뮬레이션월드_Unity산출물", x => x.식별번호);
                    table.ForeignKey(
                        name: "FK_시뮬레이션월드_Unity산출물_시뮬레이션월드_파생실행_파생실행식별번호",
                        column: x => x.파생실행식별번호,
                        principalTable: "시뮬레이션월드_파생실행",
                        principalColumn: "식별번호",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "시뮬레이션월드_Unity타일Manifest",
                columns: table => new
                {
                    식별번호 = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    파생실행식별번호 = table.Column<long>(type: "bigint", nullable: false),
                    타일Manifest고유식별자 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    공간변환고유식별자 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    타일키 = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    타일단계 = table.Column<int>(type: "int", nullable: false),
                    타일크기미터 = table.Column<decimal>(type: "decimal(12,4)", precision: 12, scale: 4, nullable: false),
                    여유영역미터 = table.Column<decimal>(type: "decimal(12,4)", precision: 12, scale: 4, nullable: false),
                    최소동쪽좌표미터 = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    최소북쪽좌표미터 = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    최대동쪽좌표미터 = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    최대북쪽좌표미터 = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    입력지문SHA256 = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ManifestSHA256 = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    생성상태코드 = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_시뮬레이션월드_Unity타일Manifest", x => x.식별번호);
                    table.ForeignKey(
                        name: "FK_시뮬레이션월드_Unity타일Manifest_시뮬레이션월드_파생실행_파생실행식별번호",
                        column: x => x.파생실행식별번호,
                        principalTable: "시뮬레이션월드_파생실행",
                        principalColumn: "식별번호",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_시뮬레이션월드_Unity공간변환Profile_파생실행식별번호_공간변환고유식별자",
                table: "시뮬레이션월드_Unity공간변환Profile",
                columns: new[] { "파생실행식별번호", "공간변환고유식별자" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_시뮬레이션월드_Unity산출물_파생실행식별번호_산출물고유식별자",
                table: "시뮬레이션월드_Unity산출물",
                columns: new[] { "파생실행식별번호", "산출물고유식별자" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_시뮬레이션월드_Unity산출물_파생실행식별번호_타일Manifest고유식별자",
                table: "시뮬레이션월드_Unity산출물",
                columns: new[] { "파생실행식별번호", "타일Manifest고유식별자" });

            migrationBuilder.CreateIndex(
                name: "IX_시뮬레이션월드_Unity타일Manifest_파생실행식별번호_타일키",
                table: "시뮬레이션월드_Unity타일Manifest",
                columns: new[] { "파생실행식별번호", "타일키" });

            migrationBuilder.CreateIndex(
                name: "IX_시뮬레이션월드_Unity타일Manifest_파생실행식별번호_타일Manifest고유식별자",
                table: "시뮬레이션월드_Unity타일Manifest",
                columns: new[] { "파생실행식별번호", "타일Manifest고유식별자" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "시뮬레이션월드_Unity공간변환Profile");

            migrationBuilder.DropTable(
                name: "시뮬레이션월드_Unity산출물");

            migrationBuilder.DropTable(
                name: "시뮬레이션월드_Unity타일Manifest");
        }
    }
}
