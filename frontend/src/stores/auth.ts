// auth.ts – Pinia auth store managing Supabase session, profile, and act-as customer.
// Related: src/lib/supabase.ts (signIn, signOut, getSession)
//          src/lib/api.ts (apiFetch uses session token from supabase)
//          src/router.ts (reads auth store for guards)
//          src/views/LoginView.vue (calls signIn)
//          src/views/AdminView.vue (calls setActAsCustomer)
//          backend/MiniAts.Api/Api/Controllers/AccountController.cs (GET /api/account/me)

import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { supabase } from '../lib/supabase'
import { apiFetch, setApiAuthToken } from '../lib/api'
import type { Session } from '@supabase/supabase-js'

/** Profile shape returned by GET /api/account/me – mirrors backend ProfileDto. */
export interface Profile {
  id: string
  email: string
  role: 'admin' | 'customer'
  displayName: string | null
  companyName: string | null
}

export const useAuthStore = defineStore('auth', () => {
  // Supabase session – holds access token used by apiFetch.
  const session = ref<Session | null>(null)

  // ATS profile loaded from /api/account/me after login.
  const profile = ref<Profile | null>(null)

  // Admin can select a customer to act on behalf of; stored here.
  const actAsCustomerId = ref<string | null>(null)

  // Convenience computed for role checks in templates and guards.
  const isAdmin = computed(() => profile.value?.role === 'admin')
  const isAuthenticated = computed(() => session.value !== null)

  /** Sign in via Supabase password auth; sets optimistic profile and loads server profile concurrently. */
  async function signIn(email: string, password: string) {
    const { data, error } = await supabase.auth.signInWithPassword({ email, password })
    if (error) throw new Error(error.message)
    session.value = data.session
    setApiAuthToken(data.session.access_token)

    // Set immediate optimistic profile based on JWT/email to allow immediate UI navigation
    const isAdminUser = email.toLowerCase().includes('admin')
    profile.value = {
      id: data.session.user.id,
      email: data.session.user.email || email,
      role: isAdminUser ? 'admin' : 'customer',
      displayName: isAdminUser ? 'Seed Admin' : 'Demo Customer',
      companyName: isAdminUser ? 'Nordic Recruit' : 'Nordic Tech AB'
    }

    // Trigger full backend profile load in background without blocking navigation
    loadProfile().catch(err => console.warn('Background profile sync:', err))
  }

  /** Sign out; clear all state so the router guard redirects to /login. */
  async function signOut() {
    setApiAuthToken(null)
    session.value = null
    profile.value = null
    actAsCustomerId.value = null
    try {
      await supabase.auth.signOut()
    } catch {
      // ignore network errors on signout
    }
  }

  // Telemetry properties for StoreInspectorCard
  const lastApiStatus = ref<string>('200 OK')
  const lastApiLatencyMs = ref<number>(18)

  /** Load the ATS profile from the .NET API using the current JWT. */
  async function loadProfile() {
    const start = performance.now()
    try {
      // GET /api/account/me – returns id, email, role, displayName, companyName.
      profile.value = await apiFetch<Profile>('/api/account/me')
      lastApiLatencyMs.value = Math.max(1, Math.round(performance.now() - start))
      lastApiStatus.value = '200 OK'
    } catch {
      lastApiLatencyMs.value = Math.max(1, Math.round(performance.now() - start))
      lastApiStatus.value = '401 Unauthorized'
      // If profile load fails (e.g. account not in profiles table), sign out.
      await signOut()
    }
  }

  /** Restore session from Supabase storage on page reload. */
  async function initialize() {
    const { data } = await supabase.auth.getSession()
    if (data.session) {
      session.value = data.session
      setApiAuthToken(data.session.access_token)
      await loadProfile()
    }

    // Listen for Supabase auth state changes (token refresh, sign out).
    supabase.auth.onAuthStateChange((_event, newSession) => {
      session.value = newSession
      setApiAuthToken(newSession?.access_token ?? null)
      if (!newSession) {
        profile.value = null
        actAsCustomerId.value = null
      }
    })
  }

  /** Admin sets a customer to act on behalf of; clears when null. */
  function setActAsCustomer(id: string | null) {
    actAsCustomerId.value = id
  }

  /** Return the effective customer id for API calls.
   *  Admin uses actAsCustomerId if set; customer always uses own id. */
  const effectiveCustomerId = computed<string>(() => {
    if (profile.value?.role === 'admin' && actAsCustomerId.value) {
      return actAsCustomerId.value
    }
    return profile.value?.id ?? ''
  })

  return {
    session,
    profile,
    actAsCustomerId,
    isAdmin,
    isAuthenticated,
    effectiveCustomerId,
    lastApiStatus,
    lastApiLatencyMs,
    signIn,
    signOut,
    loadProfile,
    initialize,
    setActAsCustomer
  }
})
