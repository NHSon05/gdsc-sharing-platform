-- =============================================================================
-- GDSC SHARING PLATFORM - PRODUCTION SEED SCRIPT
-- Idempotent: Có thể chạy an toàn nhiều lần mà không làm mất dữ liệu hay lỗi khóa
-- =============================================================================

BEGIN;

-- 1. TẠO SCHEMA & ROLES HỆ THỐNG NẾU CHƯA CÓ
CREATE SCHEMA IF NOT EXISTS gdsc;

INSERT INTO gdsc."Roles" ("Id", "Name", "NormalizedName", "ConcurrencyStamp")
VALUES 
    ('3f634639-0ac8-4d47-9e6a-240ea8ba75b1', 'Admin', 'ADMIN', gen_random_uuid()::text),
    ('e1fa0354-8143-45ee-9043-923f7d902c51', 'Member', 'MEMBER', gen_random_uuid()::text)
ON CONFLICT ("NormalizedName") DO NOTHING;

-- 2. TẠO BAN CHUYÊN MÔN (DEPARTMENTS)
INSERT INTO gdsc."Departments" ("Id", "Code", "Name", "Slug", "Description", "DisplayOrder", "SortOrder", "Color", "Icon", "IsActive", "CreatedAt", "CreatedAtUtc")
VALUES 
    ('d551067c-624f-484a-b51c-e22d329d0375', 'MANAGEMENT', 'Management', 'management', 'Club management department', 1, 1, '#64748B', 'briefcase', true, NOW(), NOW()),
    ('235294e1-d97e-4659-ba74-61a490a601d8', 'SOFTWARE', 'Software', 'software', 'Software development department', 2, 10, '#3B82F6', 'code', true, NOW(), NOW()),
    ('d059c3d1-a21b-425c-9402-72600135e9ff', 'AI', 'AI', 'ai', 'Artificial intelligence and data science', 20, 20, '#8B5CF6', 'cpu', true, NOW(), NOW()),
    ('99c3d018-4416-4efb-94cf-c787d9425c46', 'R&D', 'R&D', 'rd', 'Research and development department', 3, 3, '#06B6D4', 'flask', true, NOW(), NOW()),
    ('af1646cd-1839-487f-a3a8-80bd13952dd1', 'MARKETING', 'Marketing', 'marketing', 'Marketing department', 4, 30, '#EC4899', 'megaphone', true, NOW(), NOW()),
    ('6021569c-b4aa-49b6-a6c3-76c3f588bfd2', 'MEDIA', 'Media', 'media', 'Media and design department', 40, 40, '#F59E0B', 'camera', true, NOW(), NOW()),
    ('5b4d457f-373e-4c76-8857-683b35b543b4', 'COMMUNITY', 'Community', 'community', 'Community engagement department', 50, 50, '#10B981', 'users', true, NOW(), NOW())
ON CONFLICT ("Code") DO UPDATE 
SET "Slug" = EXCLUDED."Slug", "Name" = EXCLUDED."Name", "Color" = EXCLUDED."Color", "Icon" = EXCLUDED."Icon";

-- 3. TẠO CHỨC DANH CÂU LẠC BỘ (CLUB ROLES)
INSERT INTO gdsc."ClubRoles" ("Id", "Code", "Name", "Level", "IsActive", "CreatedAtUtc")
VALUES 
    ('53adeb89-8e13-420d-a7bb-2c90461bc151', 'LEAD', 'Lead', 10, true, NOW()),
    ('1acd7371-ea15-4704-8804-767b0c839290', 'SUBLEAD', 'Sub Lead', 20, true, NOW()),
    ('357e31aa-5f00-48c1-9f8c-4743a89f5a91', 'CORETEAM', 'Core Team', 30, true, NOW()),
    ('11bb413f-9856-4270-9b0d-cb5b099b80a7', 'ALUMNI', 'Alumni', 40, true, NOW())
ON CONFLICT ("Code") DO UPDATE 
SET "Name" = EXCLUDED."Name", "Level" = EXCLUDED."Level", "IsActive" = true;

-- 4. TẠO THẾ HỆ (CLUB GENERATION - GEN 8)
INSERT INTO gdsc."ClubGenerations" ("Id", "Number", "Name", "StartDate", "EndDate", "IsActive", "CreatedAtUtc")
VALUES ('3a9254c7-1044-48b4-82a9-0b1e4c8f5968', 8, 'Gen 8', '2025-09-01', '2026-08-31', true, NOW())
ON CONFLICT ("Number") DO UPDATE SET "Name" = 'Gen 8', "IsActive" = true;

-- 5. SEED VÀ LIÊN KẾT MEMBERSHIP CHO ADMIN & MEMBER
DO $$
DECLARE
    v_gen8_id uuid;
    v_dept_software_id uuid;
    v_dept_mgmt_id uuid;
    v_role_lead_id uuid;
    v_role_admin_id uuid;
    v_role_member_id uuid;
    v_admin_id uuid;
    v_member_id uuid;
    v_admin_club_mem_id uuid;
    v_admin_dept_mem_id uuid;
    v_member_club_mem_id uuid;
    v_member_dept_mem_id uuid;
BEGIN
    SELECT "Id" INTO v_gen8_id FROM gdsc."ClubGenerations" WHERE "Number" = 8 LIMIT 1;
    SELECT "Id" INTO v_dept_software_id FROM gdsc."Departments" WHERE "Code" = 'SOFTWARE' LIMIT 1;
    SELECT "Id" INTO v_dept_mgmt_id FROM gdsc."Departments" WHERE "Code" = 'MANAGEMENT' LIMIT 1;
    SELECT "Id" INTO v_role_lead_id FROM gdsc."ClubRoles" WHERE "Code" = 'LEAD' LIMIT 1;
    SELECT "Id" INTO v_role_admin_id FROM gdsc."Roles" WHERE "NormalizedName" = 'ADMIN' LIMIT 1;
    SELECT "Id" INTO v_role_member_id FROM gdsc."Roles" WHERE "NormalizedName" = 'MEMBER' LIMIT 1;

    -- ========================================================
    -- A. SEED ADMIN ACCOUNT (admin@gdsc.com / admin@gdsc.dev)
    -- Mật khẩu mặc định: AdminPassword@123!
    -- ========================================================
    SELECT "Id" INTO v_admin_id FROM gdsc."Users" WHERE "NormalizedEmail" IN ('ADMIN@GDSC.COM', 'ADMIN@GDSC.DEV') LIMIT 1;
    
    IF v_admin_id IS NULL THEN
        v_admin_id := '1cd84f16-2709-4718-a039-776a536dd20d';
        INSERT INTO gdsc."Users" (
            "Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", "EmailConfirmed",
            "FullName", "DisplayName", "PhoneNumber", "StudentCode", "GitHubUrl", "Bio",
            "DepartmentId", "Generation", "Status", "JoinedAt", "CreatedAt", "UpdatedAt",
            "TimeZone", "Locale", "IsDeleted", "PasswordHash", "SecurityStamp", "ConcurrencyStamp",
            "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnabled", "AccessFailedCount", "TokenVersion"
        ) VALUES (
            v_admin_id, 'admin@gdsc.com', 'ADMIN@GDSC.COM', 'admin@gdsc.com', 'ADMIN@GDSC.COM', true,
            'System Administrator', 'System Administrator', '0901234567', 'ADMIN001', 'https://github.com/gdsc-admin', 'System Administrator of GDSC Sharing Platform',
            v_dept_mgmt_id, 'Gen 8', 1, NOW(), NOW(), NOW(),
            'Asia/Ho_Chi_Minh', 'vi-VN', false,
            'AQAAAAIAAYagAAAAEKJC+2MNpq3rt8tzTEMKOr+QTI6xKZjp7xT2jvLXRYBTYEwCZVsUyuLlvyX5bWHcFA==',
            gen_random_uuid()::text, gen_random_uuid()::text,
            false, false, true, 0, 0
        );
    ELSE
        UPDATE gdsc."Users" SET
            "FullName" = 'System Administrator',
            "DisplayName" = 'System Administrator',
            "PhoneNumber" = '0901234567',
            "StudentCode" = 'ADMIN001',
            "GitHubUrl" = 'https://github.com/gdsc-admin',
            "Bio" = 'System Administrator of GDSC Sharing Platform',
            "DepartmentId" = v_dept_mgmt_id,
            "Generation" = 'Gen 8',
            "Status" = 1,
            "IsDeleted" = false,
            "EmailConfirmed" = true,
            "AccessFailedCount" = 0,
            "LockoutEnd" = NULL,
            "PasswordHash" = 'AQAAAAIAAYagAAAAEKJC+2MNpq3rt8tzTEMKOr+QTI6xKZjp7xT2jvLXRYBTYEwCZVsUyuLlvyX5bWHcFA==',
            "UpdatedAt" = NOW()
        WHERE "Id" = v_admin_id;
    END IF;

    -- Gán vai trò Identity Admin
    DELETE FROM gdsc."UserRoles" WHERE "UserId" = v_admin_id;
    INSERT INTO gdsc."UserRoles" ("UserId", "RoleId") VALUES (v_admin_id, v_role_admin_id);

    -- Gán cây phòng ban cho Admin (Ban Management, Gen 8, Role LEAD)
    SELECT "Id" INTO v_admin_club_mem_id FROM gdsc."ClubMemberships" WHERE "UserId" = v_admin_id AND "GenerationId" = v_gen8_id LIMIT 1;
    IF v_admin_club_mem_id IS NULL THEN
        v_admin_club_mem_id := gen_random_uuid();
        INSERT INTO gdsc."ClubMemberships" ("Id", "UserId", "GenerationId", "JoinedAt", "IsActive", "CreatedAtUtc")
        VALUES (v_admin_club_mem_id, v_admin_id, v_gen8_id, CURRENT_DATE, true, NOW());
    END IF;

    SELECT "Id" INTO v_admin_dept_mem_id FROM gdsc."DepartmentMemberships" WHERE "ClubMembershipId" = v_admin_club_mem_id AND "DepartmentId" = v_dept_mgmt_id LIMIT 1;
    IF v_admin_dept_mem_id IS NULL THEN
        v_admin_dept_mem_id := gen_random_uuid();
        INSERT INTO gdsc."DepartmentMemberships" ("Id", "ClubMembershipId", "DepartmentId", "IsPrimary", "JoinedAt", "IsActive", "CreatedAtUtc")
        VALUES (v_admin_dept_mem_id, v_admin_club_mem_id, v_dept_mgmt_id, true, CURRENT_DATE, true, NOW());
    END IF;

    INSERT INTO gdsc."RoleAssignments" ("Id", "DepartmentMembershipId", "ClubRoleId", "AssignedByUserId", "AssignedAtUtc", "IsActive", "CreatedAtUtc")
    VALUES (gen_random_uuid(), v_admin_dept_mem_id, v_role_lead_id, v_admin_id, NOW(), true, NOW())
    ON CONFLICT DO NOTHING;

    -- ========================================================
    -- B. SEED MEMBER ACCOUNT (snguyenhong8@gmail.com)
    -- Mật khẩu mặc định: MemberPassword@123!
    -- ========================================================
    SELECT "Id" INTO v_member_id FROM gdsc."Users" WHERE "NormalizedEmail" = 'SNGUYENHONG8@GMAIL.COM' LIMIT 1;

    IF v_member_id IS NULL THEN
        v_member_id := '3fa85f64-5717-4562-b3fc-2c963f66afa6';
        INSERT INTO gdsc."Users" (
            "Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", "EmailConfirmed",
            "FullName", "DisplayName", "PhoneNumber", "StudentCode", "GitHubUrl", "Bio",
            "DepartmentId", "Generation", "Status", "JoinedAt", "CreatedAt", "UpdatedAt",
            "TimeZone", "Locale", "IsDeleted", "PasswordHash", "SecurityStamp", "ConcurrencyStamp",
            "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnabled", "AccessFailedCount", "TokenVersion"
        ) VALUES (
            v_member_id, 'snguyenhong8@gmail.com', 'SNGUYENHONG8@GMAIL.COM', 'snguyenhong8@gmail.com', 'SNGUYENHONG8@GMAIL.COM', true,
            'Nguyễn Hồng Sơn', 'Nguyễn Hồng Sơn', '0387756949', '123230167', 'https://github.com/NHSon05', 'FrontEnd',
            v_dept_software_id, 'Gen 8', 1, NOW(), NOW(), NOW(),
            'Asia/Ho_Chi_Minh', 'vi-VN', false,
            'AQAAAAIAAYagAAAAENq/DqVZ5b9tjfOf8BRADrpRknPEjOqeDPYLlLMyWdRqYrT4/tORourVfi3fA3umaQ==',
            gen_random_uuid()::text, gen_random_uuid()::text,
            false, false, true, 0, 0
        );
    ELSE
        UPDATE gdsc."Users" SET
            "FullName" = 'Nguyễn Hồng Sơn',
            "DisplayName" = 'Nguyễn Hồng Sơn',
            "PhoneNumber" = '0387756949',
            "StudentCode" = '123230167',
            "GitHubUrl" = 'https://github.com/NHSon05',
            "Bio" = 'FrontEnd',
            "DepartmentId" = v_dept_software_id,
            "Generation" = 'Gen 8',
            "Status" = 1,
            "IsDeleted" = false,
            "EmailConfirmed" = true,
            "AccessFailedCount" = 0,
            "LockoutEnd" = NULL,
            "PasswordHash" = 'AQAAAAIAAYagAAAAENq/DqVZ5b9tjfOf8BRADrpRknPEjOqeDPYLlLMyWdRqYrT4/tORourVfi3fA3umaQ==',
            "UpdatedAt" = NOW()
        WHERE "Id" = v_member_id;
    END IF;

    -- Gán vai trò Identity Member
    DELETE FROM gdsc."UserRoles" WHERE "UserId" = v_member_id;
    INSERT INTO gdsc."UserRoles" ("UserId", "RoleId") VALUES (v_member_id, v_role_member_id);

    -- Gán cây phòng ban cho Member (Ban Software, Gen 8, Role LEAD)
    SELECT "Id" INTO v_member_club_mem_id FROM gdsc."ClubMemberships" WHERE "UserId" = v_member_id AND "GenerationId" = v_gen8_id LIMIT 1;
    IF v_member_club_mem_id IS NULL THEN
        v_member_club_mem_id := gen_random_uuid();
        INSERT INTO gdsc."ClubMemberships" ("Id", "UserId", "GenerationId", "JoinedAt", "IsActive", "CreatedAtUtc")
        VALUES (v_member_club_mem_id, v_member_id, v_gen8_id, CURRENT_DATE, true, NOW());
    END IF;

    SELECT "Id" INTO v_member_dept_mem_id FROM gdsc."DepartmentMemberships" WHERE "ClubMembershipId" = v_member_club_mem_id AND "DepartmentId" = v_dept_software_id LIMIT 1;
    IF v_member_dept_mem_id IS NULL THEN
        v_member_dept_mem_id := gen_random_uuid();
        INSERT INTO gdsc."DepartmentMemberships" ("Id", "ClubMembershipId", "DepartmentId", "IsPrimary", "JoinedAt", "IsActive", "CreatedAtUtc")
        VALUES (v_member_dept_mem_id, v_member_club_mem_id, v_dept_software_id, true, CURRENT_DATE, true, NOW());
    END IF;

    INSERT INTO gdsc."RoleAssignments" ("Id", "DepartmentMembershipId", "ClubRoleId", "AssignedByUserId", "AssignedAtUtc", "IsActive", "CreatedAtUtc")
    VALUES (gen_random_uuid(), v_member_dept_mem_id, v_role_lead_id, v_admin_id, NOW(), true, NOW())
    ON CONFLICT DO NOTHING;

END $$;

COMMIT;
