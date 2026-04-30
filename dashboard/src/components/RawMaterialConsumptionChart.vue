<template>
  <div class="chart-container">
    <div v-if="loading" class="flex items-center justify-center h-full">
      <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-green-600"></div>
      <span class="ml-2 text-gray-600">Carregando dados...</span>
    </div>
    <canvas v-else ref="chartCanvas" class="chart-canvas" />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, watch, nextTick, onBeforeUnmount } from 'vue'
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  BarElement,
  Title,
  Tooltip,
  Legend,
  BarController
} from 'chart.js'

ChartJS.register(
    CategoryScale,
    LinearScale,
    BarElement,
    Title,
    Tooltip,
    Legend,
    BarController
)

interface Props {
  filters?: {
    phaseId: string
    materialId: string
    period: string
    year: number
    month: string
  }
}

const props = withDefaults(defineProps<Props>(), {
  filters: () => ({
    phaseId: '',
    materialId: '',
    period: '',
    year: new Date().getFullYear(),
    month: ''
  })
})

const emit = defineEmits<{
  dataUpdated: [stats: { totalPhases: number, totalMaterials: number, totalConsumption: number }]
}>()

const chartCanvas = ref<HTMLCanvasElement | null>(null)
const loading = ref(false)
let chartInstance: ChartJS | null = null
const API_BASE = 'http://localhost:5181'

const colorPalette = [
  '#3B82F6', '#10B981', '#F59E0B', '#EF4444',
  '#8B5CF6', '#EC4899', '#14B8A6', '#F97316',
  '#6366F1', '#84CC16', '#F43F5E', '#06B6D4'
]

interface ManufacturingPhase {
  id: number
  phaseInfo: string
}
interface ManufacturingOrderPhase {
  id: number
  manufacturingPhaseId: number
}
interface ItemOfRawMaterial {
  itemRawId: number
  quantity: number
  lotOfRawMaterialId: number
  manufacturingOrderPhaseId: number
  dateTimeCreated?: string
}
interface LotOfRawMaterial {
  lotId: number
  rawMaterialId: number
}
interface RawMaterial {
  rawId: number
  name: string
}

async function fetchData() {
  loading.value = true

  try {
    const [phasesRes, itemsRes, lotsRes, materialsRes, orderPhasesRes] = await Promise.all([
      fetch(`${API_BASE}/api/ManufacturingPhase`),
      fetch(`${API_BASE}/api/ItemOfRawMaterial`),
      fetch(`${API_BASE}/api/LotOfRawMaterial`),
      fetch(`${API_BASE}/api/RawMaterial`),
      fetch(`${API_BASE}/api/ManufacturingOrderPhase`)
    ])

    if (!phasesRes.ok || !itemsRes.ok || !lotsRes.ok || !materialsRes.ok || !orderPhasesRes.ok) {
      throw new Error('Erro ao buscar dados das APIs')
    }

    const phases: ManufacturingPhase[] = await phasesRes.json()
    const items: ItemOfRawMaterial[] = await itemsRes.json()
    const lots: LotOfRawMaterial[] = await lotsRes.json()
    const materials: RawMaterial[] = await materialsRes.json()
    const orderPhases: ManufacturingOrderPhase[] = await orderPhasesRes.json()

    const processedData = processRawMaterialData(phases, items, lots, materials, orderPhases)
    const filteredData = applyFilters(processedData)
    const chartData = createStackedBarChart(filteredData, phases)
    emit('dataUpdated', calculateStatistics(filteredData))

    return chartData

  } catch (error) {
    console.error('Erro ao buscar dados:', error)
    emit('dataUpdated', { totalPhases: 0, totalMaterials: 0, totalConsumption: 0 })
    return {
      labels: ['Erro'],
      datasets: [{
        label: 'Erro ao carregar dados',
        data: [0],
        backgroundColor: '#EF4444'
      }]
    }
  } finally {
    loading.value = false
  }
}

function processRawMaterialData(
    phases: ManufacturingPhase[],
    items: ItemOfRawMaterial[],
    lots: LotOfRawMaterial[],
    materials: RawMaterial[],
    orderPhases: ManufacturingOrderPhase[]
) {
  return items.map(item => {
    const orderPhase = orderPhases.find(op => op.id === item.manufacturingOrderPhaseId)
    const phase = phases.find(p => p.id === orderPhase?.manufacturingPhaseId)

    const lot = lots.find(l => l.lotId === item.lotOfRawMaterialId)
    const material = materials.find(m => m.rawId === lot?.rawMaterialId)

    return {
      phaseId: phase?.id || 0,
      phaseName: phase?.phaseInfo || 'Fase desconhecida',
      materialId: material?.rawId || 0,
      materialName: material?.name || 'Material desconhecido',
      quantity: item.quantity,
      dateCreated: item.dateTimeCreated
    }
  }).filter(i => i.phaseId && i.materialId)
}

function applyFilters(data: any[]) {
  let filtered = [...data]

  if (props.filters?.phaseId) {
    const phaseId = parseInt(props.filters.phaseId)
    filtered = filtered.filter(item => item.phaseId === phaseId)
  }

  if (props.filters?.materialId) {
    filtered = filtered.filter(item => item.materialName === props.filters.materialId)
  }

  if (props.filters?.period && filtered[0]?.dateCreated) {
    const now = new Date()
    const days = parseInt(props.filters.period)
    const cutoff = new Date(now.getTime() - days * 86400000)

    filtered = filtered.filter(item => {
      if (item.dateCreated) {
        return new Date(item.dateCreated) >= cutoff
      }
      return true
    })
  }

  return filtered
}

function createStackedBarChart(data: any[], phases: ManufacturingPhase[]) {
  const grouped: Record<number, Record<string, number>> = {}

  data.forEach(item => {
    if (!grouped[item.phaseId]) grouped[item.phaseId] = {}
    if (!grouped[item.phaseId][item.materialName]) grouped[item.phaseId][item.materialName] = 0
    grouped[item.phaseId][item.materialName] += item.quantity
  })

  const sortedPhases = phases
      .filter(p => grouped[p.id])
      .sort((a, b) => a.id - b.id)

  const labels = sortedPhases.map(p => p.phaseInfo)
  const allMaterials = [...new Set(data.map(d => d.materialName))].sort()

  const datasets = allMaterials.map((mat, i) => ({
    label: mat,
    data: sortedPhases.map(p => grouped[p.id]?.[mat] || 0),
    backgroundColor: colorPalette[i % colorPalette.length],
    borderColor: colorPalette[i % colorPalette.length],
    borderWidth: 1
  }))

  return { labels, datasets }
}

function calculateStatistics(data: any[]) {
  const totalPhases = new Set(data.map(item => item.phaseId)).size
  const totalMaterials = new Set(data.map(item => item.materialName)).size
  const totalConsumption = data.reduce((sum, item) => sum + item.quantity, 0)
  return { totalPhases, totalMaterials, totalConsumption }
}

async function renderChart({ labels, datasets }: { labels: string[], datasets: any[] }) {
  if (!chartCanvas.value) return
  if (chartInstance) chartInstance.destroy()

  await nextTick()

  const ctx = chartCanvas.value.getContext('2d')
  if (!ctx) return

  chartInstance = new ChartJS(ctx, {
    type: 'bar',
    data: { labels, datasets },
    options: {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        title: {
          display: true,
          text: 'Consumo de Matéria-Prima por Fase de Fabrico',
          font: { size: 16, weight: 'bold' },
          color: '#374151'
        },
        legend: {
          position: 'bottom',
          labels: {
            font: { size: 11 },
            usePointStyle: true,
            boxWidth: 12,
            padding: 15
          }
        },
        tooltip: {
          mode: 'index',
          intersect: false,
          callbacks: {
            title: ctx => `Fase: ${ctx[0].label}`,
            label: ctx => `${ctx.dataset.label}: ${ctx.parsed.y.toFixed(2)} unidades`,
            footer: tips => `Total da Fase: ${tips.reduce((sum, i) => sum + i.parsed.y, 0).toFixed(2)} unidades`
          }
        }
      },
      scales: {
        x: {
          stacked: true,
          title: {
            display: true,
            text: 'Fases de Fabrico',
            font: { size: 14, weight: 'bold' },
            color: '#374151'
          },
          ticks: { font: { size: 11 }, color: '#6B7280' },
          grid: { display: false }
        },
        y: {
          stacked: true,
          beginAtZero: true,
          title: {
            display: true,
            text: 'Quantidade Consumida',
            font: { size: 14, weight: 'bold' },
            color: '#374151'
          },
          ticks: {
            font: { size: 11 },
            color: '#6B7280',
            callback: value => `${value} un.`
          },
          grid: { color: 'rgba(156, 163, 175, 0.2)' }
        }
      }
    }
  })
}

watch(() => props.filters, async () => {
  const data = await fetchData()
  renderChart(data)
}, { deep: true, immediate: false })

onMounted(async () => {
  await nextTick()
  const data = await fetchData()
  renderChart(data)
})

onBeforeUnmount(() => {
  if (chartInstance) chartInstance.destroy()
})
</script>


<style scoped>
.chart-container {
  position: relative;
  width: 100%;
  height: 100%;
  min-height: 300px;
  overflow: hidden;
}

.chart-canvas {
  width: 100% !important;
  height: 100% !important;
  max-width: 100% !important;
  max-height: 100% !important;
  display: block;
}
</style>
