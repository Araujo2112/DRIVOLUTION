<script lang="ts" setup>
import {onMounted, ref} from "vue";
import Button from "@/components/Button.vue";
import ItemOfRawMaterialTable from "@/components/FactoryFloorItems/ItemOfRawMaterial/ItemOfRawMaterialTable.vue";
import ItemOfRawMaterialModal from "@/components/FactoryFloorItems/ItemOfRawMaterial/ItemOfRawMaterialModal.vue";
import DeleteConfirmModal from "@/components/FactoryFloorItems/DeleteConfirmModal.vue";
import {
  deleteItemOfRawMaterial,
  fetchAllItems,
  handleSubmit
} from "@/components/FactoryFloorItems/ItemOfRawMaterial/Functions.ts";
import type {ItemOfRawMaterial} from "@/types/Interfaces.ts";

const items = ref<ItemOfRawMaterial[]>([]);
const error = ref<string | null>(null);
const showModal = ref(false);
const showDeleteModal = ref(false);
const selectedItem = ref<ItemOfRawMaterial | null>(null);
const currentItemCode = ref<string | null>(null);

onMounted(async () => {
  console.log('Componente montado');
  await loadItems();
});

const loadItems = async () => {
  console.log('Carregando itens...');
  try {
    const data = await fetchAllItems();
    console.log('Dados carregados:', data);
    items.value = Array.isArray(data) ? data : data.$values;
    console.log('Items processados:', items.value);
  } catch (err) {
    console.error('Erro ao carregar:', err);
    error.value = err instanceof Error ? err.message : "Erro desconhecido";
  }
};

const handleFormSubmit = async (data: ItemOfRawMaterial) => {
  console.log('Submitting:', JSON.stringify(data));
  try {
    const updatedItems = await handleSubmit(data, selectedItem);
    console.log('Updated items:', updatedItems);
    items.value = updatedItems;
  } catch (error) {
    console.error('Submission error:', error);
  }
};
const confirmDelete = async () => {
  console.log('Confirmando exclusão:', currentItemCode.value);
  if (!currentItemCode.value) return;

  try {
    const updatedItems = await deleteItemOfRawMaterial(currentItemCode.value);
    console.log('Após exclusão:', updatedItems);
    items.value = updatedItems;
    showDeleteModal.value = false;
    currentItemCode.value = null;
  } catch (err) {
    console.error('Erro na exclusão:', err);
    error.value = err instanceof Error ? err.message : "Erro ao excluir";
  }
};
</script>

<template>
  <div>
    <div class="w-full flex flex-col gap-2 items-end mb-2">
      <Button icon="add" @click="showModal = true">Novo Item de Matéria Prima</Button>
    </div>

    <ItemOfRawMaterialTable
        :items="items"
        @delete="(itemCode: string) => { 
        console.log('Evento delete:', itemCode);
        currentItemCode = itemCode; 
        showDeleteModal = true; 
      }"
        @edit="(item: ItemOfRawMaterial) => { 
        console.log('Evento edit:', item);
        selectedItem = item; 
        showModal = true; 
      }"
    />

    <ItemOfRawMaterialModal
        :isOpen="showModal"
        :item="selectedItem"
        @close="showModal = false"
        @submit="handleFormSubmit"
    />

    <DeleteConfirmModal
        :isOpen="showDeleteModal"
        @close="showDeleteModal = false"
        @confirm="confirmDelete"
    />
  </div>
</template>