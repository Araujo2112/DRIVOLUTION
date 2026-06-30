<template>
  <div class="p-8 w-full">

    <!-- Header -->
    <div class="flex items-start justify-between mb-8">
      <div>
        <h1 class="text-2xl font-medium text-background-900 dark:text-background-50">
          {{ t('clients.title') }}
        </h1>
        <p class="text-sm text-background-600 dark:text-background-400 mt-1">
          {{ t('clients.subtitle') }}
        </p>
      </div>
      <button
        @click="openCreateModal"
        class="flex items-center gap-2 bg-primary-500 hover:bg-primary-600 text-white text-sm font-medium px-4 py-2 rounded-lg transition-colors"
      >
        <span class="material-symbols-rounded text-base">person_add</span>
        {{ t('clients.new') }}
      </button>
    </div>

    <!-- Filtros -->
    <div class="flex gap-3 mb-6 items-center">
      <div class="relative w-64 shrink-0">
        <span class="material-symbols-rounded absolute left-3 top-1/2 -translate-y-1/2 text-background-400 text-base pointer-events-none">search</span>
        <input
          v-model="search"
          @input="onSearchInput"
          type="text"
          :placeholder="t('clients.searchPlaceholder')"
          class="w-full pl-9 pr-4 py-2 text-sm rounded-lg border border-background-300 dark:border-background-700 bg-background-50 dark:bg-background-800 text-background-900 dark:text-background-50 placeholder-background-400 focus:outline-none focus:border-primary-400"
        />
      </div>

      <button
        v-if="search"
        @click="clearFilters"
        class="shrink-0 px-3 py-2 text-sm rounded-lg border border-background-300 dark:border-background-700 text-background-500 hover:text-background-700 dark:hover:text-background-300 hover:bg-background-100 dark:hover:bg-background-700 transition-colors"
        title="Limpar filtros"
      >
        <span class="material-symbols-rounded text-base align-middle">close</span>
      </button>

      <select v-model="pageSize" @change="onPageSizeChange" class="ml-auto w-20 shrink-0">
        <option :value="25">25</option>
        <option :value="50">50</option>
        <option :value="100">100</option>
      </select>
    </div>

    <!-- Loading -->
    <div v-if="loading" class="flex items-center gap-2 text-background-500 text-sm py-12">
      <span class="material-symbols-rounded animate-spin text-lg">autorenew</span>
      {{ t('common.loading') }}
    </div>

    <!-- Tabela -->
    <div v-else>
      <div v-if="clients.length === 0" class="text-center py-16 text-background-500">
        <span class="material-symbols-rounded text-5xl block mb-3">group</span>
        <p class="text-sm">{{ t('clients.empty') }}</p>
      </div>

      <div v-else class="border border-background-300 dark:border-background-700 rounded-xl overflow-hidden">
        <!-- Header da tabela -->
        <div class="grid grid-cols-[2fr_2fr_1fr_1fr_44px] px-4 py-3 bg-background-100 dark:bg-background-800 border-b border-background-300 dark:border-background-700">
          <span class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('clients.fields.name') }}</span>
          <span class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('clients.fields.email') }}</span>
          <span class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('clients.fields.status') }}</span>
          <span class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('clients.fields.createdAt') }}</span>
          <span></span>
        </div>

        <!-- Rows -->
        <div
          v-for="client in clients"
          :key="client.id"
          class="grid grid-cols-[2fr_2fr_1fr_1fr_44px] px-4 py-3 border-b border-background-200 dark:border-background-700 last:border-0 bg-background-50 dark:bg-background-800 hover:bg-background-100 dark:hover:bg-background-750 transition-colors items-center"
        >
          <div class="flex items-center gap-3 min-w-0">
            <div class="w-8 h-8 rounded-full bg-primary-100 dark:bg-primary-950 flex items-center justify-center flex-shrink-0">
              <span class="text-xs font-semibold text-primary-600 dark:text-primary-300">
                {{ initials(client.name) }}
              </span>
            </div>
            <div class="text-sm font-medium text-background-800 dark:text-background-100 truncate">{{ client.name }}</div>
          </div>

          <span class="text-sm text-background-600 dark:text-background-400 truncate">{{ client.email || '—' }}</span>

          <div>
            <span
              class="text-xs font-medium px-2 py-1 rounded-full w-fit block"
              :class="client.status === 'active'
                ? 'bg-success-100 text-success-700 dark:bg-success-950 dark:text-success-300'
                : 'bg-background-200 text-background-500 dark:bg-background-700 dark:text-background-400'"
            >
              {{ client.status === 'active' ? t('clients.statusActive') : t('clients.statusInactive') }}
            </span>
          </div>

          <span class="text-sm text-background-500">{{ formatDate(client.createdAt) }}</span>

          <div class="flex justify-end">
            <button
              @click="openEditModal(client)"
              :title="t('clients.edit')"
              class="w-8 h-8 flex items-center justify-center rounded-lg text-background-400 hover:text-primary-600 hover:bg-primary-50 dark:hover:bg-primary-950 dark:hover:text-primary-300 transition-colors"
            >
              <span class="material-symbols-rounded text-lg">edit</span>
            </button>
          </div>
        </div>
      </div>

      <!-- Paginação -->
      <div v-if="totalPages > 1" class="flex items-center justify-between mt-4 text-sm text-background-600 dark:text-background-400">
        <span>{{ t('common.showing', { from: (currentPage - 1) * pageSize + 1, to: Math.min(currentPage * pageSize, total), total }) }}</span>
        <div class="flex gap-1">
          <button
            @click="goToPage(currentPage - 1)"
            :disabled="currentPage === 1"
            class="px-3 py-1.5 rounded-lg border border-background-300 dark:border-background-700 hover:bg-background-100 dark:hover:bg-background-700 disabled:opacity-40 disabled:cursor-not-allowed transition-colors"
          >
            <span class="material-symbols-rounded text-base">chevron_left</span>
          </button>
          <button
            v-for="p in visiblePages"
            :key="p"
            @click="goToPage(p)"
            class="px-3 py-1.5 rounded-lg border transition-colors"
            :class="p === currentPage ? 'bg-primary-500 text-white border-primary-500' : 'border-background-300 dark:border-background-700 hover:bg-background-100 dark:hover:bg-background-700'"
          >
            {{ p }}
          </button>
          <button
            @click="goToPage(currentPage + 1)"
            :disabled="currentPage === totalPages"
            class="px-3 py-1.5 rounded-lg border border-background-300 dark:border-background-700 hover:bg-background-100 dark:hover:bg-background-700 disabled:opacity-40 disabled:cursor-not-allowed transition-colors"
          >
            <span class="material-symbols-rounded text-base">chevron_right</span>
          </button>
        </div>
      </div>
    </div>

    <!-- ── Modal: criar cliente ── -->
    <div
      v-if="showCreateModal"
      class="fixed inset-0 z-50 flex items-center justify-center bg-black/40"
      @click.self="closeCreateModal"
    >
      <div class="bg-background-50 dark:bg-background-900 border border-background-300 dark:border-background-700 rounded-2xl shadow-2xl w-full max-w-md mx-4 p-6">

        <div class="flex items-center justify-between mb-6">
          <h2 class="text-lg font-medium text-background-900 dark:text-background-50">
            {{ t('clients.new') }}
          </h2>
          <button @click="closeCreateModal" class="text-background-400 hover:text-background-600 dark:hover:text-background-200 transition-colors">
            <span class="material-symbols-rounded">close</span>
          </button>
        </div>

        <div class="flex flex-col gap-4">
          <div>
            <label class="text-xs font-medium text-background-500 uppercase tracking-wider mb-1.5 block">
              {{ t('clients.fields.name') }}
            </label>
            <input v-model="createForm.name" type="text" :placeholder="t('clients.placeholders.name')" class="w-full" />
          </div>
          <div>
            <label class="text-xs font-medium text-background-500 uppercase tracking-wider mb-1.5 block">
              {{ t('clients.fields.email') }}
            </label>
            <input v-model="createForm.email" type="email" :placeholder="t('clients.placeholders.email')" class="w-full" />
          </div>
          <p class="text-xs text-background-500 dark:text-background-400">
            {{ t('clients.passwordGeneratedHint') }}
          </p>
        </div>

        <div v-if="createFormError" class="mt-4 text-sm text-danger-600 dark:text-danger-400 bg-danger-50 dark:bg-background-800 border border-danger-200 dark:border-danger-800 rounded-lg px-3 py-2">
          {{ createFormError }}
        </div>

        <div class="flex justify-end gap-3 mt-6">
          <button @click="closeCreateModal" class="px-4 py-2 rounded-lg text-sm font-medium text-background-600 dark:text-background-400 hover:bg-background-100 dark:hover:bg-background-800 transition-colors">
            {{ t('common.cancel') }}
          </button>
          <button @click="createClient" :disabled="saving" class="flex items-center gap-2 px-4 py-2 rounded-lg text-sm font-medium bg-primary-500 hover:bg-primary-600 disabled:opacity-50 disabled:cursor-not-allowed text-white transition-colors">
            <span v-if="saving" class="material-symbols-rounded animate-spin text-base">autorenew</span>
            {{ t('clients.create') }}
          </button>
        </div>
      </div>
    </div>

    <!-- ── Modal: editar cliente ── -->
    <div
      v-if="showEditModal && editingClient"
      class="fixed inset-0 z-50 flex items-center justify-center bg-black/40"
      @click.self="closeEditModal"
    >
      <div class="bg-background-50 dark:bg-background-900 border border-background-300 dark:border-background-700 rounded-2xl shadow-2xl w-full max-w-md mx-4 p-6">

        <!-- Header do modal -->
        <div class="flex items-center justify-between mb-6">
          <div class="flex items-center gap-3">
            <div class="w-9 h-9 rounded-full bg-primary-100 dark:bg-primary-950 flex items-center justify-center">
              <span class="text-sm font-semibold text-primary-600 dark:text-primary-300">{{ initials(editingClient.name) }}</span>
            </div>
            <div>
              <h2 class="text-base font-medium text-background-900 dark:text-background-50">{{ editingClient.name }}</h2>
              <p class="text-xs text-background-500">{{ editingClient.email || '—' }}</p>
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
              {{ t('clients.fields.name') }}
            </label>
            <input v-model="editForm.name" type="text" class="w-full" />
          </div>

          <!-- Email -->
          <div>
            <label class="text-xs font-medium text-background-500 uppercase tracking-wider mb-1.5 block">
              {{ t('clients.fields.email') }}
            </label>
            <input v-model="editForm.email" type="email" class="w-full" />
          </div>

          <!-- Estado -->
          <div>
            <label class="text-xs font-medium text-background-500 uppercase tracking-wider mb-1.5 block">
              {{ t('clients.fields.status') }}
            </label>
            <div class="flex gap-2">
              <button
                @click="editForm.status = 'active'"
                class="flex-1 py-2 rounded-lg text-sm font-medium border transition-colors"
                :class="editForm.status === 'active'
                  ? 'bg-success-100 border-success-300 text-success-700 dark:bg-success-950 dark:border-success-700 dark:text-success-300'
                  : 'border-background-300 dark:border-background-700 text-background-500 hover:bg-background-100 dark:hover:bg-background-800'"
              >
                {{ t('clients.statusActive') }}
              </button>
              <button
                @click="editForm.status = 'inactive'"
                class="flex-1 py-2 rounded-lg text-sm font-medium border transition-colors"
                :class="editForm.status === 'inactive'
                  ? 'bg-background-200 border-background-400 text-background-700 dark:bg-background-700 dark:border-background-500 dark:text-background-200'
                  : 'border-background-300 dark:border-background-700 text-background-500 hover:bg-background-100 dark:hover:bg-background-800'"
              >
                {{ t('clients.statusInactive') }}
              </button>
            </div>
          </div>

          <!-- Reset password -->
          <div class="border-t border-background-200 dark:border-background-700 pt-4">
            <div v-if="!confirmingReset" class="flex items-center justify-between">
              <div>
                <p class="text-sm font-medium text-background-800 dark:text-background-100">{{ t('clients.resetPassword') }}</p>
                <p class="text-xs text-background-500 mt-0.5">{{ t('clients.resetPasswordHint') }}</p>
              </div>
              <button
                @click="confirmingReset = true"
                class="flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-sm font-medium border border-background-300 dark:border-background-700 text-background-600 dark:text-background-300 hover:bg-background-100 dark:hover:bg-background-800 transition-colors"
              >
                <span class="material-symbols-rounded text-base">lock_reset</span>
                {{ t('clients.resetPasswordBtn') }}
              </button>
            </div>

            <!-- Confirmação inline -->
            <div v-else class="bg-warning-50 dark:bg-warning-950/30 border border-warning-200 dark:border-warning-800 rounded-lg p-3">
              <p class="text-sm text-warning-700 dark:text-warning-300 mb-3">{{ t('clients.resetPasswordConfirm') }}</p>
              <div class="flex gap-2 justify-end">
                <button
                  @click="confirmingReset = false"
                  class="px-3 py-1.5 rounded-lg text-sm font-medium text-background-600 dark:text-background-400 hover:bg-background-100 dark:hover:bg-background-800 transition-colors"
                >
                  {{ t('common.cancel') }}
                </button>
                <button
                  @click="resetPassword"
                  :disabled="resettingPassword"
                  class="flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-sm font-medium bg-warning-500 hover:bg-warning-600 disabled:opacity-50 text-white transition-colors"
                >
                  <span v-if="resettingPassword" class="material-symbols-rounded animate-spin text-base">autorenew</span>
                  {{ t('clients.resetPasswordConfirmBtn') }}
                </button>
              </div>
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

    <!-- ── Modal: password temporária ── -->
    <div
      v-if="showCredentialsModal"
      class="fixed inset-0 z-50 flex items-center justify-center bg-black/40"
    >
      <div class="bg-background-50 dark:bg-background-900 border border-background-300 dark:border-background-700 rounded-2xl shadow-2xl w-full max-w-md mx-4 p-6">

        <div class="flex items-center gap-2 mb-4">
          <span class="material-symbols-rounded text-warning-500">warning</span>
          <h2 class="text-lg font-medium text-background-900 dark:text-background-50">
            {{ t('clients.tempPasswordTitle') }}
          </h2>
        </div>

        <p class="text-sm text-background-600 dark:text-background-400 mb-4">
          {{ t('clients.tempPasswordWarning', { name: credentialsTarget?.name, email: credentialsTarget?.email }) }}
        </p>

        <div class="flex items-center gap-2 bg-background-100 dark:bg-background-800 border border-background-300 dark:border-background-700 rounded-lg px-3 py-2.5">
          <code class="flex-1 text-sm font-mono text-background-900 dark:text-background-50 select-all">{{ generatedPassword }}</code>
          <button @click="copyPassword" class="text-background-400 hover:text-background-700 dark:hover:text-background-200 transition-colors" :title="t('clients.tempPasswordCopy')">
            <span class="material-symbols-rounded text-lg">{{ copied ? 'check' : 'content_copy' }}</span>
          </button>
        </div>

        <p v-if="copied" class="text-xs text-success-600 dark:text-success-400 mt-2">
          {{ t('clients.tempPasswordCopied') }}
        </p>

        <div class="flex justify-end mt-6">
          <button @click="closeCredentialsModal" class="px-4 py-2 rounded-lg text-sm font-medium bg-primary-500 hover:bg-primary-600 text-white transition-colors">
            {{ t('clients.tempPasswordClose') }}
          </button>
        </div>
      </div>
    </div>

  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useI18n } from 'vue-i18n'
import { clientPortalService, type ClientAccount } from '@/services/clientPortalService'
import { formatDate } from '@/utils/dates'
import { toast } from '@/plugins/toast'

const { t } = useI18n()

// ── State ─────────────────────────────────────────────────────────────────────
const clients = ref<ClientAccount[]>([])
const loading = ref(true)
const saving  = ref(false)

// Filtro + paginação
const search = ref('')
const total = ref(0)
const currentPage = ref(1)
const pageSize = ref(25)

const totalPages = computed(() => Math.ceil(total.value / pageSize.value))
const visiblePages = computed(() => {
  const pages: number[] = []
  const start = Math.max(1, currentPage.value - 2)
  const end = Math.min(totalPages.value, currentPage.value + 2)
  for (let i = start; i <= end; i++) pages.push(i)
  return pages
})

// Modal: criar
const showCreateModal = ref(false)
const createFormError = ref('')
const createForm = ref({ name: '', email: '' })

// Modal: editar
const showEditModal     = ref(false)
const editingClient     = ref<ClientAccount | null>(null)
const editFormError     = ref('')
const resettingPassword = ref(false)
const confirmingReset   = ref(false)
const editForm = ref({ name: '', email: '', status: 'active' })

// Modal: password gerada
const showCredentialsModal = ref(false)
const generatedPassword    = ref('')
const credentialsTarget    = ref<{ name: string; email: string } | null>(null)
const copied               = ref(false)

// ── Lifecycle ─────────────────────────────────────────────────────────────────
onMounted(load)

// ── Data ──────────────────────────────────────────────────────────────────────
async function load() {
  loading.value = true
  try {
    const res = await clientPortalService.getClientsPaged({
      page: currentPage.value,
      pageSize: pageSize.value,
      search: search.value || undefined,
    })
    clients.value = res.data
    total.value = res.total
  }
  catch { toast.error(t('errors.loadFailed')) }
  finally { loading.value = false }
}

let debounceTimer: ReturnType<typeof setTimeout>
function onSearchInput() {
  clearTimeout(debounceTimer)
  debounceTimer = setTimeout(() => {
    currentPage.value = 1
    load()
  }, 300)
}

function onPageSizeChange() {
  currentPage.value = 1
  load()
}

function goToPage(page: number) {
  if (page < 1 || page > totalPages.value) return
  currentPage.value = page
  load()
}

function clearFilters() {
  search.value = ''
  currentPage.value = 1
  load()
}

// ── Modal: criar ──────────────────────────────────────────────────────────────
function openCreateModal() {
  createForm.value = { name: '', email: '' }
  createFormError.value = ''
  showCreateModal.value = true
}

function closeCreateModal() {
  showCreateModal.value = false
  createFormError.value = ''
}

async function createClient() {
  createFormError.value = ''
  if (!createForm.value.name.trim() || !createForm.value.email.trim()) {
    createFormError.value = t('clients.validationRequired')
    return
  }
  saving.value = true
  try {
    const result = await clientPortalService.createClient(createForm.value)
    closeCreateModal()
    await load()
    credentialsTarget.value = { name: createForm.value.name, email: createForm.value.email }
    generatedPassword.value = result.temporaryPassword
    copied.value = false
    showCredentialsModal.value = true
  } catch (e: any) {
    createFormError.value = e?.response?.data || t('clients.errorCreate')
  } finally {
    saving.value = false
  }
}

// ── Modal: editar ─────────────────────────────────────────────────────────────
function openEditModal(client: ClientAccount) {
  editingClient.value = client
  editForm.value = { name: client.name, email: client.email ?? '', status: client.status }
  editFormError.value = ''
  confirmingReset.value = false
  showEditModal.value = true
}

function closeEditModal() {
  showEditModal.value = false
  editingClient.value = null
  editFormError.value = ''
  confirmingReset.value = false
}

async function saveEdit() {
  if (!editingClient.value) return
  editFormError.value = ''
  if (!editForm.value.name.trim()) {
    editFormError.value = t('clients.validationRequired')
    return
  }
  saving.value = true
  try {
    await clientPortalService.updateClient(editingClient.value.id, {
      name:   editForm.value.name.trim(),
      email:  editForm.value.email.trim(),
      status: editForm.value.status,
    })
    closeEditModal()
    await load()
  } catch (e: any) {
    editFormError.value = e?.response?.data || t('errors.saveFailed')
  } finally {
    saving.value = false
  }
}

async function resetPassword() {
  if (!editingClient.value) return
  resettingPassword.value = true
  try {
    const result = await clientPortalService.resetPassword(editingClient.value.id)
    const target = { name: editingClient.value.name, email: editingClient.value.email ?? '' }
    closeEditModal()
    credentialsTarget.value = target
    generatedPassword.value = result.temporaryPassword
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
  generatedPassword.value = ''
  credentialsTarget.value = null
}

// ── Helpers ───────────────────────────────────────────────────────────────────
function initials(name: string): string {
  return name.split(' ').slice(0, 2).map(n => n[0]?.toUpperCase() ?? '').join('')
}
</script>