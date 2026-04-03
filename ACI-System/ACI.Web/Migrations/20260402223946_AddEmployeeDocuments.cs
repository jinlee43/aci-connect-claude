using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ACI.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmployeeDocuments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmployeeId = table.Column<int>(type: "integer", nullable: false),
                    FileName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    StoredFileName = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    Extension = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    UploadedByName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    LegacyPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedById = table.Column<int>(type: "integer", nullable: true),
                    UpdatedById = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeDocuments_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDocuments_EmployeeId",
                table: "EmployeeDocuments",
                column: "EmployeeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeDocuments");
        }
    }
}
