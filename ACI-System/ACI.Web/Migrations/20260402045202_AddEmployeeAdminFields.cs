using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ACI.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeAdminFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "AlienCardExpirationDate",
                table: "Employees",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "AlienCardIssuedDate",
                table: "Employees",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AlienNumberEncrypted",
                table: "Employees",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "ApplyDate",
                table: "Employees",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "BackgroudCheckOk",
                table: "Employees",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateOnly>(
                name: "BkgrndCheckDate",
                table: "Employees",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "DentalEndDate",
                table: "Employees",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "DentalStartDate",
                table: "Employees",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "DriversLicExpiration",
                table: "Employees",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "DriversLicIssuedDate",
                table: "Employees",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DriversLicNumEncrypted",
                table: "Employees",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "DrugScreeningDate",
                table: "Employees",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "DrugScreeningOk",
                table: "Employees",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateOnly>(
                name: "Eligible401kDate",
                table: "Employees",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "Enrolled401kDate",
                table: "Employees",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "HealthEndDate",
                table: "Employees",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "HealthStartDate",
                table: "Employees",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsReEmp",
                table: "Employees",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateOnly>(
                name: "OldStartDate",
                table: "Employees",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "PassportExpiredDate",
                table: "Employees",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "PassportIssedDate",
                table: "Employees",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PassportNumberEncrypted",
                table: "Employees",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SsnEncrypted",
                table: "Employees",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StatusName",
                table: "Employees",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TerminationType",
                table: "Employees",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TinEncrypted",
                table: "Employees",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "VisionEndDate",
                table: "Employees",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "VisionStartDate",
                table: "Employees",
                type: "date",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AlienCardExpirationDate",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "AlienCardIssuedDate",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "AlienNumberEncrypted",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "ApplyDate",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "BackgroudCheckOk",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "BkgrndCheckDate",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "DentalEndDate",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "DentalStartDate",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "DriversLicExpiration",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "DriversLicIssuedDate",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "DriversLicNumEncrypted",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "DrugScreeningDate",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "DrugScreeningOk",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "Eligible401kDate",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "Enrolled401kDate",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "HealthEndDate",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "HealthStartDate",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "IsReEmp",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "OldStartDate",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "PassportExpiredDate",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "PassportIssedDate",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "PassportNumberEncrypted",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "SsnEncrypted",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "StatusName",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "TerminationType",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "TinEncrypted",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "VisionEndDate",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "VisionStartDate",
                table: "Employees");
        }
    }
}
