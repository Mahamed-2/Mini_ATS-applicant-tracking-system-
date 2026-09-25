<script setup lang="ts">
// DashboardView.vue – job list and create job form for the active customer.
// Related: src/stores/ats.ts (loadJobs, createJob)
//          src/stores/auth.ts (effectiveCustomerId)
//          src/components/JobForm.vue (modal for creating jobs)
//          backend/MiniAts.Api/Api/Controllers/JobsController.cs (GET /api/jobs, POST /api/jobs)

import { onMounted, ref } from 'vue'
import { useAuthStore } from '../stores/auth'
import { useAtsStore } from '../stores/ats'
import JobForm from '../components/JobForm.vue'

const auth = useAuthStore()
const ats  = useAtsStore()

// Controls visibility of the create job modal.
const showJobForm = ref(false)

// Load jobs when the dashboard mounts.
onMounted(() => ats.loadJobs(auth.effectiveCustomerId))

// Reload job list after form submission.
function onJobCreated() {
  showJobForm.value = false
  ats.loadJobs(auth.effectiveCustomerId)
}
</script>

<template>
  <div class="page">
    <div class="page-header">
      <div>
        <h1 class="page-title">Dashboard</h1>
        <p class="page-sub">
          {{ auth.profile?.companyName || auth.profile?.email }}
        </p>
      </div>
      <button class="btn-primary" @click="showJobForm = true">+ New Job</button>
    </div>

    <!-- Loading / error states -->
    <p v-if="ats.loading" class="state-msg">Loading jobs…</p>
    <p v-else-if="ats.error" class="state-msg error">{{ ats.error }}</p>

    <!-- Job list table -->
    <div v-else class="card">
      <table v-if="ats.jobs.length" class="table">
        <thead>
          <tr>
            <th>Title</th>
            <th>Status</th>
            <th>Created</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="job in ats.jobs" :key="job.id">
            <td class="font-medium">{{ job.title }}</td>
            <td><span class="status-badge" :class="job.status">{{ job.status }}</span></td>
            <td class="text-secondary">{{ new Date(job.createdAt).toLocaleDateString() }}</td>
          </tr>
        </tbody>
      </table>

      <div v-else class="empty-state">
        <p>No jobs yet. Create your first job posting.</p>
        <button class="btn-primary" @click="showJobForm = true">Create Job</button>
      </div>
    </div>

    <!-- Quick stats row -->
    <div class="stats-row" v-if="ats.jobs.length">
      <div class="stat-card">
        <div class="stat-number">{{ ats.jobs.length }}</div>
        <div class="stat-label">Total Jobs</div>
      </div>
      <div class="stat-card">
        <div class="stat-number">{{ ats.jobs.filter(j => j.status === 'active').length }}</div>
        <div class="stat-label">Active</div>
      </div>
    </div>

    <!-- Create job modal -->
    <JobForm
      v-if="showJobForm"
      :customer-id="auth.effectiveCustomerId"
      @close="showJobForm = false"
      @created="onJobCreated"
    />
  </div>
</template>

<style scoped>
.stats-row {
  display: flex;
  gap: 12px;
  margin-top: 16px;
}

.stat-card {
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: var(--radius);
  padding: 16px 24px;
  text-align: center;
  min-width: 120px;
}

.stat-number {
  font-size: 28px;
  font-weight: 700;
  color: var(--primary);
}

.stat-label {
  font-size: 12px;
  color: var(--text-secondary);
  margin-top: 2px;
}
</style>
