using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ACI.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddProgressSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ScheduleRevisions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProjectId = table.Column<int>(type: "integer", nullable: false),
                    RevisionNumber = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    RevisionType = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ChangeOrderRef = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DataDate = table.Column<DateOnly>(type: "date", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedByName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    ApprovalNotes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SubmittedById = table.Column<int>(type: "integer", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedById = table.Column<int>(type: "integer", nullable: true),
                    UpdatedById = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleRevisions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduleRevisions_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ScheduleRevisions_Users_SubmittedById",
                        column: x => x.SubmittedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "WorkingTasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProjectId = table.Column<int>(type: "integer", nullable: false),
                    BaselineTaskId = table.Column<int>(type: "integer", nullable: true),
                    WbsCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Text = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Location = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    TaskType = table.Column<int>(type: "integer", nullable: false),
                    ParentId = table.Column<int>(type: "integer", nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsOpen = table.Column<bool>(type: "boolean", nullable: false),
                    TradeId = table.Column<int>(type: "integer", nullable: true),
                    AssignedToId = table.Column<int>(type: "integer", nullable: true),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Duration = table.Column<int>(type: "integer", nullable: false),
                    Progress = table.Column<double>(type: "double precision", precision: 5, scale: 4, nullable: false),
                    ActualStartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ActualEndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CompletedDate = table.Column<DateOnly>(type: "date", nullable: true),
                    IsDone = table.Column<bool>(type: "boolean", nullable: false),
                    ConstraintType = table.Column<int>(type: "integer", nullable: true),
                    ConstraintDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Color = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    CrewSize = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    WorkingStatus = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedById = table.Column<int>(type: "integer", nullable: true),
                    UpdatedById = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkingTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkingTasks_Employees_AssignedToId",
                        column: x => x.AssignedToId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_WorkingTasks_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkingTasks_ScheduleTasks_BaselineTaskId",
                        column: x => x.BaselineTaskId,
                        principalTable: "ScheduleTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_WorkingTasks_Trades_TradeId",
                        column: x => x.TradeId,
                        principalTable: "Trades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_WorkingTasks_WorkingTasks_ParentId",
                        column: x => x.ParentId,
                        principalTable: "WorkingTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RevisionDocuments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RevisionId = table.Column<int>(type: "integer", nullable: false),
                    DocumentType = table.Column<int>(type: "integer", nullable: false),
                    FileName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    StoredFileName = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    Extension = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    UploadedById = table.Column<int>(type: "integer", nullable: true),
                    UploadedByName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedById = table.Column<int>(type: "integer", nullable: true),
                    UpdatedById = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RevisionDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RevisionDocuments_ScheduleRevisions_RevisionId",
                        column: x => x.RevisionId,
                        principalTable: "ScheduleRevisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RevisionDocuments_Users_UploadedById",
                        column: x => x.UploadedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ScheduleChanges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RevisionId = table.Column<int>(type: "integer", nullable: false),
                    WorkingTaskId = table.Column<int>(type: "integer", nullable: false),
                    ChangeType = table.Column<int>(type: "integer", nullable: false),
                    OldStartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    OldEndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    OldDuration = table.Column<int>(type: "integer", nullable: true),
                    OldProgress = table.Column<double>(type: "double precision", nullable: true),
                    OldText = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    NewStartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    NewEndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    NewDuration = table.Column<int>(type: "integer", nullable: true),
                    NewProgress = table.Column<double>(type: "double precision", nullable: true),
                    NewText = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    DaysShifted = table.Column<int>(type: "integer", nullable: true),
                    ChangeNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ChangedById = table.Column<int>(type: "integer", nullable: true),
                    ChangedByName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleChanges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduleChanges_ScheduleRevisions_RevisionId",
                        column: x => x.RevisionId,
                        principalTable: "ScheduleRevisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ScheduleChanges_Users_ChangedById",
                        column: x => x.ChangedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ScheduleChanges_WorkingTasks_WorkingTaskId",
                        column: x => x.WorkingTaskId,
                        principalTable: "WorkingTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RevisionDocuments_RevisionId",
                table: "RevisionDocuments",
                column: "RevisionId");

            migrationBuilder.CreateIndex(
                name: "IX_RevisionDocuments_UploadedById",
                table: "RevisionDocuments",
                column: "UploadedById");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleChanges_ChangedById",
                table: "ScheduleChanges",
                column: "ChangedById");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleChanges_RevisionId",
                table: "ScheduleChanges",
                column: "RevisionId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleChanges_WorkingTaskId",
                table: "ScheduleChanges",
                column: "WorkingTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleRevisions_ProjectId_RevisionNumber",
                table: "ScheduleRevisions",
                columns: new[] { "ProjectId", "RevisionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleRevisions_SubmittedById",
                table: "ScheduleRevisions",
                column: "SubmittedById");

            migrationBuilder.CreateIndex(
                name: "IX_WorkingTasks_AssignedToId",
                table: "WorkingTasks",
                column: "AssignedToId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkingTasks_BaselineTaskId",
                table: "WorkingTasks",
                column: "BaselineTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkingTasks_ParentId",
                table: "WorkingTasks",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkingTasks_ProjectId",
                table: "WorkingTasks",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkingTasks_TradeId",
                table: "WorkingTasks",
                column: "TradeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RevisionDocuments");

            migrationBuilder.DropTable(
                name: "ScheduleChanges");

            migrationBuilder.DropTable(
                name: "ScheduleRevisions");

            migrationBuilder.DropTable(
                name: "WorkingTasks");
        }
    }
}
