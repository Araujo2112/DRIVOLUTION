<script lang="ts" setup>
import { ref, onMounted, computed } from 'vue'
import Button from "@/components/Button.vue"

interface ProductDTO {
  id: number
  name: string
}

interface ProductLotDTO {
  id: number
  lotNumber: string
  lotUnit: string
  lotQuantity: number
  ready: boolean
  productName: string
  productLotId: string
  productId: string
  info: string
}

const productLots = ref<ProductLotDTO[]>([])
const products = ref<ProductDTO[]>([])
const error = ref<string | null>(null)
const showModal = ref(false)
const isEditing = ref(false)

const currentPage = ref(1)
const pageSize = 10

const paginatedLots = computed(() => {
  const start = (currentPage.value - 1) * pageSize
  return productLots.value.slice(start, start + pageSize)
})

const totalPages = computed(() => Math.ceil(productLots.value.length / pageSize))

const newLot = ref<{
  id?: number
  lotNumber: string
  lotUnit: string
  lotQuantity: number
  ready: boolean
  productLotId: string
  productId: string
}>({
  lotNumber: '',
  lotUnit: '',
  lotQuantity: 0,
  ready: false,
  productLotId: '',
  productId: ''
})

async function fetchProductLots() {
  try {
    const res = await fetch('http://localhost:5181/api/ProductLot')
    productLots.value = await res.json()
    currentPage.value = 1
  } catch (err) {
    error.value = 'Error fetching product lots.'
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

async function saveProductLot() {
  const method = isEditing.value ? 'PUT' : 'POST'
  const url = isEditing.value
      ? `http://localhost:5181/api/ProductLot/${newLot.value.id}`
      : 'http://localhost:5181/api/ProductLot'

  const dto = isEditing.value
      ? {
        lotUnit: newLot.value.lotUnit,
        lotQuantity: newLot.value.lotQuantity,
        ready: newLot.value.ready,
        productId: parseInt(newLot.value.productId)
      }
      : {
        lotNumber: newLot.value.lotNumber,
        lotUnit: newLot.value.lotUnit,
        lotQuantity: newLot.value.lotQuantity,
        ready: newLot.value.ready,
        productLotId: newLot.value.lotQuantity.toString(),
        productId: parseInt(newLot.value.productId)
      }

  try {
    await fetch(url, {
      method,
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(dto)
    })
    await fetchProductLots()
    showModal.value = false
  } catch {
    error.value = 'Error saving product lot.'
  }
}

function openCreateModal() {
  isEditing.value = false
  newLot.value = {
    lotNumber: '',
    lotUnit: '',
    lotQuantity: 0,
    ready: false,
    productLotId: '',
    productId: products.value[0]?.id.toString() || ''
  }
  showModal.value = true
}

function openEditModal(lot: ProductLotDTO) {
  isEditing.value = true
  newLot.value = {
    id: lot.id,
    lotNumber: lot.lotNumber,
    lotUnit: lot.lotUnit,
    lotQuantity: lot.lotQuantity,
    ready: lot.ready,
    productLotId: lot.lotQuantity.toString(),
    productId: lot.productId.toString()
  }
  showModal.value = true
}

async function deleteProductLot(id: number) {
  try {
    await fetch(`http://localhost:5181/api/ProductLot/${id}`, {
      method: 'DELETE'
    })
    await fetchProductLots()
  } catch {
    error.value = 'Error deleting product lot.'
  }
}

onMounted(async () => {
  await Promise.all([fetchProductLots(), fetchProducts()])
})
</script>

<template>
  <div class="p-6">
    <h1 class="text-2xl font-bold mb-4">Product Lots</h1>

    <div class="w-full flex flex-col gap-2 items-end mb-2">
      <Button icon="add" @click="openCreateModal">New Product Lot</Button>
    </div>

    <table class="w-full">
      <thead>
      <tr class="bg-gray-500 text-white">
        <th class="text-left p-2">ID</th>
        <th class="text-left p-2">Lot Number</th>
        <th class="text-left p-2">Unit</th>
        <th class="text-left p-2">Quantity</th>
        <th class="text-left p-2">Ready</th>
        <th class="text-left p-2">Product</th>
        <th class="text-right p-2">Actions</th>
      </tr>
      </thead>
      <tbody>
      <tr v-for="lot in paginatedLots" :key="lot.id" class="border-b">
        <td class="p-2">{{ lot.id }}</td>
        <td class="p-2">{{ lot.lotNumber }}</td>
        <td class="p-2">{{ lot.lotUnit }}</td>
        <td class="p-2">{{ lot.lotQuantity }}</td>
        <td class="p-2">{{ lot.ready ? 'Yes' : 'No' }}</td>
        <td class="p-2">{{ lot.productName }}</td>
        <td class="p-2 flex gap-2 justify-end">
          <Button icon="edit" variant="ghost" @click="openEditModal(lot)" />
          <Button icon="delete" variant="ghost" @click="deleteProductLot(lot.id)" />
        </td>
      </tr>
      </tbody>
    </table>

    <div class="flex justify-center items-center gap-4 mt-4">
      <Button variant="outline" :disabled="currentPage === 1" @click="currentPage--">Previous</Button>
      <span class="text-sm text-gray-700">Page {{ currentPage }} of {{ totalPages }}</span>
      <Button variant="outline" :disabled="currentPage === totalPages" @click="currentPage++">Next</Button>
    </div>

    <div v-if="showModal" class="fixed inset-0 bg-black/50 flex items-center justify-center">
      <div class="bg-white p-6 rounded w-full max-w-md">
        <h2 class="text-lg font-bold mb-4">{{ isEditing ? 'Edit Lot' : 'New Lot' }}</h2>
        <form @submit.prevent="saveProductLot" class="space-y-4">
          <div>
            <label class="block">Lot Number</label>
            <input v-model="newLot.lotNumber" type="text" class="w-full border p-2 rounded" required />
          </div>
          <div>
            <label class="block">Unit</label>
            <input v-model="newLot.lotUnit" type="text" class="w-full border p-2 rounded" required />
          </div>
          <div>
            <label class="block">Quantity</label>
            <input v-model.number="newLot.lotQuantity" type="number" class="w-full border p-2 rounded" required />
          </div>
          <div>
            <label class="block">Ready</label>
            <input v-model="newLot.ready" type="checkbox" />
          </div>
          <div>
            <label class="block">Product</label>
            <select v-model="newLot.productId" class="w-full border p-2 rounded">
              <option v-for="p in products" :value="p.id.toString()" :key="p.id">{{ p.name }}</option>
            </select>
          </div>
          <div class="flex justify-end gap-2">
            <Button type="button" @click="showModal = false">Cancel</Button>
            <Button type="submit">Save</Button>
          </div>
        </form>
      </div>
    </div>

    <div v-if="error" class="text-red-600 mt-4">{{ error }}</div>
  </div>
</template>
