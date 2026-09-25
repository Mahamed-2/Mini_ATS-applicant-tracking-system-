<script setup lang="ts">
/**
 * LayerBreakdownCard.vue – Layout Stacking Context & Architectural Dimensions
 * Source of Truth: docs/DESIGN.md & .claude/skills/ats-orchestrator/reference/SHELL_DASHBOARD.md
 */
import { Layers } from '@/lib/icons';

interface Props {
  currentRoute?: string;
}

withDefaults(defineProps<Props>(), {
  currentRoute: '/dashboard',
});
</script>

<template>
  <div class="surface-1 bg-surface-card border border-border-subtle rounded-lg p-4 shadow-sm flex flex-col justify-between">
    <div>
      <div class="flex items-center justify-between pb-3 border-b border-border-subtle">
        <div class="flex items-center gap-2">
          <Layers class="w-4 h-4 text-primary" />
          <h3 class="text-xs font-semibold text-text-primary">
            Layout Architecture & Stacking Context
          </h3>
        </div>
        <span class="inline-flex items-center gap-1 text-[10px] font-semibold px-2 py-0.5 bg-success-subtle text-success border border-success-border rounded-full select-none">
          <span class="w-1.5 h-1.5 rounded-full bg-success" />
          Status: Fully Mounted
        </span>
      </div>

      <!-- Three Architectural Tiles: Rail, Header, Viewport -->
      <div class="grid grid-cols-1 sm:grid-cols-3 gap-2.5 mt-3">
        <div class="p-2.5 bg-surface-hover/70 border border-border-subtle rounded-sm">
          <span class="text-[10px] font-bold text-text-muted uppercase tracking-wider block">RAIL SPEC</span>
          <div class="text-xs font-bold text-text-primary mt-1">240px Fixed Left</div>
          <div class="text-[10px] text-text-muted mt-0.5 font-mono">zIndex: 50</div>
          <div class="text-[10px] text-success mt-1 font-medium">W3C Compliant</div>
        </div>

        <div class="p-2.5 bg-surface-hover/70 border border-border-subtle rounded-sm">
          <span class="text-[10px] font-bold text-text-muted uppercase tracking-wider block">HEADER SPEC</span>
          <div class="text-xs font-bold text-text-primary mt-1">56px Glass Bar</div>
          <div class="text-[10px] text-text-muted mt-0.5 font-mono">zIndex: 40</div>
          <div class="text-[10px] text-primary mt-1 font-medium">Backdrop Blur 20px</div>
        </div>

        <div class="p-2.5 bg-surface-hover/70 border border-border-subtle rounded-sm">
          <span class="text-[10px] font-bold text-text-muted uppercase tracking-wider block">VIEWPORT CANVAS</span>
          <div class="text-xs font-bold text-text-primary mt-1">60/40 Split Frame</div>
          <div class="text-[10px] text-text-muted mt-0.5 font-mono">zIndex: 0</div>
          <div class="text-[10px] text-info mt-1 font-medium">Active Mode</div>
        </div>
      </div>
    </div>

    <!-- Footer: Active Route sync note -->
    <div class="mt-4 pt-3 border-t border-border-subtle flex items-center justify-between text-[11px] text-text-muted">
      <span>Route synchronizer active on client view</span>
      <span class="font-mono text-text-primary bg-surface-hover px-1.5 py-0.5 rounded border border-border-subtle">
        Active Route: {{ currentRoute }}
      </span>
    </div>
  </div>
</template>
