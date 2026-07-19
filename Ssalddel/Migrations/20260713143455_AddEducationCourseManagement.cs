using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssalddel.Migrations
{
    /// <inheritdoc />
    public partial class AddEducationCourseManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "education_courses",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    course_code = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    course_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "text", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    delivery_mode = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    minimum_months = table.Column<int>(type: "int", nullable: false),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    source_url = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_education_courses", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "education_course_applications",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    course_id = table.Column<long>(type: "bigint", nullable: false),
                    applicant_user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    name_ciphertext = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nickname_ciphertext = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    email_ciphertext = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    phone_ciphertext = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    gender_ciphertext = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    birth_year_ciphertext = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    country_ciphertext = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    membership_confirmed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    entry_pledge_agreed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    personal_data_agreed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    third_party_data_agreed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    personal_data_consent_version = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    third_party_consent_version = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    consented_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    status = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    reviewer_user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    review_note_ciphertext = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    applied_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    reviewed_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    personal_data_deleted_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_education_course_applications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_education_course_applications_education_courses_course_id",
                        column: x => x.course_id,
                        principalTable: "education_courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "education_course_forms",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    course_id = table.Column<long>(type: "bigint", nullable: false),
                    form_code = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    form_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    purpose = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    version = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    submission_cycle = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    minimum_submission_count = table.Column<int>(type: "int", nullable: false),
                    is_required = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    field_definition_json = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    source_url = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_education_course_forms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_education_course_forms_education_courses_course_id",
                        column: x => x.course_id,
                        principalTable: "education_courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "education_course_subjects",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    course_id = table.Column<long>(type: "bigint", nullable: false),
                    subject_code = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    subject_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    display_order = table.Column<int>(type: "int", nullable: false),
                    minimum_attendance_count = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_education_course_subjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_education_course_subjects_education_courses_course_id",
                        column: x => x.course_id,
                        principalTable: "education_courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "education_course_enrollments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    course_id = table.Column<long>(type: "bigint", nullable: false),
                    application_id = table.Column<long>(type: "bigint", nullable: false),
                    participant_user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    mentor_user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    started_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ended_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_education_course_enrollments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_education_course_enrollments_education_course_applications_a~",
                        column: x => x.application_id,
                        principalTable: "education_course_applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_education_course_enrollments_education_courses_course_id",
                        column: x => x.course_id,
                        principalTable: "education_courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "education_course_attendances",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    enrollment_id = table.Column<long>(type: "bigint", nullable: false),
                    subject_id = table.Column<long>(type: "bigint", nullable: false),
                    session_key = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    session_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    session_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    attended = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    recorded_by_user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    recorded_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_education_course_attendances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_education_course_attendances_education_course_enrollments_en~",
                        column: x => x.enrollment_id,
                        principalTable: "education_course_enrollments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_education_course_attendances_education_course_subjects_subje~",
                        column: x => x.subject_id,
                        principalTable: "education_course_subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "education_course_submissions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    enrollment_id = table.Column<long>(type: "bigint", nullable: false),
                    form_id = table.Column<long>(type: "bigint", nullable: false),
                    period_key = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    answers_ciphertext = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    reviewer_user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    review_note_ciphertext = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    submitted_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    reviewed_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_education_course_submissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_education_course_submissions_education_course_enrollments_en~",
                        column: x => x.enrollment_id,
                        principalTable: "education_course_enrollments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_education_course_submissions_education_course_forms_form_id",
                        column: x => x.form_id,
                        principalTable: "education_course_forms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_education_course_applications_course_id_applicant_user_id_st~",
                table: "education_course_applications",
                columns: new[] { "course_id", "applicant_user_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_education_course_applications_status_applied_at_utc",
                table: "education_course_applications",
                columns: new[] { "status", "applied_at_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_education_course_attendances_enrollment_id_subject_id_sessio~",
                table: "education_course_attendances",
                columns: new[] { "enrollment_id", "subject_id", "session_key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_education_course_attendances_subject_id",
                table: "education_course_attendances",
                column: "subject_id");

            migrationBuilder.CreateIndex(
                name: "IX_education_course_enrollments_application_id",
                table: "education_course_enrollments",
                column: "application_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_education_course_enrollments_course_id",
                table: "education_course_enrollments",
                column: "course_id");

            migrationBuilder.CreateIndex(
                name: "IX_education_course_enrollments_participant_user_id_status",
                table: "education_course_enrollments",
                columns: new[] { "participant_user_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_education_course_forms_course_id_form_code",
                table: "education_course_forms",
                columns: new[] { "course_id", "form_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_education_course_subjects_course_id_subject_code",
                table: "education_course_subjects",
                columns: new[] { "course_id", "subject_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_education_course_submissions_enrollment_id_form_id_period_key",
                table: "education_course_submissions",
                columns: new[] { "enrollment_id", "form_id", "period_key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_education_course_submissions_form_id",
                table: "education_course_submissions",
                column: "form_id");

            migrationBuilder.CreateIndex(
                name: "IX_education_course_submissions_status_submitted_at_utc",
                table: "education_course_submissions",
                columns: new[] { "status", "submitted_at_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_education_courses_course_code",
                table: "education_courses",
                column: "course_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_education_courses_is_active_course_name",
                table: "education_courses",
                columns: new[] { "is_active", "course_name" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "education_course_attendances");

            migrationBuilder.DropTable(
                name: "education_course_submissions");

            migrationBuilder.DropTable(
                name: "education_course_subjects");

            migrationBuilder.DropTable(
                name: "education_course_enrollments");

            migrationBuilder.DropTable(
                name: "education_course_forms");

            migrationBuilder.DropTable(
                name: "education_course_applications");

            migrationBuilder.DropTable(
                name: "education_courses");
        }
    }
}
