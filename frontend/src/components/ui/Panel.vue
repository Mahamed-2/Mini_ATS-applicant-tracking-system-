<script setup lang="ts">
/**
 * Panel.vue – Surface 2 Modal/Floating Panel with Shadow
 * Source of Truth: docs/DESIGN.md & .claude/skills/ats-orchestrator/reference/DESIGN_TOKENS.md
 */
interface Props {
  padding?: 'none' | 'sm' | 'md' | 'lg';
}

withDefaults(defineProps<Props>(), {
  padding: 'md',
});
</script>

<template>
  <div
    :class="[
      'surface-2 bg-surface-modal border border-border-subtle rounded-lg shadow-modal',
      padding === 'none' ? 'p-0' : padding === 'sm' ? 'p-3' : padding === 'lg' ? 'p-6' : 'p-4',
    ]"
  >
    <slot />
  </div>
</template>
