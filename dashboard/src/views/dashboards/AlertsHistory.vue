<template>
  <div class="p-8 w-full">

    <!-- Header -->
    <div class="mb-6">
      <h1 class="text-2xl font-medium text-background-900 dark:text-background-50">
        {{ t('alertsHistory.title') }}
      </h1>
      <p class="text-sm text-background-600 dark:text-background-400 mt-1">
        {{ t('alertsHistory.subtitle') }}
      </p>
    </div>

    <!-- Filtros -->
    <div class="flex gap-4 mb-6 items-end">
      <div class="flex flex-col gap-1.5">
        <label class="text-xs font-medium text-background-600 dark:text-background-400">{{ t('alertsHistory.filterType') }}</label>
        <select v-model="filterType" @change="onFilterChange" class="w-44">
          <option value="">{{ t('alertsHistory.filterAll') }}</option>
          <option value="time_exceeded">{{ t('alerts.timeExceeded') }}</option>
          <option value="wrong_sequence">{{ t('alerts.wrongSequence') }}</option>
        </select>
      </div>
      <div class="flex flex-col gap-1.5">
        <label class="text-xs font-medium text-background-600 dark:text-background-400">{{ t('alertsHistory.filterStatus') }}</label>
        <select v-model="filterStatus" @change="onFilterChange" class="w-44">
          <option value="">{{ t('alertsHistory.filterAll') }}</option>
          <option value="open">{{ t('alertsHistory.status.open') }}</option>
          <option value="acknowledged">{{ t('alertsHistory.status.acknowledged') }}</option>
          <option value="resolved">{{ t('alertsHistory.status.resolved') }}</option>
        </select>
      </div>

      <!-- Registos por página -->
      <select v-model="pageSize" @change="onPageSizeChange" class="ml-auto w-20">
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

    <!-- Listagem -->
    <div v-else>
      <div v-if="alerts.length === 0" class="text-center py-16 text-background-500">
        <span class="material-symbols-rounded text-5xl block mb-3">notifications_off</span>
        <p class="text-sm">{{ t('alertsHistory.empty') }}</p>
      </div>

      <div v-else class="border border-background-300 dark:border-background-700 rounded-xl overflow-hidden">
        <!-- Table Header -->
        <div class="grid grid-cols-6 px-4 py-3 bg-background-100 dark:bg-background-800 border-b border-background-300 dark:border-background-700">
          <span class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('alertsHistory.fields.type') }}</span>
          <span class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('alertsHistory.fields.product') }}</span>
          <span class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('alertsHistory.fields.phase') }}</span>
          <span class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('alertsHistory.fields.triggeredAt') }}</span>
          <span class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('alertsHistory.fields.status') }}</span>
          <span class="text-xs font-medium text-background-500 uppercase tracking-wider text-right">{{ t('common.actions') }}</span>
        </div>

        <!-- Table Rows -->
        <div
          v-for="alert in alerts"
          :key="alert.id"
          class="grid grid-cols-6 px-4 py-3 border-b border-background-200 dark:border-background-700 last:border-0 bg-background-50 dark:bg-background-800 hover:bg-background-100 dark:hover:bg-background-750 transition-colors items-center"
        >
          <div class="flex items-center gap-1.5">
            <span class="material-symbols-rounded text-base" :class="alert.type === 'time_exceeded' ? 'text-red-500' : 'text-orange-500'">
              {{ alert.type === 'time_exceeded' ? 'schedule' : 'alt_route' }}
            </span>
            <span class="text-sm text-background-700 dark:text-background-300">
              {{ alert.type === 'time_exceeded' ? t('alerts.timeExceeded') : t('alerts.wrongSequence') }}
            </span>
          </div>
          <span class="text-sm font-medium text-background-900 dark:text-background-50">{{ alert.productSerial }}</span>
          <span class="text-sm text-background-600 dark:text-background-400">{{ alert.phaseName }}</span>
          <span class="text-sm text-background-500">{{ formatDate(alert.triggeredAt) }}</span>
          <span>
            <span
              class="text-xs font-medium px-2 py-1 rounded-full"
              :class="statusClass(alert.status)"
            >
              {{ t('alertsHistory.status.' + alert.status) }}
            </span>
          </span>
          <div class="flex justify-end">
            <button
              v-if="alert.status === 'open'"
              @click="acknowledge(alert.id)"
              class="text-xs px-3 py-1.5 rounded-lg border border-background-300 dark:border-background-600 text-background-700 dark:text-background-300 hover:bg-background-100 dark:hover:bg-background-700 transition-colors"
            >
              {{ t('alertsHistory.acknowledgeBtn') }}
            </button>
            <span v-else class="text-xs text-background-400">—</span>
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
import { alertService } from '@/services/alertService'
import type { Alert } from '@/services/alertService'
import { toast } from '@/plugins/toast'

const { t } = useI18n()

const loading = ref(false)
const alerts = ref<Alert[]>([])
const total = ref(0)
const currentPage = ref(1)
const pageSize = ref(25)

const filterType = ref('')
const filterStatus = ref('')

const totalPages = computed(() => Math.ceil(total.value / pageSize.value))
const visiblePages = computed(() => {
  const pages: number[] = []
  const start = Math.max(1, currentPage.value - 2)
  const end = Math.min(totalPages.value, currentPage.value + 2)
  for (let i = start; i <= end; i++) pages.push(i)
  return pages
})

async function loadAlerts() {
  loading.value = true
  try {
    const res = await alertService.getPaged({
      page: currentPage.value,
      pageSize: pageSize.value,
      type: filterType.value || undefined,
      status: filterStatus.value || undefined,
    })
    alerts.value = res.data.data
    total.value = res.data.total
  } catch {
    toast.error(t('errors.loadFailed'))
  } finally {
    loading.value = false
  }
}

function onFilterChange() {
  currentPage.value = 1
  loadAlerts()
}

function onPageSizeChange() {
  currentPage.value = 1
  loadAlerts()
}

function goToPage(page: number) {
  if (page < 1 || page > totalPages.value) return
  currentPage.value = page
  loadAlerts()
}

async function acknowledge(id: number) {
  try {
    await alertService.acknowledge(id)
    toast.success(t('alertsHistory.acknowledged'))
    await loadAlerts()
  } catch {
    toast.error(t('errors.saveFailed'))
  }
}

function statusClass(status: string) {
  if (status === 'open') return 'bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-400'
  if (status === 'acknowledged') return 'bg-yellow-100 text-yellow-700 dark:bg-yellow-900/30 dark:text-yellow-400'
  return 'bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-400'
}

function formatDate(dateStr: string) {
  return new Date(dateStr).toLocaleString('pt-PT')
}

onMounted(() => loadAlerts())
</script>