using Microsoft.EntityFrameworkCore.Migrations;

namespace BachelorProject.Migrations
{
    public partial class voucher_ValueColumnMandatory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Value",
                table: "Vouchers",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Value",
                table: "Vouchers",
                nullable: true,
                oldClrType: typeof(int));
        }
    }
}
