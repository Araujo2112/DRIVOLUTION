<template>
  <div class="col-span-12 md:col-span-6 bg-white rounded-xl shadow-xl p-6 hover:shadow-2xl transition-shadow duration-300">

    <div class="mb-4 p-3 bg-gray-50 rounded-lg border border-gray-200">
      <div class="flex flex-wrap items-center gap-4">
        <div class="flex items-center space-x-2">
          <label class="text-sm font-medium text-gray-700">Cliente:</label>
          <select v-model="selectedClient" @change="onClientChange" class="px-3 py-2 bg-white border border-gray-300 rounded-lg text-sm">
            <option value="">Todos os clientes</option>
            <option v-for="c in availableClients" :key="c.id" :value="c.id">{{ c.name }}</option>
          </select>
        </div>
        <div class="flex items-center space-x-2">
          <label class="text-sm font-medium text-gray-700">Período:</label>
          <select v-model="selectedPeriod" @change="onPeriodChange" class="px-3 py-2 bg-white border border-gray-300 rounded-lg text-sm">
            <option value="7">Últimos 7 dias</option>
            <option value="30">Últimos 30 dias</option>
            <option value="90">Últimos 90 dias</option>
          </select>
        </div>
        <button @click="resetFilters" class="px-3 py-1 bg-gray-500 text-white rounded text-sm">Reset</button>
      </div>
      <div v-if="hasActiveFilters" class="flex flex-wrap gap-2 mt-2 text-xs">
        <span class="text-gray-600">Filtros ativos:</span>
        <span v-if="selectedClient" class="px-2 py-1 bg-orange-100 text-orange-800 rounded-full">
          Cliente: {{ getClientName(selectedClient) }}
        </span>
        <span class="px-2 py-1 bg-blue-100 text-blue-800 rounded-full">
          Período: {{ getPeriodLabel(selectedPeriod) }}
        </span>
      </div>
    </div>

    <div class="space-y-2 max-h-64 overflow-y-auto">
      <div v-for="order in filteredOrders" :key="order.id" class="border rounded-lg">

        <button class="w-full px-4 py-3 bg-gray-100 flex justify-between items-center"
                @click="toggle(order.id)">
          <div>
            <span class="font-medium">Ordem #{{ order.orderNumber }}</span>
            <span class="text-gray-600 text-sm ml-2">Cliente: {{ getClientName(order.clientId) }}</span>
          </div>
          <svg :class="{ 'rotate-180': expanded === order.id }"
               class="w-5 h-5 transition-transform"
               fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                  d="M19 9l-7 7-7-7"/>
          </svg>
        </button>

        <div v-if="expanded === order.id" class="p-4 bg-white space-y-4">

          <div>
            <h4 class="font-semibold text-gray-800">
              Processo: {{ order.process?.processName || '–sem processo–' }}
            </h4>
            <p class="text-sm text-gray-500">
              {{ order.process?.info || '' }}
            </p>
          </div>

          <div v-for="step in order.phases" :key="step.manufacturingPhaseId" class="space-y-2">
            <h5 class="font-medium text-gray-700">
              Passo {{ step.numberStepOrder }}: {{ step.phaseInfo }}
            </h5>
            <p class="text-sm text-gray-500 italic mb-2">
              {{ step.description }}
            </p>
            <table class="w-full">
              <thead>
              <tr class="bg-gray-500">
                <th class="p-2 text-left">Matéria-Prima</th>
                <th class="p-2 text-right">Quant.</th>
                <th class="p-2 text-left">Unid.</th>
              </tr>
              </thead>
              <tbody>
              <tr
                  v-for="item in step.orderPhaseId ? (order.rawByPhase[step.orderPhaseId] || []) : []"
                  :key="item.itemRawId"
              >
                <td class="p-2">{{ item.rawMaterialName }}</td>
                <td class="p-2 text-right">{{ item.quantity }}</td>
                <td class="p-2">{{ item.unit }}</td>
              </tr>
              <tr v-if="!step.orderPhaseId || !(order.rawByPhase[step.orderPhaseId]?.length)">
                <td class="p-2 italic text-gray-500" colspan="3">Sem consumo registado</td>
              </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </div>

    <div class="mt-4 grid grid-cols-3 gap-4 text-center text-xs">
      <div>
        <div class="font-bold text-blue-600">{{ filteredOrders.length }}</div>
        <div>Total Ordens</div>
      </div>
      <div>
        <div class="font-bold text-orange-600">{{ uniqueClientsCount }}</div>
        <div>Clientes</div>
      </div>
      <div>
        <div class="font-bold text-green-600">{{ ordersThisWeek }}</div>
        <div>Esta Semana</div>
      </div>
    </div>

  </div>
</template>


<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'

const selectedClient = ref('')
const selectedPeriod = ref('30')
const manufacturingOrders = ref<any[]>([])
const availableClients = ref<{id:number,name:string}[]>([])
const expanded = ref<number|null>(null)

function toggle(id: number) {
  expanded.value = (expanded.value === id) ? null : id
}

const hasActiveFilters = computed(() =>
    selectedClient.value !== '' || selectedPeriod.value !== '30'
)

const filteredOrders = computed(() => {
  let arr = manufacturingOrders.value.slice()
  if (selectedClient.value) {
    arr = arr.filter(o => o.clientId === +selectedClient.value)
  }
  const cutoff = Date.now() - (+selectedPeriod.value)*86400000
  arr = arr.filter(o => new Date(o.sheduleInit).getTime() >= cutoff)
  return arr.sort((a,b)=> new Date(b.sheduleInit).getTime() - new Date(a.sheduleInit).getTime())
})

const uniqueClientsCount = computed(() =>
    new Set(filteredOrders.value.map(o=>o.clientId)).size
)
const ordersThisWeek = computed(() => {
  const weekAgo = Date.now() - 7*86400000
  return filteredOrders.value.filter(o=> new Date(o.sheduleInit).getTime() >= weekAgo).length
})

function getClientName(id: number) {
  const c = availableClients.value.find(x=>x.id===id)
  return c ? c.name : `Cliente ${id}`
}
function getPeriodLabel(p: string) {
  return {'7':'7d','30':'30d','90':'90d'}[p] || p
}
function onClientChange(){}
function onPeriodChange(){}
function resetFilters(){
  selectedClient.value = ''
  selectedPeriod.value = '30'
}

async function fetchAll() {
  const API = 'http://localhost:5181/api';

  const [
    orders,
    clients,
    processes,
    procPhases,
    ordPhases,
    phaseDefs,
    itemsRaw,
    lots,
    rawMaterials
  ] = await Promise.all([
    fetch(`${API}/ManufacturingOrder`).then(r => r.json()),
    fetch(`${API}/Client`).then(r => r.json()),
    fetch(`${API}/ManufacturingProcess`).then(r => r.json()),
    fetch(`${API}/ManufacturingProcessPhase`).then(r => r.json()),
    fetch(`${API}/ManufacturingOrderPhase`).then(r => r.json()),
    fetch(`${API}/ManufacturingPhase`).then(r => r.json()),
    fetch(`${API}/ItemOfRawMaterial`).then(r => r.json()),
    fetch(`${API}/LotOfRawMaterial`).then(r => r.json()),
    fetch(`${API}/RawMaterial`).then(r => r.json())
  ]);

  console.log('DADOS CARREGADOS:');
  console.log('ORDERS:', orders.length);
  console.log('CLIENTS:', clients.length);
  console.log('PROCESSES:', processes.length);
  console.log('ITEMS RAW:', itemsRaw.length);
  console.log('LOTS:', lots.length);
  console.log('RAW MATERIALS:', rawMaterials.length);

  availableClients.value = clients;
  manufacturingOrders.value = orders;

  type LotOfRawMaterial = { id: number; rawMaterialId: number };
  type RawMaterial = { rawId: number; name: string };

  console.log('Estrutura do primeiro lote:', lots[0]);
  const lotMap = new Map<number, LotOfRawMaterial>();
  for (const lotEntry of lots) {
    if (Array.isArray(lotEntry) && lotEntry.length >= 2) {
      const actualLot = lotEntry[1];
      if (actualLot && actualLot.lotId) {
        lotMap.set(actualLot.lotId, actualLot);
      }
    }
    else if (lotEntry && lotEntry.lotId) {
      lotMap.set(lotEntry.lotId, lotEntry);
    }
  }

  console.log('Estrutura do primeiro raw material:', rawMaterials[0]);

  const rawMap = new Map<number, RawMaterial>();
  for (const rawEntry of rawMaterials) {
    if (Array.isArray(rawEntry) && rawEntry.length >= 2) {
      const actualRaw = rawEntry[1];
      if (actualRaw && actualRaw.rawId) {
        rawMap.set(actualRaw.rawId, actualRaw);
      }
    }
    else if (rawEntry && rawEntry.rawId) {
      rawMap.set(rawEntry.rawId, rawEntry);
    }
  }

  const rawByPhase = new Map<number, Map<number, any[]>>();

  let processedItems = 0;
  let skippedItems = 0;

  for (const item of itemsRaw) {
    console.log(`Processing item ${item.itemRawId}, lotId: ${item.lotOfRawMaterialId}`);

    const lot = lotMap.get(item.lotOfRawMaterialId);
    if (!lot) {
      console.warn(`Lote ${item.lotOfRawMaterialId} não encontrado para item ${item.itemRawId}`);
      skippedItems++;
      continue;
    }

    const raw = rawMap.get(lot.rawMaterialId);
    if (!raw) {
      console.warn(`Material ${lot.rawMaterialId} não encontrado para lote ${lot.id}`);
      skippedItems++;
      continue;
    }

    const orderPhase = ordPhases.find(op => op.id === item.manufacturingOrderPhaseId);
    if (!orderPhase) {
      console.warn(`OrderPhase não encontrada para item ${item.itemRawId}`);
      skippedItems++;
      continue;
    }

    const orderId = orderPhase.manufacturingOrderId;

    const parsed = {
      itemRawId: item.itemRawId,
      quantity: item.quantity,
      unit: item.unit,
      rawMaterialName: raw.name
    };

    if (!rawByPhase.has(orderId)) {
      rawByPhase.set(orderId, new Map());
    }

    const phaseMap = rawByPhase.get(orderId)!;
    const list = phaseMap.get(item.manufacturingOrderPhaseId) || [];
    list.push(parsed);
    phaseMap.set(item.manufacturingOrderPhaseId, list);
    processedItems++;
  }


  for (const order of manufacturingOrders.value) {
    const proc = processes.find(p => p.id === order.manufacturingProcessId);
    order.process = proc || { processName: '–não encontrado–', info: '' };

    const steps = procPhases
        .filter(pp => pp.manufacturingProcessId === order.manufacturingProcessId)
        .sort((a, b) => a.numberStepOrder - b.numberStepOrder);

    order.phases = steps.map(s => {
      const def = phaseDefs.find(pd => pd.id === s.manufacturingPhaseId);
      const orderPhase = ordPhases.find(op =>
          op.manufacturingOrderId === order.id &&
          op.manufacturingPhaseId === s.manufacturingPhaseId
      );

      return {
        manufacturingPhaseId: s.manufacturingPhaseId,
        numberStepOrder: s.numberStepOrder,
        phaseInfo: def?.phaseInfo ?? '–sem definição–',
        orderPhaseId: orderPhase?.id,
        description: orderPhase?.customizationParams || '–sem descrição–'
      };
    });

    order.rawByPhase = {};
    for (const ph of order.phases) {
      if (!ph.orderPhaseId) continue;

      const phaseMap = rawByPhase.get(order.id) || new Map();
      const items = phaseMap.get(ph.orderPhaseId) || [];
      order.rawByPhase[ph.orderPhaseId] = items;
    }
  }

}


onMounted(fetchAll)
</script>


<style scoped>
table { border-collapse: collapse; width: 100%; }
th, td { border-bottom: 1px solid #E5E7EB; }
</style>
