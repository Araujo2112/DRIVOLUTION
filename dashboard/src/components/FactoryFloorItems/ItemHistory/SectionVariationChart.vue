<script setup lang="ts">
import { computed, watchEffect } from 'vue';
import { Line } from 'vue-chartjs';
import { Chart as ChartJS, Title, Tooltip, Legend, LineElement, CategoryScale, LinearScale, PointElement } from 'chart.js';

ChartJS.register(Title, Tooltip, Legend, LineElement, CategoryScale, LinearScale, PointElement);

const props = defineProps({
  itemLocalizationDetails: {
    type: Object,
    required: true,
  }
});

const chartData = computed(() => {
  const labels: string[] = [];
  const data: number[] = [];
  const historyArray = props.itemLocalizationDetails.containerLocalization.localizationHistories.$values;
  if (historyArray && historyArray.length > 0) {
    historyArray.forEach((history: any) => {
      labels.push(new Date(history.datetime).toLocaleString());
      data.push(history.sectionId);
    });
  } else {
    console.log("Nenhum histórico de localização encontrado no item selecionado.");
  }
  return {
    labels,
    datasets: [
      {
        label: 'Section History',
        data,
        fill: false,
        borderColor: 'rgba(75, 192, 192, 1)',
        tension: 0.1,
      }
    ]
  };
});

const chartOptions = computed(() => ({
  responsive: true,
  plugins: {
    legend: { position: 'top' },
    title: { display: true, text: 'Location History' },
  },
  scales: { y: { beginAtZero: true, ticks: { precision: 0 } }  },
}));

watchEffect(() => {
  console.log("Dados do gráfico:", chartData.value);
});
</script>

<template>
  <div>
    <h3>Location History Chart</h3>
    <Line v-if="chartData.labels.length > 0" :data="chartData" :options="chartOptions" />
    <p v-else>No data available to display the chart.</p>
  </div>
</template>

