<script setup lang="ts">
/**
 * ToastSandboxCard.vue – Toast Interactive Sandbox Card
 * Source of Truth: docs/DESIGN.md & .claude/skills/ats-orchestrator/reference/SHELL_DASHBOARD.md
 */
import { BellRing, CheckCircle2, Sparkles, Plus } from '@/lib/icons';
import Button from './Button.vue';
import { useToast } from './useToast';

const { toast } = useToast();

const sampleMessages = [
  { title: 'Stage updated to Screening', description: 'Candidate moved to screening queue', type: 'success' as const },
  { title: 'AI Matching Complete', description: 'Assessed keyword alignment (86% match)', type: 'ai' as const },
  { title: 'Requisition Created', description: 'Senior Frontend Engineer opening published', type: 'info' as const },
];

let counter = 0;

function spawnToast() {
  const sample = sampleMessages[counter % sampleMessages.length];
  counter++;
  toast(sample);
}
</script>

<template>
  <div class="surface-1 bg-surface-card border border-border-subtle rounded-lg p-4 shadow-sm flex flex-col justify-between">
    <div>
      <div class="flex items-center justify-between pb-3 border-b border-border-subtle">
        <div class="flex items-center gap-2">
          <BellRing class="w-4 h-4 text-primary" />
          <h3 class="text-xs font-semibold text-text-primary">
            Notification System Sandbox
          </h3>
        </div>
        <span class="text-[10px] font-mono text-text-muted">aria-live polite</span>
      </div>

      <p class="text-xs text-text-muted mt-2 leading-relaxed">
        Non-blocking portal teleports outside layout clipping with auto-dismiss timers.
      </p>

      <!-- Sample Toast Previews -->
      <div class="mt-3 flex flex-col gap-2">
        <div class="p-2 bg-surface-hover/70 border border-border-subtle rounded-sm flex items-start gap-2 text-xs">
          <CheckCircle2 class="w-3.5 h-3.5 text-success shrink-0 mt-0.5" />
          <div class="min-w-0 flex-1">
            <span class="font-medium text-text-primary block text-[11px]">Stage updated to Screening</span>
            <span class="text-[10px] text-text-muted">now</span>
          </div>
        </div>

        <div class="p-2 bg-ai-subtle/50 border border-ai-border/40 rounded-sm flex items-start gap-2 text-xs">
          <Sparkles class="w-3.5 h-3.5 text-ai-accent shrink-0 mt-0.5" />
          <div class="min-w-0 flex-1">
            <span class="font-medium text-ai-accent block text-[11px]">AI Matching Complete</span>
            <span class="text-[10px] text-text-muted">2m ago</span>
          </div>
        </div>
      </div>
    </div>

    <div class="mt-4 pt-3 border-t border-border-subtle">
      <Button
        variant="secondary"
        size="sm"
        full-width
        class="h-7 text-xs"
        @click="spawnToast"
      >
        <template #icon-left>
          <Plus class="w-3 h-3" />
        </template>
        Spawn New Toast
      </Button>
    </div>
  </div>
</template>
