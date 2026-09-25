<script setup lang="ts">
/**
 * Card.vue – Surface 1 Card with 8px Radius and Subtle Border
 * Source of Truth: docs/DESIGN.md & .claude/skills/ats-orchestrator/reference/DESIGN_TOKENS.md
 */
interface Props {
  hoverable?: boolean;
  padding?: 'none' | 'sm' | 'md' | 'lg';
}

withDefaults(defineProps<Props>(), {
  hoverable: false,
  padding: 'md',
});
</script>

<template>
  <div
    :class="[
      'surface-1 bg-surface-card border border-border-subtle rounded-lg transition-all',
      padding === 'none' ? 'p-0' : padding === 'sm' ? 'p-3' : padding === 'lg' ? 'p-6' : 'p-4',
      hoverable ? 'hover:border-border-strong hover:shadow-md cursor-pointer' : '',
    ]"
  >
    <slot />
  </div>
</template>
