-- EmpRoles migration from old system to aci-connect_v5
-- Generated: 2026-04-29 11:32:37
-- Uses WHERE NOT EXISTS to avoid duplicates

BEGIN;

-- EmpRole_20240726_01017 → Emp: Emp_20240726_01017
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 17, 9, 1, false, NULL, NULL, 'OBID:EmpRole_20240726_01017', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 17 AND "OrgUnitId" = 9 AND "JobPositionId" = 1
);

-- EmpRole_20240726_01002 → Emp: Emp_20240726_01002
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 2, 7, 27, false, NULL, NULL, 'OBID:EmpRole_20240726_01002', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 2 AND "OrgUnitId" = 7 AND "JobPositionId" = 27
);

-- EmpRole_20240726_01004 → Emp: Emp_20240726_01004
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 4, 2, 19, false, NULL, NULL, 'OBID:EmpRole_20240726_01004', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 4 AND "OrgUnitId" = 2 AND "JobPositionId" = 19
);

-- EmpRole_20240726_01011 → Emp: Emp_20240726_01011
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 11, 9, 3, false, NULL, NULL, 'OBID:EmpRole_20240726_01011', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 11 AND "OrgUnitId" = 9 AND "JobPositionId" = 3
);

-- EmpRole_20240726_01014 → Emp: Emp_20240726_01014
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 14, 2, 19, false, NULL, NULL, 'OBID:EmpRole_20240726_01014', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 14 AND "OrgUnitId" = 2 AND "JobPositionId" = 19
);

-- EmpRole_20240726_01022 → Emp: Emp_20240726_01022
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 22, 9, 3, false, NULL, NULL, 'OBID:EmpRole_20240726_01022', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 22 AND "OrgUnitId" = 9 AND "JobPositionId" = 3
);

-- EmpRole_20240726_01023 → Emp: Emp_20240726_01023
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 23, 2, 19, false, NULL, NULL, 'OBID:EmpRole_20240726_01023', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 23 AND "OrgUnitId" = 2 AND "JobPositionId" = 19
);

-- EmpRole_20240726_01024 → Emp: Emp_20240726_01024
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 24, 9, 3, false, NULL, NULL, 'OBID:EmpRole_20240726_01024', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 24 AND "OrgUnitId" = 9 AND "JobPositionId" = 3
);

-- EmpRole_20240726_01025 → Emp: Emp_20240726_01025
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 25, 2, 19, false, NULL, NULL, 'OBID:EmpRole_20240726_01025', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 25 AND "OrgUnitId" = 2 AND "JobPositionId" = 19
);

-- EmpRole_20240726_01026 → Emp: Emp_20240726_01026
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 26, 2, 19, false, NULL, NULL, 'OBID:EmpRole_20240726_01026', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 26 AND "OrgUnitId" = 2 AND "JobPositionId" = 19
);

-- EmpRole_20240726_01028 → Emp: Emp_20240726_01028
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 28, 9, 3, false, NULL, NULL, 'OBID:EmpRole_20240726_01028', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 28 AND "OrgUnitId" = 9 AND "JobPositionId" = 3
);

-- EmpRole_20240726_01029 → Emp: Emp_20240726_01029
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 29, 2, 19, false, NULL, NULL, 'OBID:EmpRole_20240726_01029', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 29 AND "OrgUnitId" = 2 AND "JobPositionId" = 19
);

-- EmpRole_20240726_01030 → Emp: Emp_20240726_01030
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 30, 9, 3, false, NULL, NULL, 'OBID:EmpRole_20240726_01030', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 30 AND "OrgUnitId" = 9 AND "JobPositionId" = 3
);

-- EmpRole_20240726_01032 → Emp: Emp_20240726_01032
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 32, 9, 3, false, NULL, NULL, 'OBID:EmpRole_20240726_01032', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 32 AND "OrgUnitId" = 9 AND "JobPositionId" = 3
);

-- EmpRole_20240726_01033 → Emp: Emp_20240726_01033
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 33, 9, 3, false, NULL, NULL, 'OBID:EmpRole_20240726_01033', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 33 AND "OrgUnitId" = 9 AND "JobPositionId" = 3
);

-- EmpRole_20240726_01035 → Emp: Emp_20240726_01035
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 35, 9, 5, false, NULL, NULL, 'OBID:EmpRole_20240726_01035', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 35 AND "OrgUnitId" = 9 AND "JobPositionId" = 5
);

-- EmpRole_20240726_01037 → Emp: Emp_20240726_01037
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 37, 7, 27, false, NULL, NULL, 'OBID:EmpRole_20240726_01037', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 37 AND "OrgUnitId" = 7 AND "JobPositionId" = 27
);

-- EmpRole_20240726_01003 → Emp: Emp_20240726_01003
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 3, 8, 16, false, NULL, NULL, 'OBID:EmpRole_20240726_01003', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 3 AND "OrgUnitId" = 8 AND "JobPositionId" = 16
);

-- EmpRole_20240726_01006 → Emp: Emp_20240726_01006
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 6, 8, 20, false, NULL, NULL, 'OBID:EmpRole_20240726_01006', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 6 AND "OrgUnitId" = 8 AND "JobPositionId" = 20
);

-- EmpRole_20240726_01008 → Emp: Emp_20240726_01008
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 8, 9, 5, false, NULL, NULL, 'OBID:EmpRole_20240726_01008', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 8 AND "OrgUnitId" = 9 AND "JobPositionId" = 5
);

-- EmpRole_20240726_01034 → Emp: Emp_20240726_01034
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 34, 10, 22, false, NULL, NULL, 'OBID:EmpRole_20240726_01034', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 34 AND "OrgUnitId" = 10 AND "JobPositionId" = 22
);

-- EmpRole_20240726_01036 → Emp: Emp_20240726_01036
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 36, 12, 11, false, NULL, NULL, 'OBID:EmpRole_20240726_01036', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 36 AND "OrgUnitId" = 12 AND "JobPositionId" = 11
);

-- EmpRole_20240726_01031 → Emp: Emp_20240726_01031
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 31, 12, 7, false, NULL, NULL, 'OBID:EmpRole_20240726_01031', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 31 AND "OrgUnitId" = 12 AND "JobPositionId" = 7
);

-- EmpRole_20240726_01021 → Emp: Emp_20240726_01021
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 21, 12, 9, false, NULL, NULL, 'OBID:EmpRole_20240726_01021', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 21 AND "OrgUnitId" = 12 AND "JobPositionId" = 9
);

-- EmpRole_20240726_01012 → Emp: Emp_20240726_01012
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 12, 4, 14, false, NULL, NULL, 'OBID:EmpRole_20240726_01012', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 12 AND "OrgUnitId" = 4 AND "JobPositionId" = 14
);

-- EmpRole_20240726_01016 → Emp: Emp_20240726_01016
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 16, 1, 21, false, NULL, NULL, 'OBID:EmpRole_20240726_01016', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 16 AND "OrgUnitId" = 1 AND "JobPositionId" = 21
);

-- EmpRole_20240726_01007 → Emp: Emp_20240726_01007
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 7, 5, 15, false, NULL, NULL, 'OBID:EmpRole_20240726_01007', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 7 AND "OrgUnitId" = 5 AND "JobPositionId" = 15
);

-- EmpRole_20240726_01027 → Emp: Emp_20240726_01027
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 27, 9, 24, false, NULL, NULL, 'OBID:EmpRole_20240726_01027', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 27 AND "OrgUnitId" = 9 AND "JobPositionId" = 24
);

-- EmpRole_20240726_01020 → Emp: Emp_20240726_01020
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 20, 8, 25, false, NULL, NULL, 'OBID:EmpRole_20240726_01020', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 20 AND "OrgUnitId" = 8 AND "JobPositionId" = 25
);

-- EmpRole_20240726_01055 → Emp: Emp_20240726_01055
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 55, 7, 5, false, NULL, NULL, 'OBID:EmpRole_20240726_01055', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 55 AND "OrgUnitId" = 7 AND "JobPositionId" = 5
);

-- EmpRole_20240726_01050 → Emp: Emp_20240726_01050
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 50, 9, 4, false, NULL, NULL, 'OBID:EmpRole_20240726_01050', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 50 AND "OrgUnitId" = 9 AND "JobPositionId" = 4
);

-- EmpRole_20240726_01059 → Emp: Emp_20240726_01059
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 59, 9, 29, false, NULL, NULL, 'OBID:EmpRole_20240726_01059', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 59 AND "OrgUnitId" = 9 AND "JobPositionId" = 29
);

-- EmpRole_20240726_01038 → Emp: Emp_20240726_01038
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 38, 12, 7, false, NULL, NULL, 'OBID:EmpRole_20240726_01038', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 38 AND "OrgUnitId" = 12 AND "JobPositionId" = 7
);

-- EmpRole_20240726_01039 → Emp: Emp_20240726_01039
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 39, 9, 3, false, NULL, NULL, 'OBID:EmpRole_20240726_01039', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 39 AND "OrgUnitId" = 9 AND "JobPositionId" = 3
);

-- EmpRole_20240726_01040 → Emp: Emp_20240726_01040
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 40, 9, 3, false, NULL, NULL, 'OBID:EmpRole_20240726_01040', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 40 AND "OrgUnitId" = 9 AND "JobPositionId" = 3
);

-- EmpRole_20240726_01041 → Emp: Emp_20240726_01041
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 41, 2, 19, false, NULL, NULL, 'OBID:EmpRole_20240726_01041', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 41 AND "OrgUnitId" = 2 AND "JobPositionId" = 19
);

-- EmpRole_20240726_01042 → Emp: Emp_20240726_01042
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 42, 12, 9, false, NULL, NULL, 'OBID:EmpRole_20240726_01042', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 42 AND "OrgUnitId" = 12 AND "JobPositionId" = 9
);

-- EmpRole_20240726_01043 → Emp: Emp_20240726_01043
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 43, 2, 19, false, NULL, NULL, 'OBID:EmpRole_20240726_01043', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 43 AND "OrgUnitId" = 2 AND "JobPositionId" = 19
);

-- EmpRole_20240726_01044 → Emp: Emp_20240726_01044
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 44, 9, 3, false, NULL, NULL, 'OBID:EmpRole_20240726_01044', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 44 AND "OrgUnitId" = 9 AND "JobPositionId" = 3
);

-- EmpRole_20240726_01045 → Emp: Emp_20240726_01045
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 45, 9, 3, false, NULL, NULL, 'OBID:EmpRole_20240726_01045', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 45 AND "OrgUnitId" = 9 AND "JobPositionId" = 3
);

-- EmpRole_20240726_01047 → Emp: Emp_20240726_01047
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 47, 2, 19, false, NULL, NULL, 'OBID:EmpRole_20240726_01047', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 47 AND "OrgUnitId" = 2 AND "JobPositionId" = 19
);

-- EmpRole_20240726_01048 → Emp: Emp_20240726_01048
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 48, 2, 19, false, NULL, NULL, 'OBID:EmpRole_20240726_01048', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 48 AND "OrgUnitId" = 2 AND "JobPositionId" = 19
);

-- EmpRole_20240726_01083 → Emp: Emp_20240726_01083
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 83, 7, 13, false, NULL, NULL, 'OBID:EmpRole_20240726_01083', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 83 AND "OrgUnitId" = 7 AND "JobPositionId" = 13
);

-- EmpRole_20240726_01088 → Emp: Emp_20240726_01088
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 88, 12, 8, false, NULL, NULL, 'OBID:EmpRole_20240726_01088', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 88 AND "OrgUnitId" = 12 AND "JobPositionId" = 8
);

-- EmpRole_20240726_01049 → Emp: Emp_20240726_01049
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 49, 10, 10, false, NULL, NULL, 'OBID:EmpRole_20240726_01049', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 49 AND "OrgUnitId" = 10 AND "JobPositionId" = 10
);

-- EmpRole_20240726_01046 → Emp: Emp_20240726_01046
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 46, 11, 17, false, NULL, NULL, 'OBID:EmpRole_20240726_01046', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 46 AND "OrgUnitId" = 11 AND "JobPositionId" = 17
);

-- EmpRole_20240726_01060 → Emp: Emp_20240726_01060
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 60, 10, 18, false, NULL, NULL, 'OBID:EmpRole_20240726_01060', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 60 AND "OrgUnitId" = 10 AND "JobPositionId" = 18
);

-- EmpRole_20240726_01067 → Emp: Emp_20240726_01067
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 67, 2, 23, false, NULL, NULL, 'OBID:EmpRole_20240726_01067', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 67 AND "OrgUnitId" = 2 AND "JobPositionId" = 23
);

-- EmpRole_20240726_01005 → Emp: Emp_20240726_01005
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 5, 4, 12, false, NULL, NULL, 'OBID:EmpRole_20240726_01005', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 5 AND "OrgUnitId" = 4 AND "JobPositionId" = 12
);

-- EmpRole_20240726_01001 → Emp: Emp_20240726_01001
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 1, 9, 3, false, NULL, NULL, 'OBID:EmpRole_20240726_01001', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 1 AND "OrgUnitId" = 9 AND "JobPositionId" = 3
);

-- EmpRole_20240726_01009 → Emp: Emp_20240726_01009
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 9, 9, 3, false, NULL, NULL, 'OBID:EmpRole_20240726_01009', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 9 AND "OrgUnitId" = 9 AND "JobPositionId" = 3
);

-- EmpRole_20240726_01010 → Emp: Emp_20240726_01010
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 10, 9, 3, false, NULL, NULL, 'OBID:EmpRole_20240726_01010', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 10 AND "OrgUnitId" = 9 AND "JobPositionId" = 3
);

-- EmpRole_20240726_01013 → Emp: Emp_20240726_01013
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 13, 9, 3, false, NULL, NULL, 'OBID:EmpRole_20240726_01013', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 13 AND "OrgUnitId" = 9 AND "JobPositionId" = 3
);

-- EmpRole_20240726_01015 → Emp: Emp_20240726_01015
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 15, 9, 3, false, NULL, NULL, 'OBID:EmpRole_20240726_01015', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 15 AND "OrgUnitId" = 9 AND "JobPositionId" = 3
);

-- EmpRole_20240726_01018 → Emp: Emp_20240726_01018
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 18, 9, 3, false, NULL, NULL, 'OBID:EmpRole_20240726_01018', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 18 AND "OrgUnitId" = 9 AND "JobPositionId" = 3
);

-- EmpRole_20240726_01019 → Emp: Emp_20240726_01019
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 19, 9, 3, false, NULL, NULL, 'OBID:EmpRole_20240726_01019', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 19 AND "OrgUnitId" = 9 AND "JobPositionId" = 3
);

-- EmpRole_20240726_01051 → Emp: Emp_20240726_01051
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 51, 9, 24, false, NULL, NULL, 'OBID:EmpRole_20240726_01051', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 51 AND "OrgUnitId" = 9 AND "JobPositionId" = 24
);

-- EmpRole_20240726_01052 → Emp: Emp_20240726_01052
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 52, 2, 19, false, NULL, NULL, 'OBID:EmpRole_20240726_01052', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 52 AND "OrgUnitId" = 2 AND "JobPositionId" = 19
);

-- EmpRole_20240726_01053 → Emp: Emp_20240726_01053
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 53, 7, 1, false, NULL, NULL, 'OBID:EmpRole_20240726_01053', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 53 AND "OrgUnitId" = 7 AND "JobPositionId" = 1
);

-- EmpRole_20240726_01054 → Emp: Emp_20240726_01054
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 54, 2, 19, false, NULL, NULL, 'OBID:EmpRole_20240726_01054', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 54 AND "OrgUnitId" = 2 AND "JobPositionId" = 19
);

-- EmpRole_20240726_01056 → Emp: Emp_20240726_01056
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 56, 9, 3, false, NULL, NULL, 'OBID:EmpRole_20240726_01056', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 56 AND "OrgUnitId" = 9 AND "JobPositionId" = 3
);

-- EmpRole_20240726_01057 → Emp: Emp_20240726_01057
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 57, 7, 1, false, NULL, NULL, 'OBID:EmpRole_20240726_01057', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 57 AND "OrgUnitId" = 7 AND "JobPositionId" = 1
);

-- EmpRole_20240726_01058 → Emp: Emp_20240726_01058
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 58, 7, 1, false, NULL, NULL, 'OBID:EmpRole_20240726_01058', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 58 AND "OrgUnitId" = 7 AND "JobPositionId" = 1
);

-- EmpRole_20240726_01061 → Emp: Emp_20240726_01061
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 61, 9, 3, false, NULL, NULL, 'OBID:EmpRole_20240726_01061', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 61 AND "OrgUnitId" = 9 AND "JobPositionId" = 3
);

-- EmpRole_20240726_01062 → Emp: Emp_20240726_01062
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 62, 9, 3, false, NULL, NULL, 'OBID:EmpRole_20240726_01062', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 62 AND "OrgUnitId" = 9 AND "JobPositionId" = 3
);

-- EmpRole_20240726_01063 → Emp: Emp_20240726_01063
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 63, 5, 15, false, NULL, NULL, 'OBID:EmpRole_20240726_01063', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 63 AND "OrgUnitId" = 5 AND "JobPositionId" = 15
);

-- EmpRole_20240726_01064 → Emp: Emp_20240726_01064
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 64, 12, 7, false, NULL, NULL, 'OBID:EmpRole_20240726_01064', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 64 AND "OrgUnitId" = 12 AND "JobPositionId" = 7
);

-- EmpRole_20240726_01065 → Emp: Emp_20240726_01065
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 65, 2, 19, false, NULL, NULL, 'OBID:EmpRole_20240726_01065', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 65 AND "OrgUnitId" = 2 AND "JobPositionId" = 19
);

-- EmpRole_20240726_01066 → Emp: Emp_20240726_01066
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 66, 9, 3, false, NULL, NULL, 'OBID:EmpRole_20240726_01066', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 66 AND "OrgUnitId" = 9 AND "JobPositionId" = 3
);

-- EmpRole_20240726_01068 → Emp: Emp_20240726_01068
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 68, 9, 3, false, NULL, NULL, 'OBID:EmpRole_20240726_01068', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 68 AND "OrgUnitId" = 9 AND "JobPositionId" = 3
);

-- EmpRole_20240726_01069 → Emp: Emp_20240726_01069
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 69, 7, 5, false, NULL, NULL, 'OBID:EmpRole_20240726_01069', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 69 AND "OrgUnitId" = 7 AND "JobPositionId" = 5
);

-- EmpRole_20240726_01070 → Emp: Emp_20240726_01070
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 70, 9, 3, false, NULL, NULL, 'OBID:EmpRole_20240726_01070', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 70 AND "OrgUnitId" = 9 AND "JobPositionId" = 3
);

-- EmpRole_20240726_01071 → Emp: Emp_20240726_01071
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 71, 7, 1, false, NULL, NULL, 'OBID:EmpRole_20240726_01071', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 71 AND "OrgUnitId" = 7 AND "JobPositionId" = 1
);

-- EmpRole_20240726_01072 → Emp: Emp_20240726_01072
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 72, 2, 19, false, NULL, NULL, 'OBID:EmpRole_20240726_01072', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 72 AND "OrgUnitId" = 2 AND "JobPositionId" = 19
);

-- EmpRole_20240726_01073 → Emp: Emp_20240726_01073
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 73, 7, 1, false, NULL, NULL, 'OBID:EmpRole_20240726_01073', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 73 AND "OrgUnitId" = 7 AND "JobPositionId" = 1
);

-- EmpRole_20240726_01074 → Emp: Emp_20240726_01074
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 74, 9, 3, false, NULL, NULL, 'OBID:EmpRole_20240726_01074', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 74 AND "OrgUnitId" = 9 AND "JobPositionId" = 3
);

-- EmpRole_20240726_01075 → Emp: Emp_20240726_01075
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 75, 9, 4, false, NULL, NULL, 'OBID:EmpRole_20240726_01075', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 75 AND "OrgUnitId" = 9 AND "JobPositionId" = 4
);

-- EmpRole_20240726_01076 → Emp: Emp_20240726_01076
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 76, 7, 1, false, NULL, NULL, 'OBID:EmpRole_20240726_01076', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 76 AND "OrgUnitId" = 7 AND "JobPositionId" = 1
);

-- EmpRole_20240726_01077 → Emp: Emp_20240726_01077
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 77, 2, 19, false, NULL, NULL, 'OBID:EmpRole_20240726_01077', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 77 AND "OrgUnitId" = 2 AND "JobPositionId" = 19
);

-- EmpRole_20240726_01078 → Emp: Emp_20240726_01078
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 78, 9, 4, false, NULL, NULL, 'OBID:EmpRole_20240726_01078', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 78 AND "OrgUnitId" = 9 AND "JobPositionId" = 4
);

-- EmpRole_20240726_01079 → Emp: Emp_20240726_01079
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 79, 5, 15, false, NULL, NULL, 'OBID:EmpRole_20240726_01079', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 79 AND "OrgUnitId" = 5 AND "JobPositionId" = 15
);

-- EmpRole_20240726_01094 → Emp: Emp_20240726_01094
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 94, 7, 26, false, NULL, NULL, 'OBID:EmpRole_20240726_01094', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 94 AND "OrgUnitId" = 7 AND "JobPositionId" = 26
);

-- EmpRole_20240726_01081 → Emp: Emp_20240726_01081
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 81, 9, 4, false, NULL, NULL, 'OBID:EmpRole_20240726_01081', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 81 AND "OrgUnitId" = 9 AND "JobPositionId" = 4
);

-- EmpRole_20240726_01082 → Emp: Emp_20240726_01082
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 82, 9, 3, false, NULL, NULL, 'OBID:EmpRole_20240726_01082', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 82 AND "OrgUnitId" = 9 AND "JobPositionId" = 3
);

-- EmpRole_20240726_01084 → Emp: Emp_20240726_01084
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 84, 9, 3, false, NULL, NULL, 'OBID:EmpRole_20240726_01084', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 84 AND "OrgUnitId" = 9 AND "JobPositionId" = 3
);

-- EmpRole_20240726_01085 → Emp: Emp_20240726_01085
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 85, 9, 4, false, NULL, NULL, 'OBID:EmpRole_20240726_01085', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 85 AND "OrgUnitId" = 9 AND "JobPositionId" = 4
);

-- EmpRole_20240726_01086 → Emp: Emp_20240726_01086
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 86, 9, 3, false, NULL, NULL, 'OBID:EmpRole_20240726_01086', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 86 AND "OrgUnitId" = 9 AND "JobPositionId" = 3
);

-- EmpRole_20240726_01087 → Emp: Emp_20240726_01087
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 87, 9, 3, false, NULL, NULL, 'OBID:EmpRole_20240726_01087', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 87 AND "OrgUnitId" = 9 AND "JobPositionId" = 3
);

-- EmpRole_20240726_01089 → Emp: Emp_20240726_01089
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 89, 9, 3, false, NULL, NULL, 'OBID:EmpRole_20240726_01089', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 89 AND "OrgUnitId" = 9 AND "JobPositionId" = 3
);

-- EmpRole_20240726_01090 → Emp: Emp_20240726_01090
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 90, 7, 1, false, NULL, NULL, 'OBID:EmpRole_20240726_01090', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 90 AND "OrgUnitId" = 7 AND "JobPositionId" = 1
);

-- EmpRole_20240726_01091 → Emp: Emp_20240726_01091
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 91, 7, 5, false, NULL, NULL, 'OBID:EmpRole_20240726_01091', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 91 AND "OrgUnitId" = 7 AND "JobPositionId" = 5
);

-- EmpRole_20240726_01092 → Emp: Emp_20240726_01092
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 92, 9, 3, false, NULL, NULL, 'OBID:EmpRole_20240726_01092', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 92 AND "OrgUnitId" = 9 AND "JobPositionId" = 3
);

-- EmpRole_20240726_01093 → Emp: Emp_20240726_01093
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 93, 9, 4, false, NULL, NULL, 'OBID:EmpRole_20240726_01093', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 93 AND "OrgUnitId" = 9 AND "JobPositionId" = 4
);

-- EmpRole_20240726_01095 → Emp: Emp_20240726_01095
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 95, 9, 4, false, NULL, NULL, 'OBID:EmpRole_20240726_01095', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 95 AND "OrgUnitId" = 9 AND "JobPositionId" = 4
);

-- EmpRole_20240726_01096 → Emp: Emp_20240726_01096
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 96, 9, 24, false, NULL, NULL, 'OBID:EmpRole_20240726_01096', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 96 AND "OrgUnitId" = 9 AND "JobPositionId" = 24
);

-- EmpRole_20240726_01097 → Emp: Emp_20240726_01097
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 97, 9, 29, false, NULL, NULL, 'OBID:EmpRole_20240726_01097', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 97 AND "OrgUnitId" = 9 AND "JobPositionId" = 29
);

-- EmpRole_20240726_01080 → Emp: Emp_20240726_01080
INSERT INTO public."EmpRoles" ("EmployeeId", "OrgUnitId", "JobPositionId", "IsPrimary", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
SELECT 80, 7, 25, false, NULL, NULL, 'OBID:EmpRole_20240726_01080', NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM public."EmpRoles"
    WHERE "EmployeeId" = 80 AND "OrgUnitId" = 7 AND "JobPositionId" = 25
);

COMMIT;

-- Summary:
-- Processed: 97
-- Skipped: 15
--   SKIP EmpRole_20241105085148071907: no employee link
--   SKIP EmpRole_20241105085718481839: no employee link
--   SKIP EmpRole_20241113011845914421: no employee link
--   SKIP EmpRole_20241113030816969054: no employee link
--   SKIP EmpRole_20241113031055063265: no employee link
--   SKIP EmpRole_20241114100002555756: no employee link
--   SKIP EmpRole_20241114012734684122: no employee link
--   SKIP EmpRole_20241114014349072264: no employee link
--   SKIP EmpRole_20241114023408463411: no employee link
--   SKIP EmpRole_20241114023353748438: no employee link
--   SKIP EmpRole_20241114025400404445: no employee link
--   SKIP EmpRole_20241114031254680355: no employee link
--   SKIP EmpRole_20241114032337610391: no employee link
--   SKIP EmpRole_20241114033523728183: no employee link
--   SKIP EmpRole_20241114034708143631: no employee link