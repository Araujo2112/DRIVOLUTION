<template>
  <div class="p-8 w-full">

    <!-- Header -->
    <div class="flex items-start justify-between mb-8">
      <div>
        <h1 class="text-2xl font-medium text-background-900 dark:text-background-50">
          {{ t('products.title') }}
        </h1>
        <p class="text-sm text-background-600 dark:text-background-400 mt-1">
          {{ t('products.subtitle') }}
        </p>
      </div>
    </div>

    <!-- Loading -->
    <div v-if="loading" class="flex items-center gap-2 text-background-500 text-sm py-12">
      <span class="material-symbols-rounded animate-spin text-lg">autorenew</span>
      {{ t('common.loading') }}
    </div>

    <div v-else>
      <!-- Empty -->
      <div v-if="products.length === 0" class="text-center py-16 text-background-500">
        <span class="material-symbols-rounded text-5xl block mb-3">directions_car</span>
        <p class="text-sm">{{ t('products.empty') }}</p>
      </div>

      <!-- Tabela -->
      <div v-else class="border border-background-300 dark:border-background-700 rounded-xl overflow-hidden">
        <!-- Header -->
        <div class="grid grid-cols-5 px-4 py-3 bg-background-100 dark:bg-background-800 border-b border-background-300 dark:border-background-700">
          <span class="text-xs font-medium text-background-500 uppercase tracking-wider col-span-2">{{ t('products.fields.serial') }}</span>
          <span class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('products.fields.model') }}</span>
          <span class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('products.fields.lot') }}</span>
          <span class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('products.fields.productionDate') }}</span>
        </div>

        <!-- Rows -->
        <template v-for="product in products" :key="product.id">
          <div
            class="grid grid-cols-5 px-4 py-3 border-b border-background-200 dark:border-background-700 bg-background-50 dark:bg-background-800 hover:bg-background-100 dark:hover:bg-background-750 transition-colors items-center cursor-pointer"
            @click="toggleProduct(product)"
          >
            <div class="col-span-2 flex items-center gap-2">
              <span
                class="material-symbols-rounded text-background-400 text-base transition-transform duration-200"
                :class="expandedProductId === product.id ? 'rotate-180' : ''"
              >expand_more</span>
              <div>
                <div class="text-sm font-medium text-background-900 dark:text-background-50">{{ product.serialNumber }}</div>
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
            class="border-b border-background-200 dark:border-background-700 bg-background-100 dark:bg-background-750 px-8 py-4"
          >
            <div v-if="loadingTimeline[product.id]" class="flex items-center gap-2 text-background-400 text-xs py-2">
              <span class="material-symbols-rounded animate-spin text-sm">autorenew</span>
              {{ t('common.loading') }}
            </div>

            <div v-else-if="timelineByProduct[product.id]">
              <!-- Status do produto -->
              <div class="flex items-center gap-2 mb-4">
                <span class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('products.timeline') }}</span>
                <span
                  class="text-xs font-medium px-2 py-0.5 rounded-full"
                  :class="timelineByProduct[product.id].status === 'completed' ? 'bg-success-100 text-success-700' : 'bg-primary-50 text-primary-600'"
                >
                  {{ timelineByProduct[product.id].status === 'completed' ? t('timeline.status.completed') : t('timeline.status.inProgress') }}
                </span>
              </div>

              <!-- Fases -->
              <div v-if="timelineByProduct[product.id].phases?.length" class="flex flex-col gap-2">
                <div
                  v-for="phase in timelineByProduct[product.id].phases"
                  :key="phase.productPhaseId"
                  class="bg-background-50 dark:bg-background-800 border border-background-300 dark:border-background-700 rounded-lg px-4 py-3"
                >
                  <div class="grid grid-cols-5 items-center gap-4">
                    <div class="flex items-center gap-2 col-span-2">
                      <div class="w-2 h-2 rounded-full flex-shrink-0" :class="phase.endedAt ? 'bg-success-500' : 'bg-primary-500 animate-pulse'"></div>
                      <span class="text-sm font-medium text-background-900 dark:text-background-50">{{ phase.phaseName }}</span>
                    </div>
                    <span class="text-xs text-background-500">{{ phase.workstation }}</span>
                    <span class="text-xs text-background-500">{{ formatDateTime(phase.startedAt) }}</span>
                    <div class="flex items-center gap-2">
                      <span class="text-xs text-background-500">{{ formatDuration(phase.durationSeconds) }}</span>
                      <span
                        v-if="phase.result"
                        class="text-xs font-medium px-2 py-0.5 rounded-full"
                        :class="phase.result === 'passed' ? 'bg-success-100 text-success-700' : phase.result === 'failed_scrapped' ? 'bg-danger-100 text-danger-700' : 'bg-warning-100 text-warning-700'"
                      >
                        {{ resultLabel(phase.result) }}
                      </span>
                      <span v-else class="text-xs text-background-400">{{ t('timeline.inProgress') }}</span>
                    </div>
                  </div>
                  <p v-if="phase.notes" class="text-xs text-background-400 mt-2 pl-4">{{ phase.notes }}</p>
                </div>
              </div>

              <!-- Sem fases ainda -->
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
    products.value = res.data
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
        phases: res.data.phases
      }
    } catch {
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

function formatDuration(seconds: number | null) {
  if (seconds === null || seconds === undefined) return '—'
  const minutes = Math.floor(seconds / 60)
  const remainingSeconds = seconds % 60
  return `${minutes}m ${remainingSeconds}s`
}

function resultLabel(result: string | null) {
  switch (result) {
    case 'passed': return t('timeline.result.passed')
    case 'failed_rework': return t('timeline.result.rework')
    case 'failed_scrapped': return t('timeline.result.scrapped')
    default: return result || '—'
  }
}
</script>