<script setup lang="ts">
/**
 * LoginView.vue – Recruiter Velocity ATS Entry Authentication Page
 * Supports English and Swedish bilingual localization.
 */
import { ref } from 'vue';
import { useRouter, useRoute } from 'vue-router';
import { useAuthStore } from '@/stores/auth';
import { useAtsStore } from '@/stores/ats';
import { useUiStore } from '@/stores/ui';
import { useToast } from '@/components/ui/useToast';
import { useI18n } from '@/i18n';
import { onMounted } from 'vue';
import LoginBackdrop from '@/components/auth/LoginBackdrop.vue';
import Button from '@/components/ui/Button.vue';
import Input from '@/components/ui/Input.vue';
import Field from '@/components/ui/Field.vue';
import LanguageSwitcher from '@/components/ui/LanguageSwitcher.vue';
import {
  ShieldCheck,
  Sparkles,
  SquareKanban,
  Mail,
  Lock,
  Eye,
  EyeOff,
  AlertTriangle,
  ArrowRight,
  Sun,
  Moon
} from '@/lib/icons';

const authStore = useAuthStore();
const atsStore = useAtsStore();
const uiStore = useUiStore();
const router = useRouter();
const route = useRoute();
const toast = useToast();
const { t } = useI18n();

const email = ref('');
const password = ref('');
const showPassword = ref(false);
const error = ref<string | null>(null);
const loading = ref(false);

const passwordInputRef = ref<HTMLInputElement | null>(null);

// Idle preloading of heavy views to guarantee instant sub-second navigation
onMounted(() => {
  const preload = () => {
    import('@/views/KanbanView.vue');
    import('@/views/CandidatesView.vue');
    import('@/views/JobsView.vue');
  };
  if (typeof window !== 'undefined' && 'requestIdleCallback' in window) {
    (window as any).requestIdleCallback(preload);
  } else {
    setTimeout(preload, 600);
  }
});

// Demo 1-click credentials for prototype evaluation
const DEMO_ADMIN_EMAIL = 'admin@nordic-recruit.demo';
const DEMO_CUSTOMER_EMAIL = 'recruiter@nordic-tech.demo';
const DEMO_PASSWORD = (import.meta.env.VITE_DEMO_PASSWORD as string) || 'NordicTechDemo2026!';

async function loginAsDemoAdmin() {
  email.value = DEMO_ADMIN_EMAIL;
  password.value = DEMO_PASSWORD;
  error.value = null;
  await handleLogin();
}

async function loginAsDemoCustomer() {
  email.value = DEMO_CUSTOMER_EMAIL;
  password.value = DEMO_PASSWORD;
  error.value = null;
  await handleLogin();
}

async function handleLogin() {
  if (!email.value || !password.value) {
    error.value = t('login.errRequired');
    return;
  }

  error.value = null;
  loading.value = true;

  try {
    // 1. Supabase Auth signs in and acquires JWT with optimistic in-memory profile
    await authStore.signIn(email.value.trim(), password.value);

    // 2. Pre-fire pipeline data fetching concurrently with route transition
    const customerId = authStore.effectiveCustomerId || '22222222-2222-4222-8222-222222222222';
    atsStore.loadJobs(customerId);
    atsStore.loadCandidates(customerId);

    // 3. Immediate instant transition to workspace dashboard (0ms waiting)
    const redirectPath = (route.query.redirect as string) || '/';
    await router.push(redirectPath);

    toast.success(t('login.welcomeBack', { name: authStore.profile?.displayName || 'User' }));
  } catch (err) {
    const msg = (err as Error).message;
    if (msg.includes('Invalid login credentials')) {
      error.value = t('login.errInvalid');
    } else if (msg.includes('profile')) {
      error.value = t('login.errProfile');
    } else {
      error.value = msg || t('login.errGeneric');
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

    <!-- Top Right Controls: Language Switcher + Dark/Light Theme Toggle -->
    <div class="absolute top-4 right-4 z-20 flex items-center gap-2">
      <LanguageSwitcher />
      <button
        id="theme-toggle-btn"
        type="button"
        class="w-8 h-8 rounded-full border border-border-subtle bg-surface-hover/80 hover:bg-surface-hover text-text-muted hover:text-text-primary flex items-center justify-center transition-all backdrop-blur-md cursor-pointer select-none"
        :title="`Switch to ${uiStore.colorMode === 'light' ? 'Dark' : 'Light'} theme`"
        aria-label="Toggle theme"
        @click="uiStore.toggleColorMode"
      >
        <Sun v-if="uiStore.colorMode === 'dark'" class="w-4 h-4 text-warning" />
        <Moon v-else class="w-4 h-4 text-slate-300" />
      </button>
    </div>

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
              <span class="px-2 py-0.5 rounded text-[10px] font-semibold tracking-wider uppercase bg-blue-500/20 text-blue-300 border border-blue-400/40">
                {{ t('login.brandTag') }}
              </span>
            </div>
            <p class="text-xs text-slate-300">{{ t('login.brandSubtitle') }}</p>
          </div>
        </div>

        <!-- Headline & Hook -->
        <div class="space-y-3">
          <h1 class="text-3xl sm:text-4xl font-extrabold text-white tracking-tight leading-tight">
            {{ t('login.headline') }}
          </h1>
          <p class="text-base text-slate-300 leading-relaxed max-w-xl">
            {{ t('login.description') }}
          </p>
        </div>

        <!-- Trust Badges with Lucide Icons -->
        <div class="space-y-3 pt-2">
          <div class="flex items-start gap-3">
            <div class="w-7 h-7 rounded-md bg-slate-800/80 border border-slate-700/60 flex items-center justify-center text-emerald-400 shrink-0 mt-0.5 shadow-sm">
              <ShieldCheck class="w-4 h-4" />
            </div>
            <div>
              <h4 class="text-xs font-semibold text-slate-100">{{ t('login.trustMultiTenant') }}</h4>
              <p class="text-xs text-slate-300">{{ t('login.trustMultiTenantDesc') }}</p>
            </div>
          </div>

          <div class="flex items-start gap-3">
            <div class="w-7 h-7 rounded-md bg-slate-800/80 border border-slate-700/60 flex items-center justify-center text-violet-400 shrink-0 mt-0.5 shadow-sm">
              <Sparkles class="w-4 h-4" />
            </div>
            <div>
              <h4 class="text-xs font-semibold text-slate-100">{{ t('login.trustAi') }}</h4>
              <p class="text-xs text-slate-300">{{ t('login.trustAiDesc') }}</p>
            </div>
          </div>

          <div class="flex items-start gap-3">
            <div class="w-7 h-7 rounded-md bg-slate-800/80 border border-slate-700/60 flex items-center justify-center text-blue-400 shrink-0 mt-0.5 shadow-sm">
              <SquareKanban class="w-4 h-4" />
            </div>
            <div>
              <h4 class="text-xs font-semibold text-slate-100">{{ t('login.trustKanban') }}</h4>
              <p class="text-xs text-slate-300">{{ t('login.trustKanbanDesc') }}</p>
            </div>
          </div>
        </div>
      </div>

      <!-- Right Column: Login Card -->
      <div class="lg:col-span-5 w-full">
        <div class="surface-1 bg-surface-card/95 backdrop-blur-xl border border-border-subtle rounded-lg shadow-surface-2 p-6 sm:p-8 space-y-6">
          <!-- Card Header -->
          <div class="space-y-1">
            <h2 class="text-headline-sm font-semibold text-text-primary tracking-tight">
              {{ t('login.formTitle') }}
            </h2>
            <p class="text-body-sm text-text-muted">
              {{ t('login.formSubtitle') }}
            </p>
          </div>

          <!-- Login Form -->
          <form class="space-y-4" @submit.prevent="handleLogin">
            <!-- Email Field -->
            <Field id="login-email-input" :label="t('login.emailLabel')" required>
              <Input
                id="login-email-input"
                name="email"
                v-model="email"
                type="email"
                :placeholder="t('login.emailPlaceholder')"
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
            <Field id="login-password-input" :label="t('login.passwordLabel')" required>
              <Input
                id="login-password-input"
                name="password"
                ref="passwordInputRef"
                v-model="password"
                :type="showPassword ? 'text' : 'password'"
                :placeholder="t('login.passwordPlaceholder')"
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
                    class="p-1 rounded text-text-muted hover:text-text-primary transition-colors focus:outline-none focus:ring-1 focus:ring-primary cursor-pointer"
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
              {{ loading ? t('login.signingIn') : t('login.signInBtn') }}
            </Button>
          </form>

          <!-- Divider -->
          <div class="relative flex items-center justify-center">
            <div class="absolute inset-0 flex items-center">
              <div class="w-full border-t border-border-subtle" />
            </div>
            <div class="relative bg-surface-card px-2 text-[10px] uppercase font-mono text-text-muted tracking-wider">
              {{ t('login.demoTitle') }}
            </div>
          </div>

          <!-- Demo 1-click login row -->
          <div class="grid grid-cols-2 gap-2">
            <Button
              id="demo-admin-btn"
              type="button"
              variant="secondary"
              size="sm"
              class="w-full justify-center text-xs truncate"
              :disabled="loading"
              @click="loginAsDemoAdmin"
            >
              {{ t('login.demoAdminBtn') }}
            </Button>
            <Button
              id="demo-customer-btn"
              type="button"
              variant="secondary"
              size="sm"
              class="w-full justify-center text-xs truncate"
              :disabled="loading"
              @click="loginAsDemoCustomer"
            >
              {{ t('login.demoCustomerBtn') }}
            </Button>
          </div>

          <!-- Credential helper note -->
          <p class="text-[11px] text-text-muted text-center leading-normal">
            {{ t('login.demoHelper') }}
          </p>
        </div>
      </div>
    </div>
  </div>
</template>
