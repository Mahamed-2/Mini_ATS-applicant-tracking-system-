<script setup lang="ts">
/**
 * CandidateDetailView.vue – 60/40 Master-Detail Candidate Scorecard & AI Inspection
 * Source of Truth: docs/DESIGN.md & .claude/skills/ats-orchestrator/reference/PAGE_RESKIN.md
 * Integrates:
 * - GET /api/candidates/{id}?customerId=...
 * - PATCH /api/candidates/{id}/stage
 * - POST /api/ai/candidates/{id}/assess (live AI evaluation)
 * - AiAssessmentPanel & CandidateProfileCard
 */
import { ref, computed, onMounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useAtsStore, type Candidate } from '@/stores/ats';
import { useAuthStore } from '@/stores/auth';
import { useToast } from '@/components/ui/useToast';
import StageChip from '@/components/ui/StageChip.vue';
import ScoreBadge from '@/components/ui/ScoreBadge.vue';
import Button from '@/components/ui/Button.vue';
import Select from '@/components/ui/Select.vue';
import AiAssessmentPanel from '@/components/AiAssessmentPanel.vue';
import EmptyState from '@/components/ui/EmptyState.vue';
import ConfirmDialog from '@/components/ui/ConfirmDialog.vue';
import {
  ArrowLeft,
  Mail,
  Linkedin,
  Briefcase,
  Calendar,
  Sparkles,
  Trash2,
  ExternalLink,
  FileText
} from '@/lib/icons';

const route = useRoute();
const router = useRouter();
const atsStore = useAtsStore();
const authStore = useAuthStore();
const toast = useToast();

const candidateId = computed(() => route.params.id as string);
const isAssessing = ref(false);
const assessError = ref<string | null>(null);

// Delete dialog
const isDeleteDialogOpen = ref(false);
const isDeleting = ref(false);

const STAGES = [
  { label: 'New', value: 'new' },
  { label: 'Screening', value: 'screening' },
  { label: 'Interview', value: 'interview' },
  { label: 'Offer', value: 'offer' },
  { label: 'Hired', value: 'hired' },
  { label: 'Rejected', value: 'rejected' },
];

const effectiveCustomerId = computed(() => {
  return authStore.actAsCustomerId || authStore.profile?.id || '';
});

// Load candidate details
onMounted(async () => {
  if (effectiveCustomerId.value) {
    if (atsStore.candidates.length === 0) {
      await atsStore.loadCandidates(effectiveCustomerId.value);
    }
    if (atsStore.jobs.length === 0) {
      await atsStore.loadJobs(effectiveCustomerId.value);
    }
  }
});

const candidate = computed<Candidate | null>(() => {
  return atsStore.candidates.find(c => c.id === candidateId.value) || null;
});

const job = computed(() => {
  if (!candidate.value?.jobId) return null;
  return atsStore.jobs.find(j => j.id === candidate.value!.jobId) || null;
});

// Run AI assessment on camera
async function runAssessment() {
  if (!candidate.value) return;
  isAssessing.value = true;
  assessError.value = null;

  try {
    await atsStore.assessCandidate(candidate.value.id, effectiveCustomerId.value);
    toast.success(`AI assessment completed for ${candidate.value.fullName}!`);
  } catch (err) {
    assessError.value = (err as Error).message;
    toast.error(`Assessment failed: ${assessError.value}`);
  } finally {
    isAssessing.value = false;
  }
}

// Stage change
async function handleStageChange(newStage: string) {
  if (!candidate.value) return;
  try {
    await atsStore.moveStage(candidate.value.id, newStage, effectiveCustomerId.value);
    toast.success(`Stage updated to ${newStage.toUpperCase()}`);
  } catch (err) {
    toast.error(`Failed to change stage: ${(err as Error).message}`);
  }
}

// Delete candidate
async function handleDelete() {
  if (!candidate.value) return;
  isDeleting.value = true;
  try {
    const idx = atsStore.candidates.findIndex(c => c.id === candidate.value!.id);
    if (idx !== -1) {
      atsStore.candidates.splice(idx, 1);
    }
    toast.success('Candidate record deleted.');
    router.push('/candidates');
  } catch (err) {
    toast.error(`Delete failed: ${(err as Error).message}`);
  } finally {
    isDeleting.value = false;
    isDeleteDialogOpen.value = false;
  }
}

function formatDate(dateStr: string) {
  try {
    return new Date(dateStr).toLocaleDateString('sv-SE', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
  } catch {
    return dateStr;
  }
}
</script>

<template>
  <div v-if="!candidate" class="py-16">
    <EmptyState
      title="Candidate not found"
      description="The candidate record may have been removed or does not belong to the current customer scope."
      action-text="Return to Candidates"
      @action="router.push('/candidates')"
    />
  </div>

  <div v-else class="space-y-6">
    <!-- Sticky Scorecard Header Bar -->
    <div class="sticky top-0 z-20 -mx-4 -mt-4 px-4 py-3 sm:-mx-6 sm:-mt-6 sm:px-6 bg-surface-canvas/90 backdrop-blur-md border-b border-border-subtle flex flex-col sm:flex-row sm:items-center justify-between gap-3 shadow-xs">
      <div class="flex items-center gap-3">
        <Button
          variant="ghost"
          size="sm"
          title="Return to candidates roster"
          @click="router.back()"
        >
          <template #iconLeft><ArrowLeft class="w-4 h-4" /></template>
          Back
        </Button>

        <div class="h-4 w-px bg-border-subtle" />

        <div class="flex items-center gap-2.5">
          <h1 class="text-headline-sm font-bold text-text-primary tracking-tight truncate">
            {{ candidate.fullName }}
          </h1>
          <StageChip :stage="candidate.stage" size="sm" />
          <ScoreBadge v-if="candidate.aiScore !== null && candidate.aiScore !== undefined" :score="candidate.aiScore" />
        </div>
      </div>

      <!-- Action Buttons -->
      <div class="flex items-center gap-2 self-end sm:self-center">
        <Button
          variant="ai"
          size="sm"
          :loading="isAssessing"
          :disabled="isAssessing"
          @click="runAssessment"
        >
          <template #iconLeft><Sparkles class="w-3.5 h-3.5" /></template>
          {{ candidate.aiFeedback ? 'Re-assess CV' : 'Assess CV with AI' }}
        </Button>

        <Button
          variant="ghost"
          size="sm"
          class="text-danger hover:text-danger hover:bg-rose-50 dark:hover:bg-rose-950/40"
          title="Delete candidate record"
          @click="isDeleteDialogOpen = true"
        >
          <Trash2 class="w-4 h-4" />
        </Button>
      </div>
    </div>

    <!-- 60/40 Master-Detail Split Grid -->
    <div class="grid grid-cols-1 lg:grid-cols-12 gap-6 items-start">
      <!-- Left Column (40% on desktop): Candidate Profile & Stage Controller -->
      <div class="lg:col-span-5 space-y-4">
        <!-- Contact & Profile Card -->
        <div class="surface-1 bg-surface-card border border-border-subtle rounded-lg shadow-sm p-5 space-y-4">
          <div class="flex items-center gap-3 pb-3 border-b border-border-subtle">
            <div class="w-11 h-11 rounded-full bg-primary/10 border border-primary/20 text-primary font-bold text-sm flex items-center justify-center">
              {{ candidate.fullName.split(' ').map(n => n[0]).join('').slice(0, 2) }}
            </div>
            <div>
              <h3 class="text-sm font-bold text-text-primary">{{ candidate.fullName }}</h3>
              <p class="text-xs text-text-muted">{{ job?.title || 'Applicant Record' }}</p>
            </div>
          </div>

          <!-- Stage Controller -->
          <div class="space-y-1.5">
            <label class="text-[11px] font-semibold uppercase tracking-wider text-text-muted">
              Pipeline Stage
            </label>
            <Select
              :model-value="candidate.stage"
              :options="STAGES"
              @update:model-value="handleStageChange($event)"
            />
          </div>

          <!-- Contact Channels -->
          <div class="space-y-2 pt-2 border-t border-border-subtle text-xs">
            <div class="flex items-center justify-between py-1">
              <span class="text-text-muted flex items-center gap-1.5">
                <Mail class="w-3.5 h-3.5" />
                Email
              </span>
              <a
                v-if="candidate.email"
                :href="`mailto:${candidate.email}`"
                class="text-text-primary hover:text-primary font-medium truncate max-w-[200px]"
              >
                {{ candidate.email }}
              </a>
              <span v-else class="text-text-muted">—</span>
            </div>

            <div class="flex items-center justify-between py-1">
              <span class="text-text-muted flex items-center gap-1.5">
                <Linkedin class="w-3.5 h-3.5" />
                LinkedIn
              </span>
              <a
                v-if="candidate.linkedinUrl"
                :href="candidate.linkedinUrl"
                target="_blank"
                rel="noopener noreferrer"
                class="text-[#0a66c2] hover:underline flex items-center gap-1 font-medium"
              >
                <span>View Profile</span>
                <ExternalLink class="w-3 h-3" />
              </a>
              <span v-else class="text-text-muted text-[11px]">Not provided</span>
            </div>

            <div class="flex items-center justify-between py-1">
              <span class="text-text-muted flex items-center gap-1.5">
                <Briefcase class="w-3.5 h-3.5" />
                Applied Requisition
              </span>
              <span class="text-text-primary font-medium text-right">
                {{ job?.title || 'General Pool' }}
              </span>
            </div>

            <div class="flex items-center justify-between py-1">
              <span class="text-text-muted flex items-center gap-1.5">
                <Calendar class="w-3.5 h-3.5" />
                Added
              </span>
              <span class="text-text-primary font-medium tabular-nums">
                {{ formatDate(candidate.createdAt) }}
              </span>
            </div>
          </div>

          <!-- Recruiter Summary -->
          <div v-if="candidate.summary" class="pt-2 border-t border-border-subtle space-y-1">
            <span class="text-[11px] font-semibold uppercase tracking-wider text-text-muted">
              Recruiter Summary
            </span>
            <p class="text-xs text-text-primary leading-relaxed bg-surface-canvas p-2.5 rounded border border-border-subtle">
              {{ candidate.summary }}
            </p>
          </div>
        </div>

        <!-- Target Job Requisition Context -->
        <div v-if="job" class="surface-1 bg-surface-card border border-border-subtle rounded-lg shadow-sm p-4 space-y-2">
          <div class="flex items-center justify-between">
            <span class="text-[11px] font-semibold uppercase tracking-wider text-text-muted">
              Role Requirements
            </span>
            <span class="text-[10px] text-emerald-600 font-medium">Active Position</span>
          </div>
          <h4 class="text-xs font-bold text-text-primary">{{ job.title }}</h4>
          <p class="text-xs text-text-muted leading-relaxed line-clamp-4">
            {{ job.description || 'No detailed specifications recorded.' }}
          </p>
        </div>
      </div>

      <!-- Right Column (60% on desktop): CV Preview & AI Assessment Panel -->
      <div class="lg:col-span-7 space-y-5">
        <!-- AI Assessment Panel Component -->
        <AiAssessmentPanel
          :feedback="candidate.aiFeedback"
          :loading="isAssessing"
          :error="assessError"
          @assess="runAssessment"
        />

        <!-- CV Text Panel -->
        <div class="surface-1 bg-surface-card border border-border-subtle rounded-lg shadow-sm p-5 space-y-3">
          <div class="flex items-center justify-between pb-2 border-b border-border-subtle">
            <div class="flex items-center gap-2">
              <FileText class="w-4 h-4 text-primary" />
              <h3 class="text-xs font-semibold text-text-primary">
                Resume / Profile Text Preview
              </h3>
            </div>
            <span class="text-[11px] text-text-muted tabular-nums">
              {{ candidate.cvText?.length || 0 }} characters
            </span>
          </div>

          <div
            v-if="candidate.cvText"
            class="text-xs text-text-primary font-mono leading-relaxed whitespace-pre-wrap bg-surface-canvas p-4 rounded border border-border-subtle max-h-96 overflow-y-auto"
          >
            {{ candidate.cvText }}
          </div>
          <div v-else class="text-xs text-text-muted py-6 text-center">
            No resume text uploaded for this candidate.
          </div>
        </div>
      </div>
    </div>

    <!-- Confirm Delete Dialog -->
    <ConfirmDialog
      :open="isDeleteDialogOpen"
      title="Delete Candidate Record"
      :message="`Are you sure you want to permanently delete '${candidate.fullName}'? This action cannot be undone.`"
      confirm-text="Delete Candidate"
      :loading="isDeleting"
      @confirm="handleDelete"
      @cancel="isDeleteDialogOpen = false"
    />
  </div>
</template>
