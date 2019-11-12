using Microsoft.EntityFrameworkCore.Migrations;

namespace BachelorProject.Migrations
{
    public partial class additionalGame_addApplicationUserReference : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "AdditionalGames",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AdditionalGames_ApplicationUserId",
                table: "AdditionalGames",
                column: "ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AdditionalGames_AspNetUsers_ApplicationUserId",
                table: "AdditionalGames",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdditionalGames_AspNetUsers_ApplicationUserId",
                table: "AdditionalGames");

            migrationBuilder.DropIndex(
                name: "IX_AdditionalGames_ApplicationUserId",
                table: "AdditionalGames");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "AdditionalGames");
        }
    }
}
