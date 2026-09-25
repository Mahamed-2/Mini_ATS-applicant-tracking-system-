<script setup lang="ts">
/**
 * CandidatesView.vue – Candidate Roster & Intake Management
 * Source of Truth: docs/DESIGN.md & .claude/skills/ats-orchestrator/reference/PAGE_RESKIN.md
 * Integrates:
 * - GET /api/candidates?customerId=...&jobId=...&search=...
 * - POST /api/candidates (intake)
 * - Navigation to /candidates/:id (detail scorecard)
 */
import { ref, computed, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { useAtsStore } from '@/stores/ats';
import { useAuthStore } from '@/stores/auth';
import { useToast } from '@/components/ui/useToast';
import { useI18n } from '@/i18n';
import SearchInput from '@/components/ui/SearchInput.vue';
import Button from '@/components/ui/Button.vue';
import Select from '@/components/ui/Select.vue';
import Input from '@/components/ui/Input.vue';
import Textarea from '@/components/ui/Textarea.vue';
import Field from '@/components/ui/Field.vue';
import Dialog from '@/components/ui/Dialog.vue';
import Avatar from '@/components/ui/Avatar.vue';
import StageChip from '@/components/ui/StageChip.vue';
import ScoreBadge from '@/components/ui/ScoreBadge.vue';
import EmptyState from '@/components/ui/EmptyState.vue';
import {
  Users,
  Plus,
  Linkedin,
  Mail,
  ChevronRight
} from '@/lib/icons';

const router = useRouter();
const atsStore = useAtsStore();
const authStore = useAuthStore();
const toast = useToast();
const { t } = useI18n();

const searchQuery = ref('');
const selectedJobFilter = ref('all');
const selectedStageFilter = ref('all');

// Intake Dialog state
const isFormDialogOpen = ref(false);
const formFullName = ref('');
const formEmail = ref('');
const formLinkedinUrl = ref('');
const formJobId = ref<string>('');
const formStage = ref('applied');
const formCvText = ref('');
const formSummary = ref('');
const formSubmitting = ref(false);

const effectiveCustomerId = computed(() => {
  return authStore.actAsCustomerId || authStore.profile?.id || '';
});

// Load candidates and jobs on mount
onMounted(async () => {
  if (effectiveCustomerId.value) {
    await Promise.all([
      atsStore.loadCandidates(effectiveCustomerId.value),
      atsStore.loadJobs(effectiveCustomerId.value)
    ]);
  }
});

// Filter options for dropdowns
const jobOptions = computed(() => [
  { label: t('candidates.allRoles'), value: 'all' },
  ...atsStore.jobs.map(j => ({ label: j.title, value: j.id }))
]);

const stageOptions = computed(() => [
  { label: t('candidates.allStages'), value: 'all' },
  { label: t('stage.applied'), value: 'applied' },
  { label: t('stage.phoneScreen'), value: 'phone_screen' },
  { label: t('stage.techInterview'), value: 'interview' },
  { label: t('stage.cultureFit'), value: 'culture_fit' },
  { label: t('stage.offer'), value: 'offer' },
  { label: t('stage.hired'), value: 'hired' },
]);

// Jobs lookup map
const jobMap = computed(() => {
  const map: Record<string, string> = {};
  for (const j of atsStore.jobs) {
    map[j.id] = j.title;
  }
  return map;
});

// Filtered candidates
const filteredCandidates = computed(() => {
  return atsStore.candidates.filter(c => {
    // Search query
    const q = searchQuery.value.trim().toLowerCase();
    if (q) {
      const matchName = c.fullName.toLowerCase().includes(q);
      const matchEmail = c.email?.toLowerCase().includes(q);
      const matchRole = c.jobId && jobMap.value[c.jobId]?.toLowerCase().includes(q);
      if (!matchName && !matchEmail && !matchRole) return false;
    }

    // Job filter
    if (selectedJobFilter.value !== 'all' && c.jobId !== selectedJobFilter.value) {
      return false;
    }

    // Stage filter
    if (selectedStageFilter.value !== 'all' && c.stage !== selectedStageFilter.value) {
      return false;
    }

    return true;
  });
});

function openCreateDialog() {
  formFullName.value = '';
  formEmail.value = '';
  formLinkedinUrl.value = '';
  formJobId.value = atsStore.jobs[0]?.id || '';
  formStage.value = 'new';
  formCvText.value = '';
  formSummary.value = '';
  isFormDialogOpen.value = true;
}

async function handleCreateCandidate() {
  if (!formFullName.value.trim()) {
    toast.error('Full name is required.');
    return;
  }

  formSubmitting.value = true;
  try {
    const newCand = await atsStore.createCandidate({
      customerId: effectiveCustomerId.value,
      fullName: formFullName.value.trim(),
      email: formEmail.value.trim() || null,
      linkedinUrl: formLinkedinUrl.value.trim() || null,
      jobId: formJobId.value || null,
      stage: formStage.value,
      cvText: formCvText.value.trim() || null,
      summary: formSummary.value.trim() || null,
    });

    toast.success(`Candidate ${newCand.fullName} added successfully.`);
    isFormDialogOpen.value = false;
  } catch (err) {
    toast.error(`Intake failed: ${(err as Error).message}`);
  } finally {
    formSubmitting.value = false;
  }
}

function openCandidateDetail(candidateId: string) {
  router.push(`/candidates/${candidateId}`);
}

function formatDate(dateStr: string) {
  try {
    return new Date(dateStr).toLocaleDateString('sv-SE', {
      month: 'short',
      day: 'numeric'
    });
  } catch {
    return dateStr;
  }
}
</script>

<template>
  <div class="space-y-6">
    <!-- View Header & Actions Toolbar -->
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
      <div>
        <div class="flex items-center gap-2">
          <h1 class="text-headline-lg font-bold text-text-primary tracking-tight">
            {{ t('candidates.title') }}
          </h1>
          <span class="px-2 py-0.5 rounded text-label-sm font-semibold tracking-wide bg-primary/10 text-primary border border-primary/20">
            {{ t('candidates.profilesCount', { count: atsStore.candidates.length }) }}
          </span>
        </div>
        <p class="text-body-sm text-text-muted mt-1">
          {{ t('candidates.subtitle', { company: authStore.profile?.companyName || 'Nordic Tech AB' }) }}
        </p>
      </div>

      <Button variant="primary" size="md" @click="openCreateDialog">
        <template #iconLeft><Plus class="w-4 h-4" /></template>
        {{ t('candidates.addBtn') }}
      </Button>
    </div>

    <!-- Filter Bar -->
    <div class="surface-1 bg-surface-card border border-border-subtle rounded-lg p-3 shadow-sm flex flex-col sm:flex-row items-center justify-between gap-3">
      <div class="flex flex-wrap items-center gap-2.5 w-full sm:w-auto">
        <SearchInput
          id="candidates-search-filter"
          name="candidatesSearch"
          v-model="searchQuery"
          :placeholder="t('candidates.searchPlaceholder')"
          class="w-full sm:w-72"
        />

        <div class="flex items-center gap-2">
          <Select
            id="candidates-job-filter"
            name="candidatesJobFilter"
            v-model="selectedJobFilter"
            :options="jobOptions"
            class="w-48 text-xs"
          />

          <Select
            id="candidates-stage-filter"
            name="candidatesStageFilter"
            v-model="selectedStageFilter"
            :options="stageOptions"
            class="w-36 text-xs"
          />
        </div>
      </div>

      <div class="text-[11px] text-text-muted select-none tabular-nums self-end sm:self-center">
        {{ t('candidates.showing', { count: filteredCandidates.length, total: atsStore.candidates.length }) }}
      </div>
    </div>

    <!-- Empty State -->
    <div v-if="filteredCandidates.length === 0" class="py-12">
      <EmptyState
        :title="t('candidates.emptyTitle')"
        :description="t('candidates.emptyDesc')"
        :action-text="t('candidates.addBtn')"
        @action="openCreateDialog"
      >
        <template #icon><Users class="w-8 h-8 text-primary" /></template>
      </EmptyState>
    </div>

    <!-- Candidates Table -->
    <div v-else class="surface-1 bg-surface-card border border-border-subtle rounded-lg shadow-sm overflow-hidden">
      <div class="overflow-x-auto">
        <table class="w-full text-left border-collapse">
          <thead>
            <tr class="border-b border-border-subtle bg-surface-canvas text-label-sm font-semibold text-text-muted select-none">
              <th class="py-3 px-4">{{ t('candidates.thCandidate') }}</th>
              <th class="py-3 px-3">{{ t('candidates.thRole') }}</th>
              <th class="py-3 px-3">{{ t('candidates.thStage') }}</th>
              <th class="py-3 px-3">{{ t('candidates.thMatch') }}</th>
              <th class="py-3 px-3">{{ t('candidates.thLinkedIn') }}</th>
              <th class="py-3 px-3">{{ t('candidates.thEmail') }}</th>
              <th class="py-3 px-3 text-right">{{ t('candidates.thAdded') }}</th>
              <th class="py-3 px-3 w-8"></th>
            </tr>
          </thead>
          <tbody class="divide-y divide-border-subtle text-body-sm">
            <tr
              v-for="cand in filteredCandidates"
              :key="cand.id"
              class="group cursor-pointer transition-colors hover:bg-surface-hover"
              @click="openCandidateDetail(cand.id)"
            >
              <!-- Candidate Name + Avatar -->
              <td class="py-3 px-4">
                <div class="flex items-center gap-3">
                  <Avatar :name="cand.fullName" size="sm" />
                  <div>
                    <div class="font-medium text-text-primary group-hover:text-primary transition-colors">
                      {{ cand.fullName }}
                    </div>
                    <div class="text-[11px] text-text-muted truncate max-w-[200px]">
                      {{ cand.summary || 'Applicant record' }}
                    </div>
                  </div>
                </div>
              </td>

              <!-- Job Opening -->
              <td class="py-3 px-3">
                <span v-if="cand.jobId && jobMap[cand.jobId]" class="text-xs font-medium text-text-primary">
                  {{ jobMap[cand.jobId] }}
                </span>
                <span v-else class="text-xs text-text-muted">—</span>
              </td>

              <!-- Stage Chip -->
              <td class="py-3 px-3">
                <StageChip :stage="cand.stage" size="sm" />
              </td>

              <!-- AI Score -->
              <td class="py-3 px-3">
                <ScoreBadge v-if="cand.aiScore !== null && cand.aiScore !== undefined" :score="cand.aiScore" />
                <span v-else class="text-xs text-text-muted">—</span>
              </td>

              <!-- LinkedIn -->
              <td class="py-3 px-3" @click.stop>
                <a
                  v-if="cand.linkedinUrl"
                  :href="cand.linkedinUrl"
                  target="_blank"
                  rel="noopener noreferrer"
                  class="inline-flex items-center gap-1 text-[#0a66c2] hover:underline text-xs"
                  title="Open verified LinkedIn profile"
                >
                  <Linkedin class="w-3.5 h-3.5 fill-[#0a66c2]" />
                  <span>{{ t('candidates.viewProfile') }}</span>
                </a>
                <span v-else class="text-xs text-text-muted">—</span>
              </td>

              <!-- Email Link -->
              <td class="py-3 px-3" @click.stop>
                <a
                  v-if="cand.email"
                  :href="`mailto:${cand.email}`"
                  class="inline-flex items-center gap-1 text-text-muted hover:text-text-primary text-xs"
                >
                  <Mail class="w-3 h-3 text-text-muted" />
                  <span class="truncate max-w-[160px]">{{ cand.email }}</span>
                </a>
                <span v-else class="text-xs text-text-muted">—</span>
              </td>

              <!-- Created Date (tabular) -->
              <td class="py-3 px-3 text-right text-text-muted text-[11px] tabular-nums">
                {{ formatDate(cand.createdAt) }}
              </td>

              <!-- Navigate Chevron -->
              <td class="py-3 px-3 text-right text-text-muted group-hover:text-primary">
                <ChevronRight class="w-4 h-4 ml-auto" />
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Candidate Intake Dialog -->
    <Dialog
      :open="isFormDialogOpen"
      :title="t('candidates.modalTitle')"
      :description="t('candidates.modalDesc')"
      @update:open="isFormDialogOpen = $event"
    >
      <form class="space-y-4" @submit.prevent="handleCreateCandidate">
        <Field id="candidate-form-name" :label="t('candidates.fullName')" required>
          <Input
            id="candidate-form-name"
            name="fullName"
            v-model="formFullName"
            placeholder="e.g. Astrid Bergström"
            autocomplete="name"
            required
          />
        </Field>

        <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <Field id="candidate-form-email" :label="t('candidates.emailAddress')">
            <Input
              id="candidate-form-email"
              name="email"
              v-model="formEmail"
              type="email"
              placeholder="candidate@example.com"
              autocomplete="email"
            />
          </Field>

          <Field id="candidate-form-linkedin" :label="t('candidates.linkedinUrl')">
            <Input
              id="candidate-form-linkedin"
              name="linkedinUrl"
              v-model="formLinkedinUrl"
              placeholder="https://linkedin.com/in/..."
              autocomplete="url"
            />
          </Field>
        </div>

        <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <Field id="candidate-form-job" :label="t('candidates.targetRole')">
            <Select
              id="candidate-form-job"
              name="jobId"
              v-model="formJobId"
              :options="atsStore.jobs.map(j => ({ label: j.title, value: j.id }))"
            />
          </Field>

          <Field id="candidate-form-stage" :label="t('candidates.initialStage')">
            <Select
              id="candidate-form-stage"
              name="stage"
              v-model="formStage"
              :options="stageOptions.filter(o => o.value !== 'all')"
            />
          </Field>
        </div>

        <Field
          id="candidate-form-cv"
          :label="t('candidates.cvText')"
          :hint="t('candidates.cvHint', { count: formCvText.length })"
        >
          <Textarea
            id="candidate-form-cv"
            name="cvText"
            v-model="formCvText"
            :rows="5"
            placeholder="Paste candidate resume, experience highlights, technical skills, and achievements here..."
          />
        </Field>

        <Field id="candidate-form-notes" :label="t('candidates.recruiterNotes')">
          <Input
            id="candidate-form-notes"
            name="recruiterNotes"
            v-model="formSummary"
            placeholder="Brief recruiter assessment notes or referral context..."
          />
        </Field>

        <div class="flex items-center justify-end gap-2 pt-4 border-t border-border-subtle">
          <Button
            type="button"
            variant="secondary"
            size="md"
            @click="isFormDialogOpen = false"
          >
            {{ t('common.cancel') }}
          </Button>
          <Button
            type="submit"
            variant="primary"
            size="md"
            :loading="formSubmitting"
          >
            {{ t('candidates.createBtn') }}
          </Button>
        </div>
      </form>
    </Dialog>
  </div>
</template>
