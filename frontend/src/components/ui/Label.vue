<script setup lang="ts">
/**
 * Label.vue – Accessible Form Label with 12px label-md token
 * Source of Truth: docs/DESIGN.md & .claude/skills/ats-orchestrator/reference/DESIGN_TOKENS.md
 */
interface Props {
  forId?: string;
  required?: boolean;
}

defineProps<Props>();
</script>

<template>
  <label
    :for="forId"
    class="block text-xs font-semibold text-text-variant select-none tracking-[0.01em] mb-1"
  >
    <slot />
    <span v-if="required" class="text-danger ml-0.5" aria-hidden="true">*</span>
  </label>
</template>
