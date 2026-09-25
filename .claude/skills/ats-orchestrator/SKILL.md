---
name: ats-orchestrator
description: Orchestrates building, extending, testing, documenting, and committing the Mini ATS prototype. Use when the user asks to implement the ATS, create backend/frontend/AI features, prepare Supabase/.NET/Vue/Python code, manage demo docs, or produce the 60-commit plan.
disable-model-invocation: true
allowed-tools: Read Write Edit Glob Grep Bash(git status *) Bash(git diff *) Bash(git log *) Bash(git add *) Bash(git commit *) Bash(dotnet build *) Bash(npm run build *) Bash(npm run lint *) Bash(pytest *)
---

# Mini ATS Orchestrator

You are the lead engineer and code generator for a prototype Applicant Tracking System.

## Mission

Build a small but coherent full-stack ATS prototype with:

- Supabase Postgres as database, Supabase Auth as identity provider.
- .NET 10 REST API with JWT validation as the main backend.
- Python FastAPI service for simplified CV assessment.
- Vue 3 + TypeScript + Vite frontend.
- Admin can create admin/customer accounts.
- Customer can log in, create jobs, create candidates, view kanban, filter by job and name.
- Admin can perform all customer actions on behalf of a selected customer.
- Optional AI: assess candidate CV/profile and return score, strengths, concerns, questions.
- Deploy frontend to Vercel, backend/AI to Railway.
- Produce documentation, demo script, placeholder images, and a 60-commit traceability plan.

## Hard limitations

Never violate these rules.

1. **Never commit secrets.**
   - No Supabase service role key, JWT secret, DB password, OpenAI/Anthropic key, AI service key.
   - Use `.env.example`, `appsettings.Development.json`, or deployment env vars.

2. **Never call external paid APIs** unless the user explicitly provided credentials and approved usage.
   - If no LLM key exists, use deterministic mock scoring.

3. **Never run destructive git commands.**
   - No `git reset --hard`, `git clean -fd`, `git push --force`, branch deletion, or history rewrite.

4. **Never push to GitHub** unless the user explicitly asks.

5. **Never modify files outside the repository.**

6. **Never invent deployment status.**
   - If not deployed, say "not deployed". If screenshots are not real, label them "placeholder".

7. **Never bypass authorization.**
   - Customers may only access their own jobs/candidates.
   - Admin may act on behalf of a customer only through explicit `customerId` scoping.

8. **Never leave the project in a non-buildable state** at the end of a phase.
   - Run the closest available build/test command before finishing a slice.

9. **Keep `SKILL.md` concise.** Move long reference material to `docs/`.

10. **Every non-trivial code line should have a short comment** explaining intent or relation to another file.
    - Comment every meaningful block so a human can trace relations between backend, frontend, AI, database, env vars, endpoints, stores, and components.
    - Do not comment obvious syntax (braces, semicolons, trivial imports).

## Source of truth

Before implementing, read:

- `docs/QA.md` for architecture rationale.
- `docs/DEMO.md` for demo expectations.
- `docs/ENVIRONMENT_VARIABLES.md` for required config.
- `supabase/migrations/0001_init.sql` for database contract.
- `.claude/skills/ats-orchestrator/reference/ARCHITECTURE.md`
- `.claude/skills/ats-orchestrator/reference/API_CONTRACT.md`
- `.claude/skills/ats-orchestrator/reference/DATABASE_CONTRACT.md`
- `.claude/skills/ats-orchestrator/reference/FRONTEND_CONTRACT.md`
- `.claude/skills/ats-orchestrator/reference/AI_CONTRACT.md`
- `.claude/skills/ats-orchestrator/reference/DEPLOYMENT.md`

## Stack rules

### Backend
- C# .NET 10, Web API controllers, OOP layering.
- JWT bearer authentication validated against Supabase JWT secret.
- Npgsql for Postgres, parameterized SQL only.
- Layers: `Domain` → `Application` → `Infrastructure` → `Api`.
- Controllers thin, Services contain business rules, Repositories contain SQL.
- DTOs separate from domain entities. Async/await for all I/O.

### Frontend
- Vue 3, TypeScript, Vite, plain CSS.
- Pinia for auth/app state. Vue Router with guards.
- Supabase JS only for authentication.
- .NET API for all domain data (jobs, candidates, AI assessment).
- Never expose service role key to frontend.

### AI service
- Python FastAPI, `POST /assess`, internal service key header.
- Return: `score`, `summary`, `strengths`, `concerns`, `questions`, `provider`.
- If no LLM key: deterministic heuristic mock scoring.
- Stateless. No protected-characteristic inference.

### Database
- Supabase Postgres, `auth.users` as identity source.
- `public.profiles`, `public.jobs`, `public.candidates`.
- RLS enabled, no broad public policies.
- .NET API is trusted server-side data access layer.

## Authentication contract

1. Frontend calls `supabase.auth.signInWithPassword`.
2. Supabase returns JWT access token.
3. Frontend sends `Authorization: Bearer <token>` to .NET API.
4. .NET validates signature using `Supabase:JwtSecret`.
5. .NET loads `public.profiles` by token `sub`.
6. .NET enforces role and `customer_id` scoping.

Bootstrap: create first admin manually in Supabase Auth dashboard, then insert profile row with role `admin`.

## API contract summary

See `.claude/skills/ats-orchestrator/reference/API_CONTRACT.md` for full spec.

- `GET /health`, `GET /api/account/me`
- `POST /api/admin/users`, `GET /api/admin/users`
- `GET|POST|PATCH|DELETE /api/jobs`
- `GET|POST|PATCH|DELETE /api/candidates`, `PATCH /api/candidates/{id}/stage`
- `POST /api/ai/candidates/{id}/assess`

## Database contract summary

See `.claude/skills/ats-orchestrator/reference/DATABASE_CONTRACT.md`.

Tables: `profiles`, `jobs`, `candidates`.
Enums: `user_role (admin|customer)`, `candidate_stage (new|screening|interview|offer|hired|rejected)`.
Indexes on customer, job, stage, name. Triggers for `updated_at`. RLS enabled.

## Frontend contract summary

See `.claude/skills/ats-orchestrator/reference/FRONTEND_CONTRACT.md`.

Routes: `/login`, `/`, `/kanban`, `/admin`.
Stores: auth store, ATS store.
Components: LoginView, DashboardView, KanbanView, AdminView, AppShell, JobForm, CandidateForm, KanbanColumn, CandidateCard, AiAssessmentPanel.

## AI contract summary

See `.claude/skills/ats-orchestrator/reference/AI_CONTRACT.md`.

Request: `candidate_name`, `job_title`, `job_description`, `cv_text`, `linkedin_url`, `summary`.
Response: `score`, `summary`, `strengths`, `concerns`, `questions`, `provider`.

## Deployment contract summary

See `.claude/skills/ats-orchestrator/reference/DEPLOYMENT.md`.

Frontend → Vercel. Backend + AI service → Railway. Health endpoints at `/health`.

## Workflow phases

Work in vertical slices. For each slice:
1. Read relevant existing files.
2. State short plan.
3. Implement smallest coherent change.
4. Add comments linking relations.
5. Run build/test if available.
6. Update docs if contract changed.
7. Create a conventional commit.

Preferred order: repo skeleton → DB → .NET auth → profile → admin → jobs → candidates → AI proxy → Python AI → Vue scaffold → auth/dashboard → kanban → admin UI → AI UI → deploy → docs → build/test → 60 commits.

## Definition of done

- Code compiles or missing-dependency notes are clear.
- No secrets committed.
- Comments explain line intent and cross-file relations.
- API and DB contracts respected.
- Customer isolation enforced.
- Admin can act on behalf of customer.
- Kanban filters by job and name.
- AI assessment returns structured JSON.
- Demo doc and commit plan exist.

## Commands to prefer

```bash
dotnet build backend/MiniAts.Api/MiniAts.Api.csproj
npm --prefix frontend install && npm --prefix frontend run build
python -m pytest backend/ai-service
git rev-list --count HEAD
```

## References

- `.claude/skills/ats-orchestrator/reference/ARCHITECTURE.md`
- `.claude/skills/ats-orchestrator/reference/API_CONTRACT.md`
- `.claude/skills/ats-orchestrator/reference/DATABASE_CONTRACT.md`
- `.claude/skills/ats-orchestrator/reference/FRONTEND_CONTRACT.md`
- `.claude/skills/ats-orchestrator/reference/AI_CONTRACT.md`
- `.claude/skills/ats-orchestrator/reference/DEPLOYMENT.md`
