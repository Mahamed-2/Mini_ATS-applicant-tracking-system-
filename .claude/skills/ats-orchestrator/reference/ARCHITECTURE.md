# Architecture Reference

## Overview

```
Browser (Vue 3 + TypeScript + Vite)
    │  Supabase JS → login/session/JWT
    │  fetch + Bearer JWT → .NET 10 API
    ▼
.NET 10 ASP.NET Core Web API (Railway)
    │  Npgsql → Supabase Postgres
    │  Supabase Auth Admin API (service role key)
    │  HTTP + service key → Python AI service
    ▼
Supabase (Postgres + Auth)
Python FastAPI AI Service (Railway)
    │  Optional → LLM API (OpenAI etc.)
```

## Component responsibilities

| Component | Responsibility |
|---|---|
| Supabase Auth | Issues JWT, stores users, verifies email |
| Supabase Postgres | Stores profiles, jobs, candidates |
| .NET 10 API | Validates JWT, enforces authorization, runs business logic |
| Python AI service | CV/profile scoring, mock + optional LLM |
| Vue frontend | User interface, auth session, API calls |
| Vercel | Hosts frontend static files |
| Railway | Hosts .NET API and Python AI service |

## Security model

- Frontend: only Supabase anon key exposed.
- Backend: holds service role key server-side for admin user creation.
- Backend: all data queries include `customer_id` scope.
- Backend: JWT validation: issuer = `{SUPABASE_URL}/auth/v1`, audience = `authenticated`.
- AI service: internal service key required if `AI_SERVICE_API_KEY` env is set.
- RLS: enabled on all public tables, no broad public policies.

## Data flow: candidate AI assessment

1. Frontend POSTs `/api/ai/candidates/{id}/assess`.
2. .NET loads candidate + related job from Postgres.
3. .NET calls `POST /assess` on Python AI service with candidate/job payload.
4. Python returns structured JSON (score, summary, strengths, concerns, questions, provider).
5. .NET stores `ai_score` and `ai_feedback` on the candidate row.
6. .NET returns updated AI feedback to frontend.

## Layer naming

```
backend/MiniAts.Api/
  Domain/          ← entities, enums, value objects
  Application/     ← DTOs, interfaces, services (business rules)
  Infrastructure/  ← repositories (SQL), external HTTP clients, auth helpers
  Api/             ← controllers, middleware, filters, response mapping
```

## Port conventions (local development)

- Frontend dev server: http://localhost:5173
- .NET API: http://localhost:5000
- Python AI service: http://localhost:8001
