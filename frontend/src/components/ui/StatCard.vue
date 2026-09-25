<script setup lang="ts">
/**
 * StatCard.vue – Metric Display Card with Mandatory Tabular Numerics
 * Source of Truth: docs/DESIGN.md & .claude/skills/ats-orchestrator/reference/DESIGN_TOKENS.md
 */
import Card from './Card.vue';

interface Props {
  label: string;
  value: string | number;
  subtext?: string;
  trend?: string;
  trendPositive?: boolean;
}

defineProps<Props>();
</script>

<template>
  <Card padding="md">
    <div class="flex items-center justify-between">
      <span class="text-[11px] font-semibold tracking-[0.02em] text-text-muted uppercase">
        {{ label }}
      </span>
      <div v-if="$slots.icon" class="text-text-muted">
        <slot name="icon" />
      </div>
    </div>
    <div class="mt-2 flex items-baseline gap-2">
      <span class="text-2xl font-bold tracking-tight text-text-primary tabular-nums">
        {{ value }}
      </span>
      <span
        v-if="trend"
        :class="[
          'text-xs font-semibold tabular-nums',
          trendPositive ? 'text-success' : 'text-danger',
        ]"
      >
        {{ trend }}
      </span>
    </div>
    <p v-if="subtext" class="mt-1 text-xs text-text-muted">
      {{ subtext }}
    </p>
  </Card>
</template>
