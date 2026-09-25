# Creating Demo Auth Users in Supabase

In Supabase Postgres, `public.profiles` enforces a foreign key constraint to `auth.users(id)`:
```sql
REFERENCES auth.users(id) ON DELETE CASCADE
```
Before running [`supabase/seed/demo_data.sql`](./demo_data.sql), the two demo auth accounts must exist in `auth.users`.

---

## Demo Accounts Overview

| Account | Email | Role | Canonical ID |
|---|---|---|---|
| **Admin** | `admin@nordic-recruit.demo` | `admin` | `11111111-1111-4111-8111-111111111111` |
| **Customer / Recruiter** | `recruiter@nordic-tech.demo` | `customer` | `22222222-2222-4222-8222-222222222222` |

> [!IMPORTANT]
> **Password Best Practice**: Never commit plaintext demo passwords. Store your demo password locally in your private environment:
> ```env
> DEMO_USER_PASSWORD=replace-with-local-demo-password
> ```

---

## Option A: Supabase Dashboard (Recommended for Quick Setup)

1. Open your Supabase project dashboard at [https://supabase.com/dashboard](https://supabase.com/dashboard).
2. Navigate to **Authentication** → **Users**.
3. Click **"Add user"** → **"Create user"**:
   - **Email**: `admin@nordic-recruit.demo`
   - **Password**: Enter your `$DEMO_USER_PASSWORD`
   - **Auto Confirm Email**: Checked (Enabled)
4. Repeat for the customer account:
   - **Email**: `recruiter@nordic-tech.demo`
   - **Password**: Enter your `$DEMO_USER_PASSWORD`
   - **Auto Confirm Email**: Checked (Enabled)
5. Copy the generated **User UID** for each user from the table.
6. **If the generated UUIDs differ from the canonical UUIDs**:
   - Copy the actual UUIDs from the dashboard.
   - Update the IDs in:
     - [`supabase/seed/demo_data.sql`](./demo_data.sql)
     - [`supabase/seed/demo_dataset.json`](./demo_dataset.json)
     - [`docs/DEMO.md`](../../docs/DEMO.md)

---

## Option B: Supabase Auth Admin API (Command Line / CI)

The Supabase Auth Admin API allows programmatic creation with auto-confirmed email. Run this securely from a terminal using your environment variables:

```bash
# Set your environment variables (never commit real values)
export SUPABASE_URL="https://YOUR_PROJECT_REF.supabase.co"
export SUPABASE_SERVICE_ROLE_KEY="YOUR_SERVICE_ROLE_KEY"
export DEMO_USER_PASSWORD="YourLocalSecurePassword123!"

# 1. Create Admin User
curl -s -X POST "${SUPABASE_URL}/auth/v1/admin/users" \
  -H "apikey: ${SUPABASE_SERVICE_ROLE_KEY}" \
  -H "Authorization: Bearer ${SUPABASE_SERVICE_ROLE_KEY}" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@nordic-recruit.demo",
    "password": "'"${DEMO_USER_PASSWORD}"'",
    "email_confirm": true
  }'

# 2. Create Customer User
curl -s -X POST "${SUPABASE_URL}/auth/v1/admin/users" \
  -H "apikey: ${SUPABASE_SERVICE_ROLE_KEY}" \
  -H "Authorization: Bearer ${SUPABASE_SERVICE_ROLE_KEY}" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "recruiter@nordic-tech.demo",
    "password": "'"${DEMO_USER_PASSWORD}"'",
    "email_confirm": true
  }'
```

After creation, inspect the JSON output to extract the generated `id` fields and verify they match your seed configuration.

---

## Next Step

Once both accounts exist in `auth.users`, proceed to execute [`supabase/seed/demo_data.sql`](./demo_data.sql) in the Supabase SQL Editor.
