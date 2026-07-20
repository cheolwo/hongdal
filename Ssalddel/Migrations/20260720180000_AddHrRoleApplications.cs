using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssalddel.Migrations
{
    /// <inheritdoc />
    public partial class AddHrRoleApplications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "hr_role_applications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    applicant_user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    participant_category = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    requested_role_code = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    requested_role_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    scope_type = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    scope_id = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status_code = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    submission_request_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    active_application_key = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    confirmed_voluntary_application = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    confirmed_no_role_or_employment_guarantee = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    confirmed_review_data_use = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    consent_version = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    submitted_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    withdrawn_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hr_role_applications", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_hr_role_applications_active_application_key",
                table: "hr_role_applications",
                column: "active_application_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hr_role_applications_applicant_user_id_submission_request_id",
                table: "hr_role_applications",
                columns: new[] { "applicant_user_id", "submission_request_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hr_role_applications_applicant_user_id_submitted_at_utc",
                table: "hr_role_applications",
                columns: new[] { "applicant_user_id", "submitted_at_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_hr_role_applications_status_code_submitted_at_utc",
                table: "hr_role_applications",
                columns: new[] { "status_code", "submitted_at_utc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "hr_role_applications");
        }
    }
}
