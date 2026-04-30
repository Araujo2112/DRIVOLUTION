<template>
  <div>
    <table class="w-full">
      <thead>
      <tr class="bg-gray-500 text-white">
        <th class="text-left p-2">Lot Number</th>
        <th class="text-left p-2">Lot Identifier</th>
        <th class="text-left p-2">Quantity</th>
        <th class="text-left p-2">Unit</th>
        <th class="text-left p-2">Raw Material ID</th>
        <th class="text-right p-2">Actions</th>
      </tr>
      </thead>
      <tbody>
      <tr v-for="lot in paginatedLots" :key="lot.lotId" class="border-b">
        <td class="p-2">{{ lot.lotId }}</td>
        <td class="p-2">{{ lot.lotCode }}</td>
        <td class="p-2">{{ lot.lotQuantity }}</td>
        <td class="p-2">{{ lot.lotUnit }}</td>
        <td class="p-2">{{ lot.rawMaterialId }}</td>
        <td class="p-2 flex gap-2 justify-end">
          <Button icon="edit" variant="ghost" @click="$emit('edit', lot)" />
        </td>
      </tr>
      </tbody>
    </table>

    <div class="flex justify-center items-center mt-4 gap-4">
      <Button variant="outline" :disabled="currentPage === 1" @click="currentPage--">Previous</Button>
      <span>Page {{ currentPage }} of {{ totalPages }}</span>
      <Button variant="outline" :disabled="currentPage === totalPages" @click="currentPage++">Next</Button>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { ref, computed, watch } from 'vue'
import Button from "@/components/Button.vue"
import { LotRawMaterial } from "@/types/Interfaces.ts"

const props = defineProps<{
  lotsRawMaterial: LotRawMaterial[]
}>()

const currentPage = ref(1)
const pageSize = 10

const paginatedLots = computed(() => {
  const start = (currentPage.value - 1) * pageSize
  return props.lotsRawMaterial.slice(start, start + pageSize)
})

const totalPages = computed(() =>
    Math.ceil(props.lotsRawMaterial.length / pageSize)
)

watch(
    () => props.lotsRawMaterial,
    () => {
      currentPage.value = 1
    }
)
</script>
