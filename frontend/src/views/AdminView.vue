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
          <h1 class="text-headline-lg font-bold text-text-primary tracking-tight">System Administration</h1>
          <span class="px-2 py-0.5 rounded text-label-sm font-semibold tracking-wide bg-ai-subtle text-ai-accent border border-ai-border flex items-center gap-1">
            <ShieldCheck class="w-3.5 h-3.5" />
            Admin Scope
          </span>
        </div>
        <p class="text-body-sm text-text-muted mt-1">
          Multi-tenant identity management and customer impersonation console
        </p>
      </div>

      <Button variant="primary" size="md" @click="isCreateDialogOpen = true">
        <template #iconLeft><Plus class="w-4 h-4" /></template>
        Create Account
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
          <span class="font-semibold">Active Impersonation Scope:</span>
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
          View Kanban
        </Button>
        <Button
          variant="ghost"
          size="sm"
          class="text-text-muted hover:text-text-primary"
          @click="clearActAs"
        >
          <template #iconLeft><X class="w-3.5 h-3.5" /></template>
          Exit Scope
        </Button>
      </div>
    </div>

    <!-- AdminSummaryCards -->
    <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
      <div class="surface-1 bg-surface-card border border-border-subtle rounded-lg p-4 shadow-sm flex items-center justify-between">
        <div>
          <span class="text-label-sm font-semibold uppercase tracking-wider text-text-muted">
            Total Identities
          </span>
          <div class="text-2xl font-bold text-text-primary tabular-nums mt-1">
            {{ totalUsersCount }}
          </div>
          <p class="text-[11px] text-text-muted mt-0.5">Provisioned auth profiles</p>
        </div>
        <div class="w-10 h-10 rounded-md bg-surface-canvas border border-border-subtle flex items-center justify-center text-text-muted">
          <Users class="w-5 h-5" />
        </div>
      </div>

      <div class="surface-1 bg-surface-card border border-border-subtle rounded-lg p-4 shadow-sm flex items-center justify-between">
        <div>
          <span class="text-label-sm font-semibold uppercase tracking-wider text-text-muted">
            Customer Tenants
          </span>
          <div class="text-2xl font-bold text-primary tabular-nums mt-1">
            {{ customerUsersCount }}
          </div>
          <p class="text-[11px] text-text-muted mt-0.5">Scoped client workspaces</p>
        </div>
        <div class="w-10 h-10 rounded-md bg-primary/10 border border-primary/20 flex items-center justify-center text-primary">
          <Building2 class="w-5 h-5" />
        </div>
      </div>

      <div class="surface-1 bg-surface-card border border-border-subtle rounded-lg p-4 shadow-sm flex items-center justify-between">
        <div>
          <span class="text-label-sm font-semibold uppercase tracking-wider text-text-muted">
            Platform Admins
          </span>
          <div class="text-2xl font-bold text-ai-accent tabular-nums mt-1">
            {{ adminUsersCount }}
          </div>
          <p class="text-[11px] text-text-muted mt-0.5">Superuser service operators</p>
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
          Provisioned Platform Accounts
        </h3>
        <span class="text-[11px] text-text-muted font-mono">
          Security Barrier: Strict RBAC Active
        </span>
      </div>

      <div class="overflow-x-auto">
        <table class="w-full text-left border-collapse">
          <thead>
            <tr class="border-b border-border-subtle bg-surface-canvas/60 text-label-sm font-semibold text-text-muted select-none">
              <th class="py-3 px-4">User</th>
              <th class="py-3 px-3">Email Address</th>
              <th class="py-3 px-3">Role Badge</th>
              <th class="py-3 px-3">Organization / Company</th>
              <th class="py-3 px-3 text-right">Tenant Scoping</th>
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
                  Admin
                </span>
                <span
                  v-else
                  class="inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-[11px] font-semibold bg-primary/10 text-primary border border-primary/20"
                >
                  <Building2 class="w-3 h-3" />
                  Customer
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
                      Active Scope
                    </span>
                    <Button
                      variant="ghost"
                      size="sm"
                      @click="clearActAs"
                    >
                      Exit
                    </Button>
                  </div>
                  <div v-else class="inline-flex items-center gap-1.5">
                    <Button
                      variant="secondary"
                      size="sm"
                      title="Impersonate this customer for scoped queries"
                      @click="handleActAs(user)"
                    >
                      Act as
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
      title="Create Platform Account"
      description="Register a new customer tenant or platform administrator. The server provisions Supabase Auth securely without exposing service keys."
      @update:open="isCreateDialogOpen = $event"
    >
      <form class="space-y-4" @submit.prevent="handleCreateUser">
        <Field label="Email Address" required>
          <Input
            v-model="form.email"
            type="email"
            placeholder="recruiter@company.com"
            required
          />
        </Field>

        <Field label="Initial Password" required hint="Minimum 8 characters. Never logged or exposed.">
          <Input
            v-model="form.password"
            type="password"
            placeholder="••••••••••••"
            required
          />
        </Field>

        <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <Field label="Account Role">
            <Select
              v-model="form.role"
              :options="[
                { label: 'Customer (Tenant Scoped)', value: 'customer' },
                { label: 'Platform Administrator', value: 'admin' }
              ]"
            />
          </Field>

          <Field label="Display Name">
            <Input
              v-model="form.displayName"
              placeholder="e.g. Elin Recruiter"
            />
          </Field>
        </div>

        <Field
          v-if="form.role === 'customer'"
          label="Company / Workspace Name"
          required
          hint="Required for customer accounts to define tenant scope."
        >
          <Input
            v-model="form.companyName"
            placeholder="e.g. Nordic Tech AB"
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
            Cancel
          </Button>
          <Button
            type="submit"
            variant="primary"
            size="md"
            :loading="createSubmitting"
          >
            Create Account
          </Button>
        </div>
      </form>
    </Dialog>
  </div>
</template>
