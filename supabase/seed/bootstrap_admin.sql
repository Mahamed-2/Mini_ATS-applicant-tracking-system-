-- ============================================================
-- Mini ATS – Bootstrap first admin account
-- Related: supabase/migrations/0001_init.sql
--          backend/MiniAts.Api/Infrastructure/SupabaseAdminClient.cs
--          docs/ENVIRONMENT_VARIABLES.md
-- ============================================================
--
-- HOW TO USE THIS SCRIPT
-- ────────────────────────────────────────────────────────────
-- Step 1: Create the first admin user in Supabase Auth.
--         Option A – Supabase dashboard:
--           Authentication → Users → Invite or Create user
--           Email: admin@demo-ats.local
--           Password: choose a strong password
--
--         Option B – Supabase Auth Admin API (curl example):
--           curl -X POST '{SUPABASE_URL}/auth/v1/admin/users' \
--             -H 'apikey: {SERVICE_ROLE_KEY}' \
--             -H 'Authorization: Bearer {SERVICE_ROLE_KEY}' \
--             -H 'Content-Type: application/json' \
--             -d '{"email":"admin@demo-ats.local","password":"YourStrongPassword!","email_confirm":true}'
--
-- Step 2: Copy the UUID from the response or from:
--           SELECT id, email FROM auth.users WHERE email = 'admin@demo-ats.local';
--
-- Step 3: Replace PASTE_AUTH_USER_UUID_HERE below with the copied UUID.
--
-- Step 4: Run this script in the Supabase SQL editor.
-- ────────────────────────────────────────────────────────────

-- Insert the admin profile row.
-- ON CONFLICT allows re-running this script safely (idempotent).
INSERT INTO public.profiles (id, email, role, display_name, company_name)
VALUES (
  'PASTE_AUTH_USER_UUID_HERE',      -- auth.users UUID from Step 2
  'admin@demo-ats.local',           -- must match the email used in auth.users
  'admin',                          -- grants access to all admin endpoints
  'Seed Admin',
  'Internal'
)
ON CONFLICT (id)
DO UPDATE SET
  role         = EXCLUDED.role,
  display_name = EXCLUDED.display_name,
  company_name = EXCLUDED.company_name;

-- Verify the result.
SELECT id, email, role, display_name FROM public.profiles WHERE email = 'admin@demo-ats.local';
