<template>
  <div class="p-8 w-full">

    <!-- Header -->
    <div class="flex items-start justify-between mb-8">
      <div>
        <h1 class="text-2xl font-medium text-background-900 dark:text-background-50">
          {{ t('presence.title') }}
        </h1>
        <p class="text-sm text-background-600 dark:text-background-400 mt-1">
          {{ t('presence.subtitle') }}
        </p>
      </div>
    </div>

    <!-- Loading inicial -->
    <div v-if="loadingWorkstations" class="flex items-center gap-2 text-background-500 text-sm py-12">
      <span class="material-symbols-rounded animate-spin text-lg">autorenew</span>
      {{ t('common.loading') }}
    </div>

    <div v-else class="grid grid-cols-1 lg:grid-cols-3 gap-6">

      <!-- ── Coluna esquerda: lista de workstations elegíveis ── -->
      <div class="lg:col-span-1 flex flex-col gap-3">
        <h2 class="text-sm font-semibold uppercase tracking-wider text-background-500 dark:text-background-400">
          {{ t('presence.workstations') }}
        </h2>

        <div v-if="workstations.length === 0" class="text-sm text-background-400 py-4">
          {{ t('presence.noWorkstations') }}
        </div>

        <button
          v-for="ws in workstations"
          :key="ws.id"
          @click="selectWorkstation(ws)"
          class="flex items-center gap-3 px-4 py-3 rounded-xl border transition-all text-left"
          :class="selectedWorkstation?.id === ws.id
            ? 'border-primary-500 bg-primary-50 dark:bg-primary-950 text-primary-700 dark:text-primary-300'
            : 'border-background-200 dark:border-background-700 bg-background-50 dark:bg-background-800 hover:bg-background-100 dark:hover:bg-background-750'"
        >
          <span
            class="material-symbols-rounded text-xl"
            :class="ws.kind === 'human' ? 'text-primary-500' : 'text-warning-500'"
          >
            {{ ws.kind === 'human' ? 'person' : 'people' }}
          </span>
          <div class="flex-1 min-w-0">
            <div class="text-sm font-medium truncate">
              WS {{ ws.type }} — {{ ws.phaseName ?? '—' }}
            </div>
            <div class="text-xs text-background-500 dark:text-background-400 truncate">
              {{ ws.productionLineName }}
            </div>
          </div>
          <span
            class="text-xs font-medium px-2 py-0.5 rounded-full shrink-0"
            :class="ws.kind === 'human'
              ? 'bg-primary-100 text-primary-600 dark:bg-primary-950 dark:text-primary-300'
              : 'bg-warning-100 text-warning-700 dark:bg-warning-950 dark:text-warning-300'"
          >
            {{ ws.kind === 'human' ? t('presence.kindHuman') : t('presence.kindHybrid') }}
          </span>
        </button>
      </div>

      <!-- ── Coluna direita: detalhe da workstation selecionada ── -->
      <div class="lg:col-span-2 flex flex-col gap-6">

        <!-- Placeholder quando nada selecionado -->
        <div v-if="!selectedWorkstation" class="flex flex-col items-center justify-center py-24 text-background-400 dark:text-background-600">
          <span class="material-symbols-rounded text-5xl mb-3">touch_app</span>
          <p class="text-sm">{{ t('presence.selectPrompt') }}</p>
        </div>

        <template v-else>

          <!-- Cabeçalho workstation -->
          <div class="flex items-center justify-between">
            <div>
              <h2 class="text-lg font-semibold text-background-800 dark:text-background-100">
                WS {{ selectedWorkstation.type }} — {{ selectedWorkstation.phaseName ?? '—' }}
              </h2>
              <p class="text-sm text-background-500 dark:text-background-400">
                {{ selectedWorkstation.productionLineName }}
              </p>
            </div>

            <!-- Botão Check-in / Check-out -->
            <div v-if="activePresence">
              <button
                @click="doCheckOut"
                :disabled="actionLoading"
                class="flex items-center gap-2 bg-danger-500 hover:bg-danger-600 text-white text-sm font-medium px-4 py-2 rounded-lg transition-colors disabled:opacity-50"
              >
                <span class="material-symbols-rounded text-base">logout</span>
                {{ t('presence.checkout') }}
              </button>
            </div>
            <div v-else>
              <button
                @click="doCheckIn"
                :disabled="actionLoading"
                class="flex items-center gap-2 bg-primary-500 hover:bg-primary-600 text-white text-sm font-medium px-4 py-2 rounded-lg transition-colors disabled:opacity-50"
              >
                <span class="material-symbols-rounded text-base">login</span>
                {{ t('presence.checkin') }}
              </button>
            </div>
          </div>

          <!-- Banner presença ativa -->
          <div
            v-if="activePresence"
            class="flex items-center gap-3 px-4 py-3 rounded-xl bg-success-50 dark:bg-success-950 border border-success-200 dark:border-success-800"
          >
            <span class="material-symbols-rounded text-success-500 text-xl">badge</span>
            <div>
              <p class="text-sm font-medium text-success-700 dark:text-success-300">
                {{ t('presence.activeLabel') }}
              </p>
              <p class="text-xs text-success-600 dark:text-success-400">
                {{ t('presence.since') }} {{ formatDateTime(activePresence.checkedInAt) }}
              </p>
            </div>
          </div>

          <!-- Erro de ação -->
          <div
            v-if="actionError"
            class="flex items-center gap-3 px-4 py-3 rounded-xl bg-danger-50 dark:bg-danger-950 border border-danger-200 dark:border-danger-800"
          >
            <span class="material-symbols-rounded text-danger-500 text-xl">error</span>
            <p class="text-sm text-danger-700 dark:text-danger-300">{{ actionError }}</p>
          </div>

          <!-- Histórico de presenças -->
          <div>
            <h3 class="text-sm font-semibold uppercase tracking-wider text-background-500 dark:text-background-400 mb-3">
              {{ t('presence.history') }}
            </h3>

            <div v-if="loadingHistory" class="flex items-center gap-2 text-background-400 text-sm py-6">
              <span class="material-symbols-rounded animate-spin text-base">autorenew</span>
              {{ t('common.loading') }}
            </div>

            <div v-else-if="history.length === 0" class="text-sm text-background-400 py-6 text-center">
              <span class="material-symbols-rounded text-3xl block mb-1">history</span>
              {{ t('presence.emptyHistory') }}
            </div>

            <div v-else class="flex flex-col gap-3">
              <div
                v-for="entry in history"
                :key="entry.id"
                class="border border-background-200 dark:border-background-700 rounded-xl overflow-hidden"
              >
                <!-- Header da entrada -->
                <div class="flex items-center justify-between px-4 py-3 bg-background-50 dark:bg-background-800">
                  <div class="flex items-center gap-3">
                    <div class="w-8 h-8 rounded-full bg-primary-100 dark:bg-primary-950 flex items-center justify-center flex-shrink-0">
                      <span class="text-xs font-semibold text-primary-600 dark:text-primary-300">
                        {{ initials(entry.userName) }}
                      </span>
                    </div>
                    <div>
                      <p class="text-sm font-medium text-background-800 dark:text-background-100">{{ entry.userName }}</p>
                      <p class="text-xs text-background-500 dark:text-background-400">{{ entry.userEmail }}</p>
                    </div>
                  </div>
                  <div class="text-right">
                    <div class="flex items-center gap-1 text-xs text-background-500 dark:text-background-400">
                      <span class="material-symbols-rounded text-sm">login</span>
                      {{ formatDateTime(entry.checkedInAt) }}
                    </div>
                    <div v-if="entry.checkedOutAt" class="flex items-center gap-1 text-xs text-background-400 dark:text-background-500 justify-end">
                      <span class="material-symbols-rounded text-sm">logout</span>
                      {{ formatDateTime(entry.checkedOutAt) }}
                    </div>
                    <div v-else class="text-xs font-medium text-success-600 dark:text-success-400">
                      {{ t('presence.activeNow') }}
                    </div>
                  </div>
                </div>

                <!-- Produtos cruzados -->
                <div v-if="entry.productsDuringPresence?.length" class="px-4 py-2 border-t border-background-100 dark:border-background-700">
                  <p class="text-xs font-medium text-background-500 uppercase tracking-wider mb-2">
                    {{ t('presence.productsPresent') }}
                  </p>
                  <div class="flex flex-wrap gap-2">
                    <span
                      v-for="prod in entry.productsDuringPresence"
                      :key="prod.productId"
                      class="text-xs px-2 py-1 rounded-md bg-background-100 dark:bg-background-700 text-background-700 dark:text-background-300 font-mono"
                    >
                      {{ prod.serialNumber }}
                    </span>
                  </div>
                </div>
              </div>
            </div>
          </div>

        </template>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useI18n } from 'vue-i18n'
import { useAuthStore } from '@/stores/authStore'

const { t } = useI18n()
const auth = useAuthStore()

const API = import.meta.env.VITE_API_URL ?? 'http://localhost:8080/api'

// ─── Types ───────────────────────────────────────────────────────────────────

interface WorkstationDTO {
  id: number
  productionLineId: number
  productionLineName: string | null
  type: string | null
  kind: string | null
  manufacturingPhaseId: number | null
  phaseName: string | null
}

interface PresenceProductCross {
  productId: number
  serialNumber: string
  phaseStart: string
  phaseEnd: string | null
}

interface PresenceDetail {
  id: number
  appUserId: number
  userName: string
  userEmail: string
  workstationId: number
  workstationIdentifier: string | null
  phaseName: string | null
  checkedInAt: string
  checkedOutAt: string | null
  productsDuringPresence: PresenceProductCross[]
}

// ─── State ───────────────────────────────────────────────────────────────────

const workstations = ref<WorkstationDTO[]>([])
const selectedWorkstation = ref<WorkstationDTO | null>(null)
const history = ref<PresenceDetail[]>([])
const activePresence = ref<PresenceDetail | null>(null)

const loadingWorkstations = ref(true)
const loadingHistory = ref(false)
const actionLoading = ref(false)
const actionError = ref<string | null>(null)

// ─── Fetch ───────────────────────────────────────────────────────────────────

function headers() {
  return {
    'Content-Type': 'application/json',
    Authorization: `Bearer ${auth.token}`,
  }
}

function unwrap(data: unknown): unknown[] {
  if (Array.isArray(data)) return data
  if (data && typeof data === 'object' && '$values' in (data as object)) {
    return (data as { $values: unknown[] }).$values
  }
  return []
}

async function loadWorkstations() {
  loadingWorkstations.value = true
  try {
    const res = await fetch(`${API}/Workstation/human-eligible`, { headers: headers() })
    if (!res.ok) throw new Error()
    const data = await res.json()
    workstations.value = unwrap(data) as WorkstationDTO[]
  } catch {
    workstations.value = []
  } finally {
    loadingWorkstations.value = false
  }
}

async function selectWorkstation(ws: WorkstationDTO) {
  selectedWorkstation.value = ws
  actionError.value = null
  activePresence.value = null
  await Promise.all([loadHistory(ws.id), loadActive(ws.id)])
}

async function loadHistory(workstationId: number) {
  loadingHistory.value = true
  try {
    const res = await fetch(`${API}/WorkstationPresence/workstation/${workstationId}`, { headers: headers() })
    if (!res.ok) throw new Error()
    const data = await res.json()
    history.value = unwrap(data) as PresenceDetail[]
  } catch {
    history.value = []
  } finally {
    loadingHistory.value = false
  }
}

async function loadActive(workstationId: number) {
  try {
    const res = await fetch(`${API}/WorkstationPresence/active/${workstationId}`, { headers: headers() })
    if (res.status === 404) {
      activePresence.value = null
      return
    }
    if (!res.ok) throw new Error()
    activePresence.value = await res.json()
  } catch {
    activePresence.value = null
  }
}

// ─── Actions ─────────────────────────────────────────────────────────────────

async function doCheckIn() {
  if (!selectedWorkstation.value) return
  actionLoading.value = true
  actionError.value = null
  try {
    const res = await fetch(`${API}/WorkstationPresence/checkin`, {
      method: 'POST',
      headers: headers(),
      body: JSON.stringify({ workstationId: selectedWorkstation.value.id }),
    })
    if (!res.ok) {
      const err = await res.json().catch(() => ({}))
      actionError.value = err.message ?? t('presence.errorCheckin')
      return
    }
    await Promise.all([loadHistory(selectedWorkstation.value.id), loadActive(selectedWorkstation.value.id)])
  } catch {
    actionError.value = t('presence.errorCheckin')
  } finally {
    actionLoading.value = false
  }
}

async function doCheckOut() {
  if (!selectedWorkstation.value) return
  actionLoading.value = true
  actionError.value = null
  try {
    const res = await fetch(`${API}/WorkstationPresence/checkout/${selectedWorkstation.value.id}`, {
      method: 'PUT',
      headers: headers(),
    })
    if (!res.ok) {
      const err = await res.json().catch(() => ({}))
      actionError.value = err.message ?? t('presence.errorCheckout')
      return
    }
    await Promise.all([loadHistory(selectedWorkstation.value.id), loadActive(selectedWorkstation.value.id)])
  } catch {
    actionError.value = t('presence.errorCheckout')
  } finally {
    actionLoading.value = false
  }
}

// ─── Helpers ─────────────────────────────────────────────────────────────────

function initials(name: string): string {
  return name.split(' ').slice(0, 2).map(n => n[0]).join('').toUpperCase()
}

function formatDateTime(iso: string): string {
  return new Date(iso).toLocaleString('pt-PT', {
    day: '2-digit', month: '2-digit', year: 'numeric',
    hour: '2-digit', minute: '2-digit',
  })
}

// ─── Init ────────────────────────────────────────────────────────────────────

onMounted(loadWorkstations)
</script>