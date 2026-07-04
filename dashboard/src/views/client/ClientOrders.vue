<template>
  <div class="px-8 py-8">
    <div class="flex items-start justify-between flex-wrap gap-3 mb-6">
      <div>
        <h1 class="text-2xl font-semibold text-background-900 dark:text-background-50">
          {{ t('client.orders.title') }}
        </h1>
        <p class="text-sm text-background-500 dark:text-background-400 mt-1">
          {{ t('client.orders.subtitle') }}
        </p>
      </div>
      <button
        :disabled="filteredOrders.length === 0"
        class="flex items-center gap-1.5 text-sm font-medium px-4 py-2 rounded-xl border border-background-200 dark:border-background-700 text-background-600 dark:text-background-300 hover:bg-background-100 dark:hover:bg-background-800 transition-colors disabled:opacity-40 disabled:cursor-not-allowed"
        @click="exportCsv"
      >
        <span class="material-symbols-rounded text-base">download</span>
        {{ t('client.orders.export') }}
      </button>
    </div>

    <!-- Filtros -->
    <div class="grid grid-cols-1 sm:grid-cols-3 gap-3 mb-6">
      <select
        v-model="statusFilter"
        class="text-sm rounded-xl border border-background-200 dark:border-background-700 bg-background-50 dark:bg-background-900 px-3 py-2.5 text-background-700 dark:text-background-300"
      >
        <option value="">{{ t('client.orders.filters.allStatuses') }}</option>
        <option v-for="s in STATUS_OPTIONS" :key="s.value" :value="s.value">{{ s.label }}</option>
      </select>

      <select
        v-model="dateRangeFilter"
        class="text-sm rounded-xl border border-background-200 dark:border-background-700 bg-background-50 dark:bg-background-900 px-3 py-2.5 text-background-700 dark:text-background-300"
      >
        <option value="all">{{ t('client.orders.filters.allDates') }}</option>
        <option value="30d">{{ t('client.orders.filters.last30d') }}</option>
        <option value="6m">{{ t('client.orders.filters.last6m') }}</option>
        <option value="year">{{ t('client.orders.filters.thisYear') }}</option>
      </select>

      <!--
        Agora com dados reais: ModelName vem do backend (extensão feita nesta
        ronda — ver ClientPortalRepository.cs). Uma encomenda pode ter mais do
        que um modelo distinto; nesse caso o valor vem como "A / B" e não
        aparece isolado nesta lista — filtra por modelos que aparecem sozinhos
        ou dentro de combinações.
      -->
      <select
        v-model="modelFilter"
        class="text-sm rounded-xl border border-background-200 dark:border-background-700 bg-background-50 dark:bg-background-900 px-3 py-2.5 text-background-700 dark:text-background-300"
      >
        <option value="">{{ t('client.orders.filters.allModels') }}</option>
        <option v-for="m in distinctModels" :key="m" :value="m">{{ m }}</option>
      </select>
    </div>

    <!-- Loading -->
    <div v-if="loading" class="flex items-center justify-center py-24">
      <div class="animate-spin rounded-full h-9 w-9 border-2 border-primary-200 border-t-primary-500" />
    </div>

    <!-- Empty -->
    <div v-else-if="orders.length === 0" class="flex flex-col items-center justify-center text-center text-background-400 dark:text-background-500 py-24">
      <span class="material-symbols-rounded text-6xl mb-4 text-background-300 dark:text-background-700">directions_car</span>
      <p class="text-base">{{ t('client.orders.empty') }}</p>
    </div>

    <template v-else>
      <div class="bg-background-50 dark:bg-background-900 border border-background-200 dark:border-background-800 rounded-2xl overflow-hidden">
        <table class="w-full text-sm">
          <thead>
            <tr class="text-left text-[11px] uppercase tracking-wider text-background-400 dark:text-background-500 border-b border-background-200 dark:border-background-800">
              <th class="px-5 py-3 font-medium">{{ t('client.orders.table.orderId') }}</th>
              <th class="px-5 py-3 font-medium">{{ t('client.orders.table.model') }}</th>
              <th class="px-5 py-3 font-medium">{{ t('client.orders.table.units') }}</th>
              <th class="px-5 py-3 font-medium">{{ t('client.orders.table.orderDate') }}</th>
              <th class="px-5 py-3 font-medium">{{ t('client.orders.table.status') }}</th>
              <th class="px-5 py-3 font-medium">{{ t('client.orders.table.progress') }}</th>
              <th class="px-5 py-3 font-medium">{{ t('client.orders.table.eta') }}</th>
              <th class="px-5 py-3 font-medium text-right">{{ t('client.orders.table.action') }}</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="order in pagedOrders"
              :key="order.id"
              class="border-b border-background-100 dark:border-background-800 last:border-0 hover:bg-background-100 dark:hover:bg-background-800/60 transition-colors"
            >
              <td class="px-5 py-3 font-medium text-primary-600 dark:text-primary-400">{{ order.orderNumber }}</td>
              <td class="px-5 py-3 text-background-600 dark:text-background-300">{{ order.modelName || '—' }}</td>
              <td class="px-5 py-3 text-background-600 dark:text-background-300">{{ order.totalCars }}</td>
              <td class="px-5 py-3 text-background-600 dark:text-background-300">{{ formatDate(order.orderDate) }}</td>
              <td class="px-5 py-3">
                <span class="text-xs font-medium px-2.5 py-1 rounded-full" :class="statusBadgeClass(order.status)">
                  {{ statusLabel(order.status) }}
                </span>
              </td>
              <td class="px-5 py-3 w-40">
                <div class="flex items-center gap-2">
                  <div class="flex-1 bg-background-200 dark:bg-background-800 rounded-full h-1.5 overflow-hidden">
                    <div
                      class="h-full rounded-full"
                      :class="isDone(order) ? 'bg-success-500' : 'bg-primary-500'"
                      :style="{ width: `${progressPct(order)}%` }"
                    />
                  </div>
                  <span class="text-xs tabular-nums text-background-500 dark:text-background-400">{{ progressPct(order) }}%</span>
                </div>
              </td>
              <td class="px-5 py-3 text-background-600 dark:text-background-300">
                {{ order.etaUtc ? relativeEtaLabel(order.etaUtc, t) : '—' }}
              </td>
              <td class="px-5 py-3 text-right">
                <RouterLink :to="`/client/orders/${order.id}`" class="text-primary-500 hover:text-primary-600 dark:hover:text-primary-400 font-medium">
                  {{ t('client.orders.table.details') }}
                </RouterLink>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Paginação -->
      <div v-if="filteredOrders.length > 0" class="flex items-center justify-between mt-4 text-sm text-background-500 dark:text-background-400">
        <span>
          {{ t('client.orders.pagination.showing', { from: rangeFrom, to: rangeTo, total: filteredOrders.length }) }}
        </span>
        <div class="flex items-center gap-1">
          <button
            class="h-8 w-8 flex items-center justify-center rounded-lg border border-background-200 dark:border-background-700 disabled:opacity-40 disabled:cursor-not-allowed hover:bg-background-100 dark:hover:bg-background-800"
            :disabled="page === 1"
            @click="page--"
          >
            <span class="material-symbols-rounded text-base">chevron_left</span>
          </button>
          <span class="px-2 tabular-nums">{{ page }} / {{ totalPages }}</span>
          <button
            class="h-8 w-8 flex items-center justify-center rounded-lg border border-background-200 dark:border-background-700 disabled:opacity-40 disabled:cursor-not-allowed hover:bg-background-100 dark:hover:bg-background-800"
            :disabled="page === totalPages"
            @click="page++"
          >
            <span class="material-symbols-rounded text-base">chevron_right</span>
          </button>
        </div>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import 'material-symbols'
import { clientPortalService, type ClientOrderSummary } from '@/services/clientPortalService'
import { relativeEtaLabel } from '@/utils/clientEta'

const { t } = useI18n()

const orders = ref<ClientOrderSummary[]>([])
const loading = ref(true)
const page = ref(1)
const pageSize = 8

const statusFilter = ref('')
const dateRangeFilter = ref<'all' | '30d' | '6m' | 'year'>('all')
const modelFilter = ref('')

const STATUS_OPTIONS = [
  { value: 'Pending', label: 'Pendente' },
  { value: 'InProgress', label: 'Em Produção' },
  { value: 'Completed', label: 'Concluído' },
  { value: 'Cancelled', label: 'Cancelado' },
]

onMounted(async () => {
  try {
    orders.value = await clientPortalService.getMyOrders()
  } finally {
    loading.value = false
  }
})

function isDone(order: ClientOrderSummary) {
  return order.totalCars > 0 && order.completedCars === order.totalCars
}

function progressPct(order: ClientOrderSummary) {
  if (order.totalCars === 0) return 0
  return Math.round((order.completedCars / order.totalCars) * 100)
}

function statusLabel(status: string) {
  return STATUS_OPTIONS.find(s => s.value === status)?.label ?? status
}

function statusBadgeClass(status: string) {
  switch (status) {
    case 'Completed': return 'bg-success-100 text-success-700'
    case 'Cancelled': return 'bg-danger-100 text-danger-700'
    case 'InProgress': return 'bg-primary-100 text-primary-700'
    default: return 'bg-warning-100 text-warning-700'
  }
}

const filteredOrders = computed(() => {
  const now = Date.now()
  return orders.value.filter(o => {
    if (statusFilter.value && o.status !== statusFilter.value) return false
    if (modelFilter.value && !o.modelName?.split(' / ').includes(modelFilter.value)) return false

    if (dateRangeFilter.value !== 'all') {
      const orderMs = new Date(o.orderDate).getTime()
      const diffDays = (now - orderMs) / 86_400_000
      if (dateRangeFilter.value === '30d' && diffDays > 30) return false
      if (dateRangeFilter.value === '6m' && diffDays > 182) return false
      if (dateRangeFilter.value === 'year' && diffDays > 365) return false
    }

    return true
  })
})

watch([statusFilter, dateRangeFilter, modelFilter], () => { page.value = 1 })

const distinctModels = computed(() => {
  const all = orders.value.flatMap(o => o.modelName ? o.modelName.split(' / ') : [])
  return [...new Set(all)].sort()
})

const totalPages = computed(() => Math.max(1, Math.ceil(filteredOrders.value.length / pageSize)))
const pagedOrders = computed(() => filteredOrders.value.slice((page.value - 1) * pageSize, page.value * pageSize))
const rangeFrom = computed(() => filteredOrders.value.length === 0 ? 0 : (page.value - 1) * pageSize + 1)
const rangeTo = computed(() => Math.min(page.value * pageSize, filteredOrders.value.length))

function formatDate(iso: string) {
  return new Date(iso).toLocaleDateString('pt-PT', { day: '2-digit', month: 'short', year: 'numeric' })
}

// Exportação real, feita no cliente a partir dos dados já carregados —
// não depende de nenhum endpoint novo no backend.
function exportCsv() {
  const header = ['Encomenda', 'Modelo', 'Unidades', 'Data do Pedido', 'Estado', 'Progresso (%)', 'ETA']
  const rows = filteredOrders.value.map(o => [
    o.orderNumber,
    o.modelName || '',
    String(o.totalCars),
    formatDate(o.orderDate),
    statusLabel(o.status),
    String(progressPct(o)),
    o.etaUtc ? new Date(o.etaUtc).toISOString() : '',
  ])
  const csv = [header, ...rows].map(r => r.map(v => `"${v.replace(/"/g, '""')}"`).join(';')).join('\n')
  const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' })
  const url = URL.createObjectURL(blob)
  const link = document.createElement('a')
  link.href = url
  link.download = 'encomendas.csv'
  link.click()
  URL.revokeObjectURL(url)
}
</script>
