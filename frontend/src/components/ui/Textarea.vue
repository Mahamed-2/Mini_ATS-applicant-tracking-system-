<script setup lang="ts">
/**
 * Textarea.vue – Base Textarea with 4px border-radius
 * Source of Truth: docs/DESIGN.md & .claude/skills/ats-orchestrator/reference/DESIGN_TOKENS.md
 */
import { inject, computed, useId, type Ref } from 'vue';

interface Props {
  modelValue?: string | null;
  placeholder?: string;
  rows?: number;
  disabled?: boolean;
  error?: boolean;
  id?: string;
  name?: string;
}

const props = withDefaults(defineProps<Props>(), {
  modelValue: '',
  placeholder: '',
  rows: 3,
  disabled: false,
  error: false,
});

const emit = defineEmits<{
  (e: 'update:modelValue', value: string): void;
}>();

const fallbackId = useId();
const injectedId = inject<Ref<string> | string | null>('fieldId', null);

const resolvedId = computed(() => {
  if (props.id) return props.id;
  if (injectedId) return typeof injectedId === 'string' ? injectedId : injectedId.value;
  return fallbackId;
});

const resolvedName = computed(() => {
  if (props.name) return props.name;
  return resolvedId.value;
});

function onInput(e: Event) {
  emit('update:modelValue', (e.target as HTMLTextAreaElement).value);
}
</script>

<template>
  <textarea
    :id="resolvedId"
    :name="resolvedName"
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
