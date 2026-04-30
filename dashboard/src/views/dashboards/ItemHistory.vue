<script lang="ts" setup>
import { computed, onMounted, ref, watch, watchEffect } from "vue";
import SectionVariationChart from '../../components/FactoryFloorItems/ItemHistory/SectionVariationChart.vue';
import Title from "@/components/Title.vue";

interface LocalizationHistory {
  datetime: string;
  sectionId: number;
}

interface ItemLocalization {
  itemLocalizationId: number;
  itemRawId: number;
  containerLocalizationId: number;
  dateTime: string;
}

interface ItemOfRawMaterial {
  itemRawId: number;
  code: { name: string };
  quantity: number;
  unit: string;
  lotOfRawMaterialId: number;
  itemInContainerId: number;
  manufacturingOrderPhaseId: number;
  manufacturingOrderId: number;
}

interface LotOfRawMaterial {
  lotOfRawMaterialId: number;
  lotCode: string;
  lotNumber: string;
  lotQuantity: number;
  lotUnit: string;
  rawMaterialId: number;
}

interface RawMaterial {
  rawMaterialId: number;
  rawMaterialName: string;
  rawMaterialInfo: string;
}

interface ContainerLocalization {
  id: number;
  containerId: number;
  sectionId: number;
  datetime: string;
  localizationHistories?: LocalizationHistory[];
}

interface ItemLocalizationDetails {
  id: number;
  itemRawId: number;
  containerLocalizationId: number;
  datetime: string;
  containerLocalization: ContainerLocalization;
  rawMaterialCode: string;
}

interface FullRawMaterial {
  itemCode: string;
  quantity: number;
  unit: string;
  lotOfRawMaterial: LotOfRawMaterial & { rawMaterial: RawMaterial };
}

interface Container {
  containerId: number;
  containerName: { name: string };
  containerVolume: number;
}

const itemLocalizations = ref<Map<number, ItemLocalizationDetails>>(new Map());
const rawIdGroups = ref<Map<number, number[]>>(new Map());
const selectedRawId = ref<number | null>(null);
const rawMaterial = ref<FullRawMaterial | null>(null);
const container = ref<Container | null>(null);

const rawIdGroupKeys = computed(() => Array.from(rawIdGroups.value.keys()));

async function fetchAllItemLocalizations(): Promise<void> {
  try {
    const response = await fetch("http://localhost:5181/api/ItemLocalization");
    if (!response.ok) throw new Error(`Erro ao buscar IDs: ${response.statusText}`);
    const data: ItemLocalization[] = await response.json();
    data.forEach((entry) => {
      const id = entry.itemLocalizationId;
      const rawId = entry.itemRawId;
      if (!rawIdGroups.value.has(rawId)) rawIdGroups.value.set(rawId, []);
      rawIdGroups.value.get(rawId)!.push(id);
    });
  } catch (error) {
    console.error("Erro ao buscar itemLocalization IDs:", error);
  }
}

async function fetchItemLocalizationDetails(id: number): Promise<void> {
  if (itemLocalizations.value.has(id)) return;
  try {
    const response = await fetch(`http://localhost:5181/api/ItemLocalization/${id}`);
    if (!response.ok) {
      console.warn(`ItemLocalization com ID ${id} não encontrado.`);
      return;
    }
    const data: ItemLocalization = await response.json();

    let containerData: ContainerLocalization = {
      id: data.containerLocalizationId,
      containerId: 0,
      sectionId: 0,
      datetime: data.dateTime,
      localizationHistories: []
    };

    try {
      const containerRes = await fetch(`http://localhost:5181/api/ContainerLocalizationHistory/${data.containerLocalizationId}`);
      if (containerRes.ok) {
        containerData = await containerRes.json();
      } else {
        console.warn(`ContainerLocalization com ID ${data.containerLocalizationId} não encontrado.`);
      }
    } catch (err) {
      console.warn(`Erro ao buscar ContainerLocalization ID ${data.containerLocalizationId}:`, err);
    }

    const detail: ItemLocalizationDetails = {
      id: data.itemLocalizationId,
      itemRawId: data.itemRawId,
      containerLocalizationId: data.containerLocalizationId,
      datetime: data.dateTime,
      containerLocalization: containerData,
      rawMaterialCode: String(data.itemRawId)
    };

    itemLocalizations.value.set(id, detail);
  } catch (error) {
    console.error("Erro ao buscar detalhes do itemLocalization:", error);
  }
}

const aggregatedData = computed(() => {
  if (selectedRawId.value === null) return null;
  const group = rawIdGroups.value.get(selectedRawId.value);
  if (!group || group.length === 0) return null;

  group.forEach(id => {
    if (!itemLocalizations.value.has(id)) {
      fetchItemLocalizationDetails(id);
    }
  });

  const firstDetail = itemLocalizations.value.get(group[0]);
  return firstDetail || null;
});

async function fetchRawMaterial(itemRawId: number): Promise<void> {
  try {
    const response = await fetch(`http://localhost:5181/api/ItemOfRawMaterial`);
    const materials: ItemOfRawMaterial[] = await response.json();
    const material = materials.find(m => m.itemRawId === itemRawId);
    if (!material) throw new Error("ItemOfRawMaterial não encontrado");

    const lotResponse = await fetch(`http://localhost:5181/api/LotOfRawMaterial`);
    const lots: LotOfRawMaterial[] = await lotResponse.json();
    const lot = lots.find(l => l.lotOfRawMaterialId === material.lotOfRawMaterialId);
    if (!lot) throw new Error("Lote não encontrado");

    const rawRes = await fetch(`http://localhost:5181/api/RawMaterial`);
    const rawList: RawMaterial[] = await rawRes.json();
    const rawMaterialInfo = rawList.find(r => r.rawMaterialId === lot.rawMaterialId);
    if (!rawMaterialInfo) throw new Error("RawMaterial não encontrado");

    rawMaterial.value = {
      itemCode: material.code.name,
      quantity: material.quantity,
      unit: material.unit,
      lotOfRawMaterial: {
        ...lot,
        rawMaterial: rawMaterialInfo
      }
    };
  } catch (error) {
    console.error("Erro ao buscar Raw Material:", error);
  }
}

async function fetchContainerDetails(containerId: number): Promise<void> {
  try {
    const response = await fetch("http://localhost:5181/api/Container");
    const data = await response.json();
    const containerDetails = data.find((c: any) => c.containerId === containerId);
    if (!containerDetails) throw new Error("Contentor não encontrado");
    container.value = containerDetails;
  } catch (error) {
    console.error("Erro ao buscar detalhes do contentor:", error);
  }
}

watchEffect(() => {
  const data = aggregatedData.value;
  if (data) {
    fetchRawMaterial(data.itemRawId);
    fetchContainerDetails(data.containerLocalization.containerId);
  }
});

onMounted(async () => {
  await fetchAllItemLocalizations();
  const groups = Array.from(rawIdGroups.value.keys());
  if (groups.length > 0) selectedRawId.value = groups[0];
});

watch(selectedRawId, (newVal) => {
  if (newVal !== null) {
    const group = rawIdGroups.value.get(newVal);
    if (group) {
      group.forEach(id => {
        fetchItemLocalizationDetails(id);
      });
    }
  }
});
</script>



<template>
  <Title>History</Title>

  <div class="mb-4">
    <select v-model="selectedRawId" class="p-2 border rounded w-full">
      <option disabled value="">Select a Raw ID</option>
      <option v-for="rawId in rawIdGroupKeys" :key="rawId" :value="rawId">
        ItemRawId: {{ rawId }}
      </option>
    </select>
  </div>

  <div class="flex flex-wrap gap-4">
    <div v-if="rawMaterial" class="w-full md:w-1/2 lg:w-1/3">
      <h2 class="text-lg font-semibold mb-2">Raw Material Data</h2>
      <table class="w-full border-collapse border border-gray-300">
        <thead class="bg-gray-500 text-white">
        <tr>
          <th class="p-2 text-left">ID</th>
          <th class="p-2 text-left">Name</th>
          <th class="p-2 text-left">Information</th>
        </tr>
        </thead>
        <tbody>
        <tr class="border-b">
          <td class="p-2 border border-gray-300">{{ rawMaterial.itemCode }}</td>
          <td class="p-2 border border-gray-300">{{ rawMaterial.lotOfRawMaterial.rawMaterial.rawMaterialName }}</td>
          <td class="p-2 border border-gray-300">{{ rawMaterial.lotOfRawMaterial.rawMaterial.rawMaterialInfo }}</td>
        </tr>
        </tbody>
      </table>
    </div>

    <div v-if="container" class="w-full md:w-1/2 lg:w-1/3">
      <h2 class="text-lg font-semibold mb-2">Container Data</h2>
      <table class="w-full border-collapse border border-gray-300">
        <thead class="bg-gray-500 text-white">
        <tr>
          <th class="p-2 text-left">ID</th>
          <th class="p-2 text-left">Name</th>
          <th class="p-2 text-left">Volume</th>
        </tr>
        </thead>
        <tbody>
        <tr class="border-b">
          <td class="p-2 border border-gray-300">{{ container.containerId }}</td>
          <td class="p-2 border border-gray-300">{{ container.containerName.name }}</td>
          <td class="p-2 border border-gray-300">{{ container.containerVolume }}</td>
        </tr>
        </tbody>
      </table>
    </div>
  </div>

  <div v-if="aggregatedData" class="mt-6">
    <h2 class="text-lg font-semibold mb-2">Item Details (Raw ID: {{ aggregatedData.itemRawId }})</h2>

    <div class="flex flex-wrap gap-4">
      <div class="w-full md:w-1/2">
        <table class="w-full border-collapse border border-gray-300">
          <thead class="bg-gray-500 text-white">
          <tr>
            <th class="p-2 text-left">Location ID</th>
            <th class="p-2 text-left">Date and Time</th>
            <th class="p-2 text-left">Section</th>
          </tr>
          </thead>
          <tbody>
          <tr class="border-b">
            <td class="p-2 border border-gray-300">{{ aggregatedData.containerLocalizationId }}</td>
            <td class="p-2 border border-gray-300">{{ aggregatedData.datetime }}</td>
            <td class="p-2 border border-gray-300">{{ aggregatedData.containerLocalization.sectionId }}</td>
          </tr>
          </tbody>
        </table>
      </div>

      <div class="w-full md:w-1/2">
        <h3 class="text-lg font-semibold mb-2">Location History</h3>
        <table class="w-full border-collapse border border-gray-300">
          <thead class="bg-gray-500 text-white">
          <tr>
            <th class="p-2 text-left">Date</th>
            <th class="p-2 text-left">Section</th>
          </tr>
          </thead>
          <tbody>
          <tr
              v-for="(history, index) in aggregatedData.containerLocalization.localizationHistories || []"
              :key="index"
          >

          <td class="p-2 border border-gray-300">{{ new Date(history.datetime).toLocaleString() }}</td>
            <td class="p-2 border border-gray-300">{{ history.sectionId }}</td>
          </tr>
          </tbody>
        </table>
      </div>
    </div>

    <div class="mt-6 flex justify-center">
      <div class="w-full md:w-2/3 lg:w-1/2 p-4 border border-gray-300 rounded-lg shadow-md">
        <SectionVariationChart :itemLocalizationDetails="aggregatedData" />
      </div>
    </div>
  </div>
</template>



<style scoped>
select {
  padding: 8px;
  min-width: 200px;
  margin-bottom: 20px;
}

table {
  border-collapse: collapse;
  width: 100%;
}

th, td {
  border: 1px solid #ddd;
}
</style>