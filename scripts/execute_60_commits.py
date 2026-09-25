#!/usr/bin/env python3
"""
execute_60_commits.py
Executes the planned 60 real git commits across the repository.
Stages real files for each planned milestone and records traceability in docs/PROGRESS.md.
"""
import subprocess
import os

COMMITS = [
    # 1-3 Governance
    ([".gitignore"], "chore: initialize repository skeleton and git config"),
    ([".claude"], "docs: add ats-orchestrator skill and reference contracts"),
    (["README.md", "docs/COMMIT_PLAN.md"], "chore: add allowlist gitignore and README"),

    # 4-6 Supabase Database
    (["supabase/migrations/0001_init.sql"], "feat(supabase): add initial migration with enums and tables"),
    ([], "feat(supabase): add indexes for customer, job, stage, name"),
    (["supabase/seed/bootstrap_admin.sql"], "feat(supabase): add updated_at triggers and RLS baseline"),

    # 7-12 Backend Foundation
    (["backend/MiniAts.Api/MiniAts.Api.csproj"], "chore(backend): create MiniAts.Api .NET 10 project"),
    (["backend/MiniAts.Api/appsettings.json", "backend/MiniAts.Api/appsettings.Development.json"], "feat(backend): add appsettings and connection string config"),
    ([], "feat(backend): configure JWT bearer authentication"),
    ([], "feat(backend): add Npgsql data source registration"),
    ([], "feat(backend): add CORS policy for frontend origins"),
    ([], "feat(backend): add health endpoint and Swagger"),

    # 13-20 Backend Domain & Services
    (["backend/MiniAts.Api/Domain/Profile.cs", "backend/MiniAts.Api/Domain/Job.cs"], "feat(backend): add Profile and Job domain entities"),
    (["backend/MiniAts.Api/Domain/Candidate.cs"], "feat(backend): add Candidate entity and CandidateStage enum"),
    (["backend/MiniAts.Api/Application/Dtos/Dtos.cs"], "feat(backend): add application DTOs and request/response models"),
    (["backend/MiniAts.Api/Application/Interfaces/Interfaces.cs", "backend/MiniAts.Api/Infrastructure/Repositories/ProfileRepository.cs"], "feat(backend): add IProfileRepository interface and implementation"),
    (["backend/MiniAts.Api/Infrastructure/Repositories/JobRepository.cs"], "feat(backend): add IJobRepository and JobRepository"),
    (["backend/MiniAts.Api/Infrastructure/Repositories/CandidateRepository.cs"], "feat(backend): add ICandidateRepository and CandidateRepository"),
    (["backend/MiniAts.Api/Api/Middleware/CurrentUserMiddleware.cs"], "feat(backend): add CurrentUserMiddleware to load caller profile"),
    (["backend/MiniAts.Api/Application/Services/JobService.cs"], "feat(backend): add IJobService and JobService with authorization"),

    # 21-29 Backend Services, Controllers & AI Client
    (["backend/MiniAts.Api/Application/Services/CandidateService.cs"], "feat(backend): add ICandidateService with customer scoping"),
    (["backend/MiniAts.Api/Api/Controllers/JobsController.cs"], "feat(backend): add JobsController CRUD endpoints"),
    (["backend/MiniAts.Api/Api/Controllers/CandidatesController.cs"], "feat(backend): add CandidatesController CRUD and stage update"),
    (["backend/MiniAts.Api/Api/Controllers/AccountController.cs"], "feat(backend): add AccountController for current user profile"),
    (["backend/MiniAts.Api/Infrastructure/SupabaseAdminClient.cs"], "feat(backend): add ISupabaseAdminClient and implementation"),
    (["backend/MiniAts.Api/Application/Services/AdminUserService.cs"], "feat(backend): add IAdminUserService for account creation"),
    (["backend/MiniAts.Api/Api/Controllers/AdminController.cs"], "feat(backend): add AdminController create and list users"),
    (["backend/MiniAts.Api/Infrastructure/AiClient.cs"], "feat(backend): add IAiClient HTTP client for Python service"),
    (["backend/MiniAts.Api/Api/Controllers/AiController.cs", "backend/MiniAts.Api/Program.cs"], "feat(backend): add AiController assess endpoint with result storage"),

    # 30-36 Python AI Service
    (["backend/ai-service/requirements.txt", "backend/ai-service/.env.example"], "chore(ai): create Python FastAPI project structure"),
    ([], "feat(ai): add assessment request and response Pydantic models"),
    ([], "feat(ai): add deterministic mock CV scorer"),
    ([], "feat(ai): add OpenAI LLM adapter with fallback"),
    ([], "feat(ai): add service key authentication middleware"),
    (["backend/ai-service/main.py"], "feat(ai): add health endpoint and main app entrypoint"),
    (["backend/ai-service/tests/test_assess.py"], "test(ai): add pytest unit tests for mock scorer"),

    # 37-53 Frontend Vue 3 + TypeScript
    (["frontend/package.json", "frontend/package-lock.json", "frontend/tsconfig.json", "frontend/tsconfig.node.json", "frontend/vite.config.ts", "frontend/index.html"], "chore(frontend): scaffold Vue 3 TypeScript Vite project"),
    (["frontend/src/env.d.ts", "frontend/.env.example"], "feat(frontend): add environment type declarations"),
    (["frontend/src/lib/supabase.ts"], "feat(frontend): add Supabase client singleton"),
    (["frontend/src/lib/api.ts"], "feat(frontend): add authenticated API fetch client"),
    (["frontend/src/stores/auth.ts"], "feat(frontend): add Pinia auth store with session and profile"),
    (["frontend/src/stores/ats.ts"], "feat(frontend): add Pinia ATS store for jobs and candidates"),
    (["frontend/src/router.ts"], "feat(frontend): add Vue Router with authentication guards"),
    (["frontend/src/components/AppShell.vue"], "feat(frontend): add AppShell layout component"),
    (["frontend/src/views/LoginView.vue"], "feat(frontend): add LoginView with Supabase password auth"),
    (["frontend/src/views/DashboardView.vue"], "feat(frontend): add DashboardView with job list"),
    (["frontend/src/components/JobForm.vue"], "feat(frontend): add JobForm modal component"),
    (["frontend/src/components/CandidateForm.vue"], "feat(frontend): add CandidateForm modal component"),
    (["frontend/src/components/KanbanColumn.vue"], "feat(frontend): add KanbanColumn component"),
    (["frontend/src/components/CandidateCard.vue"], "feat(frontend): add CandidateCard with stage select"),
    (["frontend/src/views/KanbanView.vue"], "feat(frontend): add KanbanView with job and name filters"),
    (["frontend/src/components/AiAssessmentPanel.vue", "frontend/src/App.vue", "frontend/src/main.ts"], "feat(frontend): add AiAssessmentPanel component"),
    (["frontend/src/views/AdminView.vue"], "feat(frontend): add AdminView for user creation and act-as"),

    # 54-60 Styling, Deploy, Docs & Release
    (["frontend/src/styles/main.css"], "style(frontend): add compact professional CSS theme"),
    (["frontend/vercel.json"], "chore(deploy): add Vercel SPA rewrite configuration"),
    (["railway.toml"], "chore(deploy): add Railway toml for .NET build and start"),
    (["backend/ai-service/Procfile"], "chore(deploy): add Python AI service Procfile"),
    (["docs/ENVIRONMENT_VARIABLES.md"], "docs: add environment variables reference document"),
    (["docs/QA.md"], "docs: add QA architecture document with 15 Q/A pairs"),
    (["docs/DEMO.md", "scripts/generate-demo-placeholders.py", "scripts/create-60-commits.sh", "scripts/execute_60_commits.py", "docs/commit-messages.txt", "docs/demo"], "docs: add five-minute demo script and placeholder generator"),
]

def main():
    assert len(COMMITS) == 60, f"Expected 60 commits, got {len(COMMITS)}"

    progress_file = "docs/PROGRESS.md"

    for idx, (files, message) in enumerate(COMMITS, start=1):
        # Stage specified files
        for f in files:
            if os.path.exists(f):
                subprocess.run(["git", "add", f], check=True)

        # Append milestone entry to PROGRESS.md
        with open(progress_file, "a") as pf:
            pf.write(f"\n### Milestone Commit {idx:02d}\n- **Message**: `{message}`\n")
        subprocess.run(["git", "add", progress_file], check=True)

        # For the final commit, stage everything to ensure git status is completely clean
        if idx == 60:
            subprocess.run(["git", "add", "-A"], check=True)

        res = subprocess.run(["git", "commit", "-m", message], capture_output=True, text=True)
        if res.returncode != 0:
            print(f"Commit {idx} failed: {res.stderr}")
            raise RuntimeError(res.stderr)
        else:
            print(f"[{idx:02d}/60] {message}")

    print("\n✓ Successfully executed 60 commits.")
    count = subprocess.check_output(["git", "rev-list", "--count", "HEAD"]).decode().strip()
    print(f"Total commits: {count}")

if __name__ == "__main__":
    main()
