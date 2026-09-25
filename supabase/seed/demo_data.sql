-- =============================================================================
-- Mini ATS – Idempotent Demo Data Seed Script
-- Source of Truth: supabase/seed/demo_dataset.json
-- Related Schema:  supabase/migrations/0001_init.sql
-- Backend API:     backend/MiniAts.Api/Api/Controllers/
-- Frontend View:   frontend/src/views/KanbanView.vue
-- Demo Script:     docs/DEMO.md & docs/DEMO_SEED_DATA.md
-- =============================================================================

-- -----------------------------------------------------------------------------
-- Pre-flight Verification: Validate Auth Users in auth.users
-- Constraint: public.profiles(id) REFERENCES auth.users(id) ON DELETE CASCADE.
-- If auth accounts are absent, fail with actionable guidance.
-- -----------------------------------------------------------------------------
DO $$
DECLARE
  v_admin_exists    BOOLEAN;
  v_customer_exists BOOLEAN;
BEGIN
  SELECT EXISTS(
    SELECT 1 FROM auth.users WHERE id = '11111111-1111-4111-8111-111111111111'
  ) INTO v_admin_exists;

  SELECT EXISTS(
    SELECT 1 FROM auth.users WHERE id = '22222222-2222-4222-8222-222222222222'
  ) INTO v_customer_exists;

  IF NOT v_admin_exists THEN
    RAISE EXCEPTION 'Demo Admin auth user (11111111-1111-4111-8111-111111111111 / admin@nordic-recruit.demo) not found in auth.users. Please create it via Supabase Dashboard or Admin API first. See supabase/seed/create_demo_auth_users.md for instructions.';
  END IF;

  IF NOT v_customer_exists THEN
    RAISE EXCEPTION 'Demo Customer auth user (22222222-2222-4222-8222-222222222222 / recruiter@nordic-tech.demo) not found in auth.users. Please create it via Supabase Dashboard or Admin API first. See supabase/seed/create_demo_auth_users.md for instructions.';
  END IF;
END $$;

-- -----------------------------------------------------------------------------
-- 1. Profiles (public.profiles)
-- Related: GET /api/account/me, GET /api/admin/users
-- Demo step: 0:30 Admin Login & 1:00 Admin selects/acts-as customer
-- -----------------------------------------------------------------------------
INSERT INTO public.profiles (id, email, role, display_name, company_name, created_at, updated_at)
VALUES
  (
    '11111111-1111-4111-8111-111111111111',
    'admin@nordic-recruit.demo',
    'admin'::public.user_role,
    'Seed Admin',
    'Nordic Recruit',
    NOW(),
    NOW()
  ),
  (
    '22222222-2222-4222-8222-222222222222',
    'recruiter@nordic-tech.demo',
    'customer'::public.user_role,
    'Elin Recruiter',
    'Nordic Tech AB',
    NOW(),
    NOW()
  )
ON CONFLICT (id) DO UPDATE SET
  email        = EXCLUDED.email,
  role         = EXCLUDED.role,
  display_name = EXCLUDED.display_name,
  company_name = EXCLUDED.company_name,
  updated_at   = NOW();

-- -----------------------------------------------------------------------------
-- 2. Jobs (public.jobs)
-- Related: GET /api/jobs?customerId=..., POST /api/jobs
-- Demo step: 1:45 Customer creates/views jobs on Dashboard
-- All jobs belong to customer: 22222222-2222-4222-8222-222222222222
-- -----------------------------------------------------------------------------
INSERT INTO public.jobs (id, customer_id, title, description, status, created_by, updated_by, created_at, updated_at)
VALUES
  (
    '33333333-3333-4333-8333-333333333301',
    '22222222-2222-4222-8222-222222222222',
    'Senior Frontend Engineer',
    'Build accessible Vue 3 + TypeScript interfaces, collaborate with backend and design, optimize performance, and ship user-facing features.',
    'active',
    '22222222-2222-4222-8222-222222222222',
    '22222222-2222-4222-8222-222222222222',
    NOW() - INTERVAL '5 days',
    NOW() - INTERVAL '5 days'
  ),
  (
    '33333333-3333-4333-8333-333333333302',
    '22222222-2222-4222-8222-222222222222',
    'Backend Engineer .NET',
    'Design REST APIs with .NET 10, JWT authentication, PostgreSQL and Supabase, clean OOP structure, and integration with the Python AI service.',
    'active',
    '22222222-2222-4222-8222-222222222222',
    '22222222-2222-4222-8222-222222222222',
    NOW() - INTERVAL '4 days',
    NOW() - INTERVAL '4 days'
  )
ON CONFLICT (id) DO UPDATE SET
  title       = EXCLUDED.title,
  description = EXCLUDED.description,
  status      = EXCLUDED.status,
  updated_at  = NOW();

-- -----------------------------------------------------------------------------
-- 3. Candidates (public.candidates)
-- Related: GET /api/candidates?customerId=..., PATCH /api/candidates/{id}/stage,
--          POST /api/ai/candidates/{id}/assess
-- Demo step: 2:30 Kanban board columns, 3:15 filtering/stage drag, 4:00 AI assessment
-- All 10 candidates scoped to customer: 22222222-2222-4222-8222-222222222222
-- -----------------------------------------------------------------------------
INSERT INTO public.candidates (
  id, customer_id, job_id, full_name, email, linkedin_url, stage,
  cv_text, summary, ai_score, ai_feedback, created_by, updated_by, created_at, updated_at
)
VALUES
  -- Candidate 1: Anna Lund (Frontend, New, AI: NULL for live demo assessment)
  (
    '55555555-5555-4555-8555-555555555501',
    '22222222-2222-4222-8222-222222222222',
    '33333333-3333-4333-8333-333333333301',
    'Anna Lund',
    'anna.lund@example.com',
    'https://www.linkedin.com/in/mini-ats-demo-anna-lund',
    'new'::public.candidate_stage,
    'Senior frontend engineer with 6 years of Vue 3, TypeScript, Vite, accessibility, design systems, and performance optimization experience.',
    'Strong Vue and TypeScript profile for the frontend role.',
    NULL,
    NULL,
    '22222222-2222-4222-8222-222222222222',
    '22222222-2222-4222-8222-222222222222',
    NOW() - INTERVAL '3 days',
    NOW() - INTERVAL '3 days'
  ),

  -- Candidate 2: Erik Berg (Frontend, Screening, AI: NULL)
  (
    '55555555-5555-4555-8555-555555555502',
    '22222222-2222-4222-8222-222222222222',
    '33333333-3333-4333-8333-333333333301',
    'Erik Berg',
    'erik.berg@example.com',
    'https://www.linkedin.com/in/mini-ats-demo-erik-berg',
    'screening'::public.candidate_stage,
    'Frontend developer experienced in React and Vue, TypeScript, component libraries, testing, and responsive CSS.',
    'Generalist frontend candidate currently in screening.',
    NULL,
    NULL,
    '22222222-2222-4222-8222-222222222222',
    '22222222-2222-4222-8222-222222222222',
    NOW() - INTERVAL '3 days',
    NOW() - INTERVAL '3 days'
  ),

  -- Candidate 3: Maria Karlsson (Frontend, Interview, AI: Pre-filled high score 86)
  (
    '55555555-5555-4555-8555-555555555503',
    '22222222-2222-4222-8222-222222222222',
    '33333333-3333-4333-8333-333333333301',
    'Maria Karlsson',
    'maria.karlsson@example.com',
    'https://www.linkedin.com/in/mini-ats-demo-maria-karlsson',
    'interview'::public.candidate_stage,
    'Product-minded frontend engineer with Vue 3, TypeScript, Nuxt, design systems, accessibility audits, and performance budgets.',
    'Advanced frontend candidate ready for interview.',
    86,
    '{
      "summary": "Strong frontend match with demonstrated Vue 3, TypeScript, accessibility, and performance experience.",
      "strengths": [
        "Vue 3 and TypeScript depth",
        "Accessibility and design systems",
        "Performance-minded delivery"
      ],
      "concerns": [
        "Needs more evidence of cross-team backend collaboration"
      ],
      "questions": [
        "Describe a performance budget you enforced and the outcome.",
        "How have you mentored junior frontend developers?"
      ],
      "provider": "mock"
    }'::jsonb,
    '22222222-2222-4222-8222-222222222222',
    '22222222-2222-4222-8222-222222222222',
    NOW() - INTERVAL '2 days',
    NOW() - INTERVAL '2 days'
  ),

  -- Candidate 4: Jonas Nyström (Frontend, Offer, AI: NULL)
  (
    '55555555-5555-4555-8555-555555555504',
    '22222222-2222-4222-8222-222222222222',
    '33333333-3333-4333-8333-333333333301',
    'Jonas Nyström',
    'jonas.nystrom@example.com',
    'https://www.linkedin.com/in/mini-ats-demo-jonas-nystrom',
    'offer'::public.candidate_stage,
    'Senior Vue and TypeScript engineer who led migration from legacy JavaScript to Vue 3, with mentoring, testing, and CI experience.',
    'Offer-stage candidate with leadership signals.',
    NULL,
    NULL,
    '22222222-2222-4222-8222-222222222222',
    '22222222-2222-4222-8222-222222222222',
    NOW() - INTERVAL '2 days',
    NOW() - INTERVAL '2 days'
  ),

  -- Candidate 5: Sofia Lindqvist (Frontend, Hired, AI: NULL)
  (
    '55555555-5555-4555-8555-555555555505',
    '22222222-2222-4222-8222-222222222222',
    '33333333-3333-4333-8333-333333333301',
    'Sofia Lindqvist',
    'sofia.lindqvist@example.com',
    'https://www.linkedin.com/in/mini-ats-demo-sofia-lindqvist',
    'hired'::public.candidate_stage,
    'Frontend specialist with Vue 3, TypeScript, Vite, WCAG, design tokens, and micro-frontend experience.',
    'Hired frontend candidate.',
    NULL,
    NULL,
    '22222222-2222-4222-8222-222222222222',
    '22222222-2222-4222-8222-222222222222',
    NOW() - INTERVAL '4 days',
    NOW() - INTERVAL '4 days'
  ),

  -- Candidate 6: Oscar Dahl (Backend, Rejected, AI: Pre-filled low score 48)
  (
    '55555555-5555-4555-8555-555555555506',
    '22222222-2222-4222-8222-222222222222',
    '33333333-3333-4333-8333-333333333302',
    'Oscar Dahl',
    'oscar.dahl@example.com',
    'https://www.linkedin.com/in/mini-ats-demo-oscar-dahl',
    'rejected'::public.candidate_stage,
    'Backend developer with Node.js and some C sharp, REST APIs, SQL, and limited .NET depth.',
    'Rejected due to limited .NET alignment.',
    48,
    '{
      "summary": "Backend-adjacent candidate but limited .NET alignment for this role.",
      "strengths": [
        "REST API experience",
        "SQL fundamentals"
      ],
      "concerns": [
        "Primary stack is Node.js rather than .NET or C sharp",
        "Limited evidence of JWT and PostgreSQL depth"
      ],
      "questions": [
        "Describe your most relevant .NET or C sharp project.",
        "How would you secure a REST API with JWT?"
      ],
      "provider": "mock"
    }'::jsonb,
    '22222222-2222-4222-8222-222222222222',
    '22222222-2222-4222-8222-222222222222',
    NOW() - INTERVAL '1 day',
    NOW() - INTERVAL '1 day'
  ),

  -- Candidate 7: Elsa Moreau (Frontend, New, AI: NULL)
  (
    '55555555-5555-4555-8555-555555555507',
    '22222222-2222-4222-8222-222222222222',
    '33333333-3333-4333-8333-333333333301',
    'Elsa Moreau',
    'elsa.moreau@example.com',
    'https://www.linkedin.com/in/mini-ats-demo-elsa-moreau',
    'new'::public.candidate_stage,
    'EU frontend engineer with Vue, TypeScript, CSS, internationalization, and accessible component design experience.',
    'New candidate with strong CSS and accessibility background.',
    NULL,
    NULL,
    '22222222-2222-4222-8222-222222222222',
    '22222222-2222-4222-8222-222222222222',
    NOW() - INTERVAL '1 day',
    NOW() - INTERVAL '1 day'
  ),

  -- Candidate 8: Lucas Meyer (Frontend, Screening, Missing LinkedIn, Short CV, AI: Pre-filled low score 42)
  (
    '55555555-5555-4555-8555-555555555508',
    '22222222-2222-4222-8222-222222222222',
    '33333333-3333-4333-8333-333333333301',
    'Lucas Meyer',
    'lucas.meyer@example.com',
    NULL,
    'screening'::public.candidate_stage,
    'Junior developer with some Vue tutorials, HTML, CSS, and JavaScript.',
    'Early-stage candidate with limited proof.',
    42,
    '{
      "summary": "Early-stage candidate with limited verified experience and missing LinkedIn profile.",
      "strengths": [
        "Basic HTML, CSS, and JavaScript exposure",
        "Interest in frontend work"
      ],
      "concerns": [
        "No LinkedIn URL provided",
        "CV text is short",
        "Limited evidence of Vue or TypeScript production use"
      ],
      "questions": [
        "Share a small Vue or JavaScript project you built end to end.",
        "What production experience do you have with TypeScript?"
      ],
      "provider": "mock"
    }'::jsonb,
    '22222222-2222-4222-8222-222222222222',
    '22222222-2222-4222-8222-222222222222',
    NOW() - INTERVAL '2 days',
    NOW() - INTERVAL '2 days'
  ),

  -- Candidate 9: Nina Patel (Backend, Interview, AI: Pre-filled high score 84)
  (
    '55555555-5555-4555-8555-555555555509',
    '22222222-2222-4222-8222-222222222222',
    '33333333-3333-4333-8333-333333333302',
    'Nina Patel',
    'nina.patel@example.com',
    'https://www.linkedin.com/in/mini-ats-demo-nina-patel',
    'interview'::public.candidate_stage,
    'Backend engineer with .NET 8, C sharp, REST, JWT, PostgreSQL, Docker, and Supabase exposure.',
    'Strong backend candidate for interview.',
    84,
    '{
      "summary": "Strong backend candidate with relevant .NET, REST, JWT, PostgreSQL, and containerization experience.",
      "strengths": [
        ".NET and C sharp backend experience",
        "REST and JWT understanding",
        "PostgreSQL and Docker exposure"
      ],
      "concerns": [
        "Could provide more examples of Supabase or AI-service integration"
      ],
      "questions": [
        "Describe an API you designed with authentication and authorization.",
        "How do you keep database migrations safe in production?"
      ],
      "provider": "mock"
    }'::jsonb,
    '22222222-2222-4222-8222-222222222222',
    '22222222-2222-4222-8222-222222222222',
    NOW() - INTERVAL '1 day',
    NOW() - INTERVAL '1 day'
  ),

  -- Candidate 10: Hugo Silva (Backend, New, AI: NULL for stage-move demo)
  (
    '55555555-5555-4555-8555-555555555510',
    '22222222-2222-4222-8222-222222222222',
    '33333333-3333-4333-8333-333333333302',
    'Hugo Silva',
    'hugo.silva@example.com',
    'https://www.linkedin.com/in/mini-ats-demo-hugo-silva',
    'new'::public.candidate_stage,
    'Software engineer moving from Java to .NET, with solid SQL, APIs, and eagerness to learn C sharp, Supabase, and clean architecture.',
    'Potential backend candidate needing deeper .NET proof.',
    NULL,
    NULL,
    '22222222-2222-4222-8222-222222222222',
    '22222222-2222-4222-8222-222222222222',
    NOW() - INTERVAL '6 hours',
    NOW() - INTERVAL '6 hours'
  )
ON CONFLICT (id) DO UPDATE SET
  customer_id  = EXCLUDED.customer_id,
  job_id       = EXCLUDED.job_id,
  full_name    = EXCLUDED.full_name,
  email        = EXCLUDED.email,
  linkedin_url = EXCLUDED.linkedin_url,
  stage        = EXCLUDED.stage,
  cv_text      = EXCLUDED.cv_text,
  summary      = EXCLUDED.summary,
  ai_score     = EXCLUDED.ai_score,
  ai_feedback  = EXCLUDED.ai_feedback,
  updated_at   = NOW();

-- -----------------------------------------------------------------------------
-- Verification Query
-- -----------------------------------------------------------------------------
SELECT
  c.full_name,
  c.stage,
  j.title AS job_title,
  c.ai_score,
  (c.linkedin_url IS NOT NULL) AS has_linkedin
FROM public.candidates c
JOIN public.jobs j ON j.id = c.job_id
WHERE c.customer_id = '22222222-2222-4222-8222-222222222222'
ORDER BY c.stage, c.full_name;
