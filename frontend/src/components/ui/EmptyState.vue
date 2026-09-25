<script setup lang="ts">
/**
 * EmptyState.vue – Empty Pipeline / List Placeholder
 * Source of Truth: docs/DESIGN.md & .claude/skills/ats-orchestrator/reference/DESIGN_TOKENS.md
 */
import Button from './Button.vue';

interface Props {
  title: string;
  description?: string;
  actionText?: string;
}

defineProps<Props>();
const emit = defineEmits<{
  (e: 'action'): void;
}>();
</script>

<template>
  <div class="flex flex-col items-center justify-center p-8 text-center max-w-sm mx-auto">
    <div v-if="$slots.icon" class="w-12 h-12 rounded-full bg-surface-hover flex items-center justify-center text-text-muted mb-3 border border-border-subtle">
      <slot name="icon" />
    </div>
    <h3 class="text-sm font-semibold text-text-primary mb-1">
      {{ title }}
    </h3>
    <p v-if="description" class="text-xs text-text-muted mb-4">
      {{ description }}
    </p>
    <Button
      v-if="actionText"
      variant="primary"
      size="sm"
      @click="emit('action')"
    >
      {{ actionText }}
    </Button>
    <slot name="extra" />
  </div>
</template>
