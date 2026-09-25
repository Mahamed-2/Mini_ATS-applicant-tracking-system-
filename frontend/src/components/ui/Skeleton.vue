<script setup lang="ts">
/**
 * Skeleton.vue – Layout-Preserving Shimmer Placeholder
 * Source of Truth: docs/DESIGN.md & .claude/skills/ats-orchestrator/reference/DESIGN_TOKENS.md
 */
interface Props {
  width?: string;
  height?: string;
  rounded?: 'sm' | 'md' | 'lg' | 'full';
}

withDefaults(defineProps<Props>(), {
  width: '100%',
  height: '1rem',
  rounded: 'sm',
});
</script>

<template>
  <div
    :class="[
      'bg-slate-200/70 dark:bg-slate-800 animate-pulse select-none',
      rounded === 'full' ? 'rounded-full' : rounded === 'lg' ? 'rounded-lg' : rounded === 'md' ? 'rounded-md' : 'rounded-sm',
    ]"
    :style="{ width, height }"
  />
</template>
