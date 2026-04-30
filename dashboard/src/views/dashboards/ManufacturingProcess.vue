<script lang="ts" setup>
import { ref, onMounted, computed } from 'vue'
import Button from "@/components/Button.vue"

interface ManufacturingProcessDTO {
  id: number
  processName: string
  info: string
  productId: number
  productName?: string
}

interface ProductDTO {
  id: number
  name: string
}

const processes = ref<ManufacturingProcessDTO[]>([])
const products = ref<ProductDTO[]>([])
const error = ref<string | null>(null)
const showModal = ref(false)
const isEditing = ref(false)

const currentPage = ref(1)
const pageSize = 10

const paginatedProcesses = computed(() => {
  const start = (currentPage.value - 1) * pageSize
  return processes.value.slice(start, start + pageSize)
})

const totalPages = computed(() => Math.ceil(processes.value.length / pageSize))

const newProcess = ref<ManufacturingProcessDTO>({
  id: 0,
  processName: '',
  info: '',
  productId: 0
})

async function fetchProcesses() {
  try {
    const res = await fetch('http://localhost:5181/api/ManufacturingProcess')
    processes.value = await res.json()
    currentPage.value = 1
  } catch (err) {
    error.value = 'Error fetching manufacturing processes.'
  }
}

async function fetchProducts() {
  try {
    const res = await fetch('http://localhost:5181/api/Product')
    products.value = await res.json()
  } catch (err) {
    error.value = 'Error fetching products.'
  }
}

async function saveProcess() {
  const method = isEditing.value ? 'PUT' : 'POST'
  const url = isEditing.value
      ? `http://localhost:5181/api/ManufacturingProcess/${newProcess.value.id}`
      : `http://localhost:5181/api/ManufacturingProcess`

  const dto = isEditing.value
      ? {
        processName: newProcess.value.processName,
        info: newProcess.value.info
      }
      : {
        processName: newProcess.value.processName,
        info: newProcess.value.info,
        productId: newProcess.value.productId
      }

  try {
    await fetch(url, {
      method,
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(dto)
    })
    await fetchProcesses()
    showModal.value = false
  } catch {
    error.value = 'Error saving manufacturing process.'
  }
}

function openCreateModal() {
  isEditing.value = false
  newProcess.value = {
    id: 0,
    processName: '',
    info: '',
    productId: products.value[0]?.id || 0
  }
  showModal.value = true
}

function openEditModal(process: ManufacturingProcessDTO) {
  isEditing.value = true
  newProcess.value = { ...process }
  showModal.value = true
}

async function deleteProcess(id: number) {
  try {
    await fetch(`http://localhost:5181/api/ManufacturingProcess/${id}`, {
      method: 'DELETE',
      headers: { 'Content-Type': 'application/json' }
    })
    await fetchProcesses()
  } catch {
    error.value = 'Error deleting manufacturing process.'
  }
}

onMounted(async () => {
  await Promise.all([fetchProcesses(), fetchProducts()])
})
</script>

<template>
  <div class="p-6">
    <h1 class="text-2xl font-bold mb-4">Manufacturing Processes</h1>

    <div class="w-full flex flex-col gap-2 items-end mb-2">
      <Button icon="add" @click="openCreateModal">New Process</Button>
    </div>

    <table class="w-full">
      <thead>
      <tr class="bg-gray-500 text-white">
        <th class="p-2 text-left">ID</th>
        <th class="p-2 text-left">Name</th>
        <th class="p-2 text-left">Info</th>
        <th class="p-2 text-left">Product</th>
        <th class="p-2 text-right">Actions</th>
      </tr>
      </thead>
      <tbody>
      <tr v-for="process in paginatedProcesses" :key="process.id" class="border-b">
        <td class="p-2">{{ process.id }}</td>
        <td class="p-2">{{ process.processName }}</td>
        <td class="p-2">{{ process.info }}</td>
        <td class="p-2">
          {{ process.productName || products.find(p => p.id === process.productId)?.name || process.productId }}
        </td>
        <td class="p-2 flex gap-2 justify-end">
          <Button icon="edit" variant="ghost" @click="openEditModal(process)" />
          <Button icon="delete" variant="ghost" @click="deleteProcess(process.id)" />
        </td>
      </tr>
      </tbody>
    </table>

    <!-- Pagination -->
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

    <!-- Modal -->
    <div v-if="showModal" class="fixed inset-0 bg-black/50 flex items-center justify-center">
      <div class="bg-white p-6 rounded w-full max-w-md">
        <h2 class="text-lg font-bold mb-4">{{ isEditing ? 'Edit Process' : 'New Process' }}</h2>
        <form @submit.prevent="saveProcess" class="space-y-4">
          <div>
            <label class="block">Name</label>
            <input v-model="newProcess.processName" type="text" class="w-full border p-2 rounded" required />
          </div>
          <div>
            <label class="block">Info</label>
            <input v-model="newProcess.info" type="text" class="w-full border p-2 rounded" required />
          </div>
          <div v-if="!isEditing">
            <label class="block">Product</label>
            <select v-model.number="newProcess.productId" class="w-full border p-2 rounded">
              <option v-for="p in products" :value="p.id" :key="p.id">{{ p.name }}</option>
            </select>
          </div>
          <div class="flex justify-end gap-2">
            <Button type="outline" @click="showModal = false">Cancel</Button>
            <Button type="submit">Save</Button>
          </div>
        </form>
      </div>
    </div>

    <div v-if="error" class="text-red-600 mt-4">{{ error }}</div>
  </div>
</template>
