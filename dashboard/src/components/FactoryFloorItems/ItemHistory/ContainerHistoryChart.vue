<template>
  <div>
    <h3>Container Location History</h3>
    <Line v-if="chartData" :data="chartData" :options="chartOptions" />
  </div>
</template>


<script setup lang="ts">
import { computed } from 'vue'
import { Line } from 'vue-chartjs'
import { Chart as ChartJS, Title, Tooltip, Legend, LineElement, CategoryScale, LinearScale, PointElement } from 'chart.js'

ChartJS.register(Title, Tooltip, Legend, LineElement, CategoryScale, LinearScale, PointElement)

const props = defineProps({
  containerDetails: {
    type: Object,
    required: true,
  },
})

const chartData = computed(() => {
  const labels: string[] = []
  const data: number[] = []

  if (props.containerDetails?.container?.localizationHistories?.$values) {
    props.containerDetails.container.localizationHistories.$values.forEach((history: any) => {
      labels.push(new Date(history.datetime).toLocaleString())
      data.push(history.sectionId)
    })
  }

  return {
    labels: labels,
    datasets: [
      {
        label: 'Posição ao Longo do Tempo',
        data: data,
        fill: false,
        borderColor: 'rgba(75, 192, 192, 1)',
        tension: 0.1,
      },
    ],
  }
})

const chartOptions = computed(() => ({
  responsive: true,
  plugins: {
    legend: {
      position: 'top',
    },
    title: {
      display: true,
      text: 'Container Location History',
    },
  },
  scales: {
    y: {
      beginAtZero: true,
      ticks: { precision: 0 },
    },
  },
}))
</script>
