<template>
  <div class="px-8 py-8 flex flex-col items-center justify-center text-center min-h-[calc(100vh-4rem)]">
    <span class="material-symbols-rounded text-6xl text-background-300 dark:text-background-700 mb-4">settings</span>
    <h1 class="text-xl font-semibold text-background-900 dark:text-background-50">{{ t('client.settings.title') }}</h1>
    <p class="text-sm text-background-500 dark:text-background-400 mt-1 max-w-sm">{{ t('client.settings.subtitle') }}</p>
  </div>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n'
import 'material-symbols'
const { t } = useI18n()
</script>
