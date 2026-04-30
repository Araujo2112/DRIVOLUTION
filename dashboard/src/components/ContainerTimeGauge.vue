<template>
  <div class="gauge-container">
    <div v-if="loading" class="flex items-center justify-center h-full">
      <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
      <span class="ml-2 text-gray-600">Carregando dados...</span>
    </div>
    <div v-else class="w-full h-full flex flex-col items-center">

      <canvas ref="chartCanvas" class="gauge-canvas mb-4" />

      <div v-if="containerData" class="stat-card w-full max-w-md">
        <div class="grid grid-cols-2 gap-4 text-sm">
          <div>
            <span class="text-gray-600">Items processados:</span>
            <span class="font-semibold ml-1">{{ containerData.totalItems }}</span>
          </div>
          <div v-if="containerData.minHoras !== undefined">
            <span class="text-gray-600">Tempo mínimo:</span>
            <span class="font-semibold ml-1">{{ containerData.minHoras.toFixed(2) }}h</span>
          </div>
          <div v-if="containerData.maxHoras !== undefined">
            <span class="text-gray-600">Tempo máximo:</span>
            <span class="font-semibold ml-1">{{ containerData.maxHoras.toFixed(2) }}h</span>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, watch, nextTick, onBeforeUnmount } from 'vue'
import {
  Chart as ChartJS,
  ArcElement,
  Tooltip,
  Legend,
  DoughnutController
} from 'chart.js'

ChartJS.register(ArcElement, Tooltip, Legend, DoughnutController)

interface Props {
  selectedContainer?: string
}

const props = withDefaults(defineProps<Props>(), {
  selectedContainer: ''
})

const emit = defineEmits<{
  dataUpdated: [stats: { mediaGeral: number, totalContainers: number, containerSelecionado: string }]
}>()

const chartCanvas = ref<HTMLCanvasElement | null>(null)
const loading = ref(false)
const containerData = ref<any>(null)
let chartInstance: ChartJS | null = null
const API_BASE = 'http://localhost:5181'

async function fetchData() {
  loading.value = true
  try {
    const [containersResponse, itemsResponse] = await Promise.all([
      fetch(`${API_BASE}/api/Container`),
      fetch(`${API_BASE}/api/ItemInContainer`)
    ])
    if (!containersResponse.ok || !itemsResponse.ok) {
      throw new Error('Erro ao buscar dados das APIs')
    }
    const containers = await containersResponse.json()
    const items = await itemsResponse.json()
    const completedItems = items.filter(item => item.dateTimeOut != null)

    if (props.selectedContainer) {
      const cid = parseInt(props.selectedContainer)
      const cis = completedItems.filter(i => i.containerId === cid)
      const cont = containers.find(c => c.containerId === cid)
      if (!cis.length) {
        containerData.value = { containerName: cont?.containerName?.name || 'Container não encontrado', horasMedia: 0, totalItems: 0 }
      } else {
        const times = cis.map(i => {
          const inD = new Date(i.dateTimeIn), outD = new Date(i.dateTimeOut)
          return (outD.getTime() - inD.getTime()) / 3600000
        })
        containerData.value = {
          containerName: cont?.containerName?.name || `Container ${cid}`,
          horasMedia: times.reduce((s, t) => s + t, 0) / times.length,
          totalItems: cis.length,
          minHoras: Math.min(...times),
          maxHoras: Math.max(...times)
        }
      }
      emit('dataUpdated', { mediaGeral: containerData.value.horasMedia, totalContainers: 1, containerSelecionado: containerData.value.containerName })
      return createGaugeData(containerData.value.horasMedia)
    } else {
      if (!completedItems.length) {
        containerData.value = { containerName: 'Média Geral', horasMedia: 0, totalItems: 0 }
      } else {
        const allTimes = completedItems.map(i => {
          const inD = new Date(i.dateTimeIn), outD = new Date(i.dateTimeOut)
          return (outD.getTime() - inD.getTime()) / 3600000
        })
        const avg = allTimes.reduce((s, t) => s + t, 0) / allTimes.length
        containerData.value = { containerName: 'Média Geral', horasMedia: avg, totalItems: completedItems.length, minHoras: Math.min(...allTimes), maxHoras: Math.max(...allTimes) }
      }
      emit('dataUpdated', { mediaGeral: containerData.value.horasMedia, totalContainers: containers.length, containerSelecionado: 'Todos os Containers' })
      return createGaugeData(containerData.value.horasMedia)
    }
  } catch (e) {
    console.error(e)
    emit('dataUpdated', { mediaGeral: 0, totalContainers: 0, containerSelecionado: 'Erro' })
    return createGaugeData(0)
  } finally {
    loading.value = false
  }
}

function createGaugeData(value: number) {
  const maxV = 3
  const norm = Math.min(value, maxV)
  const pct = (norm / maxV) * 100
  let color = '#3B82F6'
  if (pct > 70) color = '#1E40AF'
  else if (pct > 40) color = '#60A5FA'

  return {
    datasets: [{
      data: [norm, maxV - norm],
      backgroundColor: [color, '#E5E7EB'],
      cutout: '80%',
      rotation: 270,
      circumference: 180
    }],
    value, maxValue: maxV, label: containerData.value.containerName, color
  }
}

async function renderGauge(cfg: any) {
  if (!chartCanvas.value) return
  chartInstance?.destroy()
  await nextTick()
  const ctx = chartCanvas.value.getContext('2d')
  if (!ctx) return
  chartInstance = new ChartJS(ctx, {
    type: 'doughnut',
    data: cfg,
    options: { responsive: true, maintainAspectRatio: false, plugins: { legend: { display: false }, tooltip: { enabled: false } }, elements: { arc: { borderWidth: 0 } } },
    plugins: [{
      id: 'gText',
      afterDraw(chart) {
        const ctx = chart.ctx
        const cx = (chart.chartArea.left + chart.chartArea.right) / 2
        const cy = (chart.chartArea.top + chart.chartArea.bottom) / 2
        ctx.save()
        ctx.font = 'bold 24px Arial'
        ctx.fillStyle = '#3B82F6'
        ctx.textAlign = 'center'
        ctx.fillText(`${cfg.value.toFixed(1)}h`, cx, cy - 10)
        ctx.restore()
      }
    }]
  })
}

watch(() => props.selectedContainer, async () => {
  const cfg = await fetchData()
  await renderGauge(cfg)
}, { immediate: true })

onMounted(async () => {
  const cfg = await fetchData()
  await renderGauge(cfg)
})

onBeforeUnmount(() => {
  chartInstance?.destroy()
})
</script>

<style scoped>
.gauge-container {
  width: 100%;
  max-width: 400px;
  margin: 0 auto;
}
.form-select {
  @apply px-3 py-2 bg-white border border-gray-300 rounded-lg text-sm
  focus:ring-2 focus:ring-blue-500 focus:border-transparent;
}
.gauge-canvas {
  width: 100% !important;
  height: 200px !important;
}
.stat-card {
  background: #F3F4F6;
  padding: 1rem;
  border-radius: 0.5rem;
}
</style>
