<template>
  <div v-if="isOpen" class="fixed inset-0 bg-black/50 flex items-center justify-center">
    <div class="bg-white p-6 rounded-lg w-full max-w-md">
      <!-- Título Condicional -->
      <h2 class="text-xl font-bold mb-4">{{ localItem.containerId ? 'Edit Container' : 'Create Container' }}</h2>

      <form @submit.prevent="handleSubmit">
        <div class="space-y-4">
          <div>
            <label class="block mb-1">Code</label>
            <input
                v-model="localItem.containerCode"
                class="w-full p-2 border rounded"
                required
                type="text"
                :disabled="localItem.containerId !== 0"
            />
          </div>

          <div>
            <label class="block mb-1">Name</label>
            <input
                v-model="localItem.containerName"
                class="w-full p-2 border rounded"
                required
                type="text"
            />
          </div>

          <div>
            <label class="block mb-1">Volume</label>
            <input
                v-model.number="localItem.containerVolume"
                class="w-full p-2 border rounded"
                required
                type="number"
                step="0.01"
            />
          </div>

          <div class="flex items-center gap-2">
            <input v-model="localItem.activate" type="checkbox" class="cursor-pointer" />
            <label>Active</label>
          </div>
        </div>

        <div class="flex justify-end gap-2 mt-6">
          <Button  type="button" @click="closeModal">Cancel</Button>
          <Button  type="submit">
            {{ localItem.containerId ? 'Save' : 'Create' }}
          </Button>
        </div>
      </form>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { ref, watch, nextTick } from "vue";
import Button from "@/components/Button.vue"

const props = defineProps({
  isOpen: Boolean,
  Container: Object
});

const emit = defineEmits(["close", "submit"]);


const localItem = ref({
  containerId: 0,
  containerCode: "",
  containerName: "",
  containerVolume: 0,
  activate: false
});


watch(
    () => props.isOpen,
    (newVal) => {
      if (newVal) {
        if (localItem.value.containerId === 0) {
          localItem.value = {
            containerId: 0,
            containerCode: "",
            containerName: "",
            containerVolume: 0,
            activate: false
          };
        }
      }
    },
    { immediate: true }
);


watch(
    () => props.Container,
    (newVal) => {
      if (newVal && newVal.containerId && newVal.containerId !== 0) {

        localItem.value = { ...newVal };
      }
    },
    { immediate: true }
);

function handleSubmit() {
  emit("submit", { ...localItem.value });
  closeModal();
}

function closeModal() {
  emit("close");
}
</script>
