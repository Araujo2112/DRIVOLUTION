<template>
  <div class="p-6">
    <div class="mb-6 flex items-center justify-between">
      <h1 class="text-3xl font-bold">
        Visualizador de Linha de Produção
      </h1>

      <button
        class="rounded bg-blue-600 px-4 py-2 text-white hover:bg-blue-700"
        @click="loadStatus"
      >
        Atualizar
      </button>
    </div>

    <!-- KPIs -->
    <div class="mb-6 grid grid-cols-1 gap-4 md:grid-cols-3">
      <div class="rounded bg-white p-4 shadow">
        <p class="text-sm text-gray-500">Linhas Ativas</p>
        <p class="text-3xl font-bold text-green-600">
          {{ activeLines }}
        </p>
      </div>

      <div class="rounded bg-white p-4 shadow">
        <p class="text-sm text-gray-500">Produtos em Produção</p>
        <p class="text-3xl font-bold text-blue-600">
          {{ productsInProduction }}
        </p>
      </div>

      <div class="rounded bg-white p-4 shadow">
        <p class="text-sm text-gray-500">Workstations Livres</p>
        <p class="text-3xl font-bold text-gray-700">
          {{ freeStations }}
        </p>
      </div>
    </div>

    <!-- Tabela -->
    <div class="overflow-x-auto rounded bg-white shadow">
      <table class="min-w-full text-sm">
        <thead class="bg-gray-100">
          <tr>
            <th class="p-4 text-left">Linha</th>
            <th class="p-4 text-left">Workstation</th>
            <th class="p-4 text-left">Produto</th>
            <th class="p-4 text-left">Fase Atual</th>
            <th class="p-4 text-left">Estado</th>
            <th class="p-4 text-left">Início</th>
            <th class="p-4 text-left">Previsão de Fim</th>
            <th class="p-4 text-left">Tempo Restante</th>
          </tr>
        </thead>

        <tbody>
          <tr
            v-for="item in status"
            :key="item.workstationId"
            class="border-t hover:bg-gray-50"
          >
            <td class="p-4 font-medium">
              {{ item.productionLineName }}
            </td>

            <td class="p-4">
              {{ item.workstationType }}
            </td>

            <td class="p-4">
              {{ item.serialNumber || '—' }}
            </td>

            <td class="p-4">
              {{ item.currentPhase || '—' }}
            </td>

            <td class="p-4">
              <span
                class="rounded px-3 py-1 text-xs font-semibold text-white"
                :class="getStatusColor(item)"
              >
                {{ getStatusText(item) }}
              </span>
            </td>

            <td class="p-4">
              {{ formatDate(item.startedAt) }}
            </td>

            <td class="p-4">
              {{ formatDate(item.estimatedFinish) }}
            </td>

            <td class="p-4 font-medium">
              {{ getRemainingTime(item.estimatedFinish) }}
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref } from 'vue'
import axios from '@/axios'

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

async function loadStatus() {
  const response = await axios.get('/production-lines/status')
  status.value = response.data
}

function formatDate(value: string | null) {
  if (!value) return '—'

  return new Date(value).toLocaleString('pt-PT')
}

function getRemainingTime(date: string | null) {
  if (!date) return '—'

  const now = new Date().getTime()
  const end = new Date(date).getTime()

  const diff = end - now

  if (diff <= 0) {
    return 'Atrasado'
  }

  const seconds = Math.floor(diff / 1000)

  return `${seconds}s`
}

function getStatusText(item: ProductionLineStatus) {
  if (!item.productId) return 'Livre'

  const now = new Date().getTime()
  const end = item.estimatedFinish
    ? new Date(item.estimatedFinish).getTime()
    : 0

  if (end < now) return 'Atrasado'

  return 'Em Produção'
}

function getStatusColor(item: ProductionLineStatus) {
  if (!item.productId) {
    return 'bg-gray-500'
  }

  const now = new Date().getTime()
  const end = item.estimatedFinish
    ? new Date(item.estimatedFinish).getTime()
    : 0

  if (end < now) {
    return 'bg-red-600'
  }

  return 'bg-green-600'
}

const activeLines = computed(() =>
  status.value.filter(x => x.productId).length
)

const productsInProduction = computed(() =>
  status.value.filter(x => x.productId).length
)

const freeStations = computed(() =>
  status.value.filter(x => !x.productId).length
)

let interval: number

onMounted(() => {
  loadStatus()

  interval = window.setInterval(() => {
    loadStatus()
  }, 5000)
})

onUnmounted(() => {
  clearInterval(interval)
})
</script>