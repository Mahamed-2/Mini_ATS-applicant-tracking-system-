<script setup lang="ts">
/**
 * CandidateMicroCard.vue – Compact Kanban & List MicroCard
 * Source of Truth: docs/DESIGN.md & .claude/skills/ats-orchestrator/reference/DESIGN_TOKENS.md
 */
import { computed } from 'vue';
import { GripVertical, Zap, Linkedin } from '@/lib/icons';
import StageChip from './StageChip.vue';
import Avatar from './Avatar.vue';

export interface CandidateCardData {
  id: string;
  fullName: string;
  email?: string | null;
  linkedinUrl?: string | null;
  stage: string;
  jobTitle?: string | null;
  summary?: string | null;
  aiScore?: number | null;
  timeInStage?: string;
}

interface Props {
  candidate: CandidateCardData;
  draggable?: boolean;
}

const props = withDefaults(defineProps<Props>(), {
  draggable: true,
});

const emit = defineEmits<{
  (e: 'click', candidate: CandidateCardData): void;
  (e: 'moveStage', candidate: CandidateCardData, stage: string): void;
}>();

const hasLinkedin = computed(() => !!props.candidate.linkedinUrl);
</script>

<template>
  <div
    class="surface-1 bg-surface-card border border-border-subtle rounded-lg p-3 hover:border-border-strong hover:shadow-card transition-all cursor-pointer group select-none relative"
    @click="emit('click', candidate)"
  >
    <!-- Header row: Name + Zap match pill + Drag handle -->
    <div class="flex items-center justify-between gap-1.5 mb-1.5">
      <div class="flex items-center gap-2 min-w-0">
        <Avatar :name="candidate.fullName" size="sm" />
        <span class="text-xs font-semibold text-text-primary truncate">
          {{ candidate.fullName }}
        </span>
      </div>

      <div class="flex items-center gap-1 shrink-0">
        <span
          v-if="candidate.aiScore !== null && candidate.aiScore !== undefined"
          class="inline-flex items-center gap-0.5 px-1.5 py-0.5 bg-ai-subtle text-ai-accent border border-ai-border rounded-full text-[10px] font-semibold tabular-nums"
        >
          <Zap class="w-2.5 h-2.5" />
          <span>{{ Math.round(candidate.aiScore) }}%</span>
        </span>
        <button
          v-if="draggable"
          type="button"
          class="text-text-muted hover:text-text-primary p-0.5 rounded cursor-grab active:cursor-grabbing"
          aria-label="Drag candidate"
          @click.stop
        >
          <GripVertical class="w-3.5 h-3.5" />
        </button>
      </div>
    </div>

    <!-- Sub-row: Job title or role -->
    <div v-if="candidate.jobTitle" class="text-[11px] text-text-muted truncate mb-2">
      {{ candidate.jobTitle }}
    </div>

    <!-- Bottom metadata: StageChip + LinkedIn status + Time in stage -->
    <div class="flex items-center justify-between pt-1 border-t border-border-subtle/50 text-[11px] text-text-muted">
      <StageChip :stage="candidate.stage" size="sm" />

      <div class="flex items-center gap-2">
        <a
          v-if="hasLinkedin"
          :href="candidate.linkedinUrl!"
          target="_blank"
          rel="noopener noreferrer"
          class="text-[#0a66c2] hover:opacity-80"
          title="LinkedIn Profile"
          @click.stop
        >
          <Linkedin class="w-3 h-3" />
        </a>
        <span v-else class="text-text-muted text-[10px]" title="No LinkedIn URL">
          —
        </span>

        <span v-if="candidate.timeInStage" class="text-[10px] tabular-nums text-text-muted">
          {{ candidate.timeInStage }}
        </span>
      </div>
    </div>
  </div>
</template>
