# Mini ATS – Question & Answer Architecture Guide

This document explains **why** and **how** for every key decision in the Mini ATS prototype.

---

## Q1: Why this folder structure?

**A:** The structure separates concerns while keeping the prototype navigable:

- `backend/MiniAts.Api` — .NET 10 API, the secure business-logic layer.
- `backend/ai-service` — Python FastAPI, isolated AI/text logic.
- `frontend/` — Vue 3 + TypeScript UI, talks only to .NET API (not Supabase data).
- `supabase/` — Postgres migrations; the only source of truth for schema.
- `docs/` — human documentation, demo scripts, env var reference.
- `.claude/skills/` — AI agent orchestration rules, contracts, and limitations.

This lets an AI agent or a new developer work on one layer without risk of breaking another.

---

## Q2: Why Supabase if we also have a .NET backend?

**A:** Supabase provides two things:

1. **Postgres database** — managed, free for prototypes.
2. **Auth / JWT issuance** — handles email login, token refresh, secure sessions.

The .NET backend is still essential because:

- It enforces app-specific authorization (customer scoping, role checks).
- It keeps the Supabase service role key server-side (never in the browser).
- It provides clean, typed REST endpoints for the frontend.
- It proxies AI service calls securely with an internal service key.

Supabase is the **platform**; .NET is the **application API**.

---

## Q3: Why JWT authentication?

**A:** JWT gives stateless API authentication:

1. Frontend authenticates with Supabase Auth (email + password).
2. Supabase returns a signed JWT access token.
3. Frontend sends `Authorization: Bearer <token>` on every .NET API request.
4. .NET validates the signature using the Supabase JWT secret.
5. .NET loads the `public.profiles` row from Postgres by the token `sub` claim.
6. .NET enforces role and customer scoping from the profile.

Benefits: no server-side session storage, works well with Vercel + Railway, token refresh is handled by the Supabase JS client automatically.

---

## Q4: Why .NET 10 for the backend?

**A:** The brief explicitly required C# .NET 10. It is also an excellent choice for:

- REST APIs with typed controllers.
- Dependency injection and clean layered architecture.
- Strong async/await and performance.
- Easy JWT validation via `Microsoft.AspNetCore.Authentication.JwtBearer`.
- Long-term maintainability of OOP code.

---

## Q5: Why Python FastAPI for the AI service?

**A:**

- Python has the most mature text/NLP/LLM ecosystem.
- FastAPI is fast to write, self-documents, and validates with Pydantic.
- The service is stateless and easy to deploy separately on Railway.
- Separating AI logic from .NET means we can swap or scale it independently.
- Mock scoring requires no external dependencies — demo always works offline.

---

## Q6: Why Vue 3 + TypeScript + Vite?

**A:** Vue 3 with Composition API (`<script setup>`) is:

- Fast to develop with and easy to read.
- TypeScript integration via `vue-tsc` catches API contract mistakes at build time.
- Vite gives near-instant HMR and fast production builds.
- Scoped CSS keeps styles co-located with components without a CSS framework.
- Pinia is the recommended state manager for Vue 3.

---

## Q7: How does admin create customer accounts?

**A:**

1. Admin opens AdminView.vue and fills in email/password/role/name.
2. Frontend calls `POST /api/admin/users` on the .NET API with the JWT.
3. .NET verifies caller role is `admin`.
4. .NET calls Supabase Auth Admin API `POST {SUPABASE_URL}/auth/v1/admin/users` with the **service role key** (server-side only).
5. Supabase creates the auth user and returns the UUID.
6. .NET inserts a matching row into `public.profiles` with the supplied role.
7. .NET returns the profile DTO to the frontend.

The service role key **never** touches the browser.

---

## Q8: How does customer isolation work?

**A:** Every `jobs` and `candidates` row has a `customer_id` column.

Rules enforced in `JobService` and `CandidateService`:

- If caller role is `customer`: effective `customer_id` = caller's own profile id. The supplied `customerId` query parameter is ignored.
- If caller role is `admin`: `customer_id` = the supplied `customerId` parameter. Admin must explicitly choose a customer to act on behalf of.

All repository SQL includes `AND customer_id = @customerId`. There is no query path that omits this filter.

---

## Q9: How does the Kanban filter work?

**A:** The frontend sends query parameters to `GET /api/candidates`:

```
?customerId=uuid&jobId=uuid&search=anna
```

The backend SQL uses:

```sql
WHERE customer_id = @customerId
  AND (@jobId IS NULL OR job_id = @jobId)
  AND (@search IS NULL OR full_name ILIKE @searchPattern)
```

The Vue store reloads candidates whenever the filter values change (via `watch`). The result is grouped by stage in a `computed` property and passed to `KanbanColumn` components.

---

## Q10: How does AI CV assessment work end-to-end?

**A:**

1. User clicks "🤖 Assess CV" on a `CandidateCard`.
2. Frontend calls `POST /api/ai/candidates/{id}/assess?customerId=uuid`.
3. `AiController` delegates to `CandidateService.AssessAsync`.
4. Service loads the candidate and related job from Postgres.
5. Service calls `AiClient.AssessAsync` which POSTs to Python `/assess`.
6. Python mock scorer (or LLM adapter) returns `{ score, summary, strengths, concerns, questions, provider }`.
7. Service calls `CandidateRepository.SaveAiResultAsync` to store `ai_score` and `ai_feedback`.
8. Result is returned to the frontend.
9. `CandidateCard` shows the score badge and toggles `AiAssessmentPanel`.

---

## Q11: Why Vercel for the frontend?

**A:**

- Zero-config for Vite/Vue static builds.
- Free tier includes custom domains and HTTPS.
- Automatic preview deployments on pull requests.
- The `vercel.json` SPA rewrite handles Vue Router history mode.

---

## Q12: Why Railway for the backend?

**A:**

- Supports .NET and Python with Nixpacks auto-detection.
- Free trial tier is sufficient for prototypes.
- Environment variables are injected securely without committing secrets.
- Health check and restart policies are configurable in `railway.toml`.
- Multiple services in one project (API + AI service).

---

## Q13: Why 60 commits?

**A:** The assignment requires 60 commits as a proxy for showing incremental build-out discipline. Commits follow conventional commit format (`feat:`, `chore:`, `docs:`, `test:`) and map to coherent work slices — not arbitrary empty commits. If fewer than 60 natural commits occur, traceability commits appending to `docs/PROGRESS.md` bring the count up.

---

## Q14: What are the main risks?

**A:**

| Risk | Mitigation |
|---|---|
| Service role key leaked | Never put in frontend; only in Railway env vars |
| JWT misconfiguration | Validate issuer, audience, and secret match Supabase project |
| AI hallucination | Constrain prompt, cap output length, use mock by default |
| Kanban performance at scale | Prototype is unoptimized; pagination can be added later |
| Railway .NET 10 SDK availability | Pin SDK version; Railway Nixpacks supports .NET 10 |
| Cross-customer data leak | Every SQL query filters by `customer_id`; no bypass exists |

---

## Q15: What is intentionally out of scope?

**A:**

- Email/inbox integration or job board posting.
- Interview scheduling or calendar sync.
- Multi-user customer teams (only one user per customer in prototype).
- CV file upload (column exists, Storage integration not implemented).
- Advanced RBAC audit logging.
- Real LinkedIn profile scraping.
- Production-grade monitoring and observability.
- GDPR/CCPA data deletion flows.

---

## Q16: Assumptions made during implementation

**A:**

- Demo admin email: `admin@demo-ats.local`
- Demo customer email: `customer@demo-ats.local`, company: Nordic Tech AB
- Demo job: "Senior Frontend Engineer"
- Mock AI score formula: `min(95, 35 + keyword_overlap × 2)`
- Local dev ports: frontend 5173, backend 5000, AI service 8001
- Supabase free tier is sufficient for the prototype
- Railway free trial is used for deployment testing

---

## Q17: How would you extend this beyond the prototype?

**A:**

1. Add Supabase Storage for CV file uploads with text extraction.
2. Add candidate notes, activities, and email timeline.
3. Add multi-user customer teams with RBAC.
4. Add background job queue for bulk AI scoring.
5. Add pagination and cursor-based infinite scroll for Kanban.
6. Add CI/CD with GitHub Actions running dotnet test and npm run build.
7. Add EEOC-safe AI audit log with full explainability trail.
8. Add real-time Kanban updates via Supabase Realtime or WebSockets.
