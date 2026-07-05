using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using 홍달.Data;

#nullable disable

namespace Hongdal.Migrations
{
    [DbContext(typeof(HongdalContext))]
    [Migration("20260705010045_AddPendingHrAndPlatformTables")]
    /// <inheritdoc />
    public partial class AddPendingHrAndPlatformTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "hr_employment_contracts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    worker_user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    worker_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    employer_scope_type = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    employer_scope_id = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    employer_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    contract_type = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    contract_status = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    contract_start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    contract_end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    work_description = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    wage_type = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    wage_amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    minimum_wage_amount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    minimum_wage_check_passed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    minimum_wage_check_message = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    payment_cycle = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    payment_day_of_month = table.Column<int>(type: "int", nullable: false),
                    payment_method = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    bank_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    account_number = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    account_holder_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    signed_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    signed_by_user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    memo = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hr_employment_contracts", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "hr_role_assignments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    scope_type = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    scope_id = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    participant_category = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    role_code = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    role_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    assigned_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    assigned_by_user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    work_schedule_enabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    time_zone_id = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    allowed_days_of_week = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    work_start_local_time = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    work_end_local_time = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    worksite_ip_restriction_enabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    allowed_worksite_ip_ranges = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hr_role_assignments", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "platform_profit_return_policies",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    policy_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    target_participant_category = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    return_rate_percent = table.Column<decimal>(type: "decimal(9,4)", nullable: false),
                    company_reserve_amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    minimum_profit_threshold = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    effective_start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    effective_end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    memo = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platform_profit_return_policies", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "platform_revenue_entries",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    revenue_source = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    source_reference_type = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    source_reference_id = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    payer_user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    related_participant_user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    gross_amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    platform_revenue_amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    currency_code = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    occurred_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    memo = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platform_revenue_entries", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "hr_payroll_schedules",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    contract_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    worker_user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    employer_scope_type = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    employer_scope_id = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    work_period_start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    work_period_end_date = table.Column<DateOnly>(type: "date", nullable: false),
                    scheduled_payment_date = table.Column<DateOnly>(type: "date", nullable: false),
                    planned_amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    currency_code = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    payment_method = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    memo = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hr_payroll_schedules", x => x.id);
                    table.ForeignKey(
                        name: "FK_hr_payroll_schedules_hr_employment_contracts_contract_id",
                        column: x => x.contract_id,
                        principalTable: "hr_employment_contracts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "platform_profit_return_schedules",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    policy_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    participant_user_id = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    participant_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    participant_category = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    period_start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    period_end_date = table.Column<DateOnly>(type: "date", nullable: false),
                    scheduled_payment_date = table.Column<DateOnly>(type: "date", nullable: false),
                    total_platform_revenue_amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    operating_cost_amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    estimated_profit_amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    return_pool_amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    participant_weight = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    planned_return_amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    status = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    memo = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platform_profit_return_schedules", x => x.id);
                    table.ForeignKey(
                        name: "FK_platform_profit_return_schedules_platform_profit_return_poli~",
                        column: x => x.policy_id,
                        principalTable: "platform_profit_return_policies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_hr_employment_contracts_worker_user_id_employer_scope_type_e~",
                table: "hr_employment_contracts",
                columns: new[] { "worker_user_id", "employer_scope_type", "employer_scope_id", "contract_status" });

            migrationBuilder.CreateIndex(
                name: "IX_hr_payroll_schedules_contract_id",
                table: "hr_payroll_schedules",
                column: "contract_id");

            migrationBuilder.CreateIndex(
                name: "IX_hr_payroll_schedules_worker_user_id_scheduled_payment_date_s~",
                table: "hr_payroll_schedules",
                columns: new[] { "worker_user_id", "scheduled_payment_date", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_hr_role_assignments_user_id_scope_type_scope_id_role_code_is~",
                table: "hr_role_assignments",
                columns: new[] { "user_id", "scope_type", "scope_id", "role_code", "is_active" });

            migrationBuilder.CreateIndex(
                name: "IX_platform_profit_return_policies_target_participant_category_~",
                table: "platform_profit_return_policies",
                columns: new[] { "target_participant_category", "is_active", "effective_start_date" });

            migrationBuilder.CreateIndex(
                name: "IX_platform_profit_return_schedules_participant_user_id_schedul~",
                table: "platform_profit_return_schedules",
                columns: new[] { "participant_user_id", "scheduled_payment_date", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_platform_profit_return_schedules_policy_id",
                table: "platform_profit_return_schedules",
                column: "policy_id");

            migrationBuilder.CreateIndex(
                name: "IX_platform_revenue_entries_revenue_source_occurred_at_utc",
                table: "platform_revenue_entries",
                columns: new[] { "revenue_source", "occurred_at_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_platform_revenue_entries_source_reference_type_source_refere~",
                table: "platform_revenue_entries",
                columns: new[] { "source_reference_type", "source_reference_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "hr_role_assignments");

            migrationBuilder.DropTable(
                name: "platform_profit_return_schedules");

            migrationBuilder.DropTable(
                name: "platform_revenue_entries");

            migrationBuilder.DropTable(
                name: "hr_employment_contracts");

            migrationBuilder.DropTable(
                name: "platform_profit_return_policies");
        }
    }
}
