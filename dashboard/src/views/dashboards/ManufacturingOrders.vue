<template>
  <div class="p-8 w-full">

    <!-- Header -->
    <div class="flex items-start justify-between mb-6">
      <div>
        <h1 class="text-2xl font-medium text-background-900 dark:text-background-50">
          {{ t('mo.title') }}
        </h1>
        <p class="text-sm text-background-600 dark:text-background-400 mt-1">
          {{ t('mo.subtitle') }}
        </p>
      </div>
    </div>

    <!-- Filtros de Status -->
    <div class="flex gap-2 mb-6 flex-wrap">
      <button
        v-for="filter in statusFilters"
        :key="filter.value"
        @click="setFilter(filter.value)"
        class="flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-sm font-medium transition-colors border"
        :class="activeFilter === filter.value
          ? 'bg-primary-500 text-white border-primary-500'
          : 'bg-background-50 dark:bg-background-800 text-background-600 dark:text-background-400 border-background-300 dark:border-background-700 hover:border-primary-300'"
      >
        <span class="w-2 h-2 rounded-full" :class="filter.dot"></span>
        {{ filter.label }}
        <span class="text-xs opacity-70">({{ countByStatus(filter.value) }})</span>
      </button>
    </div>

    <!-- Loading -->
    <div v-if="loading" class="flex items-center gap-2 text-background-500 text-sm py-12">
      <span class="material-symbols-rounded animate-spin text-lg">autorenew</span>
      {{ t('common.loading') }}
    </div>

    <!-- Listagem -->
    <div v-else>
      <div v-if="filteredOrders.length === 0" class="text-center py-16 text-background-500">
        <span class="material-symbols-rounded text-5xl block mb-3">assignment_turned_in</span>
        <p class="text-sm">{{ t('mo.empty') }}</p>
      </div>

      <div v-else class="border border-background-300 dark:border-background-700 rounded-xl overflow-hidden">
        <!-- Header -->
        <div class="grid grid-cols-6 px-4 py-3 bg-background-100 dark:bg-background-800 border-b border-background-300 dark:border-background-700">
          <span class="text-xs font-medium text-background-500 uppercase tracking-wider col-span-2">{{ t('mo.fields.number') }}</span>
          <span class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('mo.fields.customer') }}</span>
          <span class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('mo.fields.startDate') }}</span>
          <span class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('mo.fields.status') }}</span>
          <span class="text-xs font-medium text-background-500 uppercase tracking-wider text-right">{{ t('common.actions') }}</span>
        </div>

        <!-- Rows -->
        <div
          v-for="order in filteredOrders"
          :key="order.id"
          class="grid grid-cols-6 px-4 py-3 border-b border-background-200 dark:border-background-700 last:border-0 bg-background-50 dark:bg-background-800 hover:bg-background-100 dark:hover:bg-background-750 transition-colors items-center"
        >
          <div class="col-span-2">
            <div class="text-sm font-medium text-background-900 dark:text-background-50">{{ order.manufacturingOrderNumber }}</div>
            <div class="text-xs text-background-400 mt-0.5">ID #{{ order.id }}</div>
          </div>
          <span class="text-sm text-background-700 dark:text-background-300">{{ order.customerName }}</span>
          <span class="text-sm text-background-500">{{ formatDate(order.startDate) }}</span>
          <div>
            <span
              class="text-xs font-medium px-2 py-1 rounded-full"
              :class="statusClass(order.status)"
            >
              {{ statusLabel(order.status) }}
            </span>
          </div>
          <div class="flex justify-end gap-1">
            <button
              @click="openUpdateStatus(order)"
              class="p-1.5 rounded-lg text-background-400 hover:text-primary-500 hover:bg-primary-50 dark:hover:bg-background-700 transition-colors"
              :title="t('mo.updateStatus')"
            >
              <span class="material-symbols-rounded text-base">edit</span>
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Modal: Atualizar Status -->
    <div v-if="showStatusModal" class="fixed inset-0 bg-black/40 flex items-center justify-center z-50" @click.self="showStatusModal = false">
      <div class="bg-background-50 dark:bg-background-800 border border-background-300 dark:border-background-700 rounded-xl w-full max-w-sm overflow-hidden">
        <div class="flex items-center justify-between px-5 py-4 border-b border-background-300 dark:border-background-700">
          <h2 class="text-base font-medium text-background-900 dark:text-background-50">{{ t('mo.updateStatus') }}</h2>
          <button @click="showStatusModal = false" class="text-background-500 hover:text-background-700">
            <span class="material-symbols-rounded">close</span>
          </button>
        </div>
        <div class="px-5 py-4 flex flex-col gap-4">
          <div class="text-sm text-background-600 dark:text-background-400">
            <span class="font-medium text-background-900 dark:text-background-50">{{ selectedOrder?.manufacturingOrderNumber }}</span>
          </div>
          <div class="flex flex-col gap-1.5">
            <label class="text-xs font-medium text-background-600 dark:text-background-400">{{ t('mo.fields.status') }}</label>
            <select v-model="newStatus">
              <option value="pending">{{ t('mo.status.pending') }}</option>
              <option value="in_progress">{{ t('mo.status.inProgress') }}</option>
              <option value="completed">{{ t('mo.status.completed') }}</option>
              <option value="cancelled">{{ t('mo.status.cancelled') }}</option>
            </select>
          </div>
        </div>
        <div class="flex justify-end gap-2 px-5 py-4 border-t border-background-300 dark:border-background-700">
          <button @click="showStatusModal = false" class="px-4 py-2 text-sm rounded-lg border border-background-300 dark:border-background-600 text-background-700 dark:text-background-300 hover:bg-background-100 dark:hover:bg-background-700 transition-colors">
            {{ t('common.cancel') }}
          </button>
          <button @click="submitUpdateStatus" class="px-4 py-2 text-sm rounded-lg bg-primary-500 hover:bg-primary-600 text-white font-medium transition-colors">
            {{ t('common.save') }}
          </button>
        </div>
      </div>
    </div>

  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { manufacturingOrderService } from '@/services/manufacturingOrderService'
import type { ManufacturingOrder } from '@/services/manufacturingOrderService'
import { toast } from '@/plugins/toast'
import { useI18n } from 'vue-i18n'

const { t } = useI18n()

const loading = ref(true)
const orders = ref<ManufacturingOrder[]>([])
const activeFilter = ref('all')
const showStatusModal = ref(false)
const selectedOrder = ref<ManufacturingOrder | null>(null)
const newStatus = ref('pending')

const statusFilters = computed(() => [
  { value: 'all', label: t('mo.status.all'), dot: 'bg-background-400' },
  { value: 'pending', label: t('mo.status.pending'), dot: 'bg-warning-500' },
  { value: 'in_progress', label: t('mo.status.inProgress'), dot: 'bg-primary-500' },
  { value: 'completed', label: t('mo.status.completed'), dot: 'bg-success-500' },
  { value: 'cancelled', label: t('mo.status.cancelled'), dot: 'bg-danger-500' },
])

const filteredOrders = computed(() => {
  if (activeFilter.value === 'all') return orders.value
  return orders.value.filter(o => o.status === activeFilter.value)
})

function countByStatus(status: string) {
  if (status === 'all') return orders.value.length
  return orders.value.filter(o => o.status === status).length
}

onMounted(async () => {
  await loadOrders()
})

async function loadOrders() {
  loading.value = true
  try {
    const res = await manufacturingOrderService.getAll()
    orders.value = res.data
  } catch {
    toast.error(t('errors.loadFailed'))
  } finally {
    loading.value = false
  }
}

function setFilter(status: string) {
  activeFilter.value = status
}

function openUpdateStatus(order: ManufacturingOrder) {
  selectedOrder.value = order
  newStatus.value = order.status ?? 'pending'
  showStatusModal.value = true
}

async function submitUpdateStatus() {
  if (!selectedOrder.value) return
  try {
    await manufacturingOrderService.update(selectedOrder.value.id, { status: newStatus.value })
    toast.success(t('mo.statusUpdated'))
    showStatusModal.value = false
    await loadOrders()
  } catch {
    toast.error(t('errors.saveFailed'))
  }
}

function formatDate(dateStr: string) {
  return new Date(dateStr).toLocaleDateString('pt-PT')
}

function statusClass(status: string | null) {
  switch (status) {
    case 'pending': return 'bg-warning-100 text-warning-700'
    case 'in_progress': return 'bg-primary-50 text-primary-600'
    case 'completed': return 'bg-success-100 text-success-700'
    case 'cancelled': return 'bg-danger-100 text-danger-700'
    default: return 'bg-background-200 text-background-600'
  }
}

function statusLabel(status: string | null) {
  switch (status) {
    case 'pending': return t('mo.status.pending')
    case 'in_progress': return t('mo.status.inProgress')
    case 'completed': return t('mo.status.completed')
    case 'cancelled': return t('mo.status.cancelled')
    default: return t('mo.status.unknown')
  }
}
</script>