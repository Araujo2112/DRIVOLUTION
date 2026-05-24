<template>
  <div class="p-8 w-full">

    <!-- Header -->
    <div class="flex items-start justify-between mb-8">
      <div>
        <h1 class="text-2xl font-medium text-background-900 dark:text-background-50">
          {{ t('phases.title') }}
        </h1>
        <p class="text-sm text-background-600 dark:text-background-400 mt-1">
          {{ t('phases.subtitle') }}
        </p>
      </div>
      <button
        @click="openCreateModal"
        class="flex items-center gap-2 bg-primary-500 hover:bg-primary-600 text-white text-sm font-medium px-4 py-2 rounded-lg transition-colors"
      >
        <span class="material-symbols-rounded text-base">add</span>
        {{ t('phases.newPhase') }}
      </button>
    </div>

    <!-- Loading -->
    <div v-if="loading" class="flex items-center gap-2 text-background-500 text-sm py-12">
      <span class="material-symbols-rounded animate-spin text-lg">autorenew</span>
      {{ t('common.loading') }}
    </div>

    <div v-else>
      <!-- Empty -->
      <div v-if="phases.length === 0" class="text-center py-16 text-background-500">
        <span class="material-symbols-rounded text-5xl block mb-3">precision_manufacturing</span>
        <p class="text-sm">{{ t('phases.empty') }}</p>
      </div>

      <!-- Tabela -->
      <div v-else class="border border-background-300 dark:border-background-700 rounded-xl overflow-hidden">
        <div class="grid grid-cols-6 px-4 py-3 bg-background-100 dark:bg-background-800 border-b border-background-300 dark:border-background-700">
          <span class="text-xs font-medium text-background-500 uppercase tracking-wider col-span-2">{{ t('phases.fields.name') }}</span>
          <span class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('phases.fields.duration') }}</span>
          <span class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('phases.fields.maxSeverity') }}</span>
          <span class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('phases.fields.reworkSeverity') }}</span>
          <span class="text-xs font-medium text-background-500 uppercase tracking-wider text-right">{{ t('common.actions') }}</span>
        </div>

        <div
          v-for="phase in phases"
          :key="phase.id"
          class="grid grid-cols-6 px-4 py-3 border-b border-background-200 dark:border-background-700 last:border-0 bg-background-50 dark:bg-background-800 hover:bg-background-100 dark:hover:bg-background-750 transition-colors items-center"
        >
          <div class="col-span-2">
            <div class="text-sm font-medium text-background-900 dark:text-background-50">{{ phase.name }}</div>
            <div class="text-xs text-background-400 mt-0.5">ID #{{ phase.id }}</div>
          </div>
          <span class="text-sm text-background-600 dark:text-background-400">
            {{ phase.estimatedDuration ? `${Math.floor(phase.estimatedDuration / 60)}m ${phase.estimatedDuration % 60}s` : '—' }}
          </span>
          <div>
            <span class="text-xs font-medium px-2 py-1 rounded-full" :class="severityClass(phase.maxAcceptableSeverity)">
              {{ severityLabel(phase.maxAcceptableSeverity) }}
            </span>
          </div>
          <div>
            <span class="text-xs font-medium px-2 py-1 rounded-full" :class="severityClass(phase.reworkSeverity)">
              {{ severityLabel(phase.reworkSeverity) }}
            </span>
          </div>
          <div class="flex justify-end gap-1">
            <button
              @click="openEditModal(phase)"
              class="p-1.5 rounded-lg text-background-400 hover:text-primary-500 hover:bg-primary-50 dark:hover:bg-background-700 transition-colors"
              :title="t('common.edit')"
            >
              <span class="material-symbols-rounded text-base">edit</span>
            </button>
            <button
              @click="deletePhase(phase)"
              class="p-1.5 rounded-lg text-background-400 hover:text-danger-500 hover:bg-danger-100 dark:hover:bg-background-700 transition-colors"
              :title="t('common.delete')"
            >
              <span class="material-symbols-rounded text-base">delete</span>
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Modal: Criar/Editar Fase -->
    <div v-if="showModal" class="fixed inset-0 bg-black/40 flex items-center justify-center z-50" @click.self="showModal = false">
      <div class="bg-background-50 dark:bg-background-800 border border-background-300 dark:border-background-700 rounded-xl w-full max-w-md overflow-hidden">
        <div class="flex items-center justify-between px-5 py-4 border-b border-background-300 dark:border-background-700">
          <h2 class="text-base font-medium text-background-900 dark:text-background-50">
            {{ editingPhase ? t('phases.editPhase') : t('phases.newPhase') }}
          </h2>
          <button @click="showModal = false" class="text-background-500 hover:text-background-700 dark:hover:text-background-300">
            <span class="material-symbols-rounded">close</span>
          </button>
        </div>
        <div class="px-5 py-4 flex flex-col gap-4">
          <div class="flex flex-col gap-1.5">
            <label class="text-xs font-medium text-background-600 dark:text-background-400">{{ t('phases.fields.name') }} *</label>
            <input v-model="form.name" type="text" :placeholder="t('phases.fields.namePlaceholder')" />
          </div>
          <div class="flex flex-col gap-1.5">
            <label class="text-xs font-medium text-background-600 dark:text-background-400">{{ t('phases.fields.duration') }} <span class="text-background-400 font-normal">({{ t('phases.fields.durationHint') }})</span></label>
            <input v-model.number="form.estimatedDuration" type="number" min="0" :placeholder="t('phases.fields.durationPlaceholder')" />
          </div>
          <div class="grid grid-cols-2 gap-3">
            <div class="flex flex-col gap-1.5">
              <label class="text-xs font-medium text-background-600 dark:text-background-400">{{ t('phases.fields.maxSeverity') }} *</label>
              <select v-model="form.maxAcceptableSeverity">
                <option value="none">{{ t('phases.severity.none') }}</option>
                <option value="minor">{{ t('phases.severity.minor') }}</option>
                <option value="major">{{ t('phases.severity.major') }}</option>
                <option value="critical">{{ t('phases.severity.critical') }}</option>
              </select>
            </div>
            <div class="flex flex-col gap-1.5">
              <label class="text-xs font-medium text-background-600 dark:text-background-400">{{ t('phases.fields.reworkSeverity') }} *</label>
              <select v-model="form.reworkSeverity">
                <option value="none">{{ t('phases.severity.none') }}</option>
                <option value="minor">{{ t('phases.severity.minor') }}</option>
                <option value="major">{{ t('phases.severity.major') }}</option>
                <option value="critical">{{ t('phases.severity.critical') }}</option>
              </select>
            </div>
          </div>
          <p class="text-xs text-background-400">{{ t('phases.severityHint') }}</p>
        </div>
        <div class="flex justify-end gap-2 px-5 py-4 border-t border-background-300 dark:border-background-700">
          <button @click="showModal = false" class="px-4 py-2 text-sm rounded-lg border border-background-300 dark:border-background-600 text-background-700 dark:text-background-300 hover:bg-background-100 dark:hover:bg-background-700 transition-colors">
            {{ t('common.cancel') }}
          </button>
          <button @click="submitForm" :disabled="!form.name" class="px-4 py-2 text-sm rounded-lg bg-primary-500 hover:bg-primary-600 disabled:opacity-50 disabled:cursor-not-allowed text-white font-medium transition-colors">
            {{ t('common.save') }}
          </button>
        </div>
      </div>
    </div>

  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { manufacturingPhaseService } from '@/services/manufacturingPhaseService'
import type { ManufacturingPhase } from '@/services/manufacturingPhaseService'
import { toast } from '@/plugins/toast'
import { useI18n } from 'vue-i18n'
import { Severity } from '@/constants/status'

const { t } = useI18n()

const loading = ref(true)
const phases = ref<ManufacturingPhase[]>([])
const showModal = ref(false)
const editingPhase = ref<ManufacturingPhase | null>(null)

const form = reactive({
  name: '',
  estimatedDuration: undefined as number | undefined,
  maxAcceptableSeverity: Severity.None,
  reworkSeverity: Severity.Minor,
})

onMounted(async () => {
  await loadPhases()
})

async function loadPhases() {
  loading.value = true
  try {
    const res = await manufacturingPhaseService.getAll()
    phases.value = res.data
  } catch {
    toast.error(t('errors.loadFailed'))
  } finally {
    loading.value = false
  }
}

function openCreateModal() {
  editingPhase.value = null
  form.name = ''
  form.estimatedDuration = undefined
  form.maxAcceptableSeverity = Severity.None
  form.reworkSeverity = Severity.Minor
  showModal.value = true
}

function openEditModal(phase: ManufacturingPhase) {
  editingPhase.value = phase
  form.name = phase.name
  form.estimatedDuration = phase.estimatedDuration ?? undefined
  form.maxAcceptableSeverity = phase.maxAcceptableSeverity
  form.reworkSeverity = phase.reworkSeverity
  showModal.value = true
}

async function submitForm() {
  try {
    if (editingPhase.value) {
      await manufacturingPhaseService.update(editingPhase.value.id, {
        name: form.name,
        estimatedDuration: form.estimatedDuration,
        maxAcceptableSeverity: form.maxAcceptableSeverity,
        reworkSeverity: form.reworkSeverity,
      })
      toast.success(t('phases.updated'))
    } else {
      await manufacturingPhaseService.create({
        name: form.name,
        estimatedDuration: form.estimatedDuration,
        maxAcceptableSeverity: form.maxAcceptableSeverity,
        reworkSeverity: form.reworkSeverity,
      })
      toast.success(t('phases.created'))
    }
    showModal.value = false
    await loadPhases()
  } catch {
    toast.error(t('errors.saveFailed'))
  }
}

async function deletePhase(phase: ManufacturingPhase) {
  try {
    await manufacturingPhaseService.delete(phase.id)
    toast.success(t('phases.deleted'))
    await loadPhases()
  } catch {
    toast.error(t('errors.deleteFailed'))
  }
}

function severityClass(severity: string) {
  switch (severity) {
    case Severity.None: return 'bg-success-100 text-success-700'
    case Severity.Minor: return 'bg-warning-100 text-warning-700'
    case Severity.Major: return 'bg-danger-100 text-danger-700'
    case Severity.Critical: return 'bg-danger-100 text-danger-700 font-bold'
    default: return 'bg-background-200 text-background-600'
  }
}

function severityLabel(severity: string) {
  switch (severity) {
    case Severity.None: return t('phases.severity.none')
    case Severity.Minor: return t('phases.severity.minor')
    case Severity.Major: return t('phases.severity.major')
    case Severity.Critical: return t('phases.severity.critical')
    default: return severity
  }
}
</script>
