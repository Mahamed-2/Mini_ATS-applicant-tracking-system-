<script setup lang="ts">
/**
 * Avatar.vue – User & Candidate Avatar with Initials Fallback
 * Source of Truth: docs/DESIGN.md & .claude/skills/ats-orchestrator/reference/DESIGN_TOKENS.md
 */
import { computed } from 'vue';

interface Props {
  name?: string;
  size?: 'sm' | 'md' | 'lg';
  src?: string;
}

const props = withDefaults(defineProps<Props>(), {
  name: 'User',
  size: 'md',
  src: '',
});

const initials = computed(() => {
  const parts = props.name.trim().split(/\s+/);
  if (parts.length >= 2) {
    return `${parts[0][0]}${parts[parts.length - 1][0]}`.toUpperCase();
  }
  return (props.name[0] || 'U').toUpperCase();
});

const sizeClasses = computed(() => {
  switch (props.size) {
    case 'sm':
      return 'w-6 h-6 text-[10px]';
    case 'lg':
      return 'w-10 h-10 text-sm';
    case 'md':
    default:
      return 'w-7 h-7 text-xs';
  }
});
</script>

<template>
  <div
    :class="[
      'inline-flex items-center justify-center rounded-full font-semibold select-none border border-border-subtle bg-primary-subtle text-primary overflow-hidden shrink-0',
      sizeClasses,
    ]"
  >
    <img v-if="src" :src="src" :alt="name" class="w-full h-full object-cover" />
    <span v-else>{{ initials }}</span>
  </div>
</template>
