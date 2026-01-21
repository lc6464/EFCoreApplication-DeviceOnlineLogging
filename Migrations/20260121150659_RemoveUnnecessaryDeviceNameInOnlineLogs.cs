using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EFCoreApplication.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUnnecessaryDeviceNameInOnlineLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeviceName",
                table: "OnlineLogs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeviceName",
                table: "OnlineLogs",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
