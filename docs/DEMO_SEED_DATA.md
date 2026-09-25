# Mini ATS – Demo Seed Data Reference

This document provides a comprehensive guide to the canonical demo dataset defined in [`supabase/seed/demo_dataset.json`](../supabase/seed/demo_dataset.json) and populated via [`supabase/seed/demo_data.sql`](../supabase/seed/demo_data.sql).

---

## 1. Demo Users

| User / Entity | Email | Role | Canonical UUID | Notes |
|---|---|---|---|---|
| **Seed Admin** | `admin@nordic-recruit.demo` | `admin` | `11111111-1111-4111-8111-111111111111` | Can act on behalf of any customer tenant |
| **Elin Recruiter** | `recruiter@nordic-tech.demo` | `customer` | `22222222-2222-4222-8222-222222222222` | Primary hiring tenant (Nordic Tech AB) |

---

## 2. Demo Jobs

Both jobs belong to Customer **Nordic Tech AB** (`22222222-2222-4222-8222-222222222222`):

| Job Title | Status | Job UUID | Focus Stack |
|---|---|---|---|
| **Senior Frontend Engineer** | `active` | `33333333-3333-4333-8333-333333333301` | Vue 3, TypeScript, Vite, CSS, accessibility |
| **Backend Engineer .NET** | `active` | `33333333-3333-4333-8333-333333333302` | .NET 10, C#, REST, JWT, PostgreSQL, Supabase |

---

## 3. Demo Candidates (10 Total)

All candidates belong to customer `22222222-2222-4222-8222-222222222222`:

| # | Name | Target Job | Stage | LinkedIn Profile | AI Score | Live Demo Role |
|---|---|---|---|---|---|---|
| 1 | **Anna Lund** | Frontend | `new` | Included | `null` | **Live AI Assessment** demonstration |
| 2 | **Erik Berg** | Frontend | `screening` | Included | `null` | Multi-framework frontend profile |
| 3 | **Maria Karlsson** | Frontend | `interview` | Included | **86** | Pre-scored high frontend match |
| 4 | **Jonas Nyström** | Frontend | `offer` | Included | `null` | Migration & leadership profile |
| 5 | **Sofia Lindqvist** | Frontend | `hired` | Included | `null` | Design tokens & WCAG specialist |
| 6 | **Oscar Dahl** | Backend | `rejected` | Included | **48** | Node.js background with limited .NET alignment |
| 7 | **Elsa Moreau** | Frontend | `new` | Included | `null` | Accessible component & i18n focus |
| 8 | **Lucas Meyer** | Frontend | `screening` | **None** | **42** | Junior profile, short CV, missing LinkedIn |
| 9 | **Nina Patel** | Backend | `interview` | Included | **84** | Pre-scored high backend match |
| 10 | **Hugo Silva** | Backend | `new` | Included | `null` | Stage drag-and-drop demonstration |

---

## 4. Distributions

### By Stage (Kanban Columns)

```
[ New ] (3)          [ Screening ] (2)    [ Interview ] (2)     [ Offer ] (1)       [ Hired ] (1)       [ Rejected ] (1)
• Anna Lund          • Erik Berg          • Maria Karlsson      • Jonas Nyström     • Sofia Lindqvist   • Oscar Dahl
• Elsa Moreau        • Lucas Meyer        • Nina Patel
• Hugo Silva
```

### By Target Job

- **Senior Frontend Engineer** (7 candidates):
  - Anna Lund, Erik Berg, Maria Karlsson, Jonas Nyström, Sofia Lindqvist, Elsa Moreau, Lucas Meyer
- **Backend Engineer .NET** (3 candidates):
  - Oscar Dahl, Nina Patel, Hugo Silva

---

## 5. Scripted Demo Filter Examples

When recording the demo, use these exact search and filter interactions on the Kanban board:

| Action | Filter Input | Expected Output |
|---|---|---|
| **Filter by Job** | Select `Backend Engineer .NET` | Displays 3 cards: Oscar Dahl, Nina Patel, Hugo Silva |
| **Filter by Job** | Select `Senior Frontend Engineer` | Displays 7 cards across all 6 columns |
| **Search by Name** | Type `Anna` | Displays only **Anna Lund** |
| **Search by Name** | Type `Kar` | Displays only **Maria Karlsson** |
| **Move Stage** | Drag **Hugo Silva** from `New` to `Screening` | Card transitions immediately with stage update persisted |

---

## 6. Scripted AI Assessment Examples

The dataset combines pre-evaluated profiles with clean profiles for live scoring:

1. **Pre-evaluated High Match (Frontend)**:
   - **Maria Karlsson** (`86/100`): Highlights Vue 3 & TypeScript depth, accessibility audits, and suggests performance budget interview questions.
2. **Pre-evaluated High Match (Backend)**:
   - **Nina Patel** (`84/100`): Highlights .NET 8, JWT, and PostgreSQL depth with safe migration questions.
3. **Pre-evaluated Concerns (Missing Data & Junior)**:
   - **Lucas Meyer** (`42/100`): Flags missing LinkedIn profile URL and brief CV text.
4. **Pre-evaluated Concerns (Stack Mismatch)**:
   - **Oscar Dahl** (`48/100`): Flags Node.js background against .NET position requirements.
5. **Live Assessment Execution**:
   - **Anna Lund**: Has `aiScore: null`. Clicking **"Assess CV"** calls the Python AI service in real time, calculating keyword density against the job description and rendering the structured results panel.
