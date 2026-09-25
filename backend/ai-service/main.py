# Mini ATS Python AI assessment service.
# Related: backend/MiniAts.Api/Infrastructure/AiClient.cs (caller)
#          .claude/skills/ats-orchestrator/reference/AI_CONTRACT.md
#          appsettings.json AiService:BaseUrl, AiService:ApiKey
#          ENV: AI_SERVICE_API_KEY, AI_PROVIDER, OPENAI_API_KEY, OPENAI_MODEL

# Enable PEP 604 union syntax (X | Y) on Python 3.9 via postponed evaluation.
from __future__ import annotations

# FastAPI provides the HTTP service layer.
from fastapi import FastAPI, Header, HTTPException
from fastapi.middleware.cors import CORSMiddleware

# Pydantic validates request and response shapes.
from pydantic import BaseModel, Field

# Standard library imports for scoring, environment, and HTTP.
import os
from typing import Optional
import json
import re

# httpx handles async HTTP calls to optional LLM APIs.
import httpx

# ── App setup ─────────────────────────────────────────────────────────────────

app = FastAPI(
    title="Mini ATS AI Service",
    version="0.1.0",
    description="Candidate CV assessment service for Mini ATS. Uses mock scoring by default, optional LLM."
)

# Allow the .NET backend to call this service (CORS for Railway internal calls).
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],  # restrict in production to .NET service IP
    allow_methods=["GET", "POST"],
    allow_headers=["*"],
)

# ── Models ────────────────────────────────────────────────────────────────────

class AssessRequest(BaseModel):
    """Candidate and job context sent by the .NET AiClient."""
    candidate_name:  str = Field(default="", max_length=200)
    job_title:       str = Field(default="", max_length=200)
    job_description: str = Field(default="", max_length=5000)
    cv_text:         str = Field(default="", max_length=20000)
    linkedin_url:    str = Field(default="", max_length=500)
    summary:         str = Field(default="", max_length=2000)


class AssessResponse(BaseModel):
    """Structured assessment returned to the .NET backend and stored in ai_feedback."""
    score:     int = Field(ge=0, le=100)
    summary:   str
    strengths: list[str]
    concerns:  list[str]
    questions: list[str]
    provider:  str  # "mock" or "llm"


class StageInsight(BaseModel):
    """Insight row for an individual pipeline stage."""
    stage:   str
    insight: str


class PipelineAggregateInput(BaseModel):
    """Pre-computed aggregations sent by the .NET backend."""
    total_candidates:       int = 0
    total_jobs:             int = 0
    stage_counts:           dict[str, int] = Field(default_factory=dict)
    conversion_rates:       dict[str, float] = Field(default_factory=dict)
    hire_rate:              float = 0.0
    avg_ai_score:           float = 0.0
    score_distribution:     dict[str, int] = Field(default_factory=dict)
    stale_count:            int = 0
    stale_candidate_names:  list[str] = Field(default_factory=list)
    coverage:               dict[str, float] = Field(default_factory=dict)
    outliers:               list[str] = Field(default_factory=list)
    top_candidate_names:    list[str] = Field(default_factory=list)
    bottom_candidate_names: list[str] = Field(default_factory=list)


class CandidateSummaryItem(BaseModel):
    """Candidate snapshot for pipeline narrative context."""
    name:          str = ""
    stage:         str = ""
    ai_score:      Optional[int] = None
    has_linkedin:  bool = False
    has_cv:        bool = False
    cv_length:     int = 0
    days_in_stage: int = 0


class JobSummaryItem(BaseModel):
    """Job snapshot for pipeline narrative context."""
    id:              str = ""
    title:           str = ""
    candidate_count: int = 0
    avg_ai_score:    float = 0.0


class PipelineAnalyzeRequest(BaseModel):
    """Aggregates and context sent by .NET for pipeline analysis."""
    customer_id:  str = ""
    company_name: str = ""
    aggregates:   PipelineAggregateInput = Field(default_factory=PipelineAggregateInput)
    jobs:         list[JobSummaryItem] = Field(default_factory=list)
    candidates:   list[CandidateSummaryItem] = Field(default_factory=list)


class PipelineAnalyzeResponse(BaseModel):
    """Executive narrative and recommendations returned to .NET and frontend."""
    headline:        str
    score:           int = Field(ge=0, le=100)
    rating:          str  # "healthy" | "watch" | "risk"
    summary:         str
    strengths:       list[str]
    risks:           list[str]
    recommendations: list[str]
    stage_insights:  list[StageInsight]
    provider:        str  # "mock" or "llm"


# ── Health ─────────────────────────────────────────────────────────────────────

@app.get("/health")
async def health() -> dict[str, str]:
    """Health probe for Railway deployment checks."""
    return {"status": "ok"}


# ── Auth helper ───────────────────────────────────────────────────────────────

def require_service_key(provided_key: str | None) -> None:
    """Validate the internal service key if AI_SERVICE_API_KEY env var is set."""
    expected = os.getenv("AI_SERVICE_API_KEY")
    # If no key is configured, allow all calls (dev/local mode).
    if expected and provided_key != expected:
        raise HTTPException(status_code=401, detail="Invalid AI service key.")


# ── LLM adapter ───────────────────────────────────────────────────────────────

def _llm_enabled() -> bool:
    """Return True if an LLM API key is present in the environment."""
    return bool(os.getenv("OPENAI_API_KEY"))


def _build_prompt(req: AssessRequest) -> str:
    """
    Build a conservative hiring-safe prompt for LLM assessment.
    Does not include candidate name to reduce potential bias signals.
    Truncates cv_text to 4000 chars to stay within token limits.
    """
    cv_text = req.cv_text[:4000] if req.cv_text else ""
    return (
        "You are an assistant helping recruiters prepare objective, hiring-safe candidate assessments. "
        "Do NOT infer age, gender, ethnicity, nationality, religion, disability, or family status. "
        "Use ONLY the information provided. "
        "Return a JSON object with these keys: "
        "score (integer 0-100), summary (string ≤150 words), "
        "strengths (array of ≤5 short strings), concerns (array of ≤5 short strings), "
        "questions (array of ≤5 interview questions). "
        "Be concise and explainable.\n\n"
        f"Job title: {req.job_title}\n"
        f"Job description: {req.job_description}\n"
        f"LinkedIn URL available: {'yes' if req.linkedin_url else 'no'}\n"
        f"CV / profile text: {cv_text}\n"
        f"Profile summary: {req.summary}"
    )


async def _assess_with_llm(req: AssessRequest) -> AssessResponse | None:
    """
    Call OpenAI chat completions with a JSON-mode request.
    Returns None on any failure so the caller can fall back to mock scoring.
    """
    api_key = os.getenv("OPENAI_API_KEY")
    if not api_key:
        return None  # LLM not configured

    model = os.getenv("OPENAI_MODEL", "gpt-4o-mini")
    url = "https://api.openai.com/v1/chat/completions"

    headers = {
        "Authorization": f"Bearer {api_key}",
        "Content-Type": "application/json",
    }
    payload = {
        "model": model,
        "messages": [{"role": "user", "content": _build_prompt(req)}],
        "temperature": 0.2,
        "response_format": {"type": "json_object"},  # ensures valid JSON back
    }

    try:
        async with httpx.AsyncClient(timeout=30.0) as client:
            res = await client.post(url, headers=headers, json=payload)
            res.raise_for_status()

        # Extract the JSON content from the first choice.
        content = res.json()["choices"][0]["message"]["content"]
        data = json.loads(content)

        return AssessResponse(
            score=int(data.get("score", 0)),
            summary=str(data.get("summary", "")),
            strengths=[str(x) for x in data.get("strengths", [])[:5]],
            concerns=[str(x) for x in data.get("concerns", [])[:5]],
            questions=[str(x) for x in data.get("questions", [])[:5]],
            provider="llm",
        )
    except Exception:
        # Any failure (network, parse, rate limit) triggers mock fallback.
        return None


# ── Mock scorer ───────────────────────────────────────────────────────────────

def _assess_with_mock(req: AssessRequest) -> AssessResponse:
    """
    Deterministic mock scorer using keyword overlap between job description and CV.
    Used when no LLM key is configured or when the LLM call fails.
    Algorithm (see AI_CONTRACT.md for full spec):
      1. Extract 3+ char words from job description and CV+summary text.
      2. Count overlap between the two word sets.
      3. score = min(95, 35 + overlap × 2).
      4. Generate strengths, concerns, and generic questions based on signals.
    """
    # Extract meaningful words (≥3 chars) from job and candidate text.
    job_words = set(re.findall(r"[a-zåäö]{3,}", req.job_description.lower()))
    cv_combined = (req.cv_text + " " + req.summary).lower()
    cv_words = set(re.findall(r"[a-zåäö]{3,}", cv_combined))

    # Keyword overlap drives the base score.
    overlap = len(job_words & cv_words)
    score = min(95, 35 + overlap * 2)

    strengths: list[str] = []
    concerns: list[str] = []

    # LinkedIn presence is a positive signal for verification.
    if req.linkedin_url:
        strengths.append("LinkedIn profile link available for verification.")
    else:
        concerns.append("No LinkedIn URL provided; profile harder to verify.")

    # CV text length signals how much information is available.
    if len(req.cv_text) > 500:
        strengths.append("CV text is detailed enough for initial screening.")
    else:
        concerns.append("CV text is short or missing; limited information for assessment.")

    # Keyword overlap quality signal.
    if overlap > 10:
        strengths.append(f"Strong keyword overlap ({overlap} matches) with job description.")
    elif overlap > 4:
        strengths.append(f"Moderate keyword overlap ({overlap} matches) detected.")
    else:
        concerns.append("Limited observable keyword match with the job requirements.")

    # Summary presence is a positive presentation signal.
    if req.summary:
        strengths.append("Profile summary provided.")

    # Generic hiring-safe interview questions (not derived from personal info).
    questions = [
        "Which tools or frameworks have you used most recently, and how did you apply them?",
        "Can you describe one measurable outcome or achievement from your most recent role?",
        "Why are you interested in this type of position at this stage of your career?",
        "How do you stay current with developments in your field?",
        "Describe a time you had to adapt your approach due to unexpected changes in a project.",
    ]

    return AssessResponse(
        score=score,
        summary=(
            f"Mock assessment based on keyword overlap ({overlap} matches) "
            "and profile completeness. Score is indicative only."
        ),
        strengths=strengths[:5],
        concerns=concerns[:5],
        questions=questions[:5],
        provider="mock",
    )


# ── Main endpoint ─────────────────────────────────────────────────────────────

@app.post("/assess", response_model=AssessResponse)
async def assess(
    req: AssessRequest,
    x_ai_service_key: Optional[str] = Header(default=None),
) -> AssessResponse:
    """
    Assess a candidate against a job description.
    Called by MiniAts.Api Infrastructure/AiClient.cs.
    Falls back to mock scoring if LLM is not configured or fails.
    """
    # Validate internal service key; protects against unauthorized calls.
    require_service_key(x_ai_service_key)

    # Attempt LLM assessment if a provider key is available.
    if _llm_enabled():
        llm_result = await _assess_with_llm(req)
        if llm_result:
            return llm_result

    # Deterministic mock fallback – always succeeds.
    return _assess_with_mock(req)


# ── Pipeline Analysis generator ───────────────────────────────────────────────

def _analyze_with_mock(req: PipelineAnalyzeRequest) -> PipelineAnalyzeResponse:
    """
    Deterministic mock pipeline analyzer that turns pre-computed aggregates
    into an executive narrative, health score, strengths, risks, and recommendations.
    Always hiring-safe: does not infer or evaluate any protected characteristics.
    """
    aggs = req.aggregates
    total = aggs.total_candidates
    stale = aggs.stale_count
    hire_rate = aggs.hire_rate
    avg_score = aggs.avg_ai_score
    stage_counts = aggs.stage_counts or {}
    coverage = aggs.coverage or {}

    if total == 0:
        return PipelineAnalyzeResponse(
            headline="No candidate activity detected in current pipeline.",
            score=50,
            rating="watch",
            summary=(
                "The active workspace currently has no candidates registered. "
                "Add applicants or publish open roles to initiate automated pipeline health monitoring."
            ),
            strengths=[
                "Clean pipeline architecture ready for inbound applicant tracking.",
                "Multi-stage Kanban framework configured for standard ATS workflow.",
            ],
            risks=[
                "Zero active candidate velocity across open positions.",
            ],
            recommendations=[
                "Publish job descriptions and source initial candidates into the New stage.",
                "Verify automated scoring criteria once candidate profiles are uploaded.",
            ],
            stage_insights=[
                StageInsight(stage="new", insight="Awaiting initial applicant submissions."),
                StageInsight(stage="screening", insight="No candidates currently in technical screening."),
                StageInsight(stage="interview", insight="Interview pipeline empty."),
                StageInsight(stage="offer", insight="No offers pending."),
                StageInsight(stage="hired", insight="Zero hires recorded in this reporting period."),
                StageInsight(stage="rejected", insight="No archived candidate records."),
            ],
            provider="mock",
        )

    # Calculate overall health score (base 70, adjusted by real indicators).
    health_score = 70

    if hire_rate >= 15.0:
        health_score += 10
    elif hire_rate >= 8.0:
        health_score += 5
    elif stage_counts.get("hired", 0) == 0 and total >= 8:
        health_score -= 5

    if stale == 0:
        health_score += 5
    elif stale >= 3:
        health_score -= 10
    else:
        health_score -= (stale * 3)

    linkedin_pct = coverage.get("linkedin_pct", 0.0)
    if linkedin_pct >= 80.0:
        health_score += 5
    elif linkedin_pct < 50.0:
        health_score -= 6

    if avg_score >= 70.0:
        health_score += 6
    elif avg_score < 50.0 and avg_score > 0:
        health_score -= 6

    if aggs.outliers:
        health_score -= min(6, len(aggs.outliers) * 2)

    # Clamp health score into 20-96 range.
    score = max(20, min(96, health_score))

    if score >= 75:
        rating = "healthy"
    elif score >= 55:
        rating = "watch"
    else:
        rating = "risk"

    # Executive headline
    if rating == "healthy":
        headline = (
            f"Strong talent velocity with {total} candidates and {hire_rate:.1f}% "
            f"hire conversion across active requisitions."
        )
    elif rating == "watch":
        headline = (
            f"Active candidate volume ({total} profiles), but {stale} stagnant "
            f"record(s) require recruiter attention."
        )
    else:
        headline = (
            f"Pipeline bottlenecks detected: slow stage progression across {total} "
            f"profiles requires immediate action."
        )

    # Executive summary
    company_label = f" for {req.company_name}" if req.company_name else ""
    summary = (
        f"Pipeline intelligence analysis{company_label} evaluates {total} candidate profiles "
        f"across {aggs.total_jobs} active role(s). Overall pipeline health is indexed at {score}/100 ({rating.upper()}). "
        f"Applicant pool averages {avg_score:.1f}% role alignment with {linkedin_pct:.0f}% verified LinkedIn coverage. "
        f"Currently, {stage_counts.get('hired', 0)} candidate(s) are hired and {stage_counts.get('interview', 0)} "
        f"are actively engaged in technical interviews."
    )

    # Strengths
    strengths: list[str] = []
    if linkedin_pct >= 70:
        strengths.append(f"High profile completeness: {linkedin_pct:.0f}% of candidates provide verifiable LinkedIn profiles.")
    if avg_score >= 65:
        strengths.append(f"Strong qualification benchmark with an overall average AI match score of {avg_score:.1f}%.")
    if stage_counts.get("interview", 0) > 0:
        strengths.append(f"Active engagement velocity with {stage_counts.get('interview', 0)} candidate(s) in technical interviews.")
    if stage_counts.get("hired", 0) > 0:
        strengths.append(f"End-to-end recruitment conversion demonstrated with {stage_counts.get('hired', 0)} verified hire(s).")
    if len(strengths) < 2:
        strengths.append("Structured stage taxonomy provides clear candidate lifecycle visibility.")
        strengths.append("Standardized scorecard evaluation active across candidate roster.")

    # Risks
    risks: list[str] = []
    if stale > 0:
        stale_names = ", ".join(aggs.stale_candidate_names[:2]) if aggs.stale_candidate_names else f"{stale} candidates"
        risks.append(f"{stale} candidate(s) remain untouched in stage for >7 days ({stale_names}).")
    if coverage.get("short_cv_pct", 0) > 15:
        risks.append(f"Brief or missing CV text on {coverage.get('short_cv_pct', 0):.0f}% of applicants limits deep semantic parsing.")
    for outlier in aggs.outliers[:2]:
        risks.append(outlier)
    if stage_counts.get("new", 0) >= 3:
        risks.append(f"Triage bottleneck: {stage_counts.get('new', 0)} inbound applicants waiting in 'New' stage.")
    if not risks:
        risks.append("No critical pipeline blockers identified; monitor time-to-hire velocity.")

    # Recommendations
    recommendations: list[str] = []
    if stale > 0:
        names = ", ".join(aggs.stale_candidate_names[:2]) if aggs.stale_candidate_names else "stale candidates"
        recommendations.append(f"Triage and advance or archive stagnant profiles ({names}) to prevent candidate drop-off.")
    if aggs.top_candidate_names:
        top_str = ", ".join(aggs.top_candidate_names[:2])
        recommendations.append(f"Fast-track highest AI match candidates ({top_str}) into next-stage stakeholder interviews.")
    if linkedin_pct < 80:
        recommendations.append("Prompt candidates with missing LinkedIn URLs or short CVs to provide updated profile links.")
    recommendations.append("Review interview stage feedback promptly to ensure continuous candidate momentum.")

    # Stage insights
    stage_insights = [
        StageInsight(
            stage="new",
            insight=f"{stage_counts.get('new', 0)} inbound candidate(s) awaiting initial recruiter review and triaging.",
        ),
        StageInsight(
            stage="screening",
            insight=f"{stage_counts.get('screening', 0)} candidate(s) in active profile and resume evaluation.",
        ),
        StageInsight(
            stage="interview",
            insight=f"{stage_counts.get('interview', 0)} candidate(s) currently advancing in team interviews.",
        ),
        StageInsight(
            stage="offer",
            insight=f"{stage_counts.get('offer', 0)} candidate(s) at or approaching formal offer negotiation.",
        ),
        StageInsight(
            stage="hired",
            insight=f"{stage_counts.get('hired', 0)} placement(s) successfully finalized ({hire_rate:.1f}% total conversion).",
        ),
        StageInsight(
            stage="rejected",
            insight=f"{stage_counts.get('rejected', 0)} candidate(s) archived with feedback recorded.",
        ),
    ]

    return PipelineAnalyzeResponse(
        headline=headline,
        score=score,
        rating=rating,
        summary=summary,
        strengths=strengths[:4],
        risks=risks[:4],
        recommendations=recommendations[:4],
        stage_insights=stage_insights,
        provider="mock",
    )


async def _analyze_with_llm(req: PipelineAnalyzeRequest) -> PipelineAnalyzeResponse | None:
    """
    Call OpenAI to produce an executive pipeline narrative from structured aggregations.
    Falls back cleanly to mock on any failure.
    """
    api_key = os.getenv("OPENAI_API_KEY")
    if not api_key:
        return None

    model = os.getenv("OPENAI_MODEL", "gpt-4o-mini")
    url = "https://api.openai.com/v1/chat/completions"

    headers = {
        "Authorization": f"Bearer {api_key}",
        "Content-Type": "application/json",
    }

    aggs_json = req.aggregates.model_dump_json()
    prompt = (
        "You are an executive talent analytics copilot for modern recruiting teams. "
        "Analyze the following recruitment pipeline aggregations and generate an executive report. "
        "Strictly adhere to objective, hiring-safe evaluation. Do NOT infer protected personal attributes. "
        "Return a valid JSON object matching this schema:\n"
        "{\n"
        '  "headline": string (one concise executive summary sentence),\n'
        '  "score": integer (0 to 100 overall pipeline health index),\n'
        '  "rating": "healthy" | "watch" | "risk",\n'
        '  "summary": string (2-4 sentences explaining current status, velocity, and quality),\n'
        '  "strengths": [string] (2-4 concrete positive signals),\n'
        '  "risks": [string] (2-4 potential bottlenecks or stale items),\n'
        '  "recommendations": [string] (2-4 actionable recruiter next steps),\n'
        '  "stage_insights": [ { "stage": string, "insight": string } ] (6 items for new, screening, interview, offer, hired, rejected)\n'
        "}\n\n"
        f"Aggregates: {aggs_json}\n"
        f"Company: {req.company_name}\n"
    )

    payload = {
        "model": model,
        "messages": [{"role": "user", "content": prompt}],
        "temperature": 0.2,
        "response_format": {"type": "json_object"},
    }

    try:
        async with httpx.AsyncClient(timeout=30.0) as client:
            res = await client.post(url, headers=headers, json=payload)
            res.raise_for_status()

        content = res.json()["choices"][0]["message"]["content"]
        data = json.loads(content)

        return PipelineAnalyzeResponse(
            headline=str(data.get("headline", "")),
            score=int(data.get("score", 70)),
            rating=str(data.get("rating", "healthy")),
            summary=str(data.get("summary", "")),
            strengths=[str(x) for x in data.get("strengths", [])[:4]],
            risks=[str(x) for x in data.get("risks", [])[:4]],
            recommendations=[str(x) for x in data.get("recommendations", [])[:4]],
            stage_insights=[
                StageInsight(stage=str(si.get("stage", "")), insight=str(si.get("insight", "")))
                for si in data.get("stage_insights", [])
            ],
            provider="llm",
        )
    except Exception:
        return None


# ── Pipeline Report Endpoint ──────────────────────────────────────────────────

@app.post("/analyze", response_model=PipelineAnalyzeResponse)
async def analyze_pipeline(
    req: PipelineAnalyzeRequest,
    x_ai_service_key: Optional[str] = Header(default=None),
) -> PipelineAnalyzeResponse:
    """
    Generate an AI pipeline intelligence report from aggregate data.
    Called by MiniAts.Api Infrastructure/AiClient.cs.
    Falls back to deterministic mock generator if LLM is not configured or fails.
    """
    # Validate internal service key; protects against unauthorized calls.
    require_service_key(x_ai_service_key)

    # Attempt LLM generation if configured.
    if _llm_enabled():
        llm_result = await _analyze_with_llm(req)
        if llm_result:
            return llm_result

    # Deterministic mock fallback – always succeeds.
    return _analyze_with_mock(req)
