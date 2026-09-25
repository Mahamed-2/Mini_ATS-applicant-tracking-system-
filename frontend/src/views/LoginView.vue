<script setup lang="ts">
// LoginView.vue – Supabase password authentication form.
// Related: src/stores/auth.ts (signIn action)
//          src/router.ts (redirects to /dashboard after login)
//          src/lib/supabase.ts (Supabase client used by auth store)
//          backend/MiniAts.Api/Program.cs (JWT validated after this login)

import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'

const auth   = useAuthStore()
const router = useRouter()

const email    = ref('')
const password = ref('')
const error    = ref<string | null>(null)
const loading  = ref(false)

async function handleLogin() {
  if (!email.value || !password.value) {
    error.value = 'Email and password are required.'
    return
  }

  error.value = null
  loading.value = true

  try {
    // signIn calls supabase.auth.signInWithPassword then loads profile from /api/account/me.
    await auth.signIn(email.value, password.value)
    router.push({ name: 'dashboard' })
  } catch (e) {
    error.value = (e as Error).message
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="login-page">
    <div class="login-card">
      <div class="login-logo">
        <span class="logo-icon">📋</span>
        <h1 class="logo-name">Mini ATS</h1>
        <p class="logo-sub">Applicant Tracking System</p>
      </div>

      <form class="login-form" @submit.prevent="handleLogin">
        <div class="field">
          <label for="email">Email</label>
          <input
            id="email"
            v-model="email"
            type="email"
            placeholder="admin@demo-ats.local"
            autocomplete="email"
            required
          />
        </div>

        <div class="field">
          <label for="password">Password</label>
          <input
            id="password"
            v-model="password"
            type="password"
            placeholder="••••••••"
            autocomplete="current-password"
            required
          />
        </div>

        <!-- Error message from Supabase or backend -->
        <p v-if="error" class="error-msg" role="alert">{{ error }}</p>

        <button type="submit" class="btn-primary btn-full" :disabled="loading">
          {{ loading ? 'Signing in…' : 'Sign in' }}
        </button>
      </form>

      <p class="login-hint">
        Demo: <code>admin@demo-ats.local</code> or <code>customer@demo-ats.local</code>
      </p>
    </div>
  </div>
</template>

<style scoped>
.login-page {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: var(--bg-page);
  padding: 24px;
}

.login-card {
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: 16px;
  padding: 40px;
  width: 100%;
  max-width: 400px;
  box-shadow: 0 4px 24px rgba(0,0,0,0.06);
}

.login-logo {
  text-align: center;
  margin-bottom: 32px;
}

.logo-icon { font-size: 40px; display: block; margin-bottom: 8px; }

.logo-name {
  font-size: 24px;
  font-weight: 700;
  color: var(--primary);
  margin: 0 0 4px;
}

.logo-sub {
  font-size: 13px;
  color: var(--text-secondary);
  margin: 0;
}

.login-form { display: flex; flex-direction: column; gap: 16px; }

.error-msg {
  color: var(--danger);
  font-size: 13px;
  padding: 8px 12px;
  background: #fef2f2;
  border: 1px solid #fecaca;
  border-radius: var(--radius);
}

.login-hint {
  margin-top: 20px;
  font-size: 12px;
  color: var(--text-secondary);
  text-align: center;
}

.login-hint code {
  background: var(--bg-page);
  padding: 1px 4px;
  border-radius: 3px;
  font-size: 11px;
}
</style>
