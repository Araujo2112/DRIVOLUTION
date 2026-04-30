<template>
  <div>
    <table class="w-full">
      <thead>
      <tr class="bg-gray-500 text-white">
        <th class="text-left p-2">ID</th>
        <th class="text-left p-2">Name</th>
        <th class="text-left p-2">Information</th>
        <th class="text-right p-2">Actions</th>
      </tr>
      </thead>
      <tbody>
      <tr v-for="rawMaterial in paginatedRawMaterials" :key="rawMaterial.rawId" class="border-b">
        <td class="p-2">{{ rawMaterial.rawId }}</td>
        <td class="p-2">{{ rawMaterial.name }}</td>
        <td class="p-2">{{ rawMaterial.info }}</td>
        <td class="p-2 flex gap-2 justify-end">
          <Button icon="edit" variant="ghost" @click="$emit('edit', rawMaterial)" />
          <Button icon="delete" variant="ghost" @click="$emit('delete', rawMaterial.rawId)" />
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
import type { RawMaterial } from '@/types/Interfaces'

const props = defineProps<{
  rawMaterials: RawMaterial[]
}>()

const currentPage = ref(1)
const pageSize = 10

const paginatedRawMaterials = computed(() => {
  const start = (currentPage.value - 1) * pageSize
  return props.rawMaterials.slice(start, start + pageSize)
})

const totalPages = computed(() =>
    Math.ceil(props.rawMaterials.length / pageSize)
)

watch(
    () => props.rawMaterials,
    () => {
      currentPage.value = 1
    }
)
</script>
