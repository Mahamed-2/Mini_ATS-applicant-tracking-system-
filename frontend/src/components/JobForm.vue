<script setup lang="ts">
// JobForm.vue – modal for creating a new job posting.
// Related: src/views/DashboardView.vue (renders this modal)
//          src/stores/ats.ts (createJob action)
//          backend/MiniAts.Api/Api/Controllers/JobsController.cs (POST /api/jobs)

import { ref } from 'vue'
import { useAtsStore } from '../stores/ats'

const props = defineProps<{
  customerId: string
}>()

const emit = defineEmits<{
  close:   []
  created: []
}>()

const ats = useAtsStore()

const title       = ref('')
const description = ref('')
const loading     = ref(false)
const error       = ref<string | null>(null)

async function handleSubmit() {
  if (!title.value.trim()) {
    error.value = 'Job title is required.'
    return
  }

  error.value = null
  loading.value = true

  try {
    // createJob POSTs to /api/jobs with customerId, title, description.
    await ats.createJob({
      customerId:  props.customerId,
      title:       title.value.trim(),
      description: description.value.trim() || undefined
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
    <div class="modal">
      <div class="modal-header">
        <h2>New Job Posting</h2>
        <button class="btn-ghost" @click="emit('close')">✕</button>
      </div>

      <form @submit.prevent="handleSubmit" class="modal-body">
        <div class="field">
          <label for="job-title">Title *</label>
          <input
            id="job-title"
            v-model="title"
            type="text"
            placeholder="Senior Frontend Engineer"
            required
            autofocus
          />
        </div>

        <div class="field">
          <label for="job-desc">Description</label>
          <textarea
            id="job-desc"
            v-model="description"
            rows="4"
            placeholder="Describe the role, responsibilities, and requirements…"
          />
        </div>

        <p v-if="error" class="error-msg">{{ error }}</p>

        <div class="modal-footer">
          <button type="button" class="btn-outline" @click="emit('close')">Cancel</button>
          <button type="submit" class="btn-primary" :disabled="loading">
            {{ loading ? 'Creating…' : 'Create Job' }}
          </button>
        </div>
      </form>
    </div>
  </div>
</template>
