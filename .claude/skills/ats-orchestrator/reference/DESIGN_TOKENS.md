# Recruiter Velocity ATS – Comprehensive Design Token Dictionary

<!--
  Source of Truth for frontend design tokens and components.
  Referenced by: docs/DESIGN.md, .claude/skills/ats-orchestrator/SKILL.md, frontend/src/styles/tokens.css
-->

## 1. Canonical Color Tokens

### 1.1 Core Brand & Functional Colors
| Token Name | Hex Code | Purpose & Constraints |
|---|---|---|
| `--primary` | `#1d68f0` | Cobalt primary: active states, primary actions, selections, focus ring. |
| `--primary-hover` | `#1554c7` | Darker cobalt for hover states on primary buttons. |
| `--primary-subtle` | `#eff6ff` | Light cobalt tint for selected table rows, hover pills. |
| `--ai-accent` | `#7c3aed` | Violet accent: **STRICTLY RESERVED FOR AI** (match %, CV parsing, copilot, AI reports). |
| `--ai-accent-hover` | `#6d28d9` | Darker violet for AI button hover. |
| `--ai-subtle` | `#f5f3ff` | Soft violet background tint for AI match pills & glow containers. |
| `--ai-border` | `#ddd6fe` | Border for AI badges and cards. |
| `--success` | `#059669` | Emerald: Hired stage, verified status, positive health ratings. |
| `--success-subtle` | `#ecfdf5` | Soft emerald background. |
| `--success-border` | `#a7f3d0` | Emerald border. |
| `--warning` | `#d97706` | Amber: Screening stage, watch rating, caution alerts. |
| `--warning-subtle` | `#fffbeb` | Soft amber background. |
| `--danger` | `#e11d48` | Rose: Rejected stage, destructive buttons, error toasts. |
| `--danger-subtle` | `#fff1f2` | Soft rose background. |
| `--info` | `#0284c7` | Sky: New candidate stage, informational notices. |
| `--info-subtle` | `#f0f9ff` | Soft sky background. |

### 1.2 Neutral & Surface Hierarchy
| Token Name | Light Theme | Dark Theme (Derived) | Role |
|---|---|---|---|
| `--canvas` | `#f8f9ff` | `#0b0f19` | Surface 0: main application background. |
| `--surface-card` | `#ffffff` | `#131b2e` | Surface 1: cards, tables, sidebar, topbar. |
| `--surface-modal` | `#ffffff` | `#1e293b` | Surface 2: dialogs, floating dropdowns, popovers. |
| `--border-subtle` | `#e2e8f0` | `#1e293b` | Slate 200: 1px divider and card borders. |
| `--border-strong` | `#cbd5e1` | `#334155` | Slate 300: input borders, column separators. |
| `--text-primary` | `#0f172a` | `#f8fafc` | Slate 900: headlines and main body text. |
| `--text-muted` | `#64748b` | `#94a3b8` | Slate 500: timestamps, secondary labels, subtext. |
| `--text-variant` | `#475569` | `#cbd5e1` | Slate 600: table headers, input labels. |

---

## 2. Pipeline Stage Taxonomy (Strict Hex Values)

All pipeline stages must render with both color AND semantic text labels (never color alone):

| Stage | Classification | Text Hex | Background Hex | Border Hex |
|---|---|---|---|---|
| `new` | Info | `#0284c7` | `#f0f9ff` | `#bae6fd` |
| `screening` | Warning | `#d97706` | `#fffbeb` | `#fde68a` |
| `interview` | Brand Cobalt | `#1d68f0` | `#eff6ff` | `#bfdbfe` |
| `offer` | Special Violet | `#7c3aed` | `#f5f3ff` | `#ddd6fe` |
| `hired` | Success Emerald | `#059669` | `#ecfdf5` | `#a7f3d0` |
| `rejected` | Danger Rose | `#e11d48` | `#fff1f2` | `#fecdd3` |

---

## 3. Typography Scale (Inter Font Stack)

**Mandatory**: `font-variant-numeric: tabular-nums` is required on all metrics, scores, candidate counts, and dates.

| Token | Size | Line Height | Letter Spacing | Weight | Usage |
|---|---|---|---|---|---|
| `headline-xl` | 30px | 38px | -0.02em | 700 | Page titles, key hero headers |
| `headline-lg` | 22px | 28px | -0.015em | 600 | Section titles, modal headers |
| `headline-sm` | 16px | 22px | -0.01em | 600 | Card titles, column headers |
| `body-lg` | 14px | 20px | normal | 400 | Introductory paragraphs, summaries |
| `body-md` | 13px | 18px | normal | 400 | Base body text, table content |
| `body-sm` | 12px | 16px | normal | 400 | Secondary descriptions, card sub-rows |
| `label-md` | 12px | 16px | +0.01em | 600 | Buttons, tabs, dropdown items |
| `label-sm` | 11px | 14px | +0.02em | 600 | Badges, stage chips, match pills |
| `code-sm` | 11px | 14px | normal | 500 (Mono) | UUIDs, endpoints, latency chips |

---

## 4. Spacing, Sizing & Radii

### 4.1 Spacing Scale
- `space-xs`: `4px`
- `space-sm`: `6px`
- `space-md`: `12px`
- `space-lg`: `16px`
- `space-xl`: `24px`
- `gutter`: `12px`
- `margin-desktop`: `20px`
- `margin-mobile`: `12px`

### 4.2 Component Radii Hierarchy
- **Inputs, badges, small buttons**: `4px` (`rounded`, 0.25rem)
- **Cards, modals, panels**: `8px` (`rounded-lg`, 0.5rem)
- **Stage chips & AI match pills**: `9999px` (`rounded-full`)

---

## 5. Elevation & Shadows
- **Surface 0 (Canvas)**: Background `#f8f9ff`, no shadow.
- **Surface 1 (Cards)**: White `#ffffff` + 1px border `#e2e8f0` + shadow `0 1px 3px rgba(15,23,42,0.04)`.
- **Surface 2 (Floating/Modals)**: White `#ffffff` + 1px border `#e2e8f0` + shadow `0 8px 24px -4px rgba(15,23,42,0.08)`.
- **Kanban Drag State**: Shadow `0 12px 28px -6px rgba(15,23,42,0.16)` + transform `rotate(2deg)`. (Disabled when `prefers-reduced-motion` is active).
- **AI Ambient Glow**: Double inner border + glow `0 0 12px -2px rgba(124,58,237,0.25)`.

---

## 6. Layout Geometry
- **Sidebar**: `240px` fixed-left width, `z-index: 50`.
- **Header**: `56px` height, glass bar with `backdrop-filter: blur(20px)`, `z-index: 40`.
- **Main Canvas**: `z-index: 0`.
- **Kanban Columns**: Min-width `260px`, horizontal overflow scroll.
- **Master-Detail**: 60% master list / 40% detail inspector.
