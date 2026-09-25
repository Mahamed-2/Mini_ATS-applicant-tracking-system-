<script setup lang="ts">
/**
 * FilterBar.vue – Horizontal Filter Strip for Tables and Kanban
 * Source of Truth: docs/DESIGN.md & .claude/skills/ats-orchestrator/reference/DESIGN_TOKENS.md
 */
</script>

<template>
  <div class="flex flex-wrap items-center justify-between gap-3 py-2 border-b border-border-subtle bg-surface-card px-4">
    <div class="flex flex-wrap items-center gap-2">
      <slot name="left" />
    </div>
    <div class="flex items-center gap-2">
      <slot name="right" />
    </div>
  </div>
</template>
