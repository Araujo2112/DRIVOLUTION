<template>
  <div class="chart-container">
    <canvas ref="canvas" />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onBeforeUnmount, nextTick } from 'vue'
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
    BarController,
    Title,
    Tooltip,
    Legend
)

const canvas = ref<HTMLCanvasElement | null>(null)
let chart: ChartJS | null = null

const colorPalette = [
  '#3B82F6', '#10B981', '#F59E0B', '#EF4444',
  '#8B5CF6', '#EC4899', '#14B8A6', '#F97316',
  '#6366F1', '#84CC16', '#F43F5E', '#06B6D4'
]

async function fetchData() {
  const [secsRes, phasesRes] = await Promise.all([
    fetch('http://localhost:5181/api/PlantFloorSection'),
    fetch('http://localhost:5181/api/ManufacturingPhase')
  ])
  const sections = secsRes.ok ? await secsRes.json() : []
  const phases   = phasesRes.ok ? await phasesRes.json() : []

  const durMap = new Map<number, number>()
  phases.forEach((p: any) => {
    const secId = p.plantFloorSectionId
    const dur   = Number(p.phaseDuration) || 0
    durMap.set(secId, (durMap.get(secId) || 0) + dur)
  })

  const labels: string[] = []
  const values: number[] = []
  durMap.forEach((sumDur, secId) => {
    const sec = sections.find((s: any) => s.sectionId === secId)
    labels.push(sec?.sectionCode || `Secção ${secId}`)
    values.push(sumDur)
  })

  const total = values.reduce((a, b) => a + b, 0) || 1
  const pctValues = values.map(v => Math.round((v / total) * 10000) / 100)

  return { labels, pctValues }
}

async function renderChart() {
  if (!canvas.value) return
  const { labels, pctValues } = await fetchData()

  const backgroundColor = labels.map((_, i) => colorPalette[i % colorPalette.length])
  const borderColor = backgroundColor.map(c => c)

  chart?.destroy()
  await nextTick()

  const ctx = canvas.value!.getContext('2d')!
  chart = new ChartJS(ctx, {
    type: 'bar',
    data: {
      labels,
      datasets: [{
        label: '% do tempo total',
        data: pctValues,
        backgroundColor,
        borderColor,
        borderWidth: 1
      }]
    },
    options: {
      indexAxis: 'y',
      responsive: true,
      plugins: {
        legend: { position: 'bottom' },
        title: { display: true, text: 'Duração Total por Secção (em %)' },
        tooltip: {
          callbacks: {
            label: ctx => `${ctx.parsed.x}%`
          }
        }
      },
      scales: {
        x: {
          max: 100,
          title: { display: true, text: 'Percentagem' }
        },
        y: {
          title: { display: true, text: 'Secção do Piso' }
        }
      }
    }
  })
}

onMounted(renderChart)
onBeforeUnmount(() => chart?.destroy())
</script>

<style scoped>
.chart-container {
  width: 100%;
  height: 100%;
  min-height: 300px;
}
canvas {
  width: 100% !important;
  height: 100% !important;
}
</style>
