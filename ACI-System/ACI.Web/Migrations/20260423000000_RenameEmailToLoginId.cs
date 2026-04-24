using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ACI.Web.Migrations
{
    /// <inheritdoc />
    public partial class RenameEmailToLoginId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 기존 unique index 삭제
            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                table: "Users");

            // 기존 Email 값의 도메인 부분을 angelescontractor.com 으로 통일
            // (예: "leej@oldomain.com" → "leej@angelescontractor.com")
            migrationBuilder.Sql(
                "UPDATE \"Users\" SET \"Email\" = SPLIT_PART(\"Email\", '@', 1) || '@angelescontractor.com'");

            // 컬럼 길이 200 → 100 으로 축소
            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            // unique index 재생성
            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                table: "Users");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }
    }
}
