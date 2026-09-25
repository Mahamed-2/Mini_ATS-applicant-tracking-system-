<script setup lang="ts">
/**
 * Input.vue – Base Input with 4px border-radius and token styling
 * Source of Truth: docs/DESIGN.md & .claude/skills/ats-orchestrator/reference/DESIGN_TOKENS.md
 */
import { inject, computed, useId, type Ref } from 'vue';

interface Props {
  modelValue?: string | number | null;
  type?: string;
  placeholder?: string;
  disabled?: boolean;
  error?: boolean;
  id?: string;
  name?: string;
  autocomplete?: string;
}

const props = withDefaults(defineProps<Props>(), {
  modelValue: '',
  type: 'text',
  placeholder: '',
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
  if (props.autocomplete && props.autocomplete !== 'off' && props.autocomplete !== 'on') {
    return props.autocomplete;
  }
  return resolvedId.value;
});

function onInput(e: Event) {
  emit('update:modelValue', (e.target as HTMLInputElement).value);
}
</script>

<template>
  <div class="relative flex items-center w-full">
    <div v-if="$slots.prefix" class="absolute left-2.5 flex items-center text-text-muted pointer-events-none">
      <slot name="prefix" />
    </div>
    <input
      :id="resolvedId"
      :name="resolvedName"
      :type="type"
      :value="modelValue ?? ''"
      :placeholder="placeholder"
      :disabled="disabled"
      :autocomplete="autocomplete"
      :class="[
        'w-full h-8 bg-surface-card text-text-primary text-xs rounded-sm border transition-colors outline-none',
        'placeholder:text-text-muted',
        $slots.prefix ? 'pl-8' : 'pl-3',
        $slots.suffix ? 'pr-8' : 'pr-3',
        error
          ? 'border-danger focus:ring-1 focus:ring-danger'
          : 'border-border-strong focus:border-primary focus:ring-1 focus:ring-primary',
        disabled ? 'opacity-50 cursor-not-allowed bg-surface-hover' : '',
      ]"
      @input="onInput"
    />
    <div v-if="$slots.suffix" class="absolute right-2.5 flex items-center text-text-muted">
      <slot name="suffix" />
    </div>
  </div>
</template>
