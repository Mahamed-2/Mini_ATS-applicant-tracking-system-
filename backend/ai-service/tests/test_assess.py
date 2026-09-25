# Tests for the Mini ATS AI assessment service.
# Related: backend/ai-service/main.py (mock scorer, LLM adapter)
#          .claude/skills/ats-orchestrator/reference/AI_CONTRACT.md
# Run: python -m pytest backend/ai-service/tests/

import sys
import os

# Add the parent directory so we can import main.py directly.
sys.path.insert(0, os.path.join(os.path.dirname(__file__), ".."))

from main import _assess_with_mock, AssessRequest, AssessResponse


def make_req(**kwargs) -> AssessRequest:
    """Helper to create AssessRequest with defaults."""
    defaults = {
        "candidate_name":  "Test Candidate",
        "job_title":       "Python Developer",
        "job_description": "We need a python fastapi developer with async experience.",
        "cv_text":         "",
        "linkedin_url":    "",
        "summary":         "",
    }
    defaults.update(kwargs)
    return AssessRequest(**defaults)


# ── Mock scorer tests ─────────────────────────────────────────────────────────

def test_mock_score_is_in_range():
    """Score must always be between 0 and 100 inclusive."""
    req = make_req(cv_text="python fastapi async rest api")
    result = _assess_with_mock(req)
    assert 0 <= result.score <= 100, f"Score out of range: {result.score}"


def test_mock_score_increases_with_keyword_overlap():
    """More matching words → higher score."""
    low_req  = make_req(cv_text="I enjoy cooking and painting.")
    high_req = make_req(cv_text="Experienced python fastapi developer with async and rest api skills.")
    low_result  = _assess_with_mock(low_req)
    high_result = _assess_with_mock(high_req)
    assert high_result.score > low_result.score, "Higher overlap should yield higher score."


def test_mock_provider_is_mock():
    """Provider field must always be 'mock' when using mock scorer."""
    req = make_req()
    result = _assess_with_mock(req)
    assert result.provider == "mock"


def test_mock_linkedin_strength():
    """LinkedIn URL present should appear in strengths."""
    req = make_req(linkedin_url="https://linkedin.com/in/test")
    result = _assess_with_mock(req)
    assert any("linkedin" in s.lower() for s in result.strengths), \
        "Expected LinkedIn strength signal."


def test_mock_no_linkedin_concern():
    """Missing LinkedIn URL should appear in concerns."""
    req = make_req(linkedin_url="")
    result = _assess_with_mock(req)
    assert any("linkedin" in c.lower() for c in result.concerns), \
        "Expected LinkedIn concern signal."


def test_mock_short_cv_concern():
    """Very short CV text should trigger a concern."""
    req = make_req(cv_text="Hi.")
    result = _assess_with_mock(req)
    assert any("short" in c.lower() or "missing" in c.lower() for c in result.concerns), \
        "Expected short CV concern."


def test_mock_long_cv_strength():
    """Long CV text should appear in strengths."""
    long_cv = "python " * 200  # >500 chars with keyword overlap
    req = make_req(cv_text=long_cv)
    result = _assess_with_mock(req)
    assert any("detailed" in s.lower() or "cv" in s.lower() for s in result.strengths), \
        "Expected detailed CV strength."


def test_mock_has_questions():
    """Mock scorer must always return at least one interview question."""
    req = make_req()
    result = _assess_with_mock(req)
    assert len(result.questions) >= 1, "Expected at least one interview question."


def test_mock_response_is_valid_model():
    """Result must be a valid AssessResponse Pydantic model."""
    req = make_req(cv_text="developer with python and api experience")
    result = _assess_with_mock(req)
    assert isinstance(result, AssessResponse)
    assert isinstance(result.strengths, list)
    assert isinstance(result.concerns, list)
    assert isinstance(result.questions, list)


def test_mock_empty_request_does_not_crash():
    """All-empty request must return a valid result without raising."""
    req = AssessRequest()
    result = _assess_with_mock(req)
    assert 0 <= result.score <= 100
    assert result.provider == "mock"
