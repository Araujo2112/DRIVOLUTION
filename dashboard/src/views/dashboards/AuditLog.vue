<template>
  <div class="p-8 w-full">

    <!-- Header -->
    <div class="mb-8">
      <h1 class="text-2xl font-medium text-background-900 dark:text-background-50">
        {{ t('audit.title') }}
      </h1>
      <p class="text-sm text-background-600 dark:text-background-400 mt-1">
        {{ t('audit.subtitle') }}
      </p>
    </div>

    <!-- Filtros -->
    <div class="flex gap-3 mb-6 items-center">
      <select v-model="filterEntity" @change="onFilterChange" class="w-44 shrink-0">
        <option value="">{{ t('audit.filterAllEntities') }}</option>
        <option v-for="e in ENTITY_VALUES" :key="e" :value="e">{{ entityLabel(e) }}</option>
      </select>

      <select v-model="filterUserId" @change="onFilterChange" class="w-44 shrink-0">
        <option value="">{{ t('audit.filterAllUsers') }}</option>
        <option v-for="u in users" :key="u.id" :value="u.id">{{ u.name }}</option>
      </select>

      <select v-model="filterAction" @change="onFilterChange" class="w-44 shrink-0">
        <option value="">{{ t('audit.filterAllActions') }}</option>
        <option value="created">{{ t('audit.actions.created') }}</option>
        <option value="updated">{{ t('audit.actions.updated') }}</option>
        <option value="deleted">{{ t('audit.actions.deleted') }}</option>
        <option value="activated">{{ t('audit.actions.activated') }}</option>
        <option value="deactivated">{{ t('audit.actions.deactivated') }}</option>
        <option value="password_reset">{{ t('audit.actions.password_reset') }}</option>
      </select>

      <button
        v-if="hasFilters"
        @click="clearFilters"
        class="shrink-0 px-3 py-2 text-sm rounded-lg border border-background-300 dark:border-background-700 text-background-500 hover:text-background-700 dark:hover:text-background-300 hover:bg-background-100 dark:hover:bg-background-700 transition-colors"
        title="Limpar filtros"
      >
        <span class="material-symbols-rounded text-base align-middle">close</span>
      </button>

      <!-- Registos por página -->
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
      <div class="border border-background-300 dark:border-background-700 rounded-xl overflow-hidden">
        <div class="grid grid-cols-12 px-4 py-3 bg-background-100 dark:bg-background-800 border-b border-background-300 dark:border-background-700">
          <span class="col-span-1 text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('audit.cols.date') }}</span>
          <span class="col-span-2 text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('audit.cols.user') }}</span>
          <span class="col-span-2 text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('audit.cols.action') }}</span>
          <span class="col-span-7 text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('audit.cols.label') }}</span>
        </div>

        <div v-if="logs.length === 0" class="text-center py-12 text-background-500">
          <span class="material-symbols-rounded text-4xl block mb-2">history</span>
          <p class="text-sm">{{ t('audit.empty') }}</p>
        </div>

        <div
          v-for="log in logs"
          :key="log.id"
          class="grid grid-cols-12 px-4 py-3 border-b border-background-200 dark:border-background-700 last:border-0 bg-background-50 dark:bg-background-800 items-center"
        >
          <!-- Data -->
          <div class="col-span-1">
            <p class="text-sm text-background-700 dark:text-background-300 whitespace-nowrap">{{ displayTime(log.createdAt) }}</p>
            <p class="text-xs text-background-400 whitespace-nowrap">{{ formatDate(log.createdAt) }}</p>
          </div>

          <!-- Utilizador -->
          <div class="col-span-2 flex items-center gap-2 min-w-0">
            <div class="w-6 h-6 rounded-full bg-primary-100 dark:bg-primary-950 flex items-center justify-center flex-shrink-0">
              <span class="text-[10px] font-semibold text-primary-600 dark:text-primary-300">{{ initials(log.userName) }}</span>
            </div>
            <span class="text-sm text-background-700 dark:text-background-300 truncate">{{ log.userName }}</span>
          </div>

          <!-- Ação -->
          <div class="col-span-2">
            <span
              class="text-xs font-medium px-2 py-0.5 rounded-full whitespace-nowrap"
              :class="actionClass(log.action)"
            >
              {{ t(`audit.actions.${log.action}`) }}
            </span>
          </div>

          <!-- Frase por extenso -->
          <div class="col-span-7 min-w-0">
            <span class="text-sm text-background-700 dark:text-background-200 truncate block">{{ sentence(log) }}</span>
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
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useI18n } from 'vue-i18n'
import axios from '@/axios'
import { toast } from '@/plugins/toast'
import { formatDate as formatDateUtc, formatRelative } from '@/utils/dates'

const { t, locale } = useI18n()

// ── Types ─────────────────────────────────────────────────────────────────────
interface AuditLog {
  id:          number
  userId:      number
  userName:    string
  action:      string
  entity:      string
  entityId:    number
  entityLabel: string
  createdAt:   string
}

interface AuditUser {
  id:   number
  name: string
}

// ── Constants ─────────────────────────────────────────────────────────────────
const ENTITY_VALUES = [
  'car_model', 'config', 'config_option', 'phase', 'phase_sequence',
  'production_line', 'workstation', 'support', 'order', 'user',
]

// ── State ─────────────────────────────────────────────────────────────────────
const loading      = ref(false)
const logs         = ref<AuditLog[]>([])
const users        = ref<AuditUser[]>([])
const total        = ref(0)
const currentPage  = ref(1)
const pageSize     = ref(25)

const filterEntity = ref('')
const filterUserId = ref<number | ''>('')
const filterAction = ref('')

// ── Computed ──────────────────────────────────────────────────────────────────
const hasFilters = computed(() =>
  filterEntity.value !== '' || filterUserId.value !== '' || filterAction.value !== '')

const totalPages = computed(() => Math.ceil(total.value / pageSize.value))
const visiblePages = computed(() => {
  const pages: number[] = []
  const start = Math.max(1, currentPage.value - 2)
  const end = Math.min(totalPages.value, currentPage.value + 2)
  for (let i = start; i <= end; i++) pages.push(i)
  return pages
})

// ── Lifecycle ─────────────────────────────────────────────────────────────────
onMounted(async () => {
  await Promise.all([loadLogs(), loadUsers()])
})

// ── Data ──────────────────────────────────────────────────────────────────────
async function loadLogs() {
  loading.value = true
  try {
    const res = await axios.get('/Audit', {
      params: {
        page: currentPage.value,
        pageSize: pageSize.value,
        entity: filterEntity.value || undefined,
        userId: filterUserId.value || undefined,
        action: filterAction.value || undefined,
      },
    })
    logs.value = res.data.data
    total.value = res.data.total
  } catch {
    toast.error(t('errors.loadFailed'))
  } finally {
    loading.value = false
  }
}

// Lista de utilizadores carregada uma vez, independente da paginação —
// senão o dropdown só mostraria quem aparece na página atual.
async function loadUsers() {
  try {
    const res = await axios.get('/Audit/users')
    users.value = res.data
  } catch {
    // silencioso — não bloqueia a página se a lista de utilizadores falhar
  }
}

function onFilterChange() {
  currentPage.value = 1
  loadLogs()
}

function onPageSizeChange() {
  currentPage.value = 1
  loadLogs()
}

function goToPage(page: number) {
  if (page < 1 || page > totalPages.value) return
  currentPage.value = page
  loadLogs()
}

// ── Helpers ───────────────────────────────────────────────────────────────────
function clearFilters() {
  filterEntity.value = ''
  filterUserId.value = ''
  filterAction.value = ''
  currentPage.value = 1
  loadLogs()
}

function formatDate(iso: string) {
  return formatDateUtc(iso)
}

function displayTime(iso: string) {
  const loc = locale.value === 'en' ? 'en-US' : 'pt-PT'
  return formatRelative(iso, loc) ?? formatDateUtc(iso)
}

function initials(name: string) {
  return name.split(' ').slice(0, 2).map(n => n[0]?.toUpperCase() ?? '').join('')
}

function entityLabel(entity: string) {
  return t(`audit.entities.${entity}`)
}

function sentence(log: AuditLog) {
  return t(`audit.sentence.${log.action}`, {
    user:   log.userName,
    entity: entityLabel(log.entity),
    label:  log.entityLabel,
  })
}

function actionClass(action: string) {
  switch (action) {
    case 'created':        return 'bg-success-100 text-success-700 dark:bg-success-950 dark:text-success-300'
    case 'updated':        return 'bg-warning-100 text-warning-700 dark:bg-warning-950 dark:text-warning-300'
    case 'deleted':        return 'bg-danger-100 text-danger-700 dark:bg-danger-950 dark:text-danger-300'
    case 'activated':      return 'bg-teal-100 text-teal-700 dark:bg-teal-950 dark:text-teal-300'
    case 'deactivated':    return 'bg-orange-100 text-orange-700 dark:bg-orange-950 dark:text-orange-300'
    case 'password_reset': return 'bg-violet-100 text-violet-700 dark:bg-violet-950 dark:text-violet-300'
    default:                return 'bg-background-200 text-background-500'
  }
}
</script>