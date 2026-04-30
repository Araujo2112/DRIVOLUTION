<script lang="ts" setup>
import {onMounted, ref} from "vue";
import Title from "@/components/Title.vue";
import Button from "@/components/Button.vue";
import PlantFloorSectionTable from "@/components/FactoryFloorItems/PlantFloorSection/PlantFloorSectionTable.vue";
import PlantFloorSectionModal from "@/components/FactoryFloorItems/PlantFloorSection/PlantFloorSectionModal.vue";
import DeleteConfirmModal from "@/components/FactoryFloorItems/DeleteConfirmModal.vue";
import type {PlantFloorSection} from "@/types/Interfaces.ts";

const sections = ref<PlantFloorSection[]>([]);
const checkpoints = ref<any[]>([]);
const error = ref<string | null>(null);
const showModal = ref(false);
const showDeleteModal = ref(false);
const selectedSection = ref<PlantFloorSection | null>(null);
const currentSectionCode = ref<string | null>(null);

async function fetchAllData() {
  try {
    const [sectionsRes, checkpointsRes] = await Promise.all([
      fetch("http://localhost:5181/api/PlantFloorSection"),
      fetch("http://localhost:5181/api/Checkpoint")
    ]);
    if (!sectionsRes.ok || !checkpointsRes.ok) {
      throw new Error("Erro ao carregar dados");
    }
    const sectionsData = await sectionsRes.json();
    sections.value = sectionsData.map((item: any) => ({
      sectionId: item.sectionId,
      sectionCode: item.id?.name,
      name: item.name,
      checkpointIds: item.checkpoints ? item.checkpoints.map((cp: any) => cp.checkpointCode) : []
    }));
    checkpoints.value = await checkpointsRes.json();
  } catch (err: unknown) {
    error.value = err instanceof Error ? err.message : "Erro desconhecido";
  }
}

function openNewSection() {
  selectedSection.value = null;
  showModal.value = true;
}

function openEdit(section: PlantFloorSection) {
  selectedSection.value = section;
  showModal.value = true;
}

function closeModal() {
  showModal.value = false;
  selectedSection.value = null;
}

async function handleSubmit(formData: any) {
  try {
    let url = "http://localhost:5181/api/PlantFloorSection";
    let method = "POST";
    let body: any = {};
    if (selectedSection.value) {
      url += `/${selectedSection.value.sectionCode}`;
      method = "PUT";
      const associatedCheckpoints = checkpoints.value.filter((cp: any) =>
          formData.checkpointIds.includes(cp.checkpointCode)
      );
      body = {
        sectionId: selectedSection.value.sectionId,
        sectionCode: selectedSection.value.sectionCode,
        name: formData.name,
        checkpoints: associatedCheckpoints
      };
    } else {
      body = {
        sectionCode: formData.sectionCode,
        name: formData.name
      };
    }
    const response = await fetch(url, {
      method,
      headers: {"Content-Type": "application/json"},
      body: JSON.stringify(body)
    });
    if (!response.ok) {
      throw new Error("Falha ao salvar");
    }
    await fetchAllData();
    closeModal();
  } catch (err: unknown) {
    error.value = err instanceof Error ? err.message : "Erro ao salvar";
  }
}

function onDelete(sectionCode: string | null) {
  if (!sectionCode) return;
  currentSectionCode.value = sectionCode;
  showDeleteModal.value = true;
}

async function deleteSection() {
  if (!currentSectionCode.value) return;
  try {
    const response = await fetch(
        `http://localhost:5181/api/PlantFloorSection/${currentSectionCode.value}`,
        {method: "DELETE"}
    );
    if (!response.ok) {
      throw new Error("Falha ao excluir");
    }
    await fetchAllData();
    currentSectionCode.value = null;
    showDeleteModal.value = false;
  } catch (err: unknown) {
    error.value = err instanceof Error ? err.message : "Erro ao excluir";
  }
}

onMounted(fetchAllData);
</script>

<template>
  <div>
    <Title>Factory Sections</Title>
    <div class="w-full flex flex-col gap-2 items-end mb-2">
      <Button icon="add" @click="openNewSection">New Section</Button>
    </div>
    <PlantFloorSectionTable
        :sections="sections"
        @delete="onDelete"
        @edit="openEdit"
    />
    <PlantFloorSectionModal
        :checkpoints="checkpoints"
        :isOpen="showModal"
        :section="selectedSection"
        @close="closeModal"
        @submit="handleSubmit"
    />
    <DeleteConfirmModal
        :isOpen="showDeleteModal"
        @close="() => (showDeleteModal.value = false)"
        @confirm="deleteSection"
    />
  </div>
</template>
