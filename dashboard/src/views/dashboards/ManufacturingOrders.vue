<template>
  <div class="p-8 w-full">
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

    <div class="flex gap-3 mb-6 items-center">
      <div class="relative w-64 shrink-0">
        <span class="material-symbols-rounded absolute left-3 top-1/2 -translate-y-1/2 text-background-400 text-base pointer-events-none">search</span>
        <input
          v-model="search"
          type="text"
          :placeholder="t('mo.searchPlaceholder')"
          class="w-full pl-9 pr-4 py-2 text-sm rounded-lg border border-background-300 dark:border-background-700 bg-background-50 dark:bg-background-800 text-background-900 dark:text-background-50 placeholder-background-400 focus:outline-none focus:border-primary-400"
        />
      </div>

      <select
        v-model="activeStatus"
        @change="onStatusChange"
        class="w-36 shrink-0 px-3 py-2 text-sm rounded-lg border border-background-300 dark:border-background-700 bg-background-50 dark:bg-background-800 text-background-700 dark:text-background-300 focus:outline-none focus:border-primary-400"
      >
        <option value="all">{{ t('mo.status.all') }}</option>
        <option :value="EntityStatus.Pending">{{ t('mo.status.pending') }}</option>
        <option :value="EntityStatus.InProgress">{{ t('mo.status.inProgress') }}</option>
        <option :value="EntityStatus.Completed">{{ t('mo.status.completed') }}</option>
        <option :value="EntityStatus.Cancelled">{{ t('mo.status.cancelled') }}</option>
      </select>

      <input
        v-model="dateFrom"
        type="date"
        class="w-36 shrink-0 px-3 py-2 text-sm rounded-lg border border-background-300 dark:border-background-700 bg-background-50 dark:bg-background-800 text-background-700 dark:text-background-300 focus:outline-none focus:border-primary-400"
      />

      <input
        v-model="dateTo"
        type="date"
        class="w-36 shrink-0 px-3 py-2 text-sm rounded-lg border border-background-300 dark:border-background-700 bg-background-50 dark:bg-background-800 text-background-700 dark:text-background-300 focus:outline-none focus:border-primary-400"
      />

      <select
        v-model="pageSize"
        @change="onPageSizeChange"
        class="ml-auto w-20 shrink-0 px-3 py-2 text-sm rounded-lg border border-background-300 dark:border-background-700 bg-background-50 dark:bg-background-800 text-background-700 dark:text-background-300 focus:outline-none focus:border-primary-400"
      >
        <option :value="25">25</option>
        <option :value="50">50</option>
        <option :value="100">100</option>
      </select>

      <button
        v-if="search || dateFrom || dateTo || activeStatus !== 'all'"
        @click="clearFilters"
        class="shrink-0 px-3 py-2 text-sm rounded-lg border border-background-300 dark:border-background-700 text-background-500 hover:text-background-700 dark:hover:text-background-300 hover:bg-background-100 dark:hover:bg-background-700 transition-colors"
        title="Limpar filtros"
      >
        <span class="material-symbols-rounded text-base align-middle">close</span>
      </button>
    </div>

    <div v-if="loading" class="flex items-center gap-2 text-background-500 text-sm py-12">
      <span class="material-symbols-rounded animate-spin text-lg">autorenew</span>
      {{ t('common.loading') }}
    </div>

    <div v-else>
      <div v-if="orders.length === 0" class="text-center py-16 text-background-500">
        <span class="material-symbols-rounded text-5xl block mb-3">assignment_turned_in</span>
        <p class="text-sm">{{ t('mo.empty') }}</p>
      </div>

      <div v-else class="border border-background-300 dark:border-background-700 rounded-xl overflow-hidden bg-background-50 dark:bg-background-800">
        <div class="grid grid-cols-6 px-4 py-3 bg-background-100 dark:bg-background-800 border-b border-background-300 dark:border-background-700">
          <span class="text-xs font-medium text-background-500 uppercase tracking-wider col-span-2">{{ t('mo.fields.number') }}</span>
          <span class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('mo.fields.customer') }}</span>
          <span class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('mo.fields.startDate') }}</span>
          <span class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('mo.fields.status') }}</span>
          <span class="text-xs font-medium text-background-500 uppercase tracking-wider text-right">{{ t('common.actions') }}</span>
        </div>

        <template v-for="order in orders" :key="order.id">
          <div
            class="grid grid-cols-6 px-4 py-3 border-b border-background-200 dark:border-background-700 bg-background-50 dark:bg-background-800 hover:bg-background-100 dark:hover:bg-background-700 transition-colors items-center cursor-pointer"
            @click="toggleOrder(order)"
          >
            <div class="col-span-2 flex items-center gap-2">
              <span
                class="material-symbols-rounded text-background-400 text-base transition-transform duration-200"
                :class="expandedOrderId === order.id ? 'rotate-180' : ''"
              >expand_more</span>
              <div>
                <div class="text-sm font-medium text-background-900 dark:text-background-50">
                  {{ order.manufacturingOrderNumber }}
                </div>
                <div class="text-xs text-background-400 mt-0.5">ID #{{ order.id }}</div>
              </div>
            </div>

            <span class="text-sm text-background-700 dark:text-background-300">{{ order.clientName }}</span>
            <span class="text-sm text-background-500">{{ formatDate(order.startDate) }}</span>

            <div>
              <span class="text-xs font-medium px-2 py-1 rounded-full" :class="statusClass(order.status)">
                {{ statusLabel(order.status) }}
              </span>
            </div>

            <div class="flex justify-end gap-1" @click.stop>
              <button
                v-if="order.status !== EntityStatus.Completed"
                @click="openUpdateStatus(order)"
                class="p-1.5 rounded-lg text-background-400 hover:text-primary-500 hover:bg-primary-50 dark:hover:bg-background-700 transition-colors"
                :title="t('mo.updateStatus')"
              >
                <span class="material-symbols-rounded text-base">edit</span>
              </button>
            </div>
          </div>

          <div
            v-if="expandedOrderId === order.id"
            class="border-b border-background-200 dark:border-background-700 bg-background-100 dark:bg-background-900 px-8 py-4"
          >
            <div v-if="loadingDetails[order.id]" class="flex items-center gap-2 text-background-400 text-xs py-2">
              <span class="material-symbols-rounded animate-spin text-sm">autorenew</span>
              {{ t('common.loading') }}
            </div>

            <div v-else class="flex flex-col gap-3">
              <div
                v-for="product in normalizeList(detailsByOrder[order.id]?.products)"
                :key="product.id"
                class="bg-background-50 dark:bg-background-800 border border-background-300 dark:border-background-700 rounded-lg px-4 py-3"
              >
                <div class="flex items-center gap-2 mb-2">
                  <span class="material-symbols-rounded text-primary-500 text-base">directions_car</span>
                  <button
                    @click.stop="goToProductTimeline(product.serialNumber)"
                    class="text-sm font-medium text-primary-600 dark:text-primary-400 hover:underline hover:text-primary-700 dark:hover:text-primary-300 transition-colors"
                    :title="t('mo.goToTimeline')"
                  >
                    {{ product.serialNumber }}
                  </button>
                  <span class="text-xs text-background-400">{{ product.modelName }}</span>
                </div>

                <div class="flex flex-wrap gap-2">
                  <span
                    v-for="config in normalizeList(product.configs)"
                    :key="config.configOptionId"
                    class="text-xs px-2 py-1 rounded-full bg-background-200 dark:bg-background-700 border border-background-300 dark:border-background-600 text-background-600 dark:text-background-400"
                  >
                    <span class="text-background-400 dark:text-background-500">{{ config.configItem }}:</span>
                    {{ config.optionValue }}
                  </span>
                </div>
              </div>
            </div>
          </div>
        </template>
      </div>

      <div v-if="totalPages > 1" class="flex items-center justify-between mt-4 text-sm text-background-600 dark:text-background-400">
        <span>
          {{ t('common.showing', { from: (currentPage - 1) * pageSize + 1, to: Math.min(currentPage * pageSize, total), total }) }}
        </span>

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
            :class="p === currentPage
              ? 'bg-primary-500 text-white border-primary-500'
              : 'border-background-300 dark:border-background-700 hover:bg-background-100 dark:hover:bg-background-700'"
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
            <span class="font-medium text-background-900 dark:text-background-50">
              {{ selectedOrder?.manufacturingOrderNumber }}
            </span>
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
          <button
            @click="showStatusModal = false"
            class="px-4 py-2 text-sm rounded-lg border border-background-300 dark:border-background-600 text-background-700 dark:text-background-300 hover:bg-background-100 dark:hover:bg-background-700 transition-colors"
          >
            {{ t('common.cancel') }}
          </button>

          <button
            @click="submitUpdateStatus"
            class="px-4 py-2 text-sm rounded-lg bg-primary-500 hover:bg-primary-600 text-white font-medium transition-colors"
          >
            {{ t('common.save') }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, reactive, watch } from 'vue'
import { useRouter } from 'vue-router'
import { manufacturingOrderService } from '@/services/manufacturingOrderService'
import type { ManufacturingOrder } from '@/services/manufacturingOrderService'
import { toast } from '@/plugins/toast'
import { useI18n } from 'vue-i18n'
import { EntityStatus } from '@/constants/status'

const { t } = useI18n()
const router = useRouter()

const loading = ref(true)
const orders = ref<ManufacturingOrder[]>([])
const total = ref(0)
const currentPage = ref(1)
const pageSize = ref(25)

const search = ref('')
const activeStatus = ref('all')
const dateFrom = ref('')
const dateTo = ref('')

const expandedOrderId = ref<number | null>(null)
const detailsByOrder = reactive<Record<number, any>>({})
const loadingDetails = reactive<Record<number, boolean>>({})

const showStatusModal = ref(false)
const selectedOrder = ref<ManufacturingOrder | null>(null)
const newStatus = ref<string>(EntityStatus.Pending)

const totalPages = computed(() => Math.ceil(total.value / pageSize.value))

const visiblePages = computed(() => {
  const pages: number[] = []
  const start = Math.max(1, currentPage.value - 2)
  const end = Math.min(totalPages.value, currentPage.value + 2)
  for (let i = start; i <= end; i++) pages.push(i)
  return pages
})

let debounceTimer: ReturnType<typeof setTimeout>

watch([search, dateFrom, dateTo], () => {
  clearTimeout(debounceTimer)
  debounceTimer = setTimeout(() => {
    currentPage.value = 1
    loadOrders()
  }, 300)
})

onMounted(async () => {
  await loadOrders()
})

function normalizeList(value: any): any[] {
  return value?.$values ?? value ?? []
}

async function loadOrders() {
  loading.value = true
  try {
    const res = await manufacturingOrderService.getPaged({
      page: currentPage.value,
      pageSize: pageSize.value,
      search: search.value || undefined,
      status: activeStatus.value === 'all' ? undefined : activeStatus.value,
      dateFrom: dateFrom.value || undefined,
      dateTo: dateTo.value || undefined,
    })

    orders.value = normalizeList(res.data.data)
    total.value = res.data.total
  } catch {
    toast.error(t('errors.loadFailed'))
  } finally {
    loading.value = false
  }
}

function onStatusChange() {
  currentPage.value = 1
  loadOrders()
}

function onPageSizeChange() {
  currentPage.value = 1
  loadOrders()
}

function goToPage(page: number) {
  if (page < 1 || page > totalPages.value) return
  currentPage.value = page
  loadOrders()
}

function clearFilters() {
  search.value = ''
  dateFrom.value = ''
  dateTo.value = ''
  activeStatus.value = 'all'
  currentPage.value = 1
  loadOrders()
}

async function toggleOrder(order: ManufacturingOrder) {
  if (expandedOrderId.value === order.id) {
    expandedOrderId.value = null
    return
  }

  expandedOrderId.value = order.id

  if (!detailsByOrder[order.id]) {
    loadingDetails[order.id] = true
    try {
      const res = await manufacturingOrderService.getDetails(order.id)

      detailsByOrder[order.id] = {
        ...res.data,
        products: normalizeList(res.data.products),
      }
    } catch {
      toast.error(t('errors.loadFailed'))
    } finally {
      loadingDetails[order.id] = false
    }
  }
}

function openUpdateStatus(order: ManufacturingOrder) {
  if (order.status === EntityStatus.Completed) return

  selectedOrder.value = order
  newStatus.value = order.status ?? EntityStatus.Pending
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

function goToProductTimeline(serialNumber: string | null) {
  if (!serialNumber) return
  router.push({ name: 'ProductTimeline', query: { vin: serialNumber } })
}

function formatDate(dateStr: string) {
  return new Date(dateStr).toLocaleDateString('pt-PT')
}

function statusClass(status: string | null) {
  switch (status) {
    case EntityStatus.Pending: return 'bg-warning-100 text-warning-700'
    case EntityStatus.InProgress: return 'bg-primary-50 text-primary-600'
    case EntityStatus.Completed: return 'bg-success-100 text-success-700'
    case EntityStatus.Cancelled: return 'bg-danger-100 text-danger-700'
    default: return 'bg-background-200 text-background-600'
  }
}

function statusLabel(status: string | null) {
  switch (status) {
    case EntityStatus.Pending: return t('mo.status.pending')
    case EntityStatus.InProgress: return t('mo.status.inProgress')
    case EntityStatus.Completed: return t('mo.status.completed')
    case EntityStatus.Cancelled: return t('mo.status.cancelled')
    default: return t('mo.status.unknown')
  }
}
</script>