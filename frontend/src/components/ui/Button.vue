<script setup lang="ts">
/**
 * Button.vue – Recruiter Velocity Semantic Button Component
 * Source of Truth: docs/DESIGN.md & .claude/skills/ats-orchestrator/reference/DESIGN_TOKENS.md
 * Variants: primary (cobalt), secondary (slate border), ghost, danger (rose), ai (violet tint + glow).
 */
import { computed } from 'vue';
import { LoaderCircle } from '@/lib/icons';

interface Props {
  variant?: 'primary' | 'secondary' | 'ghost' | 'danger' | 'ai';
  size?: 'sm' | 'md' | 'lg';
  loading?: boolean;
  disabled?: boolean;
  type?: 'button' | 'submit' | 'reset';
  fullWidth?: boolean;
}

const props = withDefaults(defineProps<Props>(), {
  variant: 'primary',
  size: 'md',
  loading: false,
  disabled: false,
  type: 'button',
  fullWidth: false,
});

const variantClasses = computed(() => {
  switch (props.variant) {
    case 'primary':
      return 'bg-primary text-white hover:bg-primary-hover shadow-sm active:translate-y-[0.5px] border-transparent';
    case 'secondary':
      return 'bg-surface-card text-text-primary border border-border-subtle hover:bg-surface-hover active:bg-border-subtle';
    case 'ghost':
      return 'bg-transparent text-text-variant hover:bg-surface-hover hover:text-text-primary border-transparent';
    case 'danger':
      return 'bg-danger text-white hover:bg-danger-hover active:translate-y-[0.5px] border-transparent';
    case 'ai':
      return 'bg-ai-subtle text-ai-accent border border-ai-border hover:shadow-ai-glow hover:bg-white active:bg-ai-subtle';
    default:
      return 'bg-primary text-white border-transparent';
  }
});

const sizeClasses = computed(() => {
  switch (props.size) {
    case 'sm':
      return 'h-7 px-2.5 text-xs rounded-sm gap-1.5';
    case 'lg':
      return 'h-10 px-4 text-sm rounded-md gap-2';
    case 'md':
    default:
      return 'h-8 px-3 text-xs font-medium rounded-sm gap-2';
  }
});
</script>

<template>
  <button
    :type="type"
    :disabled="disabled || loading"
    :class="[
      'inline-flex items-center justify-center font-medium transition-all select-none border focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary',
      variantClasses,
      sizeClasses,
      fullWidth ? 'w-full' : '',
      disabled || loading ? 'opacity-50 cursor-not-allowed pointer-events-none' : 'cursor-pointer',
    ]"
  >
    <LoaderCircle v-if="loading" class="w-3.5 h-3.5 animate-spin" />
    <slot name="icon-left" />
    <slot />
    <slot name="icon-right" />
  </button>
</template>
