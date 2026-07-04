<template>
  <div class="px-8 py-8">
    <div class="mb-6">
      <h1 class="text-2xl font-semibold text-background-900 dark:text-background-50">
        {{ t('client.myVehicles.title') }}
      </h1>
      <p class="text-sm text-background-500 dark:text-background-400 mt-1">
        {{ t('client.myVehicles.subtitle') }}
      </p>
    </div>

    <!-- Loading -->
    <div v-if="loading" class="flex items-center justify-center py-24">
      <div class="animate-spin rounded-full h-9 w-9 border-2 border-primary-200 border-t-primary-500" />
    </div>

    <!-- Empty -->
    <div v-else-if="vehicles.length === 0" class="flex flex-col items-center justify-center text-center text-background-400 dark:text-background-500 py-24">
      <span class="material-symbols-rounded text-6xl mb-4 text-background-300 dark:text-background-700">directions_car</span>
      <p class="text-base">{{ t('client.myVehicles.empty') }}</p>
    </div>

    <template v-else>
      <!-- Filtros -->
      <div class="flex flex-wrap gap-3 mb-6">
        <div class="relative flex-1 min-w-[220px] max-w-xs">
          <span class="material-symbols-rounded absolute left-3 top-1/2 -translate-y-1/2 text-background-400 text-lg">search</span>
          <input
            v-model="search"
            type="text"
            :placeholder="t('client.myVehicles.searchPlaceholder')"
            class="w-full pl-9 pr-3 py-2.5 text-sm rounded-xl border border-background-200 dark:border-background-700 bg-background-50 dark:bg-background-900 text-background-700 dark:text-background-300"
          />
        </div>
        <select
          v-model="modelFilter"
          class="text-sm rounded-xl border border-background-200 dark:border-background-700 bg-background-50 dark:bg-background-900 px-3 py-2.5 text-background-700 dark:text-background-300"
        >
          <option value="">{{ t('client.myVehicles.allModels') }}</option>
          <option v-for="m in distinctModels" :key="m" :value="m">{{ m }}</option>
        </select>
      </div>

      <!-- Grelha de veículos -->
      <div v-if="filteredVehicles.length === 0" class="text-center py-16 text-sm text-background-400 dark:text-background-500">
        {{ t('client.myVehicles.noResults') }}
      </div>

      <div v-else class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
        <RouterLink
          v-for="v in filteredVehicles"
          :key="v.productId"
          :to="`/client/orders/${v.orderId}`"
          class="bg-background-50 dark:bg-background-900 border border-background-200 dark:border-background-800 rounded-2xl p-5 hover:border-primary-400 dark:hover:border-primary-500 hover:shadow-sm transition-all"
        >
          <div class="flex items-start justify-between mb-4">
            <span class="h-10 w-10 rounded-xl bg-primary-50 dark:bg-primary-900/30 flex items-center justify-center shrink-0">
              <span class="material-symbols-rounded text-primary-500 text-xl">directions_car</span>
            </span>
            <span
              class="text-[11px] font-semibold uppercase tracking-wide px-2.5 py-1 rounded-full"
              :class="v.isCompleted
                ? 'bg-success-100 text-success-700'
                : hasStarted(v)
                  ? 'bg-primary-100 text-primary-700'
                  : 'bg-background-200 text-background-500 dark:bg-background-800 dark:text-background-400'"
            >
              {{ v.isCompleted ? t('client.detail.status.completed') : hasStarted(v) ? t('client.detail.status.inProgress') : t('client.detail.status.queued') }}
            </span>
          </div>

          <p class="text-sm font-semibold text-background-900 dark:text-background-50">{{ v.modelName || t('client.myVehicles.unknownModel') }}</p>
          <p class="text-xs font-mono text-background-500 dark:text-background-400 mt-0.5">{{ v.serialNumber }}</p>

          <div class="flex items-center justify-between mt-4 pt-4 border-t border-background-100 dark:border-background-800">
            <span class="text-xs text-background-500 dark:text-background-400">{{ v.orderNumber }}</span>
            <span class="text-xs font-medium text-primary-500">
              {{ t('client.myVehicles.viewOrder') }}
            </span>
          </div>
        </RouterLink>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useI18n } from 'vue-i18n'
import 'material-symbols'
import { clientPortalService, type ClientVehicle } from '@/services/clientPortalService'

const { t } = useI18n()

const PHASES = ['Estampagem', 'Soldadura', 'Pintura', 'Montagem', 'Inspeção']

const vehicles = ref<ClientVehicle[]>([])
const loading = ref(true)
const search = ref('')
const modelFilter = ref('')

onMounted(async () => {
  // Sem endpoint dedicado a "todos os veículos do cliente" — agregado no
  // service a partir de getMyOrders() + getOrderDetail() por encomenda.
  // Ver comentário em clientPortalService.ts (N+1, aceitável para já).
  try {
    vehicles.value = await clientPortalService.getMyVehicles()
  } finally {
    loading.value = false
  }
})

function hasStarted(v: ClientVehicle) {
  return PHASES.some(p => p.toLowerCase() === v.currentPhase?.trim().toLowerCase())
}

const distinctModels = computed(() =>
  [...new Set(vehicles.value.map(v => v.modelName).filter(Boolean))].sort()
)

const filteredVehicles = computed(() => {
  const q = search.value.trim().toLowerCase()
  return vehicles.value.filter(v => {
    if (modelFilter.value && v.modelName !== modelFilter.value) return false
    if (q && !`${v.serialNumber} ${v.modelName} ${v.orderNumber}`.toLowerCase().includes(q)) return false
    return true
  })
})
</script>
