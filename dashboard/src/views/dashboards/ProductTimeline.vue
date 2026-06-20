<template>
  <div class="p-6 w-full">

    <!-- Header -->
    <div class="flex items-start justify-between mb-5">
      <div>
        <h1 class="text-2xl font-medium text-background-900 dark:text-background-50">
          {{ t('timeline.title') }}
        </h1>
        <p v-if="product" class="text-sm text-background-600 dark:text-background-400 mt-1">
          {{ t('timeline.product') }}: <span class="font-medium text-background-800 dark:text-background-200">{{ product.serialNumber }}</span>
          <span class="ml-2 text-xs px-2 py-0.5 rounded-full"
            :class="product.status === 'completed' ? 'bg-success-100 text-success-700' : 'bg-primary-50 text-primary-600'">
            {{ product.status === 'completed' ? t('timeline.status.completed') : t('timeline.status.inProgress') }}
          </span>
        </p>
        <p v-else class="text-sm text-background-500 mt-1">{{ t('timeline.subtitle') }}</p>
      </div>

      <div class="flex items-center gap-3">
        <div v-if="timeline.length > 0"
          class="flex items-center bg-background-100 dark:bg-background-800 rounded-lg p-1 border border-background-300 dark:border-background-700">
          <button @click="viewMode = 'timeline'"
            class="flex items-center gap-1.5 px-3 py-1.5 rounded-md text-sm font-medium transition-colors"
            :class="viewMode === 'timeline'
              ? 'bg-white dark:bg-background-700 text-background-900 dark:text-background-50 shadow-sm'
              : 'text-background-500 hover:text-background-700 dark:hover:text-background-300'">
            <span class="material-symbols-rounded text-base">timeline</span>
            {{ t('timeline.viewTimeline') }}
          </button>
          <button @click="viewMode = 'graph'; nextTick(() => initHierarchyGraph())"
            class="flex items-center gap-1.5 px-3 py-1.5 rounded-md text-sm font-medium transition-colors"
            :class="viewMode === 'graph'
              ? 'bg-white dark:bg-background-700 text-background-900 dark:text-background-50 shadow-sm'
              : 'text-background-500 hover:text-background-700 dark:hover:text-background-300'">
            <span class="material-symbols-rounded text-base">account_tree</span>
            {{ t('timeline.viewGraph') }}
          </button>
        </div>
        <input v-model.number="productId" type="number" min="1" class="w-28" :placeholder="t('timeline.idPlaceholder')" />
        <button @click="loadTimeline"
          class="flex items-center gap-2 bg-primary-500 hover:bg-primary-600 text-white text-sm font-medium px-4 py-2 rounded-lg transition-colors">
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

    <div v-else-if="timeline.length > 0">

      <!-- ── TIMELINE VIEW ── -->
      <div v-if="viewMode === 'timeline'" class="flex flex-col gap-6">
        <div class="flex items-center gap-6 px-1 flex-wrap">
          <span class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('timeline.legend.title') }}</span>
          <div class="flex items-center gap-1.5"><div class="w-4 h-1 rounded-full bg-success-500"></div><span class="text-xs text-background-600 dark:text-background-400">{{ t('timeline.legend.correct') }}</span></div>
          <div class="flex items-center gap-1.5"><div class="w-4 h-1 rounded-full bg-warning-500"></div><span class="text-xs text-background-600 dark:text-background-400">{{ t('timeline.legend.repeated') }}</span></div>
          <div class="flex items-center gap-1.5"><div class="w-4 h-1 rounded-full bg-blue-500"></div><span class="text-xs text-background-600 dark:text-background-400">{{ t('timeline.legend.corrected') }}</span></div>
          <div class="flex items-center gap-1.5"><div class="w-4 h-1 rounded-full bg-danger-500"></div><span class="text-xs text-background-600 dark:text-background-400">{{ t('timeline.legend.skipped1') }}</span></div>
          <div class="flex items-center gap-1.5"><div class="w-4 h-1 rounded-full bg-purple-500"></div><span class="text-xs text-background-600 dark:text-background-400">{{ t('timeline.legend.skippedN') }}</span></div>
          <div class="flex items-center gap-1.5"><div class="w-4 h-1 rounded-full bg-background-400"></div><span class="text-xs text-background-600 dark:text-background-400">{{ t('timeline.legend.unknown') }}</span></div>
        </div>
        <div class="flex flex-col">
          <div v-for="(phase, index) in timeline" :key="phase.productPhaseId">
            <div class="flex items-start gap-4 px-4 py-3 rounded-xl border transition-colors"
              :class="phase.endedAt ? 'bg-background-50 dark:bg-background-800 border-background-200 dark:border-background-700' : 'bg-primary-50 dark:bg-primary-950 border-primary-200 dark:border-primary-800'">
              <div class="flex flex-col items-center mt-1 shrink-0">
                <div class="w-3 h-3 rounded-full" :class="phase.endedAt ? 'bg-success-500' : 'bg-primary-500 animate-pulse'"></div>
              </div>
              <div class="flex-1 grid grid-cols-7 items-center gap-2">
                <div class="col-span-2">
                  <div class="flex items-center gap-2">
                    <span class="text-sm font-medium text-background-900 dark:text-background-50">{{ phase.phaseName }}</span>
                    <span v-if="sequenceStatus(index) === 'unknown'" class="text-xs px-1.5 py-0.5 rounded bg-background-200 dark:bg-background-700 text-background-500">?</span>
                  </div>
                  <div class="text-xs text-background-400 mt-0.5">
                    {{ t('timeline.fields.phase') }} #{{ index + 1 }}
                    <span v-if="expectedOrderLabel(index)" class="ml-1 opacity-70">· {{ t('timeline.expectedOrder') }} {{ expectedOrderLabel(index) }}</span>
                  </div>
                </div>
                <span class="text-sm text-background-600 dark:text-background-400">{{ phase.workstation }}</span>
                <span class="text-sm text-background-500">{{ formatDate(phase.startedAt) }}</span>
                <span class="text-sm text-background-500">{{ formatDate(phase.endedAt) }}</span>
                <span class="text-sm text-background-600 dark:text-background-400">{{ formatDuration(phase.durationSeconds) }}</span>
                <div>
                  <span v-if="phase.result" class="text-xs font-medium px-2 py-1 rounded-full"
                    :class="phase.result === 'passed' ? 'bg-success-100 text-success-700' : phase.result === 'failed_scrapped' ? 'bg-danger-100 text-danger-700' : 'bg-warning-100 text-warning-700'">
                    {{ resultLabel(phase.result) }}
                  </span>
                  <span v-else class="text-xs text-background-400 italic">{{ t('timeline.inProgress') }}</span>
                </div>
              </div>
            </div>
            <div v-if="index < timeline.length - 1" class="flex items-center gap-3 pl-6 py-1">
              <div class="w-0.5 h-6 rounded-full" :class="connectorClass(index)"></div>
              <span class="text-xs font-medium" :class="connectorTextClass(index)">{{ connectorLabel(index) }}</span>
            </div>
          </div>
        </div>
        <div>
          <button @click="showTable = !showTable"
            class="flex items-center gap-2 text-xs text-background-500 hover:text-background-700 dark:hover:text-background-300 transition-colors">
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
            <div v-for="phase in timeline" :key="'table-' + phase.productPhaseId"
              class="grid grid-cols-7 px-4 py-3 border-b border-background-200 dark:border-background-700 last:border-0 bg-background-50 dark:bg-background-800 items-center">
              <div class="col-span-2 flex items-center gap-2">
                <div class="w-2 h-2 rounded-full shrink-0" :class="phase.endedAt ? 'bg-success-500' : 'bg-primary-500 animate-pulse'"></div>
                <span class="text-sm font-medium text-background-900 dark:text-background-50">{{ phase.phaseName }}</span>
              </div>
              <span class="text-sm text-background-600 dark:text-background-400">{{ phase.workstation }}</span>
              <span class="text-sm text-background-500">{{ formatDate(phase.startedAt) }}</span>
              <span class="text-sm text-background-500">{{ formatDate(phase.endedAt) }}</span>
              <span class="text-sm text-background-600 dark:text-background-400">{{ formatDuration(phase.durationSeconds) }}</span>
              <div>
                <span v-if="phase.result" class="text-xs font-medium px-2 py-1 rounded-full"
                  :class="phase.result === 'passed' ? 'bg-success-100 text-success-700' : phase.result === 'failed_scrapped' ? 'bg-danger-100 text-danger-700' : 'bg-warning-100 text-warning-700'">
                  {{ resultLabel(phase.result) }}
                </span>
                <span v-else class="text-xs text-background-400">{{ t('timeline.inProgress') }}</span>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- ── GRAPH VIEW ── -->
      <div v-else-if="viewMode === 'graph'" class="flex flex-col gap-3">

        <!-- Legenda + hint + reset -->
        <div class="flex flex-col gap-2 px-1">
          <div class="flex items-center justify-between gap-4 flex-wrap">
            <div class="flex items-center gap-5 flex-wrap">
              <span class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('timeline.legend.title') }}</span>
              <div class="flex items-center gap-1.5"><div class="w-6 h-0.5 rounded-full bg-success-500"></div><span class="text-xs text-background-600 dark:text-background-400">{{ t('timeline.legend.correct') }}</span></div>
              <div class="flex items-center gap-1.5"><div class="w-6 h-0.5 rounded-full bg-warning-500"></div><span class="text-xs text-background-600 dark:text-background-400">{{ t('timeline.legend.repeated') }}</span></div>
              <div class="flex items-center gap-1.5"><div class="w-6 h-0.5 rounded-full bg-blue-500"></div><span class="text-xs text-background-600 dark:text-background-400">{{ t('timeline.legend.corrected') }}</span></div>
              <div class="flex items-center gap-1.5"><div class="w-6 h-0.5 rounded-full bg-danger-500"></div><span class="text-xs text-background-600 dark:text-background-400">{{ t('timeline.legend.skipped1') }}</span></div>
              <div class="flex items-center gap-1.5"><div class="w-6 h-0.5 rounded-full bg-purple-500"></div><span class="text-xs text-background-600 dark:text-background-400">{{ t('timeline.legend.skippedN') }}</span></div>
              <div class="flex items-center gap-1.5"><div class="w-6 h-0.5 rounded-full bg-background-400"></div><span class="text-xs text-background-600 dark:text-background-400">{{ t('timeline.legend.unknown') }}</span></div>
            </div>
            <button @click="resetGraphView"
              class="flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-xs font-medium bg-background-100 dark:bg-background-800 border border-background-300 dark:border-background-700 text-background-600 dark:text-background-400 hover:text-background-900 dark:hover:text-background-100 transition-colors">
              <span class="material-symbols-rounded text-sm">center_focus_strong</span>
              {{ t('timeline.resetView') }}
            </button>
          </div>

          <!-- Legenda dos badges/dots nos nós -->
          <div class="flex items-center gap-5 flex-wrap">
            <span class="text-xs font-medium text-background-400">{{ t('timeline.nodeStatus') }}</span>
            <div class="flex items-center gap-1.5"><div class="w-2.5 h-2.5 rounded-full bg-success-500"></div><span class="text-xs text-background-600 dark:text-background-400">{{ t('timeline.nodeCompleted') }}</span></div>
            <div class="flex items-center gap-1.5"><div class="w-2.5 h-2.5 rounded-full" style="background-color:#3b82f6"></div><span class="text-xs text-background-600 dark:text-background-400">{{ t('timeline.nodeInProgress') }}</span></div>
            <div class="flex items-center gap-1.5">
              <div class="w-4 h-4 rounded-full bg-warning-500 flex items-center justify-center text-white" style="font-size:6px; font-weight:700;">N×</div>
              <span class="text-xs text-background-600 dark:text-background-400">{{ t('timeline.nodeRepeatedTimes') }}</span>
            </div>
          </div>
        </div>

        <p class="text-xs text-background-400 px-1">
          <span class="material-symbols-rounded text-sm align-middle">pan_tool</span>
          {{ t('timeline.graphHintFull') }}
        </p>

        <!-- SVG Container -->
        <div ref="graphContainer"
          class="relative w-full rounded-xl border border-background-300 dark:border-background-700 overflow-hidden"
          style="height: calc(100vh - 300px); min-height: 420px; background: transparent;">
          <svg ref="graphSvg" class="w-full h-full" style="display:block;"></svg>

          <!-- Tooltip -->
          <div v-if="tooltip.visible"
            class="fixed pointer-events-none z-50 bg-background-900 dark:bg-background-950 text-background-50 text-xs rounded-xl px-3 py-2.5 shadow-2xl border border-background-700 min-w-[160px]"
            :style="{ left: tooltip.screenX + 'px', top: tooltip.screenY + 'px', transform: 'translate(-50%, -110%)' }">
            <div class="font-semibold mb-1.5 text-sm">{{ tooltip.title }}</div>
            <div v-for="row in tooltip.rows" :key="row.label" class="flex gap-1.5 leading-5">
              <span class="text-background-400 shrink-0">{{ row.label }}:</span>
              <span class="text-background-200">{{ row.value }}</span>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Empty state -->
    <div v-else class="text-center py-16 text-background-500">
      <span class="material-symbols-rounded text-5xl block mb-3">timeline</span>
      <p class="text-sm">{{ t('timeline.empty') }}</p>
    </div>

  </div>
</template>

<script setup lang="ts">
import { nextTick, onMounted, onUnmounted, ref } from 'vue'
import { useRoute } from 'vue-router'
import * as d3 from 'd3'
import axios from '@/axios'
import { useI18n } from 'vue-i18n'
import { toast } from '@/plugins/toast'
import { phaseSequenceService } from '@/services/manufacturingPhaseService'
import type { PhaseSequence } from '@/services/manufacturingPhaseService'

const { t } = useI18n()
const route = useRoute()

// ── State ──────────────────────────────────────────────────────────────────────
const loading       = ref(false)
const productId     = ref<number>(1)
const timeline      = ref<any[]>([])
const product       = ref<any>(null)
const modelId       = ref<number | null>(null)
const phaseSequence = ref<PhaseSequence[]>([])
const showTable     = ref(false)
const viewMode      = ref<'timeline' | 'graph'>('timeline')

const clientOrder        = ref<any>(null)
const manufacturingOrder = ref<any>(null)
const skidData           = ref<any>(null)

const graphContainer = ref<HTMLDivElement>()
const graphSvg       = ref<SVGSVGElement>()
const tooltip = ref({
  visible: false, screenX: 0, screenY: 0,
  title: '', rows: [] as { label: string; value: string }[],
})

// Guardados para o botão "Reset view"
let savedZoomBehavior: d3.ZoomBehavior<SVGSVGElement, unknown> | null = null
let savedInitialTransform: d3.ZoomTransform | null = null

function resetGraphView() {
  if (!graphSvg.value || !savedZoomBehavior || !savedInitialTransform) return
  d3.select(graphSvg.value)
    .transition().duration(400)
    .call(savedZoomBehavior.transform, savedInitialTransform)
}

// ── Load ───────────────────────────────────────────────────────────────────────
async function loadTimeline() {
  loading.value = true
  timeline.value = []; product.value = null; phaseSequence.value = []
  modelId.value = null; clientOrder.value = null
  manufacturingOrder.value = null; skidData.value = null
  viewMode.value = 'timeline'

  try {
    const res = await axios.get(`/products/${productId.value}/timeline`)
    product.value = {
      id: res.data.productId,
      serialNumber: res.data.serialNumber,
      status: res.data.status,
      modelId: res.data.modelId,
    }
    timeline.value = res.data.phases?.$values ?? res.data.phases ?? []
    modelId.value  = res.data.modelId ?? null

    if (modelId.value) {
      try {
        const seqRes = await phaseSequenceService.getByModel(modelId.value)
        const raw = seqRes.data as any
        const seq: PhaseSequence[] = raw?.$values ?? seqRes.data ?? []
        phaseSequence.value = seq.slice().sort((a, b) => a.order - b.order)
      } catch { phaseSequence.value = [] }
    }

    await loadExtraData()
  } catch {
    toast.error(t('errors.loadFailed'))
  } finally {
    loading.value = false
  }
}

async function loadExtraData() {
  try {
    const supportsRes = await axios.get('/Support')
    const supports: any[] = supportsRes.data?.$values ?? supportsRes.data ?? []
    for (const sup of supports) {
      try {
        const curRes = await axios.get(`/SupportedProduct/support/${sup.id}/current`)
        if (curRes.data?.productId === product.value?.id) {
          skidData.value = sup; break
        }
      } catch { /* continuar */ }
    }
    const moRes = await axios.get('/ManufacturingOrder')
    const mos: any[] = moRes.data?.$values ?? moRes.data ?? []
    for (const mo of mos) {
      try {
        const detRes = await axios.get(`/ManufacturingOrder/${mo.id}/details`)
        const det = detRes.data
        const prods: any[] = det.products?.$values ?? det.products ?? []
        if (prods.some((p: any) => p.serialNumber === product.value?.serialNumber)) {
          manufacturingOrder.value = det
          try {
            const coRes = await axios.get(`/ClientOrder/${det.clientOrderId}`)
            clientOrder.value = coRes.data
          } catch { clientOrder.value = null }
          break
        }
      } catch { /* continuar */ }
    }
  } catch { /* dados extra opcionais */ }
}

// ── Grafo Hierárquico ──────────────────────────────────────────────────────────
function initHierarchyGraph() {
  if (!graphSvg.value || !graphContainer.value) return

  const svgEl  = graphSvg.value
  const contEl = graphContainer.value
  const isDark = document.documentElement.classList.contains('dark')
  const W = contEl.clientWidth
  const H = contEl.clientHeight

  // ── Paleta ─────────────────────────────────────────────────────────────────
  const C = {
    canvasBg:    isDark ? '#0f172a' : '#f1f5f9',
    nodeBg:      isDark ? '#1e293b' : '#ffffff',
    nodeShadow:  isDark ? 'rgba(0,0,0,0.5)' : 'rgba(100,116,139,0.18)',
    nodeText:    isDark ? '#f1f5f9' : '#0f172a',
    nodeSubText: isDark ? '#64748b' : '#94a3b8',
    typeLabel:   isDark ? '#94a3b8' : '#64748b',
    edge:        isDark ? '#334155' : '#cbd5e1',
    edgeNeutral: isDark ? '#475569' : '#94a3b8',
    edgeLabel:   isDark ? '#475569' : '#94a3b8',
    success:     '#22c55e',
    warning:     '#eab308',
    danger:      '#ef4444',
    purple:      '#a855f7',
    corrected:   '#3b82f6',
    statusActive:'#3b82f6',
    // Cores por tipo
    client:      '#7c3aed',
    order:       '#0ea5e9',
    mo:          '#3E55F2',
    product:     '#10b981',
    skid:        '#f59e0b',
    phase:       '#6366f1',
    ws:          '#64748b',
    modelprocess:'#ec4899',
  }

  // ── Ícones SVG por tipo (Material Symbols path data) ────────────────────────
  const ICONS: Record<string, string> = {
    client:  'M12 12c2.7 0 4.8-2.1 4.8-4.8S14.7 2.4 12 2.4 7.2 4.5 7.2 7.2 9.3 12 12 12zm0 2.4c-3.2 0-9.6 1.6-9.6 4.8v2.4h19.2v-2.4c0-3.2-6.4-4.8-9.6-4.8z',
    modelprocess: 'M4 6H2v14c0 1.1.9 2 2 2h14v-2H4V6zm16-4H8c-1.1 0-2 .9-2 2v12c0 1.1.9 2 2 2h12c1.1 0 2-.9 2-2V4c0-1.1-.9-2-2-2zm-1 9h-4v4h-2v-4H9V9h4V5h2v4h4v2z',
    order:   'M19 3H5c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h14c1.1 0 2-.9 2-2V5c0-1.1-.9-2-2-2zm-7 3c1.93 0 3.5 1.57 3.5 3.5S13.93 13 12 13s-3.5-1.57-3.5-3.5S10.07 6 12 6zm7 13H5v-.23c0-.62.28-1.2.76-1.58C7.47 15.82 9.64 15 12 15s4.53.82 6.24 2.19c.48.38.76.97.76 1.58V19z',
    mo:      'M19 3H5c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h14c1.1 0 2-.9 2-2V5c0-1.1-.9-2-2-2zm-5 14H7v-2h7v2zm3-4H7v-2h10v2zm0-4H7V7h10v2z',
    product: 'M18.92 5.01C18.72 4.42 18.16 4 17.5 4h-11c-.66 0-1.21.42-1.42 1.01L3 11v8c0 .55.45 1 1 1h1c.55 0 1-.45 1-1v-1h12v1c0 .55.45 1 1 1h1c.55 0 1-.45 1-1v-8l-2.08-5.99zM6.85 6h10.29l1.08 3.11H5.77L6.85 6zM19 17H5v-6h14v6zm-8.5-1c.83 0 1.5-.67 1.5-1.5S11.33 13 10.5 13 9 13.67 9 14.5s.67 1.5 1.5 1.5zm7 0c.83 0 1.5-.67 1.5-1.5S18.33 13 17.5 13 16 13.67 16 14.5s.67 1.5 1.5 1.5z',
    skid:    'M20 8h-3V4H3c-1.1 0-2 .9-2 2v11h2c0 1.66 1.34 3 3 3s3-1.34 3-3h6c0 1.66 1.34 3 3 3s3-1.34 3-3h2v-5l-3-4zm-5 9c-.55 0-1-.45-1-1s.45-1 1-1 1 .45 1 1-.45 1-1 1zm-12-1c0-.55.45-1 1-1s1 .45 1 1-.45 1-1 1-1-.45-1-1zm15-5h-2.5V9.5H17l1.5 2.5zm-8.5 0V9h3v2.5h-3z',
    phase:   'M19.14 12.94c.04-.3.06-.61.06-.94 0-.32-.02-.64-.07-.94l2.03-1.58c.18-.14.23-.41.12-.61l-1.92-3.32c-.12-.22-.37-.29-.59-.22l-2.39.96c-.5-.38-1.03-.7-1.62-.94l-.36-2.54c-.04-.24-.24-.41-.48-.41h-3.84c-.24 0-.43.17-.47.41l-.36 2.54c-.59.24-1.13.57-1.62.94l-2.39-.96c-.22-.08-.47 0-.59.22L2.74 8.87c-.12.21-.08.47.12.61l2.03 1.58c-.05.3-.09.63-.09.94s.02.64.07.94l-2.03 1.58c-.18.14-.23.41-.12.61l1.92 3.32c.12.22.37.29.59.22l2.39-.96c.5.38 1.03.7 1.62.94l.36 2.54c.05.24.24.41.48.41h3.84c.24 0 .44-.17.47-.41l.36-2.54c.59-.24 1.13-.56 1.62-.94l2.39.96c.22.08.47 0 .59-.22l1.92-3.32c.12-.22.07-.47-.12-.61l-2.01-1.58zM12 15.6c-1.98 0-3.6-1.62-3.6-3.6s1.62-3.6 3.6-3.6 3.6 1.62 3.6 3.6-1.62 3.6-3.6 3.6z',
    ws:      'M12 2C8.13 2 5 5.13 5 9c0 5.25 7 13 7 13s7-7.75 7-13c0-3.87-3.13-7-7-7zm0 9.5c-1.38 0-2.5-1.12-2.5-2.5s1.12-2.5 2.5-2.5 2.5 1.12 2.5 2.5-1.12 2.5-2.5 2.5z',
  }

  // ── Estrutura de nós/arestas ────────────────────────────────────────────────
  interface HNode {
    id: string; type: string
    typeLabel: string; nameLabel: string; subLabel?: string
    x: number; y: number
    tooltipRows?: { label: string; value: string }[]
    statusDot?: string
    occurrenceCount?: number
    isActivePulse?: boolean
  }
  interface HEdge {
    source: string; target: string; label?: string; color?: string
  }

  const nodes: HNode[] = []
  const edges: HEdge[] = []

  const addNode = (n: Omit<HNode, 'x' | 'y'>) => nodes.push({ ...n, x: 0, y: 0 })
  const addEdge = (e: HEdge) => edges.push(e)

  // Client
  const clientName = clientOrder.value?.customerName ?? manufacturingOrder.value?.customerName ?? 'Cliente'
  addNode({
    id: 'client', type: 'client', typeLabel: t('timeline.graphNodes.client'),
    nameLabel: clientName,
    tooltipRows: clientOrder.value ? [
      { label: 'ID', value: String(clientOrder.value.id) },
      { label: 'Data', value: formatDate(clientOrder.value.orderDate) },
      { label: 'Qtd', value: String(clientOrder.value.quantity) },
    ] : [],
  })

  // Order
  const orderNum = clientOrder.value?.orderNumber ?? '—'
  addNode({
    id: 'order', type: 'order', typeLabel: t('timeline.graphNodes.order'),
    nameLabel: orderNum,
    tooltipRows: clientOrder.value ? [
      { label: 'Número', value: clientOrder.value.orderNumber },
      { label: 'Data', value: formatDate(clientOrder.value.orderDate) },
      { label: 'Cliente', value: clientOrder.value.customerName },
    ] : [],
  })
  addEdge({ source: 'client', target: 'order', label: t('timeline.graphEdges.order') })

  // MO
  const moNum = manufacturingOrder.value?.manufacturingOrderNumber ?? '—'
  const moStatus = manufacturingOrder.value?.status ?? ''
  addNode({
    id: 'mo', type: 'mo', typeLabel: t('timeline.graphNodes.manufacturingOrder'),
    nameLabel: moNum,
    statusDot: moStatus === 'completed' ? C.success : moStatus === 'in_progress' ? C.statusActive : undefined,
    tooltipRows: manufacturingOrder.value ? [
      { label: 'Número', value: manufacturingOrder.value.manufacturingOrderNumber },
      { label: 'Status', value: manufacturingOrder.value.status },
      { label: 'Início', value: formatDate(manufacturingOrder.value.startDate) },
    ] : [],
  })
  addEdge({ source: 'order', target: 'mo', label: t('timeline.graphEdges.lot') })

  // Product
  const serialNum = product.value?.serialNumber ?? '—'
  const shortSerial = serialNum.length > 18 ? serialNum.slice(0, 16) + '…' : serialNum
  const moProducts: any[] = manufacturingOrder.value?.products?.$values ?? manufacturingOrder.value?.products ?? []
  const moProduct = moProducts.find((p: any) => p.serialNumber === serialNum) ?? moProducts[0]
  const modelName = moProduct?.modelName ?? '—'

  addNode({
    id: 'product', type: 'product', typeLabel: t('timeline.graphNodes.productLot'),
    nameLabel: shortSerial, subLabel: modelName,
    statusDot: product.value?.status === 'completed' ? C.success : product.value?.status === 'in_progress' ? C.statusActive : undefined,
    tooltipRows: [
      { label: 'VIN', value: serialNum },
      { label: 'Modelo', value: modelName },
      { label: 'Status', value: product.value?.status ?? '—' },
      ...(moProduct?.configs ?? []).slice(0, 3).map((c: any) => ({ label: c.configItem, value: c.optionValue })),
    ],
  })
  addEdge({ source: 'mo', target: 'product', label: t('timeline.graphEdges.process') })

  // Fases + Workstations — calcular ocorrências reais primeiro
  const phaseOccurrences: Record<number, any[]> = {}
  const phaseToWs: Record<number, string> = {}
  for (const ph of timeline.value) {
    const seq = phaseSequence.value.find(s => s.manufacturingPhaseId === ph.manufacturingPhaseId)
    if (!seq) continue
    const key = seq.manufacturingPhaseId
    if (!phaseOccurrences[key]) phaseOccurrences[key] = []
    phaseOccurrences[key].push(ph)
    phaseToWs[key] = ph.workstation
  }

  // Fase ativa atual (endedAt === null na última ocorrência)
  const activePhaseEntry = timeline.value.find(ph => !ph.endedAt)
  const activeSeq = activePhaseEntry
    ? phaseSequence.value.find(s => s.manufacturingPhaseId === activePhaseEntry.manufacturingPhaseId)
    : null
  const activePhaseNodeId = activeSeq ? `phase-${activeSeq.manufacturingPhaseId}` : null

  // Nó Model Process (Phase Sequence) — entre Product e as fases
  const modelName2 = (() => {
    const moProducts2: any[] = manufacturingOrder.value?.products?.$values ?? manufacturingOrder.value?.products ?? []
    const moProd = moProducts2.find((p: any) => p.serialNumber === product.value?.serialNumber) ?? moProducts2[0]
    return moProd?.modelName ?? '—'
  })()

  addNode({
    id: 'modelprocess', type: 'modelprocess', typeLabel: t('timeline.graphNodes.modelProcess'),
    nameLabel: modelName2,
    subLabel: `${phaseSequence.value.length} ${t('timeline.graphLabels.phasesCount')}`,
    tooltipRows: [
      { label: 'Modelo', value: modelName2 },
      { label: 'Total de fases', value: String(phaseSequence.value.length) },
      ...phaseSequence.value.map(s => ({ label: `Fase ${s.order}`, value: s.phaseName })),
    ],
  })
  addEdge({ source: 'product', target: 'modelprocess', label: t('timeline.graphEdges.process') })

  // Skid
  if (skidData.value) {
    addNode({
      id: 'skid', type: 'skid', typeLabel: t('timeline.graphNodes.skid'),
      nameLabel: skidData.value.type,
      subLabel: `${t('timeline.graphLabels.tag')}: ${skidData.value.rfidTag}`,
      tooltipRows: [
        { label: 'ID', value: String(skidData.value.id) },
        { label: 'RFID', value: skidData.value.rfidTag },
        { label: 'Tipo', value: skidData.value.type },
      ],
    })
  }

  // ── Classificação de transição entre fases por 'order', considerando histórico completo ──
  // (mesma lógica do backend: avanço correto / repetição / correção de buraco / salto)
  const sortedOrders = phaseSequence.value.map(s => s.order).sort((a, b) => a - b)

  function occurredOrders(): Set<number> {
    const set = new Set<number>()
    for (const s of phaseSequence.value) {
      if ((phaseOccurrences[s.manufacturingPhaseId] ?? []).length > 0) set.add(s.order)
    }
    return set
  }

  function classifyOrderEdge(targetOrder: number, visitedBefore: Set<number>): string {
    if (visitedBefore.size === 0) return targetOrder === sortedOrders[0] ? C.success : C.danger
    const maxVisited = Math.max(...visitedBefore)
    if (visitedBefore.has(targetOrder)) return C.warning // repetição
    if (targetOrder === maxVisited + 1) return C.success // avanço correto
    if (targetOrder < maxVisited) return C.corrected      // preencheu buraco
    if (targetOrder === maxVisited + 2) return C.danger    // saltou 1
    return C.purple                                        // saltou várias
  }

  for (const seq of phaseSequence.value) {
    const phId = `phase-${seq.manufacturingPhaseId}`
    const wsId = `ws-${seq.manufacturingPhaseId}`
    const occs = phaseOccurrences[seq.manufacturingPhaseId] ?? []
    const lastOcc = occs[occs.length - 1]
    const isActive = lastOcc && !lastOcc.endedAt
    const isDone = lastOcc && !!lastOcc.endedAt

    addNode({
      id: phId, type: 'phase', typeLabel: t('timeline.graphNodes.processPhase'),
      nameLabel: seq.phaseName, subLabel: `${t('timeline.graphLabels.phaseN')} ${seq.order}`,
      occurrenceCount: occs.length,
      statusDot: isActive ? C.statusActive : (isDone ? C.success : undefined),
      isActivePulse: !!isActive,
      tooltipRows: [
        { label: 'Fase', value: seq.phaseName },
        { label: 'Ordem', value: String(seq.order) },
        { label: 'Ocorrências', value: String(occs.length) },
        ...(lastOcc ? [
          { label: 'Última entrada', value: formatDate(lastOcc.startedAt) },
          { label: 'Última saída', value: formatDate(lastOcc.endedAt) },
        ] : []),
      ],
    })

    const prevSeq = phaseSequence.value.find(s => s.order === seq.order - 1)
    const srcId = seq.order === 1
      ? 'modelprocess'
      : `phase-${prevSeq?.manufacturingPhaseId}`

    let edgeColor = C.edgeNeutral
    if (occs.length > 0) {
      // 'visitedBefore' = orders com ocorrências, EXCLUINDO esta fase
      // (aproximação: usamos o conjunto global de orders ocorridos antes de esta ter
      // a sua primeira ocorrência, comparando timestamps)
      const firstOccStart = occs[0]?.startedAt
      const visitedBefore = new Set<number>()
      for (const s2 of phaseSequence.value) {
        if (s2.manufacturingPhaseId === seq.manufacturingPhaseId) continue
        const occs2 = phaseOccurrences[s2.manufacturingPhaseId] ?? []
        if (occs2.some(o => o.startedAt < firstOccStart)) visitedBefore.add(s2.order)
      }
      edgeColor = classifyOrderEdge(seq.order, visitedBefore)
    }

    addEdge({ source: srcId, target: phId, label: `${t('timeline.graphEdges.step')} ${seq.order}`, color: edgeColor })

    const wsName = phaseToWs[seq.manufacturingPhaseId]
      ? `WS ${phaseToWs[seq.manufacturingPhaseId]}`
      : t('timeline.notVisited')
    addNode({
      id: wsId, type: 'ws', typeLabel: t('timeline.graphNodes.section'),
      nameLabel: wsName,
      tooltipRows: occs.map((o, i) => ({
        label: `${t('timeline.visit')} ${i + 1}`,
        value: `${formatDate(o.startedAt)} → ${formatDate(o.endedAt)}`,
      })),
    })
    addEdge({ source: phId, target: wsId, label: t('timeline.graphEdges.section'), color: C.ws })
  }

  // Aresta Skid → fase ativa (ou última fase visitada se não há ativa)
  if (skidData.value) {
    let skidAtSeq: PhaseSequence | null = null
    if (activeSeq) {
      skidAtSeq = activeSeq
    } else {
      for (let i = phaseSequence.value.length - 1; i >= 0; i--) {
        const s = phaseSequence.value[i]
        if ((phaseOccurrences[s.manufacturingPhaseId] ?? []).length > 0) { skidAtSeq = s; break }
      }
    }
    const skidTargetId = skidAtSeq ? `phase-${skidAtSeq.manufacturingPhaseId}` : null

    let skidEdgeColor = C.edgeNeutral
    if (skidAtSeq) {
      const occsOfTarget = phaseOccurrences[skidAtSeq.manufacturingPhaseId] ?? []
      const firstOccStart = occsOfTarget[0]?.startedAt
      const visitedBefore = new Set<number>()
      for (const s2 of phaseSequence.value) {
        if (s2.manufacturingPhaseId === skidAtSeq.manufacturingPhaseId) continue
        const occs2 = phaseOccurrences[s2.manufacturingPhaseId] ?? []
        if (occs2.some(o => o.startedAt < firstOccStart)) visitedBefore.add(s2.order)
      }
      skidEdgeColor = classifyOrderEdge(skidAtSeq.order, visitedBefore)
    }

    if (skidTargetId) {
      addEdge({ source: 'skid', target: skidTargetId, label: t('timeline.graphEdges.isAt'), color: skidEdgeColor })
    }
  }

  // ── Layout LEFT-TO-RIGHT ────────────────────────────────────────────────────
  const BOX  = 90
  const ICON = 36
  const GAP_X = 160
  const GAP_Y = 140
  const GAP_WS = 130

  const nPhases = phaseSequence.value.length

  const mainNodeIds = ['client', 'order', 'mo', 'product', 'modelprocess']
  const MAIN_ROW_Y = 80 + BOX / 2
  const MAIN_START_X = 60 + BOX / 2

  mainNodeIds.forEach((id, i) => {
    const n = nodes.find(n => n.id === id)
    if (n) {
      n.x = MAIN_START_X + i * GAP_X
      n.y = MAIN_ROW_Y
    }
  })

  const lastMainX = MAIN_START_X + (mainNodeIds.length - 1) * GAP_X
  const phaseStartX = lastMainX + GAP_X
  const phaseColCenterY = MAIN_ROW_Y

  const phaseColH = nPhases * BOX + (nPhases - 1) * (GAP_Y - BOX)
  const phaseStartY = phaseColCenterY - phaseColH / 2 + BOX / 2

  const phaseNodes = nodes.filter(n => n.type === 'phase')
  phaseNodes.forEach((ph, i) => {
    ph.x = phaseStartX
    ph.y = phaseStartY + i * GAP_Y
  })

  for (const ph of phaseNodes) {
    const wsId = ph.id.replace('phase-', 'ws-')
    const ws = nodes.find(n => n.id === wsId)
    if (ws) {
      ws.x = phaseStartX + GAP_WS
      ws.y = ph.y
    }
  }

  const skidNode = nodes.find(n => n.id === 'skid')
  const skidTargetNode = activePhaseNodeId
    ? nodes.find(n => n.id === activePhaseNodeId)
    : (() => {
        for (let i = phaseSequence.value.length - 1; i >= 0; i--) {
          const s = phaseSequence.value[i]
          if ((phaseOccurrences[s.manufacturingPhaseId] ?? []).length > 0)
            return nodes.find(n => n.id === `phase-${s.manufacturingPhaseId}`)
        }
        return phaseNodes[phaseNodes.length - 1]
      })()

  if (skidNode && skidTargetNode) {
    skidNode.x = phaseStartX
    skidNode.y = skidTargetNode.y - GAP_Y
    if (skidNode.y > phaseStartY - GAP_Y / 2) {
      skidNode.x = phaseStartX - GAP_WS
      skidNode.y = skidTargetNode.y
    }
  }

  const canvasW = phaseStartX + GAP_WS + BOX + 100
  const canvasH = Math.max(
    MAIN_ROW_Y + BOX + 60,
    phaseStartY + phaseColH + 60,
    (skidNode?.y ?? 0) + BOX + 60
  )

  // ── SVG setup ───────────────────────────────────────────────────────────────
  d3.select(svgEl).selectAll('*').remove()

  const viewW = contEl.clientWidth
  const viewH = contEl.clientHeight

  const svg = d3.select(svgEl)
    .attr('width', viewW)
    .attr('height', viewH)

  svg.append('rect')
    .attr('width', viewW).attr('height', viewH)
    .attr('fill', C.canvasBg)

  const zoomG = svg.append('g').attr('class', 'zoom-layer')
  const zoom = d3.zoom<SVGSVGElement, unknown>()
    .scaleExtent([0.2, 3])
    .on('zoom', (event) => { zoomG.attr('transform', event.transform) })
  d3.select(svgEl).call(zoom)

  const defs = zoomG.append('defs')
  const arrowDefs: Record<string, string> = {
    edge: C.edge, edgeNeutral: C.edgeNeutral, success: C.success,
    warning: C.warning, danger: C.danger, purple: C.purple, ws: C.ws, corrected: C.corrected,
  }
  for (const [name, color] of Object.entries(arrowDefs)) {
    defs.append('marker')
      .attr('id', `ha-${name}`).attr('markerWidth', 8).attr('markerHeight', 8)
      .attr('refX', 5).attr('refY', 3).attr('orient', 'auto')
      .append('path').attr('d', 'M0,0 L0,6 L8,3 z').attr('fill', color)
  }

  function colorToArrowId(color: string): string {
    if (color === C.success) return 'success'
    if (color === C.warning) return 'warning'
    if (color === C.danger) return 'danger'
    if (color === C.purple) return 'purple'
    if (color === C.corrected) return 'corrected'
    if (color === C.ws) return 'ws'
    if (color === C.edgeNeutral) return 'edgeNeutral'
    return 'edge'
  }

  const filter = defs.append('filter').attr('id', 'node-shadow')
    .attr('x', '-20%').attr('y', '-20%').attr('width', '140%').attr('height', '140%')
  filter.append('feDropShadow')
    .attr('dx', 0).attr('dy', 2).attr('stdDeviation', 4)
    .attr('flood-color', isDark ? 'rgba(0,0,0,0.6)' : 'rgba(100,116,139,0.2)')

  const edgesG = zoomG.append('g').attr('class', 'edges')

  for (const edge of edges) {
    const src = nodes.find(n => n.id === edge.source)
    const tgt = nodes.find(n => n.id === edge.target)
    if (!src || !tgt) continue

    const color = edge.color ?? C.edge
    const arrowId = colorToArrowId(color)

    const dx = tgt.x - src.x
    const dy = tgt.y - src.y
    const isMainlyHoriz = Math.abs(dx) > Math.abs(dy)

    let x1: number, y1: number, x2: number, y2: number, path: string

    if (isMainlyHoriz) {
      x1 = src.x + BOX / 2; y1 = src.y
      x2 = tgt.x - BOX / 2; y2 = tgt.y
      const mx = (x1 + x2) / 2
      path = `M ${x1} ${y1} C ${mx} ${y1}, ${mx} ${y2}, ${x2} ${y2}`
    } else {
      x1 = src.x; y1 = src.y + BOX / 2
      x2 = tgt.x; y2 = tgt.y - BOX / 2
      const my = (y1 + y2) / 2
      path = `M ${x1} ${y1} C ${x1} ${my}, ${x2} ${my}, ${x2} ${y2 - 2}`
    }

    edgesG.append('path')
      .attr('d', path).attr('fill', 'none')
      .attr('stroke', color).attr('stroke-width', 1.5)
      .attr('stroke-dasharray', color === C.edge ? '5 4' : 'none')
      .attr('marker-end', `url(#ha-${arrowId})`)
      .attr('opacity', 0.75)

    if (edge.label) {
      const mx = (x1 + x2) / 2
      const my = (y1 + y2) / 2
      edgesG.append('text')
        .attr('x', mx + (isMainlyHoriz ? 0 : 6))
        .attr('y', isMainlyHoriz ? my - 8 : my - 4)
        .attr('text-anchor', 'middle')
        .attr('font-size', '9').attr('font-style', 'italic')
        .attr('fill', C.edgeLabel)
        .text(edge.label)
    }
  }

  const nodesG = zoomG.append('g').attr('class', 'nodes')

  const drag = d3.drag<SVGGElement, HNode>()
    .on('start', function() { d3.select(this).raise() })
    .on('drag', function(event, d) {
      d.x += event.dx; d.y += event.dy
      d3.select(this).attr('transform', `translate(${d.x - BOX / 2}, ${d.y - BOX / 2})`)
      redrawEdges()
    })

  const nodeMap = new Map<string, HNode>()
  nodes.forEach(n => nodeMap.set(n.id, n))

  function redrawEdges() {
    edgesG.selectAll('*').remove()

    for (const [name, color] of Object.entries(arrowDefs)) {
      zoomG.select('defs').append('marker')
        .attr('id', `ha-${name}`).attr('markerWidth', 8).attr('markerHeight', 8)
        .attr('refX', 5).attr('refY', 3).attr('orient', 'auto')
        .append('path').attr('d', 'M0,0 L0,6 L8,3 z').attr('fill', color)
    }

    for (const edge of edges) {
      const src = nodeMap.get(edge.source)
      const tgt = nodeMap.get(edge.target)
      if (!src || !tgt) continue

      const color = edge.color ?? C.edge
      const arrowId = colorToArrowId(color)
      const dx = tgt.x - src.x; const dy = tgt.y - src.y
      const isMainlyHoriz = Math.abs(dx) > Math.abs(dy)
      let rx1: number, ry1: number, rx2: number, ry2: number, path: string
      if (isMainlyHoriz) {
        rx1 = src.x + BOX / 2; ry1 = src.y
        rx2 = tgt.x - BOX / 2; ry2 = tgt.y
        const mx = (rx1 + rx2) / 2
        path = `M ${rx1} ${ry1} C ${mx} ${ry1}, ${mx} ${ry2}, ${rx2} ${ry2}`
      } else {
        rx1 = src.x; ry1 = src.y + BOX / 2
        rx2 = tgt.x; ry2 = tgt.y - BOX / 2
        const my = (ry1 + ry2) / 2
        path = `M ${rx1} ${ry1} C ${rx1} ${my}, ${rx2} ${my}, ${rx2} ${ry2 - 2}`
      }
      edgesG.append('path')
        .attr('d', path).attr('fill', 'none')
        .attr('stroke', color).attr('stroke-width', 1.5)
        .attr('stroke-dasharray', color === C.edge ? '5 4' : 'none')
        .attr('marker-end', `url(#ha-${arrowId})`).attr('opacity', 0.75)
      if (edge.label) {
        const mx = (rx1 + rx2) / 2; const my = (ry1 + ry2) / 2
        edgesG.append('text')
          .attr('x', mx + (isMainlyHoriz ? 0 : 6)).attr('y', isMainlyHoriz ? my - 8 : my - 4)
          .attr('text-anchor', 'middle')
          .attr('font-size', '9').attr('font-style', 'italic')
          .attr('fill', C.edgeLabel).text(edge.label)
      }
    }
  }

  const nodeGroups = nodesG.selectAll<SVGGElement, HNode>('g.node')
    .data(nodes).enter()
    .append('g').attr('class', 'node')
    .attr('transform', d => `translate(${d.x - BOX / 2}, ${d.y - BOX / 2})`)
    .attr('cursor', 'grab')
    .call(drag)

  nodeGroups.append('text')
    .attr('x', BOX / 2).attr('y', -10)
    .attr('text-anchor', 'middle')
    .attr('font-size', '10').attr('fill', C.typeLabel)
    .text(d => d.typeLabel)

  nodeGroups.filter(d => !!d.isActivePulse)
    .append('rect')
    .attr('x', -4).attr('y', -4)
    .attr('width', BOX + 8).attr('height', BOX + 8)
    .attr('rx', 16)
    .attr('fill', 'none')
    .attr('stroke', C.phase)
    .attr('stroke-width', 2)
    .attr('opacity', 0.6)
    .each(function() {
      const el = d3.select(this)
      el.append('animate')
        .attr('attributeName', 'opacity')
        .attr('values', '0.6;0.05;0.6')
        .attr('dur', '1.8s')
        .attr('repeatCount', 'indefinite')
      el.append('animate')
        .attr('attributeName', 'stroke-width')
        .attr('values', '2;3.5;2')
        .attr('dur', '1.8s')
        .attr('repeatCount', 'indefinite')
    })

  nodeGroups.append('rect')
    .attr('width', BOX).attr('height', BOX).attr('rx', 12)
    .attr('fill', C.nodeBg)
    .attr('stroke', d => (C as any)[d.type] ?? C.edge)
    .attr('stroke-width', d => d.isActivePulse ? 2.5 : 1.8)
    .attr('filter', 'url(#node-shadow)')

  nodeGroups.append('g')
    .attr('transform', `translate(${(BOX - ICON) / 2}, ${(BOX - ICON) / 2 - 8})`)
    .each(function(d) {
      const iconPath = ICONS[d.type] ?? ICONS.phase
      const color = (C as any)[d.type] ?? C.edge
      d3.select(this).append('path')
        .attr('d', iconPath)
        .attr('fill', color)
        .attr('transform', `scale(${ICON / 24})`)
        .attr('opacity', 0.85)
    })

  nodeGroups.append('text')
    .attr('x', BOX / 2).attr('y', BOX - 18)
    .attr('text-anchor', 'middle')
    .attr('font-size', '9').attr('font-weight', '600')
    .attr('fill', d => (C as any)[d.type] ?? C.nodeText)
    .each(function(d) {
      const el = d3.select(this)
      const label = d.nameLabel
      if (label.length > 14) {
        el.append('tspan').attr('x', BOX / 2).attr('dy', '0').text(label.slice(0, 13) + '…')
      } else {
        el.text(label)
      }
    })

  nodeGroups.filter(d => !!d.subLabel)
    .append('text')
    .attr('x', BOX / 2).attr('y', BOX - 6)
    .attr('text-anchor', 'middle')
    .attr('font-size', '8').attr('fill', C.nodeSubText)
    .text(d => d.subLabel!.length > 16 ? d.subLabel!.slice(0, 15) + '…' : d.subLabel!)

  nodeGroups.filter(d => !!d.statusDot)
    .append('circle')
    .attr('cx', BOX - 8).attr('cy', 8).attr('r', 5)
    .attr('fill', d => d.statusDot!)
    .attr('stroke', C.nodeBg).attr('stroke-width', 1.5)

  nodeGroups.filter(d => d.type === 'phase' && (d.occurrenceCount ?? 0) > 1)
    .append('g')
    .each(function(d) {
      const g = d3.select(this)
      g.append('circle').attr('cx', 10).attr('cy', 10).attr('r', 9)
        .attr('fill', C.warning).attr('stroke', C.nodeBg).attr('stroke-width', 1.5)
      g.append('text').attr('x', 10).attr('y', 10)
        .attr('text-anchor', 'middle').attr('dominant-baseline', 'middle')
        .attr('font-size', '8').attr('font-weight', '700').attr('fill', '#fff')
        .text(`${d.occurrenceCount}×`)
    })

  nodeGroups
    .on('mouseenter', function(event: MouseEvent, d: HNode) {
      if (!d.tooltipRows?.length) return
      tooltip.value = { visible: true, screenX: event.clientX, screenY: event.clientY, title: d.nameLabel, rows: d.tooltipRows }
    })
    .on('mousemove', function(event: MouseEvent) {
      tooltip.value.screenX = event.clientX; tooltip.value.screenY = event.clientY
    })
    .on('mouseleave', () => { tooltip.value.visible = false })

  const scaleX = (viewW - 40) / canvasW
  const scaleY = (viewH - 40) / canvasH
  const initialScale = Math.min(scaleX, scaleY, 1)
  const tx = (viewW - canvasW * initialScale) / 2
  const ty = (viewH - canvasH * initialScale) / 2
  const initialTransform = d3.zoomIdentity.translate(tx, ty).scale(initialScale)
  d3.select(svgEl).call(zoom.transform, initialTransform)

  savedZoomBehavior = zoom
  savedInitialTransform = initialTransform
}

// ── Helpers de sequência ───────────────────────────────────────────────────────
function getExpectedIndex(i: number): number {
  const phase = timeline.value[i]
  if (!phase || !phaseSequence.value.length) return -1
  return phaseSequence.value.findIndex(s => s.manufacturingPhaseId === phase.manufacturingPhaseId)
}
function expectedOrderLabel(i: number): string | null {
  const ei = getExpectedIndex(i)
  return ei === -1 ? null : `${phaseSequence.value[ei].order}`
}

// Conjunto de 'order' já visitados (0..i inclusive)
function visitedOrdersUpTo(i: number): Set<number> {
  const visited = new Set<number>()
  for (let k = 0; k <= i; k++) {
    const idx = getExpectedIndex(k)
    if (idx !== -1) visited.add(phaseSequence.value[idx].order)
  }
  return visited
}

// Classifica a transição que LEVOU à fase no índice i, com base no histórico 0..i-1
type SeqClass = 'first' | 'correct' | 'repeated' | 'corrected' | 'skipped1' | 'skippedN' | 'unknown'
function classifyTransition(i: number): SeqClass {
  if (i <= 0) return 'first'

  const currIdx = getExpectedIndex(i)
  if (currIdx === -1) return 'unknown'
  const currOrder = phaseSequence.value[currIdx].order

  const visitedBefore = visitedOrdersUpTo(i - 1)
  if (visitedBefore.size === 0) return 'unknown'

  const maxVisited = Math.max(...visitedBefore)

  if (visitedBefore.has(currOrder)) return 'repeated'
  if (currOrder === maxVisited + 1) return 'correct'
  if (currOrder < maxVisited) return 'corrected'
  if (currOrder === maxVisited + 2) return 'skipped1'
  if (currOrder > maxVisited + 2) return 'skippedN'
  return 'unknown'
}

function sequenceStatus(i: number): SeqClass {
  return classifyTransition(i)
}
function transitionStatus(i: number): SeqClass {
  return classifyTransition(i + 1)
}
function connectorClass(i: number) {
  const s = transitionStatus(i)
  return {
    correct: 'bg-success-500', repeated: 'bg-warning-500', corrected: 'bg-blue-500',
    skipped1: 'bg-danger-500', skippedN: 'bg-purple-500', unknown: 'bg-background-400', first: 'bg-background-400',
  }[s] ?? 'bg-background-400'
}
function connectorTextClass(i: number) {
  const s = transitionStatus(i)
  return {
    correct: 'text-success-600', repeated: 'text-warning-600', corrected: 'text-blue-600',
    skipped1: 'text-danger-600', skippedN: 'text-purple-600', unknown: 'text-background-400', first: 'text-background-400',
  }[s] ?? 'text-background-400'
}
function connectorLabel(i: number) {
  const s = transitionStatus(i)
  return t(`timeline.transition.${s === 'first' ? 'unknown' : s}`)
}

// ── Formatação ─────────────────────────────────────────────────────────────────
function formatDate(date: string | null) {
  if (!date) return '—'
  const n = date.endsWith('Z') ? date : date + 'Z'
  return new Date(n).toLocaleString('pt-PT')
}
function formatDuration(seconds: number | null) {
  if (seconds === null || seconds === undefined) return t('timeline.inProgress')
  return `${Math.floor(seconds / 60)}m ${seconds % 60}s`
}
function resultLabel(result: string | null) {
  switch (result) {
    case 'passed': return t('timeline.result.passed')
    case 'failed_rework': return t('timeline.result.rework')
    case 'failed_scrapped': return t('timeline.result.scrapped')
    default: return result || ''
  }
}

onMounted(() => {
  const queryId = route.query.id
  if (queryId && !isNaN(Number(queryId))) {
    productId.value = Number(queryId)
  }
  loadTimeline()
})

let resizeTimeout: ReturnType<typeof setTimeout> | null = null
function handleResize() {
  if (viewMode.value !== 'graph') return
  if (resizeTimeout) clearTimeout(resizeTimeout)
  resizeTimeout = setTimeout(() => initHierarchyGraph(), 200)
}
onMounted(() => window.addEventListener('resize', handleResize))
onUnmounted(() => window.removeEventListener('resize', handleResize))
</script>