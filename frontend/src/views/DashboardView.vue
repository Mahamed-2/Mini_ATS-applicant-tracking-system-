<script setup lang="ts">
/**
 * DashboardView.vue – Framework Specification Dashboard Console (code.html conformance)
 * Source of Truth: docs/DESIGN.md & .claude/skills/ats-orchestrator/reference/SHELL_DASHBOARD.md
 * Integrates:
 * 1. FrameworkSpecBanner
 * 2. StoreInspectorCard & LayerBreakdownCard
 * 3. RouteSimulatorGrid
 * 4. ToastSandboxCard, CandidateMicroCardPattern, MultiTenantSwitcher
 * 5. ConformanceNotesGrid
 */
import { onMounted, computed, ref } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore } from '@/stores/auth';
import { useAtsStore } from '@/stores/ats';
import { useUiStore } from '@/stores/ui';
import { ShieldCheck, Star } from '@/lib/icons';

import FrameworkSpecBanner from '@/components/ui/FrameworkSpecBanner.vue';
import StoreInspectorCard from '@/components/ui/StoreInspectorCard.vue';
import LayerBreakdownCard from '@/components/ui/LayerBreakdownCard.vue';
import RouteSimulatorGrid from '@/components/ui/RouteSimulatorGrid.vue';
import ToastSandboxCard from '@/components/ui/ToastSandboxCard.vue';
import CandidateMicroCard from '@/components/ui/CandidateMicroCard.vue';
import MultiTenantSwitcher, { type TenantOption } from '@/components/ui/MultiTenantSwitcher.vue';
import ConformanceNotesGrid from '@/components/ui/ConformanceNotesGrid.vue';

const router = useRouter();
const authStore = useAuthStore();
const atsStore = useAtsStore();
const uiStore = useUiStore();

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

const actingAsCustomerName = computed(() => {
  if (authStore.actAsCustomerId) {
    const match = availableTenants.find((t) => t.id === authStore.actAsCustomerId);
    return match?.name || 'Nordic Tech AB';
  }
  return authStore.profile?.companyName || null;
});

// Load scoped jobs & candidates on mount
onMounted(async () => {
  const customerId = authStore.effectiveCustomerId || '22222222-2222-4222-8222-222222222222';
  await Promise.all([
    atsStore.loadJobs(customerId),
    atsStore.loadCandidates(customerId),
  ]);
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

// Sample live candidate pattern for Section 4
const featuredCandidate = computed(() => {
  const candidate = atsStore.candidates.find((c) => c.aiScore && c.aiScore >= 80) || atsStore.candidates[0];
  if (candidate) {
    return {
      id: candidate.id,
      fullName: candidate.fullName,
      email: candidate.email,
      linkedinUrl: candidate.linkedinUrl,
      stage: candidate.stage,
      jobTitle: atsStore.jobs.find((j) => j.id === candidate.jobId)?.title || 'Senior Frontend Engineer',
      aiScore: candidate.aiScore,
      summary: candidate.summary,
      timeInStage: '2d ago',
    };
  }
  return {
    id: '55555555-5555-4555-8555-555555555503',
    fullName: 'Maria Karlsson',
    email: 'maria.karlsson@example.com',
    linkedinUrl: 'https://www.linkedin.com/in/mini-ats-demo-maria-karlsson',
    stage: 'interview',
    jobTitle: 'Senior Frontend Engineer',
    aiScore: 86,
    summary: 'Advanced frontend candidate ready for interview.',
    timeInStage: '2d ago',
  };
});
</script>

<template>
  <div class="flex flex-col gap-4 pb-12">
    <!-- 1. Framework Specification Header Banner -->
    <FrameworkSpecBanner
      :acting-as-customer-name="actingAsCustomerName"
      :acting-as-customer-id="authStore.actAsCustomerId"
      @exit-scope="handleExitScope"
    />

    <!-- 2. Two-Column Architectural Row: Telemetry Store Inspector & Layer Breakdown -->
    <div class="grid grid-cols-1 lg:grid-cols-2 gap-4">
      <StoreInspectorCard
        :is-sidebar-collapsed="uiStore.isSidebarCollapsed"
        :is-mobile-nav-open="uiStore.isMobileNavOpen"
        :color-mode="uiStore.colorMode"
        :act-as-customer-id="authStore.actAsCustomerId"
        :current-user="authStore.profile ? {
          id: authStore.profile.id,
          email: authStore.profile.email,
          role: authStore.profile.role,
          companyName: authStore.profile.companyName ?? undefined,
        } : null"
        :last-api-status="authStore.lastApiStatus"
        :last-api-latency-ms="authStore.lastApiLatencyMs"
        @toggle-collapse="uiStore.toggleSidebar()"
        @reset="uiStore.reset()"
      />

      <LayerBreakdownCard current-route="/dashboard" />
    </div>

    <!-- 3. Core Route Simulator & Scoped Views (5 Module Cards) -->
    <RouteSimulatorGrid
      :jobs-count="atsStore.jobs.length"
      :candidates-count="atsStore.candidates.length"
      :top-ai-match-score="86"
      :active-stages-count="6"
      :is-admin="authStore.isAdmin"
      @navigate="handleNavigate"
    />

    <!-- 4. Three-Column Interactive Row: Sandbox, MicroCard Pattern, Tenant Switcher -->
    <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
      <!-- A: Toast Sandbox -->
      <ToastSandboxCard />

      <!-- B: Live CandidateMicroCard Pattern -->
      <div class="surface-1 bg-surface-card border border-border-subtle rounded-lg p-4 shadow-sm flex flex-col justify-between">
        <div>
          <div class="flex items-center justify-between pb-3 border-b border-border-subtle mb-3">
            <h3 class="text-xs font-semibold text-text-primary">
              Candidate Micro-Card Pattern
            </h3>
            <span class="text-[10px] font-mono text-text-muted">Surface 1</span>
          </div>

          <p class="text-xs text-text-muted mb-3 leading-relaxed">
            High-density Kanban card with Zap match indicator, avatar initials, and stage chip.
          </p>

          <CandidateMicroCard
            :candidate="featuredCandidate"
            :draggable="false"
            @click="handleNavigate(`/candidates/${featuredCandidate.id}`)"
          />
        </div>

        <div class="mt-4 pt-3 border-t border-border-subtle flex items-center justify-between text-[11px] text-text-muted">
          <span>Component: Ready</span>
          <span class="inline-flex items-center gap-1 text-[10px] font-semibold text-primary">
            <Star class="w-3 h-3 fill-primary" />
            Verified Pattern
          </span>
        </div>
      </div>

      <!-- C: Multi-Tenant Switcher Card -->
      <div class="surface-1 bg-surface-card border border-border-subtle rounded-lg p-4 shadow-sm flex flex-col justify-between">
        <div>
          <div class="flex items-center justify-between pb-3 border-b border-border-subtle mb-3">
            <h3 class="text-xs font-semibold text-text-primary">
              Multi-Tenant Impersonation
            </h3>
            <span class="text-[10px] font-mono text-primary font-semibold">Admin Scope</span>
          </div>

          <p class="text-xs text-text-muted mb-3 leading-relaxed">
            Select a tenant customer context to scope requisitions, Kanban stages, and candidate pools.
          </p>

          <MultiTenantSwitcher
            v-model="selectedTenantId"
            :tenants="availableTenants"
            :disabled="!authStore.isAdmin"
            @select="handleSelectTenant"
          />
        </div>

        <div class="mt-4 pt-3 border-t border-border-subtle flex items-center justify-between text-[11px] text-text-muted">
          <div class="flex items-center gap-1.5">
            <ShieldCheck class="w-3.5 h-3.5 text-success" />
            <span class="text-[10px] font-mono">Global Scope Barrier: Strict RBAC</span>
          </div>
        </div>
      </div>
    </div>

    <!-- 5. API & Architecture Conformance Notes (4 Columns) -->
    <ConformanceNotesGrid />
  </div>
</template>
