-- 1. 모든 스키마 목록
SELECT schema_name FROM information_schema.schemata ORDER BY schema_name;

-- 2. 모든 스키마에서 task 관련 테이블
SELECT table_schema, table_name
FROM information_schema.tables
WHERE table_name ILIKE '%task%' OR table_name ILIKE '%working%' OR table_name ILIKE '%schedule%'
ORDER BY table_schema, table_name;

-- 3. Duration 컬럼을 모든 스키마에서 검색
SELECT table_schema, table_name, column_name, data_type
FROM information_schema.columns
WHERE column_name ILIKE '%duration%'
ORDER BY table_schema, table_name;
