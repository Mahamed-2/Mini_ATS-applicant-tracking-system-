<script setup lang="ts">
/**
 * Textarea.vue – Base Textarea with 4px border-radius
 * Source of Truth: docs/DESIGN.md & .claude/skills/ats-orchestrator/reference/DESIGN_TOKENS.md
 */
interface Props {
  modelValue?: string | null;
  placeholder?: string;
  rows?: number;
  disabled?: boolean;
  error?: boolean;
  id?: string;
}

withDefaults(defineProps<Props>(), {
  modelValue: '',
  placeholder: '',
  rows: 3,
  disabled: false,
  error: false,
});

const emit = defineEmits<{
  (e: 'update:modelValue', value: string): void;
}>();

function onInput(e: Event) {
  emit('update:modelValue', (e.target as HTMLTextAreaElement).value);
}
</script>

<template>
  <textarea
    :id="id"
    :rows="rows"
    :value="modelValue ?? ''"
    :placeholder="placeholder"
    :disabled="disabled"
    :class="[
      'w-full p-2.5 bg-surface-card text-text-primary text-xs rounded-sm border transition-colors outline-none resize-y',
      'placeholder:text-text-muted',
      error
        ? 'border-danger focus:ring-1 focus:ring-danger'
        : 'border-border-strong focus:border-primary focus:ring-1 focus:ring-primary',
      disabled ? 'opacity-50 cursor-not-allowed bg-surface-hover' : '',
    ]"
    @input="onInput"
  />
</template>
