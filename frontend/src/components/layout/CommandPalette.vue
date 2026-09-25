<script setup lang="ts">
/**
 * CommandPalette.vue – ⌘K Fuzzy Omnibar Palette for Routes, Jobs & Candidates
 * Source of Truth: docs/DESIGN.md & .claude/skills/ats-orchestrator/reference/SHELL_DASHBOARD.md
 */
import { ref, computed, onMounted, onUnmounted } from 'vue';
import { useRouter } from 'vue-router';
import {
  Search,
  LayoutDashboard,
  Briefcase,
  Users,
  SquareKanban,
  ShieldCheck
} from '@/lib/icons';
import { useUiStore } from '@/stores/ui';
import { useAtsStore } from '@/stores/ats';

const router = useRouter();
const uiStore = useUiStore();
const atsStore = useAtsStore();

const query = ref('');
const selectedIndex = ref(0);

const routes = [
  { name: 'Dashboard', path: '/', icon: LayoutDashboard, category: 'Navigation' },
  { name: 'Jobs Manager', path: '/jobs', icon: Briefcase, category: 'Navigation' },
  { name: 'Candidates Directory', path: '/candidates', icon: Users, category: 'Navigation' },
  { name: 'Pipeline Kanban', path: '/kanban', icon: SquareKanban, category: 'Navigation' },
  { name: 'System Admin', path: '/admin', icon: ShieldCheck, category: 'Navigation' },
];

const filteredItems = computed(() => {
  const q = query.value.trim().toLowerCase();
  const list: Array<{ name: string; path: string; category: string; icon: any; subtext?: string }> = [];

  // Match routes
  for (const r of routes) {
    if (!q || r.name.toLowerCase().includes(q)) {
      list.push(r);
    }
  }

  // Match candidates
  for (const c of atsStore.candidates) {
    if (!q || c.fullName.toLowerCase().includes(q) || (c.email && c.email.toLowerCase().includes(q))) {
      list.push({
        name: c.fullName,
        path: `/candidates/${c.id}`,
        category: 'Candidates',
        icon: Users,
        subtext: c.stage,
      });
    }
  }

  // Match jobs
  for (const j of atsStore.jobs) {
    if (!q || j.title.toLowerCase().includes(q)) {
      list.push({
        name: j.title,
        path: `/jobs`,
        category: 'Jobs',
        icon: Briefcase,
        subtext: j.status,
      });
    }
  }

  return list.slice(0, 10);
});

function selectItem(item: { path: string }) {
  uiStore.setCommandPalette(false);
  query.value = '';
  router.push(item.path);
}

function handleKeydown(e: KeyboardEvent) {
  // Toggle with ⌘K or Ctrl+K
  if ((e.metaKey || e.ctrlKey) && e.key.toLowerCase() === 'k') {
    e.preventDefault();
    uiStore.setCommandPalette(!uiStore.isCommandPaletteOpen);
  }

  // Toggle sidebar with [
  if (e.key === '[' && !['INPUT', 'TEXTAREA'].includes((e.target as HTMLElement).tagName)) {
    e.preventDefault();
    uiStore.toggleSidebar();
  }

  // Close with Escape
  if (e.key === 'Escape' && uiStore.isCommandPaletteOpen) {
    uiStore.setCommandPalette(false);
  }

  // Arrow navigation inside palette
  if (uiStore.isCommandPaletteOpen) {
    if (e.key === 'ArrowDown') {
      e.preventDefault();
      selectedIndex.value = (selectedIndex.value + 1) % Math.max(1, filteredItems.value.length);
    } else if (e.key === 'ArrowUp') {
      e.preventDefault();
      selectedIndex.value = (selectedIndex.value - 1 + filteredItems.value.length) % Math.max(1, filteredItems.value.length);
    } else if (e.key === 'Enter' && filteredItems.value[selectedIndex.value]) {
      e.preventDefault();
      selectItem(filteredItems.value[selectedIndex.value]);
    }
  }
}

onMounted(() => window.addEventListener('keydown', handleKeydown));
onUnmounted(() => window.removeEventListener('keydown', handleKeydown));
</script>

<template>
  <Teleport to="body">
    <div
      v-if="uiStore.isCommandPaletteOpen"
      class="fixed inset-0 z-50 flex items-start justify-center pt-20 p-4 bg-slate-900/40 backdrop-blur-sm transition-opacity"
      @click.self="uiStore.setCommandPalette(false)"
    >
      <div
        role="dialog"
        aria-modal="true"
        class="surface-2 bg-surface-modal border border-border-subtle rounded-lg shadow-modal w-full max-w-lg overflow-hidden flex flex-col animate-in fade-in zoom-in-95 duration-100"
      >
        <!-- Search Header -->
        <div class="flex items-center px-3.5 py-2.5 border-b border-border-subtle bg-surface-card">
          <Search class="w-4 h-4 text-text-muted mr-2.5 shrink-0" />
          <input
            v-model="query"
            type="text"
            placeholder="Type a command or search candidates, jobs…"
            class="w-full bg-transparent text-xs text-text-primary placeholder:text-text-muted outline-none"
            autofocus
          />
          <kbd class="text-[10px] font-mono px-1.5 py-0.5 bg-surface-hover border border-border-subtle rounded text-text-muted">
            ESC
          </kbd>
        </div>

        <!-- Results List -->
        <div class="max-h-72 overflow-y-auto p-1.5 divide-y divide-border-subtle/30">
          <div v-if="filteredItems.length === 0" class="p-6 text-center text-xs text-text-muted">
            No matching routes or records found.
          </div>
          <button
            v-for="(item, idx) in filteredItems"
            :key="item.path + item.name"
            type="button"
            :class="[
              'w-full flex items-center justify-between px-2.5 py-2 rounded-sm text-xs transition-colors cursor-pointer text-left',
              selectedIndex === idx ? 'bg-primary-subtle text-primary font-semibold' : 'text-text-primary hover:bg-surface-hover',
            ]"
            @mouseenter="selectedIndex = idx"
            @click="selectItem(item)"
          >
            <div class="flex items-center gap-2.5 min-w-0">
              <component :is="item.icon" class="w-3.5 h-3.5 shrink-0 text-text-muted" />
              <span class="truncate">{{ item.name }}</span>
            </div>
            <div class="flex items-center gap-2">
              <span v-if="item.subtext" class="text-[10px] text-text-muted capitalize">
                {{ item.subtext }}
              </span>
              <span class="text-[10px] text-text-muted uppercase font-mono tracking-wider">
                {{ item.category }}
              </span>
            </div>
          </button>
        </div>
      </div>
    </div>
  </Teleport>
</template>
