<script setup lang="ts">
/**
 * MultiTenantSwitcher.vue – Customer Scoping Radio Switcher for Admin
 * Source of Truth: docs/DESIGN.md & .claude/skills/ats-orchestrator/reference/DESIGN_TOKENS.md
 */
import { CircleDot, Circle } from '@/lib/icons';

export interface TenantOption {
  id: string;
  name: string;
  role?: string;
  candidateCount?: number;
}

interface Props {
  modelValue?: string | null;
  tenants: TenantOption[];
  disabled?: boolean;
}

const props = withDefaults(defineProps<Props>(), {
  modelValue: null,
  disabled: false,
});

const emit = defineEmits<{
  (e: 'update:modelValue', tenantId: string | null): void;
  (e: 'select', tenant: TenantOption): void;
}>();

function selectTenant(tenant: TenantOption) {
  if (props.disabled) return;
  emit('update:modelValue', tenant.id);
  emit('select', tenant);
}
</script>

<template>
  <div class="flex flex-col gap-1.5 w-full">
    <div
      v-for="t in tenants"
      :key="t.id"
      :class="[
        'flex items-center justify-between p-2.5 rounded-sm border transition-all cursor-pointer select-none text-xs',
        modelValue === t.id
          ? 'bg-primary-subtle/40 border-primary text-text-primary font-medium'
          : 'bg-surface-card border-border-subtle text-text-variant hover:border-border-strong hover:bg-surface-hover',
        disabled ? 'opacity-50 pointer-events-none' : '',
      ]"
      @click="selectTenant(t)"
    >
      <div class="flex items-center gap-2 min-w-0">
        <CircleDot v-if="modelValue === t.id" class="w-4 h-4 text-primary shrink-0" />
        <Circle v-else class="w-4 h-4 text-text-muted shrink-0" />
        <span class="truncate">{{ t.name }}</span>
      </div>

      <div class="flex items-center gap-1.5 shrink-0">
        <span
          v-if="t.candidateCount !== undefined"
          class="text-[11px] tabular-nums text-text-muted px-1.5 py-0.5 bg-surface-hover rounded"
        >
          {{ t.candidateCount }} candidates
        </span>
      </div>
    </div>
  </div>
</template>
