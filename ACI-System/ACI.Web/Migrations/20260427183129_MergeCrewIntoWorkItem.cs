using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ACI.Web.Migrations
{
    /// <inheritdoc />
    public partial class MergeCrewIntoWorkItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Location",
                table: "DailyReportWorkItems",
                newName: "CompanyName");

            migrationBuilder.AddColumn<string>(
                name: "Area",
                table: "DailyReportWorkItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "WorkerHours",
                table: "DailyReportWorkItems",
                type: "numeric",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Area",
                table: "DailyReportWorkItems");

            migrationBuilder.DropColumn(
                name: "WorkerHours",
                table: "DailyReportWorkItems");

            migrationBuilder.RenameColumn(
                name: "CompanyName",
                table: "DailyReportWorkItems",
                newName: "Location");
        }
    }
}
