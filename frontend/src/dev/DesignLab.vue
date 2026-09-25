<script setup lang="ts">
/**
 * DesignLab.vue – Storybook-free Visual Sanity Component Testing Lab
 * Rendered in development to verify all Recruiter Velocity components in light and dark modes.
 * Source of Truth: docs/DESIGN.md & .claude/skills/ats-orchestrator/reference/COMPONENT_KIT.md
 */
import { ref } from 'vue';
import Button from '../components/ui/Button.vue';
import IconButton from '../components/ui/IconButton.vue';
import Input from '../components/ui/Input.vue';
import Select from '../components/ui/Select.vue';
import Checkbox from '../components/ui/Checkbox.vue';
import Switch from '../components/ui/Switch.vue';
import Field from '../components/ui/Field.vue';
import Badge from '../components/ui/Badge.vue';
import Pill from '../components/ui/Pill.vue';
import StatCard from '../components/ui/StatCard.vue';
import ScoreBadge from '../components/ui/ScoreBadge.vue';
import StageChip from '../components/ui/StageChip.vue';
import ProviderChip from '../components/ui/ProviderChip.vue';
import AiScoreRing from '../components/ui/AiScoreRing.vue';
import CandidateMicroCard from '../components/ui/CandidateMicroCard.vue';
import MultiTenantSwitcher from '../components/ui/MultiTenantSwitcher.vue';
import { Sparkles, Sun, Moon, Plus } from '@/lib/icons';

const isDark = ref(false);
const checkboxVal = ref(true);
const switchVal = ref(false);
const inputVal = ref('Anna Lund');
const selectVal = ref('screening');
const tenantVal = ref('22222222-2222-4222-8222-222222222222');

const sampleCandidate = {
  id: '55555555-5555-4555-8555-555555555503',
  fullName: 'Maria Karlsson',
  stage: 'interview',
  jobTitle: 'Senior Frontend Engineer',
  aiScore: 86,
  linkedinUrl: 'https://linkedin.com/in/mariakarlsson',
  timeInStage: '2d ago',
};

const sampleTenants = [
  { id: '22222222-2222-4222-8222-222222222222', name: 'Nordic Tech AB', candidateCount: 10 },
  { id: '44444444-4444-4444-4444-444444444444', name: 'Stockholm Software', candidateCount: 0 },
];
</script>

<template>
  <div :class="['min-h-screen p-6 transition-colors', isDark ? 'dark bg-[#0b0f19] text-[#f8fafc]' : 'bg-[#f8f9ff] text-[#0f172a]']">
    <div class="max-w-5xl mx-auto flex flex-col gap-6">
      <!-- Header -->
      <div class="flex items-center justify-between pb-4 border-b border-border-subtle">
        <div>
          <h1 class="headline-lg">Recruiter Velocity ATS – Design Lab</h1>
          <p class="body-sm text-text-muted">Component visual sanity testing in light and dark modes.</p>
        </div>
        <Button variant="secondary" size="sm" @click="isDark = !isDark">
          <template #icon-left>
            <Moon v-if="!isDark" class="w-3.5 h-3.5" />
            <Sun v-else class="w-3.5 h-3.5" />
          </template>
          Toggle {{ isDark ? 'Light' : 'Dark' }} Theme
        </Button>
      </div>

      <!-- Buttons Section -->
      <div class="surface-1 p-4 rounded-lg flex flex-col gap-3">
        <h2 class="headline-sm">Buttons & Icon Buttons</h2>
        <div class="flex flex-wrap items-center gap-3">
          <Button variant="primary">Primary Cobalt</Button>
          <Button variant="secondary">Secondary Slate</Button>
          <Button variant="ghost">Ghost Button</Button>
          <Button variant="danger">Danger Rose</Button>
          <Button variant="ai">
            <template #icon-left><Sparkles class="w-3.5 h-3.5" /></template>
            AI Violet Action
          </Button>
          <Button variant="primary" loading>Loading</Button>
          <IconButton aria-label="Add" variant="secondary"><Plus class="w-4 h-4" /></IconButton>
        </div>
      </div>

      <!-- Pipeline Stage Chips, Badges & Pills -->
      <div class="surface-1 p-4 rounded-lg flex flex-col gap-3">
        <h2 class="headline-sm">Badges, Pills & Stage Chips (6 Strict Semantic Tokens)</h2>
        <div class="flex flex-wrap items-center gap-2">
          <Badge variant="primary">Badge Primary</Badge>
          <Badge variant="ai">AI Badge</Badge>
          <Pill variant="success">Pill Verified</Pill>
          <Pill variant="warning">Pill Pending</Pill>
          <StageChip stage="new" />
          <StageChip stage="screening" />
          <StageChip stage="interview" />
          <StageChip stage="offer" />
          <StageChip stage="hired" />
          <StageChip stage="rejected" />
        </div>
      </div>

      <!-- AI Elements -->
      <div class="surface-1 p-4 rounded-lg flex flex-col gap-3">
        <h2 class="headline-sm">AI Score Badges & Rings</h2>
        <div class="flex items-center gap-6">
          <ScoreBadge :score="86" />
          <ScoreBadge :score="42" />
          <ScoreBadge :score="null" />
          <ProviderChip provider="mock" />
          <ProviderChip provider="openai" />
          <AiScoreRing :score="86" :size="70" />
        </div>
      </div>

      <!-- Form Controls -->
      <div class="surface-1 p-4 rounded-lg flex flex-col gap-3">
        <h2 class="headline-sm">Form Inputs & Field Controls</h2>
        <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
          <Field id="lab-candidate-name" label="Candidate Name" hint="Full legal or preferred name">
            <Input id="lab-candidate-name" name="candidateName" v-model="inputVal" />
          </Field>
          <Field id="lab-active-stage" label="Active Stage">
            <Select id="lab-active-stage" name="activeStage" v-model="selectVal" :options="[{ value: 'new', label: 'New' }, { value: 'screening', label: 'Screening' }]" />
          </Field>
          <div class="flex flex-col gap-2 pt-5">
            <div class="flex items-center gap-2">
              <Checkbox v-model="checkboxVal" />
              <span class="text-xs">Include archived candidates</span>
            </div>
            <div class="flex items-center gap-2">
              <Switch v-model="switchVal" />
              <span class="text-xs">Real-time telemetry sync</span>
            </div>
          </div>
        </div>
      </div>

      <!-- ATS Cards Grid -->
      <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
        <StatCard label="Pipeline Velocity" value="84%" trend="+12%" trend-positive subtext="Average conversion rate" />
        <CandidateMicroCard :candidate="sampleCandidate" />
      </div>

      <!-- Multi-Tenant Switcher -->
      <div class="surface-1 p-4 rounded-lg">
        <h2 class="headline-sm mb-3">Multi-Tenant Scoping Radio Switcher</h2>
        <MultiTenantSwitcher v-model="tenantVal" :tenants="sampleTenants" />
      </div>
    </div>
  </div>
</template>
