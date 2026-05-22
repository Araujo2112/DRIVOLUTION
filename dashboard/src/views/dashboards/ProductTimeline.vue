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

    <!-- Tabela -->
    <div v-else-if="timeline.length > 0" class="border border-background-300 dark:border-background-700 rounded-xl overflow-hidden">
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
            :key="phase.productPhaseId"
            class="grid grid-cols-7 px-4 py-3 border-b border-background-200 dark:border-background-700 last:border-0 bg-background-50 dark:bg-background-800 hover:bg-background-100 dark:hover:bg-background-750 transition-colors items-center"
        >
            <div class="col-span-2 flex items-center gap-2">
            <div class="w-2 h-2 rounded-full flex-shrink-0" :class="phase.endedAt ? 'bg-success-500' : 'bg-primary-500 animate-pulse'"></div>
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

const { t } = useI18n()

const loading = ref(false)
const errorMessage = ref('')
const productId = ref<number>(1)
const timeline = ref<any[]>([])
const product = ref<any>(null)

async function loadTimeline() {
  loading.value = true
  timeline.value = []
  product.value = null
  try {
    const response = await axios.get(`/api/products/${productId.value}/timeline`)
    product.value = {
      id: response.data.productId,
      serialNumber: response.data.serialNumber,
      status: response.data.status,
    }
    timeline.value = response.data.phases?.$values ?? response.data.phases ?? []
  } catch {
    toast.error(t('errors.loadFailed'))
  } finally {
    loading.value = false
  }
}

function formatDate(date: string | null) {
  if (!date) return '—'
  return new Date(date).toLocaleString('pt-PT')
}

function formatDuration(seconds: number | null) {
  if (seconds === null || seconds === undefined) return t('timeline.inProgress')
  const minutes = Math.floor(seconds / 60)
  const remainingSeconds = seconds % 60
  return `${minutes}m ${remainingSeconds}s`
}

function resultLabel(result: string | null) {
  switch (result) {
    case 'passed': return t('timeline.result.passed')
    case 'failed_rework': return t('timeline.result.rework')
    case 'failed_scrapped': return t('timeline.result.scrapped')
    default: return result || '—'
  }
}

onMounted(() => loadTimeline())
</script>
