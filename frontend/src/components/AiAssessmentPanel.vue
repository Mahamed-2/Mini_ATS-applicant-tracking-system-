<script setup lang="ts">
// AiAssessmentPanel.vue – displays the structured AI assessment result.
// Related: src/components/CandidateCard.vue (shows this panel when assessment is available)
//          src/stores/ats.ts (AiFeedback type)
//          backend/ai-service/main.py (AssessResponse model that produced this data)
//          backend/MiniAts.Api/Api/Controllers/AiController.cs (stored the result)

import type { AiFeedback } from '../stores/ats'

defineProps<{
  feedback: AiFeedback
}>()
</script>

<template>
  <div class="ai-panel">
    <!-- Score and provider -->
    <div class="ai-header">
      <div class="ai-score-large">
        <span class="score-number">{{ feedback.score }}</span>
        <span class="score-label">/100</span>
      </div>
      <span class="provider-tag">{{ feedback.provider }}</span>
    </div>

    <!-- Summary -->
    <p class="ai-summary">{{ feedback.summary }}</p>

    <!-- Strengths list -->
    <div v-if="feedback.strengths.length" class="ai-section">
      <div class="ai-section-title">✅ Strengths</div>
      <ul class="ai-list">
        <li v-for="(s, i) in feedback.strengths" :key="i">{{ s }}</li>
      </ul>
    </div>

    <!-- Concerns list -->
    <div v-if="feedback.concerns.length" class="ai-section">
      <div class="ai-section-title">⚠️ Concerns</div>
      <ul class="ai-list">
        <li v-for="(c, i) in feedback.concerns" :key="i">{{ c }}</li>
      </ul>
    </div>

    <!-- Interview questions -->
    <div v-if="feedback.questions.length" class="ai-section">
      <div class="ai-section-title">💬 Interview Questions</div>
      <ol class="ai-list">
        <li v-for="(q, i) in feedback.questions" :key="i">{{ q }}</li>
      </ol>
    </div>
  </div>
</template>

<style scoped>
.ai-panel {
  background: #f8fafc;
  border: 1px solid var(--border);
  border-radius: 8px;
  padding: 10px;
  margin-top: 6px;
  font-size: 12px;
}

.ai-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 6px;
}

.ai-score-large { display: flex; align-items: baseline; gap: 2px; }

.score-number {
  font-size: 22px;
  font-weight: 700;
  color: var(--primary);
  line-height: 1;
}

.score-label { font-size: 12px; color: var(--text-secondary); }

.provider-tag {
  font-size: 10px;
  font-weight: 600;
  text-transform: uppercase;
  padding: 2px 6px;
  border-radius: 10px;
  background: var(--border);
  color: var(--text-secondary);
}

.ai-summary {
  color: var(--text-secondary);
  line-height: 1.5;
  margin: 4px 0 8px;
}

.ai-section { margin-bottom: 6px; }

.ai-section-title {
  font-weight: 600;
  color: var(--text);
  margin-bottom: 3px;
}

.ai-list {
  margin: 0;
  padding-left: 16px;
  color: var(--text-secondary);
  display: flex;
  flex-direction: column;
  gap: 2px;
}
</style>
