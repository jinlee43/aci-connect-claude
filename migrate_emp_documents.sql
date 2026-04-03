-- Legacy EmpDataItem Migration Script
-- Run this against the new ACI system PostgreSQL database AFTER running EF migrations
-- Files are on the NAS at their original LegacyPath; StoredFileName is empty (legacy-only)

BEGIN;

INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Alex Cho (App-Resume)-A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/chris_20240830034223658552.pdf', true, '2024-08-30 15:42:23+00', '2024-08-30 15:42:23+00'
FROM "Employees" e WHERE e."EmpNum" = 2032
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Adam Doty (App-Resume) - A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/admin_20240830040301196394.pdf', true, '2024-08-30 16:03:01+00', '2024-08-30 16:03:01+00'
FROM "Employees" e WHERE e."EmpNum" = 2031
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Alexander-Lubensky (App-Resume) - A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/chris_20240906094123323496.pdf', true, '2024-09-06 09:41:23+00', '2024-09-06 09:41:23+00'
FROM "Employees" e WHERE e."EmpNum" = 2034
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Amir Markazi (A).pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/chris_20240910014250978036.pdf', true, '2024-09-10 13:42:51+00', '2024-09-10 13:42:51+00'
FROM "Employees" e WHERE e."EmpNum" = 2035
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Ana Victor (A).pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/chris_20240910014411992627.pdf', true, '2024-09-10 13:44:12+00', '2024-09-10 13:44:12+00'
FROM "Employees" e WHERE e."EmpNum" = 2036
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Andy Campos (App-Resume)-A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/chris_20240912104956354634.pdf', true, '2024-09-12 10:49:56+00', '2024-09-12 10:49:56+00'
FROM "Employees" e WHERE e."EmpNum" = 2038
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'AndrewPhillips (App-Resume) - A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/chris_20240912112017687864.pdf', true, '2024-09-12 11:20:17+00', '2024-09-12 11:20:17+00'
FROM "Employees" e WHERE e."EmpNum" = 2037
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Angel Pineda (App-Resume) - A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/chris_20240912112039649361.pdf', true, '2024-09-12 11:20:39+00', '2024-09-12 11:20:39+00'
FROM "Employees" e WHERE e."EmpNum" = 2039
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Angel Solis (App-Resume) - A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/chris_20240912112214809839.pdf', true, '2024-09-12 11:22:14+00', '2024-09-12 11:22:14+00'
FROM "Employees" e WHERE e."EmpNum" = 2040
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Anthony Campos (App-Resume)A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240916081924970078.pdf', true, '2024-09-16 08:19:24+00', '2024-09-16 08:19:24+00'
FROM "Employees" e WHERE e."EmpNum" = 2041
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Armando (App-Resume) - A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240916081947737637.pdf', true, '2024-09-16 08:19:47+00', '2024-09-16 08:19:47+00'
FROM "Employees" e WHERE e."EmpNum" = 2042
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Brandon Choi (App-Resume)-A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240916082058168680.pdf', true, '2024-09-16 08:20:58+00', '2024-09-16 08:20:58+00'
FROM "Employees" e WHERE e."EmpNum" = 2044
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Brandon Maury - (App-Resume)-A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240916082143082046.pdf', true, '2024-09-16 08:21:43+00', '2024-09-16 08:21:43+00'
FROM "Employees" e WHERE e."EmpNum" = 2045
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Brian Shin (App-Resume)-A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240916082326195980.pdf', true, '2024-09-16 08:23:26+00', '2024-09-16 08:23:26+00'
FROM "Employees" e WHERE e."EmpNum" = 2046
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Bryant Ramos (App- Resume) - A .pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240916082410405261.pdf', true, '2024-01-01 00:00:00+00', '2024-01-01 00:00:00+00'
FROM "Employees" e WHERE e."EmpNum" = 2047
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Carlos Cruz - Resume and Application - A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240916082638227237.pdf', true, '2024-09-16 08:26:38+00', '2024-09-16 08:26:38+00'
FROM "Employees" e WHERE e."EmpNum" = 2048
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Carlos Redona  (App-Resume) - A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240916082723217534.pdf', true, '2024-09-16 08:27:23+00', '2024-09-16 08:27:23+00'
FROM "Employees" e WHERE e."EmpNum" = 2049
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Cindy (App-Resume)-A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240916083050300539.pdf', true, '2024-09-16 08:30:50+00', '2024-09-16 08:30:50+00'
FROM "Employees" e WHERE e."EmpNum" = 2051
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Craig Trejo - (App-Resume)-A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240916083248032104.pdf', true, '2024-09-16 08:32:48+00', '2024-09-16 08:32:48+00'
FROM "Employees" e WHERE e."EmpNum" = 2052
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Dan Brown (App-Resume)-A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240916083353724381.pdf', true, '2024-09-16 08:33:53+00', '2024-09-16 08:33:53+00'
FROM "Employees" e WHERE e."EmpNum" = 2053
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Daniel Braswell (App-Resume) - A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240916083506267851.pdf', true, '2024-01-01 00:00:00+00', '2024-01-01 00:00:00+00'
FROM "Employees" e WHERE e."EmpNum" = 2054
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Daniel Chon (App- Resume) - A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240916083616000618.pdf', true, '2024-09-16 08:36:16+00', '2024-09-16 08:36:16+00'
FROM "Employees" e WHERE e."EmpNum" = 2055
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Daniel Lee (App-Resume) (A).pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240916083718924102.pdf', true, '2024-09-16 08:37:18+00', '2024-09-16 08:37:18+00'
FROM "Employees" e WHERE e."EmpNum" = 2056
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Danny Kim(App-Resume) - (A).pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240916084740378932.pdf', true, '2024-09-16 08:47:40+00', '2024-09-16 08:47:40+00'
FROM "Employees" e WHERE e."EmpNum" = 2057
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'David Garcia (App-Resume) - (A).pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240916084847626189.pdf', true, '2024-09-16 08:48:47+00', '2024-09-16 08:48:47+00'
FROM "Employees" e WHERE e."EmpNum" = 2058
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Dean Song - Resume &amp; App-A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240916085051407009.pdf', true, '2024-01-01 00:00:00+00', '2024-01-01 00:00:00+00'
FROM "Employees" e WHERE e."EmpNum" = 2059
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Derek Giles (Resume-EE) (A).pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240917093840529600.pdf', true, '2024-09-17 09:38:40+00', '2024-09-17 09:38:40+00'
FROM "Employees" e WHERE e."EmpNum" = 2060
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Resume &amp; Application - Drew Maled- (A).pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240917094010695279.pdf', true, '2024-09-17 09:40:10+00', '2024-09-17 09:40:10+00'
FROM "Employees" e WHERE e."EmpNum" = 2061
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Edward Lidyoff II (App-Resume) - A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240917094124529261.pdf', true, '2024-09-17 09:41:24+00', '2024-09-17 09:41:24+00'
FROM "Employees" e WHERE e."EmpNum" = 2062
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Edward Park (Resume)2.docx', '', 'docx', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240917094249447464.docx', true, '2024-09-17 09:42:49+00', '2024-09-17 09:42:49+00'
FROM "Employees" e WHERE e."EmpNum" = 2063
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Edward (App-Resume)-A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240917094249486521.pdf', true, '2024-09-17 09:42:49+00', '2024-09-17 09:42:49+00'
FROM "Employees" e WHERE e."EmpNum" = 2063
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Eric Lee (W4-EE).pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240917094735779891.pdf', true, '2024-09-17 09:47:35+00', '2024-09-17 09:47:35+00'
FROM "Employees" e WHERE e."EmpNum" = 2064
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Eugene Kang (App-Resume) - A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240917094754502242.pdf', true, '2024-09-17 09:47:54+00', '2024-09-17 09:47:54+00'
FROM "Employees" e WHERE e."EmpNum" = 2065
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Grace Cho (A).pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240917095014223755.pdf', true, '2024-09-17 09:50:14+00', '2024-09-17 09:50:14+00'
FROM "Employees" e WHERE e."EmpNum" = 2066
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Greg Edwards (App-Resume) (A).pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240917095033144689.pdf', true, '2024-01-01 00:00:00+00', '2024-01-01 00:00:00+00'
FROM "Employees" e WHERE e."EmpNum" = 2067
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Harry Kim (App-Resume) (A).pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240917095131076649.pdf', true, '2024-09-17 09:51:31+00', '2024-09-17 09:51:31+00'
FROM "Employees" e WHERE e."EmpNum" = 2069
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Humberto (App-Resume).pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240917095225283566.pdf', true, '2024-09-17 09:52:25+00', '2024-09-17 09:52:25+00'
FROM "Employees" e WHERE e."EmpNum" = 2070
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Ismael-Nieves - (App-Resume) - A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240917095754522848.pdf', true, '2024-09-17 09:57:54+00', '2024-09-17 09:57:54+00'
FROM "Employees" e WHERE e."EmpNum" = 2071
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Janice Kim (App-Resume).pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240917095913805627.pdf', true, '2024-09-17 09:59:13+00', '2024-09-17 09:59:13+00'
FROM "Employees" e WHERE e."EmpNum" = 2072
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Javier Gil (App-Resume) - A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240917101439471364.pdf', true, '2024-09-17 10:14:39+00', '2024-09-17 10:14:39+00'
FROM "Employees" e WHERE e."EmpNum" = 2073
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Jay Kwon (App-Resume)(A).pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240917101611138300.pdf', true, '2024-09-17 10:16:11+00', '2024-09-17 10:16:11+00'
FROM "Employees" e WHERE e."EmpNum" = 2126
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Jay Park (W4-EE).pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240917101710673382.pdf', true, '2024-09-17 10:17:10+00', '2024-09-17 10:17:10+00'
FROM "Employees" e WHERE e."EmpNum" = 2074
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Application - Jhun Magpantay (A).pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240917101814957219.pdf', true, '2024-09-17 10:18:15+00', '2024-09-17 10:18:15+00'
FROM "Employees" e WHERE e."EmpNum" = 2075
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Jin Lee (App-Resume) (A).pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240917101909908637.pdf', true, '2024-09-17 10:19:09+00', '2024-09-17 10:19:09+00'
FROM "Employees" e WHERE e."EmpNum" = 2076
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Jo Padro (App-Resume)-A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240917102006152535.pdf', true, '2024-09-17 10:20:06+00', '2024-09-17 10:20:06+00'
FROM "Employees" e WHERE e."EmpNum" = 2077
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'John Kim (App-Resume)A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240917102120443009.pdf', true, '2024-09-17 10:21:20+00', '2024-09-17 10:21:20+00'
FROM "Employees" e WHERE e."EmpNum" = 2078
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'John Larios (App-Resume) - A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240917102216410468.pdf', true, '2024-09-17 10:22:16+00', '2024-09-17 10:22:16+00'
FROM "Employees" e WHERE e."EmpNum" = 2079
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'John Park (App-Resume) - A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240917102327810354.pdf', true, '2024-09-17 10:23:27+00', '2024-09-17 10:23:27+00'
FROM "Employees" e WHERE e."EmpNum" = 2080
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Johnny Lee (App-Resume)-(A).pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240917102601948583.pdf', true, '2024-09-17 10:26:01+00', '2024-09-17 10:26:01+00'
FROM "Employees" e WHERE e."EmpNum" = 2081
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Jonathan Park (App-Resume) -A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240917102713266536.pdf', true, '2024-09-17 10:27:13+00', '2024-09-17 10:27:13+00'
FROM "Employees" e WHERE e."EmpNum" = 2082
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Jose APP (12-13-13).pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240917105435103519.pdf', true, '2024-09-17 10:54:35+00', '2024-09-17 10:54:35+00'
FROM "Employees" e WHERE e."EmpNum" = 2083
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Jose APP (5-20-19).pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240917105435229011.pdf', true, '2024-09-17 10:54:35+00', '2024-09-17 10:54:35+00'
FROM "Employees" e WHERE e."EmpNum" = 2083
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Jose APP (1-9-23).pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240917105435252634.pdf', true, '2024-09-17 10:54:35+00', '2024-09-17 10:54:35+00'
FROM "Employees" e WHERE e."EmpNum" = 2083
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Joseph Choi (App-Resume)-A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240917105451027117.pdf', true, '2024-09-17 10:54:51+00', '2024-09-17 10:54:51+00'
FROM "Employees" e WHERE e."EmpNum" = 2084
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Justin Chon (App-Resume)-A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240917105632646826.pdf', true, '2024-01-01 00:00:00+00', '2024-01-01 00:00:00+00'
FROM "Employees" e WHERE e."EmpNum" = 2085
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Justin Park (App-Resume)-A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240917105810617764.pdf', true, '2024-09-17 10:58:10+00', '2024-09-17 10:58:10+00'
FROM "Employees" e WHERE e."EmpNum" = 2086
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Justing Park A - (A).pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240917105821255147.pdf', true, '2024-09-17 10:58:21+00', '2024-09-17 10:58:21+00'
FROM "Employees" e WHERE e."EmpNum" = 2087
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Karim Jacobo (App-Resume)-A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240917105909339399.pdf', true, '2024-09-17 10:59:09+00', '2024-09-17 10:59:09+00'
FROM "Employees" e WHERE e."EmpNum" = 2088
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Ken Fitz (App-Resume)- A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240917105954000890.pdf', true, '2024-09-17 10:59:54+00', '2024-09-17 10:59:54+00'
FROM "Employees" e WHERE e."EmpNum" = 2089
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Kevin Choi (App-Resume) - A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240917110040700630.pdf', true, '2024-09-17 11:00:40+00', '2024-09-17 11:00:40+00'
FROM "Employees" e WHERE e."EmpNum" = 2090
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Kevin Chung - Application &amp; Resume - A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240917110351569993.pdf', true, '2024-01-01 00:00:00+00', '2024-01-01 00:00:00+00'
FROM "Employees" e WHERE e."EmpNum" = 2091
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Louis Oropeza (App-Resume) - A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240917110604494924.pdf', true, '2024-09-17 11:06:04+00', '2024-09-17 11:06:04+00'
FROM "Employees" e WHERE e."EmpNum" = 2092
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Luke Kim (Resume).pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240917110705167429.pdf', true, '2024-09-17 11:07:05+00', '2024-09-17 11:07:05+00'
FROM "Employees" e WHERE e."EmpNum" = 2093
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Luke Kim - Resume &amp; App - A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240917110705192553.pdf', true, '2024-09-17 11:07:05+00', '2024-09-17 11:07:05+00'
FROM "Employees" e WHERE e."EmpNum" = 2093
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Maritza Leanos (App-Resume)- A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240917110723347418.pdf', true, '2024-09-17 11:07:23+00', '2024-09-17 11:07:23+00'
FROM "Employees" e WHERE e."EmpNum" = 2094
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Martin (App. - Resume) - A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240917110819641373.pdf', true, '2024-09-17 11:08:19+00', '2024-09-17 11:08:19+00'
FROM "Employees" e WHERE e."EmpNum" = 2095
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Matteo Carletti (App-Resume) - A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240917110910786937.pdf', true, '2024-09-17 11:09:10+00', '2024-09-17 11:09:10+00'
FROM "Employees" e WHERE e."EmpNum" = 2096
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Mau Medrano (Resume &amp; App) - A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240917110952777571.pdf', true, '2024-09-17 11:09:52+00', '2024-09-17 11:09:52+00'
FROM "Employees" e WHERE e."EmpNum" = 2097
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Melvin Paniagua - Application &amp; Resume - A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240917111055105581.pdf', true, '2024-09-17 11:10:55+00', '2024-09-17 11:10:55+00'
FROM "Employees" e WHERE e."EmpNum" = 2098
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Michael Lin (App-Resume) - A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240917111157060867.pdf', true, '2024-09-17 11:11:57+00', '2024-09-17 11:11:57+00'
FROM "Employees" e WHERE e."EmpNum" = 2099
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Nam Cho (App- Resume) - A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240917111242434320.pdf', true, '2024-09-17 11:12:42+00', '2024-09-17 11:12:42+00'
FROM "Employees" e WHERE e."EmpNum" = 2100
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Nathan Hwang (App-resume) - A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240917111351590679.pdf', true, '2024-09-17 11:13:51+00', '2024-09-17 11:13:51+00'
FROM "Employees" e WHERE e."EmpNum" = 2101
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Nisa Legaspino (App-Resume) - A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240917111454006427.pdf', true, '2024-09-17 11:14:54+00', '2024-09-17 11:14:54+00'
FROM "Employees" e WHERE e."EmpNum" = 2102
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Paul Greer (App - Resume) - A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240917111548541406.pdf', true, '2024-09-17 11:15:48+00', '2024-09-17 11:15:48+00'
FROM "Employees" e WHERE e."EmpNum" = 2103
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Pavan Nanikalva (App-Resume) - A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240917111726985241.pdf', true, '2024-09-17 11:17:27+00', '2024-09-17 11:17:27+00'
FROM "Employees" e WHERE e."EmpNum" = 2105
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Pedro Montes (App-Resume) - A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240917111817331299.pdf', true, '2024-01-01 00:00:00+00', '2024-01-01 00:00:00+00'
FROM "Employees" e WHERE e."EmpNum" = 2106
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Peter Park (Resume-Orient) - A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240917111957989221.pdf', true, '2024-09-17 11:19:58+00', '2024-09-17 11:19:58+00'
FROM "Employees" e WHERE e."EmpNum" = 2107
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Quang Nguyen - (App-Resume) - A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240917112008917079.pdf', true, '2024-09-17 11:20:08+00', '2024-09-17 11:20:08+00'
FROM "Employees" e WHERE e."EmpNum" = 2108
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Rafael Gonzalez  (App - Resume) - A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240917112101076329.pdf', true, '2024-09-17 11:21:01+00', '2024-09-17 11:21:01+00'
FROM "Employees" e WHERE e."EmpNum" = 2109
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Ricardo Aguero ( App-Resume)-A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240917112312602321.pdf', true, '2024-09-17 11:23:12+00', '2024-09-17 11:23:12+00'
FROM "Employees" e WHERE e."EmpNum" = 2111
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Ronne Suarez (App-Resume)-A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240917112424112966.pdf', true, '2024-09-17 11:24:24+00', '2024-09-17 11:24:24+00'
FROM "Employees" e WHERE e."EmpNum" = 2112
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Roy Cho (App-Resume)-A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240917112509109823.pdf', true, '2024-09-17 11:25:09+00', '2024-09-17 11:25:09+00'
FROM "Employees" e WHERE e."EmpNum" = 2113
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Ryan McGuire - (App-Resume)-A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240917112733423703.pdf', true, '2024-09-17 11:27:33+00', '2024-09-17 11:27:33+00'
FROM "Employees" e WHERE e."EmpNum" = 2114
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Sai Anudeep (App-Resume)-A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240917112814454592.pdf', true, '2024-09-17 11:28:14+00', '2024-09-17 11:28:14+00'
FROM "Employees" e WHERE e."EmpNum" = 2115
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Sam Moon (App-Resume) - A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240917112916131377.pdf', true, '2024-09-17 11:29:16+00', '2024-09-17 11:29:16+00'
FROM "Employees" e WHERE e."EmpNum" = 2116
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Saul Valdovinos (App-Resume) - A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240917113030532129.pdf', true, '2024-09-17 11:30:30+00', '2024-09-17 11:30:30+00'
FROM "Employees" e WHERE e."EmpNum" = 2117
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Soo Kang (App-Reusme) - A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240917113114288210.pdf', true, '2024-01-01 00:00:00+00', '2024-01-01 00:00:00+00'
FROM "Employees" e WHERE e."EmpNum" = 2118
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Steve Spooner (App-Resume) - A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240917113224782868.pdf', true, '2024-09-17 11:32:24+00', '2024-09-17 11:32:24+00'
FROM "Employees" e WHERE e."EmpNum" = 2119
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Sung Cho (App-Resume) - A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240917125540936729.pdf', true, '2024-09-17 12:55:40+00', '2024-09-17 12:55:40+00'
FROM "Employees" e WHERE e."EmpNum" = 2120
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Terry Chon (App-Resume) - A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240917125639607263.pdf', true, '2024-09-17 12:56:39+00', '2024-09-17 12:56:39+00'
FROM "Employees" e WHERE e."EmpNum" = 2121
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Tim Mc (App-Resume) - A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240917125805718396.pdf', true, '2024-09-17 12:58:05+00', '2024-09-17 12:58:05+00'
FROM "Employees" e WHERE e."EmpNum" = 2122
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Vahid Hatami (App-Resume) - A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240917125849648344.pdf', true, '2024-09-17 12:58:49+00', '2024-09-17 12:58:49+00'
FROM "Employees" e WHERE e."EmpNum" = 2123
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'William Quintero (App-Resume) - A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240917010031422269.pdf', true, '2024-09-17 13:00:31+00', '2024-09-17 13:00:31+00'
FROM "Employees" e WHERE e."EmpNum" = 2125
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Paul Hernandez (App - Resume) -A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240917011259248346.pdf', true, '2024-09-17 13:12:59+00', '2024-09-17 13:12:59+00'
FROM "Employees" e WHERE e."EmpNum" = 2104
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Ray Yoo (W4-EE).pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240917012108621802.pdf', true, '2024-09-17 13:21:08+00', '2024-09-17 13:21:08+00'
FROM "Employees" e WHERE e."EmpNum" = 2110
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Walter Shirley - (App-Resume) - A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20240917013520974945.pdf', true, '2024-09-17 13:35:20+00', '2024-09-17 13:35:20+00'
FROM "Employees" e WHERE e."EmpNum" = 2124
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Daniel Yoon (App) - (A).pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20241113100345774207.pdf', true, '2024-11-13 10:03:45+00', '2024-11-13 10:03:45+00'
FROM "Employees" e WHERE e."EmpNum" = 2128
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Ivan Del Real (App - Resume) - (A).pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20241113022512980304.pdf', true, '2024-11-13 14:25:13+00', '2024-11-13 14:25:13+00'
FROM "Employees" e WHERE e."EmpNum" = 2129
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Ray Song   (App - Resume) - A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20241113023404225220.pdf', true, '2024-11-13 14:34:04+00', '2024-11-13 14:34:04+00'
FROM "Employees" e WHERE e."EmpNum" = 2131
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Lenny Tso (App - Resume) - A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2024B/dataItem_20241113030530817195.pdf', true, '2024-11-13 15:05:30+00', '2024-11-13 15:05:30+00'
FROM "Employees" e WHERE e."EmpNum" = 2130
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Gilbert Llamas (App-Resume)-A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2025B/EmpDataItem_20251017023724982130_HQ.pdf', true, '2025-10-17 14:37:24+00', '2025-10-17 14:37:24+00'
FROM "Employees" e WHERE e."EmpNum" = 2133
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Ashley  Davis (App-Resume)-A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2025B/EmpDataItem_20251017024720898404_HQ.pdf', true, '2025-10-17 14:47:20+00', '2025-10-17 14:47:20+00'
FROM "Employees" e WHERE e."EmpNum" = 2134
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'HARRY-LE (App-Resume)- A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2025B/EmpDataItem_20251017030940742058_HQ.pdf', true, '2025-10-17 15:09:40+00', '2025-10-17 15:09:40+00'
FROM "Employees" e WHERE e."EmpNum" = 2136
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Francisco-Serrano (App-Resume)- A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2025B/EmpDataItem_20251017031550631637_HQ.pdf', true, '2025-10-17 15:15:50+00', '2025-10-17 15:15:50+00'
FROM "Employees" e WHERE e."EmpNum" = 2138
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Thomas McNeil (App-Resume) - A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2025B/EmpDataItem_20251017032004446917_HQ.pdf', true, '2025-10-17 15:20:04+00', '2025-10-17 15:20:04+00'
FROM "Employees" e WHERE e."EmpNum" = 2139
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Mark Hendriks (App-Resume) - A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2025B/EmpDataItem_20251017033645125591_HQ.pdf', true, '2025-10-17 15:36:45+00', '2025-10-17 15:36:45+00'
FROM "Employees" e WHERE e."EmpNum" = 2137
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Luis Sanchez (App-Resume)- A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2025B/EmpDataItem_20251020113325651793_HQ.pdf', true, '2025-10-20 11:33:25+00', '2025-10-20 11:33:25+00'
FROM "Employees" e WHERE e."EmpNum" = 2144
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Ruben Angiano (App-Resume) - A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2026A/EmpDataItem_20260210110528880630_HQ.pdf', true, '2026-02-10 11:05:28+00', '2026-02-10 11:05:28+00'
FROM "Employees" e WHERE e."EmpNum" = 2154
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Ruben Angiano Jr 2025-2 Revised Resume(NEW).pdf (003).pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2026A/EmpDataItem_20260210110538420321_HQ.pdf', true, '2026-02-10 11:05:38+00', '2026-02-10 11:05:38+00'
FROM "Employees" e WHERE e."EmpNum" = 2154
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Cesar Galindo - Resume - 2024.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2026A/EmpDataItem_20260210110628983429_HQ.pdf', true, '2026-02-10 11:06:28+00', '2026-02-10 11:06:28+00'
FROM "Employees" e WHERE e."EmpNum" = 2156
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Cesar Galindo (App-Resume) - A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2026A/EmpDataItem_20260210110629051283_HQ.pdf', true, '2026-02-10 11:06:29+00', '2026-02-10 11:06:29+00'
FROM "Employees" e WHERE e."EmpNum" = 2156
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Joseph Ko (App-Resume) - A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2026A/EmpDataItem_20260210110905897974_HQ.pdf', true, '2026-02-10 11:09:05+00', '2026-02-10 11:09:05+00'
FROM "Employees" e WHERE e."EmpNum" = 2157
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Minsung Ko Resume 20250220.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2026A/EmpDataItem_20260210110906264848_HQ.pdf', true, '2026-02-10 11:09:06+00', '2026-02-10 11:09:06+00'
FROM "Employees" e WHERE e."EmpNum" = 2157
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Soung (Ken) Park Resume.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2026A/EmpDataItem_20260210111154879170_HQ.pdf', true, '2026-02-10 11:11:54+00', '2026-02-10 11:11:54+00'
FROM "Employees" e WHERE e."EmpNum" = 2158
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Ken Park (App-Resume) - A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2026A/EmpDataItem_20260210111154412518_HQ.pdf', true, '2026-02-10 11:11:54+00', '2026-02-10 11:11:54+00'
FROM "Employees" e WHERE e."EmpNum" = 2158
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Ruben Angiano (App-Resume) - A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2026A/EmpDataItem_20260210111204269374_HQ.pdf', true, '2026-02-10 11:12:04+00', '2026-02-10 11:12:04+00'
FROM "Employees" e WHERE e."EmpNum" = 2158
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Loreto Ferrer (App-Resume)- A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2026A/EmpDataItem_20260210113932978886_HQ.pdf', true, '2026-02-10 11:39:32+00', '2026-02-10 11:39:32+00'
FROM "Employees" e WHERE e."EmpNum" = 2159
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Jose (Luis) Ramirez(App-Resume) -A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2026A/EmpDataItem_20260210021105749365_HQ.pdf', true, '2026-02-10 14:11:05+00', '2026-02-10 14:11:05+00'
FROM "Employees" e WHERE e."EmpNum" = 2083
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Monica Hong (App-Resume) - A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2026A/EmpDataItem_20260210022222879136_HQ.pdf', true, '2026-02-10 14:22:22+00', '2026-02-10 14:22:22+00'
FROM "Employees" e WHERE e."EmpNum" = 2147
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Monica Hong (Resume).pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2026A/EmpDataItem_20260210022224154687_HQ.pdf', true, '2024-01-01 00:00:00+00', '2024-01-01 00:00:00+00'
FROM "Employees" e WHERE e."EmpNum" = 2147
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Resume - YoungSeok Kim.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2026A/EmpDataItem_20260210022533358592_HQ.pdf', true, '2026-02-10 14:25:33+00', '2026-02-10 14:25:33+00'
FROM "Employees" e WHERE e."EmpNum" = 2148
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Kevin Kim (App-Resume) - A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2026A/EmpDataItem_20260210022549851199_HQ.pdf', true, '2026-02-10 14:25:49+00', '2026-02-10 14:25:49+00'
FROM "Employees" e WHERE e."EmpNum" = 2148
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Brian Chaparro (Resume).pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2026A/EmpDataItem_20260210023606233260_HQ.pdf', true, '2026-02-10 14:36:06+00', '2026-02-10 14:36:06+00'
FROM "Employees" e WHERE e."EmpNum" = 2149
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Brian Chaparro  (App-Resume) - A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2026A/EmpDataItem_20260210023631782309_HQ.pdf', true, '2026-02-10 14:36:31+00', '2026-02-10 14:36:31+00'
FROM "Employees" e WHERE e."EmpNum" = 2149
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Alexander Vazzu - Resume.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2026A/EmpDataItem_20260210024118357493_HQ.pdf', true, '2026-02-10 14:41:18+00', '2026-02-10 14:41:18+00'
FROM "Employees" e WHERE e."EmpNum" = 2150
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Alexander Vazzu (App-Resume) - A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2026A/EmpDataItem_20260210024125032703_HQ.pdf', true, '2024-01-01 00:00:00+00', '2024-01-01 00:00:00+00'
FROM "Employees" e WHERE e."EmpNum" = 2150
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Resume.PDF', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2026A/EmpDataItem_20260210024219680771_HQ.pdf', true, '2024-01-01 00:00:00+00', '2024-01-01 00:00:00+00'
FROM "Employees" e WHERE e."EmpNum" = 2151
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Tom Park (App-Resume) - A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2026A/EmpDataItem_20260210024226302064_HQ.pdf', true, '2026-02-10 14:42:26+00', '2026-02-10 14:42:26+00'
FROM "Employees" e WHERE e."EmpNum" = 2151
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Hyung Suk Kim_Resume.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2026A/EmpDataItem_20260210030337085210_HQ.pdf', true, '2026-02-10 15:03:37+00', '2026-02-10 15:03:37+00'
FROM "Employees" e WHERE e."EmpNum" = 2160
ON CONFLICT DO NOTHING;
INSERT INTO "EmployeeDocuments" ("EmployeeId", "FileName", "StoredFileName", "Extension", "FileSizeBytes", "UploadedByName", "LegacyPath", "IsActive", "CreatedAt", "UpdatedAt")
SELECT e."Id", 'Hyung Suk Kim (App-Resume)-A.pdf', '', 'pdf', 0, 'Legacy Import', '/home/bpms/nas1_acisystem_files/FileItems/Hr/EmpDataItems/2026A/EmpDataItem_20260210030428828667_HQ.pdf', true, '2026-02-10 15:04:28+00', '2026-02-10 15:04:28+00'
FROM "Employees" e WHERE e."EmpNum" = 2160
ON CONFLICT DO NOTHING;

COMMIT;

-- Total records: 146, Inserted: 130, Skipped: 16