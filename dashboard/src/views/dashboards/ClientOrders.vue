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
        <div class="grid grid-cols-5 px-4 py-3 bg-background-100 dark:bg-background-800 border-b border-background-300 dark:border-background-700">
          <span class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('orders.fields.orderNumber') }}</span>
          <span class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('orders.fields.customer') }}</span>
          <span class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('orders.fields.date') }}</span>
          <span class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('orders.fields.quantity') }}</span>
          <span class="text-xs font-medium text-background-500 uppercase tracking-wider text-right">{{ t('common.actions') }}</span>
        </div>

        <!-- Table Rows -->
        <div
          v-for="order in orders"
          :key="order.id"
          class="grid grid-cols-5 px-4 py-3 border-b border-background-200 dark:border-background-700 last:border-0 bg-background-50 dark:bg-background-800 hover:bg-background-100 dark:hover:bg-background-750 transition-colors items-center"
        >
          <span class="text-sm font-medium text-background-900 dark:text-background-50">{{ order.orderNumber }}</span>
          <span class="text-sm text-background-700 dark:text-background-300">{{ order.customerName }}</span>
          <span class="text-sm text-background-500">{{ formatDate(order.orderDate) }}</span>
          <span class="text-sm text-background-700 dark:text-background-300">{{ order.quantity }} un.</span>
          <div class="flex justify-end">
            <button
              @click="deleteOrder(order)"
              class="p-1.5 rounded-lg text-background-400 hover:text-danger-500 hover:bg-danger-100 dark:hover:bg-background-700 transition-colors"
            >
              <span class="material-symbols-rounded text-base">delete</span>
            </button>
          </div>
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

          <!-- Dados da Encomenda -->
          <div class="flex flex-col gap-1.5">
            <label class="text-xs font-medium text-background-600 dark:text-background-400">{{ t('orders.fields.orderNumber') }} *</label>
            <input v-model="form.orderNumber" type="text" :placeholder="t('orders.fields.orderNumberPlaceholder')" />
          </div>

          <div class="flex flex-col gap-1.5">
            <label class="text-xs font-medium text-background-600 dark:text-background-400">{{ t('orders.fields.customer') }} *</label>
            <input v-model="form.customerName" type="text" :placeholder="t('orders.fields.customerPlaceholder')" />
          </div>

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
                <span class="text-background-400 font-normal">({{ t('orders.optionalDefault') }})</span>
              </label>
              <select v-model="selectedOptions[config.id]">
                <option :value="undefined">{{ t('orders.useDefault') }}</option>
                <option
                  v-for="option in optionsByConfig[config.id]"
                  :key="option.id"
                  :value="option.id"
                >
                  {{ option.value }}{{ option.isDefault ? ` (${t('carModels.default')})` : '' }}
                </option>
              </select>
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
            {{ t('orders.createdDesc', { count: lastResult?.totalQuantity, customer: lastResult?.customerName }) }}
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
import { ref, reactive, computed, onMounted } from 'vue'
import { clientOrderService } from '@/services/clientOrderService'
import type { ClientOrder, CreateClientOrderResult } from '@/services/clientOrderService'
import { carModelService, configService, configOptionService } from '@/services/carModelService'
import type { CarModel, Config, ConfigOption } from '@/services/carModelService'
import { toast } from '@/plugins/toast'
import { useI18n } from 'vue-i18n'

const { t } = useI18n()

const loading = ref(true)
const orders = ref<ClientOrder[]>([])
const models = ref<CarModel[]>([])
const configs = ref<Config[]>([])
const optionsByConfig = reactive<Record<number, ConfigOption[]>>({})
const selectedOptions = reactive<Record<number, number | undefined>>({})

const showModal = ref(false)
const showResultModal = ref(false)
const loadingConfigs = ref(false)
const submitting = ref(false)
const lastResult = ref<CreateClientOrderResult | null>(null)

const form = reactive({
  orderNumber: '',
  customerName: '',
  orderDate: new Date().toISOString(),
  quantity: 1,
  modelId: 0,
})

const isFormValid = computed(() =>
  form.orderNumber.trim() !== '' &&
  form.customerName.trim() !== '' &&
  form.quantity >= 1 &&
  form.modelId !== 0
)

onMounted(async () => {
  await Promise.all([loadOrders(), loadModels()])
})

async function loadOrders() {
  loading.value = true
  try {
    const res = await clientOrderService.getAll()
    orders.value = res.data
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
  } catch {
    // silencioso
  }
}

async function onModelChange() {
  if (!form.modelId) return
  loadingConfigs.value = true
  configs.value = []
  Object.keys(optionsByConfig).forEach(k => delete optionsByConfig[Number(k)])
  Object.keys(selectedOptions).forEach(k => delete selectedOptions[Number(k)])

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

function openCreateModal() {
  form.orderNumber = ''
  form.customerName = ''
  form.orderDate = new Date().toISOString().split('T')[0]
  form.quantity = 1
  form.modelId = 0
  configs.value = []
  showModal.value = true
}

function closeModal() {
  showModal.value = false
}

async function submitCreate() {
  submitting.value = true
  try {
    const selectedConfigOptions = Object.entries(selectedOptions)
      .filter(([, val]) => val !== undefined)
      .map(([, val]) => ({ configOptionId: val as number }))

    const res = await clientOrderService.create({
      orderNumber: form.orderNumber,
      customerName: form.customerName,
      orderDate: new Date(form.orderDate).toISOString(),
      quantity: form.quantity,
      modelId: form.modelId,
      configs: selectedConfigOptions,
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

async function deleteOrder(order: ClientOrder) {
  try {
    await clientOrderService.delete(order.id)
    toast.success(t('orders.deleted'))
    await loadOrders()
  } catch {
    toast.error(t('errors.deleteFailed'))
  }
}

function formatDate(dateStr: string) {
  return new Date(dateStr).toLocaleDateString('pt-PT')
}
</script>
