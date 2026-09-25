// ats.ts – Pinia ATS store for jobs, candidates, and AI assessment.
// Related: src/lib/api.ts (apiFetch for all backend calls)
//          src/stores/auth.ts (effectiveCustomerId for scoping)
//          src/views/DashboardView.vue (uses jobs state)
//          src/views/KanbanView.vue (uses candidates state, calls moveStage)
//          src/components/AiAssessmentPanel.vue (calls assessCandidate)
//          backend/MiniAts.Api/Api/Controllers/ (all endpoints)

import { defineStore } from 'pinia'
import { ref } from 'vue'
import { apiFetch } from '../lib/api'

// ── Types ─────────────────────────────────────────────────────────────────────

/** Mirrors backend JobDto. */
export interface Job {
  id: string
  customerId: string
  title: string
  description: string | null
  status: string
  createdAt: string
  updatedAt: string
}

/** AI feedback stored in candidates.ai_feedback (jsonb). */
export interface AiFeedback {
  score: number
  summary: string
  strengths: string[]
  concerns: string[]
  questions: string[]
  provider: 'mock' | 'llm'
}

/** Mirrors backend CandidateDto; stage is lowercase string. */
export interface Candidate {
  id: string
  customerId: string
  jobId: string | null
  fullName: string
  email: string | null
  linkedinUrl: string | null
  cvText: string | null
  summary: string | null
  stage: string
  aiScore: number | null
  aiFeedback: AiFeedback | null
  createdAt: string
  updatedAt: string
}

/** Request body for creating a job. */
export interface CreateJobDto {
  customerId: string
  title: string
  description?: string
}

/** Request body for creating a candidate. */
export interface CreateCandidateDto {
  customerId: string
  jobId?: string | null
  fullName: string
  email?: string | null
  linkedinUrl?: string | null
  cvText?: string | null
  summary?: string | null
  stage?: string
}

// ── Store ─────────────────────────────────────────────────────────────────────

export const useAtsStore = defineStore('ats', () => {
  // List of jobs for the active customer.
  const jobs = ref<Job[]>([])

  // Flat list of candidates; grouped into columns by KanbanView.
  const candidates = ref<Candidate[]>([])

  // Active Kanban filters; updated by KanbanView filter controls.
  const filters = ref<{ jobId: string; search: string }>({ jobId: '', search: '' })

  // Loading and error state for UI feedback.
  const loading = ref(false)
  const error = ref<string | null>(null)

  /** Load all jobs for a customer from GET /api/jobs. */
  async function loadJobs(customerId: string) {
    if (!customerId) return
    loading.value = true
    error.value = null
    try {
      jobs.value = await apiFetch<Job[]>(`/api/jobs?customerId=${customerId}`)
    } catch (e) {
      error.value = (e as Error).message
    } finally {
      loading.value = false
    }
  }

  /** Load candidates with optional filters for Kanban view. */
  async function loadCandidates(customerId: string, jobId?: string, search?: string) {
    if (!customerId) return
    loading.value = true
    error.value = null
    try {
      const qs = new URLSearchParams({ customerId })
      if (jobId)  qs.set('jobId', jobId)
      if (search) qs.set('search', search)
      candidates.value = await apiFetch<Candidate[]>(`/api/candidates?${qs.toString()}`)
    } catch (e) {
      error.value = (e as Error).message
    } finally {
      loading.value = false
    }
  }

  /** Create a job via POST /api/jobs. */
  async function createJob(dto: CreateJobDto): Promise<Job> {
    const job = await apiFetch<Job>('/api/jobs', {
      method: 'POST',
      body: JSON.stringify(dto)
    })
    jobs.value.unshift(job)  // prepend so newest appears first
    return job
  }

  /** Create a candidate via POST /api/candidates. */
  async function createCandidate(dto: CreateCandidateDto): Promise<Candidate> {
    const candidate = await apiFetch<Candidate>('/api/candidates', {
      method: 'POST',
      body: JSON.stringify(dto)
    })
    candidates.value.unshift(candidate)
    return candidate
  }

  /** Move a candidate to a new stage via PATCH /api/candidates/{id}/stage. */
  async function moveStage(candidateId: string, stage: string, customerId: string) {
    await apiFetch(`/api/candidates/${candidateId}/stage?customerId=${customerId}`, {
      method: 'PATCH',
      body: JSON.stringify({ stage })
    })
    // Update local state immediately so UI is responsive before next full reload.
    const candidate = candidates.value.find(c => c.id === candidateId)
    if (candidate) candidate.stage = stage
  }

  /** Run AI assessment via POST /api/ai/candidates/{id}/assess. */
  async function assessCandidate(candidateId: string, customerId: string): Promise<AiFeedback> {
    const result = await apiFetch<AiFeedback>(
      `/api/ai/candidates/${candidateId}/assess?customerId=${customerId}`,
      { method: 'POST' }
    )
    // Update the local candidate with AI result so the card shows the score.
    const candidate = candidates.value.find(c => c.id === candidateId)
    if (candidate) {
      candidate.aiScore = result.score
      candidate.aiFeedback = result
    }
    return result
  }

  return {
    jobs,
    candidates,
    filters,
    loading,
    error,
    loadJobs,
    loadCandidates,
    createJob,
    createCandidate,
    moveStage,
    assessCandidate
  }
})
