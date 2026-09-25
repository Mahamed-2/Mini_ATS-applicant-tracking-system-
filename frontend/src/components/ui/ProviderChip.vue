<script setup lang="ts">
/**
 * ProviderChip.vue – AI Provider Status Tag (mock | llm)
 * Source of Truth: docs/DESIGN.md & .claude/skills/ats-orchestrator/reference/DESIGN_TOKENS.md
 */
import { computed } from 'vue';
import { Sparkles, Terminal } from '@/lib/icons';

interface Props {
  provider?: string | null;
}

const props = withDefaults(defineProps<Props>(), {
  provider: 'mock',
});

const isMock = computed(() => (props.provider || 'mock').toLowerCase() === 'mock');
</script>

<template>
  <span
    :class="[
      'inline-flex items-center gap-1 px-1.5 py-0.5 rounded-sm text-[10px] font-mono font-medium border select-none',
      isMock
        ? 'bg-surface-hover text-text-variant border-border-strong'
        : 'bg-ai-subtle text-ai-accent border-ai-border font-semibold',
    ]"
  >
    <Terminal v-if="isMock" class="w-2.5 h-2.5" />
    <Sparkles v-else class="w-2.5 h-2.5" />
    <span>{{ isMock ? 'provider: mock' : `provider: ${provider}` }}</span>
  </span>
</template>
