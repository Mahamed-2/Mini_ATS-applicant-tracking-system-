<script setup lang="ts">
/**
 * ToastPortal.vue – Sonner-Style Toast Portal with Aria-Live Polite
 * Source of Truth: docs/DESIGN.md & .claude/skills/ats-orchestrator/reference/DESIGN_TOKENS.md
 */
import { useToast } from './useToast';
import { CheckCircle2, AlertTriangle, Sparkles, X } from '@/lib/icons';

const { toasts, dismiss } = useToast();
</script>

<template>
  <Teleport to="body">
    <div
      class="fixed bottom-4 right-4 z-50 flex flex-col gap-2 max-w-sm w-full pointer-events-none"
      aria-live="polite"
    >
      <TransitionGroup
        enter-active-class="transition duration-200 ease-out"
        enter-from-class="transform translate-y-2 opacity-0"
        enter-to-class="transform translate-y-0 opacity-100"
        leave-active-class="transition duration-150 ease-in"
        leave-from-class="opacity-100"
        leave-to-class="opacity-0 scale-95"
      >
        <div
          v-for="t in toasts"
          :key="t.id"
          class="surface-2 bg-surface-modal border border-border-subtle rounded-lg shadow-modal p-3 pointer-events-auto flex items-start gap-2.5"
        >
          <div class="mt-0.5 shrink-0">
            <CheckCircle2 v-if="t.type === 'success'" class="w-4 h-4 text-success" />
            <Sparkles v-else-if="t.type === 'ai'" class="w-4 h-4 text-ai-accent" />
            <AlertTriangle v-else-if="t.type === 'danger'" class="w-4 h-4 text-danger" />
            <AlertTriangle v-else-if="t.type === 'warning'" class="w-4 h-4 text-warning" />
            <CheckCircle2 v-else class="w-4 h-4 text-primary" />
          </div>
          <div class="flex-1 min-w-0">
            <h5 class="text-xs font-semibold text-text-primary leading-tight">
              {{ t.title }}
            </h5>
            <p v-if="t.description" class="text-[11px] text-text-muted mt-0.5 leading-normal">
              {{ t.description }}
            </p>
          </div>
          <button
            type="button"
            class="text-text-muted hover:text-text-primary p-0.5 rounded cursor-pointer shrink-0"
            aria-label="Dismiss toast"
            @click="dismiss(t.id)"
          >
            <X class="w-3.5 h-3.5" />
          </button>
        </div>
      </TransitionGroup>
    </div>
  </Teleport>
</template>
