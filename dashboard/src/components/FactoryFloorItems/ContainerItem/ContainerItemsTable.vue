<template>
  <div class="overflow-x-auto">
    <table class="min-w-full divide-y divide-gray-200">
      <thead>
      <tr>
        <th class="px-6 py-3 bg-gray-500">Contentor</th>
        <th class="px-6 py-3 bg-gray-500">Materiais</th>
        <th class="px-6 py-3 bg-gray-500">Volume Usado</th>
        <th class="px-6 py-3 bg-gray-500">Capacidade Total</th>
        <th class="px-6 py-3 bg-gray-500"></th>
      </tr>
      </thead>
      <tbody class="bg-white divide-y divide-gray-200">
      <template v-for="container in containers" :key="container.containerCode">
        <tr v-if="containerItems.filter(item => item.containerCode === container.containerCode).length > 0">
          <td class="px-6 py-4" :rowspan="containerItems.filter(item => item.containerCode === container.containerCode).length + 1">
            <div class="font-medium">{{ container.containerName }}</div>
            <div class="text-sm text-gray-500">{{ container.containerCode }}</div>
          </td>
        </tr>
        <tr v-for="item in containerItems.filter(item => item.containerCode === container.containerCode)"
            :key="`${item.containerCode}-${item.materialId}`">
          <td class="px-6 py-4">
            <div class="font-medium">{{ item.material?.materialName }}</div>
            <div class="text-sm">Quantidade: {{ item.quantity }}</div>
          </td>
          <td class="px-6 py-4">
            {{ (item.quantity * item.material?.unitVolume).toFixed(2) }} m³
          </td>
          <td class="px-6 py-4">
            {{ container.containerVolume.toFixed(2) }} m³
          </td>
          <td class="px-6 py-4 text-right">
            <button @click="$emit('edit', item)" class="text-indigo-600 hover:text-indigo-900 mr-4">
              Editar
            </button>
            <button @click="$emit('delete', item.containerCode, item.materialId)"
                    class="text-red-600 hover:text-red-900">
              Excluir
            </button>
          </td>
        </tr>
      </template>
      </tbody>
    </table>
  </div>
</template>

<script setup lang="ts">
defineProps<{
  items: any[];
  containers: any[];
  usedVolume: (containerCode: string) => number;
}>();

defineEmits(['edit', 'delete']);
</script>