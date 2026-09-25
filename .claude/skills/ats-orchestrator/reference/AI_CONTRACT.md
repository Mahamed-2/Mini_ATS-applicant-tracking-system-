# AI Service Contract Reference

## Endpoint

```
POST /assess
GET  /health
```

Host: `AI_SERVICE_BASE_URL` (e.g. `http://localhost:8001` locally, Railway URL in prod)

## Authentication

If environment variable `AI_SERVICE_API_KEY` is set, require header:
```
X-AI-Service-Key: <value>
```
Return HTTP 401 if key is missing or wrong.

## Request model

```json
{
  "candidate_name": "string (max 200 chars)",
  "job_title": "string (max 200 chars)",
  "job_description": "string (max 5000 chars)",
  "cv_text": "string (max 20000 chars)",
  "linkedin_url": "string (max 500 chars)",
  "summary": "string (max 2000 chars)"
}
```

All fields are optional strings. Empty strings are valid.

## Response model

```json
{
  "score": 75,
  "summary": "Concise one-paragraph assessment.",
  "strengths": ["strength 1", "strength 2"],
  "concerns": ["concern 1"],
  "questions": ["interview question 1", "interview question 2", "interview question 3"],
  "provider": "mock"
}
```

- `score`: integer 0–100
- `provider`: `"mock"` when no LLM key, `"llm"` when LLM was used

## Mock scoring algorithm

When no LLM key is configured (or LLM call fails):

1. Extract words (≥3 chars) from `job_description` and from `cv_text + summary`.
2. Count keyword overlap between job and CV word sets.
3. Base score = `min(95, 35 + overlap × 2)`.
4. Strengths: add "Has LinkedIn URL" if present, "Detailed CV text" if >500 chars, "Strong keyword match" if overlap > 10.
5. Concerns: add "No LinkedIn URL" if missing, "Short CV" if ≤500 chars, "Limited keyword match" if overlap ≤ 10.
6. Questions: three generic hiring-safe questions about tools, outcomes, and motivation.
7. Provider: `"mock"`.

## LLM adapter rules

When `OPENAI_API_KEY` (or equivalent) is set:

1. Build a conservative, hiring-safe prompt.
2. Request JSON output with keys: `score`, `summary`, `strengths`, `concerns`, `questions`.
3. Truncate `cv_text` to 4000 chars before sending to LLM.
4. Do NOT include candidate name in prompt if it could correlate with protected characteristics.
5. If LLM call throws or returns malformed JSON: fall back to mock scorer.
6. Provider: `"llm"` if LLM succeeded, `"mock"` if fallback.

## Safety rules

- Never infer age, gender, ethnicity, nationality, religion, disability, or family status.
- Never include raw email or phone numbers in the prompt.
- Output should be explainable and short (summary ≤ 150 words, arrays ≤ 5 items each).
- Do not fabricate facts not present in the input.

## Environment variables

```env
PORT=8001
AI_SERVICE_API_KEY=<shared secret used by .NET backend>
AI_PROVIDER=openai
OPENAI_API_KEY=<optional>
OPENAI_MODEL=gpt-4o-mini
```

## .NET backend integration

The .NET `AiClient` calls this service:
- URL: `{AiService:BaseUrl}/assess`
- Header: `X-AI-Service-Key: {AiService:ApiKey}`
- Timeout: 30 seconds
- On HTTP error or timeout: surface error to controller (500), do not store partial result.
