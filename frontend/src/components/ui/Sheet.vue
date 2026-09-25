<script setup lang="ts">
/**
 * Sheet.vue – Slide-Over Drawer with Teleport
 * Source of Truth: docs/DESIGN.md & .claude/skills/ats-orchestrator/reference/DESIGN_TOKENS.md
 */
import { watch, onMounted, onUnmounted } from 'vue';
import { X } from '@/lib/icons';
import IconButton from './IconButton.vue';

interface Props {
  open: boolean;
  side?: 'left' | 'right';
  title?: string;
}

const props = withDefaults(defineProps<Props>(), {
  open: false,
  side: 'right',
});

const emit = defineEmits<{
  (e: 'update:open', val: boolean): void;
  (e: 'close'): void;
}>();

function close() {
  emit('update:open', false);
  emit('close');
}

function onKeydown(e: KeyboardEvent) {
  if (e.key === 'Escape' && props.open) {
    close();
  }
}

onMounted(() => window.addEventListener('keydown', onKeydown));
onUnmounted(() => window.removeEventListener('keydown', onKeydown));

watch(() => props.open, (isOpen) => {
  if (isOpen) {
    document.body.style.overflow = 'hidden';
  } else {
    document.body.style.overflow = '';
  }
});
</script>

<template>
  <Teleport to="body">
    <div
      v-if="open"
      class="fixed inset-0 z-50 flex bg-slate-900/40 backdrop-blur-sm transition-opacity"
      :class="side === 'left' ? 'justify-start' : 'justify-end'"
      @click.self="close"
    >
      <div
        role="dialog"
        aria-modal="true"
        class="surface-2 bg-surface-modal border-border-subtle w-full max-w-sm h-full shadow-modal flex flex-col animate-in duration-200"
        :class="side === 'left' ? 'border-r slide-in-from-left' : 'border-l slide-in-from-right'"
      >
        <div class="flex items-center justify-between px-4 py-3 border-b border-border-subtle bg-surface-card">
          <h3 class="text-sm font-semibold text-text-primary">
            {{ title }}
          </h3>
          <IconButton aria-label="Close drawer" size="sm" @click="close">
            <X class="w-4 h-4" />
          </IconButton>
        </div>
        <div class="p-4 overflow-y-auto flex-1">
          <slot />
        </div>
      </div>
    </div>
  </Teleport>
</template>
