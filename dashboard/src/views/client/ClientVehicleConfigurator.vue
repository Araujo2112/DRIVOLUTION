<template>
  <div class="px-8 py-8 w-full">
    <!-- Voltar -->
    <button
      class="flex items-center gap-1.5 text-sm text-background-500 dark:text-background-400 hover:text-primary-500 dark:hover:text-primary-400 mb-6 transition-colors"
      @click="router.push('/client/new-order')"
    >
      <span class="material-symbols-rounded text-base">arrow_back</span>
      {{ t('client.newOrder.configurator.back') }}
    </button>

    <!-- Loading -->
    <div v-if="loading" class="flex justify-center py-20">
      <div class="animate-spin rounded-full h-9 w-9 border-2 border-primary-200 border-t-primary-500" />
    </div>

    <!-- Não encontrado -->
    <div v-else-if="!model" class="text-center py-20 text-background-400 dark:text-background-500">
      {{ t('client.newOrder.configurator.notFound') }}
    </div>

    <template v-else>
      <div class="mb-7">
        <h1 class="text-2xl font-semibold text-background-900 dark:text-background-50">
          {{ t('client.newOrder.configurator.title') }} {{ model.name }}
          <span v-if="model.version" class="text-background-400 dark:text-background-500 font-normal">{{ model.version }}</span>
        </h1>
        <p class="text-sm text-background-500 dark:text-background-400 mt-1.5">
          {{ t('client.newOrder.configurator.subtitle') }}
        </p>
      </div>

      <div class="grid grid-cols-1 lg:grid-cols-3 gap-5 items-start">
        <!-- Opções de configuração -->
        <div class="lg:col-span-2 flex flex-col gap-4">
          <div v-if="configs.length === 0" class="bg-background-50 dark:bg-background-900 rounded-2xl border border-background-200 dark:border-background-800 p-5 text-sm text-background-500 dark:text-background-400">
            {{ t('client.newOrder.configurator.noConfigs') }}
          </div>

          <div
            v-for="config in configs"
            :key="config.id"
            class="bg-background-50 dark:bg-background-900 rounded-2xl border border-background-200 dark:border-background-800 p-5"
          >
            <p class="text-sm font-medium text-background-800 dark:text-background-100 mb-3">{{ config.item }}</p>

            <div class="flex flex-wrap gap-2">
              <button
                v-for="option in config.options"
                :key="option.id"
                type="button"
                class="px-3.5 py-2 rounded-xl text-sm border transition-colors"
                :class="isSelected(config, option.id)
                  ? 'bg-primary-500 border-primary-500 text-white'
                  : 'border-background-200 dark:border-background-700 text-background-700 dark:text-background-300 hover:border-primary-400'"
                @click="toggleOption(config, option.id)"
              >
                {{ option.value }}
              </button>
            </div>
          </div>
        </div>

        <!-- Resumo -->
        <div class="bg-background-50 dark:bg-background-900 rounded-2xl border border-background-200 dark:border-background-800 p-5 flex flex-col gap-4 sticky top-4">
          <p class="text-sm font-medium text-background-800 dark:text-background-100">
            {{ t('client.newOrder.configurator.summary') }}
          </p>

          <div class="flex flex-col gap-2.5">
            <div v-for="config in configs" :key="config.id" class="flex justify-between gap-3 text-sm">
              <span class="text-background-500 dark:text-background-400">{{ config.item }}</span>
              <span class="text-right text-background-800 dark:text-background-100 font-medium">
                {{ selectedLabels(config) }}
              </span>
            </div>
          </div>
        </div>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import 'material-symbols'
import { clientPortalService, type ClientCarModel, type ClientModelConfig } from '@/services/clientPortalService'
import { toast } from '@/plugins/toast'

const { t } = useI18n()
const route = useRoute()
const router = useRouter()

const model = ref<ClientCarModel | null>(null)
const configs = ref<ClientModelConfig[]>([])
const loading = ref(true)

// configId -> Set de optionIds selecionadas
const selections = ref<Map<number, Set<number>>>(new Map())

onMounted(async () => {
  const modelId = Number(route.params.modelId)
  try {
    const [modelResult, configsResult] = await Promise.all([
      clientPortalService.getModel(modelId),
      clientPortalService.getModelConfigs(modelId),
    ])
    model.value = modelResult
    configs.value = configsResult

    // Pré-seleciona as opções marcadas como default na BD (is_default).
    configsResult.forEach(config => {
      const defaults = config.options.filter(o => o.isDefault).map(o => o.id)
      if (defaults.length > 0) {
        selections.value.set(config.id, new Set(defaults))
      }
    })
  } catch {
    model.value = null
    toast.error(t('errors.loadFailed'))
  } finally {
    loading.value = false
  }
})

function isSelected(config: ClientModelConfig, optionId: number): boolean {
  return selections.value.get(config.id)?.has(optionId) ?? false
}

function toggleOption(config: ClientModelConfig, optionId: number) {
  const current = selections.value.get(config.id) ?? new Set<number>()

  if (config.allowMultiple) {
    // Multi-select: alterna livremente.
    current.has(optionId) ? current.delete(optionId) : current.add(optionId)
  } else {
    // Single-select: só pode haver uma opção ativa por config.
    current.clear()
    current.add(optionId)
  }

  selections.value.set(config.id, new Set(current))
}

function selectedLabels(config: ClientModelConfig): string {
  const selectedIds = selections.value.get(config.id)
  if (!selectedIds || selectedIds.size === 0) return t('client.newOrder.configurator.notSelected')

  return config.options
    .filter(o => selectedIds.has(o.id))
    .map(o => o.value)
    .join(', ')
}
</script>