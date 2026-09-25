<script setup lang="ts">
/**
 * ErrorState.vue – Graceful Error Boundary Card with Retry Action
 * Source of Truth: docs/DESIGN.md & .claude/skills/ats-orchestrator/reference/DESIGN_TOKENS.md
 */
import { AlertTriangle, RefreshCw } from '@/lib/icons';
import Button from './Button.vue';

interface Props {
  title?: string;
  message?: string;
  retryText?: string;
}

withDefaults(defineProps<Props>(), {
  title: 'Something went wrong',
  message: 'Failed to load resources. Please try again.',
  retryText: 'Retry',
});

const emit = defineEmits<{
  (e: 'retry'): void;
}>();
</script>

<template>
  <div class="flex flex-col items-center justify-center p-6 text-center max-w-sm mx-auto bg-danger-subtle/50 border border-danger-border rounded-lg">
    <div class="w-10 h-10 rounded-full bg-danger-subtle text-danger flex items-center justify-center mb-2">
      <AlertTriangle class="w-5 h-5" />
    </div>
    <h4 class="text-xs font-semibold text-danger mb-1">
      {{ title }}
    </h4>
    <p class="text-xs text-text-variant mb-3">
      {{ message }}
    </p>
    <Button
      variant="secondary"
      size="sm"
      @click="emit('retry')"
    >
      <template #icon-left>
        <RefreshCw class="w-3.5 h-3.5" />
      </template>
      {{ retryText }}
    </Button>
  </div>
</template>
