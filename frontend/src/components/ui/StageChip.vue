<script setup lang="ts">
/**
 * StageChip.vue – Pipeline Stage Status Pill with Exact Semantic Colors and Bilingual Translation
 * Source of Truth: docs/DESIGN.md & .claude/skills/ats-orchestrator/reference/DESIGN_TOKENS.md
 */
import { computed } from 'vue';
import { useI18n } from '@/i18n';

type CandidateStage = 'new' | 'screening' | 'interview' | 'offer' | 'hired' | 'rejected' | string;

interface Props {
  stage: CandidateStage;
  size?: 'sm' | 'md';
}

const props = withDefaults(defineProps<Props>(), {
  size: 'md',
});

const { t } = useI18n();

const normalizedStage = computed(() => props.stage.toLowerCase());

const stageClasses = computed(() => {
  switch (normalizedStage.value) {
    case 'new':
      return 'stage-new text-[#0284c7] bg-[#f0f9ff] border-[#bae6fd]';
    case 'screening':
      return 'stage-screening text-[#d97706] bg-[#fffbeb] border-[#fde68a]';
    case 'interview':
      return 'stage-interview text-[#1d68f0] bg-[#eff6ff] border-[#bfdbfe]';
    case 'offer':
      return 'stage-offer text-[#7c3aed] bg-[#f5f3ff] border-[#ddd6fe]';
    case 'hired':
      return 'stage-hired text-[#059669] bg-[#ecfdf5] border-[#a7f3d0]';
    case 'rejected':
      return 'stage-rejected text-[#e11d48] bg-[#fff1f2] border-[#fecdd3]';
    default:
      return 'text-text-variant bg-surface-hover border-border-subtle';
  }
});

const dotColor = computed(() => {
  switch (normalizedStage.value) {
    case 'new': return 'bg-[#0284c7]';
    case 'screening': return 'bg-[#d97706]';
    case 'interview': return 'bg-[#1d68f0]';
    case 'offer': return 'bg-[#7c3aed]';
    case 'hired': return 'bg-[#059669]';
    case 'rejected': return 'bg-[#e11d48]';
    default: return 'bg-text-muted';
  }
});

const stageLabel = computed(() => {
  switch (normalizedStage.value) {
    case 'new': return t('stage.applied');
    case 'screening': return t('stage.phoneScreen');
    case 'interview': return t('stage.techInterview');
    case 'offer': return t('stage.offer');
    case 'hired': return t('stage.hired');
    default: return props.stage;
  }
});
</script>

<template>
  <span
    :class="[
      'stage-chip inline-flex items-center gap-1.5 rounded-full font-semibold border select-none transition-colors',
      stageClasses,
      size === 'sm' ? 'px-1.5 py-0.5 text-[10px]' : 'px-2 py-0.5 text-[11px]',
    ]"
    :data-stage="normalizedStage"
  >
    <span :class="['w-1.5 h-1.5 rounded-full shrink-0', dotColor]" aria-hidden="true" />
    <span>{{ stageLabel }}</span>
  </span>
</template>
