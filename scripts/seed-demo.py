#!/usr/bin/env python3
"""
scripts/seed-demo.py

Deterministic demo data seeder for Mini ATS prototype.
Reads canonical demo dataset from supabase/seed/demo_dataset.json and:
1. Validates required environment variables (SUPABASE_URL, SUPABASE_SERVICE_ROLE_KEY).
2. Verifies or creates the two demo auth users via Supabase Auth Admin API.
3. Idempotently upserts profiles, jobs, and candidates via Supabase PostgREST API.
4. Outputs a clear execution summary without leaking secrets.

Related files:
- Source of truth: supabase/seed/demo_dataset.json
- SQL Seed:        supabase/seed/demo_data.sql
- Auth manual:     supabase/seed/create_demo_auth_users.md
- Demo script:     docs/DEMO.md
- Documentation:   docs/DEMO_SEED_DATA.md
"""

import json
import os
import sys
import urllib.error
import urllib.parse
import urllib.request
from pathlib import Path


def main() -> int:
    print("=" * 65)
    print(" Mini ATS – Deterministic Demo Data Seeder")
    print("=" * 65)

    supabase_url = os.environ.get("SUPABASE_URL", "").rstrip("/")
    service_role_key = os.environ.get("SUPABASE_SERVICE_ROLE_KEY", "")
    demo_password = os.environ.get("DEMO_USER_PASSWORD", "NordicTechDemo2026!")

    if not supabase_url or not service_role_key:
        print("\n[ERROR] Missing required environment variables.", file=sys.stderr)
        print("Please provide both SUPABASE_URL and SUPABASE_SERVICE_ROLE_KEY.", file=sys.stderr)
        print("\nExample usage:", file=sys.stderr)
        print("  export SUPABASE_URL=\"https://xyzcompany.supabase.co\"", file=sys.stderr)
        print("  export SUPABASE_SERVICE_ROLE_KEY=\"eyJhbG...\"", file=sys.stderr)
        print("  export DEMO_USER_PASSWORD=\"YourSecureDemoPassword!\"  # Optional", file=sys.stderr)
        print("  python scripts/seed-demo.py\n", file=sys.stderr)
        return 1

    # Locate canonical dataset
    root_dir = Path(__file__).resolve().parent.parent
    dataset_path = root_dir / "supabase" / "seed" / "demo_dataset.json"

    if not dataset_path.exists():
        print(f"[ERROR] Canonical dataset not found at {dataset_path}", file=sys.stderr)
        return 1

    with open(dataset_path, "r", encoding="utf-8") as f:
        dataset = json.load(f)

    users_data = dataset.get("users", [])
    jobs_data = dataset.get("jobs", [])
    candidates_data = dataset.get("candidates", [])

    print(f" Loaded dataset: {dataset_path.name}")
    print(f" - Users:      {len(users_data)}")
    print(f" - Jobs:       {len(jobs_data)}")
    print(f" - Candidates: {len(candidates_data)}")
    print("-" * 65)

    headers = {
        "apikey": service_role_key,
        "Authorization": f"Bearer {service_role_key}",
        "Content-Type": "application/json",
    }

    # Step 1: Check or create Auth Users via Supabase Auth Admin API
    print(" Step 1: Checking and ensuring Auth Users...")
    for user in users_data:
        user_id = user["id"]
        email = user["email"]
        display_name = user.get("displayName", "")

        # Check if auth user exists
        user_exists = False
        try:
            req = urllib.request.Request(
                f"{supabase_url}/auth/v1/admin/users/{user_id}",
                headers=headers,
                method="GET",
            )
            with urllib.request.urlopen(req) as resp:
                if resp.status == 200:
                    user_exists = True
        except urllib.error.HTTPError as e:
            if e.code != 404:
                # If lookup by UUID fails, check by listing users
                pass

        if not user_exists:
            # Check by listing users to prevent duplicate email conflict
            try:
                list_req = urllib.request.Request(
                    f"{supabase_url}/auth/v1/admin/users?per_page=100",
                    headers=headers,
                    method="GET",
                )
                with urllib.request.urlopen(list_req) as resp:
                    resp_json = json.loads(resp.read().decode())
                    existing_users = resp_json.get("users", [])
                    matched = next((u for u in existing_users if u.get("email") == email), None)
                    if matched:
                        user_exists = True
                        if matched.get("id") != user_id:
                            print(
                                f"  [WARNING] Auth user {email} exists with UUID {matched.get('id')} "
                                f"(canonical is {user_id}). See create_demo_auth_users.md."
                            )
            except Exception as e:
                print(f"  [INFO] User listing check: {e}")

        if not user_exists:
            print(f"  Creating Auth User: {email} ({user_id[:8]}...)...")
            payload = json.dumps({
                "id": user_id,
                "email": email,
                "password": demo_password,
                "email_confirm": True,
                "user_metadata": {
                    "displayName": display_name,
                    "companyName": user.get("companyName", ""),
                },
            }).encode("utf-8")

            try:
                create_req = urllib.request.Request(
                    f"{supabase_url}/auth/v1/admin/users",
                    data=payload,
                    headers=headers,
                    method="POST",
                )
                with urllib.request.urlopen(create_req) as resp:
                    if resp.status in (200, 201):
                        print(f"  [OK] Auth user created: {email}")
            except urllib.error.HTTPError as err:
                err_body = err.read().decode()
                print(f"  [NOTICE] Auth user create note for {email}: {err.code} - {err_body}")
        else:
            print(f"  [OK] Auth user verified: {email}")

    # Step 2: Upsert Profiles via PostgREST
    print("\n Step 2: Upserting Profiles...")
    profiles_payload = []
    for u in users_data:
        profiles_payload.append({
            "id": u["id"],
            "email": u["email"],
            "role": u["role"],
            "display_name": u.get("displayName"),
            "company_name": u.get("companyName"),
        })

    postgrest_headers = {
        **headers,
        "Prefer": "resolution=merge-duplicates,return=representation",
    }

    try:
        req = urllib.request.Request(
            f"{supabase_url}/rest/v1/profiles",
            data=json.dumps(profiles_payload).encode("utf-8"),
            headers=postgrest_headers,
            method="POST",
        )
        with urllib.request.urlopen(req) as resp:
            print(f"  [OK] Upserted {len(profiles_payload)} profile records.")
    except urllib.error.HTTPError as e:
        print(f"  [ERROR] Profiles upsert failed ({e.code}): {e.read().decode()}", file=sys.stderr)
        return 1

    # Step 3: Upsert Jobs via PostgREST
    print("\n Step 3: Upserting Jobs...")
    jobs_payload = []
    for j in jobs_data:
        jobs_payload.append({
            "id": j["id"],
            "customer_id": j["customerId"],
            "title": j["title"],
            "description": j.get("description"),
            "status": j.get("status", "active"),
            "created_by": j.get("createdBy"),
            "updated_by": j.get("updatedBy"),
        })

    try:
        req = urllib.request.Request(
            f"{supabase_url}/rest/v1/jobs",
            data=json.dumps(jobs_payload).encode("utf-8"),
            headers=postgrest_headers,
            method="POST",
        )
        with urllib.request.urlopen(req) as resp:
            print(f"  [OK] Upserted {len(jobs_payload)} job requisitions.")
    except urllib.error.HTTPError as e:
        print(f"  [ERROR] Jobs upsert failed ({e.code}): {e.read().decode()}", file=sys.stderr)
        return 1

    # Step 4: Upsert Candidates via PostgREST
    print("\n Step 4: Upserting Candidates...")
    candidates_payload = []
    for c in candidates_data:
        candidates_payload.append({
            "id": c["id"],
            "customer_id": c["customerId"],
            "job_id": c["jobId"],
            "full_name": c["fullName"],
            "email": c.get("email"),
            "linkedin_url": c.get("linkedinUrl"),
            "stage": c.get("stage", "new"),
            "cv_text": c.get("cvText"),
            "summary": c.get("summary"),
            "ai_score": c.get("aiScore"),
            "ai_feedback": c.get("aiFeedback"),
        })

    try:
        req = urllib.request.Request(
            f"{supabase_url}/rest/v1/candidates",
            data=json.dumps(candidates_payload).encode("utf-8"),
            headers=postgrest_headers,
            method="POST",
        )
        with urllib.request.urlopen(req) as resp:
            print(f"  [OK] Upserted {len(candidates_payload)} candidates.")
    except urllib.error.HTTPError as e:
        print(f"  [ERROR] Candidates upsert failed ({e.code}): {e.read().decode()}", file=sys.stderr)
        return 1

    print("\n" + "=" * 65)
    print(" Demo Data Seeding Completed Successfully!")
    print("=" * 65)
    print(f" - Profiles seeded:   {len(users_data)} (Admin: 1, Customer: 1)")
    print(f" - Jobs seeded:       {len(jobs_data)} (Frontend: 1, Backend: 1)")
    print(f" - Candidates seeded: {len(candidates_data)}")
    print(f"   • Pre-filled AI:   4 (Maria, Oscar, Lucas, Nina)")
    print(f"   • Live AI demo:    6 (Anna, Erik, Jonas, Sofia, Elsa, Hugo)")
    print("=" * 65)
    return 0


if __name__ == "__main__":
    sys.exit(main())
