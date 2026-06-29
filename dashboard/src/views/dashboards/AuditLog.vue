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
    <div class="flex flex-wrap gap-3 mb-6">
      <select v-model="filterEntity" class="text-sm min-w-[160px]">
        <option value="">{{ t('audit.filterAllEntities') }}</option>
        <option v-for="e in ENTITY_VALUES" :key="e" :value="e">{{ entityLabel(e) }}</option>
      </select>

      <select v-model="filterUserId" class="text-sm min-w-[160px]">
        <option value="">{{ t('audit.filterAllUsers') }}</option>
        <option v-for="u in uniqueUsers" :key="u.id" :value="u.id">{{ u.name }}</option>
      </select>

      <select v-model="filterAction" class="text-sm min-w-[140px]">
        <option value="">{{ t('audit.filterAllActions') }}</option>
        <option value="created">{{ t('audit.actions.created') }}</option>
        <option value="updated">{{ t('audit.actions.updated') }}</option>
        <option value="deleted">{{ t('audit.actions.deleted') }}</option>
      </select>

      <button
        v-if="hasFilters"
        @click="clearFilters"
        class="text-sm text-background-500 hover:text-background-700 dark:hover:text-background-300 underline transition-colors"
      >
        {{ t('audit.clearFilters') }}
      </button>
    </div>

    <!-- Loading -->
    <div v-if="loading" class="flex items-center gap-2 text-background-500 text-sm py-12">
      <span class="material-symbols-rounded animate-spin text-lg">autorenew</span>
      {{ t('common.loading') }}
    </div>

    <!-- Tabela -->
    <div v-else class="border border-background-300 dark:border-background-700 rounded-xl overflow-hidden">
      <div class="grid grid-cols-12 px-4 py-3 bg-background-100 dark:bg-background-800 border-b border-background-300 dark:border-background-700">
        <span class="col-span-2 text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('audit.cols.date') }}</span>
        <span class="col-span-2 text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('audit.cols.user') }}</span>
        <span class="col-span-1 text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('audit.cols.action') }}</span>
        <span class="col-span-7 text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('audit.cols.label') }}</span>
      </div>

      <div v-if="filtered.length === 0" class="text-center py-12 text-background-500">
        <span class="material-symbols-rounded text-4xl block mb-2">history</span>
        <p class="text-sm">{{ t('audit.empty') }}</p>
      </div>

      <div
        v-for="log in filtered"
        :key="log.id"
        class="grid grid-cols-12 px-4 py-3 border-b border-background-200 dark:border-background-700 last:border-0 bg-background-50 dark:bg-background-800 items-center"
      >
        <!-- Data -->
        <div class="col-span-2">
          <p class="text-sm text-background-700 dark:text-background-300">{{ displayTime(log.createdAt) }}</p>
          <p class="text-xs text-background-400">{{ formatDate(log.createdAt) }}</p>
        </div>

        <!-- Utilizador -->
        <div class="col-span-2 flex items-center gap-2 min-w-0">
          <div class="w-6 h-6 rounded-full bg-primary-100 dark:bg-primary-950 flex items-center justify-center flex-shrink-0">
            <span class="text-[10px] font-semibold text-primary-600 dark:text-primary-300">{{ initials(log.userName) }}</span>
          </div>
          <span class="text-sm text-background-700 dark:text-background-300 truncate">{{ log.userName }}</span>
        </div>

        <!-- Ação -->
        <div class="col-span-1">
          <span
            class="text-xs font-medium px-2 py-0.5 rounded-full whitespace-nowrap"
            :class="actionClass(log.action)"
          >
            {{ t(`audit.actions.${log.action}`) }}
          </span>
        </div>

        <!-- Frase por extenso (ação + entidade + objeto) -->
        <div class="col-span-7 min-w-0">
          <span class="text-sm text-background-700 dark:text-background-200 truncate block">{{ sentence(log) }}</span>
        </div>
      </div>
    </div>

    <!-- Contagem -->
    <p v-if="!loading && filtered.length > 0" class="text-xs text-background-400 mt-3 text-right">
      {{ t('audit.count', { n: filtered.length, total: logs.length }) }}
    </p>
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

// ── Constants ─────────────────────────────────────────────────────────────────
const ENTITY_VALUES = [
  'car_model', 'config', 'config_option', 'phase', 'phase_sequence',
  'production_line', 'workstation', 'support', 'order', 'user',
]

// ── State ─────────────────────────────────────────────────────────────────────
const loading      = ref(false)
const logs         = ref<AuditLog[]>([])
const filterEntity = ref('')
const filterUserId = ref<number | ''>('')
const filterAction = ref('')

// ── Computed ──────────────────────────────────────────────────────────────────
const uniqueUsers = computed(() => {
  const seen = new Map<number, string>()
  for (const l of logs.value) seen.set(l.userId, l.userName)
  return Array.from(seen.entries()).map(([id, name]) => ({ id, name }))
})

const hasFilters = computed(() =>
  filterEntity.value !== '' || filterUserId.value !== '' || filterAction.value !== '')

const filtered = computed(() =>
  logs.value.filter(l => {
    if (filterEntity.value && l.entity  !== filterEntity.value) return false
    if (filterUserId.value && l.userId  !== filterUserId.value) return false
    if (filterAction.value && l.action  !== filterAction.value) return false
    return true
  })
)

// ── Lifecycle ─────────────────────────────────────────────────────────────────
onMounted(loadLogs)

// ── Data ──────────────────────────────────────────────────────────────────────
async function loadLogs() {
  loading.value = true
  try {
    const res = await axios.get('/Audit')
    logs.value = res.data?.$values ?? res.data ?? []
  } catch {
    toast.error(t('errors.loadFailed'))
  } finally {
    loading.value = false
  }
}

// ── Helpers ───────────────────────────────────────────────────────────────────
function clearFilters() {
  filterEntity.value = ''
  filterUserId.value = ''
  filterAction.value = ''
}

// utils/dates.ts já trata a normalização UTC (string sem "Z" tratada como UTC).
function formatDate(iso: string) {
  return formatDateUtc(iso)
}

// Tempo relativo (estilo "há 5 min"); cai para a hora absoluta acima de 7 dias.
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
    case 'created': return 'bg-success-100 text-success-700 dark:bg-success-950 dark:text-success-300'
    case 'updated': return 'bg-warning-100 text-warning-700 dark:bg-warning-950 dark:text-warning-300'
    case 'deleted': return 'bg-danger-100 text-danger-700 dark:bg-danger-950 dark:text-danger-300'
    default:        return 'bg-background-200 text-background-500'
  }
}
</script>