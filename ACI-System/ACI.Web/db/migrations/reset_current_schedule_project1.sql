-- ============================================================
-- Current Schedule (WorkingTasks) 리셋 — Project ID = 1
-- 실행 전: Baseline Schedule에 XML을 다시 Import 완료할 것
-- 실행 후: 브라우저에서 "Initialize from Baseline" 클릭
-- ============================================================

BEGIN;

-- 1. ScheduleChanges 삭제 (WorkingTask 참조)
DELETE FROM "ScheduleChanges"
WHERE "RevisionId" IN (
    SELECT "Id" FROM "ScheduleRevisions" WHERE "ProjectId" = 1
);

-- 2. RevisionDocuments 삭제
DELETE FROM "RevisionDocuments"
WHERE "RevisionId" IN (
    SELECT "Id" FROM "ScheduleRevisions" WHERE "ProjectId" = 1
);

-- 3. ScheduleRevisions 삭제
DELETE FROM "ScheduleRevisions" WHERE "ProjectId" = 1;

-- 4. WorkingTasks 자기참조 FK 먼저 NULL
UPDATE "WorkingTasks" SET "ParentId" = NULL WHERE "ProjectId" = 1;

-- 5. WorkingTasks 삭제
DELETE FROM "WorkingTasks" WHERE "ProjectId" = 1;

COMMIT;

-- 결과 확인
SELECT
    (SELECT COUNT(*) FROM "WorkingTasks"      WHERE "ProjectId" = 1) AS working_tasks_remaining,
    (SELECT COUNT(*) FROM "ScheduleRevisions" WHERE "ProjectId" = 1) AS revisions_remaining,
    (SELECT COUNT(*) FROM "ScheduleTasks"     WHERE "ProjectId" = 1) AS baseline_tasks_count;
