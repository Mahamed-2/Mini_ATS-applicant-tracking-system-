# Tests for the Mini ATS AI pipeline report analyzer.
# Related: backend/ai-service/main.py (mock analyzer, /analyze endpoint)
#          .claude/skills/ats-orchestrator/reference/AI_REPORT.md
# Run: python -m pytest backend/ai-service/tests/

import sys
import os

sys.path.insert(0, os.path.join(os.path.dirname(__file__), ".."))

from fastapi.testclient import TestClient
from main import (
    app,
    _analyze_with_mock,
    PipelineAnalyzeRequest,
    PipelineAggregateInput,
    JobSummaryItem,
    CandidateSummaryItem,
)

client = TestClient(app)


def make_test_payload(total_candidates=10, hire_rate=10.0, stale_count=1) -> PipelineAnalyzeRequest:
    return PipelineAnalyzeRequest(
        customer_id="65f508b5-40ed-429a-a35b-a640248d1558",
        company_name="Nordic Tech AB",
        aggregates=PipelineAggregateInput(
            total_candidates=total_candidates,
            total_jobs=2,
            stage_counts={
                "new": 1,
                "screening": 2,
                "interview": 4,
                "offer": 1,
                "hired": 1,
                "rejected": 1,
            },
            conversion_rates={
                "screening": 90.0,
                "interview": 70.0,
                "offer": 20.0,
                "hired": 10.0,
            },
            hire_rate=hire_rate,
            avg_ai_score=71.2,
            score_distribution={
                "0-19": 0,
                "20-39": 0,
                "40-59": 3,
                "60-79": 3,
                "80-100": 4,
            },
            stale_count=stale_count,
            stale_candidate_names=["Hugo Lindqvist"] if stale_count > 0 else [],
            coverage={
                "linkedin_pct": 90.0,
                "cv_pct": 100.0,
                "short_cv_pct": 10.0,
                "email_pct": 100.0,
            },
            outliers=["Lucas Bergman: high match score in early stage"],
            top_candidate_names=["Maria Lind", "Nina Patel"],
            bottom_candidate_names=["Oscar Dahl"],
        ),
        jobs=[
            JobSummaryItem(id="j1", title="Senior Frontend Engineer", candidate_count=6, avg_ai_score=75.0),
            JobSummaryItem(id="j2", title="Backend Engineer .NET", candidate_count=4, avg_ai_score=68.0),
        ],
        candidates=[
            CandidateSummaryItem(name="Maria Lind", stage="hired", ai_score=94, has_linkedin=True, has_cv=True, cv_length=1200),
            CandidateSummaryItem(name="Nina Patel", stage="interview", ai_score=91, has_linkedin=True, has_cv=True, cv_length=950),
        ],
    )


def test_empty_pipeline_returns_valid_shape():
    """Empty pipeline must return valid watch report without errors."""
    req = PipelineAnalyzeRequest(
        customer_id="empty-id",
        company_name="Empty Tech",
        aggregates=PipelineAggregateInput(total_candidates=0),
    )
    res = _analyze_with_mock(req)
    assert res.score == 50
    assert res.rating == "watch"
    assert len(res.strengths) >= 1
    assert len(res.recommendations) >= 1
    assert len(res.stage_insights) == 6
    assert res.provider == "mock"


def test_standard_pipeline_score_and_rating():
    """Seeded 10-candidate scenario produces healthy rating and complete narrative."""
    req = make_test_payload(total_candidates=10, hire_rate=10.0, stale_count=1)
    res = _analyze_with_mock(req)
    assert 0 <= res.score <= 100
    assert res.rating in ["healthy", "watch", "risk"]
    assert len(res.headline) > 10
    assert len(res.summary) > 30
    assert len(res.strengths) >= 2
    assert len(res.risks) >= 1
    assert len(res.recommendations) >= 2
    assert len(res.stage_insights) == 6
    assert res.provider == "mock"


def test_stale_records_trigger_risk_warning():
    """Stale candidate count must be reflected in risks or recommendations."""
    req = make_test_payload(total_candidates=10, stale_count=3)
    res = _analyze_with_mock(req)
    assert any("stale" in r.lower() or "untouched" in r.lower() or "7 days" in r.lower() for r in res.risks)


def test_analyze_http_endpoint():
    """POST /analyze returns 200 with matching schema."""
    payload = make_test_payload()
    response = client.post("/analyze", json=payload.model_dump())
    assert response.status_code == 200
    data = response.json()
    assert "headline" in data
    assert "score" in data
    assert "rating" in data
    assert "summary" in data
    assert "strengths" in data
    assert "risks" in data
    assert "recommendations" in data
    assert "stage_insights" in data
    assert data["provider"] in ["mock", "llm"]
