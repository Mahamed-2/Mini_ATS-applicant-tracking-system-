<script setup lang="ts">
// AppShell.vue – layout wrapper with header, navigation links, and logout button.
// Related: src/router.ts (RouterLink targets)
//          src/stores/auth.ts (profile, isAdmin, signOut)
//          src/styles/main.css (shell, header, nav CSS classes)

import { useAuthStore } from '../stores/auth'
import { useRouter } from 'vue-router'

const auth   = useAuthStore()
const router = useRouter()

async function handleSignOut() {
  await auth.signOut()
  // Redirect to login after signing out.
  router.push({ name: 'login' })
}
</script>

<template>
  <div class="shell">
    <!-- Header shown only when user is authenticated -->
    <header v-if="auth.isAuthenticated" class="header">
      <div class="header-brand">
        <span class="brand-icon">📋</span>
        <span class="brand-name">Mini ATS</span>
      </div>

      <nav class="nav">
        <RouterLink to="/"       class="nav-link">Dashboard</RouterLink>
        <RouterLink to="/kanban" class="nav-link">Kanban</RouterLink>
        <RouterLink v-if="auth.isAdmin" to="/admin" class="nav-link">Admin</RouterLink>
      </nav>

      <div class="header-right">
        <!-- Show role badge and company/email in header -->
        <span class="role-badge" :class="auth.profile?.role">
          {{ auth.profile?.role }}
        </span>
        <span class="user-name">{{ auth.profile?.displayName || auth.profile?.email }}</span>
        <!-- Show which customer admin is acting as -->
        <span v-if="auth.isAdmin && auth.actAsCustomerId" class="acting-as">
          Acting as customer
        </span>
        <button class="btn-ghost" @click="handleSignOut">Sign out</button>
      </div>
    </header>

    <!-- Main content area: RouterView renders the matched route -->
    <main class="content">
      <RouterView />
    </main>
  </div>
</template>

<style scoped>
.shell {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
  background: var(--bg-page);
}

.header {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 0 24px;
  height: 56px;
  background: var(--surface);
  border-bottom: 1px solid var(--border);
  flex-shrink: 0;
}

.header-brand {
  display: flex;
  align-items: center;
  gap: 8px;
  font-weight: 700;
  font-size: 16px;
  color: var(--primary);
}

.brand-icon { font-size: 20px; }

.nav {
  display: flex;
  gap: 4px;
  flex: 1;
}

.nav-link {
  padding: 6px 12px;
  border-radius: var(--radius);
  font-size: 14px;
  font-weight: 500;
  color: var(--text-secondary);
  text-decoration: none;
  transition: background 0.15s, color 0.15s;
}

.nav-link:hover { background: var(--bg-hover); color: var(--text); }
.nav-link.router-link-active { background: var(--primary-light); color: var(--primary); }

.header-right {
  display: flex;
  align-items: center;
  gap: 10px;
  font-size: 13px;
}

.role-badge {
  padding: 2px 8px;
  border-radius: 20px;
  font-size: 11px;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

.role-badge.admin    { background: #fef3c7; color: #92400e; }
.role-badge.customer { background: #dbeafe; color: #1e40af; }

.user-name { color: var(--text-secondary); }

.acting-as {
  background: #fef9c3;
  color: #854d0e;
  padding: 2px 8px;
  border-radius: 4px;
  font-size: 11px;
  font-weight: 600;
}

.content { flex: 1; overflow: auto; }
</style>
