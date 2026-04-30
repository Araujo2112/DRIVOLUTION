<script lang="ts" setup>
import { onMounted, ref } from "vue";
import Title from "@/components/Title.vue";
import Button from "@/components/Button.vue";
import ContainersTable from "@/components/FactoryFloorItems/Containers/ContainersTable.vue";
import DeleteConfirmModal from "@/components/FactoryFloorItems/DeleteConfirmModal.vue";
import ContainersModal from "@/components/FactoryFloorItems/Containers/ContainersModal.vue";
import { Containers } from "@/types/Interfaces.ts";

import {
  deleteContainers,
  fetchAllContainers,
  handleSubmit,
  openDelete,
  openEdit
} from "@/components/FactoryFloorItems/Containers/Functions.ts";

const containers = ref<Containers[]>([]);
const error = ref<string | null>(null);
const showModal = ref(false);
const showDeleteModal = ref(false);
const selectedContainer = ref<Containers | null>(null);
const currentContainerId = ref<number | null>(null);
const modalKey = ref(0);

onMounted(async () => {
  try {
    containers.value = await fetchAllContainers();
  } catch (err) {
    error.value = err instanceof Error ? err.message : "Erro desconhecido";
  }
});

const handleSubmitWrapper = async (data: Containers) => {
  try {
    const updatedContainers = await handleSubmit(data, selectedContainer, showModal);
    containers.value = updatedContainers;
    closeModal();
  } catch (err) {
    error.value = err instanceof Error ? err.message : "Erro ao salvar";
  }
};

const deleteContainersWrapper = async () => {
  if (currentContainerId.value === null) return;
  try {
    const updatedContainers = await deleteContainers(currentContainerId.value, showDeleteModal);
    containers.value = updatedContainers;
    currentContainerId.value = null;
  } catch (err) {
    error.value = err instanceof Error ? err.message : "Erro ao excluir";
  }
};


const openEditWrapper = (container: Containers) => {
  openEdit(container, selectedContainer, showModal);
  modalKey.value++;
};


const openCreateWrapper = () => {
  selectedContainer.value = undefined;
  showModal.value = true;
  modalKey.value++;
};


const openDeleteWrapper = (id: number) => {
  openDelete(id, currentContainerId, showDeleteModal);
};


const closeModal = () => {
  showModal.value = false;
  selectedContainer.value = null;
  modalKey.value++;
};
</script>

<template>
  <Title>Containers</Title>

  <div class="w-full flex flex-col gap-2 items-end mb-2">
    <Button icon="add" @click="openCreateWrapper">New Container</Button>
  </div>

  <ContainersTable
      :containers="containers"
      @delete="openDeleteWrapper"
      @edit="openEditWrapper"
  />

  <ContainersModal
      :key="modalKey"
      :isOpen="showModal"
      :Container="selectedContainer"
      @close="closeModal"
      @submit="handleSubmitWrapper"
  />

  <DeleteConfirmModal
      :isOpen="showDeleteModal"
      @close="showDeleteModal = false"
      @confirm="deleteContainersWrapper"
  />
</template>
