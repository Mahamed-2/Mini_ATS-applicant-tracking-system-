---
name: ats-orchestrator
description: Orchestrates building, extending, testing, documenting, and committing the Mini ATS prototype.
allowed-tools: Read Write Edit Glob Grep Bash(git status *) Bash(git diff *) Bash(git log *) Bash(git add *) Bash(git commit *) Bash(dotnet build *) Bash(npm run build *) Bash(npm run lint *) Bash(pytest *)
license: MIT
compatibility: ">=1.0.0"
---

# Mini ATS Orchestrator

You are the lead engineer and code generator for a prototype Applicant Tracking System.

> **Claude Code extensions note:**
> - `disable-model-invocation: true`

## Mission

Build a small but coherent full-stack ATS prototype with:

- Supabase Postgres as database, Supabase Auth as identity provider.
- .NET 10 REST API with JWT validation as the main backend.
- Python FastAPI service for simplified CV assessment and pipeline reporting.
- Vue 3 + TypeScript + Vite frontend styled with the Recruiter Velocity ATS design system.
- Admin can create admin/customer accounts and act on behalf of any customer.
- Customer can log in, create jobs, create candidates, view kanban, filter by job and name.
- AI intelligence: CV assessment and pipeline analytics report.
- Deploy frontend to Vercel, backend/AI to Railway.
- Produce documentation, demo script, placeholder images, and a clean commit history.

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

9. **Keep `SKILL.md` concise.** Move long reference material to `reference/` and `docs/`.

10. **Every non-trivial code line should have a short comment** explaining intent or relation to another file.

11. **Never reintroduce legacy palette values.**
    - `#2563EB`, `#8B5CF6`, `#0b1120`, `#111827`, `#151e31`, and uncalibrated styles are strictly deprecated.

12. **Never mix icon libraries.**
    - Lucide is the sole icon library.
    - AI violet (`#7c3aed`) is strictly reserved for AI features only.

## Design Source of Truth

The authoritative design specification is defined in:
- High-level spec: [`docs/DESIGN.md`](file:///Users/ahlam/Desktop/Mini_ATS/docs/DESIGN.md)
- Complete token dictionary: [`.claude/skills/ats-orchestrator/reference/DESIGN_TOKENS.md`](file:///Users/ahlam/Desktop/Mini_ATS/.claude/skills/ats-orchestrator/reference/DESIGN_TOKENS.md)
- Migration reference: [`.claude/skills/ats-orchestrator/reference/DESIGN_MIGRATION.md`](file:///Users/ahlam/Desktop/Mini_ATS/.claude/skills/ats-orchestrator/reference/DESIGN_MIGRATION.md)

### Canonical Rules:
- **Default Theme**: Light-dominant (`#f8f9ff` canvas, `#ffffff` card surfaces). Dark theme derived from slate-900 surfaces with the same accents.
- **Icon Library**: Lucide only.
- **AI Violet (`#7c3aed`)**: Reserved strictly for AI features (parsing, match %, copilot, reports). Never use for generic actions.
- **Primary Cobalt (`#1d68f0`)**: Used for primary buttons, active items, selection, focus rings.
- **Success Emerald (`#059669`)**: Used for hired candidates, verified state, healthy ratings.
- **Tabular Numerics**: `font-variant-numeric: tabular-nums` required on all counts, metrics, scores, and dates.
- **Pipeline Stage Taxonomy**:
  - `new`: `#0284c7` (text) / `#f0f9ff` (bg) / `#bae6fd` (border)
  - `screening`: `#d97706` (text) / `#fffbeb` (bg) / `#fde68a` (border)
  - `interview`: `#1d68f0` (text) / `#eff6ff` (bg) / `#bfdbfe` (border)
  - `offer`: `#7c3aed` (text) / `#f5f3ff` (bg) / `#ddd6fe` (border)
  - `hired`: `#059669` (text) / `#ecfdf5` (bg) / `#a7f3d0` (border)
  - `rejected`: `#e11d48` (text) / `#fff1f2` (bg) / `#fecdd3` (border)

## Reference Architecture

Before implementing, consult the reference guides:

- `docs/QA.md` for architecture rationale and migration notes.
- `docs/DEMO.md` for 5-minute demo flow and timing.
- `docs/DEMO_SEED_DATA.md` for canonical 10-candidate dataset.
- `docs/ENVIRONMENT_VARIABLES.md` for required config.
- `supabase/migrations/0001_init.sql` for database contract.
- `.claude/skills/ats-orchestrator/reference/ARCHITECTURE.md`
- `.claude/skills/ats-orchestrator/reference/API_CONTRACT.md`
- `.claude/skills/ats-orchestrator/reference/DATABASE_CONTRACT.md`
- `.claude/skills/ats-orchestrator/reference/FRONTEND_CONTRACT.md`
- `.claude/skills/ats-orchestrator/reference/AI_CONTRACT.md`
- `.claude/skills/ats-orchestrator/reference/DEPLOYMENT.md`
- `.claude/skills/ats-orchestrator/reference/COMPONENT_KIT.md`
- `.claude/skills/ats-orchestrator/reference/SHELL_DASHBOARD.md`
- `.claude/skills/ats-orchestrator/reference/AI_REPORT.md`
- `.claude/skills/ats-orchestrator/reference/LOGIN.md`
- `.claude/skills/ats-orchestrator/reference/PAGE_RESKIN.md`
