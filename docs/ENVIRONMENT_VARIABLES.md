# Environment Variables Reference

This document lists all required environment variables for every service.
**Never commit real values. Use this file as a reference when configuring Vercel, Railway, and local development.**

---

## Frontend / Vercel

Set these in Vercel dashboard → Settings → Environment Variables.
Also create `frontend/.env.local` for local development (copy from `frontend/.env.example`).

| Variable | Purpose | Example |
|---|---|---|
| `VITE_SUPABASE_URL` | Supabase project URL | `https://abcdef.supabase.co` |
| `VITE_SUPABASE_ANON_KEY` | Supabase public anon key | `eyJhb...` (from Supabase Settings → API) |
| `VITE_API_BASE_URL` | .NET API base URL | `https://mini-ats-api.up.railway.app` |

**Where to find Supabase values:**
- Project URL and Anon Key: Supabase dashboard → Settings → API
- Anon key is public-safe; it's only used for Supabase Auth in the browser.

---

## Backend / .NET API / Railway

Set these as Railway environment variables for the .NET service.

| Variable | Purpose | Example |
|---|---|---|
| `ASPNETCORE_URLS` | Bind to Railway's dynamic port | `http://0.0.0.0:$PORT` |
| `ConnectionStrings__Supabase` | Postgres connection string | `Host=db.xyz.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=...;SSL Mode=Require` |
| `Supabase__Url` | Supabase project URL | `https://abcdef.supabase.co` |
| `Supabase__JwtSecret` | JWT signing secret | From Supabase Settings → API → JWT Secret |
| `Supabase__ServiceRoleKey` | Supabase admin key for user creation | From Supabase Settings → API (keep server-side ONLY) |
| `AiService__BaseUrl` | Python AI service URL | `https://mini-ats-ai.up.railway.app` |
| `AiService__ApiKey` | Internal key sent to AI service | Any random string shared with AI service |
| `Cors__AllowedOrigins` | Allowed frontend origins | `https://mini-ats.vercel.app,http://localhost:5173` |

**Where to find Supabase values:**
- JWT Secret: Supabase dashboard → Settings → API → JWT Secret
- Service Role Key: Supabase dashboard → Settings → API → `service_role` key
- Postgres URI: Supabase dashboard → Settings → Database → Connection string → URI mode

⚠️ **Service Role Key grants full database access. Never expose it in the frontend or commit it.**

---

## AI Service / Python FastAPI / Railway

Set these as Railway environment variables for the Python service.
Also create `backend/ai-service/.env` for local development (copy from `.env.example`).

| Variable | Purpose | Example |
|---|---|---|
| `PORT` | Listen port (Railway sets this automatically) | `8001` |
| `AI_SERVICE_API_KEY` | Internal key required in `X-AI-Service-Key` header | Any random string |
| `AI_PROVIDER` | AI provider name (informational) | `openai` |
| `OPENAI_API_KEY` | OpenAI API key (optional – uses mock if not set) | `sk-...` |
| `OPENAI_MODEL` | Model to use for LLM assessment | `gpt-4o-mini` |

If `OPENAI_API_KEY` is not set, the service uses **deterministic mock scoring** and no paid API is called.

---

## Local development summary

Create these files (gitignored):

### `frontend/.env.local`
```env
VITE_SUPABASE_URL=https://YOUR_PROJECT.supabase.co
VITE_SUPABASE_ANON_KEY=YOUR_ANON_KEY
VITE_API_BASE_URL=http://localhost:5000
```

### `backend/MiniAts.Api/appsettings.Development.json`
```json
{
  "ConnectionStrings": {
    "Supabase": "Host=YOUR_DB_HOST;Port=5432;Database=postgres;Username=postgres;Password=YOUR_PASSWORD;SSL Mode=Require"
  },
  "Supabase": {
    "Url": "https://YOUR_PROJECT.supabase.co",
    "JwtSecret": "YOUR_JWT_SECRET",
    "ServiceRoleKey": "YOUR_SERVICE_ROLE_KEY"
  },
  "AiService": {
    "BaseUrl": "http://localhost:8001",
    "ApiKey": "local-dev-key"
  },
  "Cors": {
    "AllowedOrigins": "http://localhost:5173"
  }
}
```

### `backend/ai-service/.env`
```env
AI_SERVICE_API_KEY=local-dev-key
AI_PROVIDER=openai
OPENAI_API_KEY=
OPENAI_MODEL=gpt-4o-mini
PORT=8001
```

---

## Demo Seeding (`scripts/seed-demo.py`)

Used by the automated demo seeder to provision the canonical 10-candidate dataset.
**Never commit these values.**

| Variable | Required | Purpose | Example |
|---|---|---|---|
| `SUPABASE_URL` | Yes | Target Supabase project API URL | `https://abcdef.supabase.co` |
| `SUPABASE_SERVICE_ROLE_KEY` | Yes | Server-side key for Auth Admin and PostgREST | `eyJhbGci...` |
| `DEMO_USER_PASSWORD` | No | Local-only password for the two demo auth accounts (defaults to safe pattern) | `replace-with-local-demo-password` |

Usage:
```bash
export SUPABASE_URL="https://your-project.supabase.co"
export SUPABASE_SERVICE_ROLE_KEY="your-service-role-key"
export DEMO_USER_PASSWORD="replace-with-local-demo-password"
python scripts/seed-demo.py
```

