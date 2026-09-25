#!/usr/bin/env python3
"""
generate-demo-placeholders.py
Generates placeholder SVG images for the Mini ATS demo script.
Uses only the Python standard library (no pip install required).

Related: docs/DEMO.md (references these image files)
         docs/demo/images/ (output directory)
Run: python scripts/generate-demo-placeholders.py
"""

from pathlib import Path

# Output directory for demo images – matches docs/DEMO.md asset list.
out_dir = Path("docs/demo/images")
out_dir.mkdir(parents=True, exist_ok=True)

# Each tuple: (filename, slide title, subtitle description)
slides = [
    (
        "01-architecture.svg",
        "Architecture",
        "Vue 3 → .NET 10 API → Supabase Postgres + Python AI Service"
    ),
    (
        "02-login.svg",
        "Login",
        "Supabase Auth → JWT → .NET API validation"
    ),
    (
        "03-admin-create-user.svg",
        "Admin: Create User",
        "Backend calls Supabase Auth Admin API with service role key"
    ),
    (
        "04-job-form.svg",
        "Job Form",
        "Customer creates job posting scoped by customer_id"
    ),
    (
        "05-candidate-form.svg",
        "Candidate Form",
        "Full name · Email · LinkedIn URL · CV text · Stage"
    ),
    (
        "06-kanban-filtered.svg",
        "Kanban Board",
        "Filter by job · Search by name · Drag or select stage"
    ),
    (
        "07-ai-assessment.svg",
        "AI Assessment",
        "Score 0–100 · Summary · Strengths · Concerns · Questions · Provider: mock"
    ),
    (
        "08-deploy-checklist.svg",
        "Deploy Checklist",
        "Vercel frontend · Railway .NET API · Railway Python AI · Health checks"
    ),
]


def make_svg(title: str, subtitle: str) -> str:
    """Generate a dark-themed SVG placeholder for a demo slide."""
    # Escape XML special characters in title and subtitle.
    def esc(s: str) -> str:
        return s.replace("&", "&amp;").replace("<", "&lt;").replace(">", "&gt;")

    t = esc(title)
    s = esc(subtitle)

    return f"""<svg xmlns="http://www.w3.org/2000/svg" width="1280" height="720" viewBox="0 0 1280 720">
  <!-- Background -->
  <rect width="1280" height="720" fill="#0f172a"/>
  <!-- Card surface -->
  <rect x="80" y="60" width="1120" height="600" rx="20" fill="#1e293b" stroke="#334155" stroke-width="2"/>
  <!-- Accent bar -->
  <rect x="80" y="60" width="1120" height="6" rx="3" fill="#2563eb"/>

  <!-- Mini ATS brand -->
  <text x="120" y="130" fill="#94a3b8" font-family="Inter, Arial, sans-serif" font-size="18" font-weight="600">
    📋 Mini ATS – Prototype Demo
  </text>

  <!-- Slide title -->
  <text x="120" y="220" fill="#f8fafc" font-family="Inter, Arial, sans-serif" font-size="56" font-weight="700">
    {t}
  </text>

  <!-- Subtitle / description -->
  <text x="120" y="280" fill="#94a3b8" font-family="Inter, Arial, sans-serif" font-size="24">
    {s}
  </text>

  <!-- Divider -->
  <line x1="120" y1="320" x2="1160" y2="320" stroke="#334155" stroke-width="1"/>

  <!-- Stack tags -->
  <rect x="120" y="340" width="120" height="32" rx="6" fill="#1d4ed8" opacity="0.3"/>
  <text x="130" y="362" fill="#93c5fd" font-family="Inter, Arial, sans-serif" font-size="14" font-weight="600">Vue 3 + Vite</text>

  <rect x="256" y="340" width="140" height="32" rx="6" fill="#1d4ed8" opacity="0.3"/>
  <text x="266" y="362" fill="#93c5fd" font-family="Inter, Arial, sans-serif" font-size="14" font-weight="600">.NET 10 API</text>

  <rect x="410" y="340" width="160" height="32" rx="6" fill="#1d4ed8" opacity="0.3"/>
  <text x="420" y="362" fill="#93c5fd" font-family="Inter, Arial, sans-serif" font-size="14" font-weight="600">Python FastAPI</text>

  <rect x="586" y="340" width="160" height="32" rx="6" fill="#1d4ed8" opacity="0.3"/>
  <text x="596" y="362" fill="#93c5fd" font-family="Inter, Arial, sans-serif" font-size="14" font-weight="600">Supabase Auth</text>

  <rect x="762" y="340" width="120" height="32" rx="6" fill="#1d4ed8" opacity="0.3"/>
  <text x="772" y="362" fill="#93c5fd" font-family="Inter, Arial, sans-serif" font-size="14" font-weight="600">Vercel</text>

  <rect x="898" y="340" width="120" height="32" rx="6" fill="#1d4ed8" opacity="0.3"/>
  <text x="908" y="362" fill="#93c5fd" font-family="Inter, Arial, sans-serif" font-size="14" font-weight="600">Railway</text>

  <!-- Footer -->
  <text x="120" y="630" fill="#475569" font-family="Inter, Arial, sans-serif" font-size="16">
    Placeholder image for Mini ATS demo · Replace with real screenshot when app is running
  </text>
</svg>"""


def main() -> None:
    generated = 0
    for filename, title, subtitle in slides:
        path = out_dir / filename
        path.write_text(make_svg(title, subtitle), encoding="utf-8")
        print(f"  ✓ {path}")
        generated += 1

    print(f"\nGenerated {generated} placeholder SVG files in '{out_dir}'")
    print("Replace with real screenshots after running the app.")


if __name__ == "__main__":
    main()
