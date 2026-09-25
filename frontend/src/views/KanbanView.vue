<script setup lang="ts">
/**
 * KanbanView.vue – 6-Stage High-Velocity ATS Recruitment Pipeline
 * Source of Truth: docs/DESIGN.md & .claude/skills/ats-orchestrator/reference/PAGE_RESKIN.md
 * Integrates:
 * - GET /api/candidates?customerId=... & GET /api/jobs
 * - PATCH /api/candidates/{id}/stage
 * - 6 Exact Stage Tokens: new, screening, interview, offer, hired, rejected
 * - Drag & Drop with lift shadow and rotate(2deg), disabled under prefers-reduced-motion
 */
import { ref, computed, onMounted } from 'vue';
import { useRouter, useRoute } from 'vue-router';
import { useAtsStore, type Candidate } from '@/stores/ats';
import { useAuthStore } from '@/stores/auth';
import { useToast } from '@/components/ui/useToast';
import SearchInput from '@/components/ui/SearchInput.vue';
import Select from '@/components/ui/Select.vue';
import Button from '@/components/ui/Button.vue';
import CandidateMicroCard, { type CandidateCardData } from '@/components/ui/CandidateMicroCard.vue';
import EmptyState from '@/components/ui/EmptyState.vue';
import {
  SquareKanban,
  RefreshCw,
  Plus,
  X
} from '@/lib/icons';
import { useI18n } from '@/i18n';

const router = useRouter();
const route = useRoute();
const atsStore = useAtsStore();
const authStore = useAuthStore();
const toast = useToast();
const { t } = useI18n();

const STAGES = computed(() => [
  { id: 'new', label: t('stage.applied'), dotColor: '#0284c7' },
  { id: 'screening', label: t('stage.phoneScreen'), dotColor: '#d97706' },
  { id: 'interview', label: t('stage.techInterview'), dotColor: '#1d68f0' },
  { id: 'offer', label: t('stage.offer'), dotColor: '#7c3aed' },
  { id: 'hired', label: t('stage.hired'), dotColor: '#059669' },
]);

// Toolbar filter state
const selectedJobId = ref<string>((route.query.jobId as string) || 'all');
const searchQuery = ref('');

// Drag & Drop state
const draggedCandidateId = ref<string | null>(null);
const dragOverStage = ref<string | null>(null);

const effectiveCustomerId = computed(() => {
  return authStore.actAsCustomerId || authStore.profile?.id || '';
});

onMounted(async () => {
  if (effectiveCustomerId.value) {
    await loadPipeline();
  }
});

async function loadPipeline() {
  await Promise.all([
    atsStore.loadJobs(effectiveCustomerId.value),
    atsStore.loadCandidates(effectiveCustomerId.value)
  ]);
}

// Jobs options for filter
const jobOptions = computed(() => [
  { label: t('kanban.allJobs'), value: 'all' },
  ...atsStore.jobs.map(j => ({ label: j.title, value: j.id }))
]);

// Map job ID to title
const jobMap = computed(() => {
  const map: Record<string, string> = {};
  for (const j of atsStore.jobs) {
    map[j.id] = j.title;
  }
  return map;
});

// Filtered candidates across all stages
const filteredCandidates = computed(() => {
  return atsStore.candidates.filter(c => {
    // Search query filter (name or email)
    const q = searchQuery.value.trim().toLowerCase();
    if (q) {
      const matchName = c.fullName.toLowerCase().includes(q);
      const matchRole = c.jobId && jobMap.value[c.jobId]?.toLowerCase().includes(q);
      if (!matchName && !matchRole) return false;
    }

    // Role filter
    if (selectedJobId.value !== 'all' && c.jobId !== selectedJobId.value) {
      return false;
    }

    return true;
  });
});

// Grouped candidates by stage
const stageColumns = computed(() => {
  return STAGES.value.map(stage => {
    const list = filteredCandidates.value.filter(c => c.stage.toLowerCase() === stage.id);
    return {
      ...stage,
      candidates: list
    };
  });
});

// Drag events
function onDragStart(e: DragEvent, candidateId: string) {
  draggedCandidateId.value = candidateId;
  if (e.dataTransfer) {
    e.dataTransfer.effectAllowed = 'move';
    e.dataTransfer.setData('text/plain', candidateId);
  }
}

function onDragEnd() {
  draggedCandidateId.value = null;
  dragOverStage.value = null;
}

function onDragOver(e: DragEvent, stageId: string) {
  e.preventDefault();
  dragOverStage.value = stageId;
}

function onDragLeave(stageId: string) {
  if (dragOverStage.value === stageId) {
    dragOverStage.value = null;
  }
}

async function onDrop(e: DragEvent, targetStage: string) {
  e.preventDefault();
  const candidateId = draggedCandidateId.value || e.dataTransfer?.getData('text/plain');
  dragOverStage.value = null;
  draggedCandidateId.value = null;

  if (!candidateId) return;

  const candidate = atsStore.candidates.find(c => c.id === candidateId);
  if (!candidate || candidate.stage === targetStage) return;

  await handleMoveStage(candidate, targetStage);
}

// Move stage with optimistic update & rollback
async function handleMoveStage(candidate: Candidate, targetStage: string) {
  const prevStage = candidate.stage;
  candidate.stage = targetStage; // Optimistic update

  try {
    await atsStore.moveStage(candidate.id, targetStage, effectiveCustomerId.value);
    toast.success(`Moved ${candidate.fullName} to ${targetStage.toUpperCase()}`);
  } catch (err) {
    candidate.stage = prevStage; // Rollback
    toast.error(`Stage move failed: ${(err as Error).message}`);
  }
}

function clearFilters() {
  selectedJobId.value = 'all';
  searchQuery.value = '';
}

function openCandidateDetail(cand: CandidateCardData) {
  router.push(`/candidates/${cand.id}`);
}

function openAddCandidate() {
  router.push('/candidates');
}
</script>

<template>
  <div class="h-full flex flex-col space-y-4">
    <!-- Pipeline Header & Filter Toolbar -->
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-3">
      <div>
        <div class="flex items-center gap-2">
          <h1 class="text-headline-lg font-bold text-text-primary tracking-tight">{{ t('kanban.title') }}</h1>
          <span class="px-2 py-0.5 rounded text-label-sm font-semibold tracking-wide bg-primary/10 text-primary border border-primary/20 tabular-nums">
            {{ t('kanban.activeCount', { count: filteredCandidates.length }) }}
          </span>
        </div>
        <p class="text-body-sm text-text-muted mt-0.5">
          {{ t('kanban.subtitle') }}
        </p>
      </div>

      <!-- Action & Filter controls -->
      <div class="flex flex-wrap items-center gap-2.5">
        <Select
          id="kanban-job-filter"
          name="kanbanJobFilter"
          v-model="selectedJobId"
          :options="jobOptions"
          class="w-52 text-xs"
        />

        <SearchInput
          id="kanban-search-filter"
          name="kanbanCandidateSearch"
          v-model="searchQuery"
          :placeholder="t('kanban.filterByName')"
          class="w-48 sm:w-56"
        />

        <Button
          v-if="selectedJobId !== 'all' || searchQuery"
          variant="ghost"
          size="sm"
          :title="t('kanban.clearFilters')"
          @click="clearFilters"
        >
          <template #iconLeft><X class="w-3.5 h-3.5" /></template>
          {{ t('kanban.clearFilters') }}
        </Button>

        <Button
          variant="secondary"
          size="sm"
          :title="t('kanban.refresh')"
          @click="loadPipeline"
        >
          <template #iconLeft><RefreshCw class="w-3.5 h-3.5" /></template>
          {{ t('kanban.refresh') }}
        </Button>

        <Button
          variant="primary"
          size="sm"
          @click="openAddCandidate"
        >
          <template #iconLeft><Plus class="w-3.5 h-3.5" /></template>
          {{ t('kanban.addCandidate') }}
        </Button>
      </div>
    </div>

    <!-- Empty Pipeline -->
    <div v-if="filteredCandidates.length === 0 && !searchQuery && selectedJobId === 'all'" class="py-16">
      <EmptyState
        title="Pipeline is currently empty"
        description="Add candidates or seed demo applicants to begin stage progression and AI assessment."
        action-text="Add Candidate"
        @action="openAddCandidate"
      >
        <template #icon><SquareKanban class="w-8 h-8 text-primary" /></template>
      </EmptyState>
    </div>

    <!-- 6-Stage Horizontal Scrolling Board -->
    <div v-else class="flex-1 overflow-x-auto pb-4 pt-1">
      <div class="flex items-start gap-4 min-w-[1600px] h-[calc(100vh-210px)]">
        <!-- Stage Column -->
        <div
          v-for="col in stageColumns"
          :key="col.id"
          :class="[
            'flex flex-col w-[260px] min-w-[260px] h-full rounded-lg surface-0 bg-surface-canvas border transition-colors select-none',
            dragOverStage === col.id ? 'border-primary ring-2 ring-primary/20 bg-primary/5' : 'border-border-subtle'
          ]"
          @dragover="onDragOver($event, col.id)"
          @dragleave="onDragLeave(col.id)"
          @drop="onDrop($event, col.id)"
        >
          <!-- Column Header -->
          <div class="p-3 border-b border-border-subtle flex items-center justify-between bg-surface-card rounded-t-lg">
            <div class="flex items-center gap-2">
              <span
                class="w-2.5 h-2.5 rounded-full shrink-0"
                :style="{ backgroundColor: col.dotColor }"
              />
              <span class="text-xs font-bold text-text-primary uppercase tracking-wider">
                {{ col.label }}
              </span>
            </div>
            <span class="px-2 py-0.5 rounded-full text-[11px] font-semibold bg-surface-canvas border border-border-subtle text-text-muted tabular-nums">
              {{ col.candidates.length }}
            </span>
          </div>

          <!-- Cards Scroll Zone -->
          <div class="flex-1 overflow-y-auto p-2.5 space-y-2.5">
            <!-- Drop placeholder if empty -->
            <div
              v-if="col.candidates.length === 0"
              class="h-28 rounded-md border-2 border-dashed border-border-subtle flex flex-col items-center justify-center text-text-muted text-xs p-3 text-center"
            >
              <span>Drop candidates here</span>
            </div>

            <!-- Candidate MicroCard -->
            <div
              v-for="cand in col.candidates"
              :key="cand.id"
              draggable="true"
              :class="[
                'transition-all duration-150',
                draggedCandidateId === cand.id ? 'opacity-40 scale-95 shadow-kanban-drag motion-safe:rotate-2 motion-reduce:rotate-0' : ''
              ]"
              @dragstart="onDragStart($event, cand.id)"
              @dragend="onDragEnd"
            >
              <CandidateMicroCard
                :candidate="{
                  id: cand.id,
                  fullName: cand.fullName,
                  email: cand.email,
                  linkedinUrl: cand.linkedinUrl,
                  stage: cand.stage,
                  jobTitle: cand.jobId ? jobMap[cand.jobId] : 'General',
                  summary: cand.summary,
                  aiScore: cand.aiScore,
                }"
                @click="openCandidateDetail"
              />

              <!-- Stage Quick Movers (Accessible click fallback for keyboard/mouse) -->
              <div class="flex items-center justify-between px-2 pt-1 text-[10px] text-text-muted select-none">
                <span class="text-[9px] uppercase font-mono tracking-wider">Move:</span>
                <div class="flex items-center gap-1">
                  <button
                    v-for="target in STAGES.filter(s => s.id !== col.id).slice(0, 3)"
                    :key="target.id"
                    type="button"
                    class="hover:text-primary transition-colors underline font-medium cursor-pointer"
                    :title="`Move directly to ${target.label}`"
                    @click="handleMoveStage(cand, target.id)"
                  >
                    {{ target.label[0] }}
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
