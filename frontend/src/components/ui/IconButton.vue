<script setup lang="ts">
/**
 * IconButton.vue – Compact Square Action Button
 * Source of Truth: docs/DESIGN.md & .claude/skills/ats-orchestrator/reference/DESIGN_TOKENS.md
 */
import { computed } from 'vue';

interface Props {
  variant?: 'primary' | 'secondary' | 'ghost' | 'danger' | 'ai';
  size?: 'sm' | 'md' | 'lg';
  disabled?: boolean;
  ariaLabel?: string;
}

const props = withDefaults(defineProps<Props>(), {
  variant: 'ghost',
  size: 'md',
  disabled: false,
  ariaLabel: 'Action button',
});

const variantClasses = computed(() => {
  switch (props.variant) {
    case 'primary':
      return 'bg-primary text-white hover:bg-primary-hover';
    case 'secondary':
      return 'bg-surface-card text-text-primary border border-border-subtle hover:bg-surface-hover';
    case 'danger':
      return 'bg-danger text-white hover:bg-danger-hover';
    case 'ai':
      return 'bg-ai-subtle text-ai-accent border border-ai-border hover:shadow-ai-glow';
    case 'ghost':
    default:
      return 'bg-transparent text-text-muted hover:text-text-primary hover:bg-surface-hover';
  }
});

const sizeClasses = computed(() => {
  switch (props.size) {
    case 'sm':
      return 'w-6 h-6 p-1 rounded-sm text-xs';
    case 'lg':
      return 'w-10 h-10 p-2 rounded-md text-base';
    case 'md':
    default:
      return 'w-8 h-8 p-1.5 rounded-sm text-sm';
  }
});
</script>

<template>
  <button
    type="button"
    :aria-label="ariaLabel"
    :disabled="disabled"
    :class="[
      'inline-flex items-center justify-center transition-colors cursor-pointer select-none focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary',
      variantClasses,
      sizeClasses,
      disabled ? 'opacity-50 cursor-not-allowed pointer-events-none' : '',
    ]"
  >
    <slot />
  </button>
</template>
