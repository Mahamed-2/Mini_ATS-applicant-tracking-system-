<script setup lang="ts">
/**
 * Spinner.vue – Loading Indicator with Lucide LoaderCircle
 * Source of Truth: docs/DESIGN.md & .claude/skills/ats-orchestrator/reference/DESIGN_TOKENS.md
 */
import { LoaderCircle } from '@/lib/icons';

interface Props {
  size?: 'sm' | 'md' | 'lg';
  color?: 'primary' | 'muted' | 'white' | 'ai';
}

withDefaults(defineProps<Props>(), {
  size: 'md',
  color: 'primary',
});
</script>

<template>
  <LoaderCircle
    :class="[
      'animate-spin',
      size === 'sm' ? 'w-3.5 h-3.5' : size === 'lg' ? 'w-8 h-8' : 'w-5 h-5',
      color === 'muted' ? 'text-text-muted' : color === 'white' ? 'text-white' : color === 'ai' ? 'text-ai-accent' : 'text-primary',
    ]"
  />
</template>
