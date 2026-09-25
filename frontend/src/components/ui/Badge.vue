<script setup lang="ts">
/**
 * Badge.vue – Compact 4px Radius Status Badge
 * Source of Truth: docs/DESIGN.md & .claude/skills/ats-orchestrator/reference/DESIGN_TOKENS.md
 */
import { computed } from 'vue';

interface Props {
  variant?: 'primary' | 'secondary' | 'success' | 'warning' | 'danger' | 'info' | 'ai';
}

const props = withDefaults(defineProps<Props>(), {
  variant: 'secondary',
});

const variantClasses = computed(() => {
  switch (props.variant) {
    case 'primary':
      return 'bg-primary-subtle text-primary border-primary/20';
    case 'success':
      return 'bg-success-subtle text-success border-success-border';
    case 'warning':
      return 'bg-warning-subtle text-warning border-warning-border';
    case 'danger':
      return 'bg-danger-subtle text-danger border-danger-border';
    case 'info':
      return 'bg-info-subtle text-info border-info-border';
    case 'ai':
      return 'bg-ai-subtle text-ai-accent border-ai-border';
    case 'secondary':
    default:
      return 'bg-surface-hover text-text-variant border-border-subtle';
  }
});
</script>

<template>
  <span
    :class="[
      'inline-flex items-center gap-1 px-1.5 py-0.5 text-[11px] font-semibold rounded-sm border select-none',
      variantClasses,
    ]"
  >
    <slot />
  </span>
</template>
