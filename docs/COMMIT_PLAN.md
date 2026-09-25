# Mini ATS – 60-Commit Plan

## Verification

```bash
git rev-list --count HEAD
```

Expected: 60

## Commit groups

| Group | Range | Prefix | Purpose |
|---|---|---|---|
| Governance | 1–3 | `chore:` `docs:` | Skill, gitignore, README |
| Database | 4–6 | `feat(supabase):` | Migration, seed, indexes |
| Backend foundation | 7–12 | `chore(backend):` `feat(backend):` | Project, JWT, auth |
| Backend domain | 13–20 | `feat(backend):` | Entities, repos, services, controllers |
| Backend AI | 21–23 | `feat(backend):` `test(backend):` | AI proxy, tests |
| Python AI | 24–29 | `chore(ai):` `feat(ai):` `test(ai):` | FastAPI, mock, LLM, tests |
| Frontend foundation | 30–36 | `chore(frontend):` `feat(frontend):` | Scaffold, env, clients, stores, router |
| Frontend views | 37–47 | `feat(frontend):` | Login, dashboard, kanban, admin, AI UI |
| Styling | 48–49 | `style(frontend):` | CSS theme |
| Deploy | 50–53 | `chore(deploy):` | Vercel, Railway, env docs |
| Docs | 54–57 | `docs:` | QA, DEMO, placeholders |
| Validation | 58–59 | `test:` | Build check, smoke checklist |
| Release | 60 | `chore:` | Final prototype release notes |

## All 60 commit messages

```
01. chore: initialize repository skeleton and git config
02. docs: add ats-orchestrator skill and reference contracts
03. chore: add allowlist gitignore and README
04. feat(supabase): add initial migration with enums and tables
05. feat(supabase): add indexes for customer, job, stage, name
06. feat(supabase): add updated_at triggers and RLS baseline
07. chore(backend): create MiniAts.Api .NET 10 project
08. feat(backend): add appsettings and connection string config
09. feat(backend): configure JWT bearer authentication
10. feat(backend): add Npgsql data source registration
11. feat(backend): add CORS policy for frontend origins
12. feat(backend): add health endpoint and Swagger
13. feat(backend): add Profile and Job domain entities
14. feat(backend): add Candidate entity and CandidateStage enum
15. feat(backend): add application DTOs and request/response models
16. feat(backend): add IProfileRepository interface and implementation
17. feat(backend): add IJobRepository and JobRepository
18. feat(backend): add ICandidateRepository and CandidateRepository
19. feat(backend): add CurrentUserMiddleware to load caller profile
20. feat(backend): add IJobService and JobService with authorization
21. feat(backend): add ICandidateService with customer scoping
22. feat(backend): add JobsController CRUD endpoints
23. feat(backend): add CandidatesController CRUD and stage update
24. feat(backend): add AccountController for current user profile
25. feat(backend): add ISupabaseAdminClient and implementation
26. feat(backend): add IAdminUserService for account creation
27. feat(backend): add AdminController create and list users
28. feat(backend): add IAiClient HTTP client for Python service
29. feat(backend): add AiController assess endpoint with result storage
30. chore(ai): create Python FastAPI project structure
31. feat(ai): add assessment request and response Pydantic models
32. feat(ai): add deterministic mock CV scorer
33. feat(ai): add OpenAI LLM adapter with fallback
34. feat(ai): add service key authentication middleware
35. feat(ai): add health endpoint and main app entrypoint
36. test(ai): add pytest unit tests for mock scorer
37. chore(frontend): scaffold Vue 3 TypeScript Vite project
38. feat(frontend): add environment type declarations
39. feat(frontend): add Supabase client singleton
40. feat(frontend): add authenticated API fetch client
41. feat(frontend): add Pinia auth store with session and profile
42. feat(frontend): add Pinia ATS store for jobs and candidates
43. feat(frontend): add Vue Router with authentication guards
44. feat(frontend): add AppShell layout component
45. feat(frontend): add LoginView with Supabase password auth
46. feat(frontend): add DashboardView with job list
47. feat(frontend): add JobForm modal component
48. feat(frontend): add CandidateForm modal component
49. feat(frontend): add KanbanColumn component
50. feat(frontend): add CandidateCard with stage select
51. feat(frontend): add KanbanView with job and name filters
52. feat(frontend): add AiAssessmentPanel component
53. feat(frontend): add AdminView for user creation and act-as
54. style(frontend): add compact professional CSS theme
55. chore(deploy): add Vercel SPA rewrite configuration
56. chore(deploy): add Railway toml for .NET build and start
57. chore(deploy): add Python AI service Procfile
58. docs: add environment variables reference document
59. docs: add QA architecture document with 15 Q/A pairs
60. docs: add five-minute demo script and placeholder generator
```

## Fallback script

If fewer than 60 commits exist after implementation, run:

```bash
./scripts/create-60-commits.sh
```

This appends traceability entries to `docs/PROGRESS.md` and creates commits from `docs/commit-messages.txt`.

## Notes

- Prefer real incremental commits during implementation.
- Do not rewrite history to reach exactly 60 (no `git rebase -i`).
- If naturally more than 60 commits exist, document the count and explain.
