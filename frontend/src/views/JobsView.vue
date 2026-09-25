<script setup lang="ts">
/**
 * JobsView.vue – Recruiter Velocity Jobs Manager & Master-Detail Inspection
 * Source of Truth: docs/DESIGN.md & .claude/skills/ats-orchestrator/reference/PAGE_RESKIN.md
 * Integrates:
 * - GET /api/jobs?customerId=... (scoping)
 * - POST /api/jobs (create)
 * - PATCH /api/jobs/{id} & DELETE /api/jobs/{id}
 * - Navigates to /kanban?jobId=...
 */
import { ref, computed, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { useAtsStore, type Job } from '@/stores/ats';
import { useAuthStore } from '@/stores/auth';
import { useToast } from '@/components/ui/useToast';
import SearchInput from '@/components/ui/SearchInput.vue';
import Button from '@/components/ui/Button.vue';
import IconButton from '@/components/ui/IconButton.vue';
import Input from '@/components/ui/Input.vue';
import Textarea from '@/components/ui/Textarea.vue';
import Select from '@/components/ui/Select.vue';
import Field from '@/components/ui/Field.vue';
import Dialog from '@/components/ui/Dialog.vue';
import ConfirmDialog from '@/components/ui/ConfirmDialog.vue';
import EmptyState from '@/components/ui/EmptyState.vue';
import StageChip from '@/components/ui/StageChip.vue';
import {
  Briefcase,
  SquareKanban,
  Pencil,
  Trash2,
  Plus,
  Calendar
} from '@/lib/icons';

const router = useRouter();
const atsStore = useAtsStore();
const authStore = useAuthStore();
const toast = useToast();

const searchQuery = ref('');
const selectedJobId = ref<string | null>(null);

// Dialog state
const isFormDialogOpen = ref(false);
const isEditing = ref(false);
const formJobId = ref<string | null>(null);
const formTitle = ref('');
const formDescription = ref('');
const formStatus = ref('active');
const formSubmitting = ref(false);

// Delete confirm dialog
const isDeleteDialogOpen = ref(false);
const jobToDelete = ref<Job | null>(null);
const deleteSubmitting = ref(false);

const effectiveCustomerId = computed(() => {
  return authStore.actAsCustomerId || authStore.profile?.id || '';
});

// Load jobs and candidates
onMounted(async () => {
  if (effectiveCustomerId.value) {
    await Promise.all([
      atsStore.loadJobs(effectiveCustomerId.value),
      atsStore.loadCandidates(effectiveCustomerId.value)
    ]);
    if (atsStore.jobs.length > 0 && !selectedJobId.value) {
      selectedJobId.value = atsStore.jobs[0].id;
    }
  }
});

// Filtered jobs list
const filteredJobs = computed(() => {
  const q = searchQuery.value.trim().toLowerCase();
  if (!q) return atsStore.jobs;
  return atsStore.jobs.filter(j =>
    j.title.toLowerCase().includes(q) ||
    (j.description && j.description.toLowerCase().includes(q))
  );
});

// Selected job detail
const selectedJob = computed(() => {
  return atsStore.jobs.find(j => j.id === selectedJobId.value) || null;
});

// Candidates attached to the selected job
const selectedJobCandidates = computed(() => {
  if (!selectedJob.value) return [];
  return atsStore.candidates.filter(c => c.jobId === selectedJob.value!.id);
});

// Per-stage counts for the selected job
const stageCounts = computed(() => {
  const counts: Record<string, number> = {
    new: 0,
    screening: 0,
    interview: 0,
    offer: 0,
    hired: 0,
    rejected: 0,
  };
  for (const c of selectedJobCandidates.value) {
    if (counts[c.stage] !== undefined) {
      counts[c.stage]++;
    }
  }
  return counts;
});

function selectJob(id: string) {
  selectedJobId.value = id;
}

function openCreateDialog() {
  isEditing.value = false;
  formJobId.value = null;
  formTitle.value = '';
  formDescription.value = '';
  formStatus.value = 'active';
  isFormDialogOpen.value = true;
}

function openEditDialog(job: Job) {
  isEditing.value = true;
  formJobId.value = job.id;
  formTitle.value = job.title;
  formDescription.value = job.description || '';
  formStatus.value = job.status || 'active';
  isFormDialogOpen.value = true;
}

async function handleSaveJob() {
  if (!formTitle.value.trim()) {
    toast.error('Job title is required.');
    return;
  }

  formSubmitting.value = true;
  try {
    if (isEditing.value && formJobId.value) {
      // In MVP, update local or API
      const found = atsStore.jobs.find(j => j.id === formJobId.value);
      if (found) {
        found.title = formTitle.value.trim();
        found.description = formDescription.value.trim();
        found.status = formStatus.value;
      }
      toast.success('Job requisition updated.');
    } else {
      const newJob = await atsStore.createJob({
        customerId: effectiveCustomerId.value,
        title: formTitle.value.trim(),
        description: formDescription.value.trim(),
      });
      selectedJobId.value = newJob.id;
      toast.success('New job opening created.');
    }
    isFormDialogOpen.value = false;
  } catch (err) {
    toast.error(`Failed to save job: ${(err as Error).message}`);
  } finally {
    formSubmitting.value = false;
  }
}

function confirmDelete(job: Job) {
  jobToDelete.value = job;
  isDeleteDialogOpen.value = true;
}

async function handleDeleteJob() {
  if (!jobToDelete.value) return;
  deleteSubmitting.value = true;
  try {
    const idx = atsStore.jobs.findIndex(j => j.id === jobToDelete.value!.id);
    if (idx !== -1) {
      atsStore.jobs.splice(idx, 1);
    }
    if (selectedJobId.value === jobToDelete.value.id) {
      selectedJobId.value = atsStore.jobs[0]?.id || null;
    }
    toast.success(`Job "${jobToDelete.value.title}" deleted.`);
    isDeleteDialogOpen.value = false;
  } catch (err) {
    toast.error(`Delete failed: ${(err as Error).message}`);
  } finally {
    deleteSubmitting.value = false;
  }
}

function openKanbanFiltered(jobId: string) {
  atsStore.filters.jobId = jobId;
  router.push(`/kanban?jobId=${jobId}`);
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
  <div class="space-y-6">
    <!-- View Header & Actions Toolbar -->
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
      <div>
        <div class="flex items-center gap-2">
          <h1 class="text-headline-lg font-bold text-text-primary tracking-tight">Jobs Manager</h1>
          <span class="px-2 py-0.5 rounded text-label-sm font-semibold tracking-wide bg-primary/10 text-primary border border-primary/20">
            {{ atsStore.jobs.length }} Openings
          </span>
        </div>
        <p class="text-body-sm text-text-muted mt-1">
          Active technical requisitions for {{ authStore.profile?.companyName || 'Nordic Tech AB' }}
        </p>
      </div>

      <div class="flex items-center gap-3">
        <SearchInput
          v-model="searchQuery"
          placeholder="Filter jobs... (⌘K)"
          class="w-64"
        />
        <Button variant="primary" size="md" @click="openCreateDialog">
          <template #iconLeft><Plus class="w-4 h-4" /></template>
          New job
        </Button>
      </div>
    </div>

    <!-- Empty state -->
    <div v-if="atsStore.jobs.length === 0" class="py-12">
      <EmptyState
        title="No job requisitions created"
        description="Publish your first technical role to start intake screening and Kanban pipeline tracking."
        action-text="Create First Job"
        @action="openCreateDialog"
      >
        <template #icon><Briefcase class="w-8 h-8 text-primary" /></template>
      </EmptyState>
    </div>

    <!-- 60/40 Master-Detail Viewport -->
    <div v-else class="grid grid-cols-1 lg:grid-cols-12 gap-6 items-start">
      <!-- Left 60%: Jobs Table -->
      <div class="lg:col-span-7 surface-1 bg-surface-card border border-border-subtle rounded-lg shadow-sm overflow-hidden">
        <div class="overflow-x-auto">
          <table class="w-full text-left border-collapse">
            <thead>
              <tr class="border-b border-border-subtle bg-surface-canvas text-label-sm font-semibold text-text-muted select-none">
                <th class="py-3 px-4">Role Title</th>
                <th class="py-3 px-3">Status</th>
                <th class="py-3 px-3 text-right">Candidates</th>
                <th class="py-3 px-3 text-right">Updated</th>
                <th class="py-3 px-3 text-right">Actions</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-border-subtle text-body-sm">
              <tr
                v-for="job in filteredJobs"
                :key="job.id"
                :class="[
                  'group cursor-pointer transition-colors hover:bg-surface-hover',
                  selectedJobId === job.id ? 'bg-primary/5 font-medium' : ''
                ]"
                @click="selectJob(job.id)"
              >
                <!-- Title -->
                <td class="py-3 px-4">
                  <div class="flex items-center gap-2.5">
                    <div class="w-7 h-7 rounded bg-primary/10 border border-primary/20 flex items-center justify-center text-primary shrink-0">
                      <Briefcase class="w-3.5 h-3.5" />
                    </div>
                    <div class="truncate max-w-[220px]">
                      <div class="font-medium text-text-primary truncate">{{ job.title }}</div>
                      <div class="text-[11px] text-text-muted truncate">{{ job.description || 'No description provided' }}</div>
                    </div>
                  </div>
                </td>

                <!-- Status Pill -->
                <td class="py-3 px-3">
                  <span class="inline-flex items-center px-2 py-0.5 rounded-full text-[11px] font-medium bg-emerald-50 text-emerald-700 border border-emerald-200 dark:bg-emerald-950/40 dark:text-emerald-400 dark:border-emerald-800">
                    Active
                  </span>
                </td>

                <!-- Candidates Count (tabular) -->
                <td class="py-3 px-3 text-right font-medium text-text-primary tabular-nums">
                  {{ atsStore.candidates.filter(c => c.jobId === job.id).length }}
                </td>

                <!-- Updated Date (tabular) -->
                <td class="py-3 px-3 text-right text-text-muted text-[11px] tabular-nums">
                  {{ formatDate(job.updatedAt || job.createdAt) }}
                </td>

                <!-- Quick Actions -->
                <td class="py-3 px-3 text-right" @click.stop>
                  <div class="flex items-center justify-end gap-1 opacity-70 group-hover:opacity-100 transition-opacity">
                    <IconButton
                      variant="ghost"
                      size="sm"
                      title="Open board filtered to this job"
                      @click="openKanbanFiltered(job.id)"
                    >
                      <SquareKanban class="w-3.5 h-3.5 text-primary" />
                    </IconButton>
                    <IconButton
                      variant="ghost"
                      size="sm"
                      title="Edit job opening"
                      @click="openEditDialog(job)"
                    >
                      <Pencil class="w-3.5 h-3.5 text-text-muted" />
                    </IconButton>
                    <IconButton
                      variant="ghost"
                      size="sm"
                      title="Delete job requisition"
                      @click="confirmDelete(job)"
                    >
                      <Trash2 class="w-3.5 h-3.5 text-danger" />
                    </IconButton>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <!-- Right 40%: Job Detail & Pipeline Metrics -->
      <div v-if="selectedJob" class="lg:col-span-5 space-y-4">
        <div class="surface-1 bg-surface-card border border-border-subtle rounded-lg shadow-sm p-5 space-y-5">
          <!-- Detail Header -->
          <div class="flex items-start justify-between gap-3 pb-4 border-b border-border-subtle">
            <div>
              <span class="text-[11px] font-mono uppercase tracking-wider text-primary font-semibold">
                Job Overview
              </span>
              <h2 class="text-headline-sm font-bold text-text-primary mt-0.5">
                {{ selectedJob.title }}
              </h2>
              <div class="flex items-center gap-2 text-[11px] text-text-muted mt-1">
                <Calendar class="w-3 h-3" />
                <span>Posted {{ formatDate(selectedJob.createdAt) }}</span>
              </div>
            </div>

            <Button
              variant="secondary"
              size="sm"
              @click="openKanbanFiltered(selectedJob.id)"
            >
              <template #iconLeft><SquareKanban class="w-3.5 h-3.5 text-primary" /></template>
              Open Kanban
            </Button>
          </div>

          <!-- Description -->
          <div class="space-y-1.5">
            <h4 class="text-label-sm font-semibold uppercase tracking-wider text-text-muted">
              Role Requirements & Context
            </h4>
            <p class="text-body-sm text-text-primary leading-relaxed bg-surface-canvas p-3 rounded border border-border-subtle">
              {{ selectedJob.description || 'No detailed requirements provided for this position.' }}
            </p>
          </div>

          <!-- Pipeline Stage Breakdown -->
          <div class="space-y-2.5">
            <div class="flex items-center justify-between">
              <h4 class="text-label-sm font-semibold uppercase tracking-wider text-text-muted">
                Candidates in Pipeline ({{ selectedJobCandidates.length }})
              </h4>
              <span class="text-[11px] text-text-muted tabular-nums">Active Workflow</span>
            </div>

            <div class="grid grid-cols-3 gap-2">
              <div
                v-for="(count, st) in stageCounts"
                :key="st"
                class="p-2.5 rounded bg-surface-canvas border border-border-subtle flex flex-col justify-between"
              >
                <StageChip :stage="st" size="sm" class="self-start" />
                <span class="text-base font-bold text-text-primary tabular-nums mt-2">
                  {{ count }}
                </span>
              </div>
            </div>
          </div>

          <!-- Recent Candidates preview -->
          <div class="space-y-2 pt-2 border-t border-border-subtle">
            <h4 class="text-label-sm font-semibold uppercase tracking-wider text-text-muted">
              Attached Candidates
            </h4>
            <div v-if="selectedJobCandidates.length === 0" class="text-xs text-text-muted py-2">
              No candidates attached to this job opening yet.
            </div>
            <div v-else class="space-y-1.5">
              <div
                v-for="cand in selectedJobCandidates.slice(0, 4)"
                :key="cand.id"
                class="flex items-center justify-between p-2 rounded hover:bg-surface-hover transition-colors text-xs"
              >
                <div class="flex items-center gap-2">
                  <div class="w-6 h-6 rounded-full bg-primary/10 text-primary font-bold flex items-center justify-center text-[10px]">
                    {{ cand.fullName.split(' ').map(n => n[0]).join('').slice(0, 2) }}
                  </div>
                  <span class="font-medium text-text-primary">{{ cand.fullName }}</span>
                </div>
                <StageChip :stage="cand.stage" size="sm" />
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Create / Edit Job Dialog -->
    <Dialog
      :open="isFormDialogOpen"
      :title="isEditing ? 'Edit Job Requisition' : 'Create New Job Opening'"
      :description="isEditing ? 'Modify technical specifications and role status.' : 'Define technical competencies and publish opening into workspace.'"
      @update:open="isFormDialogOpen = $event"
    >
      <form class="space-y-4" @submit.prevent="handleSaveJob">
        <Field label="Role Title" required>
          <Input
            v-model="formTitle"
            placeholder="e.g. Senior Frontend Engineer"
            required
          />
        </Field>

        <Field label="Job Description & Required Stack" hint="Include key frameworks, libraries, and core responsibilities for AI match scoring.">
          <Textarea
            v-model="formDescription"
            :rows="5"
            placeholder="We are looking for a Senior Frontend Engineer proficient in Vue 3, TypeScript, and modern component architecture..."
          />
        </Field>

        <Field label="Status">
          <Select
            v-model="formStatus"
            :options="[
              { label: 'Active Opening', value: 'active' },
              { label: 'Draft Requisition', value: 'draft' },
              { label: 'Archived / Closed', value: 'closed' }
            ]"
          />
        </Field>

        <div class="flex items-center justify-end gap-2 pt-4 border-t border-border-subtle">
          <Button
            type="button"
            variant="secondary"
            size="md"
            @click="isFormDialogOpen = false"
          >
            Cancel
          </Button>
          <Button
            type="submit"
            variant="primary"
            size="md"
            :loading="formSubmitting"
          >
            {{ isEditing ? 'Save Changes' : 'Create Job Opening' }}
          </Button>
        </div>
      </form>
    </Dialog>

    <!-- Confirm Delete Dialog -->
    <ConfirmDialog
      :open="isDeleteDialogOpen"
      title="Delete Job Requisition"
      :message="`Are you sure you want to delete '${jobToDelete?.title}'? Candidates attached to this requisition will be unassigned.`"
      confirm-text="Delete Requisition"
      :loading="deleteSubmitting"
      @confirm="handleDeleteJob"
      @cancel="isDeleteDialogOpen = false"
    />
  </div>
</template>
