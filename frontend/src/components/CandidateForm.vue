<script setup lang="ts">
// CandidateForm.vue – modal for adding a candidate with full profile fields.
// Related: src/views/KanbanView.vue (renders this modal)
//          src/stores/ats.ts (createCandidate action)
//          backend/MiniAts.Api/Api/Controllers/CandidatesController.cs (POST /api/candidates)
//          Domain/Candidate.cs (CandidateStage enum values)

import { ref } from 'vue'
import { useAtsStore } from '../stores/ats'
import type { Job } from '../stores/ats'

const props = defineProps<{
  customerId: string
  jobs: Job[]
}>()

const emit = defineEmits<{
  close:   []
  created: []
}>()

const ats = useAtsStore()

// Kanban stage options matching public.candidate_stage enum.
const STAGES = ['new', 'screening', 'interview', 'offer', 'hired', 'rejected']

const form = ref({
  fullName:    '',
  email:       '',
  linkedinUrl: '',
  jobId:       '',
  stage:       'new',
  cvText:      '',
  summary:     ''
})

const loading = ref(false)
const error   = ref<string | null>(null)

async function handleSubmit() {
  if (!form.value.fullName.trim()) {
    error.value = 'Full name is required.'
    return
  }

  error.value = null
  loading.value = true

  try {
    // createCandidate POSTs to /api/candidates with all profile fields.
    await ats.createCandidate({
      customerId:  props.customerId,
      jobId:       form.value.jobId || null,
      fullName:    form.value.fullName.trim(),
      email:       form.value.email.trim() || null,
      linkedinUrl: form.value.linkedinUrl.trim() || null,
      cvText:      form.value.cvText.trim() || null,
      summary:     form.value.summary.trim() || null,
      stage:       form.value.stage
    })
    emit('created')
  } catch (e) {
    error.value = (e as Error).message
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="modal-backdrop" @click.self="emit('close')">
    <div class="modal modal-lg">
      <div class="modal-header">
        <h2>Add Candidate</h2>
        <button class="btn-ghost" @click="emit('close')">✕</button>
      </div>

      <form @submit.prevent="handleSubmit" class="modal-body">
        <div class="form-row">
          <div class="field">
            <label for="cand-name">Full Name *</label>
            <input id="cand-name" v-model="form.fullName" type="text" placeholder="Anna Lund" required autofocus />
          </div>

          <div class="field">
            <label for="cand-email">Email</label>
            <input id="cand-email" v-model="form.email" type="email" placeholder="anna@example.com" />
          </div>
        </div>

        <div class="form-row">
          <div class="field">
            <label for="cand-linkedin">LinkedIn URL</label>
            <input id="cand-linkedin" v-model="form.linkedinUrl" type="url" placeholder="https://linkedin.com/in/anna-lund" />
          </div>

          <div class="field">
            <label for="cand-job">Job</label>
            <select id="cand-job" v-model="form.jobId">
              <option value="">— No job linked —</option>
              <option v-for="job in jobs" :key="job.id" :value="job.id">{{ job.title }}</option>
            </select>
          </div>
        </div>

        <div class="field">
          <label for="cand-stage">Initial Stage</label>
          <select id="cand-stage" v-model="form.stage">
            <option v-for="s in STAGES" :key="s" :value="s">{{ s }}</option>
          </select>
        </div>

        <div class="field">
          <label for="cand-summary">Profile Summary</label>
          <textarea id="cand-summary" v-model="form.summary" rows="2"
            placeholder="Brief summary about the candidate…" />
        </div>

        <div class="field">
          <label for="cand-cv">CV / Profile Text</label>
          <textarea id="cand-cv" v-model="form.cvText" rows="5"
            placeholder="Paste CV text or profile description here. Used for AI assessment." />
        </div>

        <p v-if="error" class="error-msg">{{ error }}</p>

        <div class="modal-footer">
          <button type="button" class="btn-outline" @click="emit('close')">Cancel</button>
          <button type="submit" class="btn-primary" :disabled="loading">
            {{ loading ? 'Adding…' : 'Add Candidate' }}
          </button>
        </div>
      </form>
    </div>
  </div>
</template>

<style scoped>
.form-row { display: grid; grid-template-columns: 1fr 1fr; gap: 12px; }
</style>
