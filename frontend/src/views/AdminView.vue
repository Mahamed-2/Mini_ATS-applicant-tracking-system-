<script setup lang="ts">
/**
 * AdminView.vue – Multi-Tenant Platform Administration & Identity Scoping
 * Source of Truth: docs/DESIGN.md & .claude/skills/ats-orchestrator/reference/PAGE_RESKIN.md
 * Integrates:
 * - GET /api/admin/users & POST /api/admin/users
 * - RoleBadge [admin=violet shield, customer=info]
 * - Act As impersonation setting authStore.actAsCustomerId
 * - AdminSummaryCards
 */
import { onMounted, ref, computed } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore } from '@/stores/auth';
import { useToast } from '@/components/ui/useToast';
import { useI18n } from '@/i18n';
import { apiFetch } from '@/lib/api';
import Button from '@/components/ui/Button.vue';
import Input from '@/components/ui/Input.vue';
import Select from '@/components/ui/Select.vue';
import Field from '@/components/ui/Field.vue';
import Dialog from '@/components/ui/Dialog.vue';
import Avatar from '@/components/ui/Avatar.vue';
import {
  ShieldCheck,
  Building2,
  Users,
  Plus,
  SquareKanban,
  CheckCircle2,
  X,
  UserCheck
} from '@/lib/icons';

const authStore = useAuthStore();
const router = useRouter();
const toast = useToast();
const { t } = useI18n();

interface UserRow {
  id: string;
  email: string;
  role: string;
  displayName: string | null;
  companyName: string | null;
  createdAt?: string;
}

const users = ref<UserRow[]>([]);
const loading = ref(false);
const isCreateDialogOpen = ref(false);

const form = ref({
  email: '',
  password: '',
  role: 'customer',
  displayName: '',
  companyName: '',
});
const createSubmitting = ref(false);

// Redirect non-admins
onMounted(async () => {
  if (!authStore.isAdmin) {
    toast.error('Administrative privileges required. Redirecting to workspace.');
    router.replace('/');
    return;
  }
  await loadUsers();
});

async function loadUsers() {
  loading.value = true;
  try {
    users.value = await apiFetch<UserRow[]>('/api/admin/users');
  } catch (e) {
    toast.error(`Failed to load users: ${(e as Error).message}`);
  } finally {
    loading.value = false;
  }
}

// Summary Metrics
const totalUsersCount = computed(() => users.value.length);
const customerUsersCount = computed(() => users.value.filter(u => u.role.toLowerCase() === 'customer').length);
const adminUsersCount = computed(() => users.value.filter(u => u.role.toLowerCase() === 'admin').length);

async function handleCreateUser() {
  if (!form.value.email || !form.value.password) {
    toast.error('Email and password are required.');
    return;
  }
  if (form.value.password.length < 8) {
    toast.error('Password must be at least 8 characters long.');
    return;
  }
  if (form.value.role === 'customer' && !form.value.companyName.trim()) {
    toast.error('Company name is required for customer accounts.');
    return;
  }

  createSubmitting.value = true;
  try {
    await apiFetch('/api/admin/users', {
      method: 'POST',
      body: JSON.stringify({
        email: form.value.email.trim(),
        password: form.value.password,
        role: form.value.role,
        displayName: form.value.displayName.trim() || null,
        companyName: form.value.companyName.trim() || null,
      })
    });

    toast.success(`Account created for ${form.value.email}.`);
    form.value = { email: '', password: '', role: 'customer', displayName: '', companyName: '' };
    isCreateDialogOpen.value = false;
    await loadUsers();
  } catch (e) {
    toast.error(`User creation failed: ${(e as Error).message}`);
  } finally {
    createSubmitting.value = false;
  }
}

function handleActAs(user: UserRow) {
  authStore.setActAsCustomer(user.id);
  toast.success(`Scope switched: now acting on behalf of ${user.companyName || user.email}`);
}

function clearActAs() {
  authStore.setActAsCustomer(null);
  toast.info('Returned to global administrator scope.');
}

function openKanbanScoped(user: UserRow) {
  authStore.setActAsCustomer(user.id);
  router.push('/kanban');
}
</script>

<template>
  <div class="space-y-6">
    <!-- Header Row -->
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
      <div>
        <div class="flex items-center gap-2">
          <h1 class="text-headline-lg font-bold text-text-primary tracking-tight">
            {{ t('admin.title') }}
          </h1>
          <span class="px-2 py-0.5 rounded text-label-sm font-semibold tracking-wide bg-ai-subtle text-ai-accent border border-ai-border flex items-center gap-1">
            <ShieldCheck class="w-3.5 h-3.5" />
            {{ t('admin.adminScope') }}
          </span>
        </div>
        <p class="text-body-sm text-text-muted mt-1">
          {{ t('admin.subtitle') }}
        </p>
      </div>

      <Button variant="primary" size="md" @click="isCreateDialogOpen = true">
        <template #iconLeft><Plus class="w-4 h-4" /></template>
        {{ t('admin.createUser') }}
      </Button>
    </div>

    <!-- Active Act-As Banner (if impersonation is active) -->
    <div
      v-if="authStore.actAsCustomerId"
      class="surface-2 bg-primary/10 border border-primary/30 rounded-lg p-4 flex flex-col sm:flex-row items-start sm:items-center justify-between gap-3 text-sm"
    >
      <div class="flex items-center gap-2.5 text-primary">
        <UserCheck class="w-5 h-5 shrink-0" />
        <div>
          <span class="font-semibold">{{ t('admin.activeImpersonation') }}</span>
          <span class="ml-1 text-text-primary font-medium">
            {{ users.find(u => u.id === authStore.actAsCustomerId)?.companyName || 'Nordic Tech AB' }}
          </span>
          <span class="ml-2 text-xs font-mono text-text-muted">
            ({{ authStore.actAsCustomerId.slice(0, 8) }}…)
          </span>
        </div>
      </div>

      <div class="flex items-center gap-2">
        <Button
          variant="secondary"
          size="sm"
          @click="router.push('/kanban')"
        >
          <template #iconLeft><SquareKanban class="w-3.5 h-3.5 text-primary" /></template>
          {{ t('admin.viewKanban') }}
        </Button>
        <Button
          variant="ghost"
          size="sm"
          class="text-text-muted hover:text-text-primary"
          @click="clearActAs"
        >
          <template #iconLeft><X class="w-3.5 h-3.5" /></template>
          {{ t('admin.exitScope') }}
        </Button>
      </div>
    </div>

    <!-- AdminSummaryCards -->
    <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
      <div class="surface-1 bg-surface-card border border-border-subtle rounded-lg p-4 shadow-sm flex items-center justify-between">
        <div>
          <span class="text-label-sm font-semibold uppercase tracking-wider text-text-muted">
            {{ t('admin.totalIdentities') }}
          </span>
          <div class="text-2xl font-bold text-text-primary tabular-nums mt-1">
            {{ totalUsersCount }}
          </div>
          <p class="text-[11px] text-text-muted mt-0.5">{{ t('admin.totalIdentitiesSub') }}</p>
        </div>
        <div class="w-10 h-10 rounded-md bg-surface-canvas border border-border-subtle flex items-center justify-center text-text-muted">
          <Users class="w-5 h-5" />
        </div>
      </div>

      <div class="surface-1 bg-surface-card border border-border-subtle rounded-lg p-4 shadow-sm flex items-center justify-between">
        <div>
          <span class="text-label-sm font-semibold uppercase tracking-wider text-text-muted">
            {{ t('admin.customerTenants') }}
          </span>
          <div class="text-2xl font-bold text-primary tabular-nums mt-1">
            {{ customerUsersCount }}
          </div>
          <p class="text-[11px] text-text-muted mt-0.5">{{ t('admin.customerTenantsSub') }}</p>
        </div>
        <div class="w-10 h-10 rounded-md bg-primary/10 border border-primary/20 flex items-center justify-center text-primary">
          <Building2 class="w-5 h-5" />
        </div>
      </div>

      <div class="surface-1 bg-surface-card border border-border-subtle rounded-lg p-4 shadow-sm flex items-center justify-between">
        <div>
          <span class="text-label-sm font-semibold uppercase tracking-wider text-text-muted">
            {{ t('admin.platformAdmins') }}
          </span>
          <div class="text-2xl font-bold text-ai-accent tabular-nums mt-1">
            {{ adminUsersCount }}
          </div>
          <p class="text-[11px] text-text-muted mt-0.5">{{ t('admin.platformAdminsSub') }}</p>
        </div>
        <div class="w-10 h-10 rounded-md bg-ai-subtle border border-ai-border flex items-center justify-center text-ai-accent">
          <ShieldCheck class="w-5 h-5" />
        </div>
      </div>
    </div>

    <!-- AdminUserTable -->
    <div class="surface-1 bg-surface-card border border-border-subtle rounded-lg shadow-sm overflow-hidden">
      <div class="p-4 border-b border-border-subtle flex items-center justify-between bg-surface-canvas">
        <h3 class="text-xs font-semibold text-text-primary uppercase tracking-wider">
          {{ t('admin.provisionedAccounts') }}
        </h3>
        <span class="text-[11px] text-text-muted font-mono">
          {{ t('admin.securityBarrier') }}
        </span>
      </div>

      <div class="overflow-x-auto">
        <table class="w-full text-left border-collapse">
          <thead>
            <tr class="border-b border-border-subtle bg-surface-canvas/60 text-label-sm font-semibold text-text-muted select-none">
              <th class="py-3 px-4">{{ t('admin.thUser') }}</th>
              <th class="py-3 px-3">{{ t('admin.thEmail') }}</th>
              <th class="py-3 px-3">{{ t('admin.thRole') }}</th>
              <th class="py-3 px-3">{{ t('admin.thOrg') }}</th>
              <th class="py-3 px-3 text-right">{{ t('admin.thTenant') }}</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-border-subtle text-body-sm">
            <tr
              v-for="user in users"
              :key="user.id"
              :class="[
                'hover:bg-surface-hover transition-colors',
                authStore.actAsCustomerId === user.id ? 'bg-primary/5 font-medium' : ''
              ]"
            >
              <!-- Name + Avatar -->
              <td class="py-3 px-4">
                <div class="flex items-center gap-3">
                  <Avatar :name="user.displayName || user.email" size="sm" />
                  <span class="font-medium text-text-primary">
                    {{ user.displayName || user.email.split('@')[0] }}
                  </span>
                </div>
              </td>

              <!-- Email -->
              <td class="py-3 px-3 text-text-muted text-xs font-mono">
                {{ user.email }}
              </td>

              <!-- Role Badge -->
              <td class="py-3 px-3">
                <span
                  v-if="user.role.toLowerCase() === 'admin'"
                  class="inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-[11px] font-semibold bg-ai-subtle text-ai-accent border border-ai-border"
                >
                  <ShieldCheck class="w-3 h-3" />
                  {{ t('admin.roleAdmin') }}
                </span>
                <span
                  v-else
                  class="inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-[11px] font-semibold bg-primary/10 text-primary border border-primary/20"
                >
                  <Building2 class="w-3 h-3" />
                  {{ t('admin.roleCustomer') }}
                </span>
              </td>

              <!-- Company -->
              <td class="py-3 px-3 text-xs text-text-primary font-medium">
                {{ user.companyName || 'Platform Administration' }}
              </td>

              <!-- Actions: Act As -->
              <td class="py-3 px-3 text-right">
                <template v-if="user.role.toLowerCase() === 'customer'">
                  <div
                    v-if="authStore.actAsCustomerId === user.id"
                    class="inline-flex items-center gap-1.5"
                  >
                    <span class="inline-flex items-center gap-1 px-2 py-0.5 rounded text-[11px] font-semibold bg-emerald-50 text-emerald-700 border border-emerald-200 dark:bg-emerald-950/40 dark:text-emerald-400 dark:border-emerald-800">
                      <CheckCircle2 class="w-3 h-3" />
                      {{ t('admin.activeScopeBadge') }}
                    </span>
                    <Button
                      variant="ghost"
                      size="sm"
                      @click="clearActAs"
                    >
                      {{ t('common.exit') }}
                    </Button>
                  </div>
                  <div v-else class="inline-flex items-center gap-1.5">
                    <Button
                      variant="secondary"
                      size="sm"
                      title="Impersonate this customer for scoped queries"
                      @click="handleActAs(user)"
                    >
                      {{ t('admin.actAsBtn') }}
                    </Button>
                    <Button
                      variant="ghost"
                      size="sm"
                      title="Act as and immediately view Kanban board"
                      @click="openKanbanScoped(user)"
                    >
                      <SquareKanban class="w-3.5 h-3.5 text-primary" />
                    </Button>
                  </div>
                </template>
                <span v-else class="text-xs text-text-muted">—</span>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Create User Dialog -->
    <Dialog
      :open="isCreateDialogOpen"
      :title="t('admin.modalTitle')"
      :description="t('admin.modalDesc')"
      @update:open="isCreateDialogOpen = $event"
    >
      <form class="space-y-4" @submit.prevent="handleCreateUser">
        <Field id="admin-new-user-email" :label="t('admin.thEmail')" required>
          <Input
            id="admin-new-user-email"
            name="email"
            v-model="form.email"
            type="email"
            placeholder="recruiter@company.com"
            autocomplete="email"
            required
          />
        </Field>

        <Field id="admin-new-user-password" :label="t('admin.passwordLabel')" required :hint="t('admin.passwordHint')">
          <Input
            id="admin-new-user-password"
            name="password"
            v-model="form.password"
            type="password"
            placeholder="••••••••••••"
            autocomplete="new-password"
            required
          />
        </Field>

        <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <Field id="admin-new-user-role" :label="t('admin.roleLabel')">
            <Select
              id="admin-new-user-role"
              name="role"
              v-model="form.role"
              :options="[
                { label: t('admin.roleCustomerOption'), value: 'customer' },
                { label: t('admin.roleAdminOption'), value: 'admin' }
              ]"
            />
          </Field>

          <Field id="admin-new-user-name" :label="t('admin.displayNameLabel')">
            <Input
              id="admin-new-user-name"
              name="displayName"
              v-model="form.displayName"
              placeholder="e.g. Elin Recruiter"
              autocomplete="name"
            />
          </Field>
        </div>

        <Field
          v-if="form.role === 'customer'"
          id="admin-new-user-company"
          :label="t('admin.companyLabel')"
          required
          :hint="t('admin.companyHint')"
        >
          <Input
            id="admin-new-user-company"
            name="companyName"
            v-model="form.companyName"
            placeholder="e.g. Nordic Tech AB"
            autocomplete="organization"
            required
          />
        </Field>

        <div class="flex items-center justify-end gap-2 pt-4 border-t border-border-subtle">
          <Button
            type="button"
            variant="secondary"
            size="md"
            @click="isCreateDialogOpen = false"
          >
            {{ t('common.cancel') }}
          </Button>
          <Button
            type="submit"
            variant="primary"
            size="md"
            :loading="createSubmitting"
          >
            {{ t('admin.createAccountBtn') }}
          </Button>
        </div>
      </form>
    </Dialog>
  </div>
</template>
