<template>
  <div class="p-6 min-h-screen">
    <header class="mb-8">
      <h1 class="text-3xl font-bold text-gray-800">Painel de Controlo</h1>
    </header>

    <div class="grid grid-cols-12 gap-6">
      <!-- PRIMEIRO CARD-->
      <div
          class="col-span-12 md:col-span-8 bg-white rounded-xl shadow-xl p-6 hover:shadow-2xl transition-shadow duration-300"
      >
        <div class="flex items-center justify-between mb-4">
          <div class="flex items-center space-x-3">
            <div class="p-2 bg-blue-100 rounded-lg">
              <svg
                  class="w-6 h-6 text-blue-600"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
              >
                <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    stroke-width="2"
                    d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z"
                />
              </svg>
            </div>
            <div>
              <h2 class="text-2xl font-bold text-gray-800">
                Número Diário de Itens por Contentor
              </h2>
            </div>
          </div>
        </div>

        <div class="mb-6 p-4 bg-gray-50 rounded-lg border border-gray-200">
          <div class="flex flex-col space-y-4">
            <div class="flex flex-wrap items-center gap-4">
              <div class="flex items-center space-x-2">
                <label class="text-sm font-medium text-gray-700">Período:</label>
                <select
                    v-model="selectedPeriod"
                    @change="onPeriodChange"
                    class="form-select"
                >
                  <option value="7">Últimos 7 dias</option>
                  <option value="30">Últimos 30 dias</option>
                  <option value="90">Últimos 90 dias</option>
                  <option value="custom">Período personalizado</option>
                </select>
              </div>

              <div class="flex items-center space-x-2">
                <label class="text-sm font-medium text-gray-700">Ano:</label>
                <select
                    v-model="selectedYear"
                    @change="onYearChange"
                    class="form-select"
                >
                  <option
                      v-for="year in availableYears"
                      :key="year"
                      :value="year"
                  >
                    {{ year }}
                  </option>
                </select>
              </div>

              <div class="flex items-center space-x-2">
                <label class="text-sm font-medium text-gray-700">Mês:</label>
                <select
                    v-model="selectedMonth"
                    @change="onMonthChange"
                    class="form-select"
                >
                  <option value="">Todos os meses</option>
                  <option
                      v-for="(month, idx) in months"
                      :key="idx"
                      :value="idx + 1"
                  >
                    {{ month }}
                  </option>
                </select>
              </div>

              <div class="flex items-center space-x-2">
                <label class="text-sm font-medium text-gray-700">
                  Container:
                </label>
                <select
                    v-model="selectedContainer"
                    @change="onContainerChange"
                    class="form-select"
                >
                  <option value="">Todos os containers</option>
                  <option
                      v-for="c in availableContainers"
                      :key="c.id"
                      :value="c.id"
                  >
                    {{ c.name }}
                  </option>
                </select>
              </div>
            </div>

            <div
                v-if="selectedPeriod === 'custom'"
                class="flex flex-wrap items-center gap-4"
            >
              <div class="flex items-center space-x-2">
                <label class="text-sm font-medium text-gray-700">De:</label>
                <input
                    type="date"
                    v-model="customStartDate"
                    @change="onCustomDateChange"
                    class="form-select"
                />
              </div>
              <div class="flex items-center space-x-2">
                <label class="text-sm font-medium text-gray-700">Até:</label>
                <input
                    type="date"
                    v-model="customEndDate"
                    @change="onCustomDateChange"
                    class="form-select"
                />
              </div>
            </div>

            <div class="flex justify-end">
              <button
                  @click="resetFilters"
                  class="px-4 py-2 bg-gray-500 text-white rounded-lg hover:bg-gray-600 transition-colors duration-200 text-sm"
              >
                Reset Filtros
              </button>
            </div>

            <div v-if="hasActiveFilters" class="flex flex-wrap gap-2">
              <span class="text-sm text-gray-600">Filtros ativos:</span>
              <span
                  v-if="selectedYear !== new Date().getFullYear()"
                  class="px-2 py-1 bg-blue-100 text-blue-800 rounded-full text-xs"
              >
                Ano: {{ selectedYear }}
              </span>
              <span
                  v-if="selectedMonth"
                  class="px-2 py-1 bg-green-100 text-green-800 rounded-full text-xs"
              >
                Mês: {{ months[selectedMonth - 1] }}
              </span>
              <span
                  v-if="selectedContainer"
                  class="px-2 py-1 bg-purple-100 text-purple-800 rounded-full text-xs"
              >
                Container:
                {{
                  availableContainers.find((c) => c.id == selectedContainer)
                      ?.name
                }}
              </span>
              <span
                  v-if="selectedPeriod === 'custom'"
                  class="px-2 py-1 bg-yellow-100 text-yellow-800 rounded-full text-xs"
              >
                {{ formatDate(customStartDate) }} até
                {{ formatDate(customEndDate) }}
              </span>
            </div>
          </div>
        </div>

        <div class="h-80 bg-gradient-to-br from-blue-50 to-indigo-50 rounded-lg p-4">
          <ContainerOccupancyChart
              :filters="currentFilters"
              @data-updated="onDataUpdated"
          />
        </div>
      </div>
      <!-- SEGUNDO CARD-->
      <div class="col-span-12 md:col-span-4 bg-white rounded-xl shadow-xl p-6">
        <div class="flex items-center mb-4">
          <div class="p-2 bg-blue-100 rounded-lg mr-3">
            <span class="material-symbols-rounded text-blue-600">timer</span>
          </div>
          <div>
            <h3 class="text-lg font-bold text-gray-800">Tempo médio dos itens por contentor</h3>
          </div>
        </div>

        <div class="mb-4 p-3 bg-gray-50 rounded-lg border border-gray-200">
          <label class="block text-sm font-medium text-gray-700 mb-1">Container:</label>
          <select
              v-model="selectedContainerForGauge"
              @change="onGaugeContainerChange"
              class="form-select w-full"
          >
            <option value="">Média Geral (Todos)</option>
            <option
                v-for="c in availableContainers"
                :key="c.id"
                :value="c.id"
            >{{ c.name }}</option>
          </select>
          <button
              v-if="selectedContainerForGauge"
              @click="clearGaugeFilter"
              class="mt-2 px-3 py-1 bg-gray-500 text-white rounded text-sm hover:bg-gray-600"
          >
            Limpar
          </button>
        </div>

        <div class="h-64">
          <ContainerTimeGauge
              :selected-container="selectedContainerForGauge"
              @data-updated="onGaugeDataUpdated"
          />
        </div>
      </div>
      <!-- TERCEIRO CARD -->
      <div class="col-span-12 lg:col-span-7 row-span-4 bg-white rounded-xl shadow-xl p-6 hover:shadow-2xl transition-shadow duration-300 border border-gray-100">
        <div class="flex items-center mb-6">
          <div class="p-3 bg-gradient-to-br from-blue-100 to-blue-200 rounded-xl mr-4 shadow-lg">
            <span class="material-symbols-rounded text-blue-600 text-2xl">inventory_2</span>
          </div>
          <div>
            <h3 class="text-xl font-semibold text-gray-800">
              Consumo de Matéria-Prima por Fase
            </h3>
          </div>
        </div>

        <div class="h-80 md:h-96 bg-gradient-to-br  bg-blue-50 via-blue-50 rounded-xl p-6 shadow-inner border border-gray-100">
          <RawMaterialConsumptionChart :filters="{ ...rawMaterialFilters }" @data-updated="onRMDataUpdated"/>
        </div>
      </div>
      <!-- QUARTO CARD -->
      <div class="col-span-12 lg:col-span-5 row-span-4 bg-white rounded-xl shadow-xl p-6 hover:shadow-2xl transition-shadow duration-300 border border-gray-100">
        <div class="flex items-center mb-6">
          <div class="p-3 bg-gradient-to-br from-blue-100 to-blue-200 rounded-xl mr-4 shadow-lg">
              <span class="material-symbols-rounded text-blue-600 text-2xl">factory</span>
          </div>
          <div>
            <h3 class="text-xl font-semibold text-gray-800">
               Matérias-Primas por processo de fabrico
            </h3>
          </div>
        </div>
        <ManufacturingOrdersCard />
      </div>

      <div
          class="col-span-12 lg:col-span-6 bg-white rounded-xl shadow-xl p-6 hover:shadow-2xl transition-shadow duration-300"
      >
        <div class="flex items-center mb-4">
          <div class="p-3 bg-red-100 rounded-xl mr-4 shadow-lg">
            <span class="material-symbols-rounded text-red-600 text-2xl">gradient</span>
          </div>
          <div>
            <h3 class="text-xl font-semibold text-gray-800">
              Duração Total por Secção (%)
            </h3>
          </div>
        </div>
        <div class="h-80">
          <PhaseDurationChart />
        </div>
      </div>

      <!-- SEXTO CARD -->
      <div
          class="col-span-12 lg:col-span-6 bg-white rounded-xl shadow-xl p-6 hover:shadow-2xl transition-shadow duration-300"
      >
        <div class="flex items-center mb-4">
          <div class="p-3 bg-blue-100 rounded-xl mr-4 shadow-lg">
            <span class="material-symbols-rounded text-blue-600 text-2xl">timeline</span>
          </div>
          <div>
            <h3 class="text-xl font-semibold text-gray-800">
              Fases por Ordem (Gantt Chart)
            </h3>
          </div>
        </div>
        <div class="h-80">
          <GanttChart />
        </div>
      </div>
    </div>


    </div>

</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import ContainerOccupancyChart from '@/components/ContainerOccupancyChart.vue'
import ContainerTimeGauge from '@/components/ContainerTimeGauge.vue'
import RawMaterialConsumptionChart from '@/components/RawMaterialConsumptionChart.vue'
import ManufacturingOrdersCard from '@/components/ManufacturingOrdersCard.vue'
import PhaseDurationChart from '@/components/PhaseDurationChart.vue'
import GanttChart   from '@/components/GanttChart.vue'


const selectedPeriod = ref('30')
const selectedYear = ref(new Date().getFullYear())
const selectedMonth = ref('')
const customStartDate = ref('')
const customEndDate = ref('')
const selectedContainer = ref('')

const selectedContainerForGauge = ref('')
const gaugeStats = ref({
  mediaGeral: 0,
  totalContainers: 0,
  containerSelecionado: 'Todos'
})

const rawMaterialFilters = ref({
  phaseId:    '',
  materialId: ''
})

const rawMaterialStats = ref({
  totalPhases:      0,
  totalMaterials:   0,
  totalConsumption: 0
})


const onRMDataUpdated = (stats: typeof rawMaterialStats.value) => {
  rawMaterialStats.value = stats
}

const availableYears = ref([2022, 2023, 2024, 2025])
const availableContainers = ref<Array<{ id: number; name: string }>>([])
const months = [
  'Janeiro', 'Fevereiro', 'Março', 'Abril', 'Maio', 'Junho',
  'Julho', 'Agosto', 'Setembro', 'Outubro', 'Novembro', 'Dezembro'
]

const hasActiveFilters = computed(() =>
    selectedYear.value !== new Date().getFullYear() ||
    selectedMonth.value !== '' ||
    selectedPeriod.value === 'custom' ||
    selectedContainer.value !== ''
)

const currentFilters = computed(() => ({
  period: selectedPeriod.value,
  year: selectedYear.value,
  month: selectedMonth.value,
  startDate: customStartDate.value,
  endDate: customEndDate.value,
  containerId: selectedContainer.value
}))

const onPeriodChange = () => {
  if (selectedPeriod.value !== 'custom') {
    customStartDate.value = ''
    customEndDate.value = ''
  }
}
const onYearChange = () => { selectedMonth.value = ''; }
const onMonthChange = () => {}
const onContainerChange = () => {}
const onCustomDateChange = () => {}
const resetFilters = () => {
  selectedPeriod.value = '30'
  selectedYear.value = new Date().getFullYear()
  selectedMonth.value = ''
  customStartDate.value = ''
  customEndDate.value = ''
  selectedContainer.value = ''
  selectedContainerForGauge.value = ''
}

const onDataUpdated = (stats: any) => {
  console.log('Occupancy stats:', stats)
}

const onGaugeContainerChange = () => {
  console.log('Gauge filter:', selectedContainerForGauge.value)
}
const clearGaugeFilter = () => {
  selectedContainerForGauge.value = ''
}
const onGaugeDataUpdated = (stats: any) => {
  gaugeStats.value = stats
}

const formatDate = (d: string) =>
    d ? new Date(d).toLocaleDateString('pt-PT') : ''

const fetchContainers = async () => {
  try {
    const res = await fetch('http://localhost:5181/api/Container')
    const data = await res.json()
    availableContainers.value = data.map((c: any) => ({
      id: c.containerId,
      name: c.containerName.name
    }))
  } catch (e) {
    console.error(e)
  }
}

onMounted(() => {
  fetchContainers()
})
</script>

<style scoped>
.form-select {
  @apply px-3 py-2 bg-white border border-gray-300 rounded-lg text-sm
  focus:ring-2 focus:ring-blue-500 focus:border-transparent;
}
</style>
