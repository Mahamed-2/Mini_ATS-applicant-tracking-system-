# Deployment Contract Reference

## Frontend → Vercel

### Configuration: `frontend/vercel.json`

```json
{
  "rewrites": [{ "source": "/(.*)", "destination": "/index.html" }]
}
```

### Vercel environment variables

Set these in Vercel dashboard → Settings → Environment Variables:

```env
VITE_SUPABASE_URL=https://YOUR_PROJECT.supabase.co
VITE_SUPABASE_ANON_KEY=YOUR_ANON_KEY
VITE_API_BASE_URL=https://YOUR_RAILWAY_API.up.railway.app
```

### Vercel deploy steps

1. Connect GitHub repository to Vercel.
2. Set root directory to `frontend/`.
3. Build command: `npm run build`
4. Output directory: `dist`
5. Add environment variables above.
6. Deploy.

## Backend .NET API → Railway

### Configuration: `railway.toml`

```toml
[build]
builder = "NIXPACKS"
buildCommand = "dotnet publish backend/MiniAts.Api/MiniAts.Api.csproj -c Release -o out"

[deploy]
startCommand = "out/MiniAts.Api"
healthcheckPath = "/health"
restartPolicyType = "ON_FAILURE"
restartPolicyMaxRetries = 10
```

### Railway .NET environment variables

```env
ASPNETCORE_URLS=http://0.0.0.0:$PORT
ConnectionStrings__Supabase=Host=...;Port=5432;Database=postgres;Username=postgres;Password=...;SSL Mode=Require
Supabase__Url=https://YOUR_PROJECT.supabase.co
Supabase__JwtSecret=YOUR_JWT_SECRET
Supabase__ServiceRoleKey=YOUR_SERVICE_ROLE_KEY
AiService__BaseUrl=https://YOUR_AI_SERVICE.up.railway.app
AiService__ApiKey=SHARED_SECRET
Cors__AllowedOrigins=https://YOUR_VERCEL_APP.vercel.app
```

### Railway .NET deploy steps

1. Create new Railway project.
2. Add service from GitHub repository.
3. Set environment variables above.
4. Railway auto-detects `railway.toml` and builds.
5. Note the generated Railway domain for `VITE_API_BASE_URL`.

## AI Service Python → Railway

### Configuration: `backend/ai-service/Procfile`

```
web: uvicorn main:app --host 0.0.0.0 --port ${PORT:-8001}
```

### Railway Python environment variables

```env
PORT=8001
AI_SERVICE_API_KEY=SHARED_SECRET
AI_PROVIDER=openai
OPENAI_API_KEY=optional
OPENAI_MODEL=gpt-4o-mini
```

### Railway Python deploy steps

1. Add second service to the Railway project.
2. Set root directory to `backend/ai-service/`.
3. Railway detects `Procfile` and installs `requirements.txt`.
4. Add environment variables above.
5. Note the generated domain for `AiService__BaseUrl` in the .NET service.

## Health checks

| Service | Endpoint |
|---|---|
| .NET API | GET /health → `{ "status": "ok" }` |
| Python AI | GET /health → `{ "status": "ok" }` |

## Local development

```bash
# .NET API
dotnet run --project backend/MiniAts.Api/MiniAts.Api.csproj

# Python AI service
cd backend/ai-service
pip install -r requirements.txt
uvicorn main:app --reload --port 8001

# Frontend
cd frontend
npm install
npm run dev
```

## Secret management

| Secret | Location |
|---|---|
| Supabase anon key | Vercel env (public-safe) |
| Supabase JWT secret | Railway env (.NET only) |
| Supabase service role key | Railway env (.NET only) |
| AI service API key | Railway env (both .NET and Python, same shared value) |
| Postgres connection string | Railway env (.NET only) |

Never commit any of the above. Use `.env.example` files only.
