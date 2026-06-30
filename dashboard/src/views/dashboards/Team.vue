<template>
  <div class="p-8 w-full">

    <!-- Header -->
    <div class="flex items-start justify-between mb-8">
      <div>
        <h1 class="text-2xl font-medium text-background-900 dark:text-background-50">
          {{ t('team.title') }}
        </h1>
        <p class="text-sm text-background-600 dark:text-background-400 mt-1">
          {{ t('team.subtitle') }}
        </p>
      </div>
      <button
        @click="openCreateModal"
        class="flex items-center gap-2 bg-primary-500 hover:bg-primary-600 text-white text-sm font-medium px-4 py-2 rounded-lg transition-colors"
      >
        <span class="material-symbols-rounded text-base">person_add</span>
        {{ t('team.newUser') }}
      </button>
    </div>

    <!-- Filtros -->
    <div class="flex gap-3 mb-6 items-center">
      <div class="relative w-64 shrink-0">
        <span class="material-symbols-rounded absolute left-3 top-1/2 -translate-y-1/2 text-background-400 text-base pointer-events-none">search</span>
        <input
          v-model="search"
          type="text"
          :placeholder="t('team.searchPlaceholder')"
          class="w-full pl-9 pr-4 py-2 text-sm rounded-lg border border-background-300 dark:border-background-700 bg-background-50 dark:bg-background-800 text-background-900 dark:text-background-50 placeholder-background-400 focus:outline-none focus:border-primary-400"
        />
      </div>

      <select v-model="roleFilter" class="w-40 shrink-0">
        <option value="all">{{ t('team.allRoles') }}</option>
        <option value="admin">{{ t('team.roles.admin') }}</option>
        <option value="manager">{{ t('team.roles.manager') }}</option>
        <option value="operator">{{ t('team.roles.operator') }}</option>
      </select>

      <button
        v-if="search || roleFilter !== 'all'"
        @click="clearFilters"
        class="shrink-0 px-3 py-2 text-sm rounded-lg border border-background-300 dark:border-background-700 text-background-500 hover:text-background-700 dark:hover:text-background-300 hover:bg-background-100 dark:hover:bg-background-700 transition-colors"
        title="Limpar filtros"
      >
        <span class="material-symbols-rounded text-base align-middle">close</span>
      </button>
    </div>

    <!-- Loading -->
    <div v-if="loading" class="flex items-center gap-2 text-background-500 text-sm py-12">
      <span class="material-symbols-rounded animate-spin text-lg">autorenew</span>
      {{ t('common.loading') }}
    </div>

    <!-- Tabela -->
    <div v-else class="border border-background-300 dark:border-background-700 rounded-xl overflow-hidden">
      <div class="grid grid-cols-[2fr_1.5fr_1fr_1fr_44px] px-4 py-3 bg-background-100 dark:bg-background-800 border-b border-background-300 dark:border-background-700">
        <span class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('team.fields.name') }}</span>
        <span class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('team.fields.email') }}</span>
        <span class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('team.fields.role') }}</span>
        <span class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('team.fields.status') }}</span>
        <span></span>
      </div>

      <div v-if="users.length === 0" class="text-center py-12 text-background-500">
        <span class="material-symbols-rounded text-4xl block mb-2">group</span>
        <p class="text-sm">{{ t('team.empty') }}</p>
      </div>

      <div v-else-if="filteredUsers.length === 0" class="text-center py-12 text-background-500">
        <span class="material-symbols-rounded text-4xl block mb-2">search_off</span>
        <p class="text-sm">{{ t('team.noResults') }}</p>
      </div>

      <div
        v-for="user in filteredUsers"
        :key="user.id"
        class="grid grid-cols-[2fr_1.5fr_1fr_1fr_44px] px-4 py-3 border-b border-background-200 dark:border-background-700 last:border-0 bg-background-50 dark:bg-background-800 hover:bg-background-100 dark:hover:bg-background-750 transition-colors items-center"
      >
        <div class="flex items-center gap-3 min-w-0">
          <div class="w-8 h-8 rounded-full bg-primary-100 dark:bg-primary-950 flex items-center justify-center flex-shrink-0">
            <span class="text-xs font-semibold text-primary-600 dark:text-primary-300">
              {{ initials(user.name) }}
            </span>
          </div>
          <span class="text-sm font-medium text-background-800 dark:text-background-100 truncate">{{ user.name }}</span>
        </div>
        <span class="text-sm text-background-600 dark:text-background-400 truncate">{{ user.email }}</span>
        <span
          class="text-xs font-medium px-2 py-1 rounded-full w-fit"
          :class="roleClass(user.role)"
        >
          {{ t(`team.roles.${user.role}`) }}
        </span>
        <span
          class="text-xs font-medium px-2 py-1 rounded-full w-fit"
          :class="user.status === 'active'
            ? 'bg-success-100 text-success-700 dark:bg-success-950 dark:text-success-300'
            : 'bg-background-200 text-background-500 dark:bg-background-700 dark:text-background-400'"
        >
          {{ user.status === 'active' ? t('team.statusActive') : t('team.statusInactive') }}
        </span>
        <div class="flex justify-end">
          <button
            v-if="user.role !== 'admin'"
            @click="openEditModal(user)"
            :title="t('common.edit')"
            class="w-8 h-8 flex items-center justify-center rounded-lg text-background-400 hover:text-primary-600 hover:bg-primary-50 dark:hover:bg-primary-950 dark:hover:text-primary-300 transition-colors"
          >
            <span class="material-symbols-rounded text-lg">edit</span>
          </button>
        </div>
      </div>
    </div>

    <!-- ── Modal: criar utilizador ── -->
    <div
      v-if="showCreateModal"
      class="fixed inset-0 z-50 flex items-center justify-center bg-black/40"
      @click.self="closeCreateModal"
    >
      <div class="bg-background-50 dark:bg-background-900 border border-background-300 dark:border-background-700 rounded-2xl shadow-2xl w-full max-w-md mx-4 p-6">

        <div class="flex items-center justify-between mb-6">
          <h2 class="text-lg font-medium text-background-900 dark:text-background-50">
            {{ t('team.newUser') }}
          </h2>
          <button @click="closeCreateModal" class="text-background-400 hover:text-background-600 dark:hover:text-background-200 transition-colors">
            <span class="material-symbols-rounded">close</span>
          </button>
        </div>

        <div class="flex flex-col gap-4">
          <div>
            <label class="text-xs font-medium text-background-500 uppercase tracking-wider mb-1.5 block">
              {{ t('team.fields.name') }}
            </label>
            <input v-model="createForm.name" type="text" :placeholder="t('team.placeholders.name')" class="w-full" />
          </div>

          <div>
            <label class="text-xs font-medium text-background-500 uppercase tracking-wider mb-1.5 block">
              {{ t('team.fields.email') }}
            </label>
            <div class="flex items-stretch">
              <input v-model="createForm.username" type="text" :placeholder="t('team.placeholders.username')" class="w-full rounded-r-none" />
              <span class="flex items-center px-3 text-sm text-background-500 dark:text-background-400 bg-background-100 dark:bg-background-800 border border-l-0 border-background-300 dark:border-background-700 rounded-r-lg whitespace-nowrap">
                {{ emailDomain }}
              </span>
            </div>
          </div>

          <div>
            <label class="text-xs font-medium text-background-500 uppercase tracking-wider mb-1.5 block">
              {{ t('team.fields.role') }}
            </label>
            <select v-model="createForm.role" class="w-full">
              <option value="manager">{{ t('team.roles.manager') }}</option>
              <option value="operator">{{ t('team.roles.operator') }}</option>
            </select>
          </div>

          <p class="text-xs text-background-500 dark:text-background-400">
            {{ t('team.passwordGeneratedHint') }}
          </p>
        </div>

        <div v-if="createFormError" class="mt-4 text-sm text-danger-600 dark:text-danger-400 bg-danger-50 dark:bg-background-800 border border-danger-200 dark:border-danger-800 rounded-lg px-3 py-2">
          {{ createFormError }}
        </div>

        <div class="flex justify-end gap-3 mt-6">
          <button @click="closeCreateModal" class="px-4 py-2 rounded-lg text-sm font-medium text-background-600 dark:text-background-400 hover:bg-background-100 dark:hover:bg-background-800 transition-colors">
            {{ t('common.cancel') }}
          </button>
          <button @click="createUser" :disabled="saving" class="flex items-center gap-2 px-4 py-2 rounded-lg text-sm font-medium bg-primary-500 hover:bg-primary-600 disabled:opacity-50 disabled:cursor-not-allowed text-white transition-colors">
            <span v-if="saving" class="material-symbols-rounded animate-spin text-base">autorenew</span>
            {{ t('common.save') }}
          </button>
        </div>
      </div>
    </div>

    <!-- ── Modal: editar utilizador ── -->
    <div
      v-if="showEditModal && editingUser"
      class="fixed inset-0 z-50 flex items-center justify-center bg-black/40"
      @click.self="closeEditModal"
    >
      <div class="bg-background-50 dark:bg-background-900 border border-background-300 dark:border-background-700 rounded-2xl shadow-2xl w-full max-w-md mx-4 p-6">

        <div class="flex items-center justify-between mb-6">
          <div class="flex items-center gap-3">
            <div class="w-9 h-9 rounded-full bg-primary-100 dark:bg-primary-950 flex items-center justify-center">
              <span class="text-sm font-semibold text-primary-600 dark:text-primary-300">{{ initials(editingUser.name) }}</span>
            </div>
            <div>
              <h2 class="text-base font-medium text-background-900 dark:text-background-50">{{ editingUser.name }}</h2>
              <p class="text-xs text-background-500">{{ editingUser.email }}</p>
            </div>
          </div>
          <button @click="closeEditModal" class="text-background-400 hover:text-background-600 dark:hover:text-background-200 transition-colors">
            <span class="material-symbols-rounded">close</span>
          </button>
        </div>

        <div class="flex flex-col gap-4">
          <!-- Nome -->
          <div>
            <label class="text-xs font-medium text-background-500 uppercase tracking-wider mb-1.5 block">
              {{ t('team.fields.name') }}
            </label>
            <input v-model="editForm.name" type="text" class="w-full" />
          </div>

          <!-- Role -->
          <div>
            <label class="text-xs font-medium text-background-500 uppercase tracking-wider mb-1.5 block">
              {{ t('team.fields.role') }}
            </label>
            <select v-model="editForm.role" class="w-full">
              <option value="manager">{{ t('team.roles.manager') }}</option>
              <option value="operator">{{ t('team.roles.operator') }}</option>
            </select>
          </div>

          <!-- Estado -->
          <div>
            <label class="text-xs font-medium text-background-500 uppercase tracking-wider mb-1.5 block">
              {{ t('team.fields.status') }}
            </label>
            <div class="flex gap-2">
              <button
                @click="editForm.status = 'active'"
                class="flex-1 py-2 rounded-lg text-sm font-medium border transition-colors"
                :class="editForm.status === 'active'
                  ? 'bg-success-100 border-success-300 text-success-700 dark:bg-success-950 dark:border-success-700 dark:text-success-300'
                  : 'border-background-300 dark:border-background-700 text-background-500 hover:bg-background-100 dark:hover:bg-background-800'"
              >
                {{ t('team.statusActive') }}
              </button>
              <button
                @click="editForm.status = 'inactive'"
                class="flex-1 py-2 rounded-lg text-sm font-medium border transition-colors"
                :class="editForm.status === 'inactive'
                  ? 'bg-background-200 border-background-400 text-background-700 dark:bg-background-700 dark:border-background-500 dark:text-background-200'
                  : 'border-background-300 dark:border-background-700 text-background-500 hover:bg-background-100 dark:hover:bg-background-800'"
              >
                {{ t('team.statusInactive') }}
              </button>
            </div>
          </div>

          <!-- Reset password -->
          <div class="border-t border-background-200 dark:border-background-700 pt-4">
            <div class="flex items-center justify-between">
              <div>
                <p class="text-sm font-medium text-background-800 dark:text-background-100">{{ t('team.resetPassword') }}</p>
                <p class="text-xs text-background-500 mt-0.5">{{ t('team.resetPasswordHint') }}</p>
              </div>
              <button
                @click="resetPassword"
                :disabled="resettingPassword"
                class="flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-sm font-medium border border-background-300 dark:border-background-700 text-background-600 dark:text-background-300 hover:bg-background-100 dark:hover:bg-background-800 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
              >
                <span v-if="resettingPassword" class="material-symbols-rounded animate-spin text-base">autorenew</span>
                <span v-else class="material-symbols-rounded text-base">lock_reset</span>
                {{ t('team.resetPasswordBtn') }}
              </button>
            </div>
          </div>
        </div>

        <div v-if="editFormError" class="mt-4 text-sm text-danger-600 dark:text-danger-400 bg-danger-50 dark:bg-background-800 border border-danger-200 dark:border-danger-800 rounded-lg px-3 py-2">
          {{ editFormError }}
        </div>

        <div class="flex justify-end gap-3 mt-6">
          <button @click="closeEditModal" class="px-4 py-2 rounded-lg text-sm font-medium text-background-600 dark:text-background-400 hover:bg-background-100 dark:hover:bg-background-800 transition-colors">
            {{ t('common.cancel') }}
          </button>
          <button @click="saveEdit" :disabled="saving" class="flex items-center gap-2 px-4 py-2 rounded-lg text-sm font-medium bg-primary-500 hover:bg-primary-600 disabled:opacity-50 disabled:cursor-not-allowed text-white transition-colors">
            <span v-if="saving" class="material-symbols-rounded animate-spin text-base">autorenew</span>
            {{ t('common.save') }}
          </button>
        </div>
      </div>
    </div>

    <!-- ── Modal: password temporária (criação ou reset) ── -->
    <div
      v-if="showCredentialsModal"
      class="fixed inset-0 z-50 flex items-center justify-center bg-black/40"
    >
      <div class="bg-background-50 dark:bg-background-900 border border-background-300 dark:border-background-700 rounded-2xl shadow-2xl w-full max-w-md mx-4 p-6">

        <div class="flex items-center gap-2 mb-4">
          <span class="material-symbols-rounded text-warning-500">warning</span>
          <h2 class="text-lg font-medium text-background-900 dark:text-background-50">
            {{ t('team.tempPasswordTitle') }}
          </h2>
        </div>

        <p class="text-sm text-background-600 dark:text-background-400 mb-4">
          {{ t('team.tempPasswordWarning', { name: credentialsTarget?.name, email: credentialsTarget?.email }) }}
        </p>

        <div class="flex items-center gap-2 bg-background-100 dark:bg-background-800 border border-background-300 dark:border-background-700 rounded-lg px-3 py-2.5">
          <code class="flex-1 text-sm font-mono text-background-900 dark:text-background-50 select-all">{{ generatedPassword }}</code>
          <button @click="copyPassword" class="text-background-400 hover:text-background-700 dark:hover:text-background-200 transition-colors" :title="t('team.tempPasswordCopy')">
            <span class="material-symbols-rounded text-lg">{{ copied ? 'check' : 'content_copy' }}</span>
          </button>
        </div>

        <p v-if="copied" class="text-xs text-success-600 dark:text-success-400 mt-2">
          {{ t('team.tempPasswordCopied') }}
        </p>

        <div class="flex justify-end mt-6">
          <button @click="closeCredentialsModal" class="px-4 py-2 rounded-lg text-sm font-medium bg-primary-500 hover:bg-primary-600 text-white transition-colors">
            {{ t('team.tempPasswordClose') }}
          </button>
        </div>
      </div>
    </div>

  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useI18n } from 'vue-i18n'
import axios from '@/axios'
import { toast } from '@/plugins/toast'

const { t } = useI18n()

// ── Types ─────────────────────────────────────────────────────────────────────
interface AppUser {
  id: number
  name: string
  email: string
  role: 'admin' | 'manager' | 'operator' | 'client'
  status: string
}

// ── State ─────────────────────────────────────────────────────────────────────
const loading = ref(false)
const users   = ref<AppUser[]>([])
const saving  = ref(false)

// Filtros (client-side — lista pequena, sem necessidade de paginação no servidor)
const search     = ref('')
const roleFilter = ref<'all' | 'admin' | 'manager' | 'operator'>('all')

const filteredUsers = computed(() => {
  const term = search.value.trim().toLowerCase()
  return users.value.filter(u => {
    const matchesSearch = !term || u.name.toLowerCase().includes(term)
    const matchesRole   = roleFilter.value === 'all' || u.role === roleFilter.value
    return matchesSearch && matchesRole
  })
})

function clearFilters() {
  search.value = ''
  roleFilter.value = 'all'
}

// Modal: criar
const showCreateModal  = ref(false)
const createFormError  = ref('')
const createForm = ref({ name: '', username: '', role: 'operator' as 'manager' | 'operator' })

// Modal: editar
const showEditModal     = ref(false)
const editingUser       = ref<AppUser | null>(null)
const editFormError     = ref('')
const resettingPassword = ref(false)
const editForm = ref({ name: '', role: 'operator' as 'manager' | 'operator', status: 'active' })

// Modal: password gerada
const showCredentialsModal = ref(false)
const generatedPassword    = ref('')
const credentialsTarget    = ref<{ name: string; email: string } | null>(null)
const copied               = ref(false)

const EMAIL_DOMAIN = '@drivolution.pt'
const emailDomain  = EMAIL_DOMAIN

// ── Lifecycle ─────────────────────────────────────────────────────────────────
onMounted(loadUsers)

// ── Data ──────────────────────────────────────────────────────────────────────
async function loadUsers() {
  loading.value = true
  try {
    const res = await axios.get('/User')
    users.value = res.data?.$values ?? res.data ?? []
  } catch {
    toast.error(t('errors.loadFailed'))
  } finally {
    loading.value = false
  }
}

// ── Modal: criar ──────────────────────────────────────────────────────────────
function openCreateModal() {
  createForm.value = { name: '', username: '', role: 'operator' }
  createFormError.value = ''
  showCreateModal.value = true
}

function closeCreateModal() {
  showCreateModal.value = false
  createFormError.value = ''
}

async function createUser() {
  createFormError.value = ''
  const username = createForm.value.username.trim()

  if (!createForm.value.name.trim() || !username) {
    createFormError.value = t('team.validationRequired')
    return
  }
  if (/\s|@/.test(username)) {
    createFormError.value = t('team.usernameInvalid')
    return
  }

  saving.value = true
  try {
    const res = await axios.post('/Auth/register', {
      name:  createForm.value.name.trim(),
      email: `${username}${emailDomain}`,
      role:  createForm.value.role,
    })

    toast.success(t('team.userCreated'))
    closeCreateModal()
    await loadUsers()

    credentialsTarget.value = res.data?.user ?? null
    generatedPassword.value = res.data?.temporaryPassword ?? ''
    copied.value = false
    showCredentialsModal.value = true
  } catch (err: any) {
    createFormError.value = err?.response?.data ?? t('errors.saveFailed')
  } finally {
    saving.value = false
  }
}

// ── Modal: editar ─────────────────────────────────────────────────────────────
function openEditModal(user: AppUser) {
  editingUser.value = user
  editForm.value = {
    name:   user.name,
    role:   user.role === 'admin' ? 'manager' : user.role as 'manager' | 'operator',
    status: user.status,
  }
  editFormError.value = ''
  showEditModal.value = true
}

function closeEditModal() {
  showEditModal.value = false
  editingUser.value   = null
  editFormError.value = ''
}

async function saveEdit() {
  if (!editingUser.value) return
  editFormError.value = ''

  if (!editForm.value.name.trim()) {
    editFormError.value = t('team.validationRequired')
    return
  }

  saving.value = true
  try {
    await axios.put(`/User/${editingUser.value.id}`, {
      name:   editForm.value.name.trim(),
      role:   editForm.value.role,
      status: editForm.value.status,
    })
    toast.success(t('team.userUpdated'))
    closeEditModal()
    await loadUsers()
  } catch (err: any) {
    editFormError.value = err?.response?.data ?? t('errors.saveFailed')
  } finally {
    saving.value = false
  }
}

async function resetPassword() {
  if (!editingUser.value) return
  resettingPassword.value = true
  try {
    const res = await axios.post(`/User/${editingUser.value.id}/reset-password`)
    const target = { name: editingUser.value.name, email: editingUser.value.email }
    closeEditModal()

    credentialsTarget.value = target
    generatedPassword.value = res.data?.temporaryPassword ?? ''
    copied.value = false
    showCredentialsModal.value = true
  } catch {
    toast.error(t('errors.saveFailed'))
  } finally {
    resettingPassword.value = false
  }
}

// ── Modal: password gerada ────────────────────────────────────────────────────
async function copyPassword() {
  try {
    await navigator.clipboard.writeText(generatedPassword.value)
    copied.value = true
  } catch {
    toast.error(t('errors.saveFailed'))
  }
}

function closeCredentialsModal() {
  showCredentialsModal.value = false
  generatedPassword.value    = ''
  credentialsTarget.value    = null
}

// ── Helpers ───────────────────────────────────────────────────────────────────
function initials(name: string): string {
  return name.split(' ').slice(0, 2).map(n => n[0]?.toUpperCase() ?? '').join('')
}

function roleClass(role: string): string {
  switch (role) {
    case 'admin':    return 'bg-danger-100 text-danger-700 dark:bg-danger-950 dark:text-danger-300'
    case 'manager':  return 'bg-warning-100 text-warning-700 dark:bg-warning-950 dark:text-warning-300'
    case 'operator': return 'bg-primary-100 text-primary-700 dark:bg-primary-950 dark:text-primary-300'
    case 'client':   return 'bg-background-200 text-background-600 dark:bg-background-700 dark:text-background-300'
    default:         return 'bg-background-200 text-background-500'
  }
}
</script>