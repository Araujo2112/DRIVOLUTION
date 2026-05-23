<template>
  <div class="p-8 w-full">
    <div class="flex items-start justify-between mb-8">
      <div>
        <h1 class="text-2xl font-medium text-background-900 dark:text-background-50">
          {{ t('products.title') }}
        </h1>
        <p class="text-sm text-background-600 dark:text-background-400 mt-1">
          {{ t('products.subtitle') }}
        </p>
      </div>

      <button
        @click="loadProducts"
        class="flex items-center gap-2 bg-primary-500 hover:bg-primary-600 text-white text-sm font-medium px-4 py-2 rounded-lg transition-colors"
      >
        <span class="material-symbols-rounded text-base">refresh</span>
        {{ t('common.refresh') }}
      </button>
    </div>

    <!-- Loading -->
    <div v-if="loading" class="flex items-center gap-2 text-background-500 text-sm py-12">
      <span class="material-symbols-rounded animate-spin text-lg">autorenew</span>
      {{ t('common.loading') }}
    </div>

    <div v-else>
      <!-- Empty -->
      <div
        v-if="products.length === 0"
        class="border border-background-300 dark:border-background-700 rounded-xl bg-background-50 dark:bg-background-800 text-center py-16 text-background-500"
      >
        <span class="material-symbols-rounded text-5xl block mb-3">directions_car</span>
        <p class="text-sm">{{ t('products.empty') }}</p>
      </div>

      <!-- Tabela -->
      <div v-else class="border border-background-300 dark:border-background-700 rounded-xl overflow-hidden">
        <!-- Header -->
        <div class="grid grid-cols-5 px-5 py-3 bg-background-100 dark:bg-background-800 border-b border-background-300 dark:border-background-700">
          <span class="text-xs font-medium text-background-500 uppercase tracking-wider col-span-2">{{ t('products.fields.serial') }}</span>
          <span class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('products.fields.model') }}</span>
          <span class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('products.fields.lot') }}</span>
          <span class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('products.fields.productionDate') }}</span>
        </div>

        <template v-for="product in products" :key="product.id">
          <!-- Row -->
          <div
            class="grid grid-cols-5 px-5 py-4 border-b border-background-200 dark:border-background-700 bg-background-50 dark:bg-background-800 hover:bg-background-100 dark:hover:bg-background-750 transition-colors items-center cursor-pointer"
            @click="toggleProduct(product)"
          >
            <div class="col-span-2 flex items-center gap-3">
              <span
                class="material-symbols-rounded text-background-400 text-lg transition-transform duration-200"
                :class="expandedProductId === product.id ? 'rotate-180' : ''"
              >expand_more</span>
              <div>
                <div class="text-sm font-medium text-background-900 dark:text-background-50">
                  {{ product.serialNumber || '—' }}
                </div>
                <div class="text-xs text-background-400 mt-0.5">ID #{{ product.id }}</div>
              </div>
            </div>
            <span class="text-sm text-background-700 dark:text-background-300">{{ product.modelName ?? '—' }}</span>
            <span class="text-sm text-background-500">{{ product.lotNumber ?? '—' }}</span>
            <span class="text-sm text-background-500">{{ product.productionDate ? formatDate(product.productionDate) : '—' }}</span>
          </div>

          <!-- Expand: Timeline -->
          <div
            v-if="expandedProductId === product.id"
            class="border-b border-background-200 dark:border-background-700 bg-background-100 dark:bg-background-750 px-5 py-4"
          >
            <div v-if="loadingTimeline[product.id]" class="flex items-center gap-2 text-background-400 text-xs py-2">
              <span class="material-symbols-rounded animate-spin text-sm">autorenew</span>
              {{ t('common.loading') }}
            </div>

            <div v-else-if="timelineByProduct[product.id]">
              <!-- Status -->
              <div class="flex items-center gap-2 mb-3">
                <span class="text-xs font-medium text-background-500 uppercase tracking-wider">
                  {{ t('products.timeline') }}
                </span>
                <span
                  class="text-xs font-medium px-2 py-0.5 rounded-full"
                  :class="timelineByProduct[product.id].status === 'completed'
                    ? 'bg-success-100 text-success-700'
                    : timelineByProduct[product.id].status === 'unknown'
                      ? 'bg-background-200 dark:bg-background-700 text-background-500'
                      : 'bg-primary-50 text-primary-600'"
                >
                  {{ statusLabel(timelineByProduct[product.id].status) }}
                </span>
              </div>

              <!-- Fases -->
              <div v-if="timelineByProduct[product.id].phases?.length" class="flex flex-col gap-2">
                <div
                  v-for="phase in timelineByProduct[product.id].phases"
                  :key="phase.productPhaseId"
                  class="bg-background-50 dark:bg-background-800 border border-background-300 dark:border-background-700 rounded-xl px-4 py-3"
                >
                  <div class="flex items-center justify-between gap-4">
                    <div class="flex items-start gap-3 min-w-0">
                      <div
                        class="w-2.5 h-2.5 rounded-full flex-shrink-0 mt-1.5"
                        :class="phase.endedAt ? 'bg-success-500' : 'bg-primary-500 animate-pulse'"
                      />
                      <div class="min-w-0">
                        <div class="flex items-center gap-2">
                          <span class="material-symbols-rounded text-primary-500 text-base">precision_manufacturing</span>
                          <span class="text-sm font-medium text-background-900 dark:text-background-50">{{ phase.phaseName }}</span>
                        </div>
                        <div class="text-xs text-background-500 mt-1">{{ phase.workstation || '—' }}</div>
                        <div class="text-xs text-background-400">{{ formatDateTime(phase.startedAt) }}</div>
                        <div v-if="phase.notes" class="text-xs text-background-400 mt-1">{{ phase.notes }}</div>
                      </div>
                    </div>

                    <div class="flex items-center gap-2 flex-shrink-0">
                      <span v-if="formatDuration(phase.durationSeconds, phase.endedAt)" class="text-xs text-background-500">
                        {{ formatDuration(phase.durationSeconds, phase.endedAt) }}
                      </span>
                      <span class="text-xs font-medium px-2 py-0.5 rounded-full" :class="resultClass(phase.result)">
                        {{ resultLabel(phase.result) }}
                      </span>
                    </div>
                  </div>
                </div>
              </div>

              <!-- Sem fases -->
              <div v-else class="text-xs text-background-400 py-2">
                {{ t('products.noPhases') }}
              </div>
            </div>
          </div>
        </template>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, reactive } from 'vue'
import { productService } from '@/services/productService'
import type { Product } from '@/services/productService'
import { toast } from '@/plugins/toast'
import { useI18n } from 'vue-i18n'

const { t } = useI18n()

const loading = ref(true)
const products = ref<Product[]>([])
const expandedProductId = ref<number | null>(null)
const timelineByProduct = reactive<Record<number, any>>({})
const loadingTimeline = reactive<Record<number, boolean>>({})

onMounted(async () => {
  await loadProducts()
})

async function loadProducts() {
  loading.value = true
  try {
    const res = await productService.getAll()
    products.value = (res.data as any)?.$values ?? res.data ?? []
  } catch {
    toast.error(t('errors.loadFailed'))
  } finally {
    loading.value = false
  }
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
    } catch {
      timelineByProduct[product.id] = { status: 'unknown', phases: [] }
      toast.error(t('errors.loadFailed'))
    } finally {
      loadingTimeline[product.id] = false
    }
  }
}

function formatDate(date: string) {
  return new Date(date).toLocaleDateString('pt-PT')
}

function formatDateTime(date: string) {
  return new Date(date).toLocaleString('pt-PT')
}

function formatDuration(seconds: number | null, endedAt: string | null) {
  if (!endedAt) return ''
  if (seconds === null || seconds === undefined) return '—'
  const minutes = Math.floor(seconds / 60)
  const remainingSeconds = seconds % 60
  return `${minutes}m ${remainingSeconds}s`
}

function statusLabel(status: string | null) {
  switch (status) {
    case 'completed': return t('timeline.status.completed')
    case 'in_progress': return t('timeline.status.inProgress')
    default: return t('mo.status.unknown')
  }
}

function resultLabel(result: string | null) {
  switch (result) {
    case 'passed': return t('timeline.result.passed')
    case 'failed_rework': return t('timeline.result.rework')
    case 'failed_scrapped': return t('timeline.result.scrapped')
    default: return t('timeline.inProgress')
  }
}

function resultClass(result: string | null) {
  switch (result) {
    case 'passed': return 'bg-success-100 text-success-700'
    case 'failed_scrapped': return 'bg-danger-100 text-danger-700'
    case 'failed_rework': return 'bg-warning-100 text-warning-700'
    default: return 'bg-warning-100 text-warning-700'
  }
}
</script>
