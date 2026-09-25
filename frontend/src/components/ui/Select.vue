<script setup lang="ts">
/**
 * Select.vue – Stylized Select with Lucide ChevronDown
 * Source of Truth: docs/DESIGN.md & .claude/skills/ats-orchestrator/reference/DESIGN_TOKENS.md
 */
import { ChevronDown } from '@/lib/icons';

interface Option {
  value: string | number;
  label: string;
}

interface Props {
  modelValue?: string | number | null;
  options?: Option[];
  placeholder?: string;
  disabled?: boolean;
  error?: boolean;
  id?: string;
}

withDefaults(defineProps<Props>(), {
  modelValue: '',
  options: () => [],
  placeholder: '',
  disabled: false,
  error: false,
});

const emit = defineEmits<{
  (e: 'update:modelValue', value: string): void;
}>();

function onChange(e: Event) {
  emit('update:modelValue', (e.target as HTMLSelectElement).value);
}
</script>

<template>
  <div class="relative flex items-center w-full">
    <select
      :id="id"
      :value="modelValue ?? ''"
      :disabled="disabled"
      :class="[
        'w-full h-8 pl-3 pr-8 bg-surface-card text-text-primary text-xs rounded-sm border appearance-none transition-colors outline-none cursor-pointer',
        error
          ? 'border-danger focus:ring-1 focus:ring-danger'
          : 'border-border-strong focus:border-primary focus:ring-1 focus:ring-primary',
        disabled ? 'opacity-50 cursor-not-allowed bg-surface-hover' : '',
      ]"
      @change="onChange"
    >
      <option v-if="placeholder" value="" disabled :selected="!modelValue">
        {{ placeholder }}
      </option>
      <slot>
        <option
          v-for="opt in options"
          :key="opt.value"
          :value="opt.value"
        >
          {{ opt.label }}
        </option>
      </slot>
    </select>
    <ChevronDown class="absolute right-2.5 w-3.5 h-3.5 text-text-muted pointer-events-none" />
  </div>
</template>
