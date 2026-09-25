<script setup lang="ts">
/**
 * Field.vue – Form Field Container linking label, input slot, hint, and error with ARIA
 * Source of Truth: docs/DESIGN.md & .claude/skills/ats-orchestrator/reference/DESIGN_TOKENS.md
 */
import { useId, provide, computed } from 'vue';
import Label from './Label.vue';

interface Props {
  id?: string;
  forId?: string;
  label?: string;
  hint?: string;
  error?: string;
  required?: boolean;
}

const props = defineProps<Props>();
const autoId = useId();
const fieldId = computed(() => props.id || props.forId || autoId);
const errorId = computed(() => `${fieldId.value}-error`);
const hintId = computed(() => `${fieldId.value}-hint`);

// Provide fieldId to any nested form controls (Input, Select, Textarea)
provide('fieldId', fieldId);
provide('fieldErrorId', errorId);
provide('fieldHintId', hintId);
</script>

<template>
  <div class="w-full flex flex-col">
    <Label v-if="label" :for-id="fieldId" :required="required">
      {{ label }}
    </Label>
    <div class="relative">
      <slot :id="fieldId" :aria-describedby="error ? errorId : hint ? hintId : undefined" :error="!!error" />
    </div>
    <span
      v-if="error"
      :id="errorId"
      class="text-[11px] text-danger font-medium mt-1 select-none"
      role="alert"
    >
      {{ error }}
    </span>
    <span
      v-else-if="hint"
      :id="hintId"
      class="text-[11px] text-text-muted mt-1 select-none"
    >
      {{ hint }}
    </span>
  </div>
</template>
