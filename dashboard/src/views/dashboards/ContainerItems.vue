<script lang="ts" setup>
import {computed, onMounted, ref} from "vue";
import Title from "@/components/Title.vue";
import Button from "@/components/Button.vue";
import ContainerItemsTable from "@/components/FactoryFloorItems/ContainerItem/ContainerItemsTable.vue";
import ContainerItemModal from "@/components/FactoryFloorItems/ContainerItem/ContainerItemModal.vue";
import DeleteConfirmModal from "@/components/FactoryFloorItems/DeleteConfirmModal.vue";

export interface ContainerItem {
  containerCode: string;
  materialId: string;
  quantity: number;
}

const containerItems = ref<any[]>([]);
const containers = ref<any[]>([]);
const materials = ref<any[]>([]);
const error = ref<string | null>(null);
const showModal = ref(false);
const showDeleteModal = ref(false);
const selectedItem = ref<any | null>(null);
const currentItemId = ref<string | null>(null);

async function fetchAllData() {
  try {
    const [itemsRes, containersRes, materialsRes] = await Promise.all([
      fetch("http://localhost:5181/api/ContainerItem"),
      fetch("http://localhost:5181/api/Container"),
      fetch("http://localhost:5181/api/Material")
    ]);

    if (!itemsRes.ok || !containersRes.ok || !materialsRes.ok) {
      throw new Error('Erro ao carregar dados');
    }

    containerItems.value = await itemsRes.json();
    containers.value = await containersRes.json();
    materials.value = await materialsRes.json();

  } catch (err: unknown) {
    error.value = err instanceof Error ? err.message : "Erro desconhecido";
  }
}

const usedVolume = computed(() => (containerCode: string) => {
  const container = containers.value.find(c => c.containerCode === containerCode);
  if (!container) return 0;

  return containerItems.value
      .filter(item => item.containerCode === containerCode)
      .reduce((acc, item) => acc + (item.quantity * (item.material?.unitVolume || 0)), 0);
});

async function handleSubmit(itemData: any) {
  try {
    const url = selectedItem.value
        ? `http://localhost:5181/api/ContainerItem/${selectedItem.value.containerCode}/${selectedItem.value.materialId}`
        : 'http://localhost:5181/api/ContainerItem';

    const method = selectedItem.value ? 'PUT' : 'POST';

    const response = await fetch(url, {
      method,
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(itemData)
    });

    if (!response.ok) throw new Error('Falha ao salvar');

    await fetchAllData();
    showModal.value = false;
  } catch (err) {
    error.value = err instanceof Error ? err.message : "Erro ao salvar";
  }
}

async function deleteItem() {
  if (!currentItemId.value) return;

  try {
    const [containerCode, materialId] = currentItemId.value.split('|');
    const response = await fetch(
        `http://localhost:5181/api/ContainerItem/${containerCode}/${materialId}`,
        {method: 'DELETE'}
    );

    if (!response.ok) throw new Error('Falha ao excluir');

    await fetchAllData();
    currentItemId.value = null;
  } catch (err) {
    error.value = err instanceof Error ? err.message : "Erro ao excluir";
  }
}

function openEdit(item: any) {
  selectedItem.value = {...item};
  showModal.value = true;
}

function openDelete(containerCode: string, materialId: string) {
  currentItemId.value = `${containerCode}|${materialId}`;
  showDeleteModal.value = true;
}

onMounted(fetchAllData);
</script>

<template>
  <div>
    <Title>Itens em Contentores</Title>
    <div class="w-full flex flex-col gap-2 items-end mb-2">
      <Button icon="add" @click="showModal = true">Nova Associação</Button>
    </div>

    <ContainerItemsTable
        :containers="containers"
        :items="containerItems"
        :usedVolume="usedVolume"
        @delete="openDelete"
        @edit="openEdit"
    />

    <ContainerItemModal
        :containers="containers"
        :isOpen="showModal"
        :item="selectedItem"
        :materials="materials"
        :usedVolume="usedVolume"
        @close="showModal = false"
        @submit="handleSubmit"
    />

    <DeleteConfirmModal
        :isOpen="showDeleteModal"
        @close="showDeleteModal = false"
        @confirm="deleteItem"
    />
  </div>
</template>