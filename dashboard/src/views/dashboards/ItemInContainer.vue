<script lang="ts" setup>
import {onMounted, ref} from "vue";
import Title from "@/components/Title.vue";
import ItemsInContainerTable from "@/components/FactoryFloorItems/ItemInContainer/ItemInContainerTable.vue";
import type {Containers, ItemInContainer} from "@/types/Interfaces";

const containers = ref<Containers[]>([]);
const itemsInContainer = ref<ItemInContainer[]>([]);
const error = ref<string | null>(null);
const showModal = ref(false);
const showDeleteModal = ref(false);
const selectedItem = ref<ItemInContainer | null>(null);
const currentItemCode = ref<string | null>(null);

async function fetchAllContainers(): Promise<Containers[]> {
  try {
    const response = await fetch("http://localhost:5181/api/Container");
    if (!response.ok) throw new Error(`HTTP Error: ${response.status}`);

    const data = await response.json();
    return data.map((item: any) => ({
      containerId: item.containerId,
      containerCode: item.containerCode,
      containerName: item.containerName.name,
      containerVolume: item.containerVolume,
      activate: item.activate
    }));

  } catch (err) {
    console.error("Erro ao buscar containers:", err);
    return [];
  }

}

async function fetchItems(): Promise<void> {
  try {
    const response = await fetch("http://localhost:5181/api/ItemInContainer");
    if (!response.ok) throw new Error(`Erro HTTP: ${response.status}`);

    const data = await response.json();
    itemsInContainer.value = data.map((item: any) => ({
      itemCode: item.id.name,
      containerId: item.containerId,
      dateTimeIn: item.dateTimeIn,
      dateTimeOut: item.dateTimeOut
    }));

    console.log(data);
  } catch (err) {
    error.value = err instanceof Error ? err.message : "Erro desconhecido";
  }
}

/*
async function handleSubmit(itemData: ItemInContainer): Promise<void> {
  try {
    const DateTimeIn = new Date().getTime();
    const method = selectedItem.value ? "PUT" : "POST";
    console.log("Estou aqui");
    const response = await fetch("http://localhost:5181/api/ItemInContainer", {
      method,
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({
        ...itemData,
        itemCode: String(itemData.itemCode),
        dateTimeIn: new Date(DateTimeIn).toISOString(),
        dateTimeOut: new Date(DateTimeIn + 10000).toISOString()
      })
    });

    if (!response.ok) throw new Error("Falha ao salvar associação");

    await fetchItems();
    showModal.value = false;
  } catch (err) {
    error.value = err instanceof Error ? err.message : "Erro ao salvar";
  }
}
 */
async function deleteItem(): Promise<void> {
  if (!currentItemCode.value) return;

  try {
    const response = await fetch(
        `http://localhost:5181/api/ItemInContainer/${currentItemCode.value}`,
        {method: "DELETE"}
    );

    if (!response.ok) throw new Error("Falha ao excluir associação");

    await fetchItems();
    showDeleteModal.value = false;
  } catch (err) {
    error.value = err instanceof Error ? err.message : "Erro ao excluir";
  }
}

onMounted(async () => {
  await fetchItems();
  containers.value = await fetchAllContainers();
  console.log(containers.value);
});
</script>

<template>
  <div>
    <Title>Itens in Containers</Title>
    

    <ItemsInContainerTable
        :containers="containers"
        :items="itemsInContainer"
        
    />
    
    
  </div>
</template>