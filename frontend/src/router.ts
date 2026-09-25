// router.ts – Vue Router configuration with authentication and role guards.
// Related: src/stores/auth.ts (session check, isAdmin)
//          src/lib/supabase.ts (getSession called in guard)
//          src/views/ (all routed components)
//          frontend/vercel.json (SPA rewrite so all paths serve index.html)

import { createRouter, createWebHistory } from 'vue-router'
import { supabase } from './lib/supabase'

// Eagerly import main landing view for zero-latency workspace transitions
import DashboardView from './views/DashboardView.vue'

// Lazy-load secondary views
const LoginView           = () => import('./views/LoginView.vue')
const JobsView            = () => import('./views/JobsView.vue')
const CandidatesView      = () => import('./views/CandidatesView.vue')
const CandidateDetailView = () => import('./views/CandidateDetailView.vue')
const KanbanView          = () => import('./views/KanbanView.vue')
const AdminView           = () => import('./views/AdminView.vue')

export const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/login',
      name: 'login',
      component: LoginView
      // No meta – public route, redirects away if already authenticated
    },
    {
      path: '/',
      name: 'dashboard',
      component: DashboardView,
      meta: { requiresAuth: true }
    },
    {
      path: '/jobs',
      name: 'jobs',
      component: JobsView,
      meta: { requiresAuth: true }
    },
    {
      path: '/candidates',
      name: 'candidates',
      component: CandidatesView,
      meta: { requiresAuth: true }
    },
    {
      path: '/candidates/:id',
      name: 'candidate-detail',
      component: CandidateDetailView,
      meta: { requiresAuth: true }
    },
    {
      path: '/kanban',
      name: 'kanban',
      component: KanbanView,
      meta: { requiresAuth: true }
    },
    {
      path: '/admin',
      name: 'admin',
      component: AdminView,
      meta: { requiresAuth: true, requiresAdmin: true }
    },
    {
      path: '/:pathMatch(.*)*',
      redirect: '/'
    }
  ]
})

// Global navigation guard: check session and role before each route.
router.beforeEach(async (to) => {
  // Use lazy-import of auth store to avoid circular dependency with router.
  const { useAuthStore } = await import('./stores/auth')
  const authStore = useAuthStore()

  // 1. Fast in-memory check (0ms)
  let authenticated = authStore.isAuthenticated
  if (!authenticated) {
    // 2. Fall back to Supabase storage on cold load / direct page reload
    const { data } = await supabase.auth.getSession()
    authenticated = Boolean(data.session)
    if (data.session && !authStore.session) {
      authStore.session = data.session
    }
  }

  // Redirect unauthenticated users to login.
  if (to.meta.requiresAuth && !authenticated) {
    return { name: 'login' }
  }

  // Redirect authenticated users away from the login page.
  if (to.name === 'login' && authenticated) {
    return { name: 'dashboard' }
  }

  // Admin-only route: check role claim from Supabase JWT.
  if (to.meta.requiresAdmin && authenticated) {
    // Wait for profile if it hasn't loaded yet (on direct URL navigation).
    if (!authStore.profile) await authStore.loadProfile()
    if (!authStore.isAdmin) return { name: 'dashboard' }
  }

  return true
})
