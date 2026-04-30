<template>
  <div>
    <Title>Checkpoints</Title>
    <div class="w-full flex flex-col gap-2 items-end mb-2">
      <Button icon="add" @click="openNewCheckpoint">New Checkpoint</Button>
    </div>
    <CheckpointTable
        :checkpoints="checkpoints"
        @edit="openEdit"
        @delete="openDelete"
    />
    <CheckpointModal
        :key="modalKey"
        :isOpen="showModal"
        :checkpoint="selectedCheckpoint"
        :plantFloorSections="plantFloorSections"
        @close="closeModal"
        @submit="handleSubmit"
    />
    <DeleteConfirmModal
        :isOpen="showDeleteModal"
        @close="() => (showDeleteModal = false)"
        @confirm="deleteCheckpoint"
    />
  </div>
</template>

<script lang="ts" setup>
import {onMounted, ref} from "vue";
import Title from "@/components/Title.vue";
import Button from "@/components/Button.vue";
import CheckpointTable from "@/components/FactoryFloorItems/Checkpoint/CheckpointTable.vue";
import CheckpointModal from "@/components/FactoryFloorItems/Checkpoint/CheckpointModal.vue";
import DeleteConfirmModal from "@/components/FactoryFloorItems/DeleteConfirmModal.vue";

export interface Checkpoint {
  checkpointId: number;
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

const checkpoints = ref<Checkpoint[]>([]);
const plantFloorSections = ref<PlantFloorSection[]>([]);
const error = ref<string | null>(null);
const showModal = ref(false);
const showDeleteModal = ref(false);
const selectedCheckpoint = ref<Checkpoint | null>(null);
const currentCheckpointId = ref<string | null>(null);
const modalKey = ref(0);

async function fetchAllCheckpoints() {
  try {
    const response = await fetch("http://localhost:5181/api/Checkpoint");
    if (!response.ok) throw new Error(`Erro HTTP: ${response.status}`);

    let checkpointsArray = (await response.json()) ?? [];
    const objectMap = new Map();
    checkpointsArray.forEach((cp) => {
      if (cp["$id"]) objectMap.set(cp["$id"], cp);
    });

    const resolveRef = (obj) => obj?.["$ref"] ? objectMap.get(obj["$ref"]) ?? null : obj;

    checkpoints.value = checkpointsArray.map((cp) => {
      cp = resolveRef(cp);
      return {
        checkpointId: cp?.checkpointId ?? "",
        checkpointCode: cp?.id?.name ?? "",
        name: cp?.name?.name ?? "",
        status: cp?.status ?? false,
        sectionId: cp?.sectionId ?? null,
        sectionName: plantFloorSections.value.find(s => s.sectionId === cp?.sectionId)?.name || "-"
      };
    });
  } catch (error) {
    console.error("Erro ao buscar checkpoints:", error);
  }
}

async function fetchAllPlantFloorSections() {
  try {
    const response = await fetch("http://localhost:5181/api/PlantFloorSection");
    if (!response.ok) throw new Error(`Erro HTTP: ${response.status}`);

    const data = await response.json();
    plantFloorSections.value = data.map((sec: any) => ({
      sectionId: sec.sectionId,
      sectionCode: sec.id?.name || "",
      name: sec.name || ""
    }));
  } catch (error) {
    console.error("Erro ao buscar seções:", error);
  }
}

async function handleSubmit(checkpointData: Checkpoint) {
  const payload = {
    checkpointCode: String(checkpointData.checkpointCode),
    name: String(checkpointData.name),
    status: checkpointData.status,
    sectionId: checkpointData.sectionId ? Number(checkpointData.sectionId) : null
  };

  const isEdit = !!selectedCheckpoint.value?.checkpointId;
  const url = isEdit
      ? `http://localhost:5181/api/Checkpoint`
      : "http://localhost:5181/api/Checkpoint";
  const method = isEdit ? "PUT" : "POST";

  try {
    const response = await fetch(url, {
      method,
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(payload)
    });

    if (!response.ok) throw new Error("Falha ao salvar");

    await fetchAllCheckpoints();
    closeModal();
    console.log("Checkpoint salvo:", await response.json());
  } catch (error) {
    console.error("Erro ao salvar checkpoint:", error);
  }
}

async function deleteCheckpoint() {
  if (!currentCheckpointId.value) return;

  try {
    const response = await fetch(`http://localhost:5181/api/Checkpoint/${currentCheckpointId.value}`, { method: "DELETE" });

    if (!response.ok) throw new Error("Falha ao excluir");

    await fetchAllCheckpoints();
    currentCheckpointId.value = null;
    showDeleteModal.value = false;
  } catch (error) {
    console.error("Erro ao excluir checkpoint:", error);
  }
}

function openEdit(checkpoint: Checkpoint) {
  selectedCheckpoint.value = checkpoint;
  showModal.value = true;
  modalKey.value++;
}

function openNewCheckpoint() {
  selectedCheckpoint.value = null;
  showModal.value = true;
  modalKey.value++;
}

function openDelete(id: string) {
  currentCheckpointId.value = id;
  showDeleteModal.value = true;
}

function closeModal() {
  showModal.value = false;
  selectedCheckpoint.value = null;
  modalKey.value++;
}

onMounted(async () => {
  await fetchAllPlantFloorSections();
  await fetchAllCheckpoints();
});


</script>
