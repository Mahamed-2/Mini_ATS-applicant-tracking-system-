<script setup lang="ts">
/**
 * Table.vue – High-Density Zebra-Free Table with Sticky Header
 * Source of Truth: docs/DESIGN.md & .claude/skills/ats-orchestrator/reference/DESIGN_TOKENS.md
 */
</script>

<template>
  <div class="w-full overflow-x-auto border border-border-subtle rounded-lg bg-surface-card shadow-sm">
    <table class="w-full text-left border-collapse">
      <thead class="bg-surface-hover/80 sticky top-0 z-10 border-b border-border-subtle">
        <tr class="text-[11px] font-semibold text-text-variant uppercase tracking-[0.02em]">
          <slot name="header" />
        </tr>
      </thead>
      <tbody class="divide-y divide-border-subtle text-xs text-text-primary">
        <slot />
      </tbody>
    </table>
  </div>
</template>
