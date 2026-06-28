<template>
  <div class="min-h-screen bg-background-100 dark:bg-background-950 p-8">
    <div class="max-w-screen-2xl mx-auto">
      <div class="flex items-start justify-between mb-8">
        <div>
          <div class="flex items-center gap-3 mb-6">
            <img
              src="@/assets/icons/drivolution-logo.png"
              alt="Drivolution logo"
              class="h-9 w-auto"
            />
          </div>

          <h1 class="text-2xl font-medium text-background-900 dark:text-background-50">
            Portal do Cliente
          </h1>
          <p class="text-sm text-background-600 dark:text-background-400 mt-1">
            Consulte o estado das suas encomendas e acompanhe a produção dos seus veículos.
          </p>
        </div>

        <div class="flex items-center gap-3">
          <button
            @click="loadOrders"
            class="flex items-center gap-2 bg-primary-500 hover:bg-primary-600 text-white text-sm font-medium px-4 py-2 rounded-lg transition-colors"
          >
            <span class="material-symbols-rounded text-base">refresh</span>
            Atualizar
          </button>

          <button
            @click="logout"
            class="flex items-center gap-2 bg-background-50 dark:bg-background-800 border border-background-300 dark:border-background-700 text-sm text-background-600 hover:text-danger-500 px-4 py-2 rounded-lg transition-colors"
          >
            <span class="material-symbols-rounded text-base">logout</span>
            Sair
          </button>
        </div>
      </div>

      <div v-if="loading" class="flex items-center gap-2 text-background-500 text-sm py-12">
        <span class="material-symbols-rounded animate-spin text-lg">autorenew</span>
        A carregar encomendas...
      </div>

      <div v-else class="flex flex-col gap-6">
        <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
          <div class="bg-background-50 dark:bg-background-800 border border-background-300 dark:border-background-700 rounded-xl p-5 flex items-center justify-between">
            <div>
              <p class="text-xs font-medium text-background-500 uppercase tracking-wider">
                Encomendas
              </p>
              <p class="text-3xl font-medium text-primary-500 mt-2">
                {{ orders.length }}
              </p>
            </div>
            <span class="material-symbols-rounded text-primary-500 text-3xl opacity-70">
              shopping_cart
            </span>
          </div>

          <div class="bg-background-50 dark:bg-background-800 border border-background-300 dark:border-background-700 rounded-xl p-5 flex items-center justify-between">
            <div>
              <p class="text-xs font-medium text-background-500 uppercase tracking-wider">
                Veículos
              </p>
              <p class="text-3xl font-medium text-background-900 dark:text-background-50 mt-2">
                {{ totalVehicles }}
              </p>
            </div>
            <span class="material-symbols-rounded text-background-500 text-3xl opacity-70">
              directions_car
            </span>
          </div>

          <div class="bg-background-50 dark:bg-background-800 border border-background-300 dark:border-background-700 rounded-xl p-5 flex items-center justify-between">
            <div>
              <p class="text-xs font-medium text-background-500 uppercase tracking-wider">
                Concluídos
              </p>
              <p class="text-3xl font-medium text-success-500 mt-2">
                {{ totalCompleted }}
              </p>
            </div>
            <span class="material-symbols-rounded text-success-500 text-3xl opacity-70">
              verified
            </span>
          </div>
        </div>

        <div
          v-if="orders.length === 0"
          class="bg-background-50 dark:bg-background-800 border border-background-300 dark:border-background-700 rounded-xl p-8 text-center text-background-500"
        >
          Não existem encomendas associadas à sua conta.
        </div>

        <div
          v-else
          class="bg-background-50 dark:bg-background-800 border border-background-300 dark:border-background-700 rounded-xl overflow-hidden"
        >
          <table class="min-w-full text-sm">
            <thead class="bg-background-100 dark:bg-background-900 text-background-500 uppercase text-xs">
              <tr>
                <th class="text-left px-6 py-4 font-medium">Encomenda</th>
                <th class="text-left px-6 py-4 font-medium">Data</th>
                <th class="text-left px-6 py-4 font-medium">Quantidade</th>
                <th class="text-left px-6 py-4 font-medium">Concluídos</th>
                <th class="text-left px-6 py-4 font-medium">Progresso</th>
                <th class="text-left px-6 py-4 font-medium">Estado</th>
                <th class="text-right px-6 py-4 font-medium">Ações</th>
              </tr>
            </thead>

            <tbody>
              <tr
                v-for="order in orders"
                :key="order.id"
                class="border-t border-background-200 dark:border-background-700 hover:bg-background-100 dark:hover:bg-background-900 transition-colors"
              >
                <td class="px-6 py-5 font-medium text-background-900 dark:text-background-50">
                  {{ order.orderNumber }}
                </td>

                <td class="px-6 py-5 text-background-600 dark:text-background-300">
                  {{ formatDate(order.orderDate) }}
                </td>

                <td class="px-6 py-5 text-background-600 dark:text-background-300">
                  {{ order.quantity }}
                </td>

                <td class="px-6 py-5 text-success-500 font-medium">
                  {{ order.completedProducts }}
                </td>

                <td class="px-6 py-5">
                  <div class="flex items-center gap-3">
                    <div class="h-2 w-28 rounded-full bg-background-200 dark:bg-background-700 overflow-hidden">
                      <div
                        class="h-full rounded-full bg-primary-500"
                        :style="{ width: `${progress(order)}%` }"
                      />
                    </div>
                    <span class="text-sm font-medium text-primary-500">
                      {{ progress(order) }}%
                    </span>
                  </div>
                </td>

                <td class="px-6 py-5">
                  <span
                    class="inline-flex items-center rounded-full px-3 py-1.5 text-sm font-medium"
                    :class="statusClass(order.status)"
                  >
                    {{ statusLabel(order.status) }}
                  </span>
                </td>

                <td class="px-6 py-5 text-right">
                  <button
                    @click="goToOrder(order.id)"
                    class="inline-flex items-center gap-1 text-primary-500 hover:text-primary-600 font-medium transition-colors"
                  >
                    Ver detalhe
                    <span class="material-symbols-rounded text-base">arrow_forward</span>
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import router from '@/router'
import { useAuthStore } from '@/stores/authStore'
import {
  clientPortalService,
  type ClientOrderSummary,
} from '@/services/clientPortalService'
import { toast } from '@/plugins/toast'

const auth = useAuthStore()

const loading = ref(true)
const orders = ref<ClientOrderSummary[]>([])

const totalVehicles = computed(() => {
  return orders.value.reduce((sum, order) => sum + order.quantity, 0)
})

const totalCompleted = computed(() => {
  return orders.value.reduce((sum, order) => sum + order.completedProducts, 0)
})

onMounted(async () => {
  await loadOrders()
})

async function loadOrders() {
  loading.value = true

  try {
    const res = await clientPortalService.getOrders()
    orders.value = res.data
  } catch {
    toast.error('Erro ao carregar encomendas.')
  } finally {
    loading.value = false
  }
}

function goToOrder(orderId: number) {
  router.push(`/client/orders/${orderId}`)
}

function logout() {
  auth.logout()
}

function formatDate(date: string) {
  return new Date(date).toLocaleDateString('pt-PT')
}

function progress(order: ClientOrderSummary) {
  if (order.quantity === 0) return 0
  return Math.round((order.completedProducts / order.quantity) * 100)
}

function statusLabel(status: string) {
  switch (status) {
    case 'completed':
      return 'Concluída'
    case 'in_progress':
      return 'Em produção'
    default:
      return 'Pendente'
  }
}

function statusClass(status: string) {
  switch (status) {
    case 'completed':
      return 'bg-success-100 text-success-700'
    case 'in_progress':
      return 'bg-primary-100 text-primary-700'
    default:
      return 'bg-background-200 text-background-600'
  }
}
</script>