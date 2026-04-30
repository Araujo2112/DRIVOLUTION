<template>
  <div class="chart-container">
    <div v-if="loading" class="flex items-center justify-center h-full">
      <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
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
    period: string
    year: number
    month: string
    startDate: string
    endDate: string
    containerId: string
  }
}

const props = withDefaults(defineProps<Props>(), {
  filters: () => ({
    period: '30',
    year: new Date().getFullYear(),
    month: '',
    startDate: '',
    endDate: '',
    containerId: ''
  })
})

const emit = defineEmits<{
  dataUpdated: [stats: { totalItems: number, avgPerDay: number, growth: number, activeDays: number }]
}>()

const chartCanvas = ref<HTMLCanvasElement | null>(null)
const loading = ref(false)
let chartInstance: ChartJS | null = null
const API_BASE = 'http://localhost:5181'

async function fetchData() {
  loading.value = true

  try {
    const [containersRes, itemsRes] = await Promise.all([
      fetch(`${API_BASE}/api/Container`),
      fetch(`${API_BASE}/api/ItemInContainer`)
    ])

    if (!containersRes.ok || !itemsRes.ok) {
      throw new Error('API error '+ containersRes.status +', '+ itemsRes.status)
    }

    const containers = await containersRes.json()
    const items = await itemsRes.json()

    const filteredItems = applyFilters(items)

    const dailyCounts: Record<string, number> = {}

    filteredItems.forEach((item: any) => {
      const day = new Date(item.dateTimeIn).toLocaleDateString('pt-PT', {
        day: '2-digit',
        month: '2-digit',
        year: '2-digit'
      })
      dailyCounts[day] = (dailyCounts[day] || 0) + 1
    })

    const labels = Object.keys(dailyCounts).sort((a, b) => {
      const [dayA, monthA, yearA] = a.split('/')
      const [dayB, monthB, yearB] = b.split('/')
      const dateA = new Date(2000 + parseInt(yearA), parseInt(monthA) - 1, parseInt(dayA))
      const dateB = new Date(2000 + parseInt(yearB), parseInt(monthB) - 1, parseInt(dayB))
      return dateA.getTime() - dateB.getTime()
    })

    const data = labels.map(day => dailyCounts[day])

    if (labels.length === 0) {
      emit('dataUpdated', { totalItems: 0, avgPerDay: 0, growth: 0, activeDays: 0 })

      return {
        labels: ['Sem dados'],
        datasets: [{
          label: 'Total de Items por Dia',
          data: [0],
          backgroundColor: 'rgba(156, 163, 175, 0.6)',
          borderColor: 'rgba(156, 163, 175, 1)',
          borderWidth: 1
        }]
      }
    }

    const totalItems = filteredItems.length
    const activeDays = labels.length
    const avgPerDay = Math.round(totalItems / Math.max(activeDays, 1))
    const growth = await calculateGrowth(filteredItems, items)

    emit('dataUpdated', {
      totalItems,
      avgPerDay,
      growth,
      activeDays
    })

    const datasets = [{
      label: 'Total de Items por Dia',
      data: data,
      backgroundColor: 'rgba(54, 162, 235, 0.6)',
      borderColor: 'rgba(54, 162, 235, 1)',
      borderWidth: 1,
      borderRadius: 4,
      borderSkipped: false,
    }]

    return { labels, datasets }

  } catch (error) {
    console.error('Erro ao buscar dados:', error)
    emit('dataUpdated', { totalItems: 0, avgPerDay: 0, growth: 0, activeDays: 0 })
    return {
      labels: ['Erro'],
      datasets: [{
        label: 'Erro ao carregar dados',
        data: [0],
        backgroundColor: 'rgba(239, 68, 68, 0.6)',
        borderColor: 'rgba(239, 68, 68, 1)',
        borderWidth: 1
      }]
    }
  } finally {
    loading.value = false
  }
}

function applyFilters(items: any[]) {
  if (!props.filters) return items

  let filtered = [...items]
  const now = new Date()

  if (props.filters.period !== 'custom') {
    const days = parseInt(props.filters.period)
    const cutoff = new Date(now.getTime() - days * 86400000)
    filtered = filtered.filter(item => new Date(item.dateTimeIn) >= cutoff)
  }

  if (props.filters.period === 'custom' && props.filters.startDate && props.filters.endDate) {
    const startDate = new Date(props.filters.startDate)
    const endDate = new Date(props.filters.endDate)
    endDate.setHours(23, 59, 59, 999)

    filtered = filtered.filter(item => {
      const itemDate = new Date(item.dateTimeIn)
      return itemDate >= startDate && itemDate <= endDate
    })
  }

  if (props.filters.year) {
    filtered = filtered.filter(item => {
      return new Date(item.dateTimeIn).getFullYear() === props.filters.year
    })
  }

  if (props.filters.month) {
    const month = parseInt(props.filters.month)
    filtered = filtered.filter(item => {
      return new Date(item.dateTimeIn).getMonth() + 1 === month
    })
  }

  if (props.filters.containerId) {
    const containerId = parseInt(props.filters.containerId)
    filtered = filtered.filter(item => {
      return item.containerId === containerId
    })
  }

  return filtered
}

async function calculateGrowth(currentItems: any[], allItems: any[]) {
  try {
    const currentCount = currentItems.length
    const oneMonthAgo = new Date()
    oneMonthAgo.setMonth(oneMonthAgo.getMonth() - 1)

    const previousItems = allItems.filter(item => {
      const itemDate = new Date(item.dateTimeIn)
      const monthAgo = new Date(oneMonthAgo)
      const twoMonthsAgo = new Date(oneMonthAgo)
      twoMonthsAgo.setMonth(twoMonthsAgo.getMonth() - 1)

      return itemDate >= twoMonthsAgo && itemDate < monthAgo
    })

    const previousCount = previousItems.length

    if (previousCount === 0) return 0

    return Math.round(((currentCount - previousCount) / previousCount) * 100)
  } catch (error) {
    console.error('Erro ao calcular crescimento:', error)
    return 0
  }
}

async function renderChart({ labels, datasets }: {labels:string[], datasets:any[]}) {
  if (!chartCanvas.value) {
    console.warn('Canvas não está disponível')
    return
  }

  if (chartInstance) {
    chartInstance.destroy()
    chartInstance = null
  }

  await nextTick()

  try {
    const ctx = chartCanvas.value.getContext('2d')
    if (!ctx) {
      console.error('Não foi possível obter contexto 2D do canvas')
      return
    }

    chartInstance = new ChartJS(ctx, {
      type: 'bar',
      data: { labels, datasets },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: {
            position: 'bottom',
            display: true,
            labels: {
              boxWidth: 12,
              padding: 15,
              usePointStyle: true
            }
          },
          tooltip: {
            mode: 'index',
            intersect: false,
            backgroundColor: 'rgba(0, 0, 0, 0.8)',
            titleColor: 'white',
            bodyColor: 'white',
            borderColor: 'rgba(54, 162, 235, 1)',
            borderWidth: 1,
            callbacks: {
              title: function(context: any) {
                return `Data: ${context[0].label}`
              },
              label: function(context: any) {
                return `${context.dataset.label}: ${context.parsed.y} items`
              }
            }
          }
        },
        scales: {
          x: {
            title: {
              display: true,
              text: 'Data',
              font: { size: 14, weight: 'bold' },
              color: '#374151'
            },
            ticks: {
              maxRotation: 45,
              minRotation: 45,
              font: { size: 11 },
              color: '#6B7280',
              maxTicksLimit: 15
            },
            grid: {
              display: false
            }
          },
          y: {
            beginAtZero: true,
            title: {
              display: true,
              text: 'Total de Items',
              font: { size: 14, weight: 'bold' },
              color: '#374151'
            },
            ticks: {
              stepSize: 1,
              font: { size: 11 },
              color: '#6B7280',
              callback: function(value: any) {
                return `${value} items`
              }
            },
            grid: {
              color: 'rgba(156, 163, 175, 0.2)',
              lineWidth: 1
            }
          }
        },
        animation: {
          duration: 1000,
          easing: 'easeInOutCubic'
        },
        interaction: {
          intersect: false,
          mode: 'index'
        }
      }
    })
  } catch (error) {
    console.error('Erro ao criar gráfico:', error)
  }
}

watch(() => props.filters, async (newFilters) => {
  if (newFilters) {
    const data = await fetchData()
    renderChart(data)
  }
}, { deep: true, immediate: false })

onMounted(async () => {
  await nextTick()

  const data = await fetchData()
  renderChart(data)
})

onBeforeUnmount(() => {
  if (chartInstance) {
    chartInstance.destroy()
    chartInstance = null
  }
})
</script>

<style scoped>
.chart-container {
  position: relative;
  width: 100%;
  height: 100%;
  min-height: 200px;
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
