<script setup lang="ts">
// CandidateCard.vue – compact Kanban card with drag, stage select, and AI assess button.
// Related: src/components/KanbanColumn.vue (parent)
//          src/components/AiAssessmentPanel.vue (shown after AI assess)
//          src/stores/ats.ts (assessCandidate action)
//          backend/MiniAts.Api/Api/Controllers/AiController.cs (POST /api/ai/candidates/{id}/assess)

import { ref } from 'vue'
import type { Candidate } from '../stores/ats'
import { useAtsStore } from '../stores/ats'
import AiAssessmentPanel from './AiAssessmentPanel.vue'

const props = defineProps<{
  candidate:  Candidate
  customerId: string
}>()

const emit = defineEmits<{
  stageChange: [candidateId: string, newStage: string]
}>()

const ats = useAtsStore()

const STAGES = ['new', 'screening', 'interview', 'offer', 'hired', 'rejected']

// AI assessment state.
const assessing     = ref(false)
const assessError   = ref<string | null>(null)
const showAiPanel   = ref(false)

// Make the card draggable; data is the candidate id for the column drop handler.
function onDragStart(event: DragEvent) {
  event.dataTransfer?.setData('text/plain', props.candidate.id)
}

// Stage select change emits to the parent column which calls the store.
function onStageChange(event: Event) {
  const select = event.target as HTMLSelectElement
  emit('stageChange', props.candidate.id, select.value)
}

// Trigger AI assessment and open the result panel.
async function handleAssess() {
  assessing.value   = true
  assessError.value = null
  try {
    // assessCandidate calls POST /api/ai/candidates/{id}/assess, stores result locally.
    await ats.assessCandidate(props.candidate.id, props.customerId)
    showAiPanel.value = true
  } catch (e) {
    assessError.value = (e as Error).message
  } finally {
    assessing.value = false
  }
}
</script>

<template>
  <div
    class="card candidate-card"
    draggable="true"
    @dragstart="onDragStart"
  >
    <!-- Candidate name (always shown) -->
    <div class="card-name">{{ candidate.fullName }}</div>

    <!-- Optional email -->
    <div v-if="candidate.email" class="card-meta">
      <a :href="`mailto:${candidate.email}`" class="card-link">{{ candidate.email }}</a>
    </div>

    <!-- LinkedIn link -->
    <div v-if="candidate.linkedinUrl" class="card-meta">
      <a :href="candidate.linkedinUrl" target="_blank" rel="noreferrer" class="card-link">
        🔗 LinkedIn
      </a>
    </div>

    <!-- AI score badge shown if assessment has been run -->
    <div v-if="candidate.aiScore !== null" class="ai-score-row">
      <span class="ai-score-badge" :class="scoreClass(candidate.aiScore)">
        AI {{ candidate.aiScore }}/100
      </span>
      <button class="btn-ghost tiny" @click="showAiPanel = !showAiPanel">
        {{ showAiPanel ? 'Hide' : 'Show' }} details
      </button>
    </div>

    <!-- AI assessment result panel (toggled) -->
    <AiAssessmentPanel
      v-if="showAiPanel && candidate.aiFeedback"
      :feedback="candidate.aiFeedback"
    />

    <!-- Stage select fallback (always available alongside drag/drop) -->
    <select class="stage-select" :value="candidate.stage" @change="onStageChange">
      <option v-for="s in STAGES" :key="s" :value="s">{{ s }}</option>
    </select>

    <!-- AI assess button -->
    <button
      class="btn-outline tiny assess-btn"
      :disabled="assessing"
      @click="handleAssess"
    >
      {{ assessing ? 'Assessing…' : '🤖 Assess CV' }}
    </button>

    <p v-if="assessError" class="assess-error">{{ assessError }}</p>
  </div>
</template>

<script lang="ts">
// Score color helper – not in setup() because it needs to be available in template.
export function scoreClass(score: number | null): string {
  if (!score) return ''
  if (score >= 70) return 'good'
  if (score >= 40) return 'medium'
  return 'low'
}
</script>

<style scoped>
.candidate-card {
  cursor: grab;
  padding: 10px;
  display: flex;
  flex-direction: column;
  gap: 4px;
  user-select: none;
}

.candidate-card:active { cursor: grabbing; }

.card-name {
  font-size: 13px;
  font-weight: 600;
  color: var(--text);
  line-height: 1.3;
}

.card-meta { font-size: 11px; }

.card-link {
  color: var(--primary);
  text-decoration: none;
}
.card-link:hover { text-decoration: underline; }

.ai-score-row {
  display: flex;
  align-items: center;
  gap: 6px;
  margin-top: 2px;
}

.ai-score-badge {
  font-size: 11px;
  font-weight: 600;
  padding: 2px 6px;
  border-radius: 10px;
  background: #e5e7eb;
  color: #374151;
}
.ai-score-badge.good   { background: #dcfce7; color: #15803d; }
.ai-score-badge.medium { background: #fef9c3; color: #854d0e; }
.ai-score-badge.low    { background: #fee2e2; color: #b91c1c; }

.stage-select {
  font-size: 11px;
  padding: 3px 6px;
  border: 1px solid var(--border);
  border-radius: 4px;
  background: var(--bg-page);
  color: var(--text-secondary);
  cursor: pointer;
  margin-top: 4px;
}

.assess-btn { margin-top: 4px; width: 100%; }

.assess-error { font-size: 11px; color: var(--danger); margin: 0; }

.tiny { padding: 3px 8px; font-size: 11px; }
</style>
