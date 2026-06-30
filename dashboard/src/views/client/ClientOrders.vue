<template>
  <div class="p-8 w-full">

    <!-- Header -->
    <div class="flex items-start justify-between mb-8">
      <div>
        <h1 class="text-2xl font-medium text-background-900 dark:text-background-50">
          {{ t('orders.title') }}
        </h1>
        <p class="text-sm text-background-600 dark:text-background-400 mt-1">
          {{ t('orders.subtitle') }}
        </p>
      </div>
      <button
        @click="openCreateModal"
        class="flex items-center gap-2 bg-primary-500 hover:bg-primary-600 text-white text-sm font-medium px-4 py-2 rounded-lg transition-colors"
      >
        <span class="material-symbols-rounded text-base">add</span>
        {{ t('orders.newOrder') }}
      </button>
    </div>

    <!-- Search bar + status filter + page size -->
    <div class="flex items-center justify-between gap-3 mb-4">
      <div class="flex items-center gap-2 flex-1">
        <input
          v-model="searchQuery"
          type="text"
          :placeholder="t('orders.searchPlaceholder')"
          class="w-full max-w-sm px-3 py-2 text-sm border border-background-300 dark:border-background-600 rounded-lg bg-background-50 dark:bg-background-800 text-background-900 dark:text-background-50 focus:outline-none focus:ring-2 focus:ring-primary-500"
        />
        <select
          v-model="statusFilter"
          @change="currentPage = 1"
          class="w-auto px-3 py-2 text-sm border border-background-300 dark:border-background-600 rounded-lg bg-background-50 dark:bg-background-800 text-background-900 dark:text-background-50 focus:outline-none focus:ring-2 focus:ring-primary-500"
        >
          <option value="">{{ t('common.allStatuses') }}</option>
          <option value="pending">{{ t('orders.status.pending') }}</option>
          <option value="in_progress">{{ t('orders.status.in_progress') }}</option>
          <option value="completed">{{ t('orders.status.completed') }}</option>
          <option value="cancelled">{{ t('orders.status.cancelled') }}</option>
        </select>
      </div>
      <select
        v-if="totalItems > 25"
        v-model="pageSize"
        @change="currentPage = 1"
        class="w-20 px-2 py-2 text-sm border border-background-300 dark:border-background-600 rounded-lg bg-background-50 dark:bg-background-800 text-background-900 dark:text-background-50"
      >
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
      <div v-if="orders.length === 0" class="text-center py-16 text-background-500">
        <span class="material-symbols-rounded text-5xl block mb-3">inbox</span>
        <p class="text-sm">{{ t('orders.empty') }}</p>
      </div>

      <div v-else class="border border-background-300 dark:border-background-700 rounded-xl overflow-hidden">
        <!-- Table Header -->
        <div class="grid grid-cols-6 px-4 py-3 bg-background-100 dark:bg-background-800 border-b border-background-300 dark:border-background-700">
          <span class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('orders.fields.orderNumber') }}</span>
          <span class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('orders.fields.customer') }}</span>
          <span class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('orders.fields.date') }}</span>
          <span class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('orders.fields.quantity') }}</span>
          <span class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('common.status') }}</span>
          <span class="text-xs font-medium text-background-500 uppercase tracking-wider text-right">{{ t('common.actions') }}</span>
        </div>

        <!-- Table Rows -->
        <div
          v-for="order in orders"
          :key="order.id"
          class="grid grid-cols-6 px-4 py-3 border-b border-background-200 dark:border-background-700 last:border-0 bg-background-50 dark:bg-background-800 hover:bg-background-100 dark:hover:bg-background-750 transition-colors items-center"
        >
          <span class="text-sm font-medium text-background-900 dark:text-background-50">{{ order.orderNumber }}</span>
          <span class="text-sm text-background-700 dark:text-background-300">{{ order.clientName }}</span>
          <span class="text-sm text-background-500">{{ formatDate(order.orderDate) }}</span>
          <span class="text-sm text-background-700 dark:text-background-300">{{ order.quantity }} un.</span>

          <!-- Status Badge -->
          <span :class="statusBadgeClass(order.status)" class="inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium w-fit">
            {{ t(`orders.status.${order.status}`) }}
          </span>

          <!-- Ações -->
          <div class="flex justify-end">
            <button
              v-if="order.status !== 'cancelled' && order.status !== 'completed'"
              @click="openCancelModal(order)"
              class="p-1.5 rounded-lg text-background-400 hover:text-warning-500 hover:bg-warning-100 dark:hover:bg-background-700 transition-colors"
              :title="t('orders.cancelOrder')"
            >
              <span class="material-symbols-rounded text-base">cancel</span>
            </button>
            <span v-else class="w-8" />
          </div>
        </div>
      </div>

      <!-- Pagination -->
      <div v-if="totalItems > 0" class="flex items-center justify-between mt-4 text-sm text-background-500">
        <span>{{ t('common.showing', { from: paginationFrom, to: paginationTo, total: totalItems }) }}</span>
        <div class="flex items-center gap-2">
          <button
            @click="currentPage--"
            :disabled="currentPage === 1"
            class="px-3 py-1 rounded-lg border border-background-300 dark:border-background-600 disabled:opacity-40 hover:bg-background-100 dark:hover:bg-background-700 transition-colors"
          >{{ t('common.prev') }}</button>
          <span class="tabular-nums">{{ currentPage }} / {{ totalPages }}</span>
          <button
            @click="currentPage++"
            :disabled="currentPage >= totalPages"
            class="px-3 py-1 rounded-lg border border-background-300 dark:border-background-600 disabled:opacity-40 hover:bg-background-100 dark:hover:bg-background-700 transition-colors"
          >{{ t('common.next') }}</button>
        </div>
      </div>
    </div>

    <!-- Modal: Cancelar Encomenda -->
    <div v-if="showCancelModal" class="fixed inset-0 bg-black/40 flex items-center justify-center z-50" @click.self="showCancelModal = false">
      <div class="bg-background-50 dark:bg-background-800 border border-background-300 dark:border-background-700 rounded-xl w-full max-w-md overflow-hidden">
        <div class="flex items-center justify-between px-5 py-4 border-b border-background-300 dark:border-background-700">
          <h2 class="text-base font-medium text-background-900 dark:text-background-50">{{ t('orders.cancelTitle') }}</h2>
          <button @click="showCancelModal = false" class="text-background-500 hover:text-background-700 dark:hover:text-background-300">
            <span class="material-symbols-rounded">close</span>
          </button>
        </div>
        <div class="px-5 py-4">
          <p class="text-sm text-background-600 dark:text-background-400">
            {{ t('orders.cancelConfirm', { number: orderToCancel?.orderNumber }) }}
          </p>
          <p class="text-xs text-background-400 dark:text-background-500 mt-2">
            {{ t('orders.cancelNote') }}
          </p>
        </div>
        <div class="flex justify-end gap-2 px-5 py-4 border-t border-background-300 dark:border-background-700">
          <button
            @click="showCancelModal = false"
            class="px-4 py-2 text-sm rounded-lg border border-background-300 dark:border-background-600 text-background-700 dark:text-background-300 hover:bg-background-100 dark:hover:bg-background-700 transition-colors"
          >
            {{ t('common.back') }}
          </button>
          <button
            @click="confirmCancel"
            :disabled="cancelling"
            class="px-4 py-2 text-sm rounded-lg bg-warning-500 hover:bg-warning-600 disabled:opacity-50 disabled:cursor-not-allowed text-white font-medium transition-colors"
          >
            <span v-if="cancelling" class="material-symbols-rounded animate-spin text-base mr-1">autorenew</span>
            {{ t('orders.cancelConfirmBtn') }}
          </button>
        </div>
      </div>
    </div>

    <!-- Modal: Criar Encomenda -->
    <div v-if="showModal" class="fixed inset-0 bg-black/40 flex items-center justify-center z-50" @click.self="closeModal">
      <div class="bg-background-50 dark:bg-background-800 border border-background-300 dark:border-background-700 rounded-xl w-full max-w-lg overflow-hidden">

        <!-- Modal Header -->
        <div class="flex items-center justify-between px-5 py-4 border-b border-background-300 dark:border-background-700">
          <h2 class="text-base font-medium text-background-900 dark:text-background-50">{{ t('orders.newOrder') }}</h2>
          <button @click="closeModal" class="text-background-500 hover:text-background-700 dark:hover:text-background-300">
            <span class="material-symbols-rounded">close</span>
          </button>
        </div>

        <!-- Modal Body -->
        <div class="px-5 py-4 flex flex-col gap-4 max-h-[70vh] overflow-y-auto">

          <!-- Número da Encomenda -->
          <div class="flex flex-col gap-1.5">
            <label class="text-xs font-medium text-background-600 dark:text-background-400">{{ t('orders.fields.orderNumber') }} *</label>
            <input v-model="form.orderNumber" type="text" :placeholder="t('orders.fields.orderNumberPlaceholder')" />
          </div>

          <!-- Cliente (dropdown) -->
          <div class="flex flex-col gap-1.5">
            <label class="text-xs font-medium text-background-600 dark:text-background-400">{{ t('orders.fields.customer') }} *</label>
            <select v-model="form.appUserId">
              <option :value="0" disabled>{{ t('orders.fields.customerPlaceholder') }}</option>
              <option v-for="client in clients" :key="client.id" :value="client.id">
                {{ client.name }}
              </option>
            </select>
          </div>

          <!-- Quantidade -->
          <div class="flex flex-col gap-1.5">
            <label class="text-xs font-medium text-background-600 dark:text-background-400">{{ t('orders.fields.quantity') }} *</label>
            <input v-model.number="form.quantity" type="number" min="1" placeholder="1" />
          </div>

          <!-- Selecionar Modelo -->
          <div class="flex flex-col gap-1.5">
            <label class="text-xs font-medium text-background-600 dark:text-background-400">{{ t('orders.fields.model') }} *</label>
            <select v-model="form.modelId" @change="onModelChange">
              <option value="0" disabled>{{ t('orders.fields.modelPlaceholder') }}</option>
              <option v-for="model in models" :key="model.id" :value="model.id">
                {{ model.name }} {{ model.version ? `(${model.version})` : '' }}
              </option>
            </select>
          </div>

          <!-- Configurações do Modelo (carrega dinamicamente) -->
          <div v-if="loadingConfigs" class="flex items-center gap-2 text-background-500 text-sm">
            <span class="material-symbols-rounded animate-spin text-base">autorenew</span>
            {{ t('common.loading') }}
          </div>

          <div v-if="configs.length > 0" class="flex flex-col gap-3">
            <div class="text-xs font-medium text-background-500 uppercase tracking-wider pt-1 border-t border-background-200 dark:border-background-700">
              {{ t('orders.configurations') }}
            </div>

            <div v-for="config in configs" :key="config.id" class="flex flex-col gap-1.5">
              <label class="text-xs font-medium text-background-600 dark:text-background-400">
                {{ config.item }}
                <span v-if="!config.allowMultiple" class="text-background-400 font-normal">({{ t('orders.optionalDefault') }})</span>
              </label>

              <!-- Single-select -->
              <select v-if="!config.allowMultiple" v-model="selectedOptions[config.id]">
                <option :value="undefined">{{ t('orders.useDefault') }}</option>
                <option
                  v-for="option in optionsByConfig[config.id]"
                  :key="option.id"
                  :value="option.id"
                >
                  {{ option.value }}{{ option.isDefault ? ` (${t('carModels.default')})` : '' }}
                </option>
              </select>

              <!-- Multi-select -->
              <div v-else class="flex flex-col gap-1.5">
                <label
                  v-for="option in optionsByConfig[config.id]"
                  :key="option.id"
                  class="flex items-center gap-2 text-sm text-background-700 dark:text-background-300"
                >
                  <input
                    type="checkbox"
                    :checked="(selectedAccessoryOptions[config.id] || []).includes(option.id)"
                    @change="toggleAccessoryOption(config.id, option.id, ($event.target as HTMLInputElement).checked)"
                    class="w-4 h-4 accent-primary-500"
                  />
                  {{ option.value }}
                  <span v-if="option.isDefault" class="text-xs text-background-400">({{ t('carModels.default') }})</span>
                </label>
              </div>
            </div>
          </div>

        </div>

        <!-- Modal Footer -->
        <div class="flex justify-end gap-2 px-5 py-4 border-t border-background-300 dark:border-background-700">
          <button @click="closeModal" class="px-4 py-2 text-sm rounded-lg border border-background-300 dark:border-background-600 text-background-700 dark:text-background-300 hover:bg-background-100 dark:hover:bg-background-700 transition-colors">
            {{ t('common.cancel') }}
          </button>
          <button
            @click="submitCreate"
            :disabled="!isFormValid || submitting"
            class="px-4 py-2 text-sm rounded-lg bg-primary-500 hover:bg-primary-600 disabled:opacity-50 disabled:cursor-not-allowed text-white font-medium transition-colors"
          >
            <span v-if="submitting" class="material-symbols-rounded animate-spin text-base mr-1">autorenew</span>
            {{ t('common.save') }}
          </button>
        </div>
      </div>
    </div>

    <!-- Modal: Resultado da criação -->
    <div v-if="showResultModal" class="fixed inset-0 bg-black/40 flex items-center justify-center z-50" @click.self="showResultModal = false">
      <div class="bg-background-50 dark:bg-background-800 border border-background-300 dark:border-background-700 rounded-xl w-full max-w-lg overflow-hidden">
        <div class="flex items-center justify-between px-5 py-4 border-b border-background-300 dark:border-background-700">
          <h2 class="text-base font-medium text-background-900 dark:text-background-50">{{ t('orders.created') }}</h2>
          <button @click="showResultModal = false" class="text-background-500 hover:text-background-700">
            <span class="material-symbols-rounded">close</span>
          </button>
        </div>
        <div class="px-5 py-4">
          <p class="text-sm text-background-600 dark:text-background-400 mb-3">
            {{ t('orders.createdDesc', { count: lastResult?.totalQuantity, customer: lastResult?.clientName }) }}
          </p>
          <div class="flex flex-col gap-2 max-h-64 overflow-y-auto">
            <div
              v-for="product in lastResult?.productsCreated"
              :key="product.productId"
              class="flex items-center justify-between px-3 py-2 bg-background-100 dark:bg-background-700 rounded-lg"
            >
              <span class="text-xs font-medium text-background-700 dark:text-background-300">{{ product.serialNumber }}</span>
              <span class="text-xs text-background-500">{{ product.moNumber }}</span>
            </div>
          </div>
        </div>
        <div class="flex justify-end px-5 py-4 border-t border-background-300 dark:border-background-700">
          <button @click="showResultModal = false" class="px-4 py-2 text-sm rounded-lg bg-primary-500 hover:bg-primary-600 text-white font-medium transition-colors">
            {{ t('common.close') }}
          </button>
        </div>
      </div>
    </div>

  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted, watch } from 'vue'
import { clientOrderService } from '@/services/clientOrderService'
import type { ClientOrder, CreateClientOrderResult } from '@/services/clientOrderService'
import { carModelService, configService, configOptionService } from '@/services/carModelService'
import type { CarModel, Config, ConfigOption } from '@/services/carModelService'
import { clientUserService } from '@/services/clientUserService'
import type { ClientOption } from '@/services/clientUserService'
import { toast } from '@/plugins/toast'
import { useI18n } from 'vue-i18n'

const { t } = useI18n()

// --- Data ---
const loading = ref(true)
const orders = ref<ClientOrder[]>([])
const models = ref<CarModel[]>([])
const clients = ref<ClientOption[]>([])
const configs = ref<Config[]>([])
const optionsByConfig = reactive<Record<number, ConfigOption[]>>({})
const selectedOptions = reactive<Record<number, number | undefined>>({})
const selectedAccessoryOptions = reactive<Record<number, number[]>>({})

// --- Search & Pagination (server-side) ---
const searchQuery = ref('')
const statusFilter = ref('')
const currentPage = ref(1)
const pageSize = ref(25)
const totalItems = ref(0)

const totalPages = computed(() => Math.max(1, Math.ceil(totalItems.value / pageSize.value)))
const paginationFrom = computed(() => totalItems.value === 0 ? 0 : (currentPage.value - 1) * pageSize.value + 1)
const paginationTo = computed(() => Math.min(currentPage.value * pageSize.value, totalItems.value))

let searchDebounceHandle: ReturnType<typeof setTimeout> | undefined
watch(searchQuery, () => {
  clearTimeout(searchDebounceHandle)
  searchDebounceHandle = setTimeout(() => {
    currentPage.value = 1
    loadOrders()
  }, 300)
})

watch([currentPage, pageSize, statusFilter], () => {
  loadOrders()
})

// --- Status badge ---
function statusBadgeClass(status: string) {
  switch (status) {
    case 'completed':  return 'bg-success-100 text-success-700 dark:bg-success-900/30 dark:text-success-400'
    case 'in_progress': return 'bg-primary-100 text-primary-700 dark:bg-primary-900/30 dark:text-primary-400'
    case 'cancelled':  return 'bg-background-100 text-background-500 dark:bg-background-700 dark:text-background-400'
    default:           return 'bg-warning-100 text-warning-700 dark:bg-warning-900/30 dark:text-warning-400' // pending
  }
}

// --- Cancel modal ---
const showCancelModal = ref(false)
const orderToCancel = ref<ClientOrder | null>(null)
const cancelling = ref(false)

function openCancelModal(order: ClientOrder) {
  orderToCancel.value = order
  showCancelModal.value = true
}

async function confirmCancel() {
  if (!orderToCancel.value) return
  cancelling.value = true
  try {
    await clientOrderService.cancel(orderToCancel.value.id)
    toast.success(t('orders.cancelSuccess'))
    showCancelModal.value = false
    orderToCancel.value = null
    await loadOrders()
  } catch {
    toast.error(t('errors.saveFailed'))
  } finally {
    cancelling.value = false
  }
}

// --- Modal state ---
const showModal = ref(false)
const showResultModal = ref(false)
const loadingConfigs = ref(false)
const submitting = ref(false)
const lastResult = ref<CreateClientOrderResult | null>(null)

const form = reactive({
  orderNumber: '',
  appUserId: 0,
  orderDate: new Date().toISOString(),
  quantity: 1,
  modelId: 0,
})

const isFormValid = computed(() =>
  form.orderNumber.trim() !== '' &&
  form.appUserId !== 0 &&
  form.quantity >= 1 &&
  form.modelId !== 0
)

// --- Lifecycle ---
onMounted(async () => {
  await Promise.all([loadOrders(), loadModels(), loadClients()])
})

// --- Loaders ---
async function loadOrders() {
  loading.value = true
  try {
    const res = await clientOrderService.getPaged({
      page: currentPage.value,
      pageSize: pageSize.value,
      search: searchQuery.value.trim() || undefined,
      status: statusFilter.value || undefined,
    })
    orders.value = res.data.data
    totalItems.value = res.data.total
  } catch {
    toast.error(t('errors.loadFailed'))
  } finally {
    loading.value = false
  }
}

async function loadModels() {
  try {
    const res = await carModelService.getAll()
    models.value = res.data
  } catch { /* silencioso */ }
}

async function loadClients() {
  try {
    const res = await clientUserService.getClients()
    clients.value = res.data
  } catch { /* silencioso */ }
}

async function onModelChange() {
  if (!form.modelId) return
  loadingConfigs.value = true
  configs.value = []
  Object.keys(optionsByConfig).forEach(k => delete optionsByConfig[Number(k)])
  Object.keys(selectedOptions).forEach(k => delete selectedOptions[Number(k)])
  Object.keys(selectedAccessoryOptions).forEach(k => delete selectedAccessoryOptions[Number(k)])

  try {
    const res = await configService.getByModel(form.modelId)
    configs.value = res.data

    const optRes = await configOptionService.getAll()
    for (const option of optRes.data) {
      if (!optionsByConfig[option.configId]) optionsByConfig[option.configId] = []
      optionsByConfig[option.configId].push(option)
    }
  } catch {
    toast.error(t('errors.loadFailed'))
  } finally {
    loadingConfigs.value = false
  }
}

function toggleAccessoryOption(configId: number, optionId: number, checked: boolean) {
  if (!selectedAccessoryOptions[configId]) selectedAccessoryOptions[configId] = []
  const arr = selectedAccessoryOptions[configId]
  if (checked) {
    if (!arr.includes(optionId)) arr.push(optionId)
  } else {
    const idx = arr.indexOf(optionId)
    if (idx !== -1) arr.splice(idx, 1)
  }
}

function openCreateModal() {
  form.orderNumber = ''
  form.appUserId = 0
  form.orderDate = new Date().toISOString().split('T')[0]
  form.quantity = 1
  form.modelId = 0
  configs.value = []
  Object.keys(selectedOptions).forEach(k => delete selectedOptions[Number(k)])
  Object.keys(selectedAccessoryOptions).forEach(k => delete selectedAccessoryOptions[Number(k)])
  showModal.value = true
}

function closeModal() {
  showModal.value = false
}

async function submitCreate() {
  submitting.value = true
  try {
    const fromSingles = Object.entries(selectedOptions)
      .filter(([, val]) => val !== undefined)
      .map(([, val]) => ({ configOptionId: val as number }))

    const fromAccessories = Object.values(selectedAccessoryOptions)
      .flat()
      .map(optionId => ({ configOptionId: optionId }))

    const res = await clientOrderService.create({
      orderNumber: form.orderNumber,
      orderDate: new Date(form.orderDate).toISOString(),
      appUserId: form.appUserId,
      quantity: form.quantity,
      modelId: form.modelId,
      configs: [...fromSingles, ...fromAccessories],
    })

    lastResult.value = res.data
    closeModal()
    showResultModal.value = true
    toast.success(t('orders.createdSuccess'))
    await loadOrders()
  } catch {
    toast.error(t('errors.saveFailed'))
  } finally {
    submitting.value = false
  }
}

function formatDate(dateStr: string) {
  return new Date(dateStr).toLocaleDateString('pt-PT')
}
</script>