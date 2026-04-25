using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ACI.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddJobPositionType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "JobPositions",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            // 프로젝트 현장 직책에 Type = 'Project' 태깅
            migrationBuilder.Sql(
                "UPDATE \"JobPositions\" SET \"Type\" = 'Project' " +
                "WHERE \"Code\" IN ('SPM','PM','PE','APM','SUPT','SSUPT','ASUPT')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "JobPositions");
        }
    }
}
