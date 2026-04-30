<script lang="ts" setup>
import { ref, onMounted, computed } from 'vue'
import Button from "@/components/Button.vue"

interface ProductDTO {
  id: number
  name: string
  info: string
  productId: string
}

const products = ref<ProductDTO[]>([])
const error = ref<string | null>(null)
const showModal = ref(false)
const isEditing = ref(false)

const currentPage = ref(1)
const pageSize = 10

const paginatedProducts = computed(() => {
  const start = (currentPage.value - 1) * pageSize
  return products.value.slice(start, start + pageSize)
})

const totalPages = computed(() => Math.ceil(products.value.length / pageSize))

const newProduct = ref<{
  id?: number
  name: string
  info: string
  productId: string
}>({
  name: '',
  info: '',
  productId: ''
})

async function fetchProducts() {
  try {
    const res = await fetch('http://localhost:5181/api/Product')
    products.value = await res.json()
    currentPage.value = 1
  } catch (err) {
    error.value = 'Error fetching products.'
  }
}

async function saveProduct() {
  const method = isEditing.value ? 'PUT' : 'POST'
  const url = isEditing.value
      ? `http://localhost:5181/api/Product/${newProduct.value.id}`
      : 'http://localhost:5181/api/Product'

  const dto = {
    name: newProduct.value.name,
    info: newProduct.value.info,
    productId: newProduct.value.name
  }

  try {
    await fetch(url, {
      method,
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(dto)
    })
    await fetchProducts()
    showModal.value = false
  } catch {
    error.value = 'Error saving product.'
  }
}

function openCreateModal() {
  isEditing.value = false
  newProduct.value = { name: '', info: '', productId: '' }
  showModal.value = true
}

function openEditModal(product: ProductDTO) {
  isEditing.value = true
  newProduct.value = { ...product }
  showModal.value = true
}

async function deleteProduct(id: number) {
  try {
    await fetch(`http://localhost:5181/api/Product/${id}`, {
      method: 'DELETE'
    })
    await fetchProducts()
  } catch {
    error.value = 'Error deleting product.'
  }
}

onMounted(fetchProducts)
</script>

<template>
  <div class="p-6">
    <h1 class="text-2xl font-bold mb-4">Products</h1>

    <div class="w-full flex flex-col gap-2 items-end mb-2">
      <Button icon="add" @click="openCreateModal">New Product</Button>
    </div>

    <table class="w-full">
      <thead>
      <tr class="bg-gray-500 text-white">
        <th class="text-left p-2">ID</th>
        <th class="text-left p-2">Name</th>
        <th class="text-left p-2">Info</th>
        <th class="text-left p-2">Product ID</th>
        <th class="text-right p-2"></th>
      </tr>
      </thead>
      <tbody>
      <tr v-for="product in paginatedProducts" :key="product.id" class="border-b">
        <td class="p-2">{{ product.id }}</td>
        <td class="p-2">{{ product.name }}</td>
        <td class="p-2">{{ product.info }}</td>
        <td class="p-2">{{ product.productId }}</td>
        <td class="p-2 flex gap-2 justify-end">
          <Button icon="edit" variant="ghost" @click="openEditModal(product)" />
          <Button icon="delete" variant="ghost" @click="deleteProduct(product.id)" />
        </td>
      </tr>
      </tbody>
    </table>

    <!-- Pagination -->
    <div class="flex justify-center items-center mt-4 gap-4">
      <Button variant="outline" :disabled="currentPage === 1" @click="currentPage--">Previous</Button>
      <span>Page {{ currentPage }} of {{ totalPages }}</span>
      <Button variant="outline" :disabled="currentPage === totalPages" @click="currentPage++">Next</Button>
    </div>

    <!-- Modal -->
    <div v-if="showModal" class="fixed inset-0 bg-black/50 flex items-center justify-center">
      <div class="bg-white p-6 rounded w-full max-w-md">
        <h2 class="text-lg font-bold mb-4">{{ isEditing ? 'Edit Product' : 'New Product' }}</h2>
        <form @submit.prevent="saveProduct" class="space-y-4">
          <div>
            <label class="block">Name</label>
            <input v-model="newProduct.name" type="text" class="w-full border p-2 rounded" required />
          </div>
          <div>
            <label class="block">Info</label>
            <input v-model="newProduct.info" type="text" class="w-full border p-2 rounded" required />
          </div>
          <div v-if="isEditing">
            <label class="block">Product ID</label>
            <input v-model="newProduct.productId" type="text" class="w-full border p-2 rounded" disabled />
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
