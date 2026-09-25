<script setup lang="ts">
/**
 * SidebarNav.vue – 240px Fixed-Left Navigation Rail with W3C Compliance
 * Source of Truth: docs/DESIGN.md & .claude/skills/ats-orchestrator/reference/SHELL_DASHBOARD.md
 */
import { computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import {
  LayoutDashboard,
  Briefcase,
  Users,
  SquareKanban,
  ShieldCheck,
  Settings,
  Zap,
  Columns2
} from '@/lib/icons';
import { useAuthStore } from '@/stores/auth';
import { useUiStore } from '@/stores/ui';
import { useAtsStore } from '@/stores/ats';

const route = useRoute();
const router = useRouter();
const authStore = useAuthStore();
const uiStore = useUiStore();
const atsStore = useAtsStore();

const jobsCount = computed(() => atsStore.jobs.length);
const candidatesCount = computed(() => atsStore.candidates.length);

const recruitmentItems = computed(() => [
  {
    name: 'Dashboard',
    path: '/',
    icon: LayoutDashboard,
    active: route.path === '/' || route.path === '/dashboard',
  },
  {
    name: 'Jobs',
    path: '/jobs',
    icon: Briefcase,
    badge: jobsCount.value > 0 ? jobsCount.value : undefined,
    active: route.path.startsWith('/jobs'),
  },
  {
    name: 'Candidates',
    path: '/candidates',
    icon: Users,
    badge: candidatesCount.value > 0 ? candidatesCount.value : undefined,
    active: route.path === '/candidates',
  },
  {
    name: 'Kanban Board',
    path: '/kanban',
    icon: SquareKanban,
    active: route.path === '/kanban',
  },
]);

const adminItems = computed(() => [
  {
    name: 'System Admin',
    path: '/admin',
    icon: ShieldCheck,
    isSpecial: true,
    active: route.path === '/admin',
  },
  {
    name: 'Settings',
    path: '#settings',
    icon: Settings,
    active: false,
    disabled: true,
  },
]);

const activeCompany = computed(() => {
  if (authStore.actAsCustomerId) {
    return 'Nordic Tech AB';
  }
  return authStore.profile?.companyName || 'Nordic Tech AB';
});

function navigate(path: string) {
  if (path.startsWith('#')) return;
  router.push(path);
  uiStore.setMobileNav(false);
}
</script>

<template>
  <aside
    :class="[
      'fixed top-0 bottom-0 left-0 z-50 bg-surface-card border-r border-border-subtle flex flex-col justify-between transition-all duration-200 select-none shadow-sm',
      uiStore.isSidebarCollapsed ? 'w-16' : 'w-60',
    ]"
  >
    <!-- Top Branding & Collapse Button -->
    <div>
      <div class="h-14 flex items-center justify-between px-3.5 border-b border-border-subtle">
        <div class="flex items-center gap-2.5 overflow-hidden cursor-pointer" @click="navigate('/')">
          <!-- Brand Mark Logo -->
          <div class="w-8 h-8 rounded-sm bg-primary text-white flex items-center justify-center shrink-0 shadow-sm font-bold text-sm tracking-tight">
            <Zap class="w-4 h-4 fill-white" />
          </div>
          <div v-if="!uiStore.isSidebarCollapsed" class="flex flex-col min-w-0">
            <span class="text-xs font-bold tracking-tight text-text-primary uppercase">Mini ATS</span>
            <span class="text-[10px] text-text-muted font-medium">Recruiter Velocity</span>
          </div>
        </div>

        <button
          type="button"
          class="text-text-muted hover:text-text-primary p-1 rounded hover:bg-surface-hover cursor-pointer"
          title="Toggle sidebar ([)"
          @click="uiStore.toggleSidebar()"
        >
          <Columns2 class="w-4 h-4" />
        </button>
      </div>

      <!-- Navigation Links -->
      <div class="p-2.5 flex flex-col gap-4 overflow-y-auto">
        <!-- RECRUITMENT Group -->
        <div>
          <span
            v-if="!uiStore.isSidebarCollapsed"
            class="text-[10px] font-bold text-text-muted uppercase tracking-wider px-2 block mb-1"
          >
            RECRUITMENT
          </span>
          <nav class="flex flex-col gap-0.5">
            <button
              v-for="item in recruitmentItems"
              :key="item.path"
              type="button"
              :class="[
                'flex items-center justify-between w-full px-2.5 py-1.5 rounded-sm text-xs font-medium transition-colors cursor-pointer text-left',
                item.active
                  ? 'bg-primary text-white font-semibold shadow-sm'
                  : 'text-text-variant hover:text-text-primary hover:bg-surface-hover',
              ]"
              :title="item.name"
              @click="navigate(item.path)"
            >
              <div class="flex items-center gap-2.5 min-w-0">
                <component :is="item.icon" class="w-4 h-4 shrink-0" />
                <span v-if="!uiStore.isSidebarCollapsed" class="truncate">{{ item.name }}</span>
              </div>
              <span
                v-if="!uiStore.isSidebarCollapsed && item.badge !== undefined"
                :class="[
                  'text-[10px] tabular-nums px-1.5 py-0.2 rounded-full font-semibold',
                  item.active ? 'bg-white/20 text-white' : 'bg-surface-hover text-text-muted border border-border-subtle',
                ]"
              >
                {{ item.badge }}
              </span>
            </button>
          </nav>
        </div>

        <!-- ADMINISTRATION Group -->
        <div v-if="authStore.isAdmin">
          <span
            v-if="!uiStore.isSidebarCollapsed"
            class="text-[10px] font-bold text-text-muted uppercase tracking-wider px-2 block mb-1"
          >
            ADMINISTRATION
          </span>
          <nav class="flex flex-col gap-0.5">
            <button
              v-for="item in adminItems"
              :key="item.name"
              type="button"
              :disabled="item.disabled"
              :class="[
                'flex items-center justify-between w-full px-2.5 py-1.5 rounded-sm text-xs font-medium transition-colors cursor-pointer text-left',
                item.active
                  ? 'bg-primary text-white font-semibold shadow-sm'
                  : 'text-text-variant hover:text-text-primary hover:bg-surface-hover',
                item.disabled ? 'opacity-40 pointer-events-none' : '',
              ]"
              :title="item.name"
              @click="navigate(item.path)"
            >
              <div class="flex items-center gap-2.5 min-w-0">
                <component :is="item.icon" class="w-4 h-4 shrink-0" />
                <span v-if="!uiStore.isSidebarCollapsed" class="truncate">{{ item.name }}</span>
              </div>
              <span
                v-if="!uiStore.isSidebarCollapsed && item.isSpecial"
                :class="[
                  'text-[10px] px-1.5 py-0.2 rounded font-semibold',
                  item.active ? 'bg-white/20 text-white' : 'bg-ai-subtle text-ai-accent border border-ai-border',
                ]"
              >
                Admin
              </span>
            </button>
          </nav>
        </div>
      </div>
    </div>

    <!-- Footer: Active Workspace & Live Status Dot -->
    <div class="p-3 border-t border-border-subtle bg-surface-hover/30">
      <div v-if="!uiStore.isSidebarCollapsed" class="flex flex-col gap-1">
        <div class="flex items-center gap-1.5 text-[11px] text-text-muted">
          <span class="w-2 h-2 rounded-full bg-success shrink-0 animate-pulse" />
          <span class="font-medium">Active Workspace • Live</span>
        </div>
        <span class="text-xs font-bold text-text-primary truncate" :title="activeCompany">
          {{ activeCompany }}
        </span>
      </div>
      <div v-else class="flex justify-center" :title="activeCompany">
        <span class="w-2.5 h-2.5 rounded-full bg-success" />
      </div>
    </div>
  </aside>
</template>
