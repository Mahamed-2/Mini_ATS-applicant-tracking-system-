<script setup lang="ts">
// KanbanView.vue – full Kanban board with filters, columns, cards, and stage moves.
// Related: src/stores/ats.ts (loadJobs, loadCandidates, moveStage)
//          src/stores/auth.ts (effectiveCustomerId)
//          src/components/KanbanColumn.vue (renders one stage column)
//          src/components/CandidateCard.vue (renders one candidate card)
//          src/components/CandidateForm.vue (add candidate modal)
//          backend/MiniAts.Api/Api/Controllers/CandidatesController.cs (PATCH /stage)

import { computed, onMounted, ref, watch } from 'vue'
import { useAuthStore } from '../stores/auth'
import { useAtsStore } from '../stores/ats'
import KanbanColumn from '../components/KanbanColumn.vue'
import CandidateForm from '../components/CandidateForm.vue'

const auth = useAuthStore()
const ats  = useAtsStore()

// Kanban stage order matches public.candidate_stage enum.
const STAGES = ['new', 'screening', 'interview', 'offer', 'hired', 'rejected'] as const

// Local filter state bound to the toolbar inputs.
const jobFilter = ref('')
const search    = ref('')

// Controls add-candidate modal visibility.
const showCandidateForm = ref(false)

// Load jobs and candidates on mount.
onMounted(() => loadAll())

async function loadAll() {
  const cid = auth.effectiveCustomerId
  if (!cid) return
  await ats.loadJobs(cid)
  await ats.loadCandidates(cid, jobFilter.value || undefined, search.value || undefined)
}

// Reload candidates whenever filters change.
watch([jobFilter, search], () => {
  ats.loadCandidates(
    auth.effectiveCustomerId,
    jobFilter.value || undefined,
    search.value || undefined
  )
})

// Group flat candidate list into per-stage columns for rendering.
const grouped = computed(() =>
  STAGES.map(stage => ({
    stage,
    candidates: ats.candidates.filter(c => c.stage === stage)
  }))
)

// Move a candidate to a new stage; optimistic update happens in the store.
async function onStageChange(candidateId: string, newStage: string) {
  await ats.moveStage(candidateId, newStage, auth.effectiveCustomerId)
}

// Reload after new candidate is created.
function onCandidateCreated() {
  showCandidateForm.value = false
  loadAll()
}
</script>

<template>
  <div class="kanban-page">
    <!-- Toolbar: filters and actions -->
    <div class="kanban-toolbar">
      <h1 class="page-title" style="margin: 0">Kanban</h1>

      <label class="toolbar-field">
        <span>Job</span>
        <select v-model="jobFilter" id="filter-job">
          <option value="">All jobs</option>
          <option v-for="job in ats.jobs" :key="job.id" :value="job.id">
            {{ job.title }}
          </option>
        </select>
      </label>

      <label class="toolbar-field">
        <span>Candidate</span>
        <input
          v-model="search"
          id="filter-search"
          type="search"
          placeholder="Search by name…"
        />
      </label>

      <button class="btn-outline" @click="loadAll">↻ Refresh</button>
      <button class="btn-primary" @click="showCandidateForm = true">+ Candidate</button>
    </div>

    <!-- Loading / error -->
    <p v-if="ats.loading" class="state-msg">Loading…</p>
    <p v-else-if="ats.error" class="state-msg error">{{ ats.error }}</p>

    <!-- Kanban board: one KanbanColumn per stage -->
    <div v-else class="board">
      <KanbanColumn
        v-for="col in grouped"
        :key="col.stage"
        :stage="col.stage"
        :candidates="col.candidates"
        :customer-id="auth.effectiveCustomerId"
        @stage-change="onStageChange"
      />
    </div>

    <!-- Add candidate modal -->
    <CandidateForm
      v-if="showCandidateForm"
      :customer-id="auth.effectiveCustomerId"
      :jobs="ats.jobs"
      @close="showCandidateForm = false"
      @created="onCandidateCreated"
    />
  </div>
</template>

<style scoped>
.kanban-page {
  padding: 16px;
  display: flex;
  flex-direction: column;
  height: 100%;
  gap: 12px;
}

.kanban-toolbar {
  display: flex;
  align-items: flex-end;
  gap: 10px;
  flex-wrap: wrap;
}

.toolbar-field {
  display: flex;
  flex-direction: column;
  gap: 3px;
  font-size: 12px;
  color: var(--text-secondary);
  font-weight: 500;
}

.board {
  display: flex;
  gap: 10px;
  overflow-x: auto;
  padding-bottom: 16px;
  flex: 1;
  align-items: flex-start;
}
</style>
