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
  TimeScale,
  BarController,
  BarElement,
  Tooltip,
  Legend,
} from 'chart.js'
import 'chartjs-adapter-date-fns'

ChartJS.register(
    CategoryScale,
    TimeScale,
    BarController,
    BarElement,
    Tooltip,
    Legend
)

const canvas = ref<HTMLCanvasElement|null>(null)
let chart: ChartJS|null = null

async function fetchData() {
  const [ops, phases, orders] = await Promise.all([
    fetch('http://localhost:5181/api/ManufacturingOrderPhase').then(r=>r.json()),
    fetch('http://localhost:5181/api/ManufacturingPhase').then(r=>r.json()),
    fetch('http://localhost:5181/api/ManufacturingOrder').then(r=>r.json())
  ])

  const phaseInfo = new Map<number,string>()
  phases.forEach(p=> phaseInfo.set(p.id,p.phaseInfo))
  const orderMap = new Map<number,string>()
  orders.forEach(o=> orderMap.set(o.id, `#${o.orderNumber}`))

  const data:any[] = ops.map((o:any) => ({
    x: [ new Date(o.dateTimeInit), new Date(o.dateTimeEnd) ],
    y: orderMap.get(o.manufacturingOrderId) || `#${o.manufacturingOrderId}`,
    label: phaseInfo.get(o.manufacturingPhaseId) || '—'
  }))

  const yCats = Array.from(new Set(data.map(d=>d.y)))

  return { data, yCats }
}

async function renderChart() {
  if (!canvas.value) return
  const { data, yCats } = await fetchData()
  chart?.destroy()
  await nextTick()
  const ctx = canvas.value.getContext('2d')!
  chart = new ChartJS(ctx, {
    type: 'bar',
    data: { datasets: [{
        label: 'Fases',
        data,
        backgroundColor: 'rgba(59,130,246,0.7)',
        borderColor: 'rgba(59,130,246,1)',
        borderWidth: 1
      }]},
    options: {
      indexAxis: 'y',
      parsing: { xAxisKey:'x', yAxisKey:'y' },
      scales: {
        x: { type:'time', time:{tooltipFormat:'Pp'}, title:{display:true,text:'Tempo'} },
        y: { type:'category', labels:yCats, title:{display:true,text:'Ordem'} }
      },
      plugins: {
        tooltip: { callbacks:{
            label: ctx => `${ctx.raw.label}: ${ctx.dataset.data[ctx.dataIndex].x[0].toLocaleString()} → ${ctx.dataset.data[ctx.dataIndex].x[1].toLocaleString()}`
          }},
        legend:{display:false}
      }
    }
  })
}

onMounted(renderChart)
onBeforeUnmount(() => chart?.destroy())
</script>

<style scoped>
.chart-container { width:100%; height:100%; min-height:300px }
canvas { width:100% !important; height:100% !important; }
</style>
