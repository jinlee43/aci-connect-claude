using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ACI.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeePrivateFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmgContact1Cell",
                table: "Employees",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmgContact1Email",
                table: "Employees",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmgContact1Name",
                table: "Employees",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmgContact1Relation",
                table: "Employees",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmgContact1Tel",
                table: "Employees",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmgContact2Cell",
                table: "Employees",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmgContact2Email",
                table: "Employees",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmgContact2Name",
                table: "Employees",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmgContact2Relation",
                table: "Employees",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmgContact2Tel",
                table: "Employees",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmgContact3Cell",
                table: "Employees",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmgContact3Email",
                table: "Employees",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmgContact3Name",
                table: "Employees",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmgContact3Relation",
                table: "Employees",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmgContact3Tel",
                table: "Employees",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HomeAddress1",
                table: "Employees",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HomeAddress2",
                table: "Employees",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HomeAddressCity",
                table: "Employees",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HomeAddressCounty",
                table: "Employees",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HomeAddressState",
                table: "Employees",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HomeAddressZip",
                table: "Employees",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameSuffix",
                table: "Employees",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrivCellPhone",
                table: "Employees",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrivHomePhone",
                table: "Employees",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmgContact1Cell",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "EmgContact1Email",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "EmgContact1Name",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "EmgContact1Relation",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "EmgContact1Tel",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "EmgContact2Cell",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "EmgContact2Email",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "EmgContact2Name",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "EmgContact2Relation",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "EmgContact2Tel",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "EmgContact3Cell",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "EmgContact3Email",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "EmgContact3Name",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "EmgContact3Relation",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "EmgContact3Tel",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "HomeAddress1",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "HomeAddress2",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "HomeAddressCity",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "HomeAddressCounty",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "HomeAddressState",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "HomeAddressZip",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "NameSuffix",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "PrivCellPhone",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "PrivHomePhone",
                table: "Employees");
        }
    }
}
