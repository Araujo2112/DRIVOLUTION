<template>
  <div class="px-8 py-8 w-full">
    <div class="mb-7">
      <h1 class="text-2xl font-semibold text-background-900 dark:text-background-50">
        {{ t('client.newOrder.selection.title') }}
      </h1>
      <p class="text-sm text-background-500 dark:text-background-400 mt-1.5">
        {{ t('client.newOrder.selection.subtitle') }}
      </p>
    </div>

    <!-- Loading -->
    <div v-if="loading" class="flex justify-center py-20">
      <div class="animate-spin rounded-full h-9 w-9 border-2 border-primary-200 border-t-primary-500" />
    </div>

    <!-- Vazio -->
    <div v-else-if="models.length === 0" class="text-center py-20 text-background-400 dark:text-background-500">
      {{ t('client.newOrder.selection.empty') }}
    </div>

    <!-- Grelha de modelos -->
    <!-- NOTA: só mostramos Name/Version/Type porque é o único que existe na
         tabela "model". Os mockups aprovados tinham preço, imagem, autonomia
         e badges — nenhum desses campos está na BD, por isso não os inventei
         aqui. Se vierem a ser adicionados ao schema, esta grelha é o sítio
         a atualizar. -->
    <div v-else class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
      <div
        v-for="model in models"
        :key="model.id"
        class="bg-background-50 dark:bg-background-900 rounded-2xl border border-background-200 dark:border-background-800 p-5 flex flex-col gap-4"
      >
        <div class="h-32 rounded-xl bg-background-100 dark:bg-background-800 flex items-center justify-center">
          <span class="material-symbols-rounded text-5xl text-primary-300 dark:text-primary-700">directions_car</span>
        </div>

        <div>
          <p class="text-sm font-medium text-background-800 dark:text-background-100">{{ model.name }}</p>
          <p class="text-xs text-background-500 dark:text-background-400 mt-0.5">
            <span v-if="model.version">{{ model.version }}</span>
            <span v-if="model.version && model.type"> · </span>
            <span v-if="model.type">{{ model.type }}</span>
          </p>
        </div>

        <button
          class="mt-auto flex items-center justify-center gap-1.5 w-full py-2.5 rounded-xl text-sm font-medium bg-primary-500 text-white hover:bg-primary-600 transition-colors"
          @click="router.push(`/client/new-order/${model.id}`)"
        >
          {{ t('client.newOrder.selection.configure') }}
          <span class="material-symbols-rounded text-lg">arrow_forward</span>
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import 'material-symbols'
import { clientPortalService, type ClientCarModel } from '@/services/clientPortalService'

const { t } = useI18n()
const router = useRouter()

const models = ref<ClientCarModel[]>([])
const loading = ref(true)

onMounted(async () => {
  try {
    models.value = await clientPortalService.getModels()
  } finally {
    loading.value = false
  }
})
</script>