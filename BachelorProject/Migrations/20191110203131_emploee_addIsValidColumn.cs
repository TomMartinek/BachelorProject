using Microsoft.EntityFrameworkCore.Migrations;

namespace BachelorProject.Migrations
{
    public partial class emploee_addIsValidColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsValid",
                table: "Employees",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsValid",
                table: "Employees");
        }
    }
}
