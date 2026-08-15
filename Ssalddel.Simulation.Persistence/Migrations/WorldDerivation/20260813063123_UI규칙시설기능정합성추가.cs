using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssalddel.Simulation.Persistence.Migrations.WorldDerivation
{
    /// <inheritdoc />
    public partial class UI규칙시설기능정합성추가 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "시설기능코드",
                table: "시뮬레이션월드_UI업무규칙연결",
                type: "varchar(80)",
                maxLength: 80,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "원본객체업무규칙연결고유식별자",
                table: "시뮬레이션월드_UI업무규칙연결",
                type: "varchar(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.Sql(
                """
                UPDATE `시뮬레이션월드_UI업무규칙연결` AS `ui`
                INNER JOIN `시뮬레이션월드_UI화면영역기획` AS `surface`
                    ON `surface`.`UI기획대장식별번호` = `ui`.`UI기획대장식별번호`
                    AND `surface`.`화면영역고유식별자` = `ui`.`화면영역고유식별자`
                INNER JOIN `시뮬레이션월드_UI기획대장` AS `ui_catalog`
                    ON `ui_catalog`.`식별번호` = `ui`.`UI기획대장식별번호`
                INNER JOIN `시뮬레이션월드_업무Simulation규칙대장` AS `rule_catalog`
                    ON `rule_catalog`.`규칙대장개정번호` = `ui_catalog`.`업무규칙대장개정번호`
                INNER JOIN `시뮬레이션월드_객체업무규칙연결` AS `source_binding`
                    ON `source_binding`.`규칙대장식별번호` = `rule_catalog`.`식별번호`
                    AND `source_binding`.`시설고유식별자` = `surface`.`시설고유식별자`
                    AND `source_binding`.`규칙고유식별자` = `ui`.`업무규칙고유식별자`
                    AND `source_binding`.`규칙개정번호` = `ui`.`업무규칙개정번호`
                SET `ui`.`원본객체업무규칙연결고유식별자` = `source_binding`.`연결고유식별자`,
                    `ui`.`시설기능코드` = `source_binding`.`기능코드`;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_시뮬레이션월드_UI업무규칙연결_UI기획대장식별번호_원본객체업무규칙연결고유식별자",
                table: "시뮬레이션월드_UI업무규칙연결",
                columns: new[] { "UI기획대장식별번호", "원본객체업무규칙연결고유식별자" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_시뮬레이션월드_UI업무규칙연결_UI기획대장식별번호_원본객체업무규칙연결고유식별자",
                table: "시뮬레이션월드_UI업무규칙연결");

            migrationBuilder.DropColumn(
                name: "시설기능코드",
                table: "시뮬레이션월드_UI업무규칙연결");

            migrationBuilder.DropColumn(
                name: "원본객체업무규칙연결고유식별자",
                table: "시뮬레이션월드_UI업무규칙연결");
        }
    }
}
