<template>
  <div class="min-h-screen bg-background-50 dark:bg-background-950 p-6">
    <!-- Voltar -->
    <button
      class="flex items-center gap-2 text-sm text-background-500 dark:text-background-400 hover:text-primary-500 mb-6 transition-colors"
      @click="router.push('/client')"
    >
      <span class="material-icons text-base">arrow_back</span>
      {{ t('client.detail.back') }}
    </button>

    <!-- Loading -->
    <div v-if="loading" class="flex justify-center py-16">
      <div class="animate-spin rounded-full h-10 w-10 border-t-2 border-primary-500" />
    </div>

    <template v-else-if="detail">
      <!-- Header da encomenda -->
      <div class="mb-6">
        <h1 class="text-2xl font-bold text-background-900 dark:text-background-50">
          {{ t('client.detail.title') }} {{ detail.orderNumber }}
        </h1>
        <p class="text-background-500 dark:text-background-400 mt-1 text-sm">
          {{ formatDate(detail.orderDate) }} · {{ detail.products.length }} {{ t('client.detail.cars') }}
        </p>
      </div>

      <!-- Lista de carros -->
      <div class="space-y-3">
        <div
          v-for="car in detail.products"
          :key="car.productId"
          class="bg-background-0 dark:bg-background-900 rounded-xl border border-background-200 dark:border-background-700 p-4"
        >
          <div class="flex items-center justify-between flex-wrap gap-3">
            <!-- VIN + fase -->
            <div>
              <div class="flex items-center gap-2 mb-1">
                <span class="material-icons text-background-400 dark:text-background-500 text-sm">directions_car</span>
                <span class="text-xs font-mono text-background-500 dark:text-background-400">{{ car.serialNumber }}</span>
              </div>
              <p class="font-semibold text-background-900 dark:text-background-50">
                {{ car.currentPhase }}
              </p>
            </div>

            <!-- Estado + ETA -->
            <div class="flex flex-col items-end gap-1">
              <!-- Badge de estado -->
              <span
                :class="[
                  'text-xs font-semibold px-2 py-0.5 rounded-full',
                  car.isCompleted
                    ? 'bg-success-100 dark:bg-success-900/30 text-success-700 dark:text-success-400'
                    : 'bg-warning-100 dark:bg-warning-900/30 text-warning-700 dark:text-warning-400'
                ]"
              >
                {{ car.isCompleted ? t('client.detail.completed') : t('client.detail.inProduction') }}
              </span>

              <!-- ETA -->
              <span v-if="!car.isCompleted && car.etaUtc" class="text-xs text-background-500 dark:text-background-400">
                {{ t('client.detail.eta') }}: {{ formatDateTime(car.etaUtc) }}
                <span v-if="car.etaIsMlPrediction" class="ml-1 text-primary-400 font-medium">ML</span>
              </span>
              <span v-else-if="!car.isCompleted && !car.etaUtc" class="text-xs text-background-400 dark:text-background-500">
                {{ t('client.detail.etaUnavailable') }}
              </span>
            </div>
          </div>
        </div>
      </div>
    </template>

    <!-- Não encontrado -->
    <div v-else class="text-center py-16 text-background-400 dark:text-background-500">
      {{ t('client.detail.notFound') }}
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { clientPortalService, type ClientOrderDetail } from '@/services/clientPortalService'

const { t } = useI18n()
const route = useRoute()
const router = useRouter()

const detail = ref<ClientOrderDetail | null>(null)
const loading = ref(true)

onMounted(async () => {
  const id = Number(route.params.id)
  try {
    detail.value = await clientPortalService.getOrderDetail(id)
  } finally {
    loading.value = false
  }
})

function formatDate(iso: string) {
  return new Date(iso).toLocaleDateString('pt-PT', { day: '2-digit', month: 'long', year: 'numeric' })
}

function formatDateTime(iso: string) {
  return new Date(iso).toLocaleString('pt-PT', { day: '2-digit', month: '2-digit', hour: '2-digit', minute: '2-digit' })
}
</script>