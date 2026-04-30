<template>
  <div v-if="isOpen" class="fixed inset-0 bg-black/50 flex items-center justify-center">
    <div class="bg-white p-6 rounded-lg w-full max-w-md">
      <h2 class="text-xl font-bold mb-4">
        {{ section ? "Edit Section" : "New Section" }}
      </h2>

      <form @submit.prevent="onSubmit">
        <div class="space-y-4">
          <div v-if="!section">
            <label class="block mb-1">Section Code</label>
            <input
                v-model="localData.sectionCode"
                class="w-full p-2 border rounded"
                required
                type="text"
            />
          </div>

          <div>
            <label class="block mb-1">Name</label>
            <input
                v-model="localData.name"
                class="w-full p-2 border rounded"
                required
                type="text"
            />
          </div>
        </div>

        <div class="flex justify-end gap-2 mt-6">
          <Button type="button" variant="outline" @click="$emit('close')">
            Cancel
          </Button>
          <Button type="submit">Save</Button>
        </div>
      </form>
    </div>
  </div>
</template>

<script lang="ts" setup>
import {ref, watch} from "vue";
import Button from "@/components/Button.vue";
import type {PlantFloorSection} from "@/types/Interfaces.ts";

const props = defineProps<{
  isOpen: boolean;
  section?: PlantFloorSection;
  checkpoints: any[];
}>();

const emit = defineEmits(["close", "submit"]);

interface LocalData {
  sectionCode: string;
  name: string;
  checkpointIds: string[];
}

const localData = ref<LocalData>({
  sectionCode: "",
  name: "",
  checkpointIds: []
});

watch(
    () => props.section,
    (newSection) => {
      if (newSection) {
        localData.value = {
          sectionCode: newSection.sectionCode,
          name: newSection.name,
          checkpointIds: newSection.checkpointIds ? [...newSection.checkpointIds] : []
        };
      } else {
        resetForm();
      }
    },
    {immediate: true}
);

function onSubmit() {
  emit("submit", {...localData.value});
  resetForm();
}

function resetForm() {
  localData.value = {
    sectionCode: "",
    name: "",
    checkpointIds: []
  };
}
</script>
