<template>
  <div class="p-8 w-full">

    <!-- Header -->
    <div class="mb-6">
      <h1 class="text-2xl font-medium text-background-900 dark:text-background-50">
        {{ t('products.title') }}
      </h1>
      <p class="text-sm text-background-600 dark:text-background-400 mt-1">
        {{ t('products.subtitle') }}
      </p>
    </div>

    <!-- Barra de filtros -->
    <div class="flex gap-3 mb-6 items-center">

      <!-- Search VIN / lote -->
      <div class="relative w-64 shrink-0">
        <span class="material-symbols-rounded absolute left-3 top-1/2 -translate-y-1/2 text-background-400 text-base pointer-events-none">search</span>
        <input
          v-model="search"
          type="text"
          :placeholder="t('products.searchPlaceholder')"
          class="w-full pl-9 pr-4 py-2 text-sm rounded-lg border border-background-300 dark:border-background-700 bg-background-50 dark:bg-background-800 text-background-900 dark:text-background-50 placeholder-background-400 focus:outline-none focus:border-primary-400"
        />
      </div>

      <!-- Modelo (dropdown) -->
      <select
        v-model="modelFilter"
        @change="onModelChange"
        class="w-44 shrink-0 px-3 py-2 text-sm rounded-lg border border-background-300 dark:border-background-700 bg-background-50 dark:bg-background-800 text-background-700 dark:text-background-300 focus:outline-none focus:border-primary-400"
      >
        <option value="">{{ t('products.allModels') }}</option>
        <option v-for="model in carModels" :key="model.id" :value="model.id">{{ model.name }}</option>
      </select>

      <!-- Data início -->
      <input
        v-model="dateFrom"
        type="date"
        class="w-36 shrink-0 px-3 py-2 text-sm rounded-lg border border-background-300 dark:border-background-700 bg-background-50 dark:bg-background-800 text-background-700 dark:text-background-300 focus:outline-none focus:border-primary-400"
      />

      <!-- Data fim -->
      <input
        v-model="dateTo"
        type="date"
        class="w-36 shrink-0 px-3 py-2 text-sm rounded-lg border border-background-300 dark:border-background-700 bg-background-50 dark:bg-background-800 text-background-700 dark:text-background-300 focus:outline-none focus:border-primary-400"
      />

      <!-- Limpar -->
      <button
        v-if="search || modelFilter || dateFrom || dateTo"
        @click="clearFilters"
        class="shrink-0 px-3 py-2 text-sm rounded-lg border border-background-300 dark:border-background-700 text-background-500 hover:text-background-700 dark:hover:text-background-300 hover:bg-background-100 dark:hover:bg-background-700 transition-colors"
        title="Limpar filtros"
      >
        <span class="material-symbols-rounded text-base align-middle">close</span>
      </button>

      <!-- Registos por página -->
      <select
        v-model="pageSize"
        @change="onPageSizeChange"
        class="ml-auto w-20 shrink-0 px-3 py-2 text-sm rounded-lg border border-background-300 dark:border-background-700 bg-background-50 dark:bg-background-800 text-background-700 dark:text-background-300 focus:outline-none focus:border-primary-400"
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

    <div v-else>
      <div
        v-if="products.length === 0"
        class="border border-background-300 dark:border-background-700 rounded-xl bg-background-50 dark:bg-background-800 text-center py-16 text-background-500"
      >
        <span class="material-symbols-rounded text-5xl block mb-3">directions_car</span>
        <p class="text-sm">{{ t('products.empty') }}</p>
      </div>

      <div v-else class="border border-background-300 dark:border-background-700 rounded-xl overflow-hidden bg-background-50 dark:bg-background-800">
        <!-- Header tabela -->
        <div class="grid grid-cols-5 px-5 py-3 bg-background-100 dark:bg-background-800 border-b border-background-300 dark:border-background-700">
          <span class="text-xs font-medium text-background-500 uppercase tracking-wider col-span-2">{{ t('products.fields.serial') }}</span>
          <span class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('products.fields.model') }}</span>
          <span class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('products.fields.lot') }}</span>
          <span class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('products.fields.productionDate') }}</span>
        </div>

        <!-- Rows -->
        <template v-for="product in products" :key="product.id">
          <div
            class="grid grid-cols-5 px-5 py-4 border-b border-background-200 dark:border-background-700 bg-background-50 dark:bg-background-800 hover:bg-background-100 dark:hover:bg-background-700 transition-colors items-center cursor-pointer"
            @click="toggleProduct(product)"
          >
            <div class="col-span-2 flex items-center gap-3">
              <span
                class="material-symbols-rounded text-background-400 text-lg transition-transform duration-200"
                :class="expandedProductId === product.id ? 'rotate-180' : ''"
              >expand_more</span>
              <span class="text-sm font-medium text-background-900 dark:text-background-50">
                {{ product.serialNumber || t('products.noSerial') }}
              </span>
            </div>
            <span class="text-sm text-background-700 dark:text-background-300">{{ product.modelName ?? '—' }}</span>
            <span class="text-sm text-background-500 dark:text-background-400">{{ product.lotNumber ?? '—' }}</span>
            <span class="text-sm text-background-500 dark:text-background-400">
              {{ product.productionDate ? formatDate(product.productionDate) : '—' }}
            </span>
          </div>

          <!-- Expand: Timeline -->
          <div
            v-if="expandedProductId === product.id"
            class="border-b border-background-200 dark:border-background-700 bg-background-100 dark:bg-background-900 px-5 py-4"
          >
            <div v-if="loadingTimeline[product.id]" class="flex items-center gap-2 text-background-400 text-xs py-2">
              <span class="material-symbols-rounded animate-spin text-sm">autorenew</span>
              {{ t('common.loading') }}
            </div>

            <div v-else-if="timelineByProduct[product.id]">
              <div class="flex items-center justify-between mb-3">
                <div>
                  <div class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('products.timeline') }}</div>
                  <div class="text-xs text-background-400 mt-0.5">{{ phaseCountLabel(timelineByProduct[product.id].phases.length) }}</div>
                </div>
                <span class="text-xs font-medium px-2 py-0.5 rounded-full" :class="statusClass(timelineByProduct[product.id].status)">
                  {{ statusLabel(timelineByProduct[product.id].status) }}
                </span>
              </div>

              <div v-if="timelineByProduct[product.id].phases?.length" class="flex flex-col gap-2">
                <div
                  v-for="phase in timelineByProduct[product.id].phases"
                  :key="phase.productPhaseId"
                  class="bg-background-50 dark:bg-background-800 border border-background-300 dark:border-background-700 rounded-xl px-4 py-3"
                >
                  <div class="flex items-center justify-between gap-4">
                    <div class="flex items-start gap-3 min-w-0">
                      <div class="w-2.5 h-2.5 rounded-full flex-shrink-0 mt-1.5" :class="phase.endedAt ? 'bg-success-500' : 'bg-primary-500 animate-pulse'" />
                      <div class="min-w-0">
                        <div class="flex items-center gap-2">
                          <span class="material-symbols-rounded text-background-500 dark:text-background-400 text-base">precision_manufacturing</span>
                          <span class="text-sm font-medium text-background-900 dark:text-background-50">{{ phase.phaseName }}</span>
                        </div>
                        <div class="flex flex-wrap items-center gap-x-3 gap-y-1 mt-1">
                          <span class="text-xs text-background-500 dark:text-background-400">{{ phase.workstation || '—' }}</span>
                          <span class="text-xs text-background-400">{{ formatDateTime(phase.startedAt) }}</span>
                          <span v-if="formatDuration(phase.durationSeconds, phase.endedAt)" class="text-xs text-background-500 dark:text-background-400">
                            {{ formatDuration(phase.durationSeconds, phase.endedAt) }}
                          </span>
                        </div>
                        <div v-if="phase.notes" class="text-xs text-background-400 mt-1">{{ phase.notes }}</div>
                      </div>
                    </div>
                    <span class="text-xs font-medium px-2 py-0.5 rounded-full flex-shrink-0" :class="resultClass(phase.result)">
                      {{ resultLabel(phase.result) }}
                    </span>
                  </div>
                </div>
              </div>

              <div v-else class="text-xs text-background-400 py-2">{{ t('products.noPhases') }}</div>
            </div>
          </div>
        </template>
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
import { ref, computed, onMounted, reactive, watch } from 'vue'
import { productService } from '@/services/productService'
import type { Product } from '@/services/productService'
import { carModelService } from '@/services/carModelService'
import type { CarModel } from '@/services/carModelService'
import { toast } from '@/plugins/toast'
import { useI18n } from 'vue-i18n'
import { EntityStatus, QualityStatus } from '@/constants/status'

const { t } = useI18n()

const loading = ref(true)
const products = ref<Product[]>([])
const total = ref(0)
const currentPage = ref(1)
const pageSize = ref(25)

const search = ref('')
const modelFilter = ref<number | ''>('')
const dateFrom = ref('')
const dateTo = ref('')
const carModels = ref<CarModel[]>([])

const expandedProductId = ref<number | null>(null)
const timelineByProduct = reactive<Record<number, any>>({})
const loadingTimeline = reactive<Record<number, boolean>>({})

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
    loadProducts()
  }, 300)
})

onMounted(async () => {
  await Promise.all([loadProducts(), loadCarModels()])
})

async function loadCarModels() {
  try {
    const res = await carModelService.getAll()
    carModels.value = res.data
  } catch {
    // silencioso — não bloqueia a página se a lista de modelos falhar
  }
}

async function loadProducts() {
  loading.value = true
  try {
    const res = await productService.getPaged({
      page: currentPage.value,
      pageSize: pageSize.value,
      search: search.value || undefined,
      modelId: modelFilter.value || undefined,
      dateFrom: dateFrom.value || undefined,
      dateTo: dateTo.value || undefined,
    })
    products.value = res.data.data
    total.value = res.data.total
  } catch {
    toast.error(t('errors.loadFailed'))
  } finally {
    loading.value = false
  }
}

function onModelChange() {
  currentPage.value = 1
  loadProducts()
}

function onPageSizeChange() {
  currentPage.value = 1
  loadProducts()
}

function goToPage(page: number) {
  if (page < 1 || page > totalPages.value) return
  currentPage.value = page
  loadProducts()
}

function clearFilters() {
  search.value = ''
  modelFilter.value = ''
  dateFrom.value = ''
  dateTo.value = ''
  currentPage.value = 1
  loadProducts()
}

async function toggleProduct(product: Product) {
  if (expandedProductId.value === product.id) {
    expandedProductId.value = null
    return
  }
  expandedProductId.value = product.id
  if (!timelineByProduct[product.id]) {
    loadingTimeline[product.id] = true
    try {
      const res = await productService.getTimeline(product.id)
      timelineByProduct[product.id] = {
        ...res.data,
        phases: (res.data.phases as any)?.$values ?? res.data.phases ?? [],
      }
    } catch (error: any) {
      timelineByProduct[product.id] = { status: EntityStatus.Unknown, phases: [] }
      if (error?.response?.status !== 400) {
        toast.error(t('products.loadTimelineFailed'))
      }
    } finally {
      loadingTimeline[product.id] = false
    }
  }
}

function formatDate(date: string) {
  return new Date(date).toLocaleDateString('pt-PT')
}

function formatDateTime(date: string) {
  return new Date(date).toLocaleString('pt-PT', { dateStyle: 'short', timeStyle: 'short' })
}

function formatDuration(seconds: number | null, endedAt: string | null) {
  if (!endedAt || seconds === null || seconds === undefined) return ''
  const minutes = Math.floor(seconds / 60)
  const remainingSeconds = seconds % 60
  return `${minutes}m ${remainingSeconds}s`
}

function phaseCountLabel(count: number) {
  return count === 1 ? '1 fase registada' : `${count} fases registadas`
}

function statusLabel(status: string | null) {
  switch (status) {
    case EntityStatus.Completed: return t('products.status.completed')
    case EntityStatus.InProgress: return t('products.status.inProgress')
    default: return t('products.status.unknown')
  }
}

function statusClass(status: string | null) {
  switch (status) {
    case EntityStatus.Completed: return 'bg-success-100 text-success-700'
    case EntityStatus.InProgress: return 'bg-primary-50 text-primary-600'
    default: return 'bg-background-200 dark:bg-background-700 text-background-500 dark:text-background-300'
  }
}

function resultLabel(result: string | null) {
  switch (result) {
    case QualityStatus.Passed: return t('products.result.passed')
    case QualityStatus.Rework: return t('products.result.rework')
    case QualityStatus.Scrapped: return t('products.result.scrapped')
    case EntityStatus.Completed: return t('products.result.completed')
    default: return t('products.result.inProgress')
  }
}

function resultClass(result: string | null) {
  switch (result) {
    case QualityStatus.Passed:
    case EntityStatus.Completed: return 'bg-success-100 text-success-700'
    case QualityStatus.Scrapped: return 'bg-danger-100 text-danger-700'
    case QualityStatus.Rework: return 'bg-warning-100 text-warning-700'
    default: return 'bg-warning-100 text-warning-700'
  }
}
</script>