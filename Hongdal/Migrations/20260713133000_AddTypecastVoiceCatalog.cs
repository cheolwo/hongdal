using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hongdal.Migrations
{
    public partial class AddTypecastVoiceCatalog : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "typecast_voices",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    voice_id = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    gender = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    age_group = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    voice_type = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    last_synced_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_typecast_voices", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "typecast_voice_models",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    typecast_voice_id = table.Column<long>(type: "bigint", nullable: false),
                    model_version = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    emotions_json = table.Column<string>(type: "text", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_typecast_voice_models", x => x.Id);
                    table.ForeignKey(
                        name: "FK_typecast_voice_models_typecast_voices_typecast_voice_id",
                        column: x => x.typecast_voice_id,
                        principalTable: "typecast_voices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "typecast_voice_use_cases",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    typecast_voice_id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_typecast_voice_use_cases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_typecast_voice_use_cases_typecast_voices_typecast_voice_id",
                        column: x => x.typecast_voice_id,
                        principalTable: "typecast_voices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_typecast_voice_models_model_version",
                table: "typecast_voice_models",
                column: "model_version");

            migrationBuilder.CreateIndex(
                name: "IX_typecast_voice_models_typecast_voice_id_model_version",
                table: "typecast_voice_models",
                columns: new[] { "typecast_voice_id", "model_version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_typecast_voice_use_cases_name",
                table: "typecast_voice_use_cases",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "IX_typecast_voice_use_cases_typecast_voice_id_name",
                table: "typecast_voice_use_cases",
                columns: new[] { "typecast_voice_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_typecast_voices_is_active_voice_type_gender_age_group",
                table: "typecast_voices",
                columns: new[] { "is_active", "voice_type", "gender", "age_group" });

            migrationBuilder.CreateIndex(
                name: "IX_typecast_voices_voice_id",
                table: "typecast_voices",
                column: "voice_id",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "typecast_voice_models");
            migrationBuilder.DropTable(name: "typecast_voice_use_cases");
            migrationBuilder.DropTable(name: "typecast_voices");
        }
    }
}
