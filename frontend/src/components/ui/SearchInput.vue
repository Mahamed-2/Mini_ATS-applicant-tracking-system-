<script setup lang="ts">
/**
 * SearchInput.vue – 32px Omnibar Search Input with ⌘K Badge Slot
 * Source of Truth: docs/DESIGN.md & .claude/skills/ats-orchestrator/reference/DESIGN_TOKENS.md
 */
import { ref } from 'vue';
import { Search, X } from '@/lib/icons';

interface Props {
  modelValue?: string;
  placeholder?: string;
  shortcut?: string;
  disabled?: boolean;
}

withDefaults(defineProps<Props>(), {
  modelValue: '',
  placeholder: 'Search candidates, jobs… (⌘+K)',
  shortcut: '⌘K',
  disabled: false,
});

const emit = defineEmits<{
  (e: 'update:modelValue', value: string): void;
  (e: 'submit', value: string): void;
  (e: 'shortcut'): void;
}>();

const inputRef = ref<HTMLInputElement | null>(null);

function onInput(e: Event) {
  emit('update:modelValue', (e.target as HTMLInputElement).value);
}

function clear() {
  emit('update:modelValue', '');
  inputRef.value?.focus();
}

function focus() {
  inputRef.value?.focus();
}

defineExpose({ focus });
</script>

<template>
  <div class="relative flex items-center w-full">
    <Search class="absolute left-2.5 w-3.5 h-3.5 text-text-muted pointer-events-none" />
    <input
      ref="inputRef"
      type="text"
      :value="modelValue"
      :placeholder="placeholder"
      :disabled="disabled"
      class="w-full h-8 pl-8 pr-14 bg-surface-card text-text-primary text-xs rounded-sm border border-border-strong focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-colors placeholder:text-text-muted"
      @input="onInput"
      @keydown.enter="emit('submit', modelValue)"
    />
    <div class="absolute right-2 flex items-center gap-1">
      <button
        v-if="modelValue"
        type="button"
        class="text-text-muted hover:text-text-primary p-0.5 rounded cursor-pointer"
        aria-label="Clear search"
        @click="clear"
      >
        <X class="w-3 h-3" />
      </button>
      <kbd
        v-if="shortcut && !modelValue"
        class="hidden sm:inline-flex items-center px-1.5 py-0.5 text-[10px] font-mono font-medium text-text-muted bg-surface-hover border border-border-subtle rounded cursor-pointer"
        @click="emit('shortcut')"
      >
        {{ shortcut }}
      </kbd>
    </div>
  </div>
</template>
