<template>
  <div class="p-8 w-full">

    <!-- Header -->
    <div class="flex items-start justify-between mb-8">
      <div>
        <h1 class="text-2xl font-medium text-background-900 dark:text-background-50">
          {{ t('wip.title') }}
        </h1>
        <p class="text-sm text-background-600 dark:text-background-400 mt-1">
          {{ t('wip.subtitle') }}
        </p>
      </div>
      <button
        @click="loadWip"
        class="flex items-center gap-2 bg-primary-500 hover:bg-primary-600 text-white text-sm font-medium px-4 py-2 rounded-lg transition-colors"
      >
        <span class="material-symbols-rounded text-base">refresh</span>
        {{ t('common.refresh') }}
      </button>
    </div>

    <!-- Loading -->
    <div v-if="loading" class="flex items-center gap-2 text-background-500 text-sm py-12">
      <span class="material-symbols-rounded animate-spin text-lg">autorenew</span>
      {{ t('common.loading') }}
    </div>

    <div v-else>
      <!-- KPI Cards -->
      <div class="grid grid-cols-5 gap-4 mb-8">
        <div class="bg-background-50 dark:bg-background-800 border border-background-300 dark:border-background-700 rounded-xl p-5">
          <p class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('wip.totalProducts') }}</p>
          <p class="text-3xl font-medium text-background-900 dark:text-background-50 mt-2">{{ summary.totalProducts }}</p>
        </div>
        <div class="bg-background-50 dark:bg-background-800 border border-warning-300 dark:border-warning-700 rounded-xl p-5">
          <p class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('wip.waiting') }}</p>
          <p class="text-3xl font-medium text-warning-500 mt-2">{{ summary.waiting }}</p>
        </div>
        <div class="bg-background-50 dark:bg-background-800 border border-background-300 dark:border-background-700 rounded-xl p-5">
          <p class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('wip.inProduction') }}</p>
          <p class="text-3xl font-medium text-primary-500 mt-2">{{ summary.inProgress }}</p>
        </div>
        <div class="bg-background-50 dark:bg-background-800 border border-background-300 dark:border-background-700 rounded-xl p-5">
          <p class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('wip.activeLines') }}</p>
          <p class="text-3xl font-medium text-success-500 mt-2">{{ summary.activeLines }}</p>
        </div>
        <div class="bg-background-50 dark:bg-background-800 border border-background-300 dark:border-background-700 rounded-xl p-5">
          <p class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('wip.completed') }}</p>
          <p class="text-3xl font-medium text-background-600 dark:text-background-300 mt-2">{{ summary.completed }}</p>
        </div>
      </div>

      <!-- Tabs -->
      <div class="flex items-center gap-2 mb-5">
        <button
          @click="viewMode = 'tables'"
          class="flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-sm font-medium transition-colors"
          :class="viewMode === 'tables'
            ? 'bg-primary-500 text-white'
            : 'bg-background-100 dark:bg-background-800 text-background-600 dark:text-background-400 hover:text-background-900 dark:hover:text-background-100'"
        >
          <span class="material-symbols-rounded text-base">table_rows</span>
          {{ t('wip.viewTables') }}
        </button>
        <button
          @click="viewMode = 'kanban'; kanbanOffset = 0"
          class="flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-sm font-medium transition-colors"
          :class="viewMode === 'kanban'
            ? 'bg-primary-500 text-white'
            : 'bg-background-100 dark:bg-background-800 text-background-600 dark:text-background-400 hover:text-background-900 dark:hover:text-background-100'"
        >
          <span class="material-symbols-rounded text-base">view_kanban</span>
          {{ t('wip.viewKanban') }}
        </button>
        <button
          @click="viewMode = 'graph'; nextTick(() => initGraph())"
          class="flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-sm font-medium transition-colors"
          :class="viewMode === 'graph'
            ? 'bg-primary-500 text-white'
            : 'bg-background-100 dark:bg-background-800 text-background-600 dark:text-background-400 hover:text-background-900 dark:hover:text-background-100'"
        >
          <span class="material-symbols-rounded text-base">hub</span>
          {{ t('wip.viewGraph') }}
        </button>
      </div>

      <!-- Tables View -->
      <div v-if="viewMode === 'tables'" class="flex flex-col gap-8">
        <section>
          <div class="flex items-center justify-between mb-3">
            <div>
              <h2 class="text-lg font-medium text-background-900 dark:text-background-50">
                {{ t('wip.queue.title') }}
              </h2>
              <p class="text-xs text-background-500 mt-0.5">{{ t('wip.queue.subtitle') }}</p>
            </div>
            <span class="text-xs font-medium px-2 py-1 rounded-full bg-warning-100 text-warning-700 dark:bg-warning-900 dark:text-warning-200">
              {{ summary.waiting }} {{ t('wip.queue.counter') }}
            </span>
          </div>
          <div class="border border-background-300 dark:border-background-700 rounded-xl overflow-hidden">
            <div class="grid grid-cols-7 px-4 py-3 bg-background-100 dark:bg-background-800 border-b border-background-300 dark:border-background-700">
              <span class="text-xs font-medium text-background-500 uppercase tracking-wider col-span-2">{{ t('wip.fields.product') }}</span>
              <span class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('wip.fields.model') }}</span>
              <span class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('wip.fields.order') }}</span>
              <span class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('wip.fields.support') }}</span>
              <span class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('wip.fields.nextPhase') }}</span>
              <span class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('wip.fields.reason') }}</span>
            </div>
            <div v-if="waitingItems.length === 0" class="text-center py-10 text-background-500">
              <span class="material-symbols-rounded text-4xl block mb-2">hourglass_empty</span>
              <p class="text-sm">{{ t('wip.queue.empty') }}</p>
            </div>
            <div
              v-for="item in waitingItems"
              :key="'waiting-' + item.productId"
              class="grid grid-cols-7 px-4 py-3 border-b border-background-200 dark:border-background-700 last:border-0 bg-background-50 dark:bg-background-800 hover:bg-background-100 dark:hover:bg-background-750 transition-colors items-center"
            >
              <div class="col-span-2">
                <button
                  @click="goToProduct(item.serialNumber)"
                  class="text-sm font-medium text-primary-600 dark:text-primary-400 hover:underline hover:text-primary-700 dark:hover:text-primary-300 transition-colors text-left"
                  :title="t('wip.goToTimeline')"
                >
                  {{ item.serialNumber }}
                </button>
                <div class="text-xs text-background-400 mt-0.5">ID #{{ item.productId }}</div>
              </div>
              <span class="text-sm text-background-600 dark:text-background-400">{{ item.modelName || '—' }}</span>
              <span class="text-sm text-background-600 dark:text-background-400">{{ item.manufacturingOrderNumber || '—' }}</span>
              <span class="text-sm text-background-600 dark:text-background-400">{{ item.rfidTag || '—' }}</span>
              <span class="text-sm text-background-600 dark:text-background-400">{{ item.nextPhase || '—' }}</span>
              <span class="text-xs font-medium px-2 py-1 rounded-full w-fit" :class="queueReasonClass(item.queueReason)">
                {{ queueReasonLabel(item.queueReason) }}
              </span>
            </div>
          </div>
        </section>

        <section>
          <div class="flex items-center justify-between mb-3">
            <div>
              <h2 class="text-lg font-medium text-background-900 dark:text-background-50">
                {{ t('wip.current.title') }}
              </h2>
              <p class="text-xs text-background-500 mt-0.5">{{ t('wip.current.subtitle') }}</p>
            </div>
            <span class="text-xs font-medium px-2 py-1 rounded-full bg-primary-50 text-primary-600 dark:bg-primary-950 dark:text-primary-300">
              {{ summary.inProgress }} {{ t('wip.current.counter') }}
            </span>
          </div>
          <div class="border border-background-300 dark:border-background-700 rounded-xl overflow-hidden">
            <div class="grid grid-cols-8 px-4 py-3 bg-background-100 dark:bg-background-800 border-b border-background-300 dark:border-background-700">
              <span class="text-xs font-medium text-background-500 uppercase tracking-wider col-span-2">{{ t('wip.fields.product') }}</span>
              <span class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('wip.fields.line') }}</span>
              <span class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('wip.fields.workstation') }}</span>
              <span class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('wip.fields.phase') }}</span>
              <span class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('wip.fields.start') }}</span>
              <span class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('wip.fields.elapsed') }}</span>
              <span class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('wip.fields.expected') }}</span>
            </div>
            <div v-if="items.length === 0" class="text-center py-10 text-background-500">
              <span class="material-symbols-rounded text-4xl block mb-2">inventory_2</span>
              <p class="text-sm">{{ t('wip.empty') }}</p>
            </div>
            <div
              v-for="item in items"
              :key="item.productId + '-' + item.currentPhase"
              class="grid grid-cols-8 px-4 py-3 border-b border-background-200 dark:border-background-700 last:border-0 bg-background-50 dark:bg-background-800 hover:bg-background-100 dark:hover:bg-background-750 transition-colors items-center"
            >
              <div class="col-span-2">
                <button
                  @click="goToProduct(item.serialNumber)"
                  class="text-sm font-medium text-primary-600 dark:text-primary-400 hover:underline hover:text-primary-700 dark:hover:text-primary-300 transition-colors text-left"
                  :title="t('wip.goToTimeline')"
                >
                  {{ item.serialNumber }}
                </button>
              </div>
              <span class="text-sm text-background-600 dark:text-background-400">{{ item.productionLineName }}</span>
              <span class="text-sm text-background-600 dark:text-background-400">{{ item.workstation }}</span>
              <span class="text-sm text-background-600 dark:text-background-400">{{ item.currentPhase }}</span>
              <span class="text-sm text-background-500">{{ formatDate(item.startedAt) }}</span>
              <span class="text-sm font-medium" :class="cardStatusDurationClass(item)">{{ formatDuration(liveElapsed(item)) }}</span>
              <span class="text-sm text-background-500 flex items-center gap-1.5">
                {{ formatDuration(referenceDuration(item)) }}
                <span
                  v-if="referenceDuration(item) !== null"
                  class="text-[10px] font-semibold px-1.5 py-0.5 rounded"
                  :class="item.predictedPhaseDurationIsMl
                    ? 'bg-primary-100 text-primary-700 dark:bg-primary-950 dark:text-primary-300'
                    : 'bg-background-200 text-background-500 dark:bg-background-700 dark:text-background-400'"
                >
                  {{ item.predictedPhaseDurationIsMl ? t('wip.mlBadge') : t('wip.baselineBadge') }}
                </span>
              </span>
            </div>
          </div>
        </section>
      </div>

      <!-- Kanban View -->
      <div
        v-else-if="viewMode === 'kanban'"
        ref="kanbanContainer"
        class="overflow-hidden pb-4 cursor-grab active:cursor-grabbing select-none"
        @mousedown="onKanbanMouseDown"
      >
        <div
          ref="kanbanInner"
          class="flex gap-4"
          :style="{ transform: `translateX(${kanbanOffset}px)`, transition: isDraggingKanban ? 'none' : 'transform 0.1s ease' }"
        >
          <div v-for="col in kanbanColumns" :key="col.id" class="flex-shrink-0 w-72">
            <!-- Column Header -->
            <div class="flex items-center justify-between mb-3 px-1">
              <div class="flex items-center gap-2">
                <span class="w-2.5 h-2.5 rounded-full" :class="col.type === 'waiting' ? 'bg-warning-500' : 'bg-primary-500'"></span>
                <span class="text-sm font-medium text-background-900 dark:text-background-50">{{ col.label }}</span>
              </div>
              <span class="text-xs font-medium px-2 py-0.5 rounded-full bg-background-200 dark:bg-background-700 text-background-600 dark:text-background-300">
                {{ col.items.length }}
              </span>
            </div>

            <!-- Cards -->
            <div class="flex flex-col gap-2">
              <!-- Waiting Cards -->
              <template v-if="col.type === 'waiting'">
                <div
                  v-for="item in (col.items as WaitingItem[])"
                  :key="'kanban-w-' + item.productId"
                  @click.stop="goToProduct(item.serialNumber)"
                  class="bg-background-50 dark:bg-background-800 border border-warning-300 dark:border-warning-700 rounded-lg p-3 cursor-pointer hover:shadow-md transition-shadow"
                >
                  <div class="text-xs font-medium text-warning-600 dark:text-warning-400 truncate">
                    {{ item.serialNumber }}
                  </div>
                  <div class="text-xs text-background-500 dark:text-background-400 mt-1">
                    {{ item.nextPhase ?? '—' }}
                  </div>
                  <div class="flex items-center justify-between mt-2">
                    <span class="text-xs px-1.5 py-0.5 rounded-full" :class="queueReasonClass(item.queueReason)">
                      {{ queueReasonLabel(item.queueReason) }}
                    </span>
                    <span class="text-xs text-background-400">ID #{{ item.productId }}</span>
                  </div>
                </div>
              </template>

              <!-- In Progress Cards -->
              <template v-else>
                <div
                  v-for="item in (col.items as WipItem[])"
                  :key="'kanban-p-' + item.productId"
                  @click.stop="goToProduct(item.serialNumber)"
                  class="rounded-lg p-3 cursor-pointer hover:shadow-md transition-shadow border"
                  :class="cardStatusClass(item)"
                >
                  <div class="flex items-center justify-between gap-1">
                    <span class="text-xs font-medium truncate" :class="cardStatusTextClass(item)">
                      {{ item.serialNumber }}
                    </span>
                    <span
                      v-if="openAlertProductIds.includes(item.productId)"
                      class="material-symbols-rounded text-sm text-red-500 flex-shrink-0"
                      :title="t('wip.kanban.hasAlert')"
                    >
                      notifications_active
                    </span>
                  </div>
                  <div class="text-xs text-background-500 dark:text-background-400 mt-1">
                    {{ item.currentPhase }}
                  </div>
                  <div class="flex items-center justify-between mt-2">
                    <span class="text-xs font-medium flex items-center gap-1" :class="cardStatusDurationClass(item)">
                      <span
                        v-if="item.predictedPhaseDurationSeconds !== null"
                        class="w-1.5 h-1.5 rounded-full flex-shrink-0"
                        :class="item.predictedPhaseDurationIsMl ? 'bg-primary-500' : 'bg-background-400'"
                        :title="item.predictedPhaseDurationIsMl ? t('wip.mlBadge') : t('wip.baselineBadge')"
                      ></span>
                      {{ cardDurationLabel(item) }}
                    </span>
                    <span class="text-xs text-background-400">ID #{{ item.productId }}</span>
                  </div>
                </div>
              </template>
            </div>
          </div>
        </div>
      </div>

      <!-- Graph View -->
      <div v-else class="flex flex-col gap-3">
        <div class="flex items-center justify-between gap-4 flex-wrap">
          <div class="flex items-center gap-5 flex-wrap">
            <span class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('wip.graph.legend') }}</span>
            <div class="flex items-center gap-1.5"><div class="w-3 h-3 rounded-full bg-warning-500"></div><span class="text-xs text-background-600 dark:text-background-400">{{ t('wip.waiting') }}</span></div>
            <div class="flex items-center gap-1.5"><div class="w-3 h-3 rounded-full bg-primary-500"></div><span class="text-xs text-background-600 dark:text-background-400">{{ t('wip.inProduction') }}</span></div>
            <div class="flex items-center gap-1.5"><div class="w-3 h-3 rounded-full bg-success-500"></div><span class="text-xs text-background-600 dark:text-background-400">{{ t('wip.graph.line') }}</span></div>
            <div class="flex items-center gap-1.5"><div class="w-3 h-3 rounded-full bg-background-500"></div><span class="text-xs text-background-600 dark:text-background-400">{{ t('wip.graph.workstation') }}</span></div>
          </div>
          <button
            @click="initGraph"
            class="flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-xs font-medium bg-background-100 dark:bg-background-800 border border-background-300 dark:border-background-700 text-background-600 dark:text-background-400 hover:text-background-900 dark:hover:text-background-100 transition-colors"
          >
            <span class="material-symbols-rounded text-sm">center_focus_strong</span>
            {{ t('wip.graph.recenter') }}
          </button>
        </div>
        <p class="text-xs text-background-400 px-1">
          <span class="material-symbols-rounded text-sm align-middle">pan_tool</span>
          {{ t('wip.graph.hint') }}
        </p>
        <div
          ref="graphContainer"
          class="relative w-full rounded-xl border border-background-300 dark:border-background-700 overflow-hidden"
          style="height: calc(100vh - 330px); min-height: 430px; background: transparent;"
        >
          <svg ref="graphSvg" class="w-full h-full" style="display:block;"></svg>
          <div
            v-if="tooltip.visible"
            class="fixed pointer-events-none z-50 bg-background-900 dark:bg-background-950 text-background-50 text-xs rounded-xl px-3 py-2.5 shadow-2xl border border-background-700 min-w-[160px]"
            :style="{ left: tooltip.screenX + 'px', top: tooltip.screenY + 'px', transform: 'translate(-50%, -110%)' }"
          >
            <div class="font-semibold mb-1 text-sm">{{ tooltip.title }}</div>
            <div class="text-background-300">{{ tooltip.subtitle }}</div>
          </div>
        </div>
      </div>
    </div>

  </div>
</template>

<script setup lang="ts">
import { computed, nextTick, onMounted, onUnmounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import * as d3 from 'd3'
import axios from '@/axios'
import { useI18n } from 'vue-i18n'
import { toast } from '@/plugins/toast'
import { useAlertStore } from '@/stores/alertStore'

const alertStore = useAlertStore()
const { t } = useI18n()
const router = useRouter()

type ViewMode = 'tables' | 'kanban' | 'graph'

type WipSummary = {
  totalProducts: number
  waiting: number
  inProgress: number
  completed: number
  activeLines: number
}

type WipItem = {
  productId: number
  serialNumber: string | null
  productionLineId: number
  productionLineName: string | null
  workstationId: number
  workstation: string | null
  currentPhase: string | null
  estimatedDuration: number | null
  timeThresholdPct: number | null
  startedAt: string | null
  elapsedSeconds: number | null
  predictedPhaseDurationSeconds: number | null
  predictedPhaseDurationIsMl: boolean
}

type WaitingItem = {
  productId: number
  serialNumber: string | null
  modelName: string | null
  manufacturingOrderNumber: string | null
  supportId: number | null
  rfidTag: string | null
  supportType: string | null
  productionLineName: string | null
  workstation: string | null
  lastPhase: string | null
  lastPhaseEndedAt: string | null
  nextPhase: string | null
  queueReason: string | null
}

type GraphNode = {
  id: string
  label: string
  type: string
  subtitle?: string | null
}

type GraphEdge = {
  source: string
  target: string
  type: string
}

type KanbanColumn = {
  id: string
  label: string
  type: 'waiting' | 'workstation'
  items: WaitingItem[] | WipItem[]
}

// ── State ────────────────────────────────────────────────────────────────────
const loading = ref(false)
const viewMode = ref<ViewMode>('tables')
const summary = ref<WipSummary>({
  totalProducts: 0,
  waiting: 0,
  inProgress: 0,
  completed: 0,
  activeLines: 0,
})
const items = ref<WipItem[]>([])
const waitingItems = ref<WaitingItem[]>([])
const graph = ref<{ nodes: GraphNode[]; edges: GraphEdge[] }>({ nodes: [], edges: [] })

// ── Graph refs ───────────────────────────────────────────────────────────────
const graphContainer = ref<HTMLDivElement>()
const graphSvg = ref<SVGSVGElement>()
const tooltip = ref({ visible: false, screenX: 0, screenY: 0, title: '', subtitle: '' })

// ── Kanban drag-to-pan ───────────────────────────────────────────────────────
const kanbanContainer = ref<HTMLDivElement>()
const kanbanInner = ref<HTMLDivElement>()
const kanbanOffset = ref(0)
let isDraggingKanban = false
let dragStartX = 0
let offsetAtDragStart = 0

function onKanbanMouseDown(e: MouseEvent) {
  isDraggingKanban = true
  dragStartX = e.clientX
  offsetAtDragStart = kanbanOffset.value

  const onMove = (e: MouseEvent) => {
    if (!isDraggingKanban || !kanbanContainer.value || !kanbanInner.value) return
    const delta = e.clientX - dragStartX
    const maxOffset = 0
    const minOffset = -(kanbanInner.value.scrollWidth - kanbanContainer.value.clientWidth)
    kanbanOffset.value = Math.min(maxOffset, Math.max(minOffset, offsetAtDragStart + delta))
  }

  const onUp = () => {
    isDraggingKanban = false
    window.removeEventListener('mousemove', onMove)
    window.removeEventListener('mouseup', onUp)
  }

  window.addEventListener('mousemove', onMove)
  window.addEventListener('mouseup', onUp)
}

// ── Alert computeds ──────────────────────────────────────────────────────────
const openAlertProductIds = computed<number[]>(() =>
  alertStore.activeAlerts?.filter(a => a.productId != null).map(a => a.productId) ?? []
)

const acknowledgedAlertProductIds = computed<number[]>(() =>
  [...(alertStore.acknowledgedProductIds ?? [])]
)

// ── Kanban columns ───────────────────────────────────────────────────────────
const kanbanColumns = computed<KanbanColumn[]>(() => {
  const waitingCol: KanbanColumn = {
    id: 'waiting',
    label: t('wip.queue.title'),
    type: 'waiting',
    items: waitingItems.value,
  }
  const wsMap = new Map<number, KanbanColumn>()
  for (const item of items.value) {
    if (!wsMap.has(item.workstationId)) {
      wsMap.set(item.workstationId, {
        id: `ws-${item.workstationId}`,
        label: item.workstation ?? `WS ${item.workstationId}`,
        type: 'workstation',
        items: [],
      })
    }
    ;(wsMap.get(item.workstationId)!.items as WipItem[]).push(item)
  }
  return [waitingCol, ...wsMap.values()]
})

// ── Live ticking ─────────────────────────────────────────────────────────────
// 'items' só muda quando há novo fetch (loadWip()). Sem isto, o elapsed
// mostrado nos cards e usado no cálculo de cardStatus (warning/critical)
// ficava parado no valor lido no momento do fetch — um produto podia passar
// do tempo limite enquanto a página estava aberta sem o card alguma vez
// mudar de cor, só após um refresh manual.
const nowTick = ref(Date.now())
const loadedAt = ref(Date.now())
let tickInterval: ReturnType<typeof setInterval> | null = null

function startTicking() {
  if (tickInterval) return
  tickInterval = setInterval(() => { nowTick.value = Date.now() }, 1000)
}

function stopTicking() {
  if (tickInterval) { clearInterval(tickInterval); tickInterval = null }
}

function liveElapsed(item: WipItem): number {
  const base = item.elapsedSeconds ?? 0
  const sinceLoad = Math.floor((nowTick.value - loadedAt.value) / 1000)
  return base + Math.max(0, sinceLoad)
}

// Duração de referência para esta fase: prioriza a previsão por regressão
// (histórico real, vinda do EtaPredictionService); só cai para a estimativa
// estática se ainda não houver coeficientes treinados para esta fase.
function referenceDuration(item: WipItem): number | null {
  return item.predictedPhaseDurationSeconds ?? item.estimatedDuration
}

// Kanban: mesmo espaço, texto diferente — em vez de uma linha extra, troca
// "elapsed normal" por "Atrasado Xm" quando a fase já passou da duração de
// referência, reaproveitando a cor já calculada em cardStatusDurationClass.
function cardDurationLabel(item: WipItem): string {
  const elapsed = liveElapsed(item)
  const reference = referenceDuration(item)

  if (reference && elapsed > reference) {
    return `${t('wip.late')} ${formatDuration(elapsed - reference)}`
  }

  return formatDuration(elapsed)
}

// ── Card status ──────────────────────────────────────────────────────────────
function cardStatus(item: WipItem): 'critical-open' | 'critical-ack' | 'warning' | 'normal' {
  const elapsed = liveElapsed(item)
  const estimated = referenceDuration(item)
  const thresholdPct = item.timeThresholdPct ?? 150

  const isOpen = openAlertProductIds.value?.includes(item.productId) ?? false
  const isAck = acknowledgedAlertProductIds.value?.includes(item.productId) ?? false

  if (isOpen) return 'critical-open'
  if (isAck) return 'critical-ack'

  if (estimated) {
    const limitSeconds = estimated * (thresholdPct / 100)
    if (elapsed >= limitSeconds) return 'critical-ack'
    if (elapsed > estimated) return 'warning'
  }

  return 'normal'
}

function cardStatusClass(item: WipItem): string {
  const status = cardStatus(item)
  if (status === 'critical-open' || status === 'critical-ack')
    return 'bg-red-50 dark:bg-red-950 border-red-300 dark:border-red-700'
  if (status === 'warning')
    return 'bg-orange-50 dark:bg-orange-950 border-orange-300 dark:border-orange-700'
  return 'bg-background-50 dark:bg-background-800 border-background-300 dark:border-background-700'
}

function cardStatusTextClass(item: WipItem): string {
  const status = cardStatus(item)
  if (status === 'critical-open' || status === 'critical-ack') return 'text-red-600 dark:text-red-400'
  if (status === 'warning') return 'text-orange-600 dark:text-orange-400'
  return 'text-primary-600 dark:text-primary-400'
}

function cardStatusDurationClass(item: WipItem): string {
  const status = cardStatus(item)
  if (status === 'critical-open' || status === 'critical-ack') return 'text-red-500'
  if (status === 'warning') return 'text-orange-500'
  return 'text-background-400'
}

// ── Data loading ─────────────────────────────────────────────────────────────
async function loadWip() {
  loading.value = true
  try {
    const response = await axios.get('/production-lines/wip')

    summary.value = {
      totalProducts: response.data.totalProducts ?? 0,
      waiting: response.data.waiting ?? 0,
      inProgress: response.data.inProgress ?? 0,
      completed: response.data.completed ?? 0,
      activeLines: response.data.activeLines ?? 0,
    }

    waitingItems.value = response.data.waitingItems?.$values ?? response.data.waitingItems ?? []
    items.value = response.data.items?.$values ?? response.data.items ?? []
    loadedAt.value = Date.now()
    nowTick.value = Date.now()

    kanbanOffset.value = 0

    const graphData = response.data.graph ?? {}
    graph.value = {
      nodes: graphData.nodes?.$values ?? graphData.nodes ?? [],
      edges: graphData.edges?.$values ?? graphData.edges ?? [],
    }

    if (viewMode.value === 'graph') {
      await nextTick()
      initGraph()
    }
  } catch {
    toast.error(t('errors.loadFailed'))
  } finally {
    loading.value = false
  }
}

// ── Helpers ──────────────────────────────────────────────────────────────────
function goToProduct(serialNumber: string | null) {
  if (!serialNumber) return

  router.push({
    name: 'ProductTimeline',
    query: { vin: serialNumber }
  })
}

function formatDate(date: string | null) {
  if (!date) return '—'
  const normalized = date.endsWith('Z') ? date : date + 'Z'
  return new Date(normalized).toLocaleString('pt-PT')
}

function formatDuration(seconds: number | null) {
  if (seconds === null || seconds === undefined) return '—'
  const hours = Math.floor(seconds / 3600)
  const minutes = Math.floor((seconds % 3600) / 60)
  const remainingSeconds = seconds % 60
  if (hours > 0) return `${hours}h ${minutes}m`
  if (minutes > 0) return `${minutes}m ${remainingSeconds}s`
  return `${remainingSeconds}s`
}

function queueReasonLabel(reason: string | null) {
  if (reason === 'waiting_support') return t('wip.queue.reason.support')
  if (reason === 'waiting_line') return t('wip.queue.reason.line')
  if (reason === 'waiting_next_phase') return t('wip.queue.reason.nextPhase')
  return t('wip.queue.reason.unknown')
}

function queueReasonClass(reason: string | null) {
  if (reason === 'waiting_support') return 'bg-warning-100 text-warning-700 dark:bg-warning-900 dark:text-warning-200'
  if (reason === 'waiting_line') return 'bg-primary-50 text-primary-600 dark:bg-primary-950 dark:text-primary-300'
  if (reason === 'waiting_next_phase') return 'bg-background-200 text-background-700 dark:bg-background-700 dark:text-background-200'
  return 'bg-background-200 text-background-600 dark:bg-background-700 dark:text-background-300'
}

// ── Graph ────────────────────────────────────────────────────────────────────
function nodeColor(type: string, isDark: boolean) {
  switch (type) {
    case 'factory': return '#3E55F2'
    case 'waitingProduct': return '#eab308'
    case 'activeProduct': return '#3E55F2'
    case 'line': return '#22c55e'
    case 'workstation': return isDark ? '#94a3b8' : '#64748b'
    case 'support': return '#f59e0b'
    case 'phase': return '#a855f7'
    default: return isDark ? '#64748b' : '#94a3b8'
  }
}

function initGraph() {
  if (!graphSvg.value || !graphContainer.value) return

  const svgEl = graphSvg.value
  const container = graphContainer.value
  const width = container.clientWidth
  const height = container.clientHeight
  const isDark = document.documentElement.classList.contains('dark')

  const nodes = graph.value.nodes.map(n => ({ ...n }))
  const links = graph.value.edges.map(e => ({ ...e }))

  const bg = isDark ? '#0f172a' : '#f8fafc'
  const text = isDark ? '#f8fafc' : '#0f172a'
  const muted = isDark ? '#94a3b8' : '#64748b'
  const edge = isDark ? '#475569' : '#cbd5e1'

  const svg = d3.select(svgEl)
  svg.selectAll('*').remove()
  svg.attr('width', width).attr('height', height)
  svg.append('rect').attr('width', width).attr('height', height).attr('fill', bg)

  const zoomLayer = svg.append('g')
  const zoom = d3.zoom<SVGSVGElement, unknown>()
    .scaleExtent([0.3, 2.5])
    .on('zoom', event => { zoomLayer.attr('transform', event.transform) })
  svg.call(zoom)

  const simulation = d3.forceSimulation<any>(nodes)
    .force('link', d3.forceLink<any, any>(links).id(d => d.id).distance(135))
    .force('charge', d3.forceManyBody().strength(-420))
    .force('center', d3.forceCenter(width / 2, height / 2))
    .force('collide', d3.forceCollide(58))

  const link = zoomLayer.append('g')
    .selectAll('line').data(links).enter().append('line')
    .attr('stroke', edge).attr('stroke-width', 1.5).attr('stroke-opacity', 0.8)

  const node = zoomLayer.append('g')
    .selectAll<SVGGElement, any>('g').data(nodes).enter().append('g')
    .attr('cursor', 'grab')
    .call(
      d3.drag<SVGGElement, any>()
        .on('start', (event, d) => { if (!event.active) simulation.alphaTarget(0.3).restart(); d.fx = d.x; d.fy = d.y })
        .on('drag', (event, d) => { d.fx = event.x; d.fy = event.y })
        .on('end', (event, d) => { if (!event.active) simulation.alphaTarget(0); d.fx = null; d.fy = null })
    )

  node.append('circle')
    .attr('r', 28)
    .attr('fill', d => nodeColor(d.type, isDark))
    .attr('stroke', isDark ? '#1e293b' : '#ffffff')
    .attr('stroke-width', 3)

  node.append('text')
    .attr('text-anchor', 'middle').attr('dy', 4).attr('font-size', 18)
    .attr('fill', '#ffffff').attr('font-family', 'Material Symbols Rounded')
    .text(d => {
      if (d.type === 'factory') return 'factory'
      if (d.type === 'waitingProduct' || d.type === 'activeProduct') return 'directions_car'
      if (d.type === 'line') return 'precision_manufacturing'
      if (d.type === 'workstation') return 'settings'
      if (d.type === 'support') return 'local_shipping'
      if (d.type === 'phase') return 'timeline'
      return 'circle'
    })

  node.append('text')
    .attr('text-anchor', 'middle').attr('y', 47).attr('font-size', 11).attr('font-weight', 600).attr('fill', text)
    .text(d => d.label.length > 18 ? d.label.slice(0, 17) + '…' : d.label)

  node.append('text')
    .attr('text-anchor', 'middle').attr('y', 61).attr('font-size', 9).attr('fill', muted)
    .text(d => d.subtitle ? (d.subtitle.length > 20 ? d.subtitle.slice(0, 19) + '…' : d.subtitle) : '')

  node
    .on('mouseenter', (event: MouseEvent, d: GraphNode) => {
      tooltip.value = { visible: true, screenX: event.clientX, screenY: event.clientY, title: d.label, subtitle: d.subtitle || d.type }
    })
    .on('mousemove', (event: MouseEvent) => {
      tooltip.value.screenX = event.clientX
      tooltip.value.screenY = event.clientY
    })
    .on('mouseleave', () => { tooltip.value.visible = false })

  simulation.on('tick', () => {
    link
      .attr('x1', d => (d.source as any).x).attr('y1', d => (d.source as any).y)
      .attr('x2', d => (d.target as any).x).attr('y2', d => (d.target as any).y)
    node.attr('transform', d => `translate(${d.x},${d.y})`)
  })
}

// ── Lifecycle ─────────────────────────────────────────────────────────────────
let resizeTimeout: ReturnType<typeof setTimeout> | null = null

function handleResize() {
  if (viewMode.value !== 'graph') return
  if (resizeTimeout) clearTimeout(resizeTimeout)
  resizeTimeout = setTimeout(() => initGraph(), 200)
}

onMounted(() => {
  loadWip()
  window.addEventListener('resize', handleResize)
  startTicking()
})

onUnmounted(() => {
  window.removeEventListener('resize', handleResize)
  stopTicking()
})
</script>