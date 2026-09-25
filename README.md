# Mini ATS Prototype

Prototype Applicant Tracking System built with:

- **Vue 3 + TypeScript + Vite** — frontend deployed on Vercel
- **.NET 10 ASP.NET Core Web API** — backend deployed on Railway
- **Python FastAPI** — AI CV assessment service deployed on Railway
- **Supabase Postgres + Supabase Auth** — database and identity

## Features

| Who | Can do |
|---|---|
| Admin | Log in, create admin/customer accounts, list users, act on behalf of any customer |
| Customer | Log in, create jobs, add candidates (name, email, LinkedIn URL, CV text), view Kanban board, filter by job and name, move candidates between stages |
| Anyone | AI-powered CV assessment returning score, strengths, concerns, and interview questions |

## Quick start (local)

### Prerequisites

- .NET 10 SDK
- Node.js 20+
- Python 3.10+
- Supabase project (free tier works)

### 1. Supabase setup

1. Create a Supabase project at https://supabase.com
2. Run the migration in the SQL editor:
   ```sql
   -- Copy contents of supabase/migrations/0001_init.sql and run
   ```
3. Create first admin user in Supabase Auth dashboard.
4. Run `supabase/seed/bootstrap_admin.sql` to set admin role.

### 2. Backend (.NET API)

```bash
# Copy example config and fill in your Supabase values
cp backend/MiniAts.Api/appsettings.Development.json.example backend/MiniAts.Api/appsettings.Development.json

# Run the API (listens on http://localhost:5000 by default)
dotnet run --project backend/MiniAts.Api/MiniAts.Api.csproj
```

### 3. AI service (Python)

```bash
cd backend/ai-service
python -m venv .venv
source .venv/bin/activate  # Windows: .venv\Scripts\activate
pip install -r requirements.txt

# Copy and configure env
cp .env.example .env

uvicorn main:app --reload --port 8001
```

### 4. Frontend (Vue + Vite)

```bash
cd frontend
cp .env.example .env.local  # fill in your values

npm install
npm run dev  # opens http://localhost:5173
```

## Demo seed data

```text
Admin:    admin@demo-ats.local
Customer: customer@demo-ats.local  /  Company: Nordic Tech AB

Job: Senior Frontend Engineer
Candidates: Anna Lund (new), Erik Berg (screening), Maria Karlsson (interview)
```

## Demo

See [`docs/DEMO.md`](docs/DEMO.md) for the 5-minute recording script.

Generate placeholder images:
```bash
python scripts/generate-demo-placeholders.py
```

## Documentation

| File | Purpose |
|---|---|
| [`docs/QA.md`](docs/QA.md) | Architecture Q&A (why and how) |
| [`docs/DEMO.md`](docs/DEMO.md) | 5-minute demo script |
| [`docs/ENVIRONMENT_VARIABLES.md`](docs/ENVIRONMENT_VARIABLES.md) | All env vars documented |
| [`docs/COMMIT_PLAN.md`](docs/COMMIT_PLAN.md) | 60-commit plan |
| [`docs/PROGRESS.md`](docs/PROGRESS.md) | Implementation progress log |

## AI orchestrator skill

For Claude Code or compatible AI agents:

```text
.claude/skills/ats-orchestrator/SKILL.md
```

## Deploy

- **Frontend → Vercel**: connect `frontend/` directory, set env vars.
- **Backend → Railway**: uses `railway.toml` at repo root.
- **AI service → Railway**: second service pointing at `backend/ai-service/`, uses `Procfile`.

See [`docs/ENVIRONMENT_VARIABLES.md`](docs/ENVIRONMENT_VARIABLES.md) for all required env vars.

## Known limitations (prototype)

- No CV file upload (text paste only — Supabase Storage path column exists for future use).
- No interview scheduling.
- No multi-user customer teams.
- No pagination (suitable for demo scale).
- AI assessment uses mock scoring when no LLM key is configured.

## Assumptions

- Local ports: frontend 5173, backend 5000, AI 8001.
- Demo admin email: `admin@demo-ats.local`.
- Mock AI score formula: keyword overlap between job description and CV text.
- Supabase free tier is sufficient for prototype.
