<template>
  <div class="px-8 py-8 w-full">
    <!-- Voltar -->
    <button
      class="flex items-center gap-1.5 text-sm text-background-500 dark:text-background-400 hover:text-primary-500 dark:hover:text-primary-400 mb-6 transition-colors"
      @click="router.push('/client/orders')"
    >
      <span class="material-symbols-rounded text-base">arrow_back</span>
      {{ t('client.detail.back') }}
    </button>

    <!-- Loading -->
    <div v-if="loading" class="flex justify-center py-20">
      <div class="animate-spin rounded-full h-9 w-9 border-2 border-primary-200 border-t-primary-500" />
    </div>

    <template v-else-if="detail">
      <!-- Header da encomenda -->
      <div class="mb-7">
        <h1 class="text-2xl font-semibold text-background-900 dark:text-background-50">
          {{ t('client.detail.title') }} {{ detail.orderNumber }}
        </h1>
        <div class="flex items-center gap-4 mt-1.5 text-sm text-background-500 dark:text-background-400">
          <span class="flex items-center gap-1.5">
            <span class="material-symbols-rounded text-base">event</span>
            {{ formatDate(detail.orderDate) }}
          </span>
          <span class="flex items-center gap-1.5">
            <span class="material-symbols-rounded text-base">directions_car</span>
            {{ detail.products.length }} {{ t('client.detail.cars') }}
          </span>
        </div>
      </div>

      <!-- Lista de carros -->
      <div class="space-y-4">
        <div
          v-for="car in detail.products"
          :key="car.productId"
          class="bg-background-50 dark:bg-background-900 rounded-2xl border border-background-200 dark:border-background-800 p-5"
        >
          <div class="flex items-start justify-between flex-wrap gap-3 mb-5">
            <!-- VIN + modelo -->
            <div class="flex items-center gap-2.5">
              <span class="h-9 w-9 rounded-xl bg-primary-50 dark:bg-primary-900/30 flex items-center justify-center shrink-0">
                <span class="material-symbols-rounded text-primary-500 text-lg">directions_car</span>
              </span>
              <div>
                <p class="text-xs font-mono text-background-500 dark:text-background-400">{{ car.serialNumber }}</p>
                <p v-if="car.modelName" class="text-sm font-medium text-background-800 dark:text-background-100">{{ car.modelName }}</p>
              </div>
            </div>

            <!-- Badge de estado + previsão -->
            <div class="text-right">
              <span
                class="inline-flex items-center gap-1 text-[11px] font-semibold uppercase tracking-wide px-2.5 py-1 rounded-full"
                :class="statusBadgeClass(car)"
              >
                {{ statusLabel(car) }}
              </span>
              <p class="text-xs text-background-500 dark:text-background-400 mt-1.5">
                <template v-if="car.isCompleted">
                  {{ t('client.detail.completed') }}
                </template>
                <template v-else-if="car.etaUtc">
                  <span :title="absoluteEtaDate(car.etaUtc)">
                    {{ t('client.orders.etaReady', { time: relativeEtaLabel(car.etaUtc, t) }) }}
                  </span>
                  <span v-if="car.etaIsMlPrediction" class="ml-1 text-primary-500 dark:text-primary-400 font-semibold" :title="t('client.detail.mlTooltip')">ML</span>
                </template>
                <template v-else>{{ t('client.detail.etaUnavailable') }}</template>
              </p>
            </div>
          </div>

          <!-- Linha de produção: as 5 fases reais, com checkmarks -->
          <div class="flex items-center">
            <template v-for="(phase, idx) in PHASES" :key="phase">
              <div class="flex flex-col items-center gap-1.5 flex-1 min-w-0">
                <div
                  class="h-7 w-7 rounded-full flex items-center justify-center shrink-0 transition-colors"
                  :class="stepState(car, idx) === 'done'
                    ? 'bg-success-500 text-white'
                    : stepState(car, idx) === 'current'
                      ? 'bg-primary-500 text-white ring-4 ring-primary-100 dark:ring-primary-900/40'
                      : 'bg-background-200 dark:bg-background-800 text-background-400 dark:text-background-600'"
                >
                  <span v-if="stepState(car, idx) === 'done'" class="material-symbols-rounded text-base">check</span>
                  <span v-else class="h-2 w-2 rounded-full bg-current" />
                </div>
                <span
                  class="text-[10px] text-center leading-tight truncate w-full"
                  :class="stepState(car, idx) === 'current'
                    ? 'text-primary-600 dark:text-primary-400 font-semibold'
                    : 'text-background-400 dark:text-background-500'"
                >
                  {{ phase }}
                </span>
              </div>
              <div
                v-if="idx < PHASES.length - 1"
                class="h-0.5 flex-1 -mt-4 transition-colors"
                :class="stepState(car, idx) === 'done' ? 'bg-success-500' : 'bg-background-200 dark:bg-background-700'"
              />
            </template>
          </div>
        </div>
      </div>
    </template>

    <!-- Não encontrado -->
    <div v-else class="text-center py-20 text-background-400 dark:text-background-500">
      {{ t('client.detail.notFound') }}
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import 'material-symbols'
import { clientPortalService, type ClientOrderDetail, type ClientProduct } from '@/services/clientPortalService'
import { relativeEtaLabel, absoluteEtaDate } from '@/utils/clientEta'
import { toast } from '@/plugins/toast'

const { t } = useI18n()
const route = useRoute()
const router = useRouter()

// As 5 fases reais do processo de fabrico (Estampagem → Soldadura → Pintura →
// Montagem → Inspeção), pela ordem documentada para o caso de estudo Toyota.
const PHASES = ['Estampagem', 'Soldadura', 'Pintura', 'Montagem', 'Inspeção']

const detail = ref<ClientOrderDetail | null>(null)
const loading = ref(true)

onMounted(async () => {
  const id = Number(route.params.id)
  try {
    detail.value = await clientPortalService.getOrderDetail(id)
  } catch {
    toast.error(t('errors.loadFailed'))
  } finally {
    loading.value = false
  }
})

function hasStarted(car: ClientProduct): boolean {
  return PHASES.some(p => p.toLowerCase() === car.currentPhase?.trim().toLowerCase())
}

function stepState(car: ClientProduct, stepIdx: number): 'done' | 'current' | 'pending' {
  if (car.isCompleted) return 'done'

  const currentIdx = PHASES.findIndex(
    p => p.toLowerCase() === car.currentPhase?.trim().toLowerCase()
  )
  if (currentIdx === -1) return 'pending'

  if (stepIdx < currentIdx) return 'done'
  if (stepIdx === currentIdx) return 'current'
  return 'pending'
}

function statusLabel(car: ClientProduct): string {
  if (car.isCompleted) return t('client.detail.status.completed')
  if (!hasStarted(car)) return t('client.detail.status.queued')
  return t('client.detail.status.inProgress')
}

function statusBadgeClass(car: ClientProduct): string {
  if (car.isCompleted) return 'bg-success-100 text-success-700'
  if (!hasStarted(car)) return 'bg-background-200 text-background-500 dark:bg-background-800 dark:text-background-400'
  return 'bg-primary-100 text-primary-700'
}

function formatDate(iso: string) {
  return new Date(iso).toLocaleDateString('pt-PT', { day: '2-digit', month: 'long', year: 'numeric' })
}
</script>