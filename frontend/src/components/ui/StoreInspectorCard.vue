<script setup lang="ts">
/**
 * StoreInspectorCard.vue – Reactive Pinia Store Telemetry & Inspector
 * Source of Truth: docs/DESIGN.md & .claude/skills/ats-orchestrator/reference/SHELL_DASHBOARD.md
 */
import { Braces } from '@/lib/icons';
import Button from './Button.vue';

interface Props {
  isSidebarCollapsed: boolean;
  isMobileNavOpen: boolean;
  colorMode: string;
  actAsCustomerId?: string | null;
  currentUser?: {
    id?: string;
    email?: string;
    role?: string;
    companyName?: string;
  } | null;
  lastApiStatus?: string;
  lastApiLatencyMs?: number;
}

withDefaults(defineProps<Props>(), {
  lastApiStatus: '200 OK',
  lastApiLatencyMs: 18,
});

const emit = defineEmits<{
  (e: 'toggleCollapse'): void;
  (e: 'reset'): void;
}>();
</script>

<template>
  <div class="surface-1 bg-surface-card border border-border-subtle rounded-lg p-4 shadow-sm flex flex-col justify-between">
    <div>
      <div class="flex items-center justify-between pb-3 border-b border-border-subtle">
        <div class="flex items-center gap-2">
          <Braces class="w-4 h-4 text-primary" />
          <h3 class="text-xs font-mono font-bold text-text-primary">
            useAppShellStore()
          </h3>
        </div>
        <span class="text-[10px] font-mono px-1.5 py-0.5 bg-surface-hover rounded text-text-muted border border-border-subtle">
          pinia::v2.1
        </span>
      </div>

      <!-- State telemetry key/value rows -->
      <div class="mt-3 flex flex-col gap-1.5 font-mono text-[11px]">
        <div class="flex justify-between py-0.5 border-b border-border-subtle/40">
          <span class="text-text-muted">isSidebarCollapsed</span>
          <span class="text-primary font-semibold">{{ isSidebarCollapsed }}</span>
        </div>
        <div class="flex justify-between py-0.5 border-b border-border-subtle/40">
          <span class="text-text-muted">isMobileNavOpen</span>
          <span class="text-text-primary">{{ isMobileNavOpen }}</span>
        </div>
        <div class="flex justify-between py-0.5 border-b border-border-subtle/40">
          <span class="text-text-muted">colorMode</span>
          <span class="text-success font-semibold">"{{ colorMode }}"</span>
        </div>
        <div class="flex justify-between py-0.5 border-b border-border-subtle/40">
          <span class="text-text-muted">actAsCustomerId</span>
          <span class="text-text-variant truncate max-w-[140px]">
            {{ actAsCustomerId ? `"${actAsCustomerId.slice(0, 8)}..."` : 'null' }}
          </span>
        </div>
        <div class="flex justify-between py-0.5">
          <span class="text-text-muted">currentUser</span>
          <span class="text-text-variant">
            {{ currentUser?.role ? `{ role: "${currentUser.role}", company: "${currentUser.companyName || 'Nordic'}" }` : 'null' }}
          </span>
        </div>
      </div>
    </div>

    <!-- Footer: Live Latency + Actions -->
    <div class="mt-4 pt-3 border-t border-border-subtle flex flex-col sm:flex-row items-center justify-between gap-2">
      <span class="text-[10px] font-mono text-text-muted bg-surface-hover px-2 py-0.5 rounded border border-border-subtle">
        GET /api/account/me {{ lastApiStatus }} ({{ lastApiLatencyMs }}ms)
      </span>
      <div class="flex items-center gap-1.5 w-full sm:w-auto">
        <Button variant="secondary" size="sm" class="flex-1 sm:flex-initial text-xs" @click="emit('toggleCollapse')">
          Toggle Collapse State
        </Button>
        <Button variant="ghost" size="sm" class="text-xs text-text-muted hover:text-text-primary" @click="emit('reset')">
          Reset
        </Button>
      </div>
    </div>
  </div>
</template>
