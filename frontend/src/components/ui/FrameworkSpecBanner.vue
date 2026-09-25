<script setup lang="ts">
/**
 * FrameworkSpecBanner.vue – Framework Specification Header Banner
 * Source of Truth: docs/DESIGN.md & .claude/skills/ats-orchestrator/reference/SHELL_DASHBOARD.md
 */
import { Terminal, ShieldCheck, X } from '@/lib/icons';
import Button from './Button.vue';

interface Props {
  actingAsCustomerName?: string | null;
  actingAsCustomerId?: string | null;
}

defineProps<Props>();

const emit = defineEmits<{
  (e: 'exitScope'): void;
}>();
</script>

<template>
  <div class="surface-1 bg-surface-card border border-border-subtle rounded-lg p-4 shadow-sm flex flex-col md:flex-row md:items-center justify-between gap-4">
    <div class="flex items-start gap-3">
      <div class="w-9 h-9 rounded-md bg-primary-subtle text-primary flex items-center justify-center shrink-0 mt-0.5 border border-primary/20">
        <Terminal class="w-5 h-5" />
      </div>
      <div>
        <div class="flex items-center gap-2">
          <span class="inline-flex items-center gap-1.5 px-2 py-0.5 bg-success-subtle text-success border border-success-border rounded-full text-[11px] font-semibold select-none">
            <span class="w-1.5 h-1.5 rounded-full bg-success animate-pulse" />
            <span>Store Sync Live</span>
          </span>
          <span class="text-xs font-mono text-text-muted">pinia::v2.1</span>
        </div>
        <h2 class="text-base font-bold text-text-primary mt-1">
          AppShell & Global Navigation Framework
        </h2>
        <p class="text-xs text-text-muted mt-0.5 max-w-2xl leading-relaxed">
          Deterministic route containment, 60/40 split-panes, multi-tenant Pinia context & audit portals.
        </p>
      </div>
    </div>

    <!-- Right Side: Act-as Context Chip + Exit Scope -->
    <div v-if="actingAsCustomerName" class="flex items-center gap-2 shrink-0 bg-primary-subtle/50 border border-primary/20 rounded-md p-2">
      <ShieldCheck class="w-4 h-4 text-primary shrink-0" />
      <div class="text-xs">
        <span class="text-text-muted">Scope: </span>
        <span class="font-semibold text-primary">{{ actingAsCustomerName }}</span>
        <span v-if="actingAsCustomerId" class="text-[10px] font-mono text-text-muted ml-1">
          ({{ actingAsCustomerId.slice(0, 8) }}...)
        </span>
      </div>
      <Button
        variant="ghost"
        size="sm"
        class="h-6 px-1.5 text-xs text-danger hover:bg-danger-subtle ml-1"
        @click="emit('exitScope')"
      >
        <template #icon-left>
          <X class="w-3 h-3" />
        </template>
        Exit Scope
      </Button>
    </div>
  </div>
</template>
