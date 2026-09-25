<script setup lang="ts">
/**
 * RouteSimulatorGrid.vue – Core Route Simulator & Scoped Views Grid
 * Source of Truth: docs/DESIGN.md & .claude/skills/ats-orchestrator/reference/SHELL_DASHBOARD.md
 */
import {
  LayoutDashboard,
  Briefcase,
  Users,
  SquareKanban,
  ShieldCheck,
  ArrowRight
} from '@/lib/icons';
import Button from './Button.vue';

interface RouteModule {
  id: string;
  title: string;
  path: string;
  description: string;
  icon: any;
  metric1Label: string;
  metric1Value: string | number;
  metric2Label: string;
  metric2Value: string | number;
}

interface Props {
  jobsCount?: number;
  candidatesCount?: number;
  topAiMatchScore?: number | null;
  activeStagesCount?: number;
  isAdmin?: boolean;
}

const props = withDefaults(defineProps<Props>(), {
  jobsCount: 2,
  candidatesCount: 10,
  topAiMatchScore: 86,
  activeStagesCount: 6,
  isAdmin: true,
});

const emit = defineEmits<{
  (e: 'navigate', path: string): void;
}>();

const modules: RouteModule[] = [
  {
    id: 'dashboard',
    title: 'Dashboard Console',
    path: '/',
    description: 'System telemetry, Pinia state inspector, architecture breakdown and AI reports.',
    icon: LayoutDashboard,
    metric1Label: 'Framework Status',
    metric1Value: 'Live',
    metric2Label: 'Telemetry Sync',
    metric2Value: 'Active',
  },
  {
    id: 'jobs',
    title: 'Jobs Manager',
    path: '/jobs',
    description: 'Requisition lifecycle, tenant job openings, status pills, and candidate tallies.',
    icon: Briefcase,
    metric1Label: 'Active Requisitions',
    metric1Value: props.jobsCount,
    metric2Label: 'Pipeline Scoped',
    metric2Value: 'Enabled',
  },
  {
    id: 'candidates',
    title: 'Candidate Index',
    path: '/candidates',
    description: 'High-density directory with avatar initials, LinkedIn verification, and AI match badges.',
    icon: Users,
    metric1Label: 'In Pipeline',
    metric1Value: props.candidatesCount,
    metric2Label: 'Top AI Match',
    metric2Value: props.topAiMatchScore ? `${props.topAiMatchScore}%` : '—',
  },
  {
    id: 'kanban',
    title: 'Pipeline Board',
    path: '/kanban',
    description: 'Interactive 6-stage Kanban board with instant multi-filter search and drag transitions.',
    icon: SquareKanban,
    metric1Label: 'Active Stages',
    metric1Value: props.activeStagesCount,
    metric2Label: 'Evaluated Cards',
    metric2Value: '4 / 10',
  },
  {
    id: 'admin',
    title: 'System Admin',
    path: '/admin',
    description: 'Multi-tenant provisioning, customer impersonation ("Act As"), and global accounts.',
    icon: ShieldCheck,
    metric1Label: 'Access Level',
    metric1Value: props.isAdmin ? 'Admin' : 'Restricted',
    metric2Label: 'Audit Guard',
    metric2Value: 'Strict RBAC',
  },
];
</script>

<template>
  <div class="mt-4">
    <div class="flex items-center justify-between mb-3">
      <div class="flex items-center gap-2">
        <h3 class="text-sm font-bold text-text-primary">
          Core Route Simulator & Scoped Views
        </h3>
        <span class="text-[11px] font-mono text-text-muted px-2 py-0.5 bg-surface-card border border-border-subtle rounded-full">
          5 Modules Configured
        </span>
      </div>
    </div>

    <!-- 5 Module Cards Grid -->
    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-5 gap-3">
      <div
        v-for="mod in modules"
        :key="mod.id"
        class="surface-1 bg-surface-card border border-border-subtle rounded-lg p-3.5 flex flex-col justify-between hover:border-border-strong hover:shadow-card transition-all"
      >
        <div>
          <div class="flex items-center justify-between mb-2">
            <div class="w-7 h-7 rounded-sm bg-primary-subtle text-primary flex items-center justify-center border border-primary/20">
              <component :is="mod.icon" class="w-4 h-4" />
            </div>
            <span class="font-mono text-[10px] text-text-muted bg-surface-hover px-1.5 py-0.5 rounded border border-border-subtle">
              {{ mod.path }}
            </span>
          </div>

          <h4 class="text-xs font-bold text-text-primary">
            {{ mod.title }}
          </h4>
          <p class="text-[11px] text-text-muted mt-1 leading-snug line-clamp-2">
            {{ mod.description }}
          </p>

          <div class="mt-3 pt-2.5 border-t border-border-subtle/50 flex flex-col gap-1 text-[10px]">
            <div class="flex justify-between text-text-muted">
              <span>{{ mod.metric1Label }}</span>
              <span class="font-semibold text-text-primary tabular-nums">{{ mod.metric1Value }}</span>
            </div>
            <div class="flex justify-between text-text-muted">
              <span>{{ mod.metric2Label }}</span>
              <span class="font-semibold text-text-primary tabular-nums">{{ mod.metric2Value }}</span>
            </div>
          </div>
        </div>

        <div class="mt-3 pt-2">
          <Button
            variant="secondary"
            size="sm"
            full-width
            class="h-7 text-[11px]"
            @click="emit('navigate', mod.path)"
          >
            <span>Launch View</span>
            <template #icon-right>
              <ArrowRight class="w-3 h-3" />
            </template>
          </Button>
        </div>
      </div>
    </div>
  </div>
</template>
