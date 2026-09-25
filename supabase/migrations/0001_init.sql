-- ============================================================
-- Mini ATS – Supabase Postgres migration 0001_init.sql
-- Related: backend/MiniAts.Api/Infrastructure/Repositories/
--          supabase/seed/bootstrap_admin.sql
--          .claude/skills/ats-orchestrator/reference/DATABASE_CONTRACT.md
-- ============================================================

-- Enable UUID generation used for primary keys on jobs and candidates.
CREATE EXTENSION IF NOT EXISTS pgcrypto;

-- ── Enums ─────────────────────────────────────────────────────────────────────

-- Application role: admin has full access, customer owns their own data.
CREATE TYPE public.user_role AS ENUM ('admin', 'customer');

-- Kanban stages used across all candidate records.
CREATE TYPE public.candidate_stage AS ENUM (
  'new',
  'screening',
  'interview',
  'offer',
  'hired',
  'rejected'
);

-- ── Tables ────────────────────────────────────────────────────────────────────

-- profiles extends auth.users with app-specific role and customer metadata.
-- Every Supabase Auth user that can use the ATS must have a matching profile row.
CREATE TABLE IF NOT EXISTS public.profiles (
  id            UUID        PRIMARY KEY REFERENCES auth.users(id) ON DELETE CASCADE,
  email         TEXT        NOT NULL UNIQUE,
  role          public.user_role NOT NULL DEFAULT 'customer',
  display_name  TEXT,
  company_name  TEXT,
  created_at    TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at    TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- jobs belong to one customer profile; all queries must filter by customer_id.
CREATE TABLE IF NOT EXISTS public.jobs (
  id           UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
  customer_id  UUID        NOT NULL REFERENCES public.profiles(id) ON DELETE CASCADE,
  title        TEXT        NOT NULL,
  description  TEXT,
  status       TEXT        NOT NULL DEFAULT 'active',
  created_by   UUID        REFERENCES public.profiles(id) ON DELETE SET NULL,
  updated_by   UUID        REFERENCES public.profiles(id) ON DELETE SET NULL,
  created_at   TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at   TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- candidates belong to a customer and optionally link to a specific job.
-- ai_score and ai_feedback are populated by POST /api/ai/candidates/{id}/assess.
CREATE TABLE IF NOT EXISTS public.candidates (
  id               UUID                  PRIMARY KEY DEFAULT gen_random_uuid(),
  customer_id      UUID                  NOT NULL REFERENCES public.profiles(id) ON DELETE CASCADE,
  job_id           UUID                  REFERENCES public.jobs(id) ON DELETE SET NULL,
  full_name        TEXT                  NOT NULL,
  email            TEXT,
  linkedin_url     TEXT,
  cv_storage_path  TEXT,           -- future Supabase Storage path for uploaded CV
  cv_text          TEXT,           -- pasted CV or profile text used by AI service
  summary          TEXT,
  stage            public.candidate_stage NOT NULL DEFAULT 'new',
  ai_score         NUMERIC,        -- 0–100, stored after Python AI service call
  ai_feedback      JSONB,          -- full AI response JSON from Python service
  created_by       UUID            REFERENCES public.profiles(id) ON DELETE SET NULL,
  updated_by       UUID            REFERENCES public.profiles(id) ON DELETE SET NULL,
  created_at       TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
  updated_at       TIMESTAMPTZ     NOT NULL DEFAULT NOW()
);

-- ── Indexes ───────────────────────────────────────────────────────────────────

-- Dashboard job list ordered by newest first within each customer.
CREATE INDEX IF NOT EXISTS idx_jobs_customer_created_at
  ON public.jobs (customer_id, created_at DESC);

-- Kanban filtering by customer, job, and stage (primary access pattern).
CREATE INDEX IF NOT EXISTS idx_candidates_customer_job_stage
  ON public.candidates (customer_id, job_id, stage);

-- Candidate name search within a customer scope.
CREATE INDEX IF NOT EXISTS idx_candidates_customer_full_name
  ON public.candidates (customer_id, full_name);

-- ── Updated-at trigger ────────────────────────────────────────────────────────

-- Function refreshes updated_at before every UPDATE on app tables.
CREATE OR REPLACE FUNCTION public.set_updated_at()
RETURNS TRIGGER
LANGUAGE plpgsql
AS $$
BEGIN
  -- Always stamp the current timestamp before the row is written.
  NEW.updated_at = NOW();
  RETURN NEW;
END;
$$;

-- Trigger on profiles so updated_at stays current on role/name changes.
DROP TRIGGER IF EXISTS trg_profiles_updated_at ON public.profiles;
CREATE TRIGGER trg_profiles_updated_at
  BEFORE UPDATE ON public.profiles
  FOR EACH ROW EXECUTE FUNCTION public.set_updated_at();

-- Trigger on jobs so updated_at reflects last edit.
DROP TRIGGER IF EXISTS trg_jobs_updated_at ON public.jobs;
CREATE TRIGGER trg_jobs_updated_at
  BEFORE UPDATE ON public.jobs
  FOR EACH ROW EXECUTE FUNCTION public.set_updated_at();

-- Trigger on candidates so updated_at reflects stage changes and AI results.
DROP TRIGGER IF EXISTS trg_candidates_updated_at ON public.candidates;
CREATE TRIGGER trg_candidates_updated_at
  BEFORE UPDATE ON public.candidates
  FOR EACH ROW EXECUTE FUNCTION public.set_updated_at();

-- ── Row Level Security ────────────────────────────────────────────────────────

-- Enable RLS on all three tables.
-- No broad public policies are created intentionally.
-- The .NET API connects via the Postgres connection string as a trusted server-side layer.
-- If you add a Supabase JS client that talks directly to Postgres, add appropriate policies then.

ALTER TABLE public.profiles  ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.jobs      ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.candidates ENABLE ROW LEVEL SECURITY;

-- Each user can read their own profile (needed for Supabase JS auth refresh scenarios).
DROP POLICY IF EXISTS "profiles: user reads own" ON public.profiles;
CREATE POLICY "profiles: user reads own"
  ON public.profiles
  FOR SELECT
  USING (auth.uid() = id);
