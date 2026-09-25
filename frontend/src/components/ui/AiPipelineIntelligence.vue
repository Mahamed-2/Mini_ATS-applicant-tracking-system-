<script setup lang="ts">
/**
 * AiPipelineIntelligence.vue – AI Pipeline Report & Analysis Panel
 * Source of Truth: docs/DESIGN.md & .claude/skills/ats-orchestrator/reference/AI_REPORT.md
 * Integrates with POST /api/ai/reports/pipeline and Python /analyze endpoint.
 */
import { computed } from 'vue';
import { useAtsStore } from '@/stores/ats';
import { useAuthStore } from '@/stores/auth';
import { useToast } from '@/components/ui/useToast';
import { useI18n } from '@/i18n';
import AiGlowPanel from '@/components/ui/AiGlowPanel.vue';
import AiScoreRing from '@/components/ui/AiScoreRing.vue';
import Button from '@/components/ui/Button.vue';
import StageChip from '@/components/ui/StageChip.vue';
import ScoreBadge from '@/components/ui/ScoreBadge.vue';
import ProviderChip from '@/components/ui/ProviderChip.vue';
import Skeleton from '@/components/ui/Skeleton.vue';
import ErrorState from '@/components/ui/ErrorState.vue';
import {
  Sparkles,
  Download,
  Copy,
  CheckCircle2,
  AlertTriangle,
  ArrowRight,
  Clock,
  FileText
} from '@/lib/icons';

const atsStore = useAtsStore();
const authStore = useAuthStore();
const toast = useToast();
const { t } = useI18n();

const report = computed(() => atsStore.pipelineReport);
const isLoading = computed(() => atsStore.reportLoading);
const errorMessage = computed(() => atsStore.reportError);

// Effective customer ID for reporting
const effectiveCustomerId = computed(() => {
  return authStore.actAsCustomerId || authStore.profile?.id || '';
});

// Trigger report generation via backend
async function handleGenerateReport() {
  if (!effectiveCustomerId.value) {
    toast.error('Select an active company workspace to generate an AI report.');
    return;
  }

  try {
    await atsStore.generatePipelineReport(effectiveCustomerId.value);
    toast.success('AI Pipeline Report generated successfully.');
  } catch (err) {
    toast.error(`Report generation failed: ${(err as Error).message}`);
  }
}

// Build clean Markdown document from report payload
function generateMarkdown(): string {
  if (!report.value) return '';

  const r = report.value;
  const dateStr = new Date(r.generatedAt).toLocaleString('sv-SE');

  let md = `# AI Pipeline Intelligence Report\n\n`;
  md += `**Generated**: ${dateStr}  \n`;
  md += `**Company Scope**: ${authStore.profile?.companyName || 'Nordic Tech AB'}  \n`;
  md += `**Overall Health**: ${r.score}/100 (${r.rating.toUpperCase()})  \n`;
  md += `**Provider**: ${r.provider.toUpperCase()}  \n\n`;

  md += `## Executive Summary\n\n`;
  md += `> ${r.headline}\n\n`;
  md += `${r.summary}\n\n`;

  md += `## Pipeline Metrics\n\n`;
  md += `- **Active Candidates**: ${r.totalCandidates}\n`;
  md += `- **Active Roles**: ${r.totalJobs}\n`;
  md += `- **Hire Conversion Rate**: ${r.overallHireRate}%\n`;
  md += `- **Average AI Match Score**: ${r.avgAiScore}%\n`;
  md += `- **Verified LinkedIn Coverage**: ${r.coverage.linkedinPct}%\n`;
  md += `- **Stale Records (>7d)**: ${r.staleCandidates.length}\n\n`;

  md += `### Funnel Stage Conversion\n\n`;
  md += `| Stage | Candidates | Stage Conversion |\n`;
  md += `| :--- | :--- | :--- |\n`;
  for (const f of r.funnel) {
    md += `| ${f.stage} | ${f.count} | ${f.conversionPct}% |\n`;
  }
  md += `\n`;

  md += `## Key Strengths\n\n`;
  for (const s of r.strengths) {
    md += `- ✅ ${s}\n`;
  }
  md += `\n`;

  md += `## Bottlenecks & Risks\n\n`;
  for (const risk of r.risks) {
    md += `- ⚠️ ${risk}\n`;
  }
  md += `\n`;

  md += `## Recommended Recruiter Actions\n\n`;
  for (const rec of r.recommendations) {
    md += `- 💡 ${rec}\n`;
  }
  md += `\n`;

  md += `## Stage Insights\n\n`;
  for (const si of r.stageInsights) {
    md += `- **${si.stage.toUpperCase()}**: ${si.insight}\n`;
  }

  return md;
}

// Copy markdown to clipboard
async function copyMarkdown() {
  const md = generateMarkdown();
  if (!md) return;

  try {
    await navigator.clipboard.writeText(md);
    toast.success('Pipeline report copied to clipboard as Markdown.');
  } catch {
    toast.error('Unable to copy report to clipboard.');
  }
}

// Download markdown file
function downloadMarkdown() {
  const md = generateMarkdown();
  if (!md) return;

  const blob = new Blob([md], { type: 'text/markdown;charset=utf-8' });
  const url = URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = `pipeline-report-${new Date().toISOString().slice(0, 10)}.md`;
  document.body.appendChild(a);
  a.click();
  document.body.removeChild(a);
  URL.revokeObjectURL(url);
  toast.success('Markdown report downloaded.');
}

// Semantic color helper for ratings
const ratingBadgeClass = computed(() => {
  if (!report.value) return '';
  switch (report.value.rating) {
    case 'healthy':
      return 'bg-emerald-50 text-emerald-700 border-emerald-200 dark:bg-emerald-950/40 dark:text-emerald-400 dark:border-emerald-800';
    case 'risk':
      return 'bg-rose-50 text-rose-700 border-rose-200 dark:bg-rose-950/40 dark:text-rose-400 dark:border-rose-800';
    default:
      return 'bg-amber-50 text-amber-700 border-amber-200 dark:bg-amber-950/40 dark:text-amber-400 dark:border-amber-800';
  }
});
</script>

<template>
  <AiGlowPanel padding="lg" class="relative overflow-hidden">
    <!-- Header row -->
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4 pb-4 border-b border-border-subtle">
      <div class="flex items-center gap-3">
        <div class="w-9 h-9 rounded-md bg-ai-subtle border border-ai-border flex items-center justify-center text-ai-accent shadow-sm">
          <Sparkles class="w-5 h-5 animate-pulse" />
        </div>
        <div>
          <div class="flex items-center gap-2">
            <h2 class="text-headline-sm text-text-main font-semibold tracking-tight">{{ t('ai.panelTitle') }}</h2>
            <span class="inline-flex items-center px-1.5 py-0.5 rounded text-[10px] font-medium tracking-wide uppercase bg-ai-subtle text-ai-accent border border-ai-border">
              {{ t('ai.copilotAnalysis') }}
            </span>
          </div>
          <p class="text-body-sm text-text-muted mt-0.5">
            {{ t('ai.panelSubtitle') }}
          </p>
        </div>
      </div>

      <!-- Action buttons -->
      <div class="flex items-center gap-2">
        <template v-if="report">
          <Button
            variant="secondary"
            size="sm"
            @click="copyMarkdown"
            title="Copy Report as Markdown"
          >
            <template #iconLeft><Copy class="w-3.5 h-3.5" /></template>
            {{ t('ai.copyMd') }}
          </Button>

          <Button
            variant="secondary"
            size="sm"
            @click="downloadMarkdown"
            title="Download Report as .md file"
          >
            <template #iconLeft><Download class="w-3.5 h-3.5" /></template>
            {{ t('ai.exportMd') }}
          </Button>
        </template>

        <Button
          variant="ai"
          size="sm"
          :loading="isLoading"
          :disabled="isLoading || !effectiveCustomerId"
          @click="handleGenerateReport"
        >
          <template #iconLeft>
            <Sparkles class="w-4 h-4" />
          </template>
          {{ report ? t('ai.regenerateBtn') : t('ai.generateBtn') }}
        </Button>
      </div>
    </div>

    <!-- Error state -->
    <div v-if="errorMessage" class="py-6">
      <ErrorState
        title="Failed to generate AI pipeline report"
        :message="errorMessage"
        retry-text="Try Again"
        @retry="handleGenerateReport"
      />
    </div>

    <!-- Loading skeleton -->
    <div v-else-if="isLoading" class="py-6 space-y-6">
      <div class="grid grid-cols-1 lg:grid-cols-4 gap-4 items-center">
        <Skeleton height="90px" class="rounded-lg" />
        <div class="lg:col-span-3 space-y-2">
          <Skeleton height="24px" width="60%" class="rounded" />
          <Skeleton height="16px" width="95%" class="rounded" />
          <Skeleton height="16px" width="80%" class="rounded" />
        </div>
      </div>
      <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
        <Skeleton height="140px" class="rounded-lg" />
        <Skeleton height="140px" class="rounded-lg" />
        <Skeleton height="140px" class="rounded-lg" />
      </div>
    </div>

    <!-- Empty state (No report yet generated) -->
    <div v-else-if="!report" class="py-10">
      <div class="max-w-md mx-auto text-center space-y-4">
        <div class="w-12 h-12 mx-auto rounded-full bg-ai-subtle border border-ai-border flex items-center justify-center text-ai-accent">
          <Sparkles class="w-6 h-6" />
        </div>
        <div>
          <h3 class="text-headline-sm font-semibold text-text-main">
            {{ t('ai.emptyIntelligenceTitle') }}
          </h3>
          <p class="text-body-sm text-text-muted mt-1">
            {{ t('ai.emptyIntelligenceDesc') }}
          </p>
        </div>
        <Button
          variant="ai"
          size="md"
          :disabled="!effectiveCustomerId"
          @click="handleGenerateReport"
        >
          <template #iconLeft><Sparkles class="w-4 h-4" /></template>
          {{ t('ai.generateBtn') }}
        </Button>
      </div>
    </div>

    <!-- Generated Report view -->
    <div v-else class="pt-5 space-y-6">
      <!-- 1. Executive Narrative Banner -->
      <div class="bg-surface-canvas border border-border-subtle rounded-lg p-5">
        <div class="flex flex-col lg:flex-row items-start lg:items-center justify-between gap-6">
          <!-- Left: Score Ring & Rating -->
          <div class="flex items-center gap-5 shrink-0">
            <AiScoreRing
              :score="report.score"
              :size="84"
              :stroke-width="7"
              :label="t('ai.healthIndex')"
              color-scheme="rating"
              :rating="report.rating"
            />
            <div class="space-y-1.5">
              <div class="flex items-center gap-2">
                <span
                  :class="[
                    'px-2 py-0.5 rounded text-label-sm font-semibold uppercase tracking-wider border',
                    ratingBadgeClass,
                  ]"
                >
                  {{ report.rating }}
                </span>
                <ProviderChip :provider="report.provider" />
              </div>
              <div class="text-label-sm text-text-muted">
                {{ report.totalCandidates }} candidates • {{ report.totalJobs }} open roles
              </div>
            </div>
          </div>

          <!-- Right: Executive Headline & Narrative -->
          <div class="flex-1 space-y-2 border-t lg:border-t-0 lg:border-l border-border-subtle pt-4 lg:pt-0 lg:pl-6">
            <h3 class="text-headline-sm font-semibold text-text-main leading-snug">
              {{ report.headline }}
            </h3>
            <p class="text-body-md text-text-muted leading-relaxed">
              {{ report.summary }}
            </p>
          </div>
        </div>
      </div>

      <!-- 2. Computed Data-Viz Cards Row -->
      <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
        <!-- Funnel Progression -->
        <div class="bg-surface-card border border-border-subtle rounded-lg p-4 space-y-3">
          <div class="flex items-center justify-between">
            <div class="flex items-center gap-2">
              <FileText class="w-4 h-4 text-primary" />
              <h4 class="text-label-md font-semibold text-text-main">Stage Conversions</h4>
            </div>
            <span class="text-label-sm text-text-muted tabular-nums">
              Hire Rate: <strong class="text-text-main">{{ report.overallHireRate }}%</strong>
            </span>
          </div>

          <div class="space-y-2 pt-1">
            <div
              v-for="item in report.funnel"
              :key="item.stage"
              class="flex items-center justify-between text-body-sm"
            >
              <div class="flex items-center gap-2">
                <StageChip :stage="item.stage" size="sm" />
              </div>
              <div class="flex items-center gap-3">
                <span class="font-medium text-text-main tabular-nums">{{ item.count }}</span>
                <span class="text-text-muted text-[11px] tabular-nums w-12 text-right">
                  {{ item.conversionPct }}%
                </span>
              </div>
            </div>
          </div>
        </div>

        <!-- AI Match Distribution Histogram -->
        <div class="bg-surface-card border border-border-subtle rounded-lg p-4 space-y-3">
          <div class="flex items-center justify-between">
            <div class="flex items-center gap-2">
              <Sparkles class="w-4 h-4 text-ai-accent" />
              <h4 class="text-label-md font-semibold text-text-main">AI Score Spread</h4>
            </div>
            <span class="text-label-sm text-text-muted tabular-nums">
              Avg: <strong class="text-text-main">{{ report.avgAiScore }}%</strong>
            </span>
          </div>

          <div class="space-y-2 pt-1">
            <div
              v-for="bucket in report.scoreDistribution"
              :key="bucket.bucket"
              class="space-y-0.5"
            >
              <div class="flex items-center justify-between text-[11px] text-text-muted tabular-nums">
                <span>{{ bucket.bucket }}%</span>
                <span>{{ bucket.count }} ({{ bucket.percentage }}%)</span>
              </div>
              <div class="w-full h-2 rounded-full bg-surface-canvas overflow-hidden">
                <div
                  class="h-full bg-ai-accent rounded-full transition-all duration-300"
                  :style="{ width: `${Math.max(4, bucket.percentage)}%` }"
                />
              </div>
            </div>
          </div>
        </div>

        <!-- Data Quality Coverage -->
        <div class="bg-surface-card border border-border-subtle rounded-lg p-4 space-y-3">
          <div class="flex items-center justify-between">
            <div class="flex items-center gap-2">
              <Clock class="w-4 h-4 text-amber-500" />
              <h4 class="text-label-md font-semibold text-text-main">Data Completeness</h4>
            </div>
            <span class="text-label-sm text-text-muted tabular-nums">
              Stale: <strong :class="report.staleCandidates.length > 0 ? 'text-rose-600' : 'text-emerald-600'">{{ report.staleCandidates.length }}</strong>
            </span>
          </div>

          <div class="space-y-2 pt-1">
            <div class="flex items-center justify-between text-body-sm">
              <span class="text-text-muted">LinkedIn URL:</span>
              <span class="font-semibold text-text-main tabular-nums">{{ report.coverage.linkedinPct }}%</span>
            </div>
            <div class="flex items-center justify-between text-body-sm">
              <span class="text-text-muted">CV / Profile Text:</span>
              <span class="font-semibold text-text-main tabular-nums">{{ report.coverage.cvTextPct }}%</span>
            </div>
            <div class="flex items-center justify-between text-body-sm">
              <span class="text-text-muted">Short / Minimal CV:</span>
              <span class="font-semibold text-text-main tabular-nums">{{ report.coverage.shortCvPct }}%</span>
            </div>
            <div class="flex items-center justify-between text-body-sm">
              <span class="text-text-muted">Email Provided:</span>
              <span class="font-semibold text-text-main tabular-nums">{{ 100 - report.coverage.missingEmailPct }}%</span>
            </div>
          </div>
        </div>
      </div>

      <!-- 3. Qualitative Insights (Strengths, Risks, Recommendations) -->
      <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
        <!-- Strengths -->
        <div class="bg-surface-card border border-border-subtle rounded-lg p-4 space-y-2.5">
          <div class="flex items-center gap-2 text-emerald-600">
            <CheckCircle2 class="w-4 h-4 shrink-0" />
            <h4 class="text-label-md font-semibold uppercase tracking-wider text-text-main">
              {{ t('ai.demonstratedStrengths') }}
            </h4>
          </div>
          <ul class="space-y-2 text-body-sm text-text-muted">
            <li v-for="(str, idx) in report.strengths" :key="idx" class="flex items-start gap-2">
              <span class="text-emerald-500 font-bold shrink-0 mt-0.5">•</span>
              <span>{{ str }}</span>
            </li>
          </ul>
        </div>

        <!-- Risks -->
        <div class="bg-surface-card border border-border-subtle rounded-lg p-4 space-y-2.5">
          <div class="flex items-center gap-2 text-rose-600">
            <AlertTriangle class="w-4 h-4 shrink-0" />
            <h4 class="text-label-md font-semibold uppercase tracking-wider text-text-main">
              {{ t('ai.profileConcerns') }}
            </h4>
          </div>
          <ul class="space-y-2 text-body-sm text-text-muted">
            <li v-for="(risk, idx) in report.risks" :key="idx" class="flex items-start gap-2">
              <span class="text-rose-500 font-bold shrink-0 mt-0.5">•</span>
              <span>{{ risk }}</span>
            </li>
          </ul>
        </div>

        <!-- Recommendations -->
        <div class="bg-surface-card border border-border-subtle rounded-lg p-4 space-y-2.5">
          <div class="flex items-center gap-2 text-ai-accent">
            <Sparkles class="w-4 h-4 shrink-0" />
            <h4 class="text-label-md font-semibold uppercase tracking-wider text-text-main">
              {{ t('ai.actionPlanTitle') }}
            </h4>
          </div>
          <ul class="space-y-2 text-body-sm text-text-muted">
            <li v-for="(rec, idx) in report.recommendations" :key="idx" class="flex items-start gap-2">
              <ArrowRight class="w-3.5 h-3.5 text-ai-accent shrink-0 mt-0.5" />
              <span>{{ rec }}</span>
            </li>
          </ul>
        </div>
      </div>

      <!-- 4. Top/Bottom Candidates & Stage Insights -->
      <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
        <!-- Top Matches -->
        <div class="bg-surface-card border border-border-subtle rounded-lg p-4 space-y-3">
          <h4 class="text-label-md font-semibold text-text-main">Top Candidate Matches</h4>
          <div v-if="report.topCandidates.length === 0" class="text-body-sm text-text-muted">
            No candidates scored yet.
          </div>
          <div v-else class="space-y-2">
            <div
              v-for="cand in report.topCandidates"
              :key="cand.id"
              class="flex items-center justify-between p-2 rounded bg-surface-canvas text-body-sm"
            >
              <div>
                <div class="font-medium text-text-main">{{ cand.name }}</div>
                <div class="text-[11px] text-text-muted">{{ cand.jobTitle }}</div>
              </div>
              <div class="flex items-center gap-2">
                <StageChip :stage="cand.stage" size="sm" />
                <ScoreBadge :score="cand.score" />
              </div>
            </div>
          </div>
        </div>

        <!-- Stage Insights -->
        <div class="bg-surface-card border border-border-subtle rounded-lg p-4 space-y-3">
          <h4 class="text-label-md font-semibold text-text-main">Stage Cadence & Notes</h4>
          <div class="space-y-2">
            <div
              v-for="si in report.stageInsights"
              :key="si.stage"
              class="flex items-start gap-2.5 p-2 rounded bg-surface-canvas text-body-sm"
            >
              <StageChip :stage="si.stage" size="sm" class="shrink-0 mt-0.5" />
              <div class="text-text-muted text-[12px] leading-snug">
                {{ si.insight }}
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </AiGlowPanel>
</template>
