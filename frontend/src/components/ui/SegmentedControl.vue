<script setup lang="ts">
/**
 * SegmentedControl.vue – Sliding Segmented Option Switcher
 * Source of Truth: docs/DESIGN.md & .claude/skills/ats-orchestrator/reference/DESIGN_TOKENS.md
 */
interface Option {
  value: string;
  label: string;
  count?: number;
}

interface Props {
  modelValue: string;
  options: Option[];
}

defineProps<Props>();

const emit = defineEmits<{
  (e: 'update:modelValue', val: string): void;
}>();
</script>

<template>
  <div class="inline-flex p-0.5 bg-surface-hover border border-border-subtle rounded-md select-none">
    <button
      v-for="opt in options"
      :key="opt.value"
      type="button"
      :class="[
        'px-2.5 py-1 text-xs font-medium rounded-sm transition-all flex items-center gap-1.5 cursor-pointer',
        modelValue === opt.value
          ? 'bg-surface-card text-text-primary shadow-sm font-semibold'
          : 'text-text-muted hover:text-text-primary',
      ]"
      @click="emit('update:modelValue', opt.value)"
    >
      <span>{{ opt.label }}</span>
      <span
        v-if="opt.count !== undefined"
        class="text-[10px] tabular-nums px-1 py-0.2 bg-border-subtle rounded-full text-text-muted"
      >
        {{ opt.count }}
      </span>
    </button>
  </div>
</template>
