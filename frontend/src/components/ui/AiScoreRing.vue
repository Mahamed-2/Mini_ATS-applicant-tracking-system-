<script setup lang="ts">
/**
 * AiScoreRing.vue – Circular Score Indicator with Violet Arc
 * Source of Truth: docs/DESIGN.md & .claude/skills/ats-orchestrator/reference/DESIGN_TOKENS.md
 */
import { computed } from 'vue';

interface Props {
  score: number;
  size?: number;
  strokeWidth?: number;
  label?: string;
  colorScheme?: 'violet' | 'rating';
  rating?: 'healthy' | 'watch' | 'risk';
}

const props = withDefaults(defineProps<Props>(), {
  size: 80,
  strokeWidth: 6,
  label: 'AI match',
  colorScheme: 'violet',
});

const radius = computed(() => (props.size - props.strokeWidth) / 2);
const circumference = computed(() => 2 * Math.PI * radius.value);
const strokeDashoffset = computed(() => {
  const progress = Math.min(100, Math.max(0, props.score)) / 100;
  return circumference.value * (1 - progress);
});

const strokeColor = computed(() => {
  if (props.colorScheme === 'rating' && props.rating) {
    if (props.rating === 'healthy') return '#059669';
    if (props.rating === 'watch') return '#d97706';
    if (props.rating === 'risk') return '#e11d48';
  }
  return '#7c3aed'; // Strict AI violet default
});
</script>

<template>
  <div class="relative inline-flex flex-col items-center justify-center select-none">
    <svg :width="size" :height="size" class="transform -rotate-90">
      <!-- Background track -->
      <circle
        :cx="size / 2"
        :cy="size / 2"
        :r="radius"
        stroke="currentColor"
        :stroke-width="strokeWidth"
        fill="transparent"
        class="text-border-subtle"
      />
      <!-- Active arc -->
      <circle
        :cx="size / 2"
        :cy="size / 2"
        :r="radius"
        :stroke="strokeColor"
        :stroke-width="strokeWidth"
        stroke-linecap="round"
        fill="transparent"
        :stroke-dasharray="circumference"
        :stroke-dashoffset="strokeDashoffset"
        class="transition-all duration-500 ease-out"
      />
    </svg>
    <div class="absolute inset-0 flex flex-col items-center justify-center text-center">
      <span class="text-base font-bold text-text-primary tabular-nums tracking-tight">
        {{ Math.round(score) }}%
      </span>
      <span class="text-[9px] uppercase font-semibold text-text-muted tracking-wider">
        {{ label }}
      </span>
    </div>
  </div>
</template>
