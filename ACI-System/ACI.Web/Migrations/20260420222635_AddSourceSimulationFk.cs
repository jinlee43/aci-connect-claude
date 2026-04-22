using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ACI.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddSourceSimulationFk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ScheduleBaselines_SourceSimulationId",
                table: "ScheduleBaselines",
                column: "SourceSimulationId");

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduleBaselines_ScheduleSimulations_SourceSimulationId",
                table: "ScheduleBaselines",
                column: "SourceSimulationId",
                principalTable: "ScheduleSimulations",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScheduleBaselines_ScheduleSimulations_SourceSimulationId",
                table: "ScheduleBaselines");

            migrationBuilder.DropIndex(
                name: "IX_ScheduleBaselines_SourceSimulationId",
                table: "ScheduleBaselines");
        }
    }
}
