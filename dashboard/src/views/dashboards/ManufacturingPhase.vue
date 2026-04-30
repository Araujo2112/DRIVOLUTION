<script lang="ts" setup>
import { ref, onMounted, computed } from 'vue'
import Button from "@/components/Button.vue"

interface ManufacturingPhaseDTO {
  id: number
  phaseInfo: string
  phaseDuration: number
  manufacturingPhaseId: string
  plantFloorSectionId: number
  plantFloorSectionName: string
}

interface PlantFloorSectionDTO {
  sectionId: number
  sectionCode: string
  id: { name: string }
}

const phases = ref<ManufacturingPhaseDTO[]>([])
const sections = ref<PlantFloorSectionDTO[]>([])
const error = ref<string | null>(null)
const showModal = ref(false)
const isEditing = ref(false)
const currentPage = ref(1)
const pageSize = 10

const paginatedPhases = computed(() => {
  const start = (currentPage.value - 1) * pageSize
  return phases.value.slice(start, start + pageSize)
})

const totalPages = computed(() => Math.ceil(phases.value.length / pageSize))

const newPhase = ref<{
  id?: number
  phaseInfo: string
  phaseDuration: number
  plantFloorSectionId: number
}>({
  phaseInfo: '',
  phaseDuration: 0,
  plantFloorSectionId: 0
})

async function fetchPhases() {
  try {
    const res = await fetch('http://localhost:5181/api/ManufacturingPhase')
    phases.value = await res.json()
  } catch {
    error.value = 'Error fetching manufacturing phases.'
  }
}

async function fetchSections() {
  try {
    const res = await fetch('http://localhost:5181/api/PlantFloorSection')
    sections.value = await res.json()
  } catch {
    error.value = 'Error fetching plant floor sections.'
  }
}

async function savePhase() {
  const method = isEditing.value ? 'PUT' : 'POST'
  const url = isEditing.value
      ? `http://localhost:5181/api/ManufacturingPhase/${newPhase.value.id}`
      : 'http://localhost:5181/api/ManufacturingPhase'

  const dto = isEditing.value
      ? {
        phaseInfo: newPhase.value.phaseInfo,
        phaseDuration: newPhase.value.phaseDuration
      }
      : {
        phaseInfo: newPhase.value.phaseInfo,
        phaseDuration: newPhase.value.phaseDuration,
        plantFloorSectionId: newPhase.value.plantFloorSectionId
      }

  try {
    await fetch(url, {
      method,
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(dto)
    })
    await fetchPhases()
    showModal.value = false
  } catch {
    error.value = 'Error saving manufacturing phase.'
  }
}

function openCreateModal() {
  isEditing.value = false
  newPhase.value = {
    phaseInfo: '',
    phaseDuration: 0,
    plantFloorSectionId: sections.value[0]?.sectionId || 0
  }
  showModal.value = true
}

function openEditModal(phase: ManufacturingPhaseDTO) {
  isEditing.value = true
  newPhase.value = {
    id: phase.id,
    phaseInfo: phase.phaseInfo,
    phaseDuration: phase.phaseDuration,
    plantFloorSectionId: phase.plantFloorSectionId
  }
  showModal.value = true
}

async function deletePhase(id: number) {
  try {
    await fetch(`http://localhost:5181/api/ManufacturingPhase/${id}`, {
      method: 'DELETE'
    })
    await fetchPhases()
  } catch {
    error.value = 'Error deleting manufacturing phase.'
  }
}

onMounted(async () => {
  await Promise.all([fetchPhases(), fetchSections()])
})
</script>


<template>
  <div class="p-6">
    <h1 class="text-2xl font-bold mb-4">Manufacturing Phases</h1>

    <div class="w-full flex flex-col gap-2 items-end mb-2">
      <Button icon="add" @click="openCreateModal">New Phase</Button>
    </div>

    <table class="w-full">
      <thead>
      <tr class="bg-gray-500 text-white">
        <th class="p-2 text-left">ID</th>
        <th class="p-2 text-left">Name</th>
        <th class="p-2 text-left">Duration</th>
        <th class="p-2 text-left">Section</th>
        <th class="p-2 text-right">Actions</th>
      </tr>
      </thead>
      <tbody>
      <tr v-for="phase in paginatedPhases" :key="phase.id" class="border-b">
        <td class="p-2">{{ phase.id }}</td>
        <td class="p-2">{{ phase.phaseInfo }}</td>
        <td class="p-2">{{ phase.phaseDuration }}</td>
        <td class="p-2">
          {{ sections.find(s => s.sectionId === phase.plantFloorSectionId)?.id.name || phase.plantFloorSectionId }}
        </td>
        <td class="p-2 flex gap-2 justify-end">
          <Button icon="edit" variant="ghost" @click="openEditModal(phase)" />
          <Button icon="delete" variant="ghost" @click="deletePhase(phase.id)" />
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
        <h2 class="text-lg font-bold mb-4">{{ isEditing ? 'Edit Phase' : 'New Phase' }}</h2>
        <form @submit.prevent="savePhase" class="space-y-4">
          <div>
            <label class="block">Name</label>
            <input v-model="newPhase.phaseInfo" type="text" class="w-full border p-2 rounded" required />
          </div>
          <div>
            <label class="block">Duration</label>
            <input v-model.number="newPhase.phaseDuration" type="number" class="w-full border p-2 rounded" required />
          </div>
          <div v-if="!isEditing">
            <label class="block">Plant Floor Section</label>
            <select v-model.number="newPhase.plantFloorSectionId" class="w-full border p-2 rounded">
              <option v-for="s in sections" :value="s.sectionId" :key="s.sectionId">{{ s.id.name }}</option>
            </select>
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


