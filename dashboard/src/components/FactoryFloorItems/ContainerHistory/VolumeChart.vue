
<template>
  
  <div>
    <h3>Container Volume Chart</h3>
    <Bar :data="chartData" :options="chartOptions" />
  </div>
  
</template>


<script setup lang="ts">

import { defineProps, computed } from 'vue'
import { Bar } from 'vue-chartjs'
import { Chart as ChartJS, Title, Tooltip, Legend, BarElement, CategoryScale, LinearScale } from 'chart.js'

ChartJS.register(Title, Tooltip, Legend, BarElement, CategoryScale, LinearScale)

const props = defineProps({
  containers: {
    type: Map,
    required: true,
  },
})

const chartData = computed(() => {
  const labels = []
  const data = []

  props.containers.forEach((container) => {
    labels.push(container.container.containerName.name)
    data.push(container.container.containerVolume)
  })

  return {
    labels: labels,
    datasets: [
      {
        label: 'Container Volume Chart',
        data: data,
        backgroundColor: 'rgba(75, 192, 192, 0.2)',
        borderColor: 'rgba(75, 192, 192, 1)',
        borderWidth: 1,
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
      text: 'Container Volume Chart',
    },
  },
}))

</script>
