<script setup lang="ts">
/**
 * Dialog.vue – Modal Dialog with Surface 2 Elevation & Teleport
 * Source of Truth: docs/DESIGN.md & .claude/skills/ats-orchestrator/reference/DESIGN_TOKENS.md
 */
import { watch, onMounted, onUnmounted } from 'vue';
import { X } from '@/lib/icons';
import IconButton from './IconButton.vue';

interface Props {
  open: boolean;
  title?: string;
  description?: string;
  maxWidth?: 'sm' | 'md' | 'lg' | 'xl';
}

const props = withDefaults(defineProps<Props>(), {
  open: false,
  maxWidth: 'md',
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
      class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-900/40 backdrop-blur-sm transition-opacity"
      @click.self="close"
    >
      <div
        role="dialog"
        aria-modal="true"
        :class="[
          'surface-2 bg-surface-modal border border-border-subtle rounded-lg shadow-modal w-full overflow-hidden flex flex-col max-h-[90vh] animate-in fade-in zoom-in-95 duration-150',
          maxWidth === 'sm' ? 'max-w-sm' : maxWidth === 'lg' ? 'max-w-2xl' : maxWidth === 'xl' ? 'max-w-4xl' : 'max-w-md',
        ]"
      >
        <div class="flex items-center justify-between px-5 py-3.5 border-b border-border-subtle bg-surface-card">
          <div>
            <h3 class="text-sm font-semibold text-text-primary">
              {{ title }}
            </h3>
            <p v-if="description" class="text-xs text-text-muted mt-0.5">
              {{ description }}
            </p>
          </div>
          <IconButton aria-label="Close dialog" size="sm" @click="close">
            <X class="w-4 h-4" />
          </IconButton>
        </div>

        <div class="p-5 overflow-y-auto">
          <slot />
        </div>

        <div v-if="$slots.footer" class="px-5 py-3 border-t border-border-subtle bg-surface-hover/50 flex items-center justify-end gap-2">
          <slot name="footer" />
        </div>
      </div>
    </div>
  </Teleport>
</template>
