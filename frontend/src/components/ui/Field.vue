<script setup lang="ts">
/**
 * Field.vue – Form Field Container linking label, input slot, hint, and error with ARIA
 * Source of Truth: docs/DESIGN.md & .claude/skills/ats-orchestrator/reference/DESIGN_TOKENS.md
 */
import { useId } from 'vue';
import Label from './Label.vue';

interface Props {
  label?: string;
  hint?: string;
  error?: string;
  required?: boolean;
}

defineProps<Props>();
const autoId = useId();
const errorId = `${autoId}-error`;
const hintId = `${autoId}-hint`;
</script>

<template>
  <div class="w-full flex flex-col">
    <Label v-if="label" :for-id="autoId" :required="required">
      {{ label }}
    </Label>
    <div class="relative">
      <slot :id="autoId" :aria-describedby="error ? errorId : hint ? hintId : undefined" :error="!!error" />
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
