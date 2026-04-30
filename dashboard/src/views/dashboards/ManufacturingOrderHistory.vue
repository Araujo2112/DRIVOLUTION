<script lang="ts" setup>
import { ref, onMounted } from 'vue'

interface ManufacturingOrderHistoryDTO {
  manufacturingOrderId: number
  plantFloorSectionId: number
  dateTime: string
  statusName: string
}

const histories = ref<ManufacturingOrderHistoryDTO[]>([])
const error = ref<string | null>(null)

async function fetchHistories() {
  try {
    const res = await fetch('http://localhost:5181/api/ManufacturingOrderHistory')
    histories.value = await res.json()
  } catch (err) {
    error.value = 'Erro ao buscar dados.'
  }
}

onMounted(fetchHistories)
</script>

<template>
  <div class="p-6">
    <h1 class="text-2xl font-bold mb-4">Histórico de Ordens de Fabrico</h1>

    <table class="w-full table-auto border">
      <thead>
      <tr class="bg-gray-500">
        <th class="p-2 text-left">Order ID</th>
        <th class="p-2 text-left">Section ID</th>
        <th class="p-2 text-left">Status</th>
        <th class="p-2 text-left">Data/Hora</th>
      </tr>
      </thead>
      <tbody>
      <tr
          v-for="h in histories"
          :key="`${h.manufacturingOrderId}-${h.plantFloorSectionId}`"
          class="border-t"
      >
        <td class="p-2">{{ h.manufacturingOrderId }}</td>
        <td class="p-2">{{ h.plantFloorSectionId }}</td>
        <td class="p-2">{{ h.statusName }}</td>
        <td class="p-2">{{ new Date(h.dateTime).toLocaleString() }}</td>
      </tr>
      </tbody>
    </table>

    <div v-if="error" class="text-red-600 mt-4">{{ error }}</div>
  </div>
</template>
