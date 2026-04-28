using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ACI.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddDailyReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DailyReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProjectId = table.Column<int>(type: "integer", nullable: false),
                    ReportNumber = table.Column<int>(type: "integer", nullable: false),
                    ReportDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Location = table.Column<string>(type: "text", nullable: true),
                    WeatherCondition = table.Column<string>(type: "text", nullable: true),
                    TempHigh = table.Column<int>(type: "integer", nullable: true),
                    TempLow = table.Column<int>(type: "integer", nullable: true),
                    IsWindy = table.Column<bool>(type: "boolean", nullable: false),
                    IsRainy = table.Column<bool>(type: "boolean", nullable: false),
                    WeatherNotes = table.Column<string>(type: "text", nullable: true),
                    IsNoWork = table.Column<bool>(type: "boolean", nullable: false),
                    NoWorkReason = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    AuthoredById = table.Column<int>(type: "integer", nullable: true),
                    AuthoredByName = table.Column<string>(type: "text", nullable: true),
                    AuthoredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReviewedById = table.Column<int>(type: "integer", nullable: true),
                    ReviewedByName = table.Column<string>(type: "text", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReviewNotes = table.Column<string>(type: "text", nullable: true),
                    ApprovedById = table.Column<int>(type: "integer", nullable: true),
                    ApprovedByName = table.Column<string>(type: "text", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovalNotes = table.Column<string>(type: "text", nullable: true),
                    VoidedById = table.Column<int>(type: "integer", nullable: true),
                    VoidedByName = table.Column<string>(type: "text", nullable: true),
                    VoidedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VoidReason = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedById = table.Column<int>(type: "integer", nullable: true),
                    UpdatedById = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailyReports_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DailyReports_Users_ApprovedById",
                        column: x => x.ApprovedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DailyReports_Users_AuthoredById",
                        column: x => x.AuthoredById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DailyReports_Users_ReviewedById",
                        column: x => x.ReviewedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DailyReports_Users_VoidedById",
                        column: x => x.VoidedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "DailyReportCrewEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DailyReportId = table.Column<int>(type: "integer", nullable: false),
                    CompanyName = table.Column<string>(type: "text", nullable: true),
                    TradeId = table.Column<int>(type: "integer", nullable: true),
                    CraftType = table.Column<string>(type: "text", nullable: true),
                    WorkerCount = table.Column<int>(type: "integer", nullable: false),
                    HoursWorked = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedById = table.Column<int>(type: "integer", nullable: true),
                    UpdatedById = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyReportCrewEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailyReportCrewEntries_DailyReports_DailyReportId",
                        column: x => x.DailyReportId,
                        principalTable: "DailyReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DailyReportCrewEntries_Trades_TradeId",
                        column: x => x.TradeId,
                        principalTable: "Trades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "DailyReportEquipment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DailyReportId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    EquipmentTag = table.Column<string>(type: "text", nullable: true),
                    HoursUsed = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedById = table.Column<int>(type: "integer", nullable: true),
                    UpdatedById = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyReportEquipment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailyReportEquipment_DailyReports_DailyReportId",
                        column: x => x.DailyReportId,
                        principalTable: "DailyReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DailyReportFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DailyReportId = table.Column<int>(type: "integer", nullable: false),
                    FileType = table.Column<int>(type: "integer", nullable: false),
                    FileName = table.Column<string>(type: "text", nullable: false),
                    StoredFileName = table.Column<string>(type: "text", nullable: false),
                    Extension = table.Column<string>(type: "text", nullable: true),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    Caption = table.Column<string>(type: "text", nullable: true),
                    UploadedById = table.Column<int>(type: "integer", nullable: true),
                    UploadedByName = table.Column<string>(type: "text", nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedById = table.Column<int>(type: "integer", nullable: true),
                    UpdatedById = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyReportFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailyReportFiles_DailyReports_DailyReportId",
                        column: x => x.DailyReportId,
                        principalTable: "DailyReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DailyReportFiles_Users_UploadedById",
                        column: x => x.UploadedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "DailyReportTaskProgress",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DailyReportId = table.Column<int>(type: "integer", nullable: false),
                    WorkingTaskId = table.Column<int>(type: "integer", nullable: true),
                    TaskText = table.Column<string>(type: "text", nullable: true),
                    WbsCode = table.Column<string>(type: "text", nullable: true),
                    Location = table.Column<string>(type: "text", nullable: true),
                    ProgressBefore = table.Column<double>(type: "double precision", nullable: false),
                    ProgressAfter = table.Column<double>(type: "double precision", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedById = table.Column<int>(type: "integer", nullable: true),
                    UpdatedById = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyReportTaskProgress", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailyReportTaskProgress_DailyReports_DailyReportId",
                        column: x => x.DailyReportId,
                        principalTable: "DailyReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DailyReportTaskProgress_WorkingTasks_WorkingTaskId",
                        column: x => x.WorkingTaskId,
                        principalTable: "WorkingTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "DailyReportWorkItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DailyReportId = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Location = table.Column<string>(type: "text", nullable: true),
                    TradeId = table.Column<int>(type: "integer", nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedById = table.Column<int>(type: "integer", nullable: true),
                    UpdatedById = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyReportWorkItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailyReportWorkItems_DailyReports_DailyReportId",
                        column: x => x.DailyReportId,
                        principalTable: "DailyReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DailyReportWorkItems_Trades_TradeId",
                        column: x => x.TradeId,
                        principalTable: "Trades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DailyReportCrewEntries_DailyReportId",
                table: "DailyReportCrewEntries",
                column: "DailyReportId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyReportCrewEntries_TradeId",
                table: "DailyReportCrewEntries",
                column: "TradeId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyReportEquipment_DailyReportId",
                table: "DailyReportEquipment",
                column: "DailyReportId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyReportFiles_DailyReportId",
                table: "DailyReportFiles",
                column: "DailyReportId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyReportFiles_UploadedById",
                table: "DailyReportFiles",
                column: "UploadedById");

            migrationBuilder.CreateIndex(
                name: "IX_DailyReports_ApprovedById",
                table: "DailyReports",
                column: "ApprovedById");

            migrationBuilder.CreateIndex(
                name: "IX_DailyReports_AuthoredById",
                table: "DailyReports",
                column: "AuthoredById");

            migrationBuilder.CreateIndex(
                name: "IX_DailyReports_ProjectId",
                table: "DailyReports",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyReports_ProjectId_ReportDate",
                table: "DailyReports",
                columns: new[] { "ProjectId", "ReportDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DailyReports_ProjectId_ReportNumber",
                table: "DailyReports",
                columns: new[] { "ProjectId", "ReportNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DailyReports_ReportDate",
                table: "DailyReports",
                column: "ReportDate");

            migrationBuilder.CreateIndex(
                name: "IX_DailyReports_ReviewedById",
                table: "DailyReports",
                column: "ReviewedById");

            migrationBuilder.CreateIndex(
                name: "IX_DailyReports_VoidedById",
                table: "DailyReports",
                column: "VoidedById");

            migrationBuilder.CreateIndex(
                name: "IX_DailyReportTaskProgress_DailyReportId",
                table: "DailyReportTaskProgress",
                column: "DailyReportId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyReportTaskProgress_WorkingTaskId",
                table: "DailyReportTaskProgress",
                column: "WorkingTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyReportWorkItems_DailyReportId",
                table: "DailyReportWorkItems",
                column: "DailyReportId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyReportWorkItems_TradeId",
                table: "DailyReportWorkItems",
                column: "TradeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DailyReportCrewEntries");

            migrationBuilder.DropTable(
                name: "DailyReportEquipment");

            migrationBuilder.DropTable(
                name: "DailyReportFiles");

            migrationBuilder.DropTable(
                name: "DailyReportTaskProgress");

            migrationBuilder.DropTable(
                name: "DailyReportWorkItems");

            migrationBuilder.DropTable(
                name: "DailyReports");
        }
    }
}
