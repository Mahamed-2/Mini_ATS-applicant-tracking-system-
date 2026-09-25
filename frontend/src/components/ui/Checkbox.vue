<script setup lang="ts">
/**
 * Checkbox.vue – Compact 14x14 Checkbox with 3px border-radius
 * Source of Truth: docs/DESIGN.md & .claude/skills/ats-orchestrator/reference/DESIGN_TOKENS.md
 */
import { Check } from '@/lib/icons';

interface Props {
  modelValue?: boolean;
  disabled?: boolean;
  id?: string;
}

const props = withDefaults(defineProps<Props>(), {
  modelValue: false,
  disabled: false,
});

const emit = defineEmits<{
  (e: 'update:modelValue', value: boolean): void;
}>();

function toggle() {
  if (!props.disabled) {
    emit('update:modelValue', !props.modelValue);
  }
}
</script>

<template>
  <button
    type="button"
    role="checkbox"
    :aria-checked="modelValue"
    :id="id"
    :disabled="disabled"
    :class="[
      'w-3.5 h-3.5 rounded-[3px] border flex items-center justify-center transition-colors select-none cursor-pointer',
      modelValue
        ? 'bg-primary border-primary text-white'
        : 'bg-surface-card border-border-strong hover:border-primary',
      disabled ? 'opacity-50 cursor-not-allowed pointer-events-none' : '',
    ]"
    @click="toggle"
  >
    <Check v-if="modelValue" class="w-2.5 h-2.5 stroke-[3]" />
  </button>
</template>
