<script setup lang="ts">
// AdminView.vue – admin user creation, user list, and act-as customer selector.
// Related: src/stores/auth.ts (isAdmin, setActAsCustomer, effectiveCustomerId)
//          src/lib/api.ts (apiFetch for admin endpoints)
//          backend/MiniAts.Api/Api/Controllers/AdminController.cs (POST/GET /api/admin/users)
//          frontend/src/views/DashboardView.vue (affected by actAsCustomerId)

import { onMounted, ref } from 'vue'
import { useAuthStore } from '../stores/auth'
import { apiFetch } from '../lib/api'

const auth = useAuthStore()

// Form state for creating a new user.
const form = ref({
  email: '',
  password: '',
  role: 'customer',
  displayName: '',
  companyName: ''
})

const loading = ref(false)
const error   = ref<string | null>(null)
const success  = ref<string | null>(null)

// User list returned by GET /api/admin/users.
interface UserRow { id: string; email: string; role: string; displayName: string | null; companyName: string | null }
const users = ref<UserRow[]>([])

// Load users on mount.
onMounted(loadUsers)

async function loadUsers() {
  try {
    users.value = await apiFetch<UserRow[]>('/api/admin/users')
  } catch (e) {
    error.value = (e as Error).message
  }
}

async function handleCreate() {
  if (!form.value.email || !form.value.password) {
    error.value = 'Email and password are required.'
    return
  }

  error.value = null
  success.value = null
  loading.value = true

  try {
    // POST /api/admin/users – backend calls Supabase Auth Admin API server-side.
    await apiFetch('/api/admin/users', {
      method: 'POST',
      body: JSON.stringify(form.value)
    })
    success.value = `Account created for ${form.value.email}.`
    // Reset form and refresh list.
    form.value = { email: '', password: '', role: 'customer', displayName: '', companyName: '' }
    await loadUsers()
  } catch (e) {
    error.value = (e as Error).message
  } finally {
    loading.value = false
  }
}

// Set the act-as customer so dashboard/kanban scope to that customer.
function actAs(userId: string) {
  auth.setActAsCustomer(userId)
  success.value = `Now acting as customer ${userId.slice(0, 8)}…`
}

function clearActAs() {
  auth.setActAsCustomer(null)
  success.value = 'Cleared act-as customer. Using your own admin scope.'
}
</script>

<template>
  <div class="page">
    <h1 class="page-title">Admin Panel</h1>

    <!-- ── Create user section ── -->
    <div class="card section">
      <h2 class="section-title">Create Account</h2>

      <form class="form-grid" @submit.prevent="handleCreate">
        <div class="field">
          <label for="admin-email">Email *</label>
          <input id="admin-email" v-model="form.email" type="email" placeholder="user@example.com" required />
        </div>

        <div class="field">
          <label for="admin-password">Password *</label>
          <input id="admin-password" v-model="form.password" type="password" placeholder="••••••••" required />
        </div>

        <div class="field">
          <label for="admin-role">Role</label>
          <select id="admin-role" v-model="form.role">
            <option value="customer">Customer</option>
            <option value="admin">Admin</option>
          </select>
        </div>

        <div class="field">
          <label for="admin-display-name">Display Name</label>
          <input id="admin-display-name" v-model="form.displayName" type="text" placeholder="Jane Doe" />
        </div>

        <div class="field">
          <label for="admin-company">Company Name</label>
          <input id="admin-company" v-model="form.companyName" type="text" placeholder="Acme AB" />
        </div>

        <div class="field field-action">
          <button type="submit" class="btn-primary" :disabled="loading">
            {{ loading ? 'Creating…' : 'Create Account' }}
          </button>
        </div>
      </form>

      <p v-if="error"   class="msg error">{{ error }}</p>
      <p v-if="success" class="msg success">{{ success }}</p>
    </div>

    <!-- ── User list + act-as selector ── -->
    <div class="card section">
      <div class="section-header">
        <h2 class="section-title">All Users</h2>
        <div class="act-as-status" v-if="auth.actAsCustomerId">
          Acting as: <code>{{ auth.actAsCustomerId.slice(0,8) }}…</code>
          <button class="btn-ghost small" @click="clearActAs">Clear</button>
        </div>
      </div>

      <table class="table" v-if="users.length">
        <thead>
          <tr>
            <th>Email</th>
            <th>Role</th>
            <th>Display Name</th>
            <th>Company</th>
            <th>Act As</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="user in users" :key="user.id">
            <td>{{ user.email }}</td>
            <td><span class="role-badge" :class="user.role">{{ user.role }}</span></td>
            <td>{{ user.displayName || '—' }}</td>
            <td>{{ user.companyName || '—' }}</td>
            <td>
              <!-- Admin can only act as customer accounts, not other admins. -->
              <button
                v-if="user.role === 'customer'"
                class="btn-outline small"
                :class="{ active: auth.actAsCustomerId === user.id }"
                @click="actAs(user.id)"
              >
                {{ auth.actAsCustomerId === user.id ? '✓ Active' : 'Act As' }}
              </button>
              <span v-else class="text-secondary">—</span>
            </td>
          </tr>
        </tbody>
      </table>

      <p v-else class="empty-state">No users yet.</p>
    </div>
  </div>
</template>

<style scoped>
.form-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
  gap: 14px;
  align-items: end;
}

.field-action { display: flex; align-items: flex-end; }

.section { margin-bottom: 20px; }
.section-title { font-size: 15px; font-weight: 600; margin: 0 0 14px; }
.section-header { display: flex; align-items: center; justify-content: space-between; margin-bottom: 12px; }

.act-as-status { font-size: 13px; color: var(--text-secondary); display: flex; align-items: center; gap: 6px; }
.act-as-status code { background: var(--bg-page); padding: 1px 5px; border-radius: 3px; }

.msg { margin-top: 10px; padding: 8px 12px; border-radius: var(--radius); font-size: 13px; }
.msg.error   { background: #fef2f2; color: #b91c1c; border: 1px solid #fecaca; }
.msg.success { background: #f0fdf4; color: #15803d; border: 1px solid #bbf7d0; }

.btn-outline.active { background: var(--primary-light); color: var(--primary); border-color: var(--primary); }
.small { padding: 4px 8px; font-size: 12px; }
</style>
