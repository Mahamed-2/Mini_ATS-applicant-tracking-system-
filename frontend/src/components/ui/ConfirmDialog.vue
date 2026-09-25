<script setup lang="ts">
/**
 * ConfirmDialog.vue – Confirmation Modal (Destructive = Rose Button)
 * Source of Truth: docs/DESIGN.md & .claude/skills/ats-orchestrator/reference/DESIGN_TOKENS.md
 */
import Dialog from './Dialog.vue';
import Button from './Button.vue';

interface Props {
  open: boolean;
  title: string;
  message: string;
  confirmText?: string;
  cancelText?: string;
  destructive?: boolean;
  loading?: boolean;
}

withDefaults(defineProps<Props>(), {
  confirmText: 'Confirm',
  cancelText: 'Cancel',
  destructive: false,
  loading: false,
});

const emit = defineEmits<{
  (e: 'update:open', val: boolean): void;
  (e: 'confirm'): void;
  (e: 'cancel'): void;
}>();

function onCancel() {
  emit('update:open', false);
  emit('cancel');
}

function onConfirm() {
  emit('confirm');
}
</script>

<template>
  <Dialog
    :open="open"
    :title="title"
    max-width="sm"
    @update:open="(val) => emit('update:open', val)"
    @close="onCancel"
  >
    <p class="text-xs text-text-variant leading-relaxed">
      {{ message }}
    </p>

    <template #footer>
      <Button
        variant="secondary"
        size="sm"
        :disabled="loading"
        @click="onCancel"
      >
        {{ cancelText }}
      </Button>
      <Button
        :variant="destructive ? 'danger' : 'primary'"
        size="sm"
        :loading="loading"
        @click="onConfirm"
      >
        {{ confirmText }}
      </Button>
    </template>
  </Dialog>
</template>
