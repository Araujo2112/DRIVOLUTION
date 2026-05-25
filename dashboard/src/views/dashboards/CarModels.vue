<template>
  <div class="p-8 w-full">

    <!-- Header -->
    <div class="flex items-start justify-between mb-8">
      <div>
        <h1 class="text-2xl font-medium text-background-900 dark:text-background-50">
          {{ t('carModels.title') }}
        </h1>
        <p class="text-sm text-background-600 dark:text-background-400 mt-1">
          {{ t('carModels.subtitle') }}
        </p>
      </div>
      <button
        @click="openCreateModel"
        class="flex items-center gap-2 bg-primary-500 hover:bg-primary-600 text-white text-sm font-medium px-4 py-2 rounded-lg transition-colors"
      >
        <span class="material-symbols-rounded text-base">add</span>
        {{ t('carModels.newModel') }}
      </button>
    </div>

    <!-- Loading -->
    <div v-if="loading" class="flex items-center gap-2 text-background-500 text-sm py-12">
      <span class="material-symbols-rounded animate-spin text-lg">autorenew</span>
      {{ t('common.loading') }}
    </div>

    <!-- Lista de modelos -->
    <div v-else class="flex flex-col gap-4">

      <div v-if="models.length === 0" class="text-center py-16 text-background-500">
        <span class="material-symbols-rounded text-5xl block mb-3">directions_car_off</span>
        <p class="text-sm">{{ t('carModels.empty') }}</p>
      </div>

      <div
        v-for="model in models"
        :key="model.id"
        class="bg-background-50 dark:bg-background-800 border border-background-300 dark:border-background-700 rounded-xl overflow-hidden"
      >
        <!-- Card Header -->
        <div class="flex items-center justify-between px-5 py-4 border-b border-background-300 dark:border-background-700">
          <div class="flex items-center gap-3">
            <div class="w-10 h-10 rounded-lg bg-primary-50 dark:bg-primary-950 flex items-center justify-center">
              <span class="material-symbols-rounded text-primary-500 text-xl">directions_car</span>
            </div>
            <div>
              <div class="text-sm font-medium text-background-900 dark:text-background-50">{{ model.name }}</div>
              <div class="flex gap-2 mt-1">
                <span v-if="model.version" class="text-xs px-2 py-0.5 rounded-full bg-primary-50 dark:bg-primary-950 text-primary-500 border border-primary-200 dark:border-primary-800">
                  {{ model.version }}
                </span>
                <span v-if="model.type" class="text-xs px-2 py-0.5 rounded-full bg-background-200 dark:bg-background-700 text-background-600 dark:text-background-400 border border-background-300 dark:border-background-600">
                  {{ model.type }}
                </span>
              </div>
            </div>
          </div>
          <div class="flex items-center gap-2">
            <button
              @click="toggleModel(model.id)"
              class="p-2 rounded-lg text-background-500 hover:text-background-700 hover:bg-background-100 dark:hover:bg-background-700 transition-colors"
            >
              <span class="material-symbols-rounded text-lg transition-transform duration-200" :class="expandedModelId === model.id ? 'rotate-180' : ''">
                expand_more
              </span>
            </button>
            <button
              @click="deleteModel(model)"
              class="p-2 rounded-lg text-background-500 hover:text-danger-500 hover:bg-danger-100 dark:hover:bg-background-700 transition-colors"
              :title="t('common.delete')"
            >
              <span class="material-symbols-rounded text-lg">delete</span>
            </button>
          </div>
        </div>

        <!-- Expanded Content -->
        <div v-if="expandedModelId === model.id" class="px-5 py-4 flex flex-col gap-6">

          <!-- ── Configurações ───────────────────────────────────────────── -->
          <div>
            <div class="flex items-center justify-between mb-3">
              <span class="text-xs font-medium text-background-500 uppercase tracking-wider">
                {{ t('carModels.configurations') }}
              </span>
              <button
                @click="openCreateConfig(model.id)"
                class="flex items-center gap-1 text-xs text-primary-500 hover:bg-primary-50 dark:hover:bg-primary-950 px-2 py-1 rounded-md transition-colors"
              >
                <span class="material-symbols-rounded text-sm">add</span>
                {{ t('carModels.addConfig') }}
              </button>
            </div>

            <p v-if="!configsByModel[model.id]?.length" class="text-xs text-background-500 py-1">
              {{ t('carModels.noConfigs') }}
            </p>

            <div v-else class="flex flex-col gap-2">
              <div
                v-for="config in configsByModel[model.id]"
                :key="config.id"
                class="bg-background-100 dark:bg-background-700 border border-background-300 dark:border-background-600 rounded-lg px-4 py-3"
              >
                <div class="flex items-center justify-between mb-2">
                  <div class="flex items-center gap-2 text-sm font-medium text-background-800 dark:text-background-100">
                    <span class="material-symbols-rounded text-background-500 text-base">settings</span>
                    {{ config.item }}
                  </div>
                  <div class="flex items-center gap-2">
                    <button
                      @click="openCreateOption(config.id)"
                      class="flex items-center gap-1 text-xs text-primary-500 hover:bg-primary-50 dark:hover:bg-primary-950 px-2 py-1 rounded-md transition-colors"
                    >
                      <span class="material-symbols-rounded text-sm">add</span>
                      {{ t('carModels.addOption') }}
                    </button>
                    <button
                      @click="deleteConfig(config)"
                      class="p-1 rounded text-background-400 hover:text-danger-500 hover:bg-danger-100 dark:hover:bg-background-600 transition-colors"
                    >
                      <span class="material-symbols-rounded text-base">delete</span>
                    </button>
                  </div>
                </div>

                <div v-if="optionsByConfig[config.id]?.length" class="flex flex-wrap gap-2">
                  <div
                    v-for="option in optionsByConfig[config.id]"
                    :key="option.id"
                    class="flex items-center gap-1.5 text-xs px-3 py-1 rounded-full border transition-colors"
                    :class="option.isDefault
                      ? 'bg-primary-50 dark:bg-primary-950 border-primary-200 dark:border-primary-800 text-primary-600 dark:text-primary-300'
                      : 'bg-background-50 dark:bg-background-800 border-background-300 dark:border-background-600 text-background-700 dark:text-background-300'"
                  >
                    {{ option.value }}
                    <span v-if="option.isDefault" class="text-xs bg-primary-100 dark:bg-primary-900 text-primary-500 px-1.5 py-0.5 rounded-full">
                      {{ t('carModels.default') }}
                    </span>
                    <button @click="deleteOption(option)" class="text-background-400 hover:text-danger-500 transition-colors ml-1">
                      <span class="material-symbols-rounded text-xs">close</span>
                    </button>
                  </div>
                </div>
                <p v-else class="text-xs text-background-400 mt-1">{{ t('carModels.noOptions') }}</p>
              </div>
            </div>
          </div>

          <!-- ── Sequência de Fases ──────────────────────────────────────── -->
          <div>
            <div class="flex items-center justify-between mb-3">
              <span class="text-xs font-medium text-background-500 uppercase tracking-wider">
                {{ t('carModels.phaseSequence') }}
              </span>
              <button
                @click="openAddPhase(model.id)"
                class="flex items-center gap-1 text-xs text-primary-500 hover:bg-primary-50 dark:hover:bg-primary-950 px-2 py-1 rounded-md transition-colors"
              >
                <span class="material-symbols-rounded text-sm">add</span>
                {{ t('carModels.addPhase') }}
              </button>
            </div>

            <!-- Loading fases -->
            <div v-if="sequenceLoading[model.id]" class="flex items-center gap-2 text-background-500 text-xs py-2">
              <span class="material-symbols-rounded animate-spin text-sm">autorenew</span>
              {{ t('common.loading') }}
            </div>

            <p v-else-if="!sequenceByModel[model.id]?.length" class="text-xs text-background-500 py-1">
              {{ t('carModels.noPhases') }}
            </p>

            <div v-else class="flex flex-col gap-1.5">
              <div
                v-for="(seq, index) in sequenceByModel[model.id]"
                :key="seq.id"
                class="flex items-center gap-3 bg-background-100 dark:bg-background-700 border border-background-300 dark:border-background-600 rounded-lg px-4 py-2.5"
              >
                <!-- Número de ordem -->
                <span class="text-xs font-mono font-semibold text-background-400 dark:text-background-500 w-5 text-center">
                  {{ seq.order }}
                </span>

                <!-- Ícone + Nome -->
                <span class="material-symbols-rounded text-background-400 text-base">precision_manufacturing</span>
                <span class="text-sm font-medium text-background-800 dark:text-background-100 flex-1">
                  {{ seq.phaseName }}
                </span>

                <!-- Setas + Remover -->
                <div class="flex items-center gap-1">
                  <button
                    @click="movePhase(model.id, index, 'up')"
                    :disabled="index === 0 || sequenceMoving[model.id]"
                    class="p-1 rounded text-background-400 hover:text-primary-500 hover:bg-primary-50 dark:hover:bg-background-600 disabled:opacity-30 disabled:cursor-not-allowed transition-colors"
                    :title="t('common.moveUp')"
                  >
                    <span class="material-symbols-rounded text-base">arrow_upward</span>
                  </button>
                  <button
                    @click="movePhase(model.id, index, 'down')"
                    :disabled="index === sequenceByModel[model.id].length - 1 || sequenceMoving[model.id]"
                    class="p-1 rounded text-background-400 hover:text-primary-500 hover:bg-primary-50 dark:hover:bg-background-600 disabled:opacity-30 disabled:cursor-not-allowed transition-colors"
                    :title="t('common.moveDown')"
                  >
                    <span class="material-symbols-rounded text-base">arrow_downward</span>
                  </button>
                  <button
                    @click="removePhaseFromSequence(seq)"
                    :disabled="sequenceMoving[model.id]"
                    class="p-1 rounded text-background-400 hover:text-danger-500 hover:bg-danger-100 dark:hover:bg-background-600 disabled:opacity-30 disabled:cursor-not-allowed transition-colors"
                    :title="t('common.delete')"
                  >
                    <span class="material-symbols-rounded text-base">delete</span>
                  </button>
                </div>
              </div>
            </div>
          </div>

        </div>
      </div>
    </div>

    <!-- Modal: Criar Modelo -->
    <div v-if="showModelModal" class="fixed inset-0 bg-black/40 flex items-center justify-center z-50" @click.self="showModelModal = false">
      <div class="bg-background-50 dark:bg-background-800 border border-background-300 dark:border-background-700 rounded-xl w-full max-w-md overflow-hidden">
        <div class="flex items-center justify-between px-5 py-4 border-b border-background-300 dark:border-background-700">
          <h2 class="text-base font-medium text-background-900 dark:text-background-50">{{ t('carModels.newModel') }}</h2>
          <button @click="showModelModal = false" class="text-background-500 hover:text-background-700 dark:hover:text-background-300">
            <span class="material-symbols-rounded">close</span>
          </button>
        </div>
        <div class="px-5 py-4 flex flex-col gap-4">
          <div class="flex flex-col gap-1.5">
            <label class="text-xs font-medium text-background-600 dark:text-background-400">{{ t('carModels.fields.name') }} *</label>
            <input v-model="modelForm.name" type="text" :placeholder="t('carModels.fields.namePlaceholder')" />
          </div>
          <div class="flex flex-col gap-1.5">
            <label class="text-xs font-medium text-background-600 dark:text-background-400">{{ t('carModels.fields.version') }}</label>
            <input v-model="modelForm.version" type="text" :placeholder="t('carModels.fields.versionPlaceholder')" />
          </div>
          <div class="flex flex-col gap-1.5">
            <label class="text-xs font-medium text-background-600 dark:text-background-400">{{ t('carModels.fields.type') }}</label>
            <input v-model="modelForm.type" type="text" :placeholder="t('carModels.fields.typePlaceholder')" />
          </div>
        </div>
        <div class="flex justify-end gap-2 px-5 py-4 border-t border-background-300 dark:border-background-700">
          <button @click="showModelModal = false" class="px-4 py-2 text-sm rounded-lg border border-background-300 dark:border-background-600 text-background-700 dark:text-background-300 hover:bg-background-100 dark:hover:bg-background-700 transition-colors">
            {{ t('common.cancel') }}
          </button>
          <button @click="submitCreateModel" :disabled="!modelForm.name" class="px-4 py-2 text-sm rounded-lg bg-primary-500 hover:bg-primary-600 disabled:opacity-50 disabled:cursor-not-allowed text-white font-medium transition-colors">
            {{ t('common.save') }}
          </button>
        </div>
      </div>
    </div>

    <!-- Modal: Criar Config -->
    <div v-if="showConfigModal" class="fixed inset-0 bg-black/40 flex items-center justify-center z-50" @click.self="showConfigModal = false">
      <div class="bg-background-50 dark:bg-background-800 border border-background-300 dark:border-background-700 rounded-xl w-full max-w-md overflow-hidden">
        <div class="flex items-center justify-between px-5 py-4 border-b border-background-300 dark:border-background-700">
          <h2 class="text-base font-medium text-background-900 dark:text-background-50">{{ t('carModels.addConfig') }}</h2>
          <button @click="showConfigModal = false" class="text-background-500 hover:text-background-700 dark:hover:text-background-300">
            <span class="material-symbols-rounded">close</span>
          </button>
        </div>
        <div class="px-5 py-4">
          <div class="flex flex-col gap-1.5">
            <label class="text-xs font-medium text-background-600 dark:text-background-400">{{ t('carModels.fields.configName') }} *</label>
            <input v-model="configForm.item" type="text" :placeholder="t('carModels.fields.configNamePlaceholder')" />
          </div>
        </div>
        <div class="flex justify-end gap-2 px-5 py-4 border-t border-background-300 dark:border-background-700">
          <button @click="showConfigModal = false" class="px-4 py-2 text-sm rounded-lg border border-background-300 dark:border-background-600 text-background-700 dark:text-background-300 hover:bg-background-100 dark:hover:bg-background-700 transition-colors">
            {{ t('common.cancel') }}
          </button>
          <button @click="submitCreateConfig" :disabled="!configForm.item" class="px-4 py-2 text-sm rounded-lg bg-primary-500 hover:bg-primary-600 disabled:opacity-50 disabled:cursor-not-allowed text-white font-medium transition-colors">
            {{ t('common.save') }}
          </button>
        </div>
      </div>
    </div>

    <!-- Modal: Criar Opção -->
    <div v-if="showOptionModal" class="fixed inset-0 bg-black/40 flex items-center justify-center z-50" @click.self="showOptionModal = false">
      <div class="bg-background-50 dark:bg-background-800 border border-background-300 dark:border-background-700 rounded-xl w-full max-w-md overflow-hidden">
        <div class="flex items-center justify-between px-5 py-4 border-b border-background-300 dark:border-background-700">
          <h2 class="text-base font-medium text-background-900 dark:text-background-50">{{ t('carModels.addOption') }}</h2>
          <button @click="showOptionModal = false" class="text-background-500 hover:text-background-700 dark:hover:text-background-300">
            <span class="material-symbols-rounded">close</span>
          </button>
        </div>
        <div class="px-5 py-4 flex flex-col gap-4">
          <div class="flex flex-col gap-1.5">
            <label class="text-xs font-medium text-background-600 dark:text-background-400">{{ t('carModels.fields.optionValue') }} *</label>
            <input v-model="optionForm.value" type="text" :placeholder="t('carModels.fields.optionValuePlaceholder')" />
          </div>
          <div class="flex items-center gap-2">
            <input type="checkbox" id="isDefault" v-model="optionForm.isDefault" :disabled="currentConfigHasDefault" class="w-4 h-4 accent-primary-500" />
            <label for="isDefault" class="text-sm text-background-700 dark:text-background-300">
              {{ t('carModels.fields.isDefault') }}
              <span v-if="currentConfigHasDefault" class="text-xs text-background-400 ml-1">({{ t('carModels.defaultExists') }})</span>
            </label>
          </div>
        </div>
        <div class="flex justify-end gap-2 px-5 py-4 border-t border-background-300 dark:border-background-700">
          <button @click="showOptionModal = false" class="px-4 py-2 text-sm rounded-lg border border-background-300 dark:border-background-600 text-background-700 dark:text-background-300 hover:bg-background-100 dark:hover:bg-background-700 transition-colors">
            {{ t('common.cancel') }}
          </button>
          <button @click="submitCreateOption" :disabled="!optionForm.value" class="px-4 py-2 text-sm rounded-lg bg-primary-500 hover:bg-primary-600 disabled:opacity-50 disabled:cursor-not-allowed text-white font-medium transition-colors">
            {{ t('common.save') }}
          </button>
        </div>
      </div>
    </div>

    <!-- Modal: Adicionar Fase à Sequência -->
    <div v-if="showPhaseModal" class="fixed inset-0 bg-black/40 flex items-center justify-center z-50" @click.self="showPhaseModal = false">
      <div class="bg-background-50 dark:bg-background-800 border border-background-300 dark:border-background-700 rounded-xl w-full max-w-md overflow-hidden">
        <div class="flex items-center justify-between px-5 py-4 border-b border-background-300 dark:border-background-700">
          <h2 class="text-base font-medium text-background-900 dark:text-background-50">{{ t('carModels.addPhase') }}</h2>
          <button @click="showPhaseModal = false" class="text-background-500 hover:text-background-700 dark:hover:text-background-300">
            <span class="material-symbols-rounded">close</span>
          </button>
        </div>
        <div class="px-5 py-4 flex flex-col gap-4">
          <div class="flex flex-col gap-1.5">
            <label class="text-xs font-medium text-background-600 dark:text-background-400">{{ t('carModels.fields.phase') }} *</label>
            <select v-model="phaseForm.manufacturingPhaseId">
              <option :value="0" disabled>{{ t('carModels.fields.phasePlaceholder') }}</option>
              <option
                v-for="phase in availablePhasesForModel"
                :key="phase.id"
                :value="phase.id"
              >
                {{ phase.name }}
              </option>
            </select>
            <p v-if="availablePhasesForModel.length === 0" class="text-xs text-background-400">
              {{ t('carModels.allPhasesAdded') }}
            </p>
          </div>
        </div>
        <div class="flex justify-end gap-2 px-5 py-4 border-t border-background-300 dark:border-background-700">
          <button @click="showPhaseModal = false" class="px-4 py-2 text-sm rounded-lg border border-background-300 dark:border-background-600 text-background-700 dark:text-background-300 hover:bg-background-100 dark:hover:bg-background-700 transition-colors">
            {{ t('common.cancel') }}
          </button>
          <button @click="submitAddPhase" :disabled="!phaseForm.manufacturingPhaseId" class="px-4 py-2 text-sm rounded-lg bg-primary-500 hover:bg-primary-600 disabled:opacity-50 disabled:cursor-not-allowed text-white font-medium transition-colors">
            {{ t('common.save') }}
          </button>
        </div>
      </div>
    </div>

  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, reactive, computed } from 'vue'
import { carModelService, configService, configOptionService } from '@/services/carModelService'
import type { CarModel, Config, ConfigOption } from '@/services/carModelService'
import { manufacturingPhaseService, phaseSequenceService } from '@/services/manufacturingPhaseService'
import type { ManufacturingPhase, PhaseSequence } from '@/services/manufacturingPhaseService'
import { toast } from '@/plugins/toast'
import { useI18n } from 'vue-i18n'

const { t } = useI18n()

// ── Estado base ────────────────────────────────────────────────────────────────
const loading = ref(true)
const models = ref<CarModel[]>([])
const configsByModel = reactive<Record<number, Config[]>>({})
const optionsByConfig = reactive<Record<number, ConfigOption[]>>({})
const expandedModelId = ref<number | null>(null)

// ── Estado de fases ────────────────────────────────────────────────────────────
const allPhases = ref<ManufacturingPhase[]>([])
const sequenceByModel = reactive<Record<number, PhaseSequence[]>>({})
const sequenceLoading = reactive<Record<number, boolean>>({})
const sequenceMoving = reactive<Record<number, boolean>>({})

// ── Modais ─────────────────────────────────────────────────────────────────────
const showModelModal = ref(false)
const showConfigModal = ref(false)
const showOptionModal = ref(false)
const showPhaseModal = ref(false)

// ── Forms ──────────────────────────────────────────────────────────────────────
const modelForm = reactive({ name: '', version: '', type: '' })
const configForm = reactive({ item: '', modelId: 0 })
const optionForm = reactive({ value: '', isDefault: false, configId: 0 })
const phaseForm = reactive({ manufacturingPhaseId: 0, modelId: 0 })

// ── Computed ───────────────────────────────────────────────────────────────────
const currentConfigHasDefault = computed(() => {
  const options = optionsByConfig[optionForm.configId] || []
  return options.some(o => o.isDefault)
})

// Fases ainda não adicionadas ao modelo atual (evita duplicados)
const availablePhasesForModel = computed(() => {
  const alreadyAdded = new Set(
    (sequenceByModel[phaseForm.modelId] || []).map(s => s.manufacturingPhaseId)
  )
  return allPhases.value.filter(p => !alreadyAdded.has(p.id))
})

// ── Lifecycle ──────────────────────────────────────────────────────────────────
onMounted(async () => {
  await Promise.all([loadModels(), loadAllPhases()])
})

// ── Loaders ────────────────────────────────────────────────────────────────────
async function loadModels() {
  loading.value = true
  try {
    const res = await carModelService.getAll()
    models.value = res.data
    for (const model of models.value) {
      await loadConfigs(model.id)
    }
    await loadAllOptions()
  } catch {
    toast.error(t('errors.loadFailed'))
  } finally {
    loading.value = false
  }
}

async function loadAllPhases() {
  try {
    const res = await manufacturingPhaseService.getAll()
    const raw = res.data as any
    allPhases.value = raw?.$values ?? res.data ?? []
  } catch {
    allPhases.value = []
  }
}

async function loadSequence(modelId: number) {
  sequenceLoading[modelId] = true
  try {
    const res = await phaseSequenceService.getByModel(modelId)
    const raw = res.data as any
    const data: PhaseSequence[] = raw?.$values ?? res.data ?? []
    sequenceByModel[modelId] = data.slice().sort((a, b) => a.order - b.order)
  } catch {
    sequenceByModel[modelId] = []
  } finally {
    sequenceLoading[modelId] = false
  }
}

async function loadConfigs(modelId: number) {
  try {
    const res = await carModelService.getConfigs(modelId)
    configsByModel[modelId] = res.data
  } catch {
    configsByModel[modelId] = []
  }
}

async function loadAllOptions() {
  try {
    const res = await configOptionService.getAll()
    Object.keys(optionsByConfig).forEach(key => delete optionsByConfig[Number(key)])
    for (const option of res.data) {
      if (!optionsByConfig[option.configId]) optionsByConfig[option.configId] = []
      optionsByConfig[option.configId].push(option)
    }
  } catch {}
}

// ── Toggle accordion ───────────────────────────────────────────────────────────
function toggleModel(modelId: number) {
  if (expandedModelId.value === modelId) {
    expandedModelId.value = null
  } else {
    expandedModelId.value = modelId
    // Carrega sequência só quando expande, se ainda não carregou
    if (!sequenceByModel[modelId]) {
      loadSequence(modelId)
    }
  }
}

// ── Modelo ─────────────────────────────────────────────────────────────────────
function openCreateModel() {
  modelForm.name = ''
  modelForm.version = ''
  modelForm.type = ''
  showModelModal.value = true
}

async function submitCreateModel() {
  try {
    await carModelService.create({
      name: modelForm.name,
      version: modelForm.version || undefined,
      type: modelForm.type || undefined,
    })
    showModelModal.value = false
    toast.success(t('carModels.modelCreated'))
    await loadModels()
  } catch {
    toast.error(t('errors.saveFailed'))
  }
}

async function deleteModel(model: CarModel) {
  try {
    await carModelService.delete(model.id)
    toast.success(t('carModels.modelDeleted'))
    await loadModels()
  } catch {
    toast.error(t('errors.deleteFailed'))
  }
}

// ── Config ─────────────────────────────────────────────────────────────────────
function openCreateConfig(modelId: number) {
  configForm.item = ''
  configForm.modelId = modelId
  showConfigModal.value = true
}

async function submitCreateConfig() {
  try {
    const res = await configService.create({ modelId: configForm.modelId, item: configForm.item })
    showConfigModal.value = false
    toast.success(t('carModels.configCreated'))
    optionsByConfig[res.data.id] = []
    await loadConfigs(configForm.modelId)
  } catch {
    toast.error(t('errors.saveFailed'))
  }
}

async function deleteConfig(config: Config) {
  try {
    await configService.delete(config.id)
    toast.success(t('carModels.configDeleted'))
    await loadConfigs(config.modelId)
  } catch {
    toast.error(t('errors.deleteFailed'))
  }
}

// ── Opção ──────────────────────────────────────────────────────────────────────
function openCreateOption(configId: number) {
  optionForm.value = ''
  optionForm.isDefault = false
  optionForm.configId = configId
  showOptionModal.value = true
}

async function submitCreateOption() {
  try {
    await configOptionService.create({
      configId: optionForm.configId,
      value: optionForm.value,
      isDefault: optionForm.isDefault,
    })
    showOptionModal.value = false
    toast.success(t('carModels.optionCreated'))
    await loadAllOptions()
  } catch {
    toast.error(t('errors.saveFailed'))
  }
}

async function deleteOption(option: ConfigOption) {
  try {
    await configOptionService.delete(option.id)
    toast.success(t('carModels.optionDeleted'))
    await loadAllOptions()
  } catch {
    toast.error(t('errors.deleteFailed'))
  }
}

// ── Sequência de Fases ─────────────────────────────────────────────────────────
function openAddPhase(modelId: number) {
  phaseForm.manufacturingPhaseId = 0
  phaseForm.modelId = modelId
  showPhaseModal.value = true
}

async function submitAddPhase() {
  const seq = sequenceByModel[phaseForm.modelId] || []
  const nextOrder = seq.length > 0 ? Math.max(...seq.map(s => s.order)) + 1 : 1
  try {
    await phaseSequenceService.create({
      order: nextOrder,
      manufacturingPhaseId: phaseForm.manufacturingPhaseId,
      modelId: phaseForm.modelId,
    })
    showPhaseModal.value = false
    toast.success(t('carModels.phaseAdded'))
    await loadSequence(phaseForm.modelId)
  } catch {
    toast.error(t('errors.saveFailed'))
  }
}

async function removePhaseFromSequence(seq: PhaseSequence) {
  try {
    await phaseSequenceService.delete(seq.id)
    toast.success(t('carModels.phaseRemoved'))
    await loadSequence(seq.modelId)
  } catch {
    toast.error(t('errors.deleteFailed'))
  }
}

async function movePhase(modelId: number, index: number, direction: 'up' | 'down') {
  const seq = sequenceByModel[modelId]
  const swapIndex = direction === 'up' ? index - 1 : index + 1
  const current = seq[index]
  const target  = seq[swapIndex]

  sequenceMoving[modelId] = true
  try {
    // 1. Apagar os dois registos
    await phaseSequenceService.delete(current.id)
    await phaseSequenceService.delete(target.id)

    // 2. Recriar com orders trocadas
    await phaseSequenceService.create({
      order: target.order,
      manufacturingPhaseId: current.manufacturingPhaseId,
      modelId,
    })
    await phaseSequenceService.create({
      order: current.order,
      manufacturingPhaseId: target.manufacturingPhaseId,
      modelId,
    })

    await loadSequence(modelId)
  } catch {
    toast.error(t('errors.saveFailed'))
    await loadSequence(modelId) // recarrega para ficar consistente
  } finally {
    sequenceMoving[modelId] = false
  }
}
</script>