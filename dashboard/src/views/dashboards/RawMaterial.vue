<script lang="ts" setup>
import {onMounted, ref} from "vue";
import Title from "@/components/Title.vue";
import Button from "@/components/Button.vue";
import RawMaterialTable from "@/components/FactoryFloorItems/RawMaterial/RawMaterialTable.vue";
import RawMaterialModal from "@/components/FactoryFloorItems/RawMaterial/RawMaterialModal.vue";
import DeleteConfirmModal from "@/components/FactoryFloorItems/DeleteConfirmModal.vue";
import {RawMaterial} from "@/types/Interfaces.ts";
import {
  deleteRawMaterial,
  fetchAllRawMaterials,
  handleSubmit
} from "@/components/FactoryFloorItems/RawMaterial/Functions.ts";

const rawMaterials = ref<RawMaterial[]>([]);
const error = ref<string | null>(null);
const showModal = ref(false);
const showDeleteModal = ref(false);
const selectedRawMaterial = ref<RawMaterial | null>(null);
const currentRawMaterialId = ref<number | null>(null);

const loadRawMaterials = async () => {
  try {
    rawMaterials.value = await fetchAllRawMaterials();
  } catch (err) {
    error.value = err instanceof Error ? err.message : "Erro desconhecido";
  }
};

onMounted(loadRawMaterials);

const handleSubmitWrapper = async (data: RawMaterial) => {
  try {
    await handleSubmit(data, selectedRawMaterial, showModal);
    await loadRawMaterials();
    showModal.value = false;
    selectedRawMaterial.value = null;
  } catch (err) {
    error.value = err instanceof Error ? err.message : "Erro ao salvar";
  }
};

const deleteRawMaterialWrapper = async () => {
  if (currentRawMaterialId.value === null) return;
  try {
    await deleteRawMaterial(currentRawMaterialId.value, showDeleteModal);
    await loadRawMaterials();
    currentRawMaterialId.value = null;
    showDeleteModal.value = false;
  } catch (err) {
    error.value = err instanceof Error ? err.message : "Erro ao excluir";
  }
};

const openEditWrapper = (rawMaterial: RawMaterial) => {
  selectedRawMaterial.value = { ...rawMaterial };
  showModal.value = true;
};

const openDeleteWrapper = (id: number) => {
  currentRawMaterialId.value = id;
  showDeleteModal.value = true;
};

const closeModal = () => {
  showModal.value = false;
  selectedRawMaterial.value = null;
};

const closeDeleteModal = () => {
  showDeleteModal.value = false;
  currentRawMaterialId.value = null;
};
</script>

<template>
  <Title>Raw Material</Title>
  <div>
    <div class="w-full flex flex-col gap-2 items-end mb-2">
      <Button icon="add" @click="showModal = true">New Raw Material</Button>
    </div>

    <RawMaterialTable
        :rawMaterials="rawMaterials"
        @delete="openDeleteWrapper"
        @edit="openEditWrapper"
    />

    <RawMaterialModal
        :isOpen="showModal"
        :rawMaterial="selectedRawMaterial"
        @close="closeModal"
        @submit="handleSubmitWrapper"
    />

    <DeleteConfirmModal
        :isOpen="showDeleteModal"
        @close="closeDeleteModal"
        @confirm="deleteRawMaterialWrapper"
    />
  </div>
</template>
