-- ============================================================================
-- ACI Connect DB Patch — Missing Tables & Columns
-- Run this against: aci_v4 (192.168.1.195)
-- Safe to run multiple times (IF NOT EXISTS guards throughout)
-- ============================================================================

BEGIN;

-- ─── 1. ScheduleBaselines ────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS "ScheduleBaselines" (
    "Id"               SERIAL PRIMARY KEY,
    "ProjectId"        INTEGER NOT NULL REFERENCES "Projects"("Id") ON DELETE CASCADE,
    "VersionNumber"    INTEGER NOT NULL,
    "Title"            VARCHAR(200) NOT NULL,
    "Description"      VARCHAR(2000),
    "Status"           INTEGER NOT NULL DEFAULT 0,

    "FrozenAt"         TIMESTAMP WITH TIME ZONE,
    "FrozenById"       INTEGER REFERENCES "Users"("Id") ON DELETE SET NULL,
    "FrozenByName"     VARCHAR(150),

    "SubmittedAt"      TIMESTAMP WITH TIME ZONE,
    "ApprovedAt"       TIMESTAMP WITH TIME ZONE,
    "ApprovedByName"   VARCHAR(150),
    "ApprovalNotes"    VARCHAR(1000),

    "DataDate"         DATE,

    "IsAutoSnapshot"      BOOLEAN NOT NULL DEFAULT FALSE,
    "SourceSimulationId"  INTEGER,   -- FK added later (circular dep workaround)

    "TaskCount"        INTEGER NOT NULL DEFAULT 0,
    "EarliestStart"    DATE,
    "LatestFinish"     DATE,
    "TotalCalendarDays" INTEGER,

    "IsActive"         BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedAt"        TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "UpdatedAt"        TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "CreatedById"      INTEGER,
    "UpdatedById"      INTEGER
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_ScheduleBaselines_ProjectId_VersionNumber"
    ON "ScheduleBaselines" ("ProjectId", "VersionNumber");

CREATE INDEX IF NOT EXISTS "IX_ScheduleBaselines_ProjectId"
    ON "ScheduleBaselines" ("ProjectId");

-- ─── 2. BaselineTaskSnapshots ────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS "BaselineTaskSnapshots" (
    "Id"                    SERIAL PRIMARY KEY,
    "BaselineId"            INTEGER NOT NULL REFERENCES "ScheduleBaselines"("Id") ON DELETE CASCADE,

    "SourceScheduleTaskId"  INTEGER REFERENCES "ScheduleTasks"("Id") ON DELETE SET NULL,
    "SourceWorkingTaskId"   INTEGER REFERENCES "WorkingTasks"("Id") ON DELETE SET NULL,

    "WbsCode"               VARCHAR(30),
    "Text"                  VARCHAR(300) NOT NULL,
    "Description"           VARCHAR(1000),
    "Location"              VARCHAR(200),
    "TaskType"              INTEGER NOT NULL DEFAULT 0,

    "ParentSnapshotId"      INTEGER REFERENCES "BaselineTaskSnapshots"("Id") ON DELETE RESTRICT,
    "SortOrder"             INTEGER NOT NULL DEFAULT 0,
    "IsOpen"                BOOLEAN NOT NULL DEFAULT TRUE,

    "TradeId"               INTEGER,
    "TradeName"             VARCHAR(100),
    "TradeColor"            VARCHAR(10),
    "AssignedToId"          INTEGER,
    "AssignedToName"        VARCHAR(150),

    "StartDate"             DATE NOT NULL,
    "EndDate"               DATE NOT NULL,
    "Duration"              INTEGER NOT NULL DEFAULT 1,
    "Progress"              NUMERIC(5,4) NOT NULL DEFAULT 0,
    "ActualStartDate"       DATE,
    "ActualEndDate"         DATE,

    "ConstraintType"        INTEGER,
    "ConstraintDate"        DATE,

    "Color"                 VARCHAR(10),
    "CrewSize"              INTEGER NOT NULL DEFAULT 0,
    "Notes"                 VARCHAR(1000)
);

CREATE INDEX IF NOT EXISTS "IX_BaselineTaskSnapshots_BaselineId"
    ON "BaselineTaskSnapshots" ("BaselineId");
CREATE INDEX IF NOT EXISTS "IX_BaselineTaskSnapshots_SourceScheduleTaskId"
    ON "BaselineTaskSnapshots" ("SourceScheduleTaskId");
CREATE INDEX IF NOT EXISTS "IX_BaselineTaskSnapshots_SourceWorkingTaskId"
    ON "BaselineTaskSnapshots" ("SourceWorkingTaskId");

-- ─── 3. ScheduleSimulations ──────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS "ScheduleSimulations" (
    "Id"                  SERIAL PRIMARY KEY,
    "ProjectId"           INTEGER NOT NULL REFERENCES "Projects"("Id") ON DELETE CASCADE,
    "Name"                VARCHAR(200) NOT NULL,
    "Description"         VARCHAR(2000),
    "Status"              INTEGER NOT NULL DEFAULT 0,

    "SourceType"          INTEGER NOT NULL DEFAULT 0,
    "SourceBaselineId"    INTEGER REFERENCES "ScheduleBaselines"("Id") ON DELETE SET NULL,

    "CreatedByUserId"     INTEGER REFERENCES "Users"("Id") ON DELETE SET NULL,
    "CreatedByName"       VARCHAR(150),

    "ModifiedTaskCount"   INTEGER NOT NULL DEFAULT 0,
    "TotalDaysImpact"     INTEGER,
    "SimulatedEndDate"    DATE,

    "IsActive"            BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedAt"           TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "UpdatedAt"           TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "CreatedById"         INTEGER,
    "UpdatedById"         INTEGER
);

CREATE INDEX IF NOT EXISTS "IX_ScheduleSimulations_ProjectId"
    ON "ScheduleSimulations" ("ProjectId");

-- ─── 4. SimulationTasks ──────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS "SimulationTasks" (
    "Id"                   SERIAL PRIMARY KEY,
    "SimulationId"         INTEGER NOT NULL REFERENCES "ScheduleSimulations"("Id") ON DELETE CASCADE,

    "SourceWorkingTaskId"  INTEGER REFERENCES "WorkingTasks"("Id") ON DELETE SET NULL,
    "SourceSnapshotId"     INTEGER REFERENCES "BaselineTaskSnapshots"("Id") ON DELETE SET NULL,

    "Text"                 VARCHAR(300),
    "StartDate"            DATE,
    "EndDate"              DATE,
    "Duration"             INTEGER,
    "Progress"             DOUBLE PRECISION,
    "TradeId"              INTEGER,
    "AssignedToId"         INTEGER,
    "CrewSize"             INTEGER,
    "Notes"                VARCHAR(1000),

    "DaysShifted"          INTEGER,
    "ChangeReason"         VARCHAR(500),
    "IsNewTask"            BOOLEAN NOT NULL DEFAULT FALSE,
    "IsRemoved"            BOOLEAN NOT NULL DEFAULT FALSE
);

CREATE INDEX IF NOT EXISTS "IX_SimulationTasks_SimulationId"
    ON "SimulationTasks" ("SimulationId");

-- ─── 5. ScheduleRevisions.BaselineId ─────────────────────────────────────────
ALTER TABLE "ScheduleRevisions"
    ADD COLUMN IF NOT EXISTS "BaselineId" INTEGER REFERENCES "ScheduleBaselines"("Id") ON DELETE SET NULL;

CREATE INDEX IF NOT EXISTS "IX_ScheduleRevisions_BaselineId"
    ON "ScheduleRevisions" ("BaselineId");

-- ─── 6. ScheduleBaselines.SourceSimulationId → FK (deferred, avoids circular dep) ──
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'FK_ScheduleBaselines_SourceSimulation'
    ) THEN
        ALTER TABLE "ScheduleBaselines"
            ADD CONSTRAINT "FK_ScheduleBaselines_SourceSimulation"
            FOREIGN KEY ("SourceSimulationId")
            REFERENCES "ScheduleSimulations"("Id") ON DELETE SET NULL;
    END IF;
END $$;

-- ─── 7. TaskDependencies (간트 의존성 링크 — 기존 마이그레이션에 누락) ─────────
CREATE TABLE IF NOT EXISTS "TaskDependencies" (
    "Id"       SERIAL PRIMARY KEY,
    "SourceId" INTEGER NOT NULL REFERENCES "ScheduleTasks"("Id") ON DELETE CASCADE,
    "TargetId" INTEGER NOT NULL REFERENCES "ScheduleTasks"("Id") ON DELETE CASCADE,
    "Type"     INTEGER NOT NULL DEFAULT 0,   -- 0=FS, 1=SS, 2=FF, 3=SF
    "Lag"      INTEGER NOT NULL DEFAULT 0
);

CREATE INDEX IF NOT EXISTS "IX_TaskDependencies_SourceId"
    ON "TaskDependencies" ("SourceId");
CREATE INDEX IF NOT EXISTS "IX_TaskDependencies_TargetId"
    ON "TaskDependencies" ("TargetId");

-- ─── 검증 쿼리 (적용 후 확인용) ──────────────────────────────────────────────
SELECT
    table_name,
    (SELECT COUNT(*) FROM information_schema.columns c
     WHERE c.table_name = t.table_name AND c.table_schema = 'public') AS col_count
FROM information_schema.tables t
WHERE table_schema = 'public'
  AND table_name IN (
      'ScheduleBaselines', 'BaselineTaskSnapshots',
      'ScheduleSimulations', 'SimulationTasks',
      'TaskDependencies', 'ScheduleRevisions'
  )
ORDER BY table_name;

COMMIT;
