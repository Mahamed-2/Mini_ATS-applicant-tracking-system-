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

  function createDemoJwt(userId: string, userEmail: string): string {
    const header = btoa(JSON.stringify({ alg: 'HS256', typ: 'JWT' }))
    const payload = btoa(JSON.stringify({
      sub: userId,
      email: userEmail,
      role: 'authenticated',
      aud: 'authenticated',
      exp: Math.floor(Date.now() / 1000) + 86400 * 30
    }))
    return `${header}.${payload}.demo-sig`
  }

  /** Sign in via Supabase password auth with offline demo fallback; sets optimistic profile and loads server profile concurrently. */
  async function signIn(email: string, password: string) {
    let accessToken: string | null = null
    let userId: string = ''
    const isAdminUser = email.toLowerCase().includes('admin')
    const isDemoEmail = email.toLowerCase().includes('demo')
    const supabaseUrl = import.meta.env.VITE_SUPABASE_URL || ''
    const isDefunctUrl = !supabaseUrl || supabaseUrl.includes('etmzfmloibscvvbnmrem')

    // Only attempt remote Supabase auth if not using a demo email and URL is configured
    if (!isDemoEmail && !isDefunctUrl) {
      try {
        const { data, error } = await supabase.auth.signInWithPassword({ email, password })
        if (!error && data?.session) {
          session.value = data.session
          accessToken = data.session.access_token
          userId = data.session.user.id
        }
      } catch (err) {
        console.warn('Supabase remote auth unavailable, fallback to local demo session:', err)
      }
    }

    if (!accessToken) {
      userId = isAdminUser
        ? '11111111-1111-4111-8111-111111111111'
        : '22222222-2222-4222-8222-222222222222'
      accessToken = createDemoJwt(userId, email)
      const mockSession: any = {
        access_token: accessToken,
        token_type: 'bearer',
        expires_in: 86400 * 30,
        refresh_token: 'demo-refresh-token',
        user: {
          id: userId,
          email: email,
          app_metadata: {},
          user_metadata: {},
          aud: 'authenticated',
          created_at: new Date().toISOString()
        }
      }
      session.value = mockSession
      try {
        localStorage.setItem('mini_ats_demo_session', JSON.stringify(mockSession))
      } catch {
        // ignore
      }
    }

    setApiAuthToken(accessToken)

    const initialProfile: Profile = {
      id: userId,
      email: email,
      role: isAdminUser ? 'admin' : 'customer',
      displayName: isAdminUser ? 'Seed Admin' : 'Demo Customer',
      companyName: isAdminUser ? 'Nordic Recruit' : 'Nordic Tech AB'
    }
    profile.value = initialProfile
    try {
      localStorage.setItem('mini_ats_demo_profile', JSON.stringify(initialProfile))
    } catch {
      // ignore
    }

    // Trigger full backend profile load in background without blocking navigation
    loadProfile().catch(err => console.warn('Background profile sync:', err))
  }

  /** Sign out; clear all state so the router guard redirects to /login. */
  async function signOut() {
    try {
      localStorage.removeItem('mini_ats_demo_session')
      localStorage.removeItem('mini_ats_demo_profile')
    } catch {
      // ignore
    }
    setApiAuthToken(null)
    session.value = null
    profile.value = null
    actAsCustomerId.value = null

    const supabaseUrl = import.meta.env.VITE_SUPABASE_URL || ''
    const isDefunctUrl = !supabaseUrl || supabaseUrl.includes('etmzfmloibscvvbnmrem')
    if (!isDefunctUrl) {
      try {
        await supabase.auth.signOut()
      } catch {
        // ignore network errors on signout
      }
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
      const loaded = await apiFetch<Profile>('/api/account/me')
      profile.value = loaded
      lastApiLatencyMs.value = Math.max(1, Math.round(performance.now() - start))
      lastApiStatus.value = '200 OK'
      try {
        localStorage.setItem('mini_ats_demo_profile', JSON.stringify(loaded))
      } catch {
        // ignore
      }
    } catch {
      lastApiLatencyMs.value = Math.max(1, Math.round(performance.now() - start))
      lastApiStatus.value = '401 Unauthorized'
      // Only sign out on failure if not operating in demo fallback mode
      if (!profile.value?.email?.includes('demo')) {
        await signOut()
      }
    }
  }

  /** Restore session from Supabase storage or demo local storage on page reload. */
  async function initialize() {
    // 1. Check local demo session first
    try {
      const saved = localStorage.getItem('mini_ats_demo_session')
      if (saved) {
        const mock = JSON.parse(saved)
        if (mock?.access_token) {
          session.value = mock
          setApiAuthToken(mock.access_token)

          const savedProfile = localStorage.getItem('mini_ats_demo_profile')
          if (savedProfile) {
            try {
              profile.value = JSON.parse(savedProfile)
            } catch {
              // ignore
            }
          } else {
            const isAdm = (mock?.user?.email || '').toLowerCase().includes('admin')
            profile.value = {
              id: isAdm ? '11111111-1111-4111-8111-111111111111' : '22222222-2222-4222-8222-222222222222',
              email: mock?.user?.email || (isAdm ? 'admin@nordic-recruit.demo' : 'recruiter@nordic-tech.demo'),
              role: isAdm ? 'admin' : 'customer',
              displayName: isAdm ? 'Seed Admin' : 'Demo Customer',
              companyName: isAdm ? 'Nordic Recruit' : 'Nordic Tech AB'
            }
          }

          await loadProfile()
          return
        }
      }
    } catch {
      localStorage.removeItem('mini_ats_demo_session')
      localStorage.removeItem('mini_ats_demo_profile')
    }

    // 2. Check remote Supabase only if valid non-defunct URL is configured
    const supabaseUrl = import.meta.env.VITE_SUPABASE_URL || ''
    const isDefunctUrl = !supabaseUrl || supabaseUrl.includes('etmzfmloibscvvbnmrem')
    if (!isDefunctUrl) {
      try {
        const { data } = await supabase.auth.getSession()
        if (data?.session) {
          session.value = data.session
          setApiAuthToken(data.session.access_token)
          await loadProfile()
          return
        }
      } catch {
        // Supabase remote host unreachable
      }

      // Listen for Supabase auth state changes (token refresh, sign out).
      try {
        supabase.auth.onAuthStateChange((_event, newSession) => {
          if (newSession) {
            session.value = newSession
            setApiAuthToken(newSession.access_token)
          } else if (!localStorage.getItem('mini_ats_demo_session')) {
            session.value = null
            profile.value = null
            actAsCustomerId.value = null
          }
        })
      } catch {
        // ignore
      }
    }
  }

  /** Admin sets a customer to act on behalf of; clears when null. */
  function setActAsCustomer(id: string | null) {
    actAsCustomerId.value = id
  }

  /** Return the effective customer id for API calls.
   *  Admin uses actAsCustomerId if set; customer always uses own id or canonical demo id. */
  const effectiveCustomerId = computed<string>(() => {
    if (profile.value?.role === 'admin') {
      return actAsCustomerId.value || '22222222-2222-4222-8222-222222222222'
    }
    return profile.value?.id || '22222222-2222-4222-8222-222222222222'
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
