<template>
  <div v-if="isOpen" class="fixed inset-0 bg-black/50 flex items-center justify-center">
    <div class="bg-white p-6 rounded-lg w-full max-w-md">
      <h2 class="text-xl font-bold mb-4">
        {{ item ? "Editar Item" : "Novo Item" }}
      </h2>

      <form @submit.prevent="handleSubmit">
        <div class="space-y-4">
          <div>
            <label class="block mb-1">Código</label>
            <input
                v-model="localItem.itemCode"
                class="w-full p-2 border rounded"
                disabled
                type="text"
            />
          </div>

          <div>
            <label class="block mb-1">Quantidade</label>
            <input
                v-model.number="localItem.quantity"
                class="w-full p-2 border rounded"
                required
                min="0"
                type="number"
            />
          </div>

          <div>
            <label class="block mb-1">Unidade</label>
            <select
                v-model="localItem.unit"
                class="w-full p-2 border rounded"
                required
            >
              <option value="Kilo">Kilo</option>
              <option value="Unidades">Unidades</option>
            </select>
          </div>

          <div>
            <label class="block mb-1">Lote de Matéria Prima</label>
            <select
                v-model.number="localItem.lotOfRawMaterialId"
                class="w-full p-2 border rounded"
                required
            >
              <option :value="null" disabled>Selecione um Lote</option>
              <option
                  v-for="lot in lots"
                  :key="lot.lotId"
                  :value="lot.lotId"
              >
                {{ lot.lotNumber.name }} (ID: {{ lot.lotId }})
              </option>
            </select>
          </div>

          <div>
            <label class="block mb-1">Container</label>
            <select
                v-model.number="localItem.itemInContainerId"
                class="w-full p-2 border rounded"
                required
            >
              <option :value="null" disabled>Selecione um Container</option>
              <option
                  v-for="container in containers"
                  :key="container.containerId"
                  :value="container.containerId"
              >
                {{ container.containerCode }} - {{ container.containerName }}
              </option>
            </select>
          </div>
        </div>

        <div class="flex justify-end gap-2 mt-6">
          <button
              type="button"
              class="px-4 py-2 border rounded hover:bg-gray-100"
              @click="$emit('close')"
          >
            Cancelar
          </button>
          <button
              type="submit"
              class="px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600"
          >
            Salvar
          </button>
        </div>
      </form>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { ref, watch, onMounted } from "vue";
import type { ItemOfRawMaterial, LotRawMaterial, Containers } from "@/types/Interfaces.ts";
import { fetchLots } from "./Functions.ts";
import {fetchAllContainers} from "../Containers/Functions.ts"

const props = defineProps({
  isOpen: Boolean,
  item: Object as () => ItemOfRawMaterial | null
});

const emit = defineEmits(["close", "submit"]);

const localItem = ref<ItemOfRawMaterial>({
  itemRawId: 0,
  itemCode: "" ,
  quantity: 0,
  unit: "Kilo",
  lotOfRawMaterialId: null,
  itemInContainerId: null
});

const lots = ref<LotRawMaterial[]>([]);
const containers = ref<Containers[]>([]);

onMounted(async () => {
  try {
    const [lotsData, containersData] = await Promise.all([
      fetchLots(),
      fetchAllContainers()
    ]);

    lots.value = lotsData;
    containers.value = containersData;
  } catch (error) {
    console.error("Erro ao carregar dados:", error);
  }
});

watch(
    () => props.item,
    (newVal) => {
      if (newVal) {
        localItem.value = { ...newVal };
      } else {
        resetForm();
      }
    },
    { deep: true }
);

const resetForm = () => {
  localItem.value = {
    itemRawId: 0,
    itemCode: "",
    quantity: 0,
    unit: "Kilo",
    lotOfRawMaterialId: null,
    itemInContainerId: null
  };
};

const handleSubmit = () => {
  if (!localItem.value.lotOfRawMaterialId || !localItem.value.itemInContainerId) {
    alert("Selecione um lote e um container!");
    return;
  }
  emit("submit", localItem.value);
  resetForm();
};
</script>

<style scoped>
select {
  appearance: none;
  background-image: url("data:image/svg+xml;charset=UTF-8,%3csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='none' stroke='currentColor' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'%3e%3cpolyline points='6 9 12 15 18 9'%3e%3c/polyline%3e%3c/svg%3e");
  background-repeat: no-repeat;
  background-position: right 0.5rem center;
  background-size: 1em;
}
</style>