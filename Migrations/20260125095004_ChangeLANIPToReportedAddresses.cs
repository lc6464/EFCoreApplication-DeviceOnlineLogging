using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EFCoreApplication.Migrations
{
    /// <inheritdoc />
    public partial class ChangeLANIPToReportedAddresses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LANIPAddress",
                table: "OnlineLogs",
                newName: "ReportedAddresses");

            migrationBuilder.RenameColumn(
                name: "LatestLANIPAddress",
                table: "Devices",
                newName: "LatestReportedAddresses");

            string upSqlLogs = "UPDATE \"OnlineLogs\" SET \"ReportedAddresses\" = '[\"' || \"ReportedAddresses\" || '\"]' WHERE \"ReportedAddresses\" IS NOT NULL AND \"ReportedAddresses\" NOT LIKE '[%';";
            string upSqlDevices = "UPDATE \"Devices\" SET \"LatestReportedAddresses\" = '[\"' || \"LatestReportedAddresses\" || '\"]' WHERE \"LatestReportedAddresses\" IS NOT NULL AND \"LatestReportedAddresses\" NOT LIKE '[%';";

            migrationBuilder.Sql(upSqlLogs);
            migrationBuilder.Sql(upSqlDevices);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ReportedAddresses",
                table: "OnlineLogs",
                newName: "LANIPAddress");

            migrationBuilder.RenameColumn(
                name: "LatestReportedAddresses",
                table: "Devices",
                newName: "LatestLANIPAddress");

            // SQLite 使用 json_extract
            string downSqlLogs = "UPDATE \"OnlineLogs\" SET \"ReportedAddresses\" = json_extract(\"ReportedAddresses\", '$[0]') WHERE \"ReportedAddresses\" LIKE '[%';";
            string downSqlDevices = "UPDATE \"Devices\" SET \"LatestReportedAddresses\" = json_extract(\"LatestReportedAddresses\", '$[0]') WHERE \"LatestReportedAddresses\" LIKE '[%';";

            migrationBuilder.Sql(downSqlLogs);
            migrationBuilder.Sql(downSqlDevices);
        }
    }
}
