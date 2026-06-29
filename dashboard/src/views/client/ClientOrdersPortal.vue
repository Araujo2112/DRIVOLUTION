<template>
  <div class="min-h-screen bg-background-50 dark:bg-background-950 p-6">
    <!-- Header -->
    <div class="mb-8">
      <h1 class="text-2xl font-bold text-background-900 dark:text-background-50">
        {{ t('client.orders.title') }}
      </h1>
      <p class="text-background-500 dark:text-background-400 mt-1">
        {{ t('client.orders.subtitle') }}
      </p>
    </div>

    <!-- Loading -->
    <div v-if="loading" class="flex justify-center py-16">
      <div class="animate-spin rounded-full h-10 w-10 border-t-2 border-primary-500" />
    </div>

    <!-- Empty -->
    <div
      v-else-if="orders.length === 0"
      class="text-center py-16 text-background-400 dark:text-background-500"
    >
      {{ t('client.orders.empty') }}
    </div>

    <!-- Lista de encomendas -->
    <div v-else class="grid gap-4 sm:grid-cols-2 xl:grid-cols-3">
      <div
        v-for="order in orders"
        :key="order.id"
        class="bg-background-0 dark:bg-background-900 rounded-xl border border-background-200 dark:border-background-700 p-5 cursor-pointer hover:border-primary-400 dark:hover:border-primary-500 transition-colors"
        @click="goToDetail(order.id)"
      >
        <div class="flex items-start justify-between mb-3">
          <div>
            <p class="text-xs text-background-400 dark:text-background-500 font-medium uppercase tracking-wide">
              {{ t('client.orders.orderNumber') }}
            </p>
            <p class="text-lg font-bold text-background-900 dark:text-background-50">
              {{ order.orderNumber }}
            </p>
          </div>
          <!-- Badge de progresso -->
          <span
            :class="[
              'text-xs font-semibold px-2 py-1 rounded-full',
              order.completedCars === order.totalCars
                ? 'bg-success-100 dark:bg-success-900/30 text-success-700 dark:text-success-400'
                : 'bg-primary-100 dark:bg-primary-900/30 text-primary-700 dark:text-primary-400'
            ]"
          >
            {{ order.completedCars === order.totalCars ? t('client.orders.done') : t('client.orders.inProgress') }}
          </span>
        </div>

        <p class="text-sm text-background-500 dark:text-background-400 mb-4">
          {{ formatDate(order.orderDate) }}
        </p>

        <!-- Barra de progresso -->
        <div class="mb-2">
          <div class="flex justify-between text-xs text-background-500 dark:text-background-400 mb-1">
            <span>{{ order.status }}</span>
            <span>{{ Math.round((order.completedCars / order.totalCars) * 100) }}%</span>
          </div>
          <div class="w-full bg-background-200 dark:bg-background-700 rounded-full h-2">
            <div
              class="bg-primary-500 h-2 rounded-full transition-all"
              :style="{ width: `${(order.completedCars / order.totalCars) * 100}%` }"
            />
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { clientPortalService, type ClientOrderSummary } from '@/services/clientPortalService'

const { t } = useI18n()
const router = useRouter()

const orders = ref<ClientOrderSummary[]>([])
const loading = ref(true)

onMounted(async () => {
  try {
    orders.value = await clientPortalService.getMyOrders()
  } finally {
    loading.value = false
  }
})

function goToDetail(orderId: number) {
  router.push(`/client/orders/${orderId}`)
}

function formatDate(iso: string) {
  return new Date(iso).toLocaleDateString('pt-PT', { day: '2-digit', month: 'long', year: 'numeric' })
}
</script>