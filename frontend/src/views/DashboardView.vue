<script setup lang="ts">
/**
 * DashboardView.vue – Production Recruiter Velocity ATS Dashboard
 * Clean, high-velocity overview featuring:
 * 1. Recruiter KPI Summary (Total Candidates, Active Jobs, Interviewing, AI Match)
 * 2. AI Pipeline Intelligence & Copilot Analysis Report
 * 3. Priority Candidate Highlights & Fast Navigation
 * 4. Multi-Tenant Workspace Switching for Admins
 */
import { onMounted, computed, ref } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore } from '@/stores/auth';
import { useAtsStore } from '@/stores/ats';
import {
  Users,
  Briefcase,
  CheckCircle2,
  Sparkles,
  SquareKanban,
  Building2,
  X,
  ArrowRight,
  Plus
} from '@/lib/icons';

import StatCard from '@/components/ui/StatCard.vue';
import Button from '@/components/ui/Button.vue';
import CandidateMicroCard from '@/components/ui/CandidateMicroCard.vue';
import MultiTenantSwitcher, { type TenantOption } from '@/components/ui/MultiTenantSwitcher.vue';
import AiPipelineIntelligence from '@/components/ui/AiPipelineIntelligence.vue';
import { useI18n } from '@/i18n';

const router = useRouter();
const authStore = useAuthStore();
const atsStore = useAtsStore();
const { t } = useI18n();

// Customer tenant options for MultiTenantSwitcher
const availableTenants: TenantOption[] = [
  {
    id: '22222222-2222-4222-8222-222222222222',
    name: 'Nordic Tech AB',
    role: 'Customer',
    candidateCount: 10,
  },
  {
    id: '44444444-4444-4444-4444-444444444444',
    name: 'Stockholm Software AB',
    role: 'Customer',
    candidateCount: 0,
  },
];

const selectedTenantId = ref<string | null>(authStore.actAsCustomerId || '22222222-2222-4222-8222-222222222222');

const companyName = computed(() => {
  if (authStore.actAsCustomerId) {
    const match = availableTenants.find((t) => t.id === authStore.actAsCustomerId);
    return match?.name || 'Nordic Tech AB';
  }
  return authStore.profile?.companyName || 'Nordic Tech AB';
});

// Load scoped jobs & candidates on mount if not already primed
onMounted(async () => {
  const customerId = authStore.effectiveCustomerId || '22222222-2222-4222-8222-222222222222';
  if (atsStore.jobs.length === 0 || atsStore.candidates.length === 0) {
    await Promise.all([
      atsStore.loadJobs(customerId),
      atsStore.loadCandidates(customerId),
    ]);
  }
});

function handleExitScope() {
  authStore.setActAsCustomer(null);
  selectedTenantId.value = null;
  atsStore.loadJobs(authStore.effectiveCustomerId);
  atsStore.loadCandidates(authStore.effectiveCustomerId);
}

function handleSelectTenant(tenant: TenantOption) {
  authStore.setActAsCustomer(tenant.id);
  selectedTenantId.value = tenant.id;
  atsStore.loadJobs(tenant.id);
  atsStore.loadCandidates(tenant.id);
}

function handleNavigate(path: string) {
  router.push(path);
}

// Top match candidates
const topCandidates = computed(() => {
  return [...atsStore.candidates]
    .sort((a, b) => (b.aiScore ?? 0) - (a.aiScore ?? 0))
    .slice(0, 3)
    .map((c) => ({
      id: c.id,
      fullName: c.fullName,
      email: c.email,
      linkedinUrl: c.linkedinUrl,
      stage: c.stage,
      jobTitle: atsStore.jobs.find((j) => j.id === c.jobId)?.title || 'Role Requisition',
      aiScore: c.aiScore ?? 80,
      summary: c.summary,
      timeInStage: 'Active',
    }));
});

const interviewCount = computed(() => {
  return atsStore.candidates.filter((c) => ['interview', 'offer'].includes(c.stage)).length;
});

const topScore = computed(() => {
  const scores = atsStore.candidates.map((c) => c.aiScore ?? 0).filter((s) => s > 0);
  return scores.length > 0 ? Math.max(...scores) : 88;
});
</script>

<template>
  <div class="flex flex-col gap-6 pb-12">
    <!-- Header: Workspace Scope & Quick Actions -->
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
      <div>
        <div class="flex items-center gap-3">
          <h1 class="text-headline-lg font-bold text-text-primary tracking-tight">
            {{ t('dashboard.title') }}
          </h1>
          <span class="px-2.5 py-0.5 rounded-full text-label-sm font-semibold tracking-wide bg-primary/10 text-primary border border-primary/20">
            {{ companyName }}
          </span>
        </div>
        <p class="text-body-sm text-text-muted mt-1">
          {{ t('dashboard.subtitle', { name: authStore.profile?.displayName || 'Recruiter' }) }}
        </p>
      </div>

      <!-- Quick Action Buttons -->
      <div class="flex flex-wrap items-center gap-2.5">
        <div
          v-if="authStore.isAdmin && authStore.actAsCustomerId"
          class="flex items-center gap-1.5 px-3 py-1 bg-primary-subtle text-primary border border-primary/20 rounded-md text-xs font-medium"
        >
          <Building2 class="w-3.5 h-3.5" />
          <span>{{ t('dashboard.scope') }} <strong>{{ companyName }}</strong></span>
          <button
            type="button"
            class="ml-1 text-danger hover:opacity-80 p-0.5 rounded cursor-pointer"
            :title="t('dashboard.exitScope')"
            @click="handleExitScope"
          >
            <X class="w-3 h-3 stroke-[3]" />
          </button>
        </div>

        <Button
          variant="secondary"
          size="sm"
          @click="handleNavigate('/kanban')"
        >
          <template #iconLeft><SquareKanban class="w-4 h-4 text-primary" /></template>
          {{ t('nav.kanban') }}
        </Button>

        <Button
          variant="secondary"
          size="sm"
          @click="handleNavigate('/candidates')"
        >
          <template #iconLeft><Users class="w-4 h-4 text-emerald-500" /></template>
          {{ t('nav.candidates') }}
        </Button>

        <Button
          variant="primary"
          size="sm"
          @click="handleNavigate('/jobs')"
        >
          <template #iconLeft><Plus class="w-4 h-4" /></template>
          {{ t('nav.jobs') }}
        </Button>
      </div>
    </div>

    <!-- 1. Executive Metric KPI Cards (4 Stat Cards) -->
    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
      <StatCard
        :label="t('dashboard.pipelineCandidates')"
        :value="atsStore.candidates.length"
        :subtext="t('dashboard.pipelineCandidatesSub')"
        trend="+100%"
        :trend-positive="true"
        class="cursor-pointer hover:border-primary transition-colors"
        @click="handleNavigate('/candidates')"
      >
        <template #icon>
          <Users class="w-4 h-4 text-primary" />
        </template>
      </StatCard>

      <StatCard
        :label="t('dashboard.activeJobs')"
        :value="atsStore.jobs.length"
        :subtext="t('dashboard.activeJobsSub')"
        class="cursor-pointer hover:border-primary transition-colors"
        @click="handleNavigate('/jobs')"
      >
        <template #icon>
          <Briefcase class="w-4 h-4 text-text-muted" />
        </template>
      </StatCard>

      <StatCard
        :label="t('dashboard.interviewStage')"
        :value="interviewCount"
        :subtext="t('dashboard.interviewStageSub')"
        trend="High Velocity"
        :trend-positive="true"
        class="cursor-pointer hover:border-primary transition-colors"
        @click="handleNavigate('/kanban')"
      >
        <template #icon>
          <CheckCircle2 class="w-4 h-4 text-emerald-500" />
        </template>
      </StatCard>

      <StatCard
        :label="t('dashboard.topAiScore')"
        :value="`${topScore}%`"
        :subtext="t('dashboard.topAiScoreSub')"
        class="border-violet-200 dark:border-violet-900/60 bg-gradient-to-br from-violet-50/40 via-surface-card to-surface-card dark:from-violet-950/20"
      >
        <template #icon>
          <Sparkles class="w-4 h-4 text-violet-500" />
        </template>
      </StatCard>
    </div>

    <!-- 2. AI Pipeline Intelligence & Copilot Analysis Report -->
    <AiPipelineIntelligence />

    <!-- 3. Two-Column Operational Row: Top Matching Candidates + Workspace Controls -->
    <div class="grid grid-cols-1 lg:grid-cols-3 gap-6 items-start">
      <!-- Left 2 Cols: Priority Candidates -->
      <div class="lg:col-span-2 surface-1 bg-surface-card border border-border-subtle rounded-lg p-5 shadow-sm space-y-4">
        <div class="flex items-center justify-between pb-3 border-b border-border-subtle">
          <div>
            <h2 class="text-sm font-semibold text-text-primary">
              {{ t('dashboard.highMatchTitle') }}
            </h2>
            <p class="text-xs text-text-muted mt-0.5">
              {{ t('dashboard.highMatchSub') }}
            </p>
          </div>
          <Button
            variant="ghost"
            size="sm"
            @click="handleNavigate('/candidates')"
          >
            {{ t('dashboard.viewAll') }}
            <template #iconRight><ArrowRight class="w-3.5 h-3.5" /></template>
          </Button>
        </div>

        <div class="grid grid-cols-1 sm:grid-cols-3 gap-3">
          <CandidateMicroCard
            v-for="cand in topCandidates"
            :key="cand.id"
            :candidate="cand"
            :draggable="false"
            class="cursor-pointer"
            @click="handleNavigate(`/candidates/${cand.id}`)"
          />
        </div>
      </div>

      <!-- Right 1 Col: Tenant Management / Quick Overview -->
      <div class="surface-1 bg-surface-card border border-border-subtle rounded-lg p-5 shadow-sm space-y-4">
        <div class="flex items-center justify-between pb-3 border-b border-border-subtle">
          <div>
            <h2 class="text-sm font-semibold text-text-primary">
              {{ t('dashboard.workspaceTitle') }}
            </h2>
            <p class="text-xs text-text-muted mt-0.5">
              {{ t('dashboard.workspaceSub') }}
            </p>
          </div>
          <span
            v-if="authStore.isAdmin"
            class="px-2 py-0.5 rounded text-[10px] font-semibold bg-primary/10 text-primary border border-primary/20 uppercase tracking-wider"
          >
            Admin
          </span>
          <span
            v-else
            class="px-2 py-0.5 rounded text-[10px] font-semibold bg-emerald-50 text-emerald-700 border border-emerald-200 uppercase tracking-wider dark:bg-emerald-950/40 dark:text-emerald-400"
          >
            {{ t('common.role') }}: Customer
          </span>
        </div>

        <div v-if="authStore.isAdmin" class="space-y-3">
          <p class="text-xs text-text-muted leading-relaxed">
            {{ t('dashboard.switchOrgDesc') }}
          </p>
          <MultiTenantSwitcher
            v-model="selectedTenantId"
            :tenants="availableTenants"
            :disabled="!authStore.isAdmin"
            @select="handleSelectTenant"
          />
        </div>

        <div v-else class="space-y-3 text-xs text-text-muted">
          <div class="p-3 rounded-md bg-surface-canvas border border-border-subtle space-y-1.5">
            <div class="font-medium text-text-primary">{{ companyName }}</div>
            <div>{{ t('dashboard.isolatedNotice') }}</div>
          </div>
        </div>

        <div class="pt-3 border-t border-border-subtle flex items-center justify-between text-xs text-text-muted">
          <span>{{ t('dashboard.activeStages') }}</span>
          <span class="font-semibold text-text-primary tabular-nums">6 Stages</span>
        </div>
      </div>
    </div>
  </div>
</template>
