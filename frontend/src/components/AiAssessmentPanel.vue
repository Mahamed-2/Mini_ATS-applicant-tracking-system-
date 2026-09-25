<script setup lang="ts">
/**
 * AiAssessmentPanel.vue – Candidate CV Assessment Scorecard Panel
 * Source of Truth: docs/DESIGN.md & .claude/skills/ats-orchestrator/reference/PAGE_RESKIN.md
 * Integrates:
 * - POST /api/ai/candidates/{id}/assess?customerId=...
 * - AiScoreRing with color bands [0-39 rose, 40-69 amber, 70-100 emerald]
 * - AiGlowPanel container
 * - Strengths (CheckCircle2), Concerns (AlertTriangle), Questions (HelpCircle)
 */
import { computed } from 'vue';
import type { AiFeedback } from '@/stores/ats';
import AiGlowPanel from '@/components/ui/AiGlowPanel.vue';
import AiScoreRing from '@/components/ui/AiScoreRing.vue';
import ProviderChip from '@/components/ui/ProviderChip.vue';
import Button from '@/components/ui/Button.vue';
import Skeleton from '@/components/ui/Skeleton.vue';
import ErrorState from '@/components/ui/ErrorState.vue';
import {
  Sparkles,
  CheckCircle2,
  AlertTriangle,
  HelpCircle,
  ShieldCheck
} from '@/lib/icons';

interface Props {
  feedback?: AiFeedback | null;
  loading?: boolean;
  error?: string | null;
  canAssess?: boolean;
}

const props = withDefaults(defineProps<Props>(), {
  feedback: null,
  loading: false,
  error: null,
  canAssess: true,
});

const emit = defineEmits<{
  (e: 'assess'): void;
}>();

// Compute rating band based on score [0-39 risk, 40-69 watch, 70-100 healthy]
const ratingBand = computed<'healthy' | 'watch' | 'risk'>(() => {
  if (!props.feedback) return 'watch';
  const s = props.feedback.score;
  if (s >= 70) return 'healthy';
  if (s >= 40) return 'watch';
  return 'risk';
});

const ratingLabel = computed(() => {
  switch (ratingBand.value) {
    case 'healthy':
      return 'Strong Match';
    case 'watch':
      return 'Moderate Fit';
    case 'risk':
      return 'Low Alignment';
  }
});

const ratingColorClass = computed(() => {
  switch (ratingBand.value) {
    case 'healthy':
      return 'text-emerald-700 bg-emerald-50 border-emerald-200 dark:text-emerald-400 dark:bg-emerald-950/40 dark:border-emerald-800';
    case 'watch':
      return 'text-amber-700 bg-amber-50 border-amber-200 dark:text-amber-400 dark:bg-amber-950/40 dark:border-amber-800';
    case 'risk':
      return 'text-rose-700 bg-rose-50 border-rose-200 dark:text-rose-400 dark:bg-rose-950/40 dark:border-rose-800';
  }
});
</script>

<template>
  <div>
    <!-- Loading State -->
    <div v-if="loading" class="surface-1 bg-surface-card border border-border-subtle rounded-lg p-5 space-y-4">
      <div class="flex items-center gap-3">
        <Skeleton height="72px" width="72px" class="rounded-full shrink-0" />
        <div class="space-y-2 flex-1">
          <Skeleton height="20px" width="40%" class="rounded" />
          <Skeleton height="16px" width="70%" class="rounded" />
        </div>
      </div>
      <Skeleton height="60px" class="rounded-md" />
      <Skeleton height="100px" class="rounded-md" />
    </div>

    <!-- Error State -->
    <div v-else-if="error" class="surface-1 bg-surface-card border border-border-subtle rounded-lg p-5">
      <ErrorState
        title="AI assessment failed"
        :message="error"
        retry-text="Retry Assessment"
        @retry="emit('assess')"
      />
    </div>

    <!-- Unassessed State (e.g. Anna Lund before live demo) -->
    <div
      v-else-if="!feedback"
      class="surface-1 bg-surface-card border border-border-subtle rounded-lg p-6 text-center space-y-4 shadow-sm"
    >
      <div class="w-12 h-12 mx-auto rounded-full bg-ai-subtle border border-ai-border flex items-center justify-center text-ai-accent">
        <Sparkles class="w-6 h-6" />
      </div>
      <div class="max-w-xs mx-auto">
        <h4 class="text-sm font-semibold text-text-primary">AI Evaluation Pending</h4>
        <p class="text-xs text-text-muted mt-1 leading-relaxed">
          Analyze resume text against job requisition keywords, extract strengths, concerns, and interview prompts.
        </p>
      </div>
      <Button
        v-if="canAssess"
        variant="ai"
        size="md"
        @click="emit('assess')"
      >
        <template #iconLeft><Sparkles class="w-4 h-4" /></template>
        Assess CV with AI
      </Button>
    </div>

    <!-- Complete Assessment Card (AiGlowPanel) -->
    <AiGlowPanel v-else padding="lg" class="space-y-5">
      <!-- Header Row: Score Ring + Summary Details -->
      <div class="flex items-start justify-between gap-4 pb-4 border-b border-border-subtle">
        <div class="flex items-center gap-4">
          <AiScoreRing
            :score="feedback.score"
            :size="76"
            :stroke-width="6"
            label="AI Match"
            color-scheme="rating"
            :rating="ratingBand"
          />

          <div class="space-y-1">
            <div class="flex items-center gap-2">
              <span
                :class="[
                  'px-2 py-0.5 rounded text-[11px] font-semibold uppercase tracking-wider border',
                  ratingColorClass
                ]"
              >
                {{ ratingLabel }}
              </span>
              <ProviderChip :provider="feedback.provider" />
            </div>
            <p class="text-xs text-text-muted">
              Evaluated against role requirements
            </p>
          </div>
        </div>

        <Button
          variant="ai"
          size="sm"
          title="Re-run AI evaluation"
          @click="emit('assess')"
        >
          <template #iconLeft><Sparkles class="w-3.5 h-3.5" /></template>
          Re-assess
        </Button>
      </div>

      <!-- Narrative Summary -->
      <div class="space-y-1">
        <h4 class="text-label-sm font-semibold uppercase tracking-wider text-text-muted">
          Executive Match Summary
        </h4>
        <p class="text-body-sm text-text-primary leading-relaxed bg-surface-canvas p-3 rounded border border-border-subtle">
          {{ feedback.summary }}
        </p>
      </div>

      <!-- Strengths & Concerns Grid -->
      <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
        <!-- Strengths -->
        <div class="p-3 rounded bg-surface-card border border-border-subtle space-y-2">
          <div class="flex items-center gap-1.5 text-emerald-600 font-semibold text-xs uppercase tracking-wide">
            <CheckCircle2 class="w-4 h-4 shrink-0" />
            <span>Demonstrated Strengths</span>
          </div>
          <ul class="space-y-1.5 text-xs text-text-muted">
            <li v-for="(s, idx) in feedback.strengths" :key="idx" class="flex items-start gap-1.5">
              <span class="text-emerald-500 font-bold shrink-0">•</span>
              <span>{{ s }}</span>
            </li>
          </ul>
        </div>

        <!-- Concerns -->
        <div class="p-3 rounded bg-surface-card border border-border-subtle space-y-2">
          <div class="flex items-center gap-1.5 text-amber-600 font-semibold text-xs uppercase tracking-wide">
            <AlertTriangle class="w-4 h-4 shrink-0" />
            <span>Profile Concerns</span>
          </div>
          <ul class="space-y-1.5 text-xs text-text-muted">
            <li v-for="(c, idx) in feedback.concerns" :key="idx" class="flex items-start gap-1.5">
              <span class="text-amber-500 font-bold shrink-0">•</span>
              <span>{{ c }}</span>
            </li>
          </ul>
        </div>
      </div>

      <!-- Interview Questions -->
      <div v-if="feedback.questions?.length" class="p-3.5 rounded bg-surface-card border border-border-subtle space-y-2">
        <div class="flex items-center gap-1.5 text-ai-accent font-semibold text-xs uppercase tracking-wide">
          <HelpCircle class="w-4 h-4 shrink-0" />
          <span>Recommended Interview Prompts</span>
        </div>
        <ol class="space-y-2 text-xs text-text-muted list-decimal list-inside leading-relaxed">
          <li v-for="(q, idx) in feedback.questions" :key="idx" class="pl-1">
            <span class="text-text-primary">{{ q }}</span>
          </li>
        </ol>
      </div>

      <!-- Screening Disclaimer -->
      <div class="flex items-center gap-2 pt-2 border-t border-border-subtle text-[11px] text-text-muted">
        <ShieldCheck class="w-3.5 h-3.5 text-emerald-600 shrink-0" />
        <span>Objective screening aid. Final hiring decisions remain with the recruiter.</span>
      </div>
    </AiGlowPanel>
  </div>
</template>
