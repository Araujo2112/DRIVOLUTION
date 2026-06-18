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
      <div class="grid grid-cols-3 gap-4 mb-8">
        <div class="bg-background-50 dark:bg-background-800 border border-background-300 dark:border-background-700 rounded-xl p-5">
          <p class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('wip.totalProducts') }}</p>
          <p class="text-3xl font-medium text-background-900 dark:text-background-50 mt-2">{{ summary.totalProducts }}</p>
        </div>
        <div class="bg-background-50 dark:bg-background-800 border border-background-300 dark:border-background-700 rounded-xl p-5">
          <p class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('wip.inProduction') }}</p>
          <p class="text-3xl font-medium text-primary-500 mt-2">{{ summary.inProgress }}</p>
        </div>
        <div class="bg-background-50 dark:bg-background-800 border border-background-300 dark:border-background-700 rounded-xl p-5">
          <p class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('wip.completed') }}</p>
          <p class="text-3xl font-medium text-success-500 mt-2">{{ summary.completed }}</p>
        </div>
      </div>

      <!-- Tabela -->
      <div class="border border-background-300 dark:border-background-700 rounded-xl overflow-hidden">
        <div class="grid grid-cols-7 px-4 py-3 bg-background-100 dark:bg-background-800 border-b border-background-300 dark:border-background-700">
          <span class="text-xs font-medium text-background-500 uppercase tracking-wider col-span-2">{{ t('wip.fields.product') }}</span>
          <span class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('wip.fields.line') }}</span>
          <span class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('wip.fields.workstation') }}</span>
          <span class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('wip.fields.phase') }}</span>
          <span class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('wip.fields.start') }}</span>
          <span class="text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('wip.fields.elapsed') }}</span>
        </div>

        <div v-if="items.length === 0" class="text-center py-12 text-background-500">
          <span class="material-symbols-rounded text-4xl block mb-2">inventory_2</span>
          <p class="text-sm">{{ t('wip.empty') }}</p>
        </div>

        <div
          v-for="item in items"
          :key="item.productId + '-' + item.currentPhase"
          class="grid grid-cols-7 px-4 py-3 border-b border-background-200 dark:border-background-700 last:border-0 bg-background-50 dark:bg-background-800 hover:bg-background-100 dark:hover:bg-background-750 transition-colors items-center"
        >
          <div class="col-span-2">
            <button
              @click="goToProduct(item.productId)"
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
          <span class="text-sm font-medium text-primary-500">{{ formatDuration(item.elapsedSeconds) }}</span>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import axios from '@/axios'
import { useI18n } from 'vue-i18n'
import { toast } from '@/plugins/toast'

const { t } = useI18n()
const router = useRouter()

const loading = ref(false)
const errorMessage = ref('')
const summary = ref({ totalProducts: 0, inProgress: 0, completed: 0 })
const items = ref<any[]>([])

async function loadWip() {
  loading.value = true
  try {
    const response = await axios.get('/production-lines/wip')
    summary.value = {
      totalProducts: response.data.totalProducts ?? 0,
      inProgress: response.data.inProgress ?? 0,
      completed: response.data.completed ?? 0,
    }
    items.value = response.data.items?.$values ?? response.data.items ?? []
  } catch {
    toast.error(t('errors.loadFailed'))
  } finally {
    loading.value = false
  }
}

function goToProduct(productId: number) {
  router.push({ name: 'ProductTimeline', query: { id: String(productId) } })
}

function formatDate(date: string | null) {
  if (!date) return '-'
  return new Date(date).toLocaleString('pt-PT')
}

function formatDuration(seconds: number | null) {
  if (seconds === null || seconds === undefined) return '-'
  const hours = Math.floor(seconds / 3600)
  const minutes = Math.floor((seconds % 3600) / 60)
  const remainingSeconds = seconds % 60
  if (hours > 0) return `${hours}h ${minutes}m`
  if (minutes > 0) return `${minutes}m ${remainingSeconds}s`
  return `${remainingSeconds}s`
}

onMounted(() => loadWip())
</script>