# Database Contract Reference

## Enums

```sql
public.user_role: admin, customer
public.candidate_stage: new, screening, interview, offer, hired, rejected
```

## Tables

### public.profiles

Extends `auth.users` with app-specific role and metadata.

| Column | Type | Notes |
|---|---|---|
| id | uuid PK | references auth.users(id) on delete cascade |
| email | text | not null unique |
| role | user_role | not null default 'customer' |
| display_name | text | nullable |
| company_name | text | nullable |
| created_at | timestamptz | not null default now() |
| updated_at | timestamptz | not null default now() |

### public.jobs

Jobs belong to one customer. Scoped by `customer_id` in every query.

| Column | Type | Notes |
|---|---|---|
| id | uuid PK | default gen_random_uuid() |
| customer_id | uuid FK | references profiles(id) on delete cascade |
| title | text | not null |
| description | text | nullable |
| status | text | not null default 'active' |
| created_by | uuid FK | references profiles(id) on delete set null |
| updated_by | uuid FK | references profiles(id) on delete set null |
| created_at | timestamptz | not null default now() |
| updated_at | timestamptz | not null default now() |

### public.candidates

Candidates belong to a customer, optionally to a job.

| Column | Type | Notes |
|---|---|---|
| id | uuid PK | default gen_random_uuid() |
| customer_id | uuid FK | references profiles(id) on delete cascade |
| job_id | uuid FK | references jobs(id) on delete set null; nullable |
| full_name | text | not null |
| email | text | nullable |
| linkedin_url | text | nullable |
| cv_storage_path | text | nullable – future Supabase Storage path |
| cv_text | text | nullable – pasted CV text |
| summary | text | nullable |
| stage | candidate_stage | not null default 'new' |
| ai_score | numeric | nullable – set by AI assess endpoint |
| ai_feedback | jsonb | nullable – full AI response stored here |
| created_by | uuid FK | references profiles(id) on delete set null |
| updated_by | uuid FK | references profiles(id) on delete set null |
| created_at | timestamptz | not null default now() |
| updated_at | timestamptz | not null default now() |

## Indexes

```sql
idx_jobs_customer_created_at        ON jobs (customer_id, created_at DESC)
idx_candidates_customer_job_stage   ON candidates (customer_id, job_id, stage)
idx_candidates_customer_full_name   ON candidates (customer_id, full_name)
```

## Triggers

`public.set_updated_at()` is fired BEFORE UPDATE on:
- `public.profiles`
- `public.jobs`
- `public.candidates`

## RLS

RLS is enabled on all three tables. No broad public policies are created. The .NET API is the trusted server-side data access layer and uses the Postgres connection string directly (not the Supabase anon key).

## Bootstrap admin

1. Create a user in Supabase Auth dashboard (email + password).
2. Copy the auth user UUID from `auth.users`.
3. Run the bootstrap SQL in `supabase/seed/bootstrap_admin.sql` replacing the UUID and email.

## Key SQL patterns

### Customer-scoped candidate query
```sql
SELECT * FROM public.candidates
WHERE customer_id = @customerId
  AND (@jobId IS NULL OR job_id = @jobId)
  AND (@search IS NULL OR full_name ILIKE @searchPattern)
ORDER BY created_at DESC;
```

### Stage update
```sql
UPDATE public.candidates
SET stage = @stage::public.candidate_stage, updated_by = @updatedBy
WHERE id = @id AND customer_id = @customerId;
```

### AI result save
```sql
UPDATE public.candidates
SET ai_score = @score, ai_feedback = @feedback::jsonb, updated_by = @updatedBy
WHERE id = @id AND customer_id = @customerId;
```
