<script setup lang="ts">
/**
 * ScoreBadge.vue – AI Match Score Pill (Violet Tint Only)
 * Source of Truth: docs/DESIGN.md & .claude/skills/ats-orchestrator/reference/DESIGN_TOKENS.md
 */
import { computed } from 'vue';
import { Zap } from '@/lib/icons';

interface Props {
  score?: number | null;
  size?: 'sm' | 'md';
}

const props = withDefaults(defineProps<Props>(), {
  score: null,
  size: 'md',
});

const formattedScore = computed(() => {
  if (props.score === null || props.score === undefined) return null;
  return Math.round(props.score);
});
</script>

<template>
  <span
    v-if="formattedScore !== null"
    :class="[
      'inline-flex items-center gap-1 bg-ai-subtle text-ai-accent border border-ai-border rounded-full font-semibold select-none shadow-sm',
      size === 'sm' ? 'px-1.5 py-0.5 text-[10px]' : 'px-2 py-0.5 text-[11px]',
    ]"
  >
    <Zap :class="size === 'sm' ? 'w-2.5 h-2.5' : 'w-3 h-3'" />
    <span class="tabular-nums">{{ formattedScore }}% Match</span>
  </span>
  <span
    v-else
    :class="[
      'inline-flex items-center gap-1 bg-surface-hover text-text-muted border border-border-subtle rounded-full font-normal select-none',
      size === 'sm' ? 'px-1.5 py-0.5 text-[10px]' : 'px-2 py-0.5 text-[11px]',
    ]"
  >
    <span>Not evaluated</span>
  </span>
</template>
