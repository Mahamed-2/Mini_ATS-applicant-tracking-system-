<script setup lang="ts">
// KanbanColumn.vue – renders one stage column with its candidate cards and a drop zone.
// Related: src/views/KanbanView.vue (renders one per stage)
//          src/components/CandidateCard.vue (rendered inside this column)
//          src/stores/ats.ts (moveStage called via emit)

import type { Candidate } from '../stores/ats'
import CandidateCard from './CandidateCard.vue'

const props = defineProps<{
  stage:       string
  candidates:  Candidate[]
  customerId:  string
}>()

const emit = defineEmits<{
  stageChange: [candidateId: string, newStage: string]
}>()

// Dragged-over state for visual feedback.
import { ref } from 'vue'
const isDragOver = ref(false)

function onDrop(event: DragEvent) {
  isDragOver.value = false
  const candidateId = event.dataTransfer?.getData('text/plain')
  if (candidateId) emit('stageChange', candidateId, props.stage)
}

// Stage display labels for the column header.
const LABELS: Record<string, string> = {
  new:        '🆕 New',
  screening:  '🔍 Screening',
  interview:  '💬 Interview',
  offer:      '📄 Offer',
  hired:      '✅ Hired',
  rejected:   '❌ Rejected'
}
</script>

<template>
  <div
    class="column"
    :class="{ 'drag-over': isDragOver }"
    @dragover.prevent="isDragOver = true"
    @dragleave="isDragOver = false"
    @drop="onDrop"
  >
    <!-- Column header with stage name and candidate count -->
    <div class="column-header">
      <span class="column-title">{{ LABELS[stage] || stage }}</span>
      <span class="count-badge">{{ candidates.length }}</span>
    </div>

    <!-- Candidate cards in this stage -->
    <div class="cards">
      <CandidateCard
        v-for="candidate in candidates"
        :key="candidate.id"
        :candidate="candidate"
        :customer-id="customerId"
        @stage-change="(id, s) => emit('stageChange', id, s)"
      />

      <!-- Empty drop zone when column has no cards -->
      <div v-if="candidates.length === 0" class="empty-column">
        Drop card here
      </div>
    </div>
  </div>
</template>

<style scoped>
.column {
  min-width: 220px;
  flex: 1;
  background: var(--bg-page);
  border: 1px solid var(--border);
  border-radius: 10px;
  padding: 8px;
  transition: border-color 0.15s, background 0.15s;
}

.column.drag-over {
  border-color: var(--primary);
  background: var(--primary-light);
}

.column-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 4px 4px 8px;
  border-bottom: 1px solid var(--border);
  margin-bottom: 8px;
}

.column-title {
  font-size: 12px;
  font-weight: 600;
  color: var(--text-secondary);
  text-transform: uppercase;
  letter-spacing: 0.04em;
}

.count-badge {
  background: var(--border);
  color: var(--text-secondary);
  font-size: 11px;
  font-weight: 600;
  padding: 1px 6px;
  border-radius: 10px;
}

.cards { display: flex; flex-direction: column; gap: 6px; }

.empty-column {
  border: 2px dashed var(--border);
  border-radius: var(--radius);
  padding: 16px;
  text-align: center;
  color: var(--text-secondary);
  font-size: 12px;
}
</style>
