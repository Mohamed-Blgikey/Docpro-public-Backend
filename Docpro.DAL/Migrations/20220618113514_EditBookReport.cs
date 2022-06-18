using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Docpro.DAL.Migrations
{
    public partial class EditBookReport : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Diagnosis",
                table: "Reports",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "treatment",
                table: "Reports",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Diagnosis",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "treatment",
                table: "Reports");
        }
    }
}
