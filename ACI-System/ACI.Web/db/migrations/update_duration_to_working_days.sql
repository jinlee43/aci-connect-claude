-- =============================================================================
-- Duration → Working Days 변환 스크립트  (DB: aci_v4)
-- 토요일(DOW=6), 일요일(DOW=0)을 제외한 평일 수로 재계산
-- 실행: psql -h 192.168.1.195 -U bpms -d aci_v4 -f update_duration_to_working_days.sql
-- =============================================================================

BEGIN;

-- ──────────────────────────────────────────────────────────────────────────────
-- 1. 변경 전 미리보기 (diff != 0 인 항목만 표시)
-- ──────────────────────────────────────────────────────────────────────────────

SELECT '=== WorkingTasks ===' AS info;
SELECT
    "Id",
    "Text",
    "StartDate",
    "EndDate",
    "Duration"                                          AS current_duration,
    (SELECT COUNT(*)::int
     FROM generate_series("StartDate"::date, "EndDate"::date, '1 day') d
     WHERE EXTRACT(DOW FROM d) NOT IN (0, 6))          AS new_working_days,
    (SELECT COUNT(*)::int
     FROM generate_series("StartDate"::date, "EndDate"::date, '1 day') d
     WHERE EXTRACT(DOW FROM d) NOT IN (0, 6))
    - "Duration"                                        AS diff
FROM "WorkingTasks"
WHERE "StartDate" IS NOT NULL AND "EndDate" IS NOT NULL
  AND (SELECT COUNT(*)::int
       FROM generate_series("StartDate"::date, "EndDate"::date, '1 day') d
       WHERE EXTRACT(DOW FROM d) NOT IN (0, 6)) <> "Duration"
ORDER BY "Id";

SELECT '=== ScheduleTasks ===' AS info;
SELECT
    "Id",
    "Text",
    "StartDate",
    "EndDate",
    "Duration"                                          AS current_duration,
    (SELECT COUNT(*)::int
     FROM generate_series("StartDate"::date, "EndDate"::date, '1 day') d
     WHERE EXTRACT(DOW FROM d) NOT IN (0, 6))          AS new_working_days,
    (SELECT COUNT(*)::int
     FROM generate_series("StartDate"::date, "EndDate"::date, '1 day') d
     WHERE EXTRACT(DOW FROM d) NOT IN (0, 6))
    - "Duration"                                        AS diff
FROM "ScheduleTasks"
WHERE "StartDate" IS NOT NULL AND "EndDate" IS NOT NULL
  AND (SELECT COUNT(*)::int
       FROM generate_series("StartDate"::date, "EndDate"::date, '1 day') d
       WHERE EXTRACT(DOW FROM d) NOT IN (0, 6)) <> "Duration"
ORDER BY "Id";

SELECT '=== LookaheadTasks ===' AS info;
SELECT
    "Id",
    "Text",
    "StartDate",
    "EndDate",
    "Duration"                                          AS current_duration,
    (SELECT COUNT(*)::int
     FROM generate_series("StartDate"::date, "EndDate"::date, '1 day') d
     WHERE EXTRACT(DOW FROM d) NOT IN (0, 6))          AS new_working_days,
    (SELECT COUNT(*)::int
     FROM generate_series("StartDate"::date, "EndDate"::date, '1 day') d
     WHERE EXTRACT(DOW FROM d) NOT IN (0, 6))
    - "Duration"                                        AS diff
FROM "LookaheadTasks"
WHERE "StartDate" IS NOT NULL AND "EndDate" IS NOT NULL
  AND (SELECT COUNT(*)::int
       FROM generate_series("StartDate"::date, "EndDate"::date, '1 day') d
       WHERE EXTRACT(DOW FROM d) NOT IN (0, 6)) <> "Duration"
ORDER BY "Id";


-- ──────────────────────────────────────────────────────────────────────────────
-- 2. 업데이트
-- ──────────────────────────────────────────────────────────────────────────────

UPDATE "WorkingTasks"
SET "Duration" = GREATEST(1,
    (SELECT COUNT(*)::int
     FROM generate_series("StartDate"::date, "EndDate"::date, '1 day') d
     WHERE EXTRACT(DOW FROM d) NOT IN (0, 6))
)
WHERE "StartDate" IS NOT NULL AND "EndDate" IS NOT NULL;

UPDATE "ScheduleTasks"
SET "Duration" = GREATEST(1,
    (SELECT COUNT(*)::int
     FROM generate_series("StartDate"::date, "EndDate"::date, '1 day') d
     WHERE EXTRACT(DOW FROM d) NOT IN (0, 6))
)
WHERE "StartDate" IS NOT NULL AND "EndDate" IS NOT NULL;

UPDATE "LookaheadTasks"
SET "Duration" = GREATEST(1,
    (SELECT COUNT(*)::int
     FROM generate_series("StartDate"::date, "EndDate"::date, '1 day') d
     WHERE EXTRACT(DOW FROM d) NOT IN (0, 6))
)
WHERE "StartDate" IS NOT NULL AND "EndDate" IS NOT NULL;


-- ──────────────────────────────────────────────────────────────────────────────
-- 3. 업데이트 결과 확인
-- ──────────────────────────────────────────────────────────────────────────────

SELECT 'WorkingTasks 업데이트 완료' AS result,
       COUNT(*) AS total_rows,
       SUM(CASE WHEN "Duration" < 1 THEN 1 ELSE 0 END) AS zero_or_less
FROM "WorkingTasks";

SELECT 'ScheduleTasks 업데이트 완료' AS result,
       COUNT(*) AS total_rows,
       SUM(CASE WHEN "Duration" < 1 THEN 1 ELSE 0 END) AS zero_or_less
FROM "ScheduleTasks";

SELECT 'LookaheadTasks 업데이트 완료' AS result,
       COUNT(*) AS total_rows,
       SUM(CASE WHEN "Duration" < 1 THEN 1 ELSE 0 END) AS zero_or_less
FROM "LookaheadTasks";


-- ──────────────────────────────────────────────────────────────────────────────
-- 결과 확인 후 이상없으면 COMMIT, 문제있으면 ROLLBACK
-- ──────────────────────────────────────────────────────────────────────────────
COMMIT;
