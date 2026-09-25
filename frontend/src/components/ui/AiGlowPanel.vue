<script setup lang="ts">
/**
 * AiGlowPanel.vue – Container with Double Inner Border and Ambient Violet Glow
 * Source of Truth: docs/DESIGN.md & .claude/skills/ats-orchestrator/reference/DESIGN_TOKENS.md
 */
interface Props {
  padding?: 'sm' | 'md' | 'lg' | 'none';
}

withDefaults(defineProps<Props>(), {
  padding: 'md',
});
</script>

<template>
  <div
    :class="[
      'ai-glow-panel bg-surface-card border border-ai-border rounded-lg shadow-ai-glow transition-all',
      padding === 'none' ? 'p-0' : padding === 'sm' ? 'p-3' : padding === 'lg' ? 'p-6' : 'p-4',
    ]"
  >
    <slot />
  </div>
</template>
