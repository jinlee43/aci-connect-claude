using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ACI.Web.Migrations
{
    /// <inheritdoc />
    public partial class RefactorOwnerContacts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OwnerEmail",
                table: "Projects",
                newName: "OwnerContact2Name");

            migrationBuilder.RenameColumn(
                name: "OwnerContact",
                table: "Projects",
                newName: "OwnerContact2Email");

            migrationBuilder.AddColumn<string>(
                name: "OwnerAddress2",
                table: "Projects",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwnerContact1Email",
                table: "Projects",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwnerContact1Name",
                table: "Projects",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwnerContact1Phone",
                table: "Projects",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwnerContact1Title",
                table: "Projects",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwnerContact2Phone",
                table: "Projects",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwnerContact2Title",
                table: "Projects",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwnerPhone",
                table: "Projects",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OwnerAddress2",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "OwnerContact1Email",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "OwnerContact1Name",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "OwnerContact1Phone",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "OwnerContact1Title",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "OwnerContact2Phone",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "OwnerContact2Title",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "OwnerPhone",
                table: "Projects");

            migrationBuilder.RenameColumn(
                name: "OwnerContact2Name",
                table: "Projects",
                newName: "OwnerEmail");

            migrationBuilder.RenameColumn(
                name: "OwnerContact2Email",
                table: "Projects",
                newName: "OwnerContact");
        }
    }
}
