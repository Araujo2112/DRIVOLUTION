<script lang="ts" setup>
import { ref, watch } from "vue";
import Button from "@/components/Button.vue"

export interface Checkpoint {
  checkpointCode: string;
  name: string;
  status: boolean;
  sectionId: string;
  sectionName?: string;
}

export interface PlantFloorSection {
  sectionId: number;
  sectionCode: string;
  name: string;
}

const props = defineProps<{
  isOpen: boolean;
  checkpoint: Checkpoint | null;
  plantFloorSections: PlantFloorSection[];
}>();

const emit = defineEmits<{
  (e: "close"): void;
  (e: "submit", checkpoint: Checkpoint): void;
}>();

const localCheckpoint = ref<Checkpoint>({
  checkpointCode: "",
  name: "",
  status: false,
  sectionId: ""
});

watch(
    () => props.checkpoint,
    (newVal) => {
      localCheckpoint.value = newVal
          ? { ...newVal }
          : { checkpointCode: "", name: "", status: false, sectionId: "" };
    },
    { immediate: true }
);

const onSubmit = () => {
  emit("submit", localCheckpoint.value);
  onClose();
};

const onClose = () => {
  localCheckpoint.value = { checkpointCode: "", name: "", status: false, sectionId: "" };
  emit("close");
};
</script>


<template>
  <div v-if="isOpen" class="fixed inset-0 flex items-center justify-center bg-black bg-opacity-50">
    <form
        class="bg-white p-6 rounded-lg shadow-lg w-full max-w-md"
        @submit.prevent="onSubmit"
    >
      <h2 class="text-xl font-semibold mb-4">Manage Checkpoint</h2>

      <div v-if="!checkpoint" class="mb-4">
        <label class="block text-sm font-medium text-gray-700">Checkpoint Code</label>
        <input
            v-model="localCheckpoint.checkpointCode"
            class="w-full mt-1 p-2 border rounded focus:outline-none focus:ring focus:ring-blue-300"
            placeholder="Checkpoint Code"
        />
      </div>

      <div class="mb-4">
        <label class="block text-sm font-medium text-gray-700">Checkpoint Name</label>
        <input
            v-model="localCheckpoint.name"
            class="w-full mt-1 p-2 border rounded focus:outline-none focus:ring focus:ring-blue-300"
            placeholder="Checkpoint Name"
        />
      </div>

      <div class="mb-4 flex items-center">
        <input
            v-model="localCheckpoint.status"
            type="checkbox"
            class="mr-2"
        />
        <label class="text-sm font-medium text-gray-700">Active</label>
      </div>

      <div class="mb-4">
        <label class="block text-sm font-medium text-gray-700">Section</label>
        <select
            v-model="localCheckpoint.sectionId"
            class="w-full mt-1 p-2 border rounded focus:outline-none focus:ring focus:ring-blue-300"
        >
          <option value="">Select a section</option>
          <option
              v-for="section in plantFloorSections"
              :key="section.sectionCode"
              :value="section.sectionId.toString()"
          >
            {{ section.name }}
          </option>
        </select>
      </div>

      <div class="flex justify-end gap-2 mt-4">
        <Button
            type="button"
            @click="onClose"
        >
          Cancel
        </Button>
        <Button
            type="submit"
        >
          Save
        </Button>
      </div>
    </form>
  </div>
</template>

