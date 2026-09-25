<script setup lang="ts">
/**
 * AppShell.vue – Global Master Layout Shell
 * Source of Truth: docs/DESIGN.md & .claude/skills/ats-orchestrator/reference/SHELL_DASHBOARD.md
 * Integrates:
 * - 240px Fixed-Left Sidebar (z-50)
 * - 56px Glass Header Bar (z-40)
 * - Main Viewport (z-0)
 * - ⌘K Command Palette & Toast Portal
 */
import { useAuthStore } from '@/stores/auth';
import { useUiStore } from '@/stores/ui';
import SidebarNav from './layout/SidebarNav.vue';
import HeaderBar from './layout/HeaderBar.vue';
import CommandPalette from './layout/CommandPalette.vue';
import ToastPortal from './ui/ToastPortal.vue';

const authStore = useAuthStore();
const uiStore = useUiStore();
</script>

<template>
  <div class="min-h-screen bg-canvas text-text-primary flex flex-col">
    <!-- Authenticated Master Layout -->
    <template v-if="authStore.isAuthenticated">
      <!-- Fixed Left Sidebar Rail (z-50) -->
      <SidebarNav />

      <!-- Fixed Glass Top Header (z-40) -->
      <HeaderBar />

      <!-- Main Scrollable Canvas Viewport (z-0) -->
      <main
        :class="[
          'flex-1 pt-14 min-h-screen transition-all duration-200 flex flex-col',
          uiStore.isSidebarCollapsed ? 'ml-16' : 'ml-60',
        ]"
      >
        <div class="flex-1 p-4 md:p-6 max-w-7xl w-full mx-auto">
          <RouterView />
        </div>
      </main>

      <!-- Global Portals: ⌘K Command Palette & Sonner-Style Toasts -->
      <CommandPalette />
      <ToastPortal />
    </template>

    <!-- Unauthenticated / Login Viewport -->
    <template v-else>
      <main class="min-h-screen w-full flex flex-col">
        <RouterView />
      </main>
      <ToastPortal />
    </template>
  </div>
</template>
