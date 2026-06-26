<template>
  <div class="p-8 w-full">

    <!-- Header -->
    <div class="flex items-start justify-between mb-8">
      <div>
        <h1 class="text-2xl font-medium text-background-900 dark:text-background-50">
          {{ t('lineStatus.title') }}
        </h1>
        <p class="text-sm text-background-600 dark:text-background-400 mt-1">
          {{ t('lineStatus.subtitle') }}
        </p>
      </div>
      <button
        @click="loadStatus"
        class="flex items-center gap-2 bg-primary-500 hover:bg-primary-600 text-white text-sm font-medium px-4 py-2 rounded-lg transition-colors"
      >
        <span class="material-symbols-rounded text-base">refresh</span>
        {{ t('common.refresh') }}
      </button>
    </div>

    <div v-if="loading" class="flex items-center gap-2 text-background-500 text-sm py-12">
      <span class="material-symbols-rounded animate-spin text-lg">autorenew</span>
      {{ t('common.loading') }}
    </div>

    <div v-else>
      <!-- KPI Cards -->
      <div class="grid grid-cols-4 gap-4 mb-8">
        <div class="bg-background-50 dark:bg-background-800 border border-background-300 dark:border-background-700 rounded-xl p-5">
          <p class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('lineStatus.activeLines') }}</p>
          <p class="text-3xl font-medium text-success-500 mt-2">{{ activeLines }}</p>
        </div>
        <div class="bg-background-50 dark:bg-background-800 border border-background-300 dark:border-background-700 rounded-xl p-5">
          <p class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('lineStatus.inProduction') }}</p>
          <p class="text-3xl font-medium text-primary-500 mt-2">{{ productsInProduction }}</p>
        </div>
        <div class="bg-background-50 dark:bg-background-800 border border-background-300 dark:border-background-700 rounded-xl p-5">
          <p class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('lineStatus.freeStations') }}</p>
          <p class="text-3xl font-medium text-background-600 dark:text-background-300 mt-2">{{ freeStations }}</p>
        </div>
        <div
          class="bg-background-50 dark:bg-background-800 border rounded-xl p-5"
          :class="lateProducts > 0 ? 'border-danger-300 dark:border-danger-700' : 'border-background-300 dark:border-background-700'"
        >
          <p class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('lineStatus.lateProducts') }}</p>
          <p class="text-3xl font-medium mt-2" :class="lateProducts > 0 ? 'text-danger-500' : 'text-background-600 dark:text-background-300'">
            {{ lateProducts }}
          </p>
        </div>
      </div>

      <!-- Tabela -->
      <div class="border border-background-300 dark:border-background-700 rounded-xl overflow-hidden">
        <div class="grid grid-cols-8 px-4 py-3 bg-background-100 dark:bg-background-800 border-b border-background-300 dark:border-background-700">
          <span class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('lineStatus.fields.line') }}</span>
          <span class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('lineStatus.fields.workstation') }}</span>
          <span class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('lineStatus.fields.product') }}</span>
          <span class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('lineStatus.fields.phase') }}</span>
          <span class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('lineStatus.fields.status') }}</span>
          <span class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('lineStatus.fields.start') }}</span>
          <span class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('lineStatus.fields.estimatedEnd') }}</span>
          <span class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('lineStatus.fields.remaining') }}</span>
        </div>

        <div v-if="status.length === 0" class="text-center py-12 text-background-500">
          <span class="material-symbols-rounded text-4xl block mb-2">factory</span>
          <p class="text-sm">{{ t('lineStatus.empty') }}</p>
        </div>

        <div
          v-for="item in status"
          :key="item.workstationId"
          :class="[
            'grid grid-cols-8 px-4 py-3 border-b border-background-200 dark:border-background-700 last:border-0 items-center transition-colors',
            isLate(item)
              ? 'bg-danger-50 dark:bg-danger-950 hover:bg-danger-100 dark:hover:bg-danger-900'
              : 'bg-background-50 dark:bg-background-800 hover:bg-background-100 dark:hover:bg-background-750'
          ]"
        >
          <span class="text-sm font-medium text-background-900 dark:text-background-50">{{ item.productionLineName }}</span>
          <span class="text-sm text-background-600 dark:text-background-400">{{ item.workstationType }}</span>
          <span class="text-sm text-background-600 dark:text-background-400">{{ item.serialNumber || '—' }}</span>
          <span class="text-sm text-background-600 dark:text-background-400">{{ item.currentPhase || '—' }}</span>
          <div>
            <span
              class="text-xs font-medium px-2 py-1 rounded-full"
              :class="statusClass(item)"
            >
              {{ statusText(item) }}
            </span>
          </div>
          <span class="text-sm text-background-500">{{ formatDate(item.startedAt) }}</span>
          <span class="text-sm text-background-500">{{ formatDate(item.estimatedFinish) }}</span>
          <span class="text-sm font-medium" :class="isLate(item) ? 'text-danger-600 dark:text-danger-400' : 'text-background-700 dark:text-background-300'">
            {{ getRemainingTime(item.estimatedFinish) }}
          </span>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref } from 'vue'
import axios from '@/axios'
import { useI18n } from 'vue-i18n'
import { toast } from '@/plugins/toast'

const { t } = useI18n()

type ProductionLineStatus = {
  productionLineId: number
  productionLineName: string
  location: string
  lineStatus: string
  workstationId: number
  workstationType: string
  productId: number | null
  serialNumber: string | null
  currentPhase: string | null
  startedAt: string | null
  estimatedFinish: string | null
  productStatus: string | null
}

const status = ref<ProductionLineStatus[]>([])
const loading = ref(false)

// ── Live ticking ──────────────────────────────────────────────────────────────
// Sem isto, o Vue não tem motivo para voltar a chamar getRemainingTime() —
// 'status' só muda quando há novo fetch, por isso o remaining time ficava
// congelado no valor calculado no momento da carga inicial.
const nowTick = ref(Date.now())
let tickInterval: ReturnType<typeof setInterval> | null = null

function startTicking() {
  if (tickInterval) return
  tickInterval = setInterval(() => { nowTick.value = Date.now() }, 1000)
}

function stopTicking() {
  if (tickInterval) { clearInterval(tickInterval); tickInterval = null }
}

async function loadStatus() {
  loading.value = true
  try {
    const response = await axios.get('/production-lines/status')
    status.value = response.data?.$values ?? response.data ?? []
  } catch {
    toast.error(t('errors.loadFailed'))
  } finally {
    loading.value = false
  }
}

function formatDate(value: string | null) {
  if (!value) return '—'
  const normalized = value.endsWith('Z') ? value : value + 'Z'
  return new Date(normalized).toLocaleString('pt-PT')
}

function isLate(item: ProductionLineStatus) {
  if (!item.productId || !item.estimatedFinish) return false
  const normalized = item.estimatedFinish.endsWith('Z') ? item.estimatedFinish : item.estimatedFinish + 'Z'
  return new Date(normalized).getTime() < nowTick.value
}

function getRemainingTime(date: string | null) {
  if (!date) return '—'

  const normalized = date.endsWith('Z') ? date : date + 'Z'
  const diff = new Date(normalized).getTime() - nowTick.value
  const seconds = Math.abs(Math.floor(diff / 1000))
  const minutes = Math.floor(seconds / 60)

  const value = minutes > 0
    ? `${minutes}m ${seconds % 60}s`
    : `${seconds}s`

  if (diff < 0) {
    return `${t('lineStatus.late')} • ${value}`
  }

  return value
}

function statusText(item: ProductionLineStatus) {
  if (!item.productId) return t('lineStatus.status.free')
  if (isLate(item)) return t('lineStatus.status.late')
  return t('lineStatus.status.inProduction')
}

function statusClass(item: ProductionLineStatus) {
  if (!item.productId) return 'bg-background-200 dark:bg-background-700 text-background-600 dark:text-background-400'
  if (isLate(item)) return 'bg-danger-100 text-danger-700 dark:bg-danger-900 dark:text-danger-200'
  return 'bg-success-100 text-success-700 dark:bg-success-900 dark:text-success-200'
}

const activeLines = computed(() =>
  new Set(status.value.filter(x => x.productId).map(x => x.productionLineId)).size
)

const productsInProduction = computed(() =>
  new Set(status.value.filter(x => x.productId).map(x => x.productId)).size
)

const freeStations = computed(() =>
  status.value.filter(x => !x.productId).length
)

const lateProducts = computed(() =>
  status.value.filter(x => x.productId && isLate(x)).length
)

onMounted(() => {
  loadStatus()
  startTicking()
})

onUnmounted(() => {
  stopTicking()
})
</script>