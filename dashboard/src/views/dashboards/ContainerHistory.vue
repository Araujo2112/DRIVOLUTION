<script lang="ts" setup>
import { ref, computed, onMounted } from 'vue'
import ContainerHistoryChart from '../../components/FactoryFloorItems/ContainerHistory/ContainerHistoryChart.vue'

interface LocalizationHistory {
  id: number;
  containerId: number;
  sectionId: number;
  datetime: string;
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

interface ItemLocalization {
  itemLocalizationId: number;
  itemRawId: number;
  containerLocalizationId: number;
  dateTime: string;
}

interface LotOfRawMaterial {
  lotOfRawMaterialId: number;
  rawMaterialId: number;
}

interface RawMaterial {
  rawMaterialId: number;
  name: string;
}

const allHistories = ref<LocalizationHistory[]>([]);
const itemLocalizations = ref<ItemLocalization[]>([]);
const itemOfRawMaterials = ref<ItemOfRawMaterial[]>([]);
const lots = ref<LotOfRawMaterial[]>([]);
const rawMaterials = ref<RawMaterial[]>([]);
const selectedContainerId = ref<number | null>(null);
const openModalSectionId = ref<number | null>(null);
const sectionItemMap = ref<Record<number, {
  itemOfRawMaterial: ItemOfRawMaterial;
  rawMaterialName: string;
  lotInfo: string;
  lotId: number;
}[]>>({});
const currentPage = ref(1);
const rowsPerPage = 5;

const fetchData = async () => {
  const [historiesRes, localizationsRes, materialsRes, lotsRes, rawsRes] = await Promise.all([
    fetch('http://localhost:5181/api/ContainerLocalizationHistory'),
    fetch('http://localhost:5181/api/ItemLocalization'),
    fetch('http://localhost:5181/api/ItemOfRawMaterial'),
    fetch('http://localhost:5181/api/LotOfRawMaterial'),
    fetch('http://localhost:5181/api/RawMaterial')
  ])

  allHistories.value = await historiesRes.json();
  itemLocalizations.value = await localizationsRes.json();
  itemOfRawMaterials.value = await materialsRes.json();
  lots.value = await lotsRes.json();
  rawMaterials.value = await rawsRes.json();

  console.log("Fetched ContainerLocalizationHistory:", JSON.parse(JSON.stringify(allHistories.value)));
  console.log("Fetched ItemLocalization:", JSON.parse(JSON.stringify(itemLocalizations.value)));
  console.log("Fetched ItemOfRawMaterial:", JSON.parse(JSON.stringify(itemOfRawMaterials.value)));
  console.log("Fetched LotOfRawMaterial:", JSON.parse(JSON.stringify(lots.value)));
  console.log("Fetched RawMaterial:", JSON.parse(JSON.stringify(rawMaterials.value)));

  const map: Record<number, {
    itemOfRawMaterial: ItemOfRawMaterial;
    rawMaterialName: string;
    lotInfo: string;
    lotId: number;
  }[]> = {};

  itemLocalizations.value.forEach(loc => {
    const matchingMaterial = itemOfRawMaterials.value.find(mat => mat.itemRawId === loc.itemRawId);
    const container = allHistories.value.find(hist => hist.id === loc.containerLocalizationId);
    if (!matchingMaterial || !container) return;

    const sectionId = container.sectionId;
    if (!map[sectionId]) map[sectionId] = [];

    const lot = lots.value.find(l => l.lotOfRawMaterialId === matchingMaterial.lotOfRawMaterialId);
    const rawMaterialName = rawMaterials.value.find(r => r.rawMaterialId === lot?.rawMaterialId)?.name || matchingMaterial.code.name;

    const lotInfo = `${matchingMaterial.quantity} ${matchingMaterial.unit}`;
    const lotId = matchingMaterial.lotOfRawMaterialId;

    if (!map[sectionId].some(i => i.itemOfRawMaterial.itemRawId === matchingMaterial.itemRawId)) {
      map[sectionId].push({
        itemOfRawMaterial: matchingMaterial,
        rawMaterialName,
        lotInfo,
        lotId
      });
    }
  });

  sectionItemMap.value = map;
  console.log("Processed section-to-items map:", JSON.parse(JSON.stringify(sectionItemMap.value)));
};

const paginatedHistories = computed(() => {
  const filtered = allHistories.value.filter(h => h.containerId === selectedContainerId.value);
  const start = (currentPage.value - 1) * rowsPerPage;
  return filtered.slice(start, start + rowsPerPage);
});

const totalPages = computed(() => {
  const total = allHistories.value.filter(h => h.containerId === selectedContainerId.value).length;
  return Math.ceil(total / rowsPerPage);
});

const containerStats = computed(() => {
  if (!selectedContainerId.value) return null;

  const containerHistories = allHistories.value.filter(h => h.containerId === selectedContainerId.value);
  const uniqueSections = new Set(containerHistories.map(h => h.sectionId));

  // Get first and last movement dates
  let firstMovement = 'N/A';
  let lastMovement = 'N/A';

  if (containerHistories.length > 0) {
    const sortedHistories = [...containerHistories].sort((a, b) =>
        new Date(a.datetime).getTime() - new Date(b.datetime).getTime()
    );

    firstMovement = new Date(sortedHistories[0].datetime).toLocaleString();
    lastMovement = new Date(sortedHistories[sortedHistories.length - 1].datetime).toLocaleString();
  }

  return {
    totalMovements: containerHistories.length,
    uniqueSections: uniqueSections.size,
    firstMovement,
    lastMovement
  };
});

const changePage = (direction: 'prev' | 'next') => {
  if (direction === 'prev' && currentPage.value > 1) currentPage.value--;
  if (direction === 'next' && currentPage.value < totalPages.value) currentPage.value++;
};

function openModal(sectionId: number) {
  openModalSectionId.value = sectionId;
}

function closeModal() {
  openModalSectionId.value = null;
}

onMounted(fetchData);
</script>


<template>
  <div class="p-6">
    <h1 class="text-2xl font-semibold mb-4">Container History</h1>
    <div class="mb-4">
      <select v-model="selectedContainerId" class="p-2 border rounded w-full md:w-1/2">
        <option v-for="id in Array.from(new Set(allHistories.map(h => h.containerId)))" :key="id" :value="id">
          Container ID: {{ id }}
        </option>
      </select>
    </div>

    <div v-if="selectedContainerId">

      <div class="mb-6">
        <div class="container-details-header">
          <h2 class="text-xl font-semibold">
            <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="icon"><path d="M22 12H2"/><path d="M5.45 5.11 2 12v6a2 2 0 0 0 2 2h16a2 2 0 0 0 2-2v-6l-3.45-6.89A2 2 0 0 0 16.76 4H7.24a2 2 0 0 0-1.79 1.11z"/><line x1="6" x2="6" y1="16" y2="16"/><line x1="10" x2="10" y1="16" y2="16"/></svg>
            Container Details
          </h2>
        </div>

        <div class="stats-grid">
          <div class="stat-card">
            <div class="stat-icon">
              <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect width="18" height="18" x="3" y="3" rx="2"/><path d="M3 9h18"/><path d="M9 21V9"/></svg>
            </div>
            <div class="stat-content">
              <div class="stat-value">{{ selectedContainerId }}</div>
              <div class="stat-label">Container ID</div>
            </div>
          </div>

          <div class="stat-card">
            <div class="stat-icon">
              <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M12 2v20"/><path d="m17 5-5-3-5 3"/><path d="m17 19-5 3-5-3"/><path d="M2 12h20"/><path d="m5 7-3 5 3 5"/><path d="m19 7 3 5-3 5"/></svg>
            </div>
            <div class="stat-content">
              <div class="stat-value">{{ containerStats?.totalMovements }}</div>
              <div class="stat-label">Total Movements</div>
            </div>
          </div>

          <div class="stat-card">
            <div class="stat-icon">
              <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M20 10c0 6-8 12-8 12s-8-6-8-12a8 8 0 0 1 16 0Z"/><circle cx="12" cy="10" r="3"/></svg>
            </div>
            <div class="stat-content">
              <div class="stat-value">{{ containerStats?.uniqueSections }}</div>
              <div class="stat-label">Unique Sections Visited</div>
            </div>
          </div>

          <div class="stat-card">
            <div class="stat-icon">
              <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"/><polyline points="12 6 12 12 16 14"/></svg>
            </div>
            <div class="stat-content">
              <div class="stat-value time-value">{{ containerStats?.lastMovement }}</div>
              <div class="stat-label">Last Movement</div>
            </div>
          </div>
        </div>
      </div>

      <div class="mt-6">
        <h3 class="text-lg font-semibold mb-2">Location History</h3>
        <div class="flex flex-col md:flex-row gap-4">
          <div class="flex-1">
            <div class="mb-4">
              <table class="w-full border-collapse border border-gray-300 bg-white shadow-sm rounded-lg">
                <thead class="bg-gray-500 text-black">
                <tr>
                  <th class="p-2 text-left border">History ID</th>
                  <th class="p-2 text-left border">Section ID</th>
                  <th class="p-2 text-left border">Date/Time</th>
                  <th class="p-2 text-left border">Items</th>
                </tr>
                </thead>
                <tbody>
                <tr v-for="history in paginatedHistories" :key="history.id" class="border-b">
                  <td class="p-2 border">{{ history.id }}</td>
                  <td class="p-2 border">{{ history.sectionId }}</td>
                  <td class="p-2 border">{{ new Date(history.datetime).toLocaleString() }}</td>
                  <td class="p-2 border">
                    <button class="p-1 bg-blue-500 text-white rounded" @click="openModal(history.sectionId)">
                      View Items
                    </button>
                  </td>
                </tr>
                </tbody>
              </table>

              <div class="flex justify-between mt-2">
                <button :disabled="currentPage <= 1" class="p-2 bg-blue-500 text-white rounded-md" @click="changePage('prev')">
                  Previous
                </button>
                <span class="p-2">{{ currentPage }} of {{ totalPages }}</span>
                <button :disabled="currentPage >= totalPages" class="p-2 bg-blue-500 text-white rounded-md" @click="changePage('next')">
                  Next
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>

      <div class="mt-8">
        <ContainerHistoryChart :containerDetails="{ container: { localizationHistories: allHistories.filter(h => h.containerId === selectedContainerId) } }" />
      </div>
    </div>

    <div v-if="openModalSectionId !== null" class="modal-overlay" @click.self="closeModal">
      <div class="modal-content">
        <button class="close-button" @click="closeModal">X</button>
        <div class="modal-body">
          <h3 class="text-lg font-semibold mb-4">Item Details (Section {{ openModalSectionId }})</h3>
          <div class="overflow-x-auto">
            <table class="w-full">
              <thead class="bg-gray-500">
              <tr>
                <th class="p-2 text-left text-black">ID Fiware</th>
                <th class="p-2 text-left text-black">Raw Material</th>
                <th class="p-2 text-left text-black">Quantity</th>
              </tr>
              </thead>
              <tbody>
              <tr v-for="(item, index) in sectionItemMap[openModalSectionId] || []" :key="index" class="border-b">
                <td class="p-2">{{ item.itemOfRawMaterial.code.name }}</td>
                <td class="p-2">{{ item.rawMaterialName }}</td>
                <td class="p-2">{{ item.lotInfo }}</td>
              </tr>
              <tr v-if="(sectionItemMap[openModalSectionId] || []).length === 0">
                <td class="p-4 text-center text-gray-500" colspan="2">
                  No items found for this section.
                </td>
              </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>


<style scoped>
:root {
  --primary-color: hsl(200, 22%, 40%);
  --primary-light: hsl(200, 22%, 50%);
  --primary-dark: hsl(200, 22%, 30%);
  --text-color: hsl(200, 22%, 20%);
  --text-light: hsl(200, 15%, 35%);
  --text-lighter: hsl(200, 10%, 50%);
  --bg-color: hsl(200, 20%, 97%);
  --bg-light: hsl(200, 15%, 95%);
  --bg-dark: hsl(200, 15%, 93%);
  --border-color: hsl(200, 15%, 85%);
}

/* Original styles */
table {
  width: 100%;
  border-collapse: collapse;
}

th,
td {
  border: 1px solid #e2e8f0;
}

th {
  background-color: #f7fafc;
  text-align: left;
  padding: 0.5rem;
}

td {
  padding: 0.5rem;
}

.mt-8 {
  margin-top: 2rem;
}

button {
  background-color: #63899C;
  color: white;
  padding: 0.5rem 1rem;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  transition: background-color 0.3s ease;
}

button:hover {
  background-color: #4f7282;
}

button:disabled {
  background-color: #ccc;
  cursor: not-allowed;
}

.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 100;
  overflow-y: auto;
  padding: 1rem;
}

.modal-content {
  background: #fff;
  border-radius: 8px;
  max-width: 600px;
  width: 100%;
  max-height: 80vh;
  overflow-y: auto;
  position: relative;
  padding: 1rem;
}

.close-button {
  position: absolute;
  top: 0.5rem;
  right: 0.5rem;
  background: transparent;
  border: none;
  font-size: 1.2rem;
  cursor: pointer;
}

/* New styles for Container Details */
.container-details-header {
  display: flex;
  align-items: center;
  margin-bottom: 16px;
}

.container-details-header h2 {
  display: flex;
  align-items: center;
  gap: 8px;
  color: var(--primary-color);
}

.icon {
  color: var(--primary-color);
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
  gap: 16px;
  margin-bottom: 24px;
}



.stat-card {
  background-color: white;
  border-radius: 8px;
  padding: 20px;
  display: flex;
  align-items: center;
  gap: 16px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
  border-top: 3px solid var(--primary-color);
  transition: transform 0.2s ease, box-shadow 0.2s ease;
}

.stat-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.08);
}

.stat-icon {
  background-color: var(--bg-light);
  width: 48px;
  height: 48px;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--primary-color);
}

.stat-content {
  flex: 1;
}

.stat-value {
  font-size: 24px;
  font-weight: 600;
  color: var(--text-color);
  line-height: 1.2;
}

.time-value {
  font-size: 16px;
}

.stat-label {
  font-size: 14px;
  color: var(--text-lighter);
  margin-top: 4px;
}

/* Responsive adjustments */
@media (max-width: 768px) {
  .stats-grid {
    grid-template-columns: 1fr;
  }
}
</style>