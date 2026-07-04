<template>
  <div class="px-8 py-8">
    <div class="mb-6">
      <h1 class="text-2xl font-semibold text-background-900 dark:text-background-50">
        {{ t('client.dashboard.title') }}
      </h1>
      <p class="text-sm text-background-500 dark:text-background-400 mt-1">
        {{ t('client.dashboard.subtitle') }}
      </p>
    </div>

    <!-- Loading -->
    <div v-if="loading" class="flex items-center justify-center py-24">
      <div class="animate-spin rounded-full h-9 w-9 border-2 border-primary-200 border-t-primary-500" />
    </div>

    <template v-else>
      <!-- Cartões de resumo -->
      <div class="grid grid-cols-1 sm:grid-cols-3 gap-4 mb-6">
        <div class="bg-background-50 dark:bg-background-900 border border-background-200 dark:border-background-800 rounded-2xl p-5">
          <p class="text-xs font-medium text-background-500 dark:text-background-400 mb-2">{{ t('client.dashboard.total') }}</p>
          <p class="text-3xl font-semibold text-background-900 dark:text-background-50">{{ orders.length }}</p>
        </div>
        <div class="bg-background-50 dark:bg-background-900 border border-background-200 dark:border-background-800 rounded-2xl p-5">
          <p class="text-xs font-medium text-background-500 dark:text-background-400 mb-2">{{ t('client.dashboard.inProduction') }}</p>
          <p class="text-3xl font-semibold text-background-900 dark:text-background-50">{{ inProductionCount }}</p>
        </div>
        <div class="bg-background-50 dark:bg-background-900 border border-background-200 dark:border-background-800 rounded-2xl p-5">
          <p class="text-xs font-medium text-background-500 dark:text-background-400 mb-2">{{ t('client.dashboard.ready') }}</p>
          <p class="text-3xl font-semibold text-background-900 dark:text-background-50">{{ readyCount }}</p>
        </div>
      </div>

      <!-- Tabela: só as 5 mais avançadas, sem filtrar/exportar -->
      <div class="bg-background-50 dark:bg-background-900 border border-background-200 dark:border-background-800 rounded-2xl overflow-hidden">
        <div class="flex items-center justify-between px-5 py-4 border-b border-background-200 dark:border-background-800">
          <h2 class="text-sm font-semibold text-background-900 dark:text-background-50">
            {{ t('client.dashboard.details') }}
          </h2>
          <RouterLink
            to="/client/orders"
            class="text-sm font-medium text-primary-500 hover:text-primary-600 dark:hover:text-primary-400 flex items-center gap-1"
          >
            {{ t('client.dashboard.viewAll') }}
            <span class="material-symbols-rounded text-base">arrow_forward</span>
          </RouterLink>
        </div>

        <div v-if="topOrders.length === 0" class="px-5 py-10 text-center text-sm text-background-400 dark:text-background-500">
          {{ t('client.orders.empty') }}
        </div>

        <table v-else class="w-full text-sm">
          <thead>
            <tr class="text-left text-[11px] uppercase tracking-wider text-background-400 dark:text-background-500 border-b border-background-200 dark:border-background-800">
              <th class="px-5 py-3 font-medium">{{ t('client.dashboard.col.orderId') }}</th>
              <th class="px-5 py-3 font-medium">{{ t('client.dashboard.col.model') }}</th>
              <th class="px-5 py-3 font-medium">{{ t('client.dashboard.col.status') }}</th>
              <th class="px-5 py-3 font-medium">{{ t('client.dashboard.col.progress') }}</th>
              <th class="px-5 py-3 font-medium">{{ t('client.dashboard.col.eta') }}</th>
              <th class="px-5 py-3 font-medium text-right">{{ t('client.dashboard.col.action') }}</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="order in topOrders"
              :key="order.id"
              class="border-b border-background-100 dark:border-background-800 last:border-0 hover:bg-background-100 dark:hover:bg-background-800/60 transition-colors"
            >
              <td class="px-5 py-3 font-medium text-primary-600 dark:text-primary-400">{{ order.orderNumber }}</td>
              <td class="px-5 py-3 text-background-600 dark:text-background-300">{{ order.modelName || '—' }}</td>
              <td class="px-5 py-3">
                <span
                  class="text-xs font-medium px-2.5 py-1 rounded-full"
                  :class="isDone(order)
                    ? 'bg-success-100 text-success-700'
                    : 'bg-primary-100 text-primary-700'"
                >
                  {{ isDone(order) ? t('client.orders.done') : t('client.orders.inProgress') }}
                </span>
              </td>
              <td class="px-5 py-3 w-40">
                <div class="flex items-center gap-2">
                  <div class="flex-1 bg-background-200 dark:bg-background-800 rounded-full h-1.5 overflow-hidden">
                    <div
                      class="h-full rounded-full"
                      :class="isDone(order) ? 'bg-success-500' : 'bg-primary-500'"
                      :style="{ width: `${progressPct(order)}%` }"
                    />
                  </div>
                  <span class="text-xs tabular-nums text-background-500 dark:text-background-400">{{ progressPct(order) }}%</span>
                </div>
              </td>
              <td class="px-5 py-3 text-background-600 dark:text-background-300">
                {{ order.etaUtc ? relativeEtaLabel(order.etaUtc, t) : '—' }}
              </td>
              <td class="px-5 py-3 text-right">
                <RouterLink :to="`/client/orders/${order.id}`" class="text-primary-500 hover:text-primary-600 dark:hover:text-primary-400 font-medium text-sm">
                  {{ t('client.dashboard.col.details') }}
                </RouterLink>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useI18n } from 'vue-i18n'
import 'material-symbols'
import { clientPortalService, type ClientOrderSummary } from '@/services/clientPortalService'
import { relativeEtaLabel } from '@/utils/clientEta'

const { t } = useI18n()

const orders = ref<ClientOrderSummary[]>([])
const loading = ref(true)

onMounted(async () => {
  try {
    orders.value = await clientPortalService.getMyOrders()
  } finally {
    loading.value = false
  }
})

function isDone(order: ClientOrderSummary) {
  return order.totalCars > 0 && order.completedCars === order.totalCars
}

function progressPct(order: ClientOrderSummary) {
  if (order.totalCars === 0) return 0
  return Math.round((order.completedCars / order.totalCars) * 100)
}

const inProductionCount = computed(() => orders.value.filter(o => !isDone(o)).length)
const readyCount = computed(() => orders.value.filter(isDone).length)

// Critério para "mais avançadas" (assumido, por não estar definido no briefing):
// progresso decrescente e, em empate, ETA mais próxima primeiro. Ajusta aqui
// se preferires outro critério (ex: data de entrega mais próxima acima de tudo).
const topOrders = computed(() =>
  [...orders.value]
    .sort((a, b) => {
      const diff = progressPct(b) - progressPct(a)
      if (diff !== 0) return diff
      if (!a.etaUtc) return 1
      if (!b.etaUtc) return -1
      return new Date(a.etaUtc).getTime() - new Date(b.etaUtc).getTime()
    })
    .slice(0, 5)
)
</script>
