<script setup lang="ts">
/**
 * LoginView.vue – Recruiter Velocity ATS Entry Authentication Page
 * Source of Truth: docs/DESIGN.md & .claude/skills/ats-orchestrator/reference/LOGIN.md
 * Integrates:
 * - src/components/auth/LoginBackdrop.vue (local asset + CSS/SVG fallback)
 * - src/stores/auth.ts (signIn via Supabase Auth + /api/account/me)
 * - supabase/seed/demo_dataset.json (demo quick-fill accounts)
 * - docs/DEMO.md Scene 2 (Admin login demonstration)
 */
import { ref, nextTick } from 'vue';
import { useRouter, useRoute } from 'vue-router';
import { useAuthStore } from '@/stores/auth';
import { useToast } from '@/components/ui/useToast';
import LoginBackdrop from '@/components/auth/LoginBackdrop.vue';
import Button from '@/components/ui/Button.vue';
import Input from '@/components/ui/Input.vue';
import Field from '@/components/ui/Field.vue';
import {
  ShieldCheck,
  Sparkles,
  SquareKanban,
  Mail,
  Lock,
  Eye,
  EyeOff,
  AlertTriangle,
  ArrowRight
} from '@/lib/icons';

const authStore = useAuthStore();
const router = useRouter();
const route = useRoute();
const toast = useToast();

const email = ref('');
const password = ref('');
const showPassword = ref(false);
const error = ref<string | null>(null);
const loading = ref(false);

const passwordInputRef = ref<HTMLInputElement | null>(null);

// Demo quick-fill credentials (emails only – passwords NEVER autofilled or committed)
const DEMO_ADMIN_EMAIL = 'admin@nordic-recruit.demo';
const DEMO_CUSTOMER_EMAIL = 'recruiter@nordic-tech.demo';

function fillDemoAdmin() {
  email.value = DEMO_ADMIN_EMAIL;
  error.value = null;
  toast.info('Filled demo admin email. Enter password to continue.');
  focusPassword();
}

function fillDemoCustomer() {
  email.value = DEMO_CUSTOMER_EMAIL;
  error.value = null;
  toast.info('Filled demo customer email. Enter password to continue.');
  focusPassword();
}

function focusPassword() {
  nextTick(() => {
    const el = document.getElementById('login-password-input');
    el?.focus();
  });
}

async function handleLogin() {
  if (!email.value || !password.value) {
    error.value = 'Both email and password are required.';
    return;
  }

  error.value = null;
  loading.value = true;

  try {
    // 1. Supabase Auth signs in and acquires JWT
    // 2. Loads profile from .NET /api/account/me
    await authStore.signIn(email.value.trim(), password.value);

    toast.success(`Welcome back, ${authStore.profile?.displayName || 'User'}!`);

    // Redirect to intended route or default dashboard
    const redirectPath = (route.query.redirect as string) || '/';
    await router.push(redirectPath);
  } catch (err) {
    const msg = (err as Error).message;
    if (msg.includes('Invalid login credentials')) {
      error.value = 'Invalid email or password. Please verify your credentials.';
    } else if (msg.includes('profile')) {
      error.value = 'Authenticated successfully, but no ATS profile was found for this user. Please contact an administrator.';
    } else {
      error.value = msg || 'Sign-in failed. Please check your network connection.';
    }
    toast.error(error.value);
  } finally {
    loading.value = false;
  }
}
</script>

<template>
  <div class="relative min-h-screen w-full flex items-center justify-center p-4 sm:p-6 lg:p-12 overflow-hidden bg-slate-950 font-sans">
    <!-- Professional Photographic Asset + CSS Fallback Layer -->
    <LoginBackdrop />

    <!-- Split-Layout Container (Desktop ~55% branding / ~45% form card) -->
    <div class="relative z-10 w-full max-w-5xl grid grid-cols-1 lg:grid-cols-12 gap-8 lg:gap-14 items-center">
      <!-- Left Column: Value Prop & Trust Elements -->
      <div class="lg:col-span-7 space-y-6 text-white text-left select-none">
        <!-- Brand Lockup -->
        <div class="flex items-center gap-3">
          <div class="w-10 h-10 rounded-md bg-primary flex items-center justify-center text-white shadow-lg shadow-primary/30">
            <SquareKanban class="w-6 h-6" />
          </div>
          <div>
            <div class="flex items-center gap-2">
              <span class="text-xl font-bold tracking-tight text-white">Mini ATS</span>
              <span class="px-2 py-0.5 rounded text-[10px] font-semibold tracking-wider uppercase bg-primary/20 text-primary-subtle border border-primary/30">
                Velocity ATS
              </span>
            </div>
            <p class="text-xs text-slate-300">Enterprise Talent Acquisition Suite</p>
          </div>
        </div>

        <!-- Headline & Hook -->
        <div class="space-y-3">
          <h1 class="text-3xl sm:text-4xl font-extrabold text-white tracking-tight leading-tight">
            Move candidates from inbound to hired, with AI-assisted velocity.
          </h1>
          <p class="text-base text-slate-300 leading-relaxed max-w-xl">
            Streamlined 6-stage recruitment workflows, multi-tenant role isolation,
            and explainable AI screening built for modern engineering teams.
          </p>
        </div>

        <!-- Trust Badges with Lucide Icons -->
        <div class="space-y-3 pt-2">
          <div class="flex items-start gap-3">
            <div class="w-7 h-7 rounded-md bg-slate-800/80 border border-slate-700/60 flex items-center justify-center text-emerald-400 shrink-0 mt-0.5">
              <ShieldCheck class="w-4 h-4" />
            </div>
            <div>
              <h4 class="text-xs font-semibold text-slate-100">Strict Multi-Tenant Scoping</h4>
              <p class="text-xs text-slate-400">Deterministic workspace isolation enforced server-side via ASP.NET Core.</p>
            </div>
          </div>

          <div class="flex items-start gap-3">
            <div class="w-7 h-7 rounded-md bg-slate-800/80 border border-slate-700/60 flex items-center justify-center text-violet-400 shrink-0 mt-0.5">
              <Sparkles class="w-4 h-4" />
            </div>
            <div>
              <h4 class="text-xs font-semibold text-slate-100">Objective AI CV Assessment</h4>
              <p class="text-xs text-slate-400">Explainable match scoring and interview guidance with deterministic mock fallback.</p>
            </div>
          </div>

          <div class="flex items-start gap-3">
            <div class="w-7 h-7 rounded-md bg-slate-800/80 border border-slate-700/60 flex items-center justify-center text-primary-subtle shrink-0 mt-0.5">
              <SquareKanban class="w-4 h-4" />
            </div>
            <div>
              <h4 class="text-xs font-semibold text-slate-100">High-Density Pipeline Management</h4>
              <p class="text-xs text-slate-400">6-stage Kanban board with instant multi-facet filtering and optimistic updates.</p>
            </div>
          </div>
        </div>

        <!-- Architecture metadata footer -->
        <div class="pt-4 border-t border-slate-800/80 flex items-center gap-2 text-[11px] font-mono text-slate-400">
          <span>Stack:</span>
          <span class="text-slate-300">Vue 3.4</span>
          <span>•</span>
          <span class="text-slate-300">.NET 10</span>
          <span>•</span>
          <span class="text-slate-300">Supabase</span>
          <span>•</span>
          <span class="text-slate-300">Python AI</span>
        </div>
      </div>

      <!-- Right Column: Login Card -->
      <div class="lg:col-span-5 w-full">
        <div class="surface-1 bg-surface-card/95 backdrop-blur-xl border border-border-subtle rounded-lg shadow-surface-2 p-6 sm:p-8 space-y-6">
          <!-- Card Header -->
          <div class="space-y-1">
            <h2 class="text-headline-sm font-semibold text-text-primary tracking-tight">
              Sign in to Mini ATS
            </h2>
            <p class="text-body-sm text-text-muted">
              Enter your credentials to access your talent pipeline
            </p>
          </div>

          <!-- Login Form -->
          <form class="space-y-4" @submit.prevent="handleLogin">
            <!-- Email Field -->
            <Field label="Email address" required>
              <Input
                id="login-email-input"
                v-model="email"
                type="email"
                placeholder="name@company.com"
                autocomplete="email"
                :disabled="loading"
                :error="!!error"
              >
                <template #prefix>
                  <Mail class="w-4 h-4" />
                </template>
              </Input>
            </Field>

            <!-- Password Field -->
            <Field label="Password" required>
              <Input
                id="login-password-input"
                ref="passwordInputRef"
                v-model="password"
                :type="showPassword ? 'text' : 'password'"
                placeholder="••••••••••••"
                autocomplete="current-password"
                :disabled="loading"
                :error="!!error"
              >
                <template #prefix>
                  <Lock class="w-4 h-4" />
                </template>
                <template #suffix>
                  <button
                    type="button"
                    class="p-1 rounded text-text-muted hover:text-text-primary transition-colors focus:outline-none focus:ring-1 focus:ring-primary"
                    tabindex="-1"
                    :aria-label="showPassword ? 'Hide password' : 'Show password'"
                    @click="showPassword = !showPassword"
                  >
                    <EyeOff v-if="showPassword" class="w-3.5 h-3.5" />
                    <Eye v-else class="w-3.5 h-3.5" />
                  </button>
                </template>
              </Input>
            </Field>

            <!-- Inline error alert -->
            <div
              v-if="error"
              class="p-3 rounded bg-rose-50 dark:bg-rose-950/40 border border-rose-200 dark:border-rose-800 text-rose-700 dark:text-rose-400 text-xs flex items-start gap-2 select-none"
              role="alert"
            >
              <AlertTriangle class="w-4 h-4 shrink-0 mt-0.5" />
              <span>{{ error }}</span>
            </div>

            <!-- Submit Button -->
            <Button
              type="submit"
              variant="primary"
              size="md"
              class="w-full justify-center text-sm font-medium h-9"
              :loading="loading"
              :disabled="loading"
            >
              <template #iconRight>
                <ArrowRight class="w-4 h-4" />
              </template>
              Sign in to Workspace
            </Button>
          </form>

          <!-- Divider -->
          <div class="relative flex items-center justify-center">
            <div class="absolute inset-0 flex items-center">
              <div class="w-full border-t border-border-subtle" />
            </div>
            <div class="relative bg-surface-card px-2 text-[10px] uppercase font-mono text-text-muted tracking-wider">
              Demo Quick-Fill
            </div>
          </div>

          <!-- Demo quick-fill row (Emails only) -->
          <div class="grid grid-cols-2 gap-2">
            <Button
              type="button"
              variant="secondary"
              size="sm"
              class="w-full justify-center text-xs truncate"
              @click="fillDemoAdmin"
            >
              Demo Admin
            </Button>
            <Button
              type="button"
              variant="secondary"
              size="sm"
              class="w-full justify-center text-xs truncate"
              @click="fillDemoCustomer"
            >
              Demo Customer
            </Button>
          </div>

          <!-- Credential security note -->
          <p class="text-[11px] text-text-muted text-center leading-normal">
            Password comes from local environment / your Supabase credentials. No sensitive secrets are stored in client code.
          </p>
        </div>
      </div>
    </div>
  </div>
</template>
