# Recruiter Velocity ATS – Design System Specification

## Overview

The Mini ATS user interface is governed by the **Recruiter Velocity ATS** design system. Designed for high-density recruiting productivity, it balances information clarity, rapid pipeline traversal, and instant visual hierarchy.

Full token dictionary: [`.claude/skills/ats-orchestrator/reference/DESIGN_TOKENS.md`](../.claude/skills/ats-orchestrator/reference/DESIGN_TOKENS.md)

---

## Core Tenets

1. **Light-First Ergonomics**:
   - The default mode is light-dominant with a soft `#f8f9ff` canvas and crisp `#ffffff` card surfaces.
   - Dark theme is strictly derived from the same semantic palette (slate-900 surfaces + cobalt/violet/emerald accents).
2. **Intentional Accent Scoping**:
   - **Cobalt (`#1d68f0`)**: Primary actions, active routes, focus indicators, selection states.
   - **Violet (`#7c3aed`)**: **Strictly reserved for AI intelligence** (match %, CV parsing, copilot insights, pipeline reports). Never use violet for generic buttons.
   - **Emerald (`#059669`)**: Successful outcomes, hired status, verified states.
3. **Tabular Numerics**:
   - All numbers, counts, scores, and dates must use `font-variant-numeric: tabular-nums` to eliminate column jitter.
4. **Strict Iconography**:
   - Only `Lucide` icons are permitted throughout the application. No emojis or mixed icon libraries.
5. **Clear Radii Hierarchy**:
   - `4px` (`rounded`): Inputs, badges, small buttons.
   - `8px` (`rounded-lg`): Cards, modal dialogs, drawers, panels.
   - `9999px` (`rounded-full`): Stage chips, match score pills, status badges.

---

## Pipeline Stage Taxonomy

Stage chips always feature both explicit text labels and high-contrast semantic borders/backgrounds:

- **New**: `#0284c7` text / `#f0f9ff` bg / `#bae6fd` border
- **Screening**: `#d97706` text / `#fffbeb` bg / `#fde68a` border
- **Interview**: `#1d68f0` text / `#eff6ff` bg / `#bfdbfe` border
- **Offer**: `#7c3aed` text / `#f5f3ff` bg / `#ddd6fe` border
- **Hired**: `#059669` text / `#ecfdf5` bg / `#a7f3d0` border
- **Rejected**: `#e11d48` text / `#fff1f2` bg / `#fecdd3` border

---

## Shell Architecture

- **Sidebar (Fixed Left)**: `240px` wide, `z-50`, collapsible via `[` key.
  - Groups: `RECRUITMENT` (Dashboard, Jobs, Candidates, Kanban) and `ADMINISTRATION` (Admin, Settings).
  - Footer indicates active workspace tenant (`Active Workspace • Live / Nordic Tech AB`).
- **Header (Glass Bar)**: `56px` height, `z-40`, backdrop blur 20px.
  - Left: Global omni-search input (`⌘K`).
  - Center: Tenant `Acting as: <Company>` context chip with `Exit Scope` action.
  - Right: Theme toggle + user profile badge.
- **Viewport Layout**: Master-detail split (60% list / 40% scorecard) or responsive Kanban board (`260px` minimum column width).
