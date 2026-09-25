# Prompt 3 – AI Pipeline Report & Analysis Specification

- Button on Dashboard: "Generate AI Report" (variant=ai, Sparkles icon, loading state, AiGlowPanel styling)
- Secondary button: "Export as Markdown"
- Backend endpoint: `POST /api/ai/reports/pipeline` { customerId }
- Python service endpoint: `POST /analyze` with mock fallback
- Computes:
  - Funnel conversion % across all 6 stages
  - Candidate distribution per job & average AI score
  - AI score distribution histogram (0-19, 20-39, 40-59, 60-79, 80-100)
  - Stale candidates (> 7 days)
  - Data-quality coverage (LinkedIn URL %, CV %, short CV %, email %)
  - Top 3 and bottom 3 candidates
  - AI narrative: headline, score (0-100), rating (healthy/watch/risk), summary, strengths, risks, recommendations, stage insights
