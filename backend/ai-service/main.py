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
