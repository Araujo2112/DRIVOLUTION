<template>
  <div class="p-8 w-full">

    <!-- Header -->
    <div class="flex items-start justify-between mb-8">
      <div>
        <h1 class="text-2xl font-medium text-background-900 dark:text-background-50">
          {{ t('productionLines.title') }}
        </h1>
        <p class="text-sm text-background-600 dark:text-background-400 mt-1">
          {{ t('productionLines.subtitle') }}
        </p>
      </div>
      <button
        @click="openCreateLine"
        class="flex items-center gap-2 bg-primary-500 hover:bg-primary-600 text-white text-sm font-medium px-4 py-2 rounded-lg transition-colors"
      >
        <span class="material-symbols-rounded text-base">add</span>
        {{ t('productionLines.newLine') }}
      </button>
    </div>

    <!-- Loading -->
    <div v-if="loading" class="flex items-center gap-2 text-background-500 text-sm py-12">
      <span class="material-symbols-rounded animate-spin text-lg">autorenew</span>
      {{ t('common.loading') }}
    </div>

    <!-- Grelha de Linhas -->
    <div v-else>
      <div v-if="lines.length === 0" class="text-center py-16 text-background-500">
        <span class="material-symbols-rounded text-5xl block mb-3">factory</span>
        <p class="text-sm">{{ t('productionLines.empty') }}</p>
      </div>

      <div v-else class="flex flex-col gap-4">
        <div
          v-for="line in lines"
          :key="line.id"
          class="bg-background-50 dark:bg-background-800 border border-background-300 dark:border-background-700 rounded-xl overflow-hidden"
        >
          <!-- Line Header -->
          <div class="flex items-center justify-between px-5 py-4 border-b border-background-300 dark:border-background-700">
            <div class="flex items-center gap-3">
              <div class="w-10 h-10 rounded-lg bg-primary-50 dark:bg-primary-950 flex items-center justify-center">
                <span class="material-symbols-rounded text-primary-500 text-xl">conveyor_belt</span>
              </div>
              <div>
                <div class="text-sm font-medium text-background-900 dark:text-background-50">{{ line.name }}</div>
                <div class="flex gap-2 mt-1 flex-wrap">
                  <span v-if="line.location" class="text-xs px-2 py-0.5 rounded-full bg-background-200 dark:bg-background-700 text-background-600 dark:text-background-400 border border-background-300 dark:border-background-600 flex items-center gap-1">
                    <span class="material-symbols-rounded text-xs">location_on</span>
                    {{ line.location }}
                  </span>
                  <span v-if="line.capacity" class="text-xs px-2 py-0.5 rounded-full bg-background-200 dark:bg-background-700 text-background-600 dark:text-background-400 border border-background-300 dark:border-background-600">
                    {{ line.capacity }} {{ t('productionLines.capacity') }}
                  </span>
                  <span
                    v-if="line.status"
                    class="text-xs px-2 py-0.5 rounded-full font-medium"
                    :class="lineStatusClass(line.status)"
                  >
                    {{ t(`productionLines.status.${line.status}`) }}
                  </span>
                </div>
              </div>
            </div>
            <div class="flex items-center gap-1">
              <button
                @click="openEditStatus(line)"
                class="p-2 rounded-lg text-background-500 hover:text-primary-500 hover:bg-primary-50 dark:hover:bg-primary-950 transition-colors"
              >
                <span class="material-symbols-rounded text-lg">edit</span>
              </button>
              <button
                @click="toggleLine(line.id)"
                class="p-2 rounded-lg text-background-500 hover:text-background-700 hover:bg-background-100 dark:hover:bg-background-700 transition-colors"
              >
                <span class="material-symbols-rounded text-lg transition-transform duration-200" :class="expandedLineId === line.id ? 'rotate-180' : ''">
                  expand_more
                </span>
              </button>
              <button
                @click="deleteLine(line)"
                class="p-2 rounded-lg text-background-500 hover:text-danger-500 hover:bg-danger-100 dark:hover:bg-background-700 transition-colors"
              >
                <span class="material-symbols-rounded text-lg">delete</span>
              </button>
            </div>
          </div>

          <!-- Workstations -->
          <div v-if="expandedLineId === line.id" class="px-5 py-4">
            <div class="flex items-center justify-between mb-3">
              <span class="text-xs font-medium text-background-500 uppercase tracking-wider">
                {{ t('productionLines.workstations') }}
                <span class="ml-1 text-background-400">({{ workstationsByLine[line.id]?.length ?? 0 }})</span>
              </span>
              <button
                @click="openCreateWorkstation(line.id)"
                class="flex items-center gap-1 text-xs text-primary-500 hover:bg-primary-50 dark:hover:bg-primary-950 px-2 py-1 rounded-md transition-colors"
              >
                <span class="material-symbols-rounded text-sm">add</span>
                {{ t('productionLines.addWorkstation') }}
              </button>
            </div>

            <div v-if="loadingWorkstations[line.id]" class="flex items-center gap-2 text-background-400 text-xs py-2">
              <span class="material-symbols-rounded animate-spin text-sm">autorenew</span>
              {{ t('common.loading') }}
            </div>

            <p v-else-if="!workstationsByLine[line.id]?.length" class="text-xs text-background-400 py-1">
              {{ t('productionLines.noWorkstations') }}
            </p>

            <div v-else class="grid grid-cols-2 gap-2 sm:grid-cols-3 lg:grid-cols-4">
              <div
                v-for="ws in workstationsByLine[line.id]"
                :key="ws.id"
                class="flex items-center justify-between bg-background-100 dark:bg-background-700 border border-background-300 dark:border-background-600 rounded-lg px-3 py-2"
              >
                <div class="flex items-center gap-2">
                  <span class="material-symbols-rounded text-background-400 text-base">precision_manufacturing</span>
                  <div>
                    <div class="text-xs font-medium text-background-800 dark:text-background-100">
                      {{ ws.type ?? t('productionLines.unknownType') }}
                    </div>
                    <div class="text-xs text-background-400">ID #{{ ws.id }}</div>
                  </div>
                </div>
                <button
                  @click="deleteWorkstation(ws, line.id)"
                  class="text-background-300 hover:text-danger-500 transition-colors"
                >
                  <span class="material-symbols-rounded text-sm">close</span>
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Modal: Criar Linha -->
    <div v-if="showLineModal" class="fixed inset-0 bg-black/40 flex items-center justify-center z-50" @click.self="showLineModal = false">
      <div class="bg-background-50 dark:bg-background-800 border border-background-300 dark:border-background-700 rounded-xl w-full max-w-md overflow-hidden">
        <div class="flex items-center justify-between px-5 py-4 border-b border-background-300 dark:border-background-700">
          <h2 class="text-base font-medium text-background-900 dark:text-background-50">{{ t('productionLines.newLine') }}</h2>
          <button @click="showLineModal = false" class="text-background-500 hover:text-background-700"><span class="material-symbols-rounded">close</span></button>
        </div>
        <div class="px-5 py-4 flex flex-col gap-4">
          <div class="flex flex-col gap-1.5">
            <label class="text-xs font-medium text-background-600 dark:text-background-400">{{ t('productionLines.fields.name') }} *</label>
            <input v-model="lineForm.name" type="text" :placeholder="t('productionLines.fields.namePlaceholder')" />
          </div>
          <div class="flex flex-col gap-1.5">
            <label class="text-xs font-medium text-background-600 dark:text-background-400">{{ t('productionLines.fields.location') }}</label>
            <input v-model="lineForm.location" type="text" :placeholder="t('productionLines.fields.locationPlaceholder')" />
          </div>
          <div class="flex flex-col gap-1.5">
            <label class="text-xs font-medium text-background-600 dark:text-background-400">{{ t('productionLines.fields.capacity') }}</label>
            <input v-model.number="lineForm.capacity" type="number" min="1" placeholder="Ex: 10" />
          </div>
        </div>
        <div class="flex justify-end gap-2 px-5 py-4 border-t border-background-300 dark:border-background-700">
          <button @click="showLineModal = false" class="px-4 py-2 text-sm rounded-lg border border-background-300 dark:border-background-600 text-background-700 dark:text-background-300 hover:bg-background-100 dark:hover:bg-background-700 transition-colors">{{ t('common.cancel') }}</button>
          <button @click="submitCreateLine" :disabled="!lineForm.name" class="px-4 py-2 text-sm rounded-lg bg-primary-500 hover:bg-primary-600 disabled:opacity-50 disabled:cursor-not-allowed text-white font-medium transition-colors">{{ t('common.save') }}</button>
        </div>
      </div>
    </div>

    <!-- Modal: Editar Status -->
    <div v-if="showStatusModal" class="fixed inset-0 bg-black/40 flex items-center justify-center z-50" @click.self="showStatusModal = false">
      <div class="bg-background-50 dark:bg-background-800 border border-background-300 dark:border-background-700 rounded-xl w-full max-w-xs overflow-hidden">
        <div class="flex items-center justify-between px-5 py-4 border-b border-background-300 dark:border-background-700">
          <h2 class="text-base font-medium text-background-900 dark:text-background-50">{{ t('productionLines.editStatus') }}</h2>
          <button @click="showStatusModal = false" class="text-background-500 hover:text-background-700"><span class="material-symbols-rounded">close</span></button>
        </div>
        <div class="px-5 py-4">
          <select v-model="statusForm.status" class="w-full">
            <option value="functional">{{ t('productionLines.status.functional') }}</option>
            <option value="maintenance">{{ t('productionLines.status.maintenance') }}</option>
            <option value="broken">{{ t('productionLines.status.broken') }}</option>
            <option value="inactive">{{ t('productionLines.status.inactive') }}</option>
          </select>
        </div>
        <div class="flex justify-end gap-2 px-5 py-4 border-t border-background-300 dark:border-background-700">
          <button @click="showStatusModal = false" class="px-4 py-2 text-sm rounded-lg border border-background-300 dark:border-background-600 text-background-700 dark:text-background-300 hover:bg-background-100 dark:hover:bg-background-700 transition-colors">{{ t('common.cancel') }}</button>
          <button @click="submitEditStatus" class="px-4 py-2 text-sm rounded-lg bg-primary-500 hover:bg-primary-600 text-white font-medium transition-colors">{{ t('common.save') }}</button>
        </div>
      </div>
    </div>

    <!-- Modal: Criar Workstation -->
    <div v-if="showWorkstationModal" class="fixed inset-0 bg-black/40 flex items-center justify-center z-50" @click.self="showWorkstationModal = false">
      <div class="bg-background-50 dark:bg-background-800 border border-background-300 dark:border-background-700 rounded-xl w-full max-w-sm overflow-hidden">
        <div class="flex items-center justify-between px-5 py-4 border-b border-background-300 dark:border-background-700">
          <h2 class="text-base font-medium text-background-900 dark:text-background-50">{{ t('productionLines.addWorkstation') }}</h2>
          <button @click="showWorkstationModal = false" class="text-background-500 hover:text-background-700"><span class="material-symbols-rounded">close</span></button>
        </div>
        <div class="px-5 py-4">
          <div class="flex flex-col gap-1.5">
            <label class="text-xs font-medium text-background-600 dark:text-background-400">{{ t('productionLines.fields.wsType') }}</label>
            <input v-model="wsForm.type" type="text" :placeholder="t('productionLines.fields.wsTypePlaceholder')" />
          </div>
        </div>
        <div class="flex justify-end gap-2 px-5 py-4 border-t border-background-300 dark:border-background-700">
          <button @click="showWorkstationModal = false" class="px-4 py-2 text-sm rounded-lg border border-background-300 dark:border-background-600 text-background-700 dark:text-background-300 hover:bg-background-100 dark:hover:bg-background-700 transition-colors">{{ t('common.cancel') }}</button>
          <button @click="submitCreateWorkstation" class="px-4 py-2 text-sm rounded-lg bg-primary-500 hover:bg-primary-600 text-white font-medium transition-colors">{{ t('common.save') }}</button>
        </div>
      </div>
    </div>

  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { productionLineService, workstationService } from '@/services/productionLineService'
import type { ProductionLine, Workstation } from '@/services/productionLineService'
import { toast } from '@/plugins/toast'
import { useI18n } from 'vue-i18n'
import { EntityStatus } from '@/constants/status'

const { t } = useI18n()

const loading = ref(true)
const lines = ref<ProductionLine[]>([])
const workstationsByLine = reactive<Record<number, Workstation[]>>({})
const loadingWorkstations = reactive<Record<number, boolean>>({})
const expandedLineId = ref<number | null>(null)

const showLineModal = ref(false)
const showWorkstationModal = ref(false)
const showStatusModal = ref(false)

const lineForm = reactive({ name: '', location: '', capacity: undefined as number | undefined })
const wsForm = reactive({ type: '', lineId: 0 })
const statusForm = reactive<{ lineId: number, status: string }>({ lineId: 0, status: EntityStatus.Functional })

onMounted(async () => {
  await loadLines()
})

async function loadLines() {
  loading.value = true
  try {
    const res = await productionLineService.getAll()
    lines.value = res.data
  } catch {
    toast.error(t('errors.loadFailed'))
  } finally {
    loading.value = false
  }
}

async function loadWorkstations(lineId: number) {
  loadingWorkstations[lineId] = true
  try {
    const res = await workstationService.getByLine(lineId)
    workstationsByLine[lineId] = res.data
  } catch {
    workstationsByLine[lineId] = []
  } finally {
    loadingWorkstations[lineId] = false
  }
}

async function toggleLine(lineId: number) {
  if (expandedLineId.value === lineId) {
    expandedLineId.value = null
    return
  }
  expandedLineId.value = lineId
  if (!workstationsByLine[lineId]) {
    await loadWorkstations(lineId)
  }
}

function openCreateLine() {
  lineForm.name = ''
  lineForm.location = ''
  lineForm.capacity = undefined
  showLineModal.value = true
}

async function submitCreateLine() {
  try {
    await productionLineService.create({
      name: lineForm.name,
      location: lineForm.location || undefined,
      status: EntityStatus.Functional,
      capacity: lineForm.capacity,
    })
    showLineModal.value = false
    toast.success(t('productionLines.lineCreated'))
    await loadLines()
  } catch {
    toast.error(t('errors.saveFailed'))
  }
}

async function deleteLine(line: ProductionLine) {
  try {
    await productionLineService.delete(line.id)
    toast.success(t('productionLines.lineDeleted'))
    await loadLines()
  } catch {
    toast.error(t('errors.deleteFailed'))
  }
}

function openEditStatus(line: ProductionLine) {
  statusForm.lineId = line.id
  statusForm.status = line.status ?? EntityStatus.Functional
  showStatusModal.value = true
}

async function submitEditStatus() {
  try {
    await productionLineService.update(statusForm.lineId, { status: statusForm.status })
    showStatusModal.value = false
    toast.success(t('productionLines.statusUpdated'))
    await loadLines()
  } catch {
    toast.error(t('errors.saveFailed'))
  }
}

function openCreateWorkstation(lineId: number) {
  wsForm.type = ''
  wsForm.lineId = lineId
  showWorkstationModal.value = true
}

async function submitCreateWorkstation() {
  try {
    await workstationService.create({
      productionLineId: wsForm.lineId,
      type: wsForm.type || undefined,
    })
    showWorkstationModal.value = false
    toast.success(t('productionLines.workstationCreated'))
    await loadWorkstations(wsForm.lineId)
  } catch {
    toast.error(t('errors.saveFailed'))
  }
}

async function deleteWorkstation(ws: Workstation, lineId: number) {
  try {
    await workstationService.delete(ws.id)
    toast.success(t('productionLines.workstationDeleted'))
    await loadWorkstations(lineId)
  } catch {
    toast.error(t('errors.deleteFailed'))
  }
}

function lineStatusClass(status: string) {
  switch (status) {
    case EntityStatus.Functional: return 'bg-success-100 text-success-700'
    case EntityStatus.Maintenance: return 'bg-warning-100 text-warning-700'
    case EntityStatus.Broken: return 'bg-danger-100 text-danger-700'
    case EntityStatus.Inactive: return 'bg-background-200 text-background-500'
    default: return 'bg-background-200 text-background-600'
  }
}
</script>