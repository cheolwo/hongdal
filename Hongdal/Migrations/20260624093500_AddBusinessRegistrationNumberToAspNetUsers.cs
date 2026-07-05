using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using 홍달.Data;

#nullable disable

namespace Hongdal.Migrations
{
    [DbContext(typeof(HongdalContext))]
    [Migration("20260624093500_AddBusinessRegistrationNumberToAspNetUsers")]
    public partial class AddBusinessRegistrationNumberToAspNetUsers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BusinessRegistrationNumber",
                table: "AspNetUsers",
                type: "longtext",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BusinessRegistrationNumber",
                table: "AspNetUsers");
        }
    }
}