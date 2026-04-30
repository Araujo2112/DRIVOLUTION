<template>
  <div class="p-6">
    <h1 class="text-2xl font-bold mb-4">Clients</h1>

    <div class="w-full flex flex-col gap-2 items-end mb-2">
      <Button icon="add" @click="openCreateModal">New Client</Button>
    </div>

    <table class="w-full">
      <thead>
      <tr class="bg-gray-500 text-white">
        <th class="p-2 text-left">ID</th>
        <th class="p-2 text-left">Name</th>
        <th class="p-2 text-left">NIF</th>
        <th class="p-2 text-right">Actions</th>
      </tr>
      </thead>
      <tbody>
      <tr v-for="client in paginatedClients" :key="client.id" class="border-b">
        <td class="p-2">{{ client.id }}</td>
        <td class="p-2">{{ client.name }}</td>
        <td class="p-2">{{ client.fiscalNumber }}</td>
        <td class="p-2 flex gap-2 justify-end">
          <Button icon="graph_4" variant="ghost" @click="viewGraphForClient(client.id)" />
          <Button icon="edit" variant="ghost" @click="openEditModal(client)" />
          <Button icon="delete" variant="ghost" @click="deleteClient(client.id)" />
        </td>
      </tr>
      </tbody>
    </table>

    <div class="flex justify-center items-center gap-4 mt-4">
      <Button variant="outline" :disabled="currentPage === 1" @click="currentPage--">
        Previous
      </Button>
      <span class="text-sm text-gray-700">
        Page {{ currentPage }} of {{ totalPages }}
      </span>
      <Button variant="outline" :disabled="currentPage === totalPages" @click="currentPage++">
        Next
      </Button>
    </div>

    <div v-if="showModal" class="fixed inset-0 bg-black/50 flex items-center justify-center">
      <div class="bg-white p-6 rounded w-full max-w-md">
        <h2 class="text-lg font-bold mb-4">{{ isEditing ? 'Edit Client' : 'New Client' }}</h2>
        <form @submit.prevent="saveClient" class="space-y-4">
          <div>
            <label class="block">Name</label>
            <input v-model="newClient.name" type="text" class="w-full border p-2 rounded" required />
          </div>
          <div>
            <label class="block">NIF</label>
            <input v-model="newClient.fiscalNumber" type="number" class="w-full border p-2 rounded" required />
          </div>
          <div class="flex justify-end gap-2">
            <Button variant="outline" @click="showModal = false">Cancel</Button>
            <Button type="submit">Save</Button>
          </div>
        </form>
      </div>
    </div>

    <div v-if="error" class="text-red-600 mt-4">{{ error }}</div>
  </div>
</template>

<script lang="ts" setup>
import { ref, onMounted, computed } from 'vue'
import Button from '@/components/Button.vue'

interface ClientDTO {
  id: number
  name: string
  fiscalNumber: string
}

interface ManufacturingOrder {
  id: number
  clientId: number
  createdAt: string
}

const clients = ref<ClientDTO[]>([])
const ordersByClient = ref<ManufacturingOrder[]>([])
const error = ref<string | null>(null)
const showModal = ref(false)
const isEditing = ref(false)

const currentPage = ref(1)
const pageSize = 10

const paginatedClients = computed(() => {
  const start = (currentPage.value - 1) * pageSize
  return clients.value.slice(start, start + pageSize)
})

const totalPages = computed(() => Math.ceil(clients.value.length / pageSize))

const newClient = ref<ClientDTO>({
  id: 0,
  name: '',
  fiscalNumber: ''
})

async function fetchClients() {
  try {
    const res = await fetch('http://localhost:5181/api/Client')
    clients.value = await res.json()
    currentPage.value = 1
  } catch (err) {
    error.value = 'Error fetching clients.'
  }
}

async function fetchOrdersByClient() {
  try {
    const res = await fetch('http://localhost:5181/api/ManufacturingOrder')
    ordersByClient.value = await res.json()
  } catch (err) {
    error.value = 'Erro ao carregar ordens.'
  }
}

async function saveClient() {
  const method = isEditing.value ? 'PUT' : 'POST'
  const url = isEditing.value
      ? `http://localhost:5181/api/Client/${newClient.value.id}`
      : `http://localhost:5181/api/Client`

  const dto = {
    name: newClient.value.name,
    fiscalNumber: newClient.value.fiscalNumber
  }

  try {
    await fetch(url, {
      method,
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(dto)
    })
    await fetchClients()
    showModal.value = false
  } catch (err) {
    error.value = 'Error saving client.'
  }
}

function openCreateModal() {
  isEditing.value = false
  newClient.value = { id: 0, name: '', fiscalNumber: '' }
  showModal.value = true
}

function openEditModal(client: ClientDTO) {
  isEditing.value = true
  newClient.value = { ...client }
  showModal.value = true
}

async function deleteClient(id: number) {
  try {
    await fetch(`http://localhost:5181/api/Client/${id}`, {
      method: 'DELETE'
    })
    await fetchClients()
  } catch (err) {
    error.value = 'Error deleting client.'
  }
}

function viewGraphForClient(clientId: number) {
  const orders = ordersByClient.value
      .filter(o => o.clientId === clientId)
      .sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime())

  if (orders.length > 0) {
    const mostRecentOrder = orders[0]
    window.location.href = `/dashboard/manufacturingOrdersGraph?orderId=${mostRecentOrder.id}`
  } else {
    error.value = 'Nenhuma ordem encontrada para este cliente.'
  }
}

onMounted(() => {
  fetchClients()
  fetchOrdersByClient()
})
</script>
