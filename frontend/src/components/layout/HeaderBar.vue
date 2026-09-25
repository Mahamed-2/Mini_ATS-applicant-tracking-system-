<script setup lang="ts">
/**
 * HeaderBar.vue – 56px Glass Bar Header with Backdrop Blur
 * Source of Truth: docs/DESIGN.md & .claude/skills/ats-orchestrator/reference/SHELL_DASHBOARD.md
 */
import { ref, computed } from 'vue';
import { useRouter } from 'vue-router';
import {
  Sun,
  Moon,
  ShieldCheck,
  X,
  LogOut,
  ChevronDown,
  Building2,
  Columns2
} from '@/lib/icons';
import SearchInput from '../ui/SearchInput.vue';
import Avatar from '../ui/Avatar.vue';
import Badge from '../ui/Badge.vue';
import { useAuthStore } from '@/stores/auth';
import { useUiStore } from '@/stores/ui';

const router = useRouter();
const authStore = useAuthStore();
const uiStore = useUiStore();

const isUserMenuOpen = ref(false);
const searchQuery = ref('');

const displayName = computed(() => authStore.profile?.displayName || 'Seed Admin');
const email = computed(() => authStore.profile?.email || 'admin@nordic-recruit.demo');
const role = computed(() => authStore.profile?.role || 'admin');

function onSearchSubmit(query: string) {
  if (query.trim()) {
    router.push({ path: '/candidates', query: { search: query.trim() } });
  }
}

function openCommandPalette() {
  uiStore.setCommandPalette(true);
}

function toggleTheme() {
  uiStore.toggleColorMode();
}

function clearActAs() {
  authStore.setActAsCustomer(null);
}

async function handleSignOut() {
  isUserMenuOpen.value = false;
  await authStore.signOut();
  router.push('/login');
}
</script>

<template>
  <header
    :class="[
      'h-14 fixed top-0 right-0 z-40 bg-surface-card/85 backdrop-blur-[20px] border-b border-border-subtle flex items-center justify-between px-4 transition-all duration-200',
      uiStore.isSidebarCollapsed ? 'left-16' : 'left-60',
    ]"
  >
    <!-- Left Section: Mobile Menu Toggle + Omni-Search Input -->
    <div class="flex items-center gap-3 w-full max-w-md">
      <button
        type="button"
        class="sm:hidden text-text-muted hover:text-text-primary p-1.5 rounded hover:bg-surface-hover cursor-pointer"
        aria-label="Toggle navigation menu"
        @click="uiStore.toggleSidebar()"
      >
        <Columns2 class="w-4 h-4" />
      </button>

      <SearchInput
        v-model="searchQuery"
        placeholder="Search candidates, jobs… (⌘+K)"
        @submit="onSearchSubmit"
        @shortcut="openCommandPalette"
      />
    </div>

    <!-- Center Section: Acting-As Banner Chip (Admin only) -->
    <div v-if="authStore.isAdmin && authStore.actAsCustomerId" class="hidden md:flex items-center gap-1.5 px-2.5 py-1 bg-primary-subtle text-primary border border-primary/20 rounded-full text-xs font-medium">
      <Building2 class="w-3.5 h-3.5" />
      <span>Acting as: <strong>Nordic Tech AB</strong></span>
      <button
        type="button"
        class="ml-1 text-danger hover:opacity-80 p-0.5 rounded cursor-pointer"
        title="Exit customer scope"
        aria-label="Exit customer scope"
        @click="clearActAs"
      >
        <X class="w-3 h-3 stroke-[3]" />
      </button>
    </div>

    <!-- Right Section: Theme Toggle + User Menu -->
    <div class="flex items-center gap-2">
      <!-- Theme Toggle Button -->
      <button
        type="button"
        class="w-8 h-8 rounded-sm border border-border-subtle bg-surface-card hover:bg-surface-hover text-text-muted hover:text-text-primary flex items-center justify-center transition-colors cursor-pointer"
        :title="`Switch to ${uiStore.colorMode === 'light' ? 'Dark' : 'Light'} theme`"
        aria-label="Toggle theme"
        @click="toggleTheme"
      >
        <Sun v-if="uiStore.colorMode === 'dark'" class="w-4 h-4 text-warning" />
        <Moon v-else class="w-4 h-4 text-text-variant" />
      </button>

      <!-- User Menu Dropdown -->
      <div class="relative">
        <button
          type="button"
          class="flex items-center gap-2 p-1 rounded-sm hover:bg-surface-hover transition-colors cursor-pointer select-none"
          aria-haspopup="true"
          :aria-expanded="isUserMenuOpen"
          @click="isUserMenuOpen = !isUserMenuOpen"
        >
          <Avatar :name="displayName" size="sm" />
          <div class="hidden lg:flex flex-col text-left">
            <span class="text-xs font-semibold text-text-primary leading-tight">{{ displayName }}</span>
            <span class="text-[10px] text-text-muted leading-tight truncate max-w-[120px]">{{ email }}</span>
          </div>
          <Badge v-if="role === 'admin'" variant="ai" class="hidden sm:inline-flex">Admin</Badge>
          <ChevronDown class="w-3.5 h-3.5 text-text-muted" />
        </button>

        <!-- Dropdown Menu Popover -->
        <div
          v-if="isUserMenuOpen"
          class="surface-2 absolute right-0 mt-1 w-56 bg-surface-modal border border-border-subtle rounded-lg shadow-modal p-2 z-50 animate-in fade-in zoom-in-95 duration-100"
        >
          <div class="px-2 py-1.5 border-b border-border-subtle mb-1">
            <div class="text-xs font-bold text-text-primary">{{ displayName }}</div>
            <div class="text-[11px] text-text-muted truncate">{{ email }}</div>
            <div class="mt-1 flex items-center gap-1.5">
              <span class="text-[10px] uppercase font-semibold text-text-muted">Role:</span>
              <Badge :variant="role === 'admin' ? 'ai' : 'primary'">{{ role }}</Badge>
            </div>
          </div>

          <div class="flex flex-col gap-0.5">
            <button
              v-if="authStore.isAdmin"
              type="button"
              class="w-full text-left px-2 py-1.5 text-xs text-text-variant hover:text-text-primary hover:bg-surface-hover rounded-sm flex items-center gap-2 cursor-pointer"
              @click="isUserMenuOpen = false; router.push('/admin')"
            >
              <ShieldCheck class="w-3.5 h-3.5 text-primary" />
              <span>Admin Center</span>
            </button>

            <button
              type="button"
              class="w-full text-left px-2 py-1.5 text-xs text-danger hover:bg-danger-subtle rounded-sm flex items-center gap-2 cursor-pointer mt-1 pt-1.5 border-t border-border-subtle"
              @click="handleSignOut"
            >
              <LogOut class="w-3.5 h-3.5" />
              <span>Sign out</span>
            </button>
          </div>
        </div>
      </div>
    </div>
  </header>
</template>
