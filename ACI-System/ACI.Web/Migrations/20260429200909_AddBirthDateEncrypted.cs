using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ACI.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddBirthDateEncrypted : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BirthDateEncrypted",
                table: "Employees",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BirthDateEncrypted",
                table: "Employees");
        }
    }
}
