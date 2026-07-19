using Ssalddel.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssalddel.Migrations
{
    [DbContext(typeof(SsalddelContext))]
    [Migration("20260707142000_AddGroupPurchaseDispatchQueueScope")]
    public partial class AddGroupPurchaseDispatchQueueScope : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "group_purchase_destination_type_code",
                table: "배차_대기",
                type: "longtext",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "group_purchase_driver_unit_distribution",
                table: "배차_대기",
                type: "tinyint(1)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "group_purchase_unit_distribution_mode_code",
                table: "배차_대기",
                type: "longtext",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "group_purchase_unit_delivery_count",
                table: "배차_대기",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "group_purchase_distribution_responsibility_code",
                table: "배차_대기",
                type: "longtext",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "group_purchase_destination_type_code",
                table: "배차_대기");

            migrationBuilder.DropColumn(
                name: "group_purchase_driver_unit_distribution",
                table: "배차_대기");

            migrationBuilder.DropColumn(
                name: "group_purchase_unit_distribution_mode_code",
                table: "배차_대기");

            migrationBuilder.DropColumn(
                name: "group_purchase_unit_delivery_count",
                table: "배차_대기");

            migrationBuilder.DropColumn(
                name: "group_purchase_distribution_responsibility_code",
                table: "배차_대기");
        }
    }
}
