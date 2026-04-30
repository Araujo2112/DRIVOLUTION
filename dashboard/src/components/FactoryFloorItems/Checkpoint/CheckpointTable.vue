<template>
  <div>
    <table class="w-full">
      <thead>
      <tr class="bg-gray-500 text-white">
        <th class="px-4 py-2 text-left">ID</th>
        <th class="px-4 py-2 text-left">Name</th>
        <th class="px-4 py-2 text-left">Status</th>
        <th class="px-4 py-2 text-left">Section</th>
        <th class="px-4 py-2 text-right">Actions</th>
      </tr>
      </thead>
      <tbody>
      <tr
          v-for="checkpoint in paginatedCheckpoints"
          :key="checkpoint.checkpointId"
          class="border-b"
      >
        <td class="px-4 py-2 text-center">{{ checkpoint.checkpointId }}</td>
        <td class="px-4 py-2 text-center">{{ checkpoint.name }}</td>
        <td class="px-4 py-2 text-center">
            <span :class="checkpoint.status ? 'text-green-500' : 'text-red-500'">
              {{ checkpoint.status ? 'Active' : 'Inactive' }}
            </span>
        </td>
        <td class="px-4 py-2 text-center">{{ checkpoint.sectionName || '-' }}</td>
        <td class="px-4 py-2 flex gap-2 justify-end">
          <Button icon="edit" variant="ghost" @click="$emit('edit', checkpoint)" />
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
import type { Checkpoint } from "./CheckpointModal.vue";

const props = defineProps<{ checkpoints: Checkpoint[] }>();
defineEmits(["edit", "delete"]);

const currentPage = ref(1);
const pageSize = 10;

const sortedCheckpoints = computed(() =>
    props.checkpoints.slice().sort((a, b) => a.checkpointId - b.checkpointId)
);

const paginatedCheckpoints = computed(() => {
  const start = (currentPage.value - 1) * pageSize;
  return sortedCheckpoints.value.slice(start, start + pageSize);
});

const totalPages = computed(() =>
    Math.ceil(sortedCheckpoints.value.length / pageSize)
);

watch(
    () => props.checkpoints,
    () => {
      currentPage.value = 1;
    }
);
</script>
