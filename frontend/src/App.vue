<script setup lang="ts">
// App.vue – root component wrapping all views via AppShell or RouterView.
// Related: src/router.ts (RouterView renders matched route)
//          src/stores/auth.ts (initialize called here on app boot)
//          src/components/AppShell.vue (layout for authenticated views)

import { onMounted } from 'vue'
import { useAuthStore } from './stores/auth'
import AppShell from './components/AppShell.vue'

const authStore = useAuthStore()

// Restore Supabase session from local storage on page reload.
onMounted(() => authStore.initialize())
</script>

<template>
  <!-- AppShell provides header, navigation, and logout for all authenticated views. -->
  <!-- RouterView renders the matched route component into the shell's content area. -->
  <AppShell />
</template>
