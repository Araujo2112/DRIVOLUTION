<script lang="ts" setup>
import { ref, onMounted } from 'vue'

interface ManufacturingOrderDTO {
  id: number
  phaseInfo: string
  phaseDuration: number
  manufacturingOrderId: string
  clientId: number
  manufacturingProcessId: number
  productLotId: number
}

interface ClientDTO {
  id: number
  name: string
}

interface ManufacturingProcessDTO {
  id: number
  processName: string
}

interface ProductLotDTO {
  id: number
  lotNumber: string
}

const orders = ref<ManufacturingOrderDTO[]>([])
const clients = ref<ClientDTO[]>([])
const processes = ref<ManufacturingProcessDTO[]>([])
const lots = ref<ProductLotDTO[]>([])
const error = ref<string | null>(null)
const showModal = ref(false)
const isEditing = ref(false)

const newOrder = ref<ManufacturingOrderDTO>({
  id: 0,
  phaseInfo: '',
  phaseDuration: 0,
  manufacturingOrderId: '',
  clientId: 0,
  manufacturingProcessId: 0,
  productLotId: 0
})

async function fetchAll() {
  try {
    const [ordersRes, clientsRes, processesRes, lotsRes] = await Promise.all([
      fetch('http://localhost:5181/api/ManufacturingOrder'),
      fetch('http://localhost:5181/api/Client'),
      fetch('http://localhost:5181/api/ManufacturingProcess'),
      fetch('http://localhost:5181/api/ProductLot')
    ])

    orders.value = await ordersRes.json()
    clients.value = await clientsRes.json()
    processes.value = await processesRes.json()
    lots.value = await lotsRes.json()
  } catch (err) {
    error.value = 'Erro ao buscar dados.'
  }
}

async function saveOrder() {
  const method = isEditing.value ? 'PUT' : 'POST'
  const url = `http://localhost:5181/api/ManufacturingOrder/${isEditing.value ? 'update' : 'create'}`
  const dto = isEditing.value
      ? {
        id: newOrder.value.id,
        phaseInfo: newOrder.value.phaseInfo,
        phaseDuration: newOrder.value.phaseDuration
      }
      : {
        phaseInfo: newOrder.value.phaseInfo,
        phaseDuration: newOrder.value.phaseDuration,
        manufacturingOrderId: newOrder.value.manufacturingOrderId,
        clientId: newOrder.value.clientId,
        manufacturingProcessId: newOrder.value.manufacturingProcessId,
        productLotId: newOrder.value.productLotId
      }

  try {
    await fetch(url, {
      method,
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(dto)
    })
    await fetchAll()
    showModal.value = false
  } catch {
    error.value = 'Erro ao salvar ordem.'
  }
}

function openCreateModal() {
  isEditing.value = false
  newOrder.value = {
    id: 0,
    phaseInfo: '',
    phaseDuration: 0,
    manufacturingOrderId: '',
    clientId: clients.value[0]?.id || 0,
    manufacturingProcessId: processes.value[0]?.id || 0,
    productLotId: lots.value[0]?.id || 0
  }
  showModal.value = true
}

function openEditModal(order: ManufacturingOrderDTO) {
  isEditing.value = true
  newOrder.value = { ...order }
  showModal.value = true
}

async function deleteOrder(id: number) {
  try {
    await fetch('http://localhost:5181/api/ManufacturingOrder/delete', {
      method: 'DELETE',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ id })
    })
    await fetchAll()
  } catch {
    error.value = 'Erro ao deletar ordem.'
  }
}

onMounted(fetchAll)
</script>

<template>
  <div class="p-6">
    <h1 class="text-2xl font-bold mb-4">Ordens de Fabrico</h1>

    <button class="mb-4 px-4 py-2 bg-blue-600 text-white rounded" @click="openCreateModal">
      Nova Ordem
    </button>

    <table class="w-full table-auto border">
      <thead>
      <tr class="bg-gray-500">
        <th class="p-2 text-left">ID</th>
        <th class="p-2 text-left">Identificador</th>
        <th class="p-2 text-left">Cliente</th>
        <th class="p-2 text-left">Processo</th>
        <th class="p-2 text-left">Lote</th>
        <th class="p-2 text-left">Duração</th>
        <th class="p-2 text-left">Fase</th>
        <th class="p-2 text-left">Ações</th>
      </tr>
      </thead>
      <tbody>
      <tr v-for="order in orders" :key="order.id" class="border-t" >
        <td class="p-2">{{ order.id }}</td>
        <td class="p-2">{{ order.manufacturingOrderId }}</td>
        <td class="p-2">{{ clients.find(c => c.id === order.clientId)?.name || '—' }}</td>
        <td class="p-2">{{ processes.find(p => p.id === order.manufacturingProcessId)?.processName || '—' }}</td>
        <td class="p-2">{{ lots.find(l => l.id === order.productLotId)?.lotNumber || '—' }}</td>
        <td class="p-2">{{ order.phaseDuration }}</td>
        <td class="p-2">{{ order.phaseInfo }}</td>
        <td class="p-2 flex gap-2">
          <button class="text-blue-600" @click="openEditModal(order)">Editar</button>
          <button class="text-red-600" @click="deleteOrder(order.id)">Eliminar</button>
        </td>
      </tr>
      </tbody>
    </table>

    <div v-if="showModal" class="fixed inset-0 bg-black/50 flex items-center justify-center">
      <div class="bg-white p-6 rounded w-full max-w-md">
        <h2 class="text-lg font-bold mb-4">{{ isEditing ? 'Editar' : 'Nova' }} Ordem</h2>
        <form @submit.prevent="saveOrder" class="space-y-4">
          <div v-if="isEditing">
            <label class="block">ID (só leitura)</label>
            <input :value="newOrder.id" class="w-full border p-2 rounded bg-gray-100" disabled />
          </div>
          <div v-else>
            <input :value="newOrder.id" class="w-full border p-2 rounded bg-gray-100" hidden="hidden" />
          </div>
          <div>
            <label class="block">Identificador</label>
            <input v-model="newOrder.manufacturingOrderId" type="text" class="w-full border p-2 rounded" required />
          </div>
          <div>
            <label class="block">Fase</label>
            <input v-model="newOrder.phaseInfo" type="text" class="w-full border p-2 rounded" required />
          </div>
          <div>
            <label class="block">Duração</label>
            <input v-model.number="newOrder.phaseDuration" type="number" class="w-full border p-2 rounded" required />
          </div>
          <div v-if="!isEditing">
            <label class="block">Cliente</label>
            <select v-model.number="newOrder.clientId" class="w-full border p-2 rounded" required>
              <option v-for="c in clients" :value="c.id">{{ c.name }}</option>
            </select>
          </div>
          <div v-if="!isEditing">
            <label class="block">Processo</label>
            <select v-model.number="newOrder.manufacturingProcessId" class="w-full border p-2 rounded" required>
              <option v-for="p in processes" :value="p.id">{{ p.processName }}</option>
            </select>
          </div>
          <div v-if="!isEditing">
            <label class="block">Lote</label>
            <select v-model.number="newOrder.productLotId" class="w-full border p-2 rounded" required>
              <option v-for="l in lots" :value="l.id">{{ l.lotNumber }}</option>
            </select>
          </div>
          <div class="flex justify-end gap-2">
            <button type="button" class="px-4 py-2 border rounded" @click="showModal = false">Cancelar</button>
            <button type="submit" class="px-4 py-2 bg-blue-600 text-white rounded">Salvar</button>
          </div>
        </form>
      </div>
    </div>

    <div v-if="error" class="text-red-600 mt-4">{{ error }}</div>
  </div>
</template>

<style scoped>
th, td {
  border: 1px solid #ccc;
}
</style>
