<template>
  <div class="p-8 w-full">

    <!-- Header -->
    <div class="flex items-start justify-between mb-8">
      <div>
        <h1 class="text-2xl font-medium text-background-900 dark:text-background-50">
          {{ t('timeline.title') }}
        </h1>
        <p v-if="product" class="text-sm text-background-600 dark:text-background-400 mt-1">
          {{ t('timeline.product') }}: <span class="font-medium text-background-800 dark:text-background-200">{{ product.serialNumber }}</span>
          <span class="ml-2 text-xs px-2 py-0.5 rounded-full" :class="product.status === 'completed' ? 'bg-success-100 text-success-700' : 'bg-primary-50 text-primary-600'">
            {{ product.status === 'completed' ? t('timeline.status.completed') : t('timeline.status.inProgress') }}
          </span>
        </p>
        <p v-else class="text-sm text-background-500 mt-1">{{ t('timeline.subtitle') }}</p>
      </div>

      <div class="flex items-center gap-2">
        <input
          v-model.number="productId"
          type="number"
          min="1"
          class="w-28"
          :placeholder="t('timeline.idPlaceholder')"
        />
        <button
          @click="loadTimeline"
          class="flex items-center gap-2 bg-primary-500 hover:bg-primary-600 text-white text-sm font-medium px-4 py-2 rounded-lg transition-colors"
        >
          <span class="material-symbols-rounded text-base">search</span>
          {{ t('common.search') }}
        </button>
      </div>
    </div>

    <!-- Loading -->
    <div v-if="loading" class="flex items-center gap-2 text-background-500 text-sm py-12">
      <span class="material-symbols-rounded animate-spin text-lg">autorenew</span>
      {{ t('common.loading') }}
    </div>

    <div v-else-if="timeline.length > 0" class="flex flex-col gap-6">

      <!-- Legenda -->
      <div class="flex items-center gap-6 px-1">
        <span class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('timeline.legend.title') }}</span>
        <div class="flex items-center gap-1.5">
          <div class="w-4 h-1 rounded-full bg-success-500"></div>
          <span class="text-xs text-background-600 dark:text-background-400">{{ t('timeline.legend.correct') }}</span>
        </div>
        <div class="flex items-center gap-1.5">
          <div class="w-4 h-1 rounded-full bg-warning-500"></div>
          <span class="text-xs text-background-600 dark:text-background-400">{{ t('timeline.legend.repeated') }}</span>
        </div>
        <div class="flex items-center gap-1.5">
          <div class="w-4 h-1 rounded-full bg-danger-500"></div>
          <span class="text-xs text-background-600 dark:text-background-400">{{ t('timeline.legend.skipped1') }}</span>
        </div>
        <div class="flex items-center gap-1.5">
          <div class="w-4 h-1 rounded-full bg-purple-500"></div>
          <span class="text-xs text-background-600 dark:text-background-400">{{ t('timeline.legend.skippedN') }}</span>
        </div>
        <div class="flex items-center gap-1.5">
          <div class="w-4 h-1 rounded-full bg-background-400"></div>
          <span class="text-xs text-background-600 dark:text-background-400">{{ t('timeline.legend.unknown') }}</span>
        </div>
      </div>

      <!-- Timeline visual -->
      <div class="flex flex-col">
        <div
          v-for="(phase, index) in timeline"
          :key="phase.productPhaseId"
        >
          <!-- Linha de fase -->
          <div
            class="flex items-start gap-4 px-4 py-3 rounded-xl border transition-colors"
            :class="[
              phase.endedAt
                ? 'bg-background-50 dark:bg-background-800 border-background-200 dark:border-background-700'
                : 'bg-primary-50 dark:bg-primary-950 border-primary-200 dark:border-primary-800'
            ]"
          >
            <!-- Indicador de estado -->
            <div class="flex flex-col items-center mt-1 shrink-0">
              <div
                class="w-3 h-3 rounded-full"
                :class="phase.endedAt ? 'bg-success-500' : 'bg-primary-500 animate-pulse'"
              ></div>
            </div>

            <!-- Conteúdo principal -->
            <div class="flex-1 grid grid-cols-7 items-center gap-2">
              <!-- Fase + alerta de sequência -->
              <div class="col-span-2">
                <div class="flex items-center gap-2">
                  <span class="text-sm font-medium text-background-900 dark:text-background-50">
                    {{ phase.phaseName }}
                  </span>
                  <!-- Badge de aviso se fase fora de sequência -->
                  <span
                    v-if="sequenceStatus(index) === 'unknown'"
                    class="text-xs px-1.5 py-0.5 rounded bg-background-200 dark:bg-background-700 text-background-500"
                    :title="t('timeline.legend.unknown')"
                  >
                    ?
                  </span>
                </div>
                <div class="text-xs text-background-400 mt-0.5">
                  {{ t('timeline.fields.phase') }} #{{ index + 1 }}
                  <span
                    v-if="expectedOrderLabel(index)"
                    class="ml-1 opacity-70"
                  >
                    · {{ t('timeline.expectedOrder') }} {{ expectedOrderLabel(index) }}
                  </span>
                </div>
              </div>

              <span class="text-sm text-background-600 dark:text-background-400">{{ phase.workstation }}</span>
              <span class="text-sm text-background-500">{{ formatDate(phase.startedAt) }}</span>
              <span class="text-sm text-background-500">{{ formatDate(phase.endedAt) }}</span>
              <span class="text-sm text-background-600 dark:text-background-400">{{ formatDuration(phase.durationSeconds) }}</span>

              <div>
                <span
                  v-if="phase.result"
                  class="text-xs font-medium px-2 py-1 rounded-full"
                  :class="phase.result === 'passed'
                    ? 'bg-success-100 text-success-700'
                    : phase.result === 'failed_scrapped'
                    ? 'bg-danger-100 text-danger-700'
                    : 'bg-warning-100 text-warning-700'"
                >
                  {{ resultLabel(phase.result) }}
                </span>
                <span v-else class="text-xs text-background-400 italic">{{ t('timeline.inProgress') }}</span>
              </div>
            </div>
          </div>

          <!-- Conector entre fases -->
          <div
            v-if="index < timeline.length - 1"
            class="flex items-center gap-3 pl-6 py-1"
          >
            <div class="flex flex-col items-center">
              <!-- Linha vertical colorida -->
              <div
                class="w-0.5 h-6 rounded-full"
                :class="connectorClass(index)"
              ></div>
            </div>
            <!-- Label do conector -->
            <span
              class="text-xs font-medium"
              :class="connectorTextClass(index)"
            >
              {{ connectorLabel(index) }}
            </span>
          </div>
        </div>
      </div>

      <!-- Tabela detalhada (colapsável) -->
      <div>
        <button
          @click="showTable = !showTable"
          class="flex items-center gap-2 text-xs text-background-500 hover:text-background-700 dark:hover:text-background-300 transition-colors"
        >
          <span class="material-symbols-rounded text-sm">{{ showTable ? 'expand_less' : 'expand_more' }}</span>
          {{ showTable ? t('timeline.hideTable') : t('timeline.showTable') }}
        </button>

        <div v-if="showTable" class="mt-3 border border-background-300 dark:border-background-700 rounded-xl overflow-hidden">
          <div class="grid grid-cols-7 px-4 py-3 bg-background-100 dark:bg-background-800 border-b border-background-300 dark:border-background-700">
            <span class="text-xs font-medium text-background-500 uppercase tracking-wider col-span-2">{{ t('timeline.fields.phase') }}</span>
            <span class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('timeline.fields.workstation') }}</span>
            <span class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('timeline.fields.start') }}</span>
            <span class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('timeline.fields.end') }}</span>
            <span class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('timeline.fields.duration') }}</span>
            <span class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('timeline.fields.result') }}</span>
          </div>
          <div
            v-for="phase in timeline"
            :key="'table-' + phase.productPhaseId"
            class="grid grid-cols-7 px-4 py-3 border-b border-background-200 dark:border-background-700 last:border-0 bg-background-50 dark:bg-background-800 items-center"
          >
            <div class="col-span-2 flex items-center gap-2">
              <div class="w-2 h-2 rounded-full shrink-0" :class="phase.endedAt ? 'bg-success-500' : 'bg-primary-500 animate-pulse'"></div>
              <span class="text-sm font-medium text-background-900 dark:text-background-50">{{ phase.phaseName }}</span>
            </div>
            <span class="text-sm text-background-600 dark:text-background-400">{{ phase.workstation }}</span>
            <span class="text-sm text-background-500">{{ formatDate(phase.startedAt) }}</span>
            <span class="text-sm text-background-500">{{ formatDate(phase.endedAt) }}</span>
            <span class="text-sm text-background-600 dark:text-background-400">{{ formatDuration(phase.durationSeconds) }}</span>
            <div>
              <span
                v-if="phase.result"
                class="text-xs font-medium px-2 py-1 rounded-full"
                :class="phase.result === 'passed' ? 'bg-success-100 text-success-700' : phase.result === 'failed_scrapped' ? 'bg-danger-100 text-danger-700' : 'bg-warning-100 text-warning-700'"
              >
                {{ resultLabel(phase.result) }}
              </span>
              <span v-else class="text-xs text-background-400">{{ t('timeline.inProgress') }}</span>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Empty -->
    <div v-else class="text-center py-16 text-background-500">
      <span class="material-symbols-rounded text-5xl block mb-3">timeline</span>
      <p class="text-sm">{{ t('timeline.empty') }}</p>
    </div>

  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue'
import axios from '@/axios'
import { useI18n } from 'vue-i18n'
import { toast } from '@/plugins/toast'
import { phaseSequenceService } from '@/services/manufacturingPhaseService'
import type { PhaseSequence } from '@/services/manufacturingPhaseService'

const { t } = useI18n()

const loading = ref(false)
const productId = ref<number>(1)
const timeline = ref<any[]>([])
const product = ref<any>(null)
const modelId = ref<number | null>(null)
const phaseSequence = ref<PhaseSequence[]>([])
const showTable = ref(false)

// ── Carregar timeline + sequência do modelo ────────────────────────────────────
async function loadTimeline() {
  loading.value = true
  timeline.value = []
  product.value = null
  phaseSequence.value = []
  modelId.value = null

  try {
    const response = await axios.get(`/products/${productId.value}/timeline`)
    product.value = {
      id: response.data.productId,
      serialNumber: response.data.serialNumber,
      status: response.data.status,
      modelId: response.data.modelId,
    }
    timeline.value = response.data.phases?.$values ?? response.data.phases ?? []
    modelId.value = response.data.modelId ?? null

    // Carregar sequência de fases do modelo para validação
    if (modelId.value) {
      try {
        const seqRes = await phaseSequenceService.getByModel(modelId.value)
        const raw = seqRes.data as any
        const seq: PhaseSequence[] = raw?.$values ?? seqRes.data ?? []
        phaseSequence.value = seq.slice().sort((a, b) => a.order - b.order)
      } catch {
        phaseSequence.value = []
      }
    }
  } catch {
    toast.error(t('errors.loadFailed'))
  } finally {
    loading.value = false
  }
}

// ── Lógica de validação de sequência ──────────────────────────────────────────

// Devolve o índice na sequência esperada para a fase no índice i da timeline
function getExpectedIndex(phaseIndex: number): number {
  const phase = timeline.value[phaseIndex]
  if (!phase || !phaseSequence.value.length) return -1
  return phaseSequence.value.findIndex(s => s.manufacturingPhaseId === phase.manufacturingPhaseId)
}

// Para mostrar a ordem esperada na sequência
function expectedOrderLabel(index: number): string | null {
  const ei = getExpectedIndex(index)
  if (ei === -1) return null
  return `${phaseSequence.value[ei].order}`
}

// Estado da transição PARA esta fase (compara com a anterior)
function sequenceStatus(index: number): 'correct' | 'repeated' | 'skipped1' | 'skippedN' | 'unknown' | 'first' {
  if (index === 0) return 'first'
  const prevEi = getExpectedIndex(index - 1)
  const currEi = getExpectedIndex(index)
  if (prevEi === -1 || currEi === -1) return 'unknown'
  const diff = currEi - prevEi
  if (diff === 1) return 'correct'
  if (diff === 0) return 'repeated'
  if (diff === 2) return 'skipped1'
  if (diff > 2) return 'skippedN'
  return 'unknown'
}

// Conector entre fase[index] e fase[index+1]
function transitionStatus(index: number): 'correct' | 'repeated' | 'skipped1' | 'skippedN' | 'unknown' {
  const currEi = getExpectedIndex(index)
  const nextEi = getExpectedIndex(index + 1)
  if (currEi === -1 || nextEi === -1) return 'unknown'
  const diff = nextEi - currEi
  if (diff === 1) return 'correct'
  if (diff === 0) return 'repeated'
  if (diff === 2) return 'skipped1'
  if (diff > 2) return 'skippedN'
  return 'unknown'
}

function connectorClass(index: number): string {
  const status = transitionStatus(index)
  switch (status) {
    case 'correct':  return 'bg-success-500'
    case 'repeated': return 'bg-warning-500'
    case 'skipped1': return 'bg-danger-500'
    case 'skippedN': return 'bg-purple-500'
    default:         return 'bg-background-400'
  }
}

function connectorTextClass(index: number): string {
  const status = transitionStatus(index)
  switch (status) {
    case 'correct':  return 'text-success-600'
    case 'repeated': return 'text-warning-600'
    case 'skipped1': return 'text-danger-600'
    case 'skippedN': return 'text-purple-600'
    default:         return 'text-background-400'
  }
}

function connectorLabel(index: number): string {
  const status = transitionStatus(index)
  switch (status) {
    case 'correct':  return t('timeline.transition.correct')
    case 'repeated': return t('timeline.transition.repeated')
    case 'skipped1': return t('timeline.transition.skipped1')
    case 'skippedN': return t('timeline.transition.skippedN')
    default:         return t('timeline.transition.unknown')
  }
}

// ── Formatação ─────────────────────────────────────────────────────────────────
function formatDate(date: string | null) {
  if (!date) return '—'
  const normalized = date.endsWith('Z') ? date : date + 'Z'
  return new Date(normalized).toLocaleString('pt-PT')
}

function formatDuration(seconds: number | null) {
  if (seconds === null || seconds === undefined) return t('timeline.inProgress')
  const minutes = Math.floor(seconds / 60)
  const remainingSeconds = seconds % 60
  return `${minutes}m ${remainingSeconds}s`
}

function resultLabel(result: string | null) {
  switch (result) {
    case 'passed':         return t('timeline.result.passed')
    case 'failed_rework':  return t('timeline.result.rework')
    case 'failed_scrapped':return t('timeline.result.scrapped')
    default: return result || '—'
  }
}

onMounted(() => loadTimeline())
</script>