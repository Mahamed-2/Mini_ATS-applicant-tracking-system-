<script setup lang="ts">
/**
 * Switch.vue – Accessible Toggle Switch with smooth animation
 * Source of Truth: docs/DESIGN.md & .claude/skills/ats-orchestrator/reference/DESIGN_TOKENS.md
 */
interface Props {
  modelValue?: boolean;
  disabled?: boolean;
  ariaLabel?: string;
}

const props = withDefaults(defineProps<Props>(), {
  modelValue: false,
  disabled: false,
  ariaLabel: 'Toggle switch',
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
    role="switch"
    :aria-checked="modelValue"
    :aria-label="ariaLabel"
    :disabled="disabled"
    :class="[
      'w-8 h-4.5 rounded-full p-0.5 transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary select-none cursor-pointer',
      modelValue ? 'bg-primary' : 'bg-border-strong',
      disabled ? 'opacity-50 cursor-not-allowed pointer-events-none' : '',
    ]"
    @click="toggle"
  >
    <span
      :class="[
        'block w-3.5 h-3.5 bg-white rounded-full shadow-sm transition-transform',
        modelValue ? 'translate-x-3.5' : 'translate-x-0',
      ]"
    />
  </button>
</template>
