# Mini ATS – Recruiter Velocity ATS

<p align="center">
  <img src="https://img.shields.io/badge/Vue.js_3.4-%234FC08D.svg?style=for-the-badge&logo=vuedotjs&logoColor=white" alt="Vue 3" />
  <img src="https://img.shields.io/badge/TypeScript_5.3-%233178C6.svg?style=for-the-badge&logo=typescript&logoColor=white" alt="TypeScript" />
  <img src="https://img.shields.io/badge/Vite_5.4-%23646CFF.svg?style=for-the-badge&logo=vite&logoColor=white" alt="Vite" />
  <img src="https://img.shields.io/badge/.NET_10.0-%23512BD4.svg?style=for-the-badge&logo=dotnet&logoColor=white" alt=".NET 10" />
  <img src="https://img.shields.io/badge/FastAPI-%23009688.svg?style=for-the-badge&logo=fastapi&logoColor=white" alt="FastAPI" />
  <img src="https://img.shields.io/badge/Python_3.10+-%233776AB.svg?style=for-the-badge&logo=python&logoColor=white" alt="Python" />
  <img src="https://img.shields.io/badge/Supabase-%233ECF8E.svg?style=for-the-badge&logo=supabase&logoColor=white" alt="Supabase" />
  <img src="https://img.shields.io/badge/PostgreSQL-Multi--Tenant-%234169E1.svg?style=for-the-badge&logo=postgresql&logoColor=white" alt="PostgreSQL" />
  <img src="https://img.shields.io/badge/i18n-English_%7C_Svenska-%231d68f0.svg?style=for-the-badge" alt="Localization" />
  <img src="https://img.shields.io/badge/Theme-Light_%7C_Dark-%230f172a.svg?style=for-the-badge" alt="Dark Mode" />
</p>

Executive-grade, high-density Applicant Tracking System built with:

- **Vue 3.4 + TypeScript + Vite** — frontend styled with the Recruiter Velocity ATS design system
- **.NET 10 ASP.NET Core Web API** — multi-tenant scoped backend with JWT authorization
- **Python FastAPI** — AI pipeline analysis and candidate CV match scoring service
- **Supabase PostgreSQL + Supabase Auth** — deterministic workspace isolation and identity

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

The project includes a deterministic 10-candidate canonical demo dataset designed for a complete 5-minute presentation:

```text
Admin:      admin@nordic-recruit.demo  (Nordic Recruit)
Customer:   recruiter@nordic-tech.demo (Nordic Tech AB)

Jobs (2):   Senior Frontend Engineer (Vue 3 + TS)
            Backend Engineer .NET (.NET 10 + Postgres + AI)

Candidates (10):
- New:       Anna Lund (live AI demo), Elsa Moreau, Hugo Silva
- Screening: Erik Berg, Lucas Meyer (low score: 42, missing LinkedIn)
- Interview: Maria Karlsson (high score: 86), Nina Patel (high score: 84)
- Offer:     Jonas Nyström
- Hired:     Sofia Lindqvist
- Rejected:  Oscar Dahl (low score: 48, stack mismatch)
```

To seed this dataset:
1. **SQL Method**: Ensure the two demo auth users exist (see [`supabase/seed/create_demo_auth_users.md`](supabase/seed/create_demo_auth_users.md)), then execute [`supabase/seed/demo_data.sql`](supabase/seed/demo_data.sql) in your Supabase SQL editor.
2. **Automated Helper**:
   ```bash
   export SUPABASE_URL="https://your-project.supabase.co"
   export SUPABASE_SERVICE_ROLE_KEY="your-service-role-key"
   python scripts/seed-demo.py
   ```

For the comprehensive dataset breakdown and test filter queries, consult [`docs/DEMO_SEED_DATA.md`](docs/DEMO_SEED_DATA.md).


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
- Mock AI score formula: keyword overlap between job description and CV text.
- Supabase free tier is sufficient for prototype.
- Login as admin or as customer by just clicking the button on the login page and you will be logged in with the demo credentials