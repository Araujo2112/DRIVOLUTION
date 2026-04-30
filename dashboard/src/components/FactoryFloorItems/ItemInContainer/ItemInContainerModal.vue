<template>
  <div v-if="isOpen" class="fixed inset-0 bg-black/50 flex items-center justify-center p-4">
    <div class="bg-white p-6 rounded-lg w-full max-w-md">
      <h2 class="text-xl font-bold mb-4">
        {{ item ? "Editar Associação" : "Nova Associação" }}
      </h2>

      <form @submit.prevent="$emit('submit', formData)">
        <div class="space-y-4">
          <div>
            <label class="block mb-1">Código do Item</label>
            <input
                v-model="formData.itemCode"
                class="w-full p-2 border rounded"
                required 
                type="text"
            />
          </div>
          <div>
            <label class="block mb-1">Contentor</label>
            <select
                v-model="formData.containerId"
                class="w-full p-2 border rounded"
                required
            >
              <option
                  v-for="container in containers"
                  :key="container.containerId"
                  :value="container.containerId"
              >
                {{ container.containerName }} ({{ container.containerCode }})
              </option>
            </select>
          </div>
        </div>

        <div class="flex justify-end gap-2 mt-6">
          <button
              type="button"
              class="px-4 py-2 border rounded"
              @click="$emit('close')"
          >
            Cancelar
          </button>
          <button
              type="submit"
              class="px-4 py-2 bg-blue-500 text-white rounded"
          >
            Salvar
          </button>
        </div>
      </form>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { ref, watch } from "vue";
import type { ItemInContainer, Containers } from "@/types/Interfaces";

const props = defineProps<{
  isOpen: boolean;
  item?: ItemInContainer | null;
  containers: Containers[];
}>();

const emit = defineEmits(["close", "submit"]);

const formData = ref<ItemInContainer>({
  itemCode: "",
  containerId: 0,
  dateTimeIn: 0,
  dateTimeOut: 0
});

watch(() => props.item, (newVal) => {
  if (newVal) {
    formData.value = { ...newVal };
  } else {
    formData.value = {
      itemCode: "",
      containerId: 0,
      dateTimeIn: 0,
      dateTimeOut: 0
    };
  }
});
</script>