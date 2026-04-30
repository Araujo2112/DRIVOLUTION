<template>
  <div>
    <table class="w-full">
      <thead>
      <tr class="bg-gray-500 text-white">
        <th class="text-left p-2">Container Identifier</th>
        <th class="text-left p-2">Container Code</th>
        <th class="text-left p-2">Container Name</th>
        <th class="text-left p-2">Load Capacity</th>
        <th class="text-left p-2">Active?</th>
        <th class="text-right p-2">Actions</th>
      </tr>
      </thead>
      <tbody>
      <tr
          v-for="container in paginatedContainers"
          :key="container.containerCode"
          class="border-b"
      >
        <td class="p-2">{{ container.containerId }}</td>
        <td class="p-2">{{ container.containerCode }}</td>
        <td class="p-2">{{ container.containerName }}</td>
        <td class="p-2">{{ container.containerVolume }}</td>
        <td class="p-2">{{ container.activate ? "Yes" : "No" }}</td>
        <td class="p-2 flex gap-2 justify-end">
          <Button icon="edit" variant="ghost" @click="$emit('edit', container)" />
          <Button icon="delete" variant="ghost" @click="$emit('delete', container.containerCode)" />
          <router-link :to="`contentores/${container.containerId}`">
            <Button icon="visibility" />
          </router-link>
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
import { ref, computed, watch } from "vue";
import Button from "@/components/Button.vue";
import type { Containers } from "@/types/Interfaces.ts";

const props = defineProps<{ containers: Containers[] }>();
defineEmits(["edit", "delete"]);

const currentPage = ref(1);
const pageSize = 10;

const paginatedContainers = computed(() => {
  const start = (currentPage.value - 1) * pageSize;
  return props.containers.slice(start, start + pageSize);
});

const totalPages = computed(() => Math.ceil(props.containers.length / pageSize));

watch(
    () => props.containers,
    () => {
      currentPage.value = 1;
    }
);
</script>
