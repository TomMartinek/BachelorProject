using Microsoft.EntityFrameworkCore.Migrations;

namespace BachelorProject.Migrations
{
    public partial class voucher_addApplicationUserReference : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "Vouchers",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vouchers_ApplicationUserId",
                table: "Vouchers",
                column: "ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Vouchers_AspNetUsers_ApplicationUserId",
                table: "Vouchers",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vouchers_AspNetUsers_ApplicationUserId",
                table: "Vouchers");

            migrationBuilder.DropIndex(
                name: "IX_Vouchers_ApplicationUserId",
                table: "Vouchers");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "Vouchers");
        }
    }
}
