<script lang="ts" setup>
import {onMounted, ref} from "vue";
import Title from "@/components/Title.vue";
import Button from "@/components/Button.vue";
import LotRawMaterialTable from "@/components/FactoryFloorItems/LotOfRawMaterial/LotOfRawMaterialTable.vue";
import LotRawMaterialModal from "@/components/FactoryFloorItems/LotOfRawMaterial/LotOfRawMaterialModal.vue";
import type {LotRawMaterial, RawMaterial} from "@/types/Interfaces";
import {fetchAllRawMaterials} from "@/components/FactoryFloorItems/RawMaterial/Functions";

const rawMaterials = ref<RawMaterial[]>([]);
const lotsRawMaterial = ref<LotRawMaterial[]>([]);
const error = ref<string | null>(null);
const showModal = ref(false);
const showDeleteModal = ref(false);
const selectedLot = ref<LotRawMaterial | null>(null);
const currentLotCode = ref<string | null>(null);

async function fetchLots(): Promise<void> {
  try {
    const response = await fetch("http://localhost:5181/api/LotOfRawMaterial");
    if (!response.ok) throw new Error(`Erro HTTP: ${response.status}`);

    const data = await response.json();
    const lotData: LotRawMaterial[] = data.map((item: any) => ({
      lotId: item.lotId,
      lotCode: item.id?.name || "",
      lotNumber: item.lotNumber?.name || "",
      lotQuantity: item.lotQuantity,
      lotUnit: item.lotUnit,
      rawMaterialId: item.rawMaterialId
    }));

    lotData.sort((a, b) => a.lotId - b.lotId);
    lotsRawMaterial.value = lotData;
    console.log(lotData);

  } catch (err) {
    error.value = err instanceof Error ? err.message : "Erro desconhecido";
  }
}

async function handleSubmit(lotData: LotRawMaterial): Promise<void> {
  try {
    const method = selectedLot.value ? "PUT" : "POST";
    const response = await fetch("http://localhost:5181/api/LotOfRawMaterial", {
      method,
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(lotData)
    });

    if (!response.ok) throw new Error("Falha ao salvar lote");

    await fetchLots();  
    showModal.value = false;  
    selectedLot.value = null; 

  } catch (err) {
    error.value = err instanceof Error ? err.message : "Erro ao salvar";
  }
}

async function deleteLot(): Promise<void> {
  if (!currentLotCode.value) return;

  try {
    const response = await fetch(
        `http://localhost:5181/api/LotOfRawMaterial/${currentLotCode.value}`,
        { method: "DELETE" }
    );

    if (!response.ok) throw new Error("Falha ao excluir lote");

    await fetchLots();  
    currentLotCode.value = null;
    showDeleteModal.value = false;  

  } catch (err) {
    error.value = err instanceof Error ? err.message : "Erro ao excluir";
  }
}

function openEdit(lot: LotRawMaterial): void {
  selectedLot.value = lot;
  showModal.value = true;
}

function openDelete(lotNumber: string): void {
  currentLotCode.value = lotNumber;
  showDeleteModal.value = true;
}

function closeModal() {
  showModal.value = false;
  selectedLot.value = null;
}

function closeDeleteModal() {
  showDeleteModal.value = false;
  currentLotCode.value = null;
}

onMounted(async () => {
  try {
    await fetchLots();
    const materials = await fetchAllRawMaterials();
    rawMaterials.value = materials; 
    console.log("Estado após carga:", rawMaterials.value);
  } catch (err) {
    error.value = "Erro ao carregar dados";
    rawMaterials.value = [];
  }
});
</script>

<template>
  <div>
    <Title>Raw Material Lots</Title>

    <div class="w-full flex flex-col gap-2 items-end mb-2">
      <Button icon="add" @click="showModal = true">New Lot</Button>
    </div>

    <LotRawMaterialTable
        :lotsRawMaterial="lotsRawMaterial"
        @delete="openDelete"
        @edit="openEdit"
    />

    <LotRawMaterialModal
        :isOpen="showModal"
        :lotRawMaterial="selectedLot"
        :rawMaterials="rawMaterials"
        @close="closeModal"
        @submit="handleSubmit"
    />

    <div v-if="error" class="text-red-500 mt-4">{{ error }}</div>
  </div>
</template>
