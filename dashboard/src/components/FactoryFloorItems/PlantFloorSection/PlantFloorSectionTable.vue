<template>
  <div>
    <table class="w-full">
      <thead>
      <tr class="bg-gray-500 text-white">
        <th class="text-left p-2">Code</th>
        <th class="text-left p-2">Name</th>
        <th class="text-right p-2">Actions</th>
      </tr>
      </thead>
      <tbody>
      <tr v-for="section in paginatedSections" :key="section.sectionCode" class="border-b">
        <td class="p-2">{{ section.sectionCode }}</td>
        <td class="p-2">{{ section.name }}</td>
        <td class="p-2 flex gap-2 justify-end">
          <Button icon="edit" variant="ghost" @click="$emit('edit', section)" />
          <Button icon="delete" variant="ghost" @click="$emit('delete', section.sectionCode)" />
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
import type { PlantFloorSection } from "@/types/Interfaces.ts"

const props = defineProps<{
  sections: PlantFloorSection[]
}>()

const currentPage = ref(1)
const pageSize = 10

const paginatedSections = computed(() => {
  const start = (currentPage.value - 1) * pageSize
  return props.sections.slice(start, start + pageSize)
})

const totalPages = computed(() =>
    Math.ceil(props.sections.length / pageSize)
)

watch(
    () => props.sections,
    () => {
      currentPage.value = 1
    }
)
</script>
