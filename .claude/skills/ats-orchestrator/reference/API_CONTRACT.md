# API Contract Reference

All endpoints return JSON. All authenticated endpoints require:
`Authorization: Bearer <supabase_access_token>`

## Common

### GET /health
- Auth: none
- Response: `{ "status": "ok" }`

### GET /api/account/me
- Auth: any authenticated user
- Response:
```json
{
  "id": "uuid",
  "email": "string",
  "role": "admin|customer",
  "displayName": "string|null",
  "companyName": "string|null"
}
```

## Admin

### POST /api/admin/users
- Auth: admin only
- Body:
```json
{
  "email": "string",
  "password": "string",
  "role": "admin|customer",
  "displayName": "string",
  "companyName": "string"
}
```
- Behavior: Creates Supabase Auth user via Admin API (service role key), inserts `public.profiles` row.
- Response: `{ "id": "uuid", "email": "string", "role": "string" }`

### GET /api/admin/users
- Auth: admin only
- Response: array of profile objects

## Jobs

### GET /api/jobs
- Query: `customerId=uuid` (required)
- Auth: customer (own id only) or admin (any id)
- Response: array of job objects

### POST /api/jobs
- Auth: any authenticated
- Body:
```json
{
  "customerId": "uuid",
  "title": "string",
  "description": "string|null"
}
```
- Response: created job object

### GET /api/jobs/{id}
- Query: `customerId=uuid`
- Auth: customer or admin

### PATCH /api/jobs/{id}
- Query: `customerId=uuid`
- Body: `{ "title": "string", "description": "string", "status": "string" }`

### DELETE /api/jobs/{id}
- Query: `customerId=uuid`

## Candidates

### GET /api/candidates
- Query: `customerId=uuid` (required), `jobId=uuid` (optional), `search=string` (optional)
- Auth: customer or admin

### POST /api/candidates
- Body:
```json
{
  "customerId": "uuid",
  "jobId": "uuid|null",
  "fullName": "string",
  "email": "string|null",
  "linkedinUrl": "string|null",
  "cvText": "string|null",
  "summary": "string|null",
  "stage": "new|screening|interview|offer|hired|rejected"
}
```

### GET /api/candidates/{id}
- Query: `customerId=uuid`

### PATCH /api/candidates/{id}
- Query: `customerId=uuid`
- Body: same shape as POST minus `customerId`

### DELETE /api/candidates/{id}
- Query: `customerId=uuid`

### PATCH /api/candidates/{id}/stage
- Auth: customer or admin
- Body: `{ "stage": "screening" }`
- Response: `{ "success": true }`

## AI

### POST /api/ai/candidates/{id}/assess
- Query: `customerId=uuid`
- Auth: customer or admin
- Behavior:
  1. Load candidate + related job from DB.
  2. Call Python AI service `POST /assess`.
  3. Store `ai_score` and `ai_feedback` on candidate.
  4. Return AI result.
- Response:
```json
{
  "score": 75,
  "summary": "string",
  "strengths": ["string"],
  "concerns": ["string"],
  "questions": ["string"],
  "provider": "mock|llm"
}
```

## Scoping rules

- Customer callers: `customerId` must equal their own profile id.
- Admin callers: `customerId` can be any valid customer profile id.
- `created_by` and `updated_by` are always set server-side.

## Error responses

```json
{ "message": "string", "errors": { "field": ["string"] } }
```

HTTP status codes:
- 400: validation error
- 401: unauthenticated or expired token
- 403: insufficient role or wrong customer scope
- 404: resource not found
- 500: internal server error
