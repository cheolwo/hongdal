using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssalddel.Migrations
{
    /// <inheritdoc />
    public partial class AddFarmOperationsWorldProjection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "농장",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    stable_id = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    소유자_user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    농장명 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    운영상태_code = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    revision = table.Column<long>(type: "bigint", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_농장", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "농장구획",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    농장_id = table.Column<long>(type: "bigint", nullable: false),
                    stable_id = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    구획명 = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    토양관리_profile_code = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    revision = table.Column<long>(type: "bigint", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_농장구획", x => x.id);
                    table.ForeignKey(
                        name: "FK_농장구획_농장_농장_id",
                        column: x => x.농장_id,
                        principalTable: "농장",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "농업센서",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    농장구획_id = table.Column<long>(type: "bigint", nullable: false),
                    stable_id = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    센서유형_code = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    상태_code = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    revision = table.Column<long>(type: "bigint", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_농업센서", x => x.id);
                    table.ForeignKey(
                        name: "FK_농업센서_농장구획_농장구획_id",
                        column: x => x.농장구획_id,
                        principalTable: "농장구획",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "농장작업",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    농장_id = table.Column<long>(type: "bigint", nullable: false),
                    농장구획_id = table.Column<long>(type: "bigint", nullable: true),
                    stable_id = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    npc_stable_id = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    작업유형_code = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    route_code = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    current_waypoint_key = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    destination_waypoint_key = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    movement_state_code = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    arrival_action_code = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    revision = table.Column<long>(type: "bigint", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_농장작업", x => x.id);
                    table.ForeignKey(
                        name: "FK_농장작업_농장_농장_id",
                        column: x => x.농장_id,
                        principalTable: "농장",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_농장작업_농장구획_농장구획_id",
                        column: x => x.농장구획_id,
                        principalTable: "농장구획",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "재배작기",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    농장구획_id = table.Column<long>(type: "bigint", nullable: false),
                    stable_id = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    작물명 = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    작물기준_stable_id = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    작물기준_source_key = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    생육상태_code = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    파종일 = table.Column<DateOnly>(type: "date", nullable: true),
                    예상수확일 = table.Column<DateOnly>(type: "date", nullable: true),
                    revision = table.Column<long>(type: "bigint", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_재배작기", x => x.id);
                    table.ForeignKey(
                        name: "FK_재배작기_농장구획_농장구획_id",
                        column: x => x.농장구획_id,
                        principalTable: "농장구획",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "농업센서관측",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    농업센서_id = table.Column<long>(type: "bigint", nullable: false),
                    관측값 = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    단위_code = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    관측시각_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    최신성상태_code = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    판정상태_code = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    판정규칙_revision = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    근거카드_id = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    확신도_code = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    판정한계 = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_농업센서관측", x => x.id);
                    table.ForeignKey(
                        name: "FK_농업센서관측_농업센서_농업센서_id",
                        column: x => x.농업센서_id,
                        principalTable: "농업센서",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_농업센서_농장구획_id_상태_code",
                table: "농업센서",
                columns: new[] { "농장구획_id", "상태_code" });

            migrationBuilder.CreateIndex(
                name: "IX_농업센서_stable_id",
                table: "농업센서",
                column: "stable_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_농업센서관측_농업센서_id_관측시각_utc",
                table: "농업센서관측",
                columns: new[] { "농업센서_id", "관측시각_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_농장_소유자_user_id_운영상태_code",
                table: "농장",
                columns: new[] { "소유자_user_id", "운영상태_code" });

            migrationBuilder.CreateIndex(
                name: "IX_농장_stable_id",
                table: "농장",
                column: "stable_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_농장구획_농장_id_구획명",
                table: "농장구획",
                columns: new[] { "농장_id", "구획명" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_농장구획_stable_id",
                table: "농장구획",
                column: "stable_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_농장작업_농장_id_npc_stable_id",
                table: "농장작업",
                columns: new[] { "농장_id", "npc_stable_id" });

            migrationBuilder.CreateIndex(
                name: "IX_농장작업_농장구획_id",
                table: "농장작업",
                column: "농장구획_id");

            migrationBuilder.CreateIndex(
                name: "IX_농장작업_stable_id",
                table: "농장작업",
                column: "stable_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_재배작기_농장구획_id_생육상태_code",
                table: "재배작기",
                columns: new[] { "농장구획_id", "생육상태_code" });

            migrationBuilder.CreateIndex(
                name: "IX_재배작기_stable_id",
                table: "재배작기",
                column: "stable_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "농업센서관측");

            migrationBuilder.DropTable(
                name: "농장작업");

            migrationBuilder.DropTable(
                name: "재배작기");

            migrationBuilder.DropTable(
                name: "농업센서");

            migrationBuilder.DropTable(
                name: "농장구획");

            migrationBuilder.DropTable(
                name: "농장");
        }
    }
}
