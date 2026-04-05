-- ============================================================================
-- Migration: Add Baselines (multi-version) + Simulations (What-If)
-- Date: 2026-04-04
-- Description:
--   1. ScheduleBaselines — Procore-style multi-version baseline management
--   2. BaselineTaskSnapshots — Frozen task data per baseline version
--   3. ScheduleSimulations — What-If scenario headers
--   4. SimulationTasks — Task overrides within simulations
--   5. ScheduleRevisions.BaselineId — link revisions to baseline versions
-- ============================================================================

BEGIN;

-- ─── 1. ScheduleBaselines ────────────────────────────────────────────────────

CREATE TABLE IF NOT EXISTS "ScheduleBaselines" (
    "Id"               SERIAL PRIMARY KEY,
    "ProjectId"        INTEGER NOT NULL REFERENCES "Projects"("Id") ON DELETE CASCADE,
    "VersionNumber"    INTEGER NOT NULL,
    "Title"            VARCHAR(200) NOT NULL,
    "Description"      VARCHAR(2000),
    "Status"           INTEGER NOT NULL DEFAULT 0,       -- 0=Draft,1=Frozen,2=Submitted,3=Approved,4=Rejected,5=Superseded

    -- Freeze info
    "FrozenAt"         TIMESTAMP WITH TIME ZONE,
    "FrozenById"       INTEGER REFERENCES "Users"("Id") ON DELETE SET NULL,
    "FrozenByName"     VARCHAR(150),

    -- Owner approval
    "SubmittedAt"      TIMESTAMP WITH TIME ZONE,
    "ApprovedAt"       TIMESTAMP WITH TIME ZONE,
    "ApprovedByName"   VARCHAR(150),
    "ApprovalNotes"    VARCHAR(1000),

    -- Data date
    "DataDate"         DATE,

    -- Auto Snapshot (for What-If simulations based on Current Plan)
    "IsAutoSnapshot"      BOOLEAN NOT NULL DEFAULT FALSE,
    "SourceSimulationId"  INTEGER,   -- FK added after ScheduleSimulations table created

    -- Denormalized stats
    "TaskCount"        INTEGER NOT NULL DEFAULT 0,
    "EarliestStart"    DATE,
    "LatestFinish"     DATE,
    "TotalCalendarDays" INTEGER,

    -- BaseEntity fields
    "IsActive"         BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedAt"        TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "UpdatedAt"        TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "CreatedById"      INTEGER,
    "UpdatedById"      INTEGER
);

CREATE UNIQUE INDEX "IX_ScheduleBaselines_ProjectId_VersionNumber"
    ON "ScheduleBaselines" ("ProjectId", "VersionNumber");

-- ─── 2. BaselineTaskSnapshots ────────────────────────────────────────────────

CREATE TABLE IF NOT EXISTS "BaselineTaskSnapshots" (
    "Id"                    SERIAL PRIMARY KEY,
    "BaselineId"            INTEGER NOT NULL REFERENCES "ScheduleBaselines"("Id") ON DELETE CASCADE,

    -- Source references
    "SourceScheduleTaskId"  INTEGER REFERENCES "ScheduleTasks"("Id") ON DELETE SET NULL,
    "SourceWorkingTaskId"   INTEGER REFERENCES "WorkingTasks"("Id") ON DELETE SET NULL,

    -- Frozen task data
    "WbsCode"               VARCHAR(30),
    "Text"                  VARCHAR(300) NOT NULL,
    "Description"           VARCHAR(1000),
    "Location"              VARCHAR(200),
    "TaskType"              INTEGER NOT NULL DEFAULT 0,   -- 0=Task,1=Project,2=Milestone

    -- Hierarchy (snapshot-internal)
    "ParentSnapshotId"      INTEGER REFERENCES "BaselineTaskSnapshots"("Id") ON DELETE RESTRICT,
    "SortOrder"             INTEGER NOT NULL DEFAULT 0,
    "IsOpen"                BOOLEAN NOT NULL DEFAULT TRUE,

    -- Trade / Assignment (denormalized)
    "TradeId"               INTEGER,
    "TradeName"             VARCHAR(100),
    "TradeColor"            VARCHAR(10),
    "AssignedToId"          INTEGER,
    "AssignedToName"        VARCHAR(150),

    -- Dates
    "StartDate"             DATE NOT NULL,
    "EndDate"               DATE NOT NULL,
    "Duration"              INTEGER NOT NULL DEFAULT 1,
    "Progress"              NUMERIC(5,4) NOT NULL DEFAULT 0,
    "ActualStartDate"       DATE,
    "ActualEndDate"         DATE,

    -- Constraint
    "ConstraintType"        INTEGER,
    "ConstraintDate"        DATE,

    -- Display
    "Color"                 VARCHAR(10),
    "CrewSize"              INTEGER NOT NULL DEFAULT 0,
    "Notes"                 VARCHAR(1000)
);

CREATE INDEX "IX_BaselineTaskSnapshots_BaselineId"
    ON "BaselineTaskSnapshots" ("BaselineId");

-- ─── 3. ScheduleSimulations ──────────────────────────────────────────────────

CREATE TABLE IF NOT EXISTS "ScheduleSimulations" (
    "Id"                  SERIAL PRIMARY KEY,
    "ProjectId"           INTEGER NOT NULL REFERENCES "Projects"("Id") ON DELETE CASCADE,
    "Name"                VARCHAR(200) NOT NULL,
    "Description"         VARCHAR(2000),
    "Status"              INTEGER NOT NULL DEFAULT 0,     -- 0=Active,1=Saved,2=Archived

    -- Source
    "SourceType"          INTEGER NOT NULL DEFAULT 0,     -- 0=CurrentPlan,1=Baseline
    "SourceBaselineId"    INTEGER REFERENCES "ScheduleBaselines"("Id") ON DELETE SET NULL,

    -- Creator
    "CreatedByUserId"     INTEGER REFERENCES "Users"("Id") ON DELETE SET NULL,
    "CreatedByName"       VARCHAR(150),

    -- Impact summary
    "ModifiedTaskCount"   INTEGER NOT NULL DEFAULT 0,
    "TotalDaysImpact"     INTEGER,
    "SimulatedEndDate"    DATE,

    -- BaseEntity fields
    "IsActive"            BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedAt"           TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "UpdatedAt"           TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "CreatedById"         INTEGER,
    "UpdatedById"         INTEGER
);

CREATE INDEX "IX_ScheduleSimulations_ProjectId"
    ON "ScheduleSimulations" ("ProjectId");

-- ─── 4. SimulationTasks ──────────────────────────────────────────────────────

CREATE TABLE IF NOT EXISTS "SimulationTasks" (
    "Id"                   SERIAL PRIMARY KEY,
    "SimulationId"         INTEGER NOT NULL REFERENCES "ScheduleSimulations"("Id") ON DELETE CASCADE,

    -- Source task reference
    "SourceWorkingTaskId"  INTEGER REFERENCES "WorkingTasks"("Id") ON DELETE SET NULL,
    "SourceSnapshotId"     INTEGER REFERENCES "BaselineTaskSnapshots"("Id") ON DELETE SET NULL,

    -- Overridden fields (null = use source value)
    "Text"                 VARCHAR(300),
    "StartDate"            DATE,
    "EndDate"              DATE,
    "Duration"             INTEGER,
    "Progress"             DOUBLE PRECISION,
    "TradeId"              INTEGER,
    "AssignedToId"         INTEGER,
    "CrewSize"             INTEGER,
    "Notes"                VARCHAR(1000),

    -- Delta info
    "DaysShifted"          INTEGER,
    "ChangeReason"         VARCHAR(500),
    "IsNewTask"            BOOLEAN NOT NULL DEFAULT FALSE,
    "IsRemoved"            BOOLEAN NOT NULL DEFAULT FALSE
);

CREATE INDEX "IX_SimulationTasks_SimulationId"
    ON "SimulationTasks" ("SimulationId");

-- ─── 5. Add BaselineId to ScheduleRevisions ──────────────────────────────────

ALTER TABLE "ScheduleRevisions"
    ADD COLUMN IF NOT EXISTS "BaselineId" INTEGER REFERENCES "ScheduleBaselines"("Id") ON DELETE SET NULL;

-- ─── 6. Deferred FK: ScheduleBaselines.SourceSimulationId ────────────────────
-- Added after ScheduleSimulations table exists to avoid circular dependency

ALTER TABLE "ScheduleBaselines"
    ADD CONSTRAINT "FK_ScheduleBaselines_SourceSimulation"
    FOREIGN KEY ("SourceSimulationId") REFERENCES "ScheduleSimulations"("Id") ON DELETE SET NULL;

COMMIT;
