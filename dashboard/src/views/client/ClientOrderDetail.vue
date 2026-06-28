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

          <button
            @click="goBack"
            class="inline-flex items-center gap-1 text-sm text-primary-500 hover:text-primary-600 mb-4"
          >
            <span class="material-symbols-rounded text-base">arrow_back</span>
            Voltar às encomendas
          </button>

          <h1 class="text-2xl font-medium text-background-900 dark:text-background-50">
            Detalhe da Encomenda
          </h1>
          <p class="text-sm text-background-600 dark:text-background-400 mt-1">
            Consulte os veículos associados à encomenda e a respetiva fase atual.
          </p>
        </div>

        <div class="flex items-center gap-3">
          <button
            @click="loadProducts"
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
        A carregar veículos...
      </div>

      <div v-else class="flex flex-col gap-6">
        <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
          <div class="bg-background-50 dark:bg-background-800 border border-background-300 dark:border-background-700 rounded-xl p-5 flex items-center justify-between">
            <div>
              <p class="text-xs font-medium text-background-500 uppercase tracking-wider">
                Veículos
              </p>
              <p class="text-3xl font-medium text-primary-500 mt-2">
                {{ products.length }}
              </p>
            </div>
            <span class="material-symbols-rounded text-primary-500 text-3xl opacity-70">
              directions_car
            </span>
          </div>

          <div class="bg-background-50 dark:bg-background-800 border border-background-300 dark:border-background-700 rounded-xl p-5 flex items-center justify-between">
            <div>
              <p class="text-xs font-medium text-background-500 uppercase tracking-wider">
                Fase predominante
              </p>
              <p class="text-2xl font-medium text-background-900 dark:text-background-50 mt-2">
                {{ mainPhase }}
              </p>
            </div>
            <span class="material-symbols-rounded text-background-500 text-3xl opacity-70">
              precision_manufacturing
            </span>
          </div>

          <div class="bg-background-50 dark:bg-background-800 border border-background-300 dark:border-background-700 rounded-xl p-5 flex items-center justify-between">
            <div>
              <p class="text-xs font-medium text-background-500 uppercase tracking-wider">
                Com previsão
              </p>
              <p class="text-3xl font-medium text-success-500 mt-2">
                {{ productsWithEta }}
              </p>
            </div>
            <span class="material-symbols-rounded text-success-500 text-3xl opacity-70">
              event_available
            </span>
          </div>
        </div>

        <div
          v-if="products.length === 0"
          class="bg-background-50 dark:bg-background-800 border border-background-300 dark:border-background-700 rounded-xl p-8 text-center text-background-500"
        >
          Não existem veículos associados a esta encomenda.
        </div>

        <div
          v-else
          class="bg-background-50 dark:bg-background-800 border border-background-300 dark:border-background-700 rounded-xl overflow-hidden"
        >
          <table class="min-w-full text-sm">
            <thead class="bg-background-100 dark:bg-background-900 text-background-500 uppercase text-xs">
              <tr>
                <th class="text-left px-6 py-4 font-medium">VIN</th>
                <th class="text-left px-6 py-4 font-medium">Fase atual</th>
                <th class="text-left px-6 py-4 font-medium">Previsão de entrega</th>
              </tr>
            </thead>

            <tbody>
              <tr
                v-for="product in products"
                :key="product.vin"
                class="border-t border-background-200 dark:border-background-700 hover:bg-background-100 dark:hover:bg-background-900 transition-colors"
              >
                <td class="px-6 py-5 font-medium text-background-900 dark:text-background-50">
                  {{ product.vin }}
                </td>

                <td class="px-6 py-5">
                  <span class="inline-flex items-center rounded-full px-3 py-1.5 bg-primary-100 text-primary-700 text-sm font-medium">
                    {{ product.currentPhase || 'Pendente' }}
                  </span>
                </td>

                <td class="px-6 py-5 text-background-600 dark:text-background-300">
                  {{ product.estimatedFinish ? formatDate(product.estimatedFinish) : 'Sem previsão' }}
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
import { useRoute } from 'vue-router'
import router from '@/router'
import { useAuthStore } from '@/stores/authStore'
import {
  clientPortalService,
  type ClientOrderProduct,
} from '@/services/clientPortalService'
import { toast } from '@/plugins/toast'

const route = useRoute()
const auth = useAuthStore()

const loading = ref(true)
const products = ref<ClientOrderProduct[]>([])

const productsWithEta = computed(() => {
  return products.value.filter(product => product.estimatedFinish).length
})

const mainPhase = computed(() => {
  if (products.value.length === 0) return '—'

  const counts = products.value.reduce<Record<string, number>>((acc, product) => {
    const phase = product.currentPhase || 'Pendente'
    acc[phase] = (acc[phase] ?? 0) + 1
    return acc
  }, {})

  return Object.entries(counts).sort((a, b) => b[1] - a[1])[0]?.[0] ?? '—'
})

onMounted(async () => {
  await loadProducts()
})

async function loadProducts() {
  loading.value = true

  const orderId = Number(route.params.id)

  try {
    const res = await clientPortalService.getOrderProducts(orderId)
    products.value = res.data
  } catch {
    toast.error('Erro ao carregar veículos.')
  } finally {
    loading.value = false
  }
}

function goBack() {
  router.push('/client')
}

function logout() {
  auth.logout()
}

function formatDate(date: string) {
  return new Date(date).toLocaleString('pt-PT')
}
</script>