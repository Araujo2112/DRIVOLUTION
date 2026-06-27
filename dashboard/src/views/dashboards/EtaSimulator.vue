<template>
  <div class="p-8 w-full">

    <!-- Header -->
    <div class="mb-8">
      <h1 class="text-2xl font-medium text-background-900 dark:text-background-50">
        {{ t('etaSimulation.title') }}
      </h1>
      <p class="text-sm text-background-600 dark:text-background-400 mt-1">
        {{ t('etaSimulation.subtitle') }}
      </p>
    </div>

    <div class="flex flex-col lg:flex-row gap-6">

      <!-- ── Painel de escolha (modelo + opções) ──────────────────────────── -->
      <div class="lg:w-[640px] flex-shrink-0 flex flex-col gap-4">

        <!-- Modelo -->
        <div class="bg-background-50 dark:bg-background-800 border border-background-300 dark:border-background-700 rounded-xl p-5">
          <label class="text-xs font-medium text-background-500 uppercase tracking-wider mb-2 block">
            {{ t('etaSimulation.model') }}
          </label>
          <select v-model="selectedModelId" @change="onModelChange" class="max-w-sm">
            <option :value="0" disabled>{{ t('etaSimulation.modelPlaceholder') }}</option>
            <option v-for="model in models" :key="model.id" :value="model.id">
              {{ model.name }}{{ model.version ? ` (${model.version})` : '' }}
            </option>
          </select>
        </div>

        <!-- Loading configs -->
        <div v-if="configsLoading" class="flex items-center gap-2 text-background-500 text-sm py-6 justify-center">
          <span class="material-symbols-rounded animate-spin text-lg">autorenew</span>
          {{ t('common.loading') }}
        </div>

        <!-- Sem modelo escolhido -->
        <div v-else-if="!selectedModelId" class="text-center py-12 text-background-400">
          <span class="material-symbols-rounded text-4xl block mb-2">directions_car</span>
          <p class="text-sm">{{ t('etaSimulation.chooseModelFirst') }}</p>
        </div>

        <!-- Sem configurações para o modelo -->
        <div v-else-if="configs.length === 0" class="text-center py-12 text-background-400">
          <p class="text-sm">{{ t('etaSimulation.noConfigsForModel') }}</p>
        </div>

        <!-- Opções de configuração, agrupadas por categoria.
             Grid de 2 colunas reduz a altura total quando o modelo tem muitas
             categorias (ex. 6 categorias passam de 6 a empilhar para 3 linhas). -->
        <div v-else class="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <div
            v-for="config in configs"
            :key="config.id"
            class="bg-background-50 dark:bg-background-800 border border-background-300 dark:border-background-700 rounded-xl p-5"
          >
            <div class="flex items-center justify-between mb-3 gap-2">
              <span class="text-sm font-medium text-background-800 dark:text-background-100">{{ config.item }}</span>
              <span class="text-xs text-background-400 whitespace-nowrap">
                {{ config.allowMultiple ? t('etaSimulation.multipleAllowed') : t('etaSimulation.singleChoice') }}
              </span>
            </div>

            <div v-if="!optionsByConfig[config.id]?.length" class="text-xs text-background-400">
              {{ t('etaSimulation.noOptionsForConfig') }}
            </div>

            <!-- Chips em vez de lista vertical com rádio: ocupam muito menos
                 altura quando há várias categorias com várias opções, e evitam
                 o desalinhamento de um input nativo contra texto em 2 linhas. -->
            <div v-else class="flex flex-wrap gap-2">
              <button
                v-for="option in optionsByConfig[config.id]"
                :key="option.id"
                type="button"
                @click="toggleOption(config, option.id)"
                class="text-sm px-3 py-1.5 rounded-full border transition-colors"
                :class="selectedOptionIds.has(option.id)
                  ? 'bg-primary-50 dark:bg-primary-950 border-primary-300 dark:border-primary-700 text-primary-700 dark:text-primary-300 font-medium'
                  : 'bg-background-100 dark:bg-background-700 border-background-300 dark:border-background-600 text-background-700 dark:text-background-300 hover:border-background-400 dark:hover:border-background-500'"
              >
                {{ option.value }}
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- ── Resultado da simulação ────────────────────────────────────────── -->
      <div class="flex-1">
        <div v-if="!selectedModelId" class="h-full flex items-center justify-center text-background-400 py-16">
          <p class="text-sm">{{ t('etaSimulation.resultPlaceholder') }}</p>
        </div>

        <div v-else-if="simulationLoading" class="flex items-center gap-2 text-background-500 text-sm py-16 justify-center">
          <span class="material-symbols-rounded animate-spin text-lg">autorenew</span>
          {{ t('common.loading') }}
        </div>

        <div v-else-if="simulationError" class="bg-danger-50 dark:bg-background-800 border border-danger-200 dark:border-danger-800 rounded-xl p-5 text-danger-600 dark:text-danger-400 text-sm">
          {{ simulationError }}
        </div>

        <div v-else-if="result" class="flex flex-col gap-4">

          <!-- Aviso: modelo sem histórico de produção -->
          <div
            v-if="!result.estimateIsTrained"
            class="flex items-start gap-2 bg-warning-50 dark:bg-background-800 border border-warning-200 dark:border-warning-800 rounded-xl px-4 py-3 text-sm text-warning-700 dark:text-warning-400"
          >
            <span class="material-symbols-rounded text-base mt-0.5">info</span>
            {{ t('etaSimulation.noHistoryWarning') }}
          </div>

          <!-- Total -->
          <div class="bg-background-50 dark:bg-background-800 border border-background-300 dark:border-background-700 rounded-xl p-6 flex items-center justify-between">
            <div>
              <div class="text-xs font-medium text-background-500 uppercase tracking-wider mb-1">
                {{ t('etaSimulation.totalEstimated') }}
              </div>
              <div class="text-3xl font-semibold text-background-900 dark:text-background-50">
                {{ formatDuration(result.totalEstimatedSeconds) }}
              </div>
            </div>
            <span class="material-symbols-rounded text-5xl text-primary-200 dark:text-primary-900">schedule</span>
          </div>

          <!-- Por fase -->
          <div class="bg-background-50 dark:bg-background-800 border border-background-300 dark:border-background-700 rounded-xl overflow-hidden">
            <div class="px-5 py-4 border-b border-background-300 dark:border-background-700">
              <span class="text-xs font-medium text-background-500 uppercase tracking-wider">
                {{ t('etaSimulation.byPhase') }}
              </span>
            </div>

            <div v-if="result.phases.length === 0" class="px-5 py-6 text-sm text-background-400">
              {{ t('etaSimulation.noPhasesForModel') }}
            </div>

            <div v-else class="divide-y divide-background-200 dark:divide-background-700">
              <div
                v-for="phase in result.phases"
                :key="phase.manufacturingPhaseId"
                class="flex items-center gap-4 px-5 py-3"
              >
                <span class="text-xs font-mono font-semibold text-background-400 dark:text-background-500 w-5 text-center">
                  {{ phase.order }}
                </span>
                <span class="material-symbols-rounded text-background-400 text-base">precision_manufacturing</span>
                <span class="text-sm text-background-800 dark:text-background-100 flex-1">{{ phase.phaseName }}</span>
                <span class="text-sm font-medium text-background-700 dark:text-background-300">
                  {{ formatDuration(phase.estimatedSeconds) }}
                </span>
              </div>
            </div>
          </div>

        </div>
      </div>

    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { carModelService, configOptionService } from '@/services/carModelService'
import type { CarModel, Config, ConfigOption, EtaSimulationResult } from '@/services/carModelService'
import { useI18n } from 'vue-i18n'

const { t } = useI18n()

// ── Estado ───────────────────────────────────────────────────────────────────
const models = ref<CarModel[]>([])
const selectedModelId = ref(0)

const configs = ref<Config[]>([])
const optionsByConfig = reactive<Record<number, ConfigOption[]>>({})
const configsLoading = ref(false)

const selectedOptionIds = ref<Set<number>>(new Set())

const result = ref<EtaSimulationResult | null>(null)
const simulationLoading = ref(false)
const simulationError = ref('')

// ── Lifecycle ────────────────────────────────────────────────────────────────
onMounted(async () => {
  const res = await carModelService.getAll()
  models.value = res.data
})

// ── Modelo ───────────────────────────────────────────────────────────────────
async function onModelChange() {
  selectedOptionIds.value = new Set()
  result.value = null
  simulationError.value = ''
  configs.value = []

  if (!selectedModelId.value) return

  configsLoading.value = true
  try {
    const [configsRes, optionsRes] = await Promise.all([
      carModelService.getConfigs(selectedModelId.value),
      configOptionService.getAll(),
    ])

    configs.value = unwrap<Config>(configsRes.data)

    const configIds = new Set(configs.value.map(c => c.id))
    const allOptions = unwrap<ConfigOption>(optionsRes.data)

    Object.keys(optionsByConfig).forEach(key => delete optionsByConfig[Number(key)])
    for (const option of allOptions) {
      if (!configIds.has(option.configId)) continue
      if (!optionsByConfig[option.configId]) optionsByConfig[option.configId] = []
      optionsByConfig[option.configId].push(option)
    }

    // Pré-seleciona as opções marcadas como default, como ponto de partida razoável.
    // Construído num Set novo e só atribuído no fim, para não disparar simulações
    // a meio do preenchimento.
    const defaults = new Set<number>()
    for (const config of configs.value) {
      const defaultOption = (optionsByConfig[config.id] || []).find(o => o.isDefault)
      if (defaultOption) defaults.add(defaultOption.id)
    }
    selectedOptionIds.value = defaults
  } finally {
    configsLoading.value = false
  }

  await runSimulation()
}

// ── Opções ───────────────────────────────────────────────────────────────────
// Cada chamada substitui a referência do Set (nunca muta o existente) e dispara
// a simulação uma única vez, de forma explícita — evita disparos múltiplos ou
// fora de ordem que um watch automático sobre um Set mutável provocaria.
function toggleOption(config: Config, optionId: number) {
  const ids = new Set(selectedOptionIds.value)
  if (config.allowMultiple) {
    if (ids.has(optionId)) ids.delete(optionId)
    else ids.add(optionId)
  } else {
    // Single-choice: remove as outras opções desta config antes de marcar a nova.
    const siblingIds = (optionsByConfig[config.id] || []).map(o => o.id)
    siblingIds.forEach(id => ids.delete(id))
    ids.add(optionId)
  }
  selectedOptionIds.value = ids
  runSimulation()
}

// ── Simulação ────────────────────────────────────────────────────────────────
// Contador de pedidos: se o utilizador marcar/desmarcar opções rapidamente,
// várias chamadas ficam em curso ao mesmo tempo. Sem isto, a resposta de um
// pedido mais antigo (que demorou mais a chegar) podia sobrescrever o
// resultado de um pedido mais recente. Só a resposta do pedido mais recente
// é aplicada ao resultado mostrado.
let latestRequestId = 0

async function runSimulation() {
  if (!selectedModelId.value) {
    result.value = null
    return
  }

  const requestId = ++latestRequestId
  simulationLoading.value = true
  simulationError.value = ''
  try {
    const res = await carModelService.getEtaSimulation(
      selectedModelId.value,
      Array.from(selectedOptionIds.value),
    )
    if (requestId !== latestRequestId) return // resposta desatualizada, ignora
    result.value = res.data
  } catch (err: any) {
    if (requestId !== latestRequestId) return // erro de um pedido já ultrapassado
    result.value = null
    simulationError.value = err?.response?.data ?? t('etaSimulation.simulationFailed')
  } finally {
    if (requestId === latestRequestId) simulationLoading.value = false
  }
}

// ── Helpers ──────────────────────────────────────────────────────────────────
// A API pode devolver tanto um array simples como { $values: [...] }, dependendo
// da configuração de serialização — trata os dois casos por segurança.
function unwrap<T>(data: any): T[] {
  return data?.$values ?? data ?? []
}

function formatDuration(totalSeconds: number): string {
  const h = Math.floor(totalSeconds / 3600)
  const m = Math.round((totalSeconds % 3600) / 60)
  if (h === 0) return t('etaSimulation.minutesShort', { m })
  if (m === 0) return t('etaSimulation.hoursShort', { h })
  return t('etaSimulation.hoursMinutesShort', { h, m })
}
</script>