/**
 * ui.ts – Pinia UI Store for AppShell & Navigation Telemetry
 * Source of Truth: docs/DESIGN.md & .claude/skills/ats-orchestrator/reference/SHELL_DASHBOARD.md
 */
import { defineStore } from 'pinia';
import { ref } from 'vue';

export const useUiStore = defineStore('ui', () => {
  const isSidebarCollapsed = ref(false);
  const isMobileNavOpen = ref(false);
  const colorMode = ref<'light' | 'dark'>('light');
  const isCommandPaletteOpen = ref(false);

  // Initialize color mode from localStorage or system preference
  const savedTheme = localStorage.getItem('mini_ats_theme') as 'light' | 'dark' | null;
  if (savedTheme) {
    colorMode.value = savedTheme;
    applyTheme(savedTheme);
  }

  function applyTheme(theme: 'light' | 'dark') {
    if (theme === 'dark') {
      document.documentElement.classList.add('dark');
      document.documentElement.setAttribute('data-theme', 'dark');
    } else {
      document.documentElement.classList.remove('dark');
      document.documentElement.setAttribute('data-theme', 'light');
    }
  }

  function toggleSidebar() {
    isSidebarCollapsed.value = !isSidebarCollapsed.value;
  }

  function setMobileNav(open: boolean) {
    isMobileNavOpen.value = open;
  }

  function toggleColorMode() {
    colorMode.value = colorMode.value === 'light' ? 'dark' : 'light';
    localStorage.setItem('mini_ats_theme', colorMode.value);
    applyTheme(colorMode.value);
  }

  function setCommandPalette(open: boolean) {
    isCommandPaletteOpen.value = open;
  }

  function reset() {
    isSidebarCollapsed.value = false;
    isMobileNavOpen.value = false;
    colorMode.value = 'light';
    localStorage.setItem('mini_ats_theme', 'light');
    applyTheme('light');
  }

  return {
    isSidebarCollapsed,
    isMobileNavOpen,
    colorMode,
    isCommandPaletteOpen,
    toggleSidebar,
    setMobileNav,
    toggleColorMode,
    setCommandPalette,
    reset,
  };
});
