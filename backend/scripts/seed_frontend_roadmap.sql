-- =============================================================================
-- GDSC SHARING PLATFORM - FRONTEND DEVELOPER ROADMAP SEED SCRIPT
-- Idempotent script: Seeds complete roadmap.sh Frontend Developer graph
-- =============================================================================

BEGIN;

DO $$
DECLARE
    v_author_id uuid;
    v_category_id uuid;
    v_roadmap_id uuid := 'c7300001-6b87-4d48-9812-000000000001';
    
    -- Node UUIDs
    -- 1. Basics & Trunk
    n_internet uuid := 'c7300002-6b87-4d48-9812-000000000001';
    n_how_internet uuid := 'c7300002-6b87-4d48-9812-000000000002';
    n_what_http uuid := 'c7300002-6b87-4d48-9812-000000000003';
    n_domain_name uuid := 'c7300002-6b87-4d48-9812-000000000004';
    n_hosting uuid := 'c7300002-6b87-4d48-9812-000000000005';
    n_dns uuid := 'c7300002-6b87-4d48-9812-000000000006';
    n_browsers uuid := 'c7300002-6b87-4d48-9812-000000000007';
    n_html uuid := 'c7300002-6b87-4d48-9812-000000000008';
    n_css uuid := 'c7300002-6b87-4d48-9812-000000000009';
    n_javascript uuid := 'c7300002-6b87-4d48-9812-000000000010';
    n_version_control uuid := 'c7300002-6b87-4d48-9812-000000000011';
    n_git uuid := 'c7300002-6b87-4d48-9812-000000000012';
    n_vcs_hosting uuid := 'c7300002-6b87-4d48-9812-000000000013';
    n_github uuid := 'c7300002-6b87-4d48-9812-000000000014';
    n_gitlab uuid := 'c7300002-6b87-4d48-9812-000000000015';
    n_package_managers uuid := 'c7300002-6b87-4d48-9812-000000000016';
    n_npm uuid := 'c7300002-6b87-4d48-9812-000000000017';
    n_yarn uuid := 'c7300002-6b87-4d48-9812-000000000018';
    n_pnpm uuid := 'c7300002-6b87-4d48-9812-000000000019';
    n_bun uuid := 'c7300002-6b87-4d48-9812-000000000020';
    n_css_frameworks uuid := 'c7300002-6b87-4d48-9812-000000000021';
    n_tailwind uuid := 'c7300002-6b87-4d48-9812-000000000022';
    n_learn_framework uuid := 'c7300002-6b87-4d48-9812-000000000023';
    n_react uuid := 'c7300002-6b87-4d48-9812-000000000024';
    n_vue uuid := 'c7300002-6b87-4d48-9812-000000000025';
    n_angular uuid := 'c7300002-6b87-4d48-9812-000000000026';
    n_svelte uuid := 'c7300002-6b87-4d48-9812-000000000027';
    n_solid_js uuid := 'c7300002-6b87-4d48-9812-000000000028';

    -- 2. AI in Development
    n_ai_assisted uuid := 'c7300002-6b87-4d48-9812-000000000029';
    n_claude_code uuid := 'c7300002-6b87-4d48-9812-000000000030';
    n_cursor uuid := 'c7300002-6b87-4d48-9812-000000000031';
    n_copilot uuid := 'c7300002-6b87-4d48-9812-000000000032';
    n_antigravity uuid := 'c7300002-6b87-4d48-9812-000000000033';
    n_ai_basics uuid := 'c7300002-6b87-4d48-9812-000000000034';
    n_how_llms uuid := 'c7300002-6b87-4d48-9812-000000000035';
    n_ai_vs_trad uuid := 'c7300002-6b87-4d48-9812-000000000036';
    n_ai_apps uuid := 'c7300002-6b87-4d48-9812-000000000037';
    n_code_reviews uuid := 'c7300002-6b87-4d48-9812-000000000038';
    n_refactoring uuid := 'c7300002-6b87-4d48-9812-000000000039';
    n_docs_gen uuid := 'c7300002-6b87-4d48-9812-000000000040';
    n_prompting uuid := 'c7300002-6b87-4d48-9812-000000000041';
    n_prompt_eng uuid := 'c7300002-6b87-4d48-9812-000000000042';
    n_agents uuid := 'c7300002-6b87-4d48-9812-000000000043';
    n_ai_agents_rm uuid := 'c7300002-6b87-4d48-9812-000000000044';
    n_mcp uuid := 'c7300002-6b87-4d48-9812-000000000045';
    n_skills uuid := 'c7300002-6b87-4d48-9812-000000000046';
    n_implementing_ai uuid := 'c7300002-6b87-4d48-9812-000000000047';
    n_gemini uuid := 'c7300002-6b87-4d48-9812-000000000048';
    n_openai uuid := 'c7300002-6b87-4d48-9812-000000000049';
    n_anthropic uuid := 'c7300002-6b87-4d48-9812-000000000050';

    -- 3. Tooling, Bundlers, Linters & Auth
    n_auth_strategies uuid := 'c7300002-6b87-4d48-9812-000000000051';
    n_module_bundlers uuid := 'c7300002-6b87-4d48-9812-000000000052';
    n_vite uuid := 'c7300002-6b87-4d48-9812-000000000053';
    n_swc uuid := 'c7300002-6b87-4d48-9812-000000000054';
    n_esbuild uuid := 'c7300002-6b87-4d48-9812-000000000055';
    n_rollup uuid := 'c7300002-6b87-4d48-9812-000000000056';
    n_rolldown uuid := 'c7300002-6b87-4d48-9812-000000000057';
    n_parcel uuid := 'c7300002-6b87-4d48-9812-000000000058';
    n_linters uuid := 'c7300002-6b87-4d48-9812-000000000059';
    n_biome uuid := 'c7300002-6b87-4d48-9812-000000000060';
    n_prettier uuid := 'c7300002-6b87-4d48-9812-000000000061';
    n_eslint uuid := 'c7300002-6b87-4d48-9812-000000000062';

    -- 4. Testing & Web Security & APIs
    n_testing uuid := 'c7300002-6b87-4d48-9812-000000000063';
    n_vitest uuid := 'c7300002-6b87-4d48-9812-000000000064';
    n_playwright uuid := 'c7300002-6b87-4d48-9812-000000000065';
    n_cypress uuid := 'c7300002-6b87-4d48-9812-000000000066';
    n_jest uuid := 'c7300002-6b87-4d48-9812-000000000067';
    n_web_apis uuid := 'c7300002-6b87-4d48-9812-000000000068';
    n_web_security uuid := 'c7300002-6b87-4d48-9812-000000000069';
    n_cors uuid := 'c7300002-6b87-4d48-9812-000000000070';
    n_https uuid := 'c7300002-6b87-4d48-9812-000000000071';
    n_csp uuid := 'c7300002-6b87-4d48-9812-000000000072';
    n_owasp uuid := 'c7300002-6b87-4d48-9812-000000000073';

    -- 5. SSR, SSG, Meta-frameworks & Deployment
    n_angular_meta uuid := 'c7300002-6b87-4d48-9812-000000000074';
    n_vue_meta uuid := 'c7300002-6b87-4d48-9812-000000000075';
    n_nuxtjs uuid := 'c7300002-6b87-4d48-9812-000000000076';
    n_sveltekit uuid := 'c7300002-6b87-4d48-9812-000000000077';
    n_svelte_meta uuid := 'c7300002-6b87-4d48-9812-000000000078';
    n_react_meta uuid := 'c7300002-6b87-4d48-9812-000000000079';
    n_nextjs uuid := 'c7300002-6b87-4d48-9812-000000000080';
    n_tanstack uuid := 'c7300002-6b87-4d48-9812-000000000081';
    n_astro uuid := 'c7300002-6b87-4d48-9812-000000000082';
    n_react_router uuid := 'c7300002-6b87-4d48-9812-000000000083';
    n_ssr uuid := 'c7300002-6b87-4d48-9812-000000000084';
    n_ssg uuid := 'c7300002-6b87-4d48-9812-000000000085';
    n_ssg_astro uuid := 'c7300002-6b87-4d48-9812-000000000086';
    n_ssg_nextjs uuid := 'c7300002-6b87-4d48-9812-000000000087';
    n_vuepress uuid := 'c7300002-6b87-4d48-9812-000000000088';
    n_eleventy uuid := 'c7300002-6b87-4d48-9812-000000000089';
    n_deployment uuid := 'c7300002-6b87-4d48-9812-000000000090';
    n_gh_pages uuid := 'c7300002-6b87-4d48-9812-000000000091';
    n_vercel uuid := 'c7300002-6b87-4d48-9812-000000000092';
    n_netlify uuid := 'c7300002-6b87-4d48-9812-000000000093';
    n_cloudflare uuid := 'c7300002-6b87-4d48-9812-000000000094';
    n_railway uuid := 'c7300002-6b87-4d48-9812-000000000095';
    n_render uuid := 'c7300002-6b87-4d48-9812-000000000096';

    -- 6. Type Checkers, Performance & Architecture
    n_type_checkers uuid := 'c7300002-6b87-4d48-9812-000000000097';
    n_typescript uuid := 'c7300002-6b87-4d48-9812-000000000098';
    n_design_systems uuid := 'c7300002-6b87-4d48-9812-000000000099';
    n_performance uuid := 'c7300002-6b87-4d48-9812-000000000100';
    n_lighthouse uuid := 'c7300002-6b87-4d48-9812-000000000101';
    n_devtools uuid := 'c7300002-6b87-4d48-9812-000000000102';
    n_service_workers uuid := 'c7300002-6b87-4d48-9812-000000000103';
    n_cache_control uuid := 'c7300002-6b87-4d48-9812-000000000104';
    n_streamed_resp uuid := 'c7300002-6b87-4d48-9812-000000000105';
    n_web_components uuid := 'c7300002-6b87-4d48-9812-000000000106';
    n_html_templates uuid := 'c7300002-6b87-4d48-9812-000000000107';
    n_custom_elements uuid := 'c7300002-6b87-4d48-9812-000000000108';
    n_shadow_dom uuid := 'c7300002-6b87-4d48-9812-000000000109';
    n_graphql uuid := 'c7300002-6b87-4d48-9812-000000000110';
    n_apollo uuid := 'c7300002-6b87-4d48-9812-000000000111';
    n_relay uuid := 'c7300002-6b87-4d48-9812-000000000112';
    n_accessibility uuid := 'c7300002-6b87-4d48-9812-000000000113';

    -- 7. Desktop, Mobile & PWAs
    n_desktop_apps uuid := 'c7300002-6b87-4d48-9812-000000000114';
    n_electron uuid := 'c7300002-6b87-4d48-9812-000000000115';
    n_tauri uuid := 'c7300002-6b87-4d48-9812-000000000116';
    n_flutter_desktop uuid := 'c7300002-6b87-4d48-9812-000000000117';
    n_mobile_apps uuid := 'c7300002-6b87-4d48-9812-000000000118';
    n_pwas uuid := 'c7300002-6b87-4d48-9812-000000000119';
    n_react_native uuid := 'c7300002-6b87-4d48-9812-000000000120';
    n_flutter_mobile uuid := 'c7300002-6b87-4d48-9812-000000000121';
    n_ionic uuid := 'c7300002-6b87-4d48-9812-000000000122';
BEGIN
    -- 1. TÌM TÀI KHOẢN ADMIN
    SELECT u."Id" INTO v_author_id
    FROM gdsc."Users" u
    JOIN gdsc."UserRoles" ur ON u."Id" = ur."UserId"
    JOIN gdsc."Roles" r ON ur."RoleId" = r."Id"
    WHERE r."NormalizedName" = 'ADMIN' AND u."IsDeleted" = false
    ORDER BY u."Id"
    LIMIT 1;

    IF v_author_id IS NULL THEN
        RAISE NOTICE 'Skipping roadmap seed: no active administrator found';
        RETURN;
    END IF;

    -- 2. ĐẢM BẢO DANH MỤC "Frontend" TỒN TẠI
    SELECT "Id" INTO v_category_id FROM gdsc."RoadmapCategories" WHERE "Slug" = 'frontend' LIMIT 1;
    IF v_category_id IS NULL THEN
        v_category_id := 'c7300000-6b87-4d48-9812-000000000001';
        INSERT INTO gdsc."RoadmapCategories" ("Id", "Name", "Slug", "Description", "Color", "Icon", "SortOrder", "IsActive", "CreatedAtUtc")
        VALUES (v_category_id, 'Frontend', 'frontend', 'Web development, interfaces, UI frameworks and modern client technologies.', '#4285F4', 'globe', 1, true, NOW());
    END IF;

    -- 3. TẠO HOẶC CẬP NHẬT BẢN GHI ROADMAP CHÍNH
    INSERT INTO gdsc."Roadmaps" (
        "Id", "CategoryId", "Title", "Slug", "ShortDescription", "Description",
        "ThumbnailUrl", "Level", "EstimatedDuration", "Prerequisites", "Status",
        "CreatedByUserId", "SortOrder", "PublishedAtUtc", "CreatedAtUtc", "UpdatedAtUtc"
    ) VALUES (
        v_roadmap_id, v_category_id, 'Frontend Developer', 'frontend',
        'Step by step guide to becoming a modern frontend developer in 2026',
        'Comprehensive step-by-step roadmap to learn modern frontend development, covering core web fundamentals, frameworks, build tools, AI-assisted workflows, testing, and fullstack architecture.',
        NULL, 0, '6 - 9 months', 'Basic computer literacy and curiosity to learn web development.',
        1, v_author_id, 0, NOW(), NOW(), NOW()
    ) ON CONFLICT ("Id") DO UPDATE SET
        "Title" = EXCLUDED."Title",
        "ShortDescription" = EXCLUDED."ShortDescription",
        "Description" = EXCLUDED."Description",
        "Status" = 1,
        "UpdatedAtUtc" = NOW();

    -- 4. LÀM SẠCH CÁC NODE & EDGE CŨ CỦA ROADMAP NÀY (ĐỂ SEED BẢN MỚI CHÍNH XÁC 100%)
    DELETE FROM gdsc."RoadmapEdges" WHERE "RoadmapId" = v_roadmap_id;
    DELETE FROM gdsc."RoadmapNodes" WHERE "RoadmapId" = v_roadmap_id;

    -- =========================================================================
    -- 5. CHÈN TẤT CẢ ROADMAP NODES
    -- =========================================================================
    
    -- TRUNK 1: Core Fundamentals
    INSERT INTO gdsc."RoadmapNodes" ("Id", "RoadmapId", "Title", "Slug", "NodeType", "PositionX", "PositionY", "Width", "Color", "Icon", "SortOrder", "IsActive", "CreatedAtUtc") VALUES
    (n_internet, v_roadmap_id, 'Internet', 'internet', 0, 430, 120, 220, '#FFE600', NULL, 1, true, NOW()),
    (n_how_internet, v_roadmap_id, 'How does the internet work?', 'how-internet-works', 0, 720, -20, 270, '#FFF4C2', 'check-purple', 2, true, NOW()),
    (n_what_http, v_roadmap_id, 'What is HTTP?', 'what-is-http', 0, 720, 30, 270, '#FFF4C2', 'check-purple', 3, true, NOW()),
    (n_domain_name, v_roadmap_id, 'What is Domain Name?', 'what-is-domain-name', 0, 720, 80, 270, '#FFF4C2', 'check-purple', 4, true, NOW()),
    (n_hosting, v_roadmap_id, 'What is hosting?', 'what-is-hosting', 0, 720, 130, 270, '#FFF4C2', 'check-purple', 5, true, NOW()),
    (n_dns, v_roadmap_id, 'DNS and how it works?', 'dns-how-it-works', 0, 720, 180, 270, '#FFF4C2', 'check-purple', 6, true, NOW()),
    (n_browsers, v_roadmap_id, 'Browsers and how they work?', 'browsers-how-they-work', 0, 720, 230, 270, '#FFF4C2', 'check-purple', 7, true, NOW()),
    (n_html, v_roadmap_id, 'HTML', 'html', 0, 430, 220, 220, '#FFE600', NULL, 8, true, NOW()),
    (n_css, v_roadmap_id, 'CSS', 'css', 0, 430, 275, 220, '#FFE600', NULL, 9, true, NOW()),
    (n_javascript, v_roadmap_id, 'JavaScript', 'javascript', 0, 430, 330, 220, '#FFE600', NULL, 10, true, NOW()),
    (n_version_control, v_roadmap_id, 'Version Control', 'version-control', 0, 430, 440, 220, '#FFE600', NULL, 11, true, NOW()),
    (n_git, v_roadmap_id, 'Git', 'git', 0, 180, 440, 110, '#FFF4C2', 'check-purple', 12, true, NOW()),
    (n_vcs_hosting, v_roadmap_id, 'VCS Hosting', 'vcs-hosting', 0, 430, 495, 220, '#FFE600', NULL, 13, true, NOW()),
    (n_github, v_roadmap_id, 'GitHub', 'github', 0, 180, 495, 110, '#FFF4C2', 'check-purple', 14, true, NOW()),
    (n_gitlab, v_roadmap_id, 'GitLab', 'gitlab', 0, 60, 495, 110, '#FFF4C2', 'check-green', 15, true, NOW()),
    (n_package_managers, v_roadmap_id, 'Package Managers', 'package-managers', 0, 760, 495, 220, '#FFE600', NULL, 16, true, NOW()),
    (n_npm, v_roadmap_id, 'npm', 'npm', 0, 760, 395, 105, '#FFF4C2', 'check-purple', 17, true, NOW()),
    (n_yarn, v_roadmap_id, 'yarn', 'yarn', 0, 875, 395, 105, '#FFF4C2', 'check-green', 18, true, NOW()),
    (n_pnpm, v_roadmap_id, 'pnpm', 'pnpm', 0, 760, 440, 105, '#FFF4C2', 'check-green', 19, true, NOW()),
    (n_bun, v_roadmap_id, 'Bun', 'bun', 0, 875, 440, 105, '#FFF4C2', 'check-green', 20, true, NOW()),
    (n_css_frameworks, v_roadmap_id, 'CSS Frameworks', 'css-frameworks', 0, 760, 620, 220, '#FFE600', NULL, 21, true, NOW()),
    (n_tailwind, v_roadmap_id, 'Tailwind', 'tailwind', 0, 760, 700, 220, '#FFF4C2', 'check-purple', 22, true, NOW()),
    (n_learn_framework, v_roadmap_id, 'Learn a Framework', 'learn-a-framework', 0, 430, 820, 220, '#FFE600', NULL, 23, true, NOW()),
    (n_react, v_roadmap_id, 'React', 'react', 0, 80, 620, 210, '#FFF4C2', 'check-purple', 24, true, NOW()),
    (n_vue, v_roadmap_id, 'Vue.js', 'vue-js', 0, 80, 670, 210, '#FFF4C2', 'check-green', 25, true, NOW()),
    (n_angular, v_roadmap_id, 'Angular', 'angular', 0, 80, 720, 210, '#FFF4C2', 'check-green', 26, true, NOW()),
    (n_svelte, v_roadmap_id, 'Svelte', 'svelte', 0, 80, 770, 210, '#FFF4C2', 'check-green', 27, true, NOW()),
    (n_solid_js, v_roadmap_id, 'Solid JS', 'solid-js', 0, 80, 820, 210, '#FFF4C2', 'check-green', 28, true, NOW());

    -- TRUNK 2: AI in Development
    INSERT INTO gdsc."RoadmapNodes" ("Id", "RoadmapId", "Title", "Slug", "NodeType", "PositionX", "PositionY", "Width", "Color", "Icon", "SortOrder", "IsActive", "CreatedAtUtc") VALUES
    (n_ai_assisted, v_roadmap_id, 'AI Assisted Coding', 'ai-assisted-coding', 0, 430, 980, 220, '#FFE600', NULL, 29, true, NOW()),
    (n_claude_code, v_roadmap_id, 'Claude Code', 'claude-code', 0, 80, 900, 210, '#FFF4C2', 'check-purple', 30, true, NOW()),
    (n_cursor, v_roadmap_id, 'Cursor', 'cursor', 0, 80, 950, 210, '#FFF4C2', 'check-green', 31, true, NOW()),
    (n_copilot, v_roadmap_id, 'Copilot', 'copilot', 0, 80, 1000, 210, '#FFF4C2', 'check-green', 32, true, NOW()),
    (n_antigravity, v_roadmap_id, 'Antigravity', 'antigravity', 0, 80, 1050, 210, '#FFF4C2', 'check-green', 33, true, NOW()),
    (n_ai_basics, v_roadmap_id, 'Learn the Basics', 'ai-learn-basics', 0, 740, 940, 210, '#FFE600', NULL, 34, true, NOW()),
    (n_how_llms, v_roadmap_id, 'How LLMs work', 'how-llms-work', 0, 740, 1000, 210, '#FFF4C2', NULL, 35, true, NOW()),
    (n_ai_vs_trad, v_roadmap_id, 'AI vs Traditional Coding', 'ai-vs-trad-coding', 0, 740, 1045, 210, '#FFF4C2', NULL, 36, true, NOW()),
    (n_ai_apps, v_roadmap_id, 'Applications', 'ai-applications', 0, 740, 1090, 210, '#FFF4C2', NULL, 37, true, NOW()),
    (n_code_reviews, v_roadmap_id, 'Code Reviews', 'ai-code-reviews', 0, 740, 1170, 210, '#FFF4C2', NULL, 38, true, NOW()),
    (n_refactoring, v_roadmap_id, 'Refactoring', 'ai-refactoring', 0, 740, 1215, 210, '#FFF4C2', NULL, 39, true, NOW()),
    (n_docs_gen, v_roadmap_id, 'Docs Generation', 'ai-docs-gen', 0, 740, 1260, 210, '#FFF4C2', NULL, 40, true, NOW()),
    (n_prompting, v_roadmap_id, 'Prompting Techniques', 'prompting-techniques', 0, 430, 1130, 220, '#FFE600', NULL, 41, true, NOW()),
    (n_prompt_eng, v_roadmap_id, 'Prompt Engineering', 'prompt-engineering', 0, 80, 1130, 210, '#2563EB', NULL, 42, true, NOW()),
    (n_agents, v_roadmap_id, 'Agents', 'agents', 0, 430, 1220, 220, '#FFE600', NULL, 43, true, NOW()),
    (n_ai_agents_rm, v_roadmap_id, 'AI Agents Roadmap', 'ai-agents-roadmap', 0, 80, 1220, 210, '#2563EB', NULL, 44, true, NOW()),
    (n_mcp, v_roadmap_id, 'MCP', 'mcp', 0, 430, 1275, 220, '#FFE600', NULL, 45, true, NOW()),
    (n_skills, v_roadmap_id, 'Skills', 'skills', 0, 430, 1330, 220, '#FFE600', NULL, 46, true, NOW()),
    (n_implementing_ai, v_roadmap_id, 'Implementing AI', 'implementing-ai', 0, 430, 1430, 220, '#FFE600', NULL, 47, true, NOW()),
    (n_gemini, v_roadmap_id, 'Gemini', 'gemini', 0, 80, 1370, 210, '#FFF4C2', NULL, 48, true, NOW()),
    (n_openai, v_roadmap_id, 'OpenAI', 'openai', 0, 80, 1420, 210, '#FFF4C2', NULL, 49, true, NOW()),
    (n_anthropic, v_roadmap_id, 'Anthropic', 'anthropic', 0, 80, 1470, 210, '#FFF4C2', NULL, 50, true, NOW());

    -- TRUNK 3: Tooling, Bundlers, Linters & Auth
    INSERT INTO gdsc."RoadmapNodes" ("Id", "RoadmapId", "Title", "Slug", "NodeType", "PositionX", "PositionY", "Width", "Color", "Icon", "SortOrder", "IsActive", "CreatedAtUtc") VALUES
    (n_auth_strategies, v_roadmap_id, 'Auth Strategies', 'auth-strategies', 0, 70, 1580, 210, '#FFE600', NULL, 51, true, NOW()),
    (n_module_bundlers, v_roadmap_id, 'Module Bundlers', 'module-bundlers', 0, 430, 1580, 220, '#FFE600', NULL, 52, true, NOW()),
    (n_vite, v_roadmap_id, 'Vite', 'vite', 0, 430, 1660, 210, '#FFF4C2', 'check-purple', 53, true, NOW()),
    (n_swc, v_roadmap_id, 'SWC', 'swc', 0, 430, 1715, 100, '#FFF4C2', 'check-green', 54, true, NOW()),
    (n_esbuild, v_roadmap_id, 'esbuild', 'esbuild', 0, 540, 1715, 100, '#FFF4C2', 'check-purple', 55, true, NOW()),
    (n_rollup, v_roadmap_id, 'Rollup', 'rollup', 0, 430, 1765, 100, '#FFF4C2', 'check-green', 56, true, NOW()),
    (n_rolldown, v_roadmap_id, 'Rolldown', 'rolldown', 0, 540, 1765, 100, '#FFF4C2', NULL, 57, true, NOW()),
    (n_parcel, v_roadmap_id, 'Parcel', 'parcel', 0, 430, 1815, 210, '#FFF4C2', 'check-green', 58, true, NOW()),
    (n_linters, v_roadmap_id, 'Linters & Formatters', 'linters-formatters', 0, 750, 1580, 210, '#FFE600', NULL, 59, true, NOW()),
    (n_biome, v_roadmap_id, 'Biome', 'biome', 0, 750, 1660, 210, '#FFF4C2', 'check-green', 60, true, NOW()),
    (n_prettier, v_roadmap_id, 'Prettier', 'prettier', 0, 750, 1715, 210, '#FFF4C2', 'check-purple', 61, true, NOW()),
    (n_eslint, v_roadmap_id, 'ESLint', 'eslint', 0, 750, 1770, 210, '#FFF4C2', 'check-purple', 62, true, NOW());

    -- TRUNK 4: Testing & Web Security
    INSERT INTO gdsc."RoadmapNodes" ("Id", "RoadmapId", "Title", "Slug", "NodeType", "PositionX", "PositionY", "Width", "Color", "Icon", "SortOrder", "IsActive", "CreatedAtUtc") VALUES
    (n_testing, v_roadmap_id, 'Testing', 'testing', 0, 70, 1960, 210, '#FFE600', NULL, 63, true, NOW()),
    (n_vitest, v_roadmap_id, 'Vitest', 'vitest', 0, 70, 2030, 210, '#FFF4C2', 'check-purple', 64, true, NOW()),
    (n_playwright, v_roadmap_id, 'Playwright', 'playwright', 0, 70, 2080, 210, '#FFF4C2', 'check-purple', 65, true, NOW()),
    (n_cypress, v_roadmap_id, 'Cypress', 'cypress', 0, 70, 2130, 210, '#FFF4C2', 'check-green', 66, true, NOW()),
    (n_jest, v_roadmap_id, 'Jest', 'jest', 0, 70, 2180, 210, '#FFF4C2', 'check-green', 67, true, NOW()),
    (n_web_apis, v_roadmap_id, 'Web APIs', 'web-apis', 0, 430, 2080, 220, '#FFE600', NULL, 68, true, NOW()),
    (n_web_security, v_roadmap_id, 'Web Security', 'web-security', 0, 740, 2080, 210, '#FFE600', NULL, 69, true, NOW()),
    (n_cors, v_roadmap_id, 'CORS', 'cors', 0, 740, 1900, 210, '#FFF4C2', 'check-purple', 70, true, NOW()),
    (n_https, v_roadmap_id, 'HTTPS', 'https', 0, 740, 1945, 210, '#FFF4C2', 'check-purple', 71, true, NOW()),
    (n_csp, v_roadmap_id, 'CSP', 'csp', 0, 740, 1990, 210, '#FFF4C2', 'check-purple', 72, true, NOW()),
    (n_owasp, v_roadmap_id, 'OWASP Risks', 'owasp-risks', 0, 740, 2035, 210, '#FFF4C2', 'check-purple', 73, true, NOW());

    -- TRUNK 5: Meta-frameworks, SSR, SSG & Deployment
    INSERT INTO gdsc."RoadmapNodes" ("Id", "RoadmapId", "Title", "Slug", "NodeType", "PositionX", "PositionY", "Width", "Color", "Icon", "SortOrder", "IsActive", "CreatedAtUtc") VALUES
    (n_angular_meta, v_roadmap_id, 'Angular', 'angular-meta', 0, 330, 2160, 150, '#FFF4C2', 'check-green', 74, true, NOW()),
    (n_vue_meta, v_roadmap_id, 'Vue.js', 'vue-meta', 0, 330, 2205, 150, '#FFF4C2', 'check-green', 75, true, NOW()),
    (n_nuxtjs, v_roadmap_id, 'Nuxt.js', 'nuxt-js', 0, 350, 2250, 130, '#FFF4C2', 'check-purple', 76, true, NOW()),
    (n_sveltekit, v_roadmap_id, 'SvelteKit', 'sveltekit', 0, 330, 2305, 150, '#FFF4C2', 'check-green', 77, true, NOW()),
    (n_svelte_meta, v_roadmap_id, 'Svelte', 'svelte-meta', 0, 350, 2350, 130, '#FFF4C2', NULL, 78, true, NOW()),
    (n_react_meta, v_roadmap_id, 'React', 'react-meta', 0, 500, 2160, 150, '#FFF4C2', 'check-purple', 79, true, NOW()),
    (n_nextjs, v_roadmap_id, 'Next.js', 'nextjs', 0, 520, 2210, 130, '#FFF4C2', 'check-purple', 80, true, NOW()),
    (n_tanstack, v_roadmap_id, 'Tanstack Start', 'tanstack-start', 0, 520, 2260, 130, '#FFF4C2', 'check-purple', 81, true, NOW()),
    (n_astro, v_roadmap_id, 'Astro', 'astro', 0, 520, 2310, 130, '#FFF4C2', 'check-green', 82, true, NOW()),
    (n_react_router, v_roadmap_id, 'react-router', 'react-router', 0, 520, 2360, 130, '#FFF4C2', 'check-green', 83, true, NOW()),
    (n_ssr, v_roadmap_id, 'SSR', 'ssr', 0, 740, 2290, 210, '#FFE600', NULL, 84, true, NOW()),
    (n_ssg, v_roadmap_id, 'SSG', 'ssg', 0, 740, 2470, 210, '#FFE600', NULL, 85, true, NOW()),
    (n_ssg_astro, v_roadmap_id, 'Astro', 'ssg-astro', 0, 740, 2540, 210, '#FFF4C2', 'check-purple', 86, true, NOW()),
    (n_ssg_nextjs, v_roadmap_id, 'Next.js', 'ssg-nextjs', 0, 740, 2585, 210, '#FFF4C2', 'check-green', 87, true, NOW()),
    (n_vuepress, v_roadmap_id, 'Vuepress', 'vuepress', 0, 740, 2630, 210, '#FFF4C2', 'check-green', 88, true, NOW()),
    (n_eleventy, v_roadmap_id, 'Eleventy', 'eleventy', 0, 740, 2675, 210, '#FFF4C2', 'check-green', 89, true, NOW()),
    (n_deployment, v_roadmap_id, 'Deployment', 'deployment', 0, 70, 2470, 210, '#FFE600', NULL, 90, true, NOW()),
    (n_gh_pages, v_roadmap_id, 'GitHub Pages', 'github-pages', 0, 70, 2260, 210, '#FFF4C2', 'check-purple', 91, true, NOW()),
    (n_vercel, v_roadmap_id, 'Vercel', 'vercel', 0, 70, 2315, 100, '#FFF4C2', 'check-green', 92, true, NOW()),
    (n_netlify, v_roadmap_id, 'Netlify', 'netlify', 0, 180, 2315, 100, '#FFF4C2', 'check-green', 93, true, NOW()),
    (n_cloudflare, v_roadmap_id, 'Cloudflare', 'cloudflare', 0, 70, 2365, 210, '#FFF4C2', 'check-purple', 94, true, NOW()),
    (n_railway, v_roadmap_id, 'Railway', 'railway', 0, 70, 2415, 100, '#FFF4C2', 'check-green', 95, true, NOW()),
    (n_render, v_roadmap_id, 'Render', 'render', 0, 180, 2415, 100, '#FFF4C2', 'check-green', 96, true, NOW());

    -- TRUNK 6: Type Checkers, Performance & Architecture
    INSERT INTO gdsc."RoadmapNodes" ("Id", "RoadmapId", "Title", "Slug", "NodeType", "PositionX", "PositionY", "Width", "Color", "Icon", "SortOrder", "IsActive", "CreatedAtUtc") VALUES
    (n_type_checkers, v_roadmap_id, 'Type Checkers', 'type-checkers', 0, 430, 2470, 220, '#FFE600', NULL, 97, true, NOW()),
    (n_typescript, v_roadmap_id, 'TypeScript', 'typescript', 0, 430, 2545, 210, '#2563EB', NULL, 98, true, NOW()),
    (n_design_systems, v_roadmap_id, 'Design Systems', 'design-systems', 0, 70, 2620, 210, '#FFE600', NULL, 99, true, NOW()),
    (n_performance, v_roadmap_id, 'Performance', 'performance', 0, 70, 2740, 210, '#FFE600', NULL, 100, true, NOW()),
    (n_lighthouse, v_roadmap_id, 'Lighthouse', 'lighthouse', 0, 430, 2630, 210, '#FFF4C2', 'check-purple', 101, true, NOW()),
    (n_devtools, v_roadmap_id, 'DevTools Usage', 'devtools-usage', 0, 430, 2675, 210, '#FFF4C2', 'check-purple', 102, true, NOW()),
    (n_service_workers, v_roadmap_id, 'Service Workers', 'service-workers', 0, 430, 2720, 210, '#FFF4C2', 'check-purple', 103, true, NOW()),
    (n_cache_control, v_roadmap_id, 'Cache-Control', 'cache-control', 0, 430, 2765, 210, '#FFF4C2', 'check-purple', 104, true, NOW()),
    (n_streamed_resp, v_roadmap_id, 'Streamed Responses', 'streamed-responses', 0, 430, 2810, 210, '#FFF4C2', 'check-purple', 105, true, NOW()),
    (n_web_components, v_roadmap_id, 'Web Components', 'web-components', 0, 70, 2960, 210, '#FFE600', NULL, 106, true, NOW()),
    (n_html_templates, v_roadmap_id, 'HTML Templates', 'html-templates', 0, 70, 3050, 210, '#FFF4C2', 'check-gray', 107, true, NOW()),
    (n_custom_elements, v_roadmap_id, 'Custom Elements', 'custom-elements', 0, 70, 3100, 210, '#FFF4C2', 'check-gray', 108, true, NOW()),
    (n_shadow_dom, v_roadmap_id, 'Shadow DOM', 'shadow-dom', 0, 70, 3150, 210, '#FFF4C2', 'check-gray', 109, true, NOW()),
    (n_graphql, v_roadmap_id, 'GraphQL', 'graphql', 0, 430, 2960, 210, '#FFE600', 'check-purple', 110, true, NOW()),
    (n_apollo, v_roadmap_id, 'Apollo', 'apollo', 0, 430, 3045, 210, '#FFF4C2', 'check-purple', 111, true, NOW()),
    (n_relay, v_roadmap_id, 'Relay Modern', 'relay-modern', 0, 430, 3095, 210, '#FFF4C2', 'check-green', 112, true, NOW()),
    (n_accessibility, v_roadmap_id, 'Accessibility', 'accessibility', 0, 740, 2960, 210, '#FFE600', NULL, 113, true, NOW());

    -- TRUNK 7: Desktop, Mobile & PWAs
    INSERT INTO gdsc."RoadmapNodes" ("Id", "RoadmapId", "Title", "Slug", "NodeType", "PositionX", "PositionY", "Width", "Color", "Icon", "SortOrder", "IsActive", "CreatedAtUtc") VALUES
    (n_desktop_apps, v_roadmap_id, 'Desktop Apps', 'desktop-apps', 0, 430, 3250, 210, '#FFE600', NULL, 114, true, NOW()),
    (n_electron, v_roadmap_id, 'Electron', 'electron', 0, 70, 3215, 210, '#FFF4C2', 'check-purple', 115, true, NOW()),
    (n_tauri, v_roadmap_id, 'Tauri', 'tauri', 0, 70, 3265, 210, '#FFF4C2', 'check-purple', 116, true, NOW()),
    (n_flutter_desktop, v_roadmap_id, 'Flutter', 'flutter-desktop', 0, 70, 3315, 210, '#FFF4C2', 'check-green', 117, true, NOW()),
    (n_mobile_apps, v_roadmap_id, 'Mobile Apps', 'mobile-apps', 0, 740, 3250, 210, '#FFE600', 'check-gray', 118, true, NOW()),
    (n_pwas, v_roadmap_id, 'PWAs', 'pwas', 0, 740, 3160, 210, '#FFE600', 'check-gray', 119, true, NOW()),
    (n_react_native, v_roadmap_id, 'React Native', 'react-native', 0, 740, 3330, 210, '#FFF4C2', 'check-purple', 120, true, NOW()),
    (n_flutter_mobile, v_roadmap_id, 'Flutter', 'flutter-mobile', 0, 740, 3380, 210, '#FFF4C2', 'check-green', 121, true, NOW()),
    (n_ionic, v_roadmap_id, 'Ionic', 'ionic', 0, 740, 3430, 210, '#FFF4C2', 'check-green', 122, true, NOW());

    -- =========================================================================
    -- 6. CHÈN TẤT CẢ ROADMAP EDGES (ĐƯỜNG NỐI)
    -- =========================================================================
    
    -- Trunk 1: Web Fundamentals
    INSERT INTO gdsc."RoadmapEdges" ("Id", "RoadmapId", "SourceNodeId", "TargetNodeId", "RelationType", "LineStyle", "SortOrder", "IsActive", "CreatedAtUtc") VALUES
    (gen_random_uuid(), v_roadmap_id, n_internet, n_how_internet, 1, 1, 1, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_internet, n_what_http, 1, 1, 2, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_internet, n_domain_name, 1, 1, 3, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_internet, n_hosting, 1, 1, 4, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_internet, n_dns, 1, 1, 5, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_internet, n_browsers, 1, 1, 6, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_internet, n_html, 0, 0, 7, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_html, n_css, 0, 0, 8, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_css, n_javascript, 0, 0, 9, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_javascript, n_version_control, 0, 0, 10, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_version_control, n_git, 1, 1, 11, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_version_control, n_vcs_hosting, 0, 0, 12, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_vcs_hosting, n_github, 1, 1, 13, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_vcs_hosting, n_gitlab, 2, 1, 14, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_vcs_hosting, n_package_managers, 0, 0, 15, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_package_managers, n_npm, 1, 1, 16, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_package_managers, n_yarn, 2, 1, 17, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_package_managers, n_pnpm, 2, 1, 18, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_package_managers, n_bun, 2, 1, 19, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_package_managers, n_css_frameworks, 0, 0, 20, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_css_frameworks, n_tailwind, 1, 1, 21, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_vcs_hosting, n_learn_framework, 0, 0, 22, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_learn_framework, n_react, 1, 1, 23, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_learn_framework, n_vue, 2, 1, 24, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_learn_framework, n_angular, 2, 1, 25, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_learn_framework, n_svelte, 2, 1, 26, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_learn_framework, n_solid_js, 2, 1, 27, true, NOW());

    -- Trunk 2: AI in Development
    INSERT INTO gdsc."RoadmapEdges" ("Id", "RoadmapId", "SourceNodeId", "TargetNodeId", "RelationType", "LineStyle", "SortOrder", "IsActive", "CreatedAtUtc") VALUES
    (gen_random_uuid(), v_roadmap_id, n_learn_framework, n_ai_assisted, 0, 0, 28, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_ai_assisted, n_claude_code, 1, 1, 29, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_ai_assisted, n_cursor, 2, 1, 30, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_ai_assisted, n_copilot, 2, 1, 31, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_ai_assisted, n_antigravity, 2, 1, 32, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_ai_assisted, n_ai_basics, 0, 0, 33, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_ai_basics, n_how_llms, 1, 1, 34, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_ai_basics, n_ai_vs_trad, 1, 1, 35, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_ai_basics, n_ai_apps, 1, 1, 36, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_ai_basics, n_code_reviews, 1, 1, 37, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_ai_basics, n_refactoring, 1, 1, 38, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_ai_basics, n_docs_gen, 1, 1, 39, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_ai_assisted, n_prompting, 0, 0, 40, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_prompting, n_prompt_eng, 1, 1, 41, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_prompting, n_agents, 0, 0, 42, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_agents, n_ai_agents_rm, 1, 1, 43, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_agents, n_mcp, 0, 0, 44, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_mcp, n_skills, 0, 0, 45, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_skills, n_implementing_ai, 0, 0, 46, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_implementing_ai, n_gemini, 1, 1, 47, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_implementing_ai, n_openai, 1, 1, 48, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_implementing_ai, n_anthropic, 1, 1, 49, true, NOW());

    -- Trunk 3: Tooling, Bundlers & Linters
    INSERT INTO gdsc."RoadmapEdges" ("Id", "RoadmapId", "SourceNodeId", "TargetNodeId", "RelationType", "LineStyle", "SortOrder", "IsActive", "CreatedAtUtc") VALUES
    (gen_random_uuid(), v_roadmap_id, n_implementing_ai, n_module_bundlers, 0, 0, 50, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_module_bundlers, n_auth_strategies, 0, 0, 51, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_module_bundlers, n_linters, 0, 0, 52, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_module_bundlers, n_vite, 1, 1, 53, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_module_bundlers, n_swc, 2, 1, 54, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_module_bundlers, n_esbuild, 1, 1, 55, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_module_bundlers, n_rollup, 2, 1, 56, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_module_bundlers, n_parcel, 2, 1, 57, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_linters, n_biome, 2, 1, 58, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_linters, n_prettier, 1, 1, 59, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_linters, n_eslint, 1, 1, 60, true, NOW());

    -- Trunk 4: Testing, Security & Web APIs
    INSERT INTO gdsc."RoadmapEdges" ("Id", "RoadmapId", "SourceNodeId", "TargetNodeId", "RelationType", "LineStyle", "SortOrder", "IsActive", "CreatedAtUtc") VALUES
    (gen_random_uuid(), v_roadmap_id, n_auth_strategies, n_testing, 0, 0, 61, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_testing, n_vitest, 1, 1, 62, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_testing, n_playwright, 1, 1, 63, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_testing, n_cypress, 2, 1, 64, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_testing, n_jest, 2, 1, 65, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_module_bundlers, n_web_apis, 0, 0, 66, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_web_apis, n_web_security, 0, 0, 67, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_web_security, n_cors, 1, 1, 68, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_web_security, n_https, 1, 1, 69, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_web_security, n_csp, 1, 1, 70, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_web_security, n_owasp, 1, 1, 71, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_web_apis, n_nextjs, 1, 1, 72, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_web_apis, n_tanstack, 1, 1, 73, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_web_apis, n_nuxtjs, 1, 1, 74, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_nextjs, n_ssr, 0, 0, 75, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_ssr, n_ssg, 0, 0, 76, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_ssg, n_ssg_astro, 1, 1, 77, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_ssg, n_ssg_nextjs, 2, 1, 78, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_ssg, n_vuepress, 2, 1, 79, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_ssg, n_eleventy, 2, 1, 80, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_testing, n_deployment, 0, 0, 81, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_deployment, n_gh_pages, 1, 1, 82, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_deployment, n_vercel, 2, 1, 83, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_deployment, n_netlify, 2, 1, 84, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_deployment, n_cloudflare, 1, 1, 85, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_deployment, n_railway, 2, 1, 86, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_deployment, n_render, 2, 1, 87, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_deployment, n_type_checkers, 0, 0, 88, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_type_checkers, n_ssg, 0, 0, 89, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_type_checkers, n_typescript, 1, 1, 90, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_deployment, n_design_systems, 0, 0, 91, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_design_systems, n_performance, 0, 0, 92, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_performance, n_lighthouse, 1, 1, 93, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_performance, n_devtools, 1, 1, 94, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_performance, n_service_workers, 1, 1, 95, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_performance, n_cache_control, 1, 1, 96, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_performance, n_streamed_resp, 1, 1, 97, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_performance, n_web_components, 0, 0, 98, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_web_components, n_html_templates, 2, 1, 99, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_web_components, n_custom_elements, 2, 1, 100, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_web_components, n_shadow_dom, 2, 1, 101, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_web_components, n_graphql, 0, 0, 102, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_graphql, n_apollo, 1, 1, 103, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_graphql, n_relay, 2, 1, 104, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_graphql, n_accessibility, 0, 0, 105, true, NOW());

    -- Trunk 5: Cross-platform, Desktop & Mobile
    INSERT INTO gdsc."RoadmapEdges" ("Id", "RoadmapId", "SourceNodeId", "TargetNodeId", "RelationType", "LineStyle", "SortOrder", "IsActive", "CreatedAtUtc") VALUES
    (gen_random_uuid(), v_roadmap_id, n_graphql, n_desktop_apps, 0, 0, 106, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_desktop_apps, n_electron, 1, 1, 107, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_desktop_apps, n_tauri, 1, 1, 108, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_desktop_apps, n_flutter_desktop, 2, 1, 109, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_desktop_apps, n_mobile_apps, 0, 0, 110, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_mobile_apps, n_pwas, 2, 1, 111, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_mobile_apps, n_react_native, 1, 1, 112, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_mobile_apps, n_flutter_mobile, 2, 1, 113, true, NOW()),
    (gen_random_uuid(), v_roadmap_id, n_mobile_apps, n_ionic, 2, 1, 114, true, NOW());

    RAISE NOTICE 'Successfully seeded Frontend Roadmap with 122 nodes and 114 edges.';
END $$;

COMMIT;
