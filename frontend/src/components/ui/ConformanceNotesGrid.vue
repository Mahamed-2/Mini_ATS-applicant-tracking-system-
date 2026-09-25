<script setup lang="ts">
/**
 * ConformanceNotesGrid.vue – 4-Column Architectural Conformance Specification
 * Source of Truth: docs/DESIGN.md & .claude/skills/ats-orchestrator/reference/SHELL_DASHBOARD.md
 */
import { Sun, ShieldCheck, Terminal, Columns2 } from '@/lib/icons';

const notes = [
  {
    title: 'THEME SYSTEM',
    subtitle: 'Light-Dominant Ergonomics',
    icon: Sun,
    description: 'Light-dominant #f8f9ff canvas with slate card nesting. High contrast typography and strict semantic token isolation across all views.',
    spec: 'Canvas: #f8f9ff • Card: #ffffff',
  },
  {
    title: 'STATE PERSISTENCE',
    subtitle: 'Multi-Tenant Scoping',
    icon: ShieldCheck,
    description: 'Pinia syncs impersonation identity, customerId query boundaries, and bearer tenant headers server-side via .NET API validation.',
    spec: 'RBAC Scoped • Zero Client Leaks',
  },
  {
    title: 'KEYBOARD SHORTCUTS',
    subtitle: 'High-Velocity Controls',
    icon: Terminal,
    description: '⌘K opens fuzzy search across candidates and jobs. ESC closes dialogs and sheets. [ toggles the 240px navigation sidebar.',
    spec: '⌘K (Search) • [ (Nav) • ESC (Close)',
  },
  {
    title: 'MASTER-DETAIL LAYOUT',
    subtitle: '60/40 Split Viewport',
    icon: Columns2,
    description: 'Responsive 60/40 split-pane design preserves list position while rendering deep AI scorecard audits and candidate details.',
    spec: '60% Directory / 40% Detail Scorecard',
  },
];
</script>

<template>
  <div class="mt-4">
    <div class="flex items-center justify-between mb-3">
      <div class="flex items-center gap-2">
        <h3 class="text-sm font-bold text-text-primary">
          API & Architecture Conformance Notes
        </h3>
        <span class="text-[11px] font-mono text-text-muted px-2 py-0.5 bg-surface-card border border-border-subtle rounded-full">
          Vue 3.5.x • Tailwind Token Engine
        </span>
      </div>
    </div>

    <!-- 4 Architectural Columns -->
    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-3">
      <div
        v-for="note in notes"
        :key="note.title"
        class="surface-1 bg-surface-card border border-border-subtle rounded-lg p-3.5 flex flex-col justify-between"
      >
        <div>
          <div class="flex items-center justify-between mb-2">
            <span class="text-[10px] font-bold text-text-muted uppercase tracking-wider">
              {{ note.title }}
            </span>
            <component :is="note.icon" class="w-4 h-4 text-primary" />
          </div>

          <h4 class="text-xs font-bold text-text-primary mb-1">
            {{ note.subtitle }}
          </h4>
          <p class="text-[11px] text-text-muted leading-relaxed">
            {{ note.description }}
          </p>
        </div>

        <div class="mt-3 pt-2 border-t border-border-subtle/50 text-[10px] font-mono text-text-variant">
          {{ note.spec }}
        </div>
      </div>
    </div>
  </div>
</template>
