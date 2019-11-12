using Microsoft.EntityFrameworkCore.Migrations;

namespace BachelorProject.Migrations
{
    public partial class gameTypeAddCommisions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BarmaidCommision",
                table: "GameTypes",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "InstruktorCommision",
                table: "GameTypes",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SoloCommision",
                table: "GameTypes",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BarmaidCommision",
                table: "GameTypes");

            migrationBuilder.DropColumn(
                name: "InstruktorCommision",
                table: "GameTypes");

            migrationBuilder.DropColumn(
                name: "SoloCommision",
                table: "GameTypes");
        }
    }
}
