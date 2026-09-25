# Frontend Contract Reference

## Technology

- Vue 3 (Composition API + `<script setup>`)
- TypeScript
- Vite
- Plain/scoped CSS (no UI framework)
- Pinia stores
- Vue Router
- Supabase JS (`@supabase/supabase-js`) – auth only
- Custom `apiFetch` helper – all domain data

## Routes

| Path | Component | Guard |
|---|---|---|
| `/login` | LoginView | redirect to `/` if authenticated |
| `/` | DashboardView | requiresAuth |
| `/kanban` | KanbanView | requiresAuth |
| `/admin` | AdminView | requiresAuth + requiresAdmin |

## Stores

### Auth store (`stores/auth.ts`)

State:
- `session` – Supabase session object
- `profile` – `{ id, email, role, displayName, companyName }`
- `actAsCustomerId` – admin selects a customer to act on behalf of

Actions:
- `signIn(email, password)` → calls `supabase.auth.signInWithPassword`
- `signOut()` → calls `supabase.auth.signOut`, clears state, redirects to login
- `loadProfile()` → calls `GET /api/account/me`, stores result
- `setActAsCustomer(id)` → sets `actAsCustomerId`

### ATS store (`stores/ats.ts`)

State:
- `jobs: Job[]`
- `candidates: Candidate[]`
- `filters: { jobId, search }`
- `loading: boolean`
- `error: string | null`

Actions:
- `loadJobs(customerId)`
- `loadCandidates(customerId, jobId?, search?)`
- `createJob(dto)` → POST /api/jobs
- `createCandidate(dto)` → POST /api/candidates
- `updateCandidateStage(id, stage, customerId)` → PATCH /api/candidates/{id}/stage
- `assessCandidate(id, customerId)` → POST /api/ai/candidates/{id}/assess

## Key types

```ts
interface Profile {
  id: string
  email: string
  role: 'admin' | 'customer'
  displayName: string | null
  companyName: string | null
}

interface Job {
  id: string
  customerId: string
  title: string
  description: string | null
  status: string
  createdAt: string
}

interface Candidate {
  id: string
  customerId: string
  jobId: string | null
  fullName: string
  email: string | null
  linkedinUrl: string | null
  cvText: string | null
  summary: string | null
  stage: CandidateStage
  aiScore: number | null
  aiFeedback: AiFeedback | null
  createdAt: string
}

type CandidateStage = 'new' | 'screening' | 'interview' | 'offer' | 'hired' | 'rejected'

interface AiFeedback {
  score: number
  summary: string
  strengths: string[]
  concerns: string[]
  questions: string[]
  provider: 'mock' | 'llm'
}
```

## Components

| Component | Purpose |
|---|---|
| `AppShell.vue` | Layout wrapper: header, nav, role badge, logout |
| `LoginView.vue` | Supabase password login form |
| `DashboardView.vue` | Job list + create job button |
| `KanbanView.vue` | Kanban board with filters |
| `AdminView.vue` | Create user + list users + act-as customer |
| `JobForm.vue` | Create/edit job modal |
| `CandidateForm.vue` | Create/edit candidate modal |
| `KanbanColumn.vue` | Single stage column, receives candidates array |
| `CandidateCard.vue` | Compact card: name, email, LinkedIn, score, stage select |
| `AiAssessmentPanel.vue` | Shows AI result: score, summary, strengths, concerns, questions |

## API client rules

File: `src/lib/api.ts`

- Always reads session from `supabase.auth.getSession()`.
- Attaches `Authorization: Bearer <access_token>` if session exists.
- On 401 response: calls `supabase.auth.signOut()` and throws.
- Throws readable error using `message` field from backend JSON.

## Environment variables

```env
VITE_SUPABASE_URL=
VITE_SUPABASE_ANON_KEY=
VITE_API_BASE_URL=
```

Declared in `src/env.d.ts` for TypeScript completion.

## Style guidelines

- CSS variables in `src/styles/main.css` for colors, spacing, radius.
- Compact, professional light theme.
- Kanban board: horizontal scroll, columns 240px wide.
- Cards: white background, subtle border, 8px border-radius.
- Responsive enough for desktop demo (1280px viewport).
