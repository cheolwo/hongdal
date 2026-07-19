using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssalddel.Migrations
{
    public partial class AddDispatchQueueFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "confirmed_driver_id",
                table: "배차_대기",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "current_recommended_driver_id",
                table: "배차_대기",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "exposure_state",
                table: "배차_대기",
                type: "int",
                nullable: false,
                defaultValue: 100);

            migrationBuilder.AddColumn<string>(
                name: "last_rejected_driver_id",
                table: "배차_대기",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "plan_attempts",
                table: "배차_대기",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "public_transition_at",
                table: "배차_대기",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "queue_stage",
                table: "배차_대기",
                type: "int",
                nullable: false,
                defaultValue: 10);

            migrationBuilder.AddColumn<DateTime>(
                name: "recommendation_expires_at",
                table: "배차_대기",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "recommendation_round",
                table: "배차_대기",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "recommendation_started_at",
                table: "배차_대기",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "row_version",
                table: "배차_대기",
                type: "timestamp(6)",
                rowVersion: true,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "confirmed_driver_id",
                table: "배차_대기");

            migrationBuilder.DropColumn(
                name: "current_recommended_driver_id",
                table: "배차_대기");

            migrationBuilder.DropColumn(
                name: "exposure_state",
                table: "배차_대기");

            migrationBuilder.DropColumn(
                name: "last_rejected_driver_id",
                table: "배차_대기");

            migrationBuilder.DropColumn(
                name: "plan_attempts",
                table: "배차_대기");

            migrationBuilder.DropColumn(
                name: "public_transition_at",
                table: "배차_대기");

            migrationBuilder.DropColumn(
                name: "queue_stage",
                table: "배차_대기");

            migrationBuilder.DropColumn(
                name: "recommendation_expires_at",
                table: "배차_대기");

            migrationBuilder.DropColumn(
                name: "recommendation_round",
                table: "배차_대기");

            migrationBuilder.DropColumn(
                name: "recommendation_started_at",
                table: "배차_대기");

            migrationBuilder.DropColumn(
                name: "row_version",
                table: "배차_대기");
        }
    }
}
