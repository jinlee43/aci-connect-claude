using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ACI.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkItemTimeTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeOnly>(
                name: "EndTime",
                table: "DailyReportWorkItems",
                type: "time without time zone",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "StartTime",
                table: "DailyReportWorkItems",
                type: "time without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TradeText",
                table: "DailyReportWorkItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WorkerCount",
                table: "DailyReportWorkItems",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "DailyReportWorkItems");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "DailyReportWorkItems");

            migrationBuilder.DropColumn(
                name: "TradeText",
                table: "DailyReportWorkItems");

            migrationBuilder.DropColumn(
                name: "WorkerCount",
                table: "DailyReportWorkItems");
        }
    }
}
