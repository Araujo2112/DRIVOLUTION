<template>
  <div class="min-h-screen bg-background-50 dark:bg-background-950 flex flex-col">
    <!-- Topbar do cliente -->
    <header class="bg-background-0 dark:bg-background-900 border-b border-background-200 dark:border-background-700 px-6 py-4 flex items-center justify-between">
      <div class="flex items-center gap-3">
        <span class="text-primary-500 font-bold text-lg tracking-tight">DRIVOLUTION</span>
        <span class="text-background-300 dark:text-background-600">|</span>
        <span class="text-sm text-background-500 dark:text-background-400">{{ t('client.portal') }}</span>
      </div>
      <div class="flex items-center gap-4">
        <span class="text-sm text-background-600 dark:text-background-400">{{ authStore.user?.name }}</span>
        <button
          class="text-sm text-danger-500 hover:text-danger-400 transition-colors"
          @click="authStore.logout()"
        >
          {{ t('client.logout') }}
        </button>
      </div>
    </header>

    <!-- Conteúdo -->
    <main class="flex-1">
      <router-view />
    </main>
  </div>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n'
import { useAuthStore } from '@/stores/authStore'

const { t } = useI18n()
const authStore = useAuthStore()
</script>